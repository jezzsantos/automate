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
            InitialiseSchema();
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

        public SolutionItem Model { get; private set; }

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
                    var automation = solutionItem.PatternSchema.Automation.Safe()
                        .FirstOrDefault(auto => auto.Id.EqualsIgnoreCase(automationId));
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

                if (solutionItem.IsCollection)
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
                    var codeTemplate = solutionItem.PatternSchema.CodeTemplates.Safe()
                        .FirstOrDefault(temp => temp.Id.EqualsIgnoreCase(codeTemplateId));
                    if (codeTemplate.Exists())
                    {
                        return solutionItem;
                    }
                }

                if (solutionItem.IsElement)
                {
                    var codeTemplate = solutionItem.ElementSchema.CodeTemplates.Safe()
                        .FirstOrDefault(tem => tem.Id.EqualsIgnoreCase(codeTemplateId));
                    if (codeTemplate.Exists())
                    {
                        return solutionItem;
                    }
                }
                if (solutionItem.IsCollection || solutionItem.IsAttribute || solutionItem.IsValue)
                {
                    return default;
                }

                foreach (var (_, value) in solutionItem.Properties.Safe())
                {
                    var result = FindDescendantCodeTemplate(value);
                    if (result.Exists())
                    {
                        return result;
                    }
                }
                foreach (var item in solutionItem.Items.Safe())
                {
                    var result = FindDescendantCodeTemplate(item);
                    if (result.Exists())
                    {
                        return result;
                    }
                }

                return default;
            }
        }

        public ValidationResults Validate(ValidationContext context)
        {
            return Model.Validate(context);
        }

        public string Id { get; }

        public string Name { get; }

        private void PopulateAncestry()
        {
            PopulateDescendantParents(Model, null);

            void PopulateDescendantParents(SolutionItem solutionItem, SolutionItem parent)
            {
                solutionItem.SetParent(parent);
                var properties = solutionItem.Properties.Safe();
                foreach (var property in properties)
                {
                    PopulateDescendantParents(property.Value, solutionItem);
                }
                var items = solutionItem.Items.Safe();
                foreach (var item in items)
                {
                    //NOTE: We skip the "ephemeral" collection element, to get it its parent instead
                    PopulateDescendantParents(item, solutionItem.Parent);
                }
            }
        }

        private static string GetRandomNumber()
        {
            var number = DateTime.Now.Ticks.ToString();
            return number.Substring(number.Length - 3);
        }

        private void InitialiseSchema()
        {
            Model = new SolutionItem(Toolkit.Pattern);
        }
    }

    internal class SolutionItemCommandPair
    {
        public SolutionItemCommandPair(Automation automation, SolutionItem solutionItem)
        {
            Automation = automation;
            SolutionItem = solutionItem;
        }

        public Automation Automation { get; }

        public SolutionItem SolutionItem { get; }
    }
}