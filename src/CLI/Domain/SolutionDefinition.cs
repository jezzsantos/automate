using System;
using System.Collections.Generic;
using System.Linq;
using Automate.CLI.Extensions;

namespace Automate.CLI.Domain
{
    internal class SolutionDefinition : INamedEntity, IPersistable
    {
        public SolutionDefinition(ToolkitDefinition toolkit, string name = null)
        {
            toolkit.GuardAgainstNull(nameof(toolkit));

            Id = IdGenerator.Create();
            Name = name.HasValue()
                ? name
                : $"{toolkit.PatternName}{GetRandomNumber()}";
            Toolkit = toolkit;
            Model = new SolutionItem(toolkit, toolkit.Pattern);
        }

        private SolutionDefinition(PersistableProperties properties, IPersistableFactory factory)
        {
            Id = properties.Rehydrate<string>(factory, nameof(Id));
            Name = properties.Rehydrate<string>(factory, nameof(Name));
            Toolkit = properties.Rehydrate<ToolkitDefinition>(factory, nameof(Toolkit));
            Model = properties.Rehydrate<SolutionItem>(factory, nameof(Model));
        }

        public ToolkitDefinition Toolkit { get; }

        public string PatternName => Toolkit.Pattern?.Name;

        public SolutionItem Model { get; }

        public PersistableProperties Dehydrate()
        {
            var properties = new PersistableProperties();
            properties.Dehydrate(nameof(Id), Id);
            properties.Dehydrate(nameof(Name), Name);
            properties.Dehydrate(nameof(Toolkit), Toolkit);
            properties.Dehydrate(nameof(Model), Model);

            return properties;
        }

        public static SolutionDefinition Rehydrate(PersistableProperties properties, IPersistableFactory factory)
        {
            var instance = new SolutionDefinition(properties, factory);
            instance.PopulateAncestry();
            return instance;
        }

        public string GetConfiguration()
        {
            return Model.GetConfiguration(false).ToJson();
        }

        public List<SolutionItemCommandPair> FindByAutomation(string automationId)
        {
            return FindDescendantAutomation(Model);

            List<SolutionItemCommandPair> FindDescendantAutomation(SolutionItem solutionItem)
            {
                var pairs = new List<SolutionItemCommandPair>();

                if (solutionItem.IsPattern)
                {
                    var automation = solutionItem.PatternSchema.FindAutomationById(automationId);
                    if (automation.Exists())
                    {
                        pairs.Add(new SolutionItemCommandPair(automation, solutionItem));
                    }
                }

                if (solutionItem.IsElement)
                {
                    var automation = solutionItem.ElementSchema.Automation.Safe()
                        .FirstOrDefault(auto => auto.Id.EqualsIgnoreCase(automationId));
                    if (automation.Exists())
                    {
                        pairs.Add(new SolutionItemCommandPair(automation, solutionItem));
                    }
                }

                if (solutionItem.IsPattern || solutionItem.IsElement)
                {
                    foreach (var (_, value) in solutionItem.Properties.Safe())
                    {
                        var result = FindDescendantAutomation(value);
                        if (result.HasAny())
                        {
                            pairs.AddRange(result);
                        }
                    }
                }

                if (solutionItem.IsEphemeralCollection)
                {
                    foreach (var item in solutionItem.Items.Safe())
                    {
                        var result = FindDescendantAutomation(item);
                        if (result.HasAny())
                        {
                            pairs.AddRange(result);
                        }
                    }
                }

                if (solutionItem.IsAttribute || solutionItem.IsValue)
                {
                    return new List<SolutionItemCommandPair>();
                }

                return pairs;
            }
        }

        public SolutionItem FindByCodeTemplate(string codeTemplateId)
        {
            return FindDescendantCodeTemplate(Model);

            SolutionItem FindDescendantCodeTemplate(SolutionItem solutionItem)
            {
                if (solutionItem.IsPattern)
                {
                    var codeTemplate = solutionItem.PatternSchema.FindCodeTemplateById(codeTemplateId);
                    if (codeTemplate.Exists())
                    {
                        return solutionItem;
                    }
                }

                if (solutionItem.IsElement)
                {
                    var codeTemplate = solutionItem.ElementSchema.FindCodeTemplateById(codeTemplateId);
                    if (codeTemplate.Exists())
                    {
                        return solutionItem;
                    }
                }

                if (solutionItem.IsPattern || solutionItem.IsElement)
                {
                    foreach (var (_, value) in solutionItem.Properties.Safe())
                    {
                        var result = FindDescendantCodeTemplate(value);
                        if (result.Exists())
                        {
                            return result;
                        }
                    }
                }

                if (solutionItem.IsEphemeralCollection)
                {
                    foreach (var item in solutionItem.Items.Safe())
                    {
                        var result = FindDescendantCodeTemplate(item);
                        if (result.Exists())
                        {
                            return result;
                        }
                    }
                }

                if (solutionItem.IsAttribute || solutionItem.IsValue)
                {
                    return default;
                }

                return default;
            }
        }

        public ValidationResults Validate(ValidationContext context)
        {
            return Model.Validate(context);
        }

        public SolutionUpgradeResult Upgrade(ToolkitDefinition latestToolkit, bool force)
        {
            latestToolkit.GuardAgainstNull(nameof(latestToolkit));

            var result = new SolutionUpgradeResult(this, Toolkit.Version, latestToolkit.Version);
            if (Toolkit.Version.EqualsOrdinal(latestToolkit.Version))
            {
                result.Add(MigrationChangeType.Abort, MigrationMessages.SolutionDefinition_Upgrade_SameToolkitVersion, latestToolkit.PatternName, latestToolkit.Version);
                return result;
            }

            if (IsBreakingChange(Toolkit, latestToolkit))
            {
                if (!force)
                {
                    result.Fail(MigrationChangeType.Abort, MigrationMessages.SolutionDefinition_Upgrade_BreakingChangeForbidden, latestToolkit.PatternName, latestToolkit.Version);
                    return result;
                }

                result.Add(MigrationChangeType.Breaking, MigrationMessages.SolutionDefinition_Upgrade_BreakingChangeForced, latestToolkit.PatternName, latestToolkit.Version);
            }

            Model.Migrate(latestToolkit, result);
            Toolkit.MigratePattern(latestToolkit, result);

            return result;

            bool IsBreakingChange(ToolkitDefinition currentToolkit, ToolkitDefinition nextToolkit)
            {
                return new Version(nextToolkit.Version).Major > new Version(currentToolkit.Version).Major;
            }
        }

        public string Id { get; }

        public string Name { get; }

        private void PopulateAncestry()
        {
            PopulateDescendantParents(Model, null);

            void PopulateDescendantParents(SolutionItem solutionItem, SolutionItem parent)
            {
                solutionItem.SetAncestry(Toolkit, parent);
                var properties = solutionItem.Properties.Safe();
                foreach (var property in properties)
                {
                    PopulateDescendantParents(property.Value, solutionItem);
                }
                var items = solutionItem.Items.Safe();
                foreach (var item in items)
                {
                    PopulateDescendantParents(item, solutionItem);
                }
            }
        }

        private static string GetRandomNumber()
        {
            var number = DateTime.Now.Ticks.ToString();
            return number.Substring(number.Length - 3);
        }
    }

    internal class SolutionItemCommandPair
    {
        public SolutionItemCommandPair(IAutomationSchema automation, SolutionItem solutionItem)
        {
            Automation = automation;
            SolutionItem = solutionItem;
        }

        public IAutomationSchema Automation { get; }

        public SolutionItem SolutionItem { get; }
    }
}