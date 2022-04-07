using System;
using System.Collections.Generic;
using System.Linq;
using Automate.CLI.Extensions;

namespace Automate.CLI.Domain
{
    internal class SolutionDefinition : INamedEntity, IPersistable, ISolutionVisitable
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
            var aggregator = new AutomationAggregator(automationId);
            TraverseSolution(aggregator);
            return aggregator.Automation;
        }

        public SolutionItem FindByCodeTemplate(string codeTemplateId)
        {
            var finder = new CodeTemplateFinder(codeTemplateId);
            TraverseSolution(finder);
            return finder.SolutionItem;
        }

        public ValidationResults Validate()
        {
            return Model.Validate();
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

        public bool Accept(ISolutionVisitor visitor)
        {
            if (visitor.VisitSolutionEnter(this))
            {
                Model.Accept(visitor);
            }

            return visitor.VisitSolutionExit(this);
        }

        private void TraverseSolution(ISolutionVisitor visitor)
        {
            Accept(visitor);
        }

        internal void PopulateAncestry()
        {
            var populator = new AncestryPopulator(Toolkit);
            TraverseSolution(populator);
        }

        private static string GetRandomNumber()
        {
            var number = DateTime.Now.Ticks.ToString();
            return number.Substring(number.Length - 3);
        }

        private class AncestryPopulator : ISolutionVisitor
        {
            private readonly Stack<SolutionItem> ancestry = new Stack<SolutionItem>();
            private readonly ToolkitDefinition toolkit;

            public AncestryPopulator(ToolkitDefinition toolkit)
            {
                toolkit.GuardAgainstNull(nameof(toolkit));

                this.toolkit = toolkit;
            }

            public bool VisitPatternEnter(SolutionItem item)
            {
                this.ancestry.Push(item);

                item.SetAncestry(this.toolkit, null);
                return true;
            }

            public bool VisitPatternExit(SolutionItem item)
            {
                this.ancestry.Clear();
                return true;
            }

            public bool VisitElementEnter(SolutionItem item)
            {
                var parent = this.ancestry.Peek();
                if (parent.NotExists())
                {
                    throw new InvalidOperationException();
                }
                this.ancestry.Push(item);

                item.SetAncestry(this.toolkit, parent);
                return true;
            }

            public bool VisitElementExit(SolutionItem item)
            {
                this.ancestry.Pop();
                return true;
            }

            public bool VisitEphemeralCollectionEnter(SolutionItem item)
            {
                var parent = this.ancestry.Peek();
                if (parent.NotExists())
                {
                    throw new InvalidOperationException();
                }
                this.ancestry.Push(item);

                item.SetAncestry(this.toolkit, parent);
                return true;
            }

            public bool VisitEphemeralCollectionExit(SolutionItem item)
            {
                this.ancestry.Pop();
                return true;
            }

            public bool VisitAttributeEnter(SolutionItem item)
            {
                var parent = this.ancestry.Peek();
                if (parent.NotExists())
                {
                    throw new InvalidOperationException();
                }

                item.SetAncestry(this.toolkit, parent);
                return true;
            }

            public bool VisitAttributeExit(SolutionItem item)
            {
                return true;
            }
        }

        private class CodeTemplateFinder : ISolutionVisitor
        {
            private readonly string codeTemplateId;

            public CodeTemplateFinder(string codeTemplateId)
            {
                codeTemplateId.GuardAgainstNullOrEmpty(nameof(codeTemplateId));
                this.codeTemplateId = codeTemplateId;
                SolutionItem = null;
            }

            public SolutionItem SolutionItem { get; private set; }

            public bool VisitPatternEnter(SolutionItem item)
            {
                var codeTemplate = item.PatternSchema.FindCodeTemplateById(this.codeTemplateId);
                if (codeTemplate.Exists())
                {
                    SolutionItem = item;
                    return false;
                }

                return true;
            }

            public bool VisitPatternExit(SolutionItem item)
            {
                return true;
            }

            public bool VisitElementEnter(SolutionItem item)
            {
                var codeTemplate = item.ElementSchema.FindCodeTemplateById(this.codeTemplateId);
                if (codeTemplate.Exists())
                {
                    SolutionItem = item;
                    return false;
                }

                return true;
            }

            public bool VisitElementExit(SolutionItem item)
            {
                return true;
            }
        }

        private class AutomationAggregator : ISolutionVisitor
        {
            private readonly string automationId;

            public AutomationAggregator(string automationId)
            {
                automationId.GuardAgainstNullOrEmpty(nameof(automationId));

                this.automationId = automationId;
                Automation = new List<SolutionItemCommandPair>();
            }

            public List<SolutionItemCommandPair> Automation { get; }

            public bool VisitPatternEnter(SolutionItem item)
            {
                var automation = item.PatternSchema.FindAutomationById(this.automationId);
                if (automation.Exists())
                {
                    Automation.Add(new SolutionItemCommandPair(automation, item));
                }

                return true;
            }

            public bool VisitPatternExit(SolutionItem item)
            {
                return true;
            }

            public bool VisitElementEnter(SolutionItem item)
            {
                var automation = item.ElementSchema.Automation.Safe()
                    .FirstOrDefault(auto => auto.Id.EqualsIgnoreCase(this.automationId));
                if (automation.Exists())
                {
                    Automation.Add(new SolutionItemCommandPair(automation, item));
                }

                return true;
            }

            public bool VisitElementExit(SolutionItem item)
            {
                return true;
            }
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