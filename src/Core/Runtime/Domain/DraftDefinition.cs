using System;
using System.Collections.Generic;
using System.Linq;
using Automate.Authoring.Domain;
using Automate.Common;
using Automate.Common.Domain;
using Automate.Common.Extensions;

namespace Automate.Runtime.Domain
{
    public class DraftDefinition : INamedEntity, IPersistable, IDraftVisitable
    {
        public DraftDefinition(ToolkitDefinition toolkit, string name = null)
        {
            toolkit.GuardAgainstNull(nameof(toolkit));

            Id = IdGenerator.Create();
            Name = name.HasValue()
                ? name
                : $"{toolkit.PatternName}{GetRandomNumber()}";
            Toolkit = toolkit;
            Model = new DraftItem(toolkit, toolkit.Pattern);
        }

        private DraftDefinition(PersistableProperties properties,
            IPersistableFactory factory)
        {
            Id = properties.Rehydrate<string>(factory, nameof(Id));
            Name = properties.Rehydrate<string>(factory, nameof(Name));
            Toolkit = properties.Rehydrate<ToolkitDefinition>(factory, nameof(Toolkit));
            Model = properties.Rehydrate<DraftItem>(factory, nameof(Model));
        }

        public ToolkitDefinition Toolkit { get; }

        public string PatternName => Toolkit.Pattern?.Name;

        public DraftItem Model { get; }

        public PersistableProperties Dehydrate()
        {
            var properties = new PersistableProperties();
            properties.Dehydrate(nameof(Id), Id);
            properties.Dehydrate(nameof(Name), Name);
            properties.Dehydrate(nameof(Toolkit), Toolkit);
            properties.Dehydrate(nameof(Model), Model);

            return properties;
        }

        public static DraftDefinition Rehydrate(PersistableProperties properties,
            IPersistableFactory factory)
        {
            var instance = new DraftDefinition(properties, factory);
            instance.PopulateAncestry();
            return instance;
        }

        public LazyDraftItemDictionary GetConfiguration()
        {
            return Model.GetConfiguration(false);
        }

        public List<DraftItemCommandPair> FindByAutomation(string automationId)
        {
            var aggregator = new AutomationAggregator(automationId);
            TraverseDraft(aggregator);
            return aggregator.Automation;
        }

        public DraftItem FindByCodeTemplate(string codeTemplateId)
        {
            var finder = new CodeTemplateFinder(codeTemplateId);
            TraverseDraft(finder);
            return finder.DraftItem;
        }

        public DraftUpgradeResult Upgrade(ToolkitDefinition latestToolkit, bool force)
        {
            latestToolkit.GuardAgainstNull(nameof(latestToolkit));

            var result = new DraftUpgradeResult(this, Toolkit.Version, latestToolkit.Version);
            if (Toolkit.Version.EqualsOrdinal(latestToolkit.Version))
            {
                result.Add(MigrationChangeType.Abort, MigrationMessages.DraftDefinition_Upgrade_SameToolkitVersion,
                    latestToolkit.PatternName, latestToolkit.Version);
                return result;
            }

            if (IsBreakingChange(Toolkit, latestToolkit))
            {
                if (!force)
                {
                    result.Fail(MigrationChangeType.Abort,
                        MigrationMessages.DraftDefinition_Upgrade_BreakingChangeForbidden, latestToolkit.PatternName,
                        latestToolkit.Version);
                    return result;
                }

                result.Add(MigrationChangeType.Breaking, MigrationMessages.DraftDefinition_Upgrade_BreakingChangeForced,
                    latestToolkit.PatternName, latestToolkit.Version);
            }

            Model.Migrate(latestToolkit, result);
            Toolkit.MigratePattern(latestToolkit, result);

            return result;

            bool IsBreakingChange(ToolkitDefinition currentToolkit, ToolkitDefinition nextToolkit)
            {
                return new Version(nextToolkit.Version).Major > new Version(currentToolkit.Version).Major;
            }
        }

        public CommandExecutionResult ExecuteCommand(IDraftPathResolver draftPathResolver, string itemExpression,
            string name)
        {
            draftPathResolver.GuardAgainstNull(nameof(draftPathResolver));

            var target = ResolveTargetItem(draftPathResolver, itemExpression);

            var validationResults = Model.Validate();
            if (validationResults.Any())
            {
                return new CommandExecutionResult(name, validationResults);
            }

            return target.ExecuteCommand(this, name);
        }

        public ValidationResults Validate(IDraftPathResolver draftPathResolver, string itemExpression)
        {
            draftPathResolver.GuardAgainstNull(nameof(draftPathResolver));

            var target = ResolveTargetItem(draftPathResolver, itemExpression);

            return target.Validate();
        }

        public bool Accept(IDraftVisitor visitor)
        {
            if (visitor.VisitDraftEnter(this))
            {
                Model.Accept(visitor);
            }

            return visitor.VisitDraftExit(this);
        }

        public string Id { get; }

        public string Name { get; }

        private void TraverseDraft(IDraftVisitor visitor)
        {
            Accept(visitor);
        }

        internal void PopulateAncestry()
        {
            var populator = new AncestryPopulator(Toolkit);
            TraverseDraft(populator);
        }

        private static string GetRandomNumber()
        {
            var number = DateTime.Now.Ticks.ToString();
            return number.Substring(number.Length - 3);
        }

        private DraftItem ResolveTargetItem(IDraftPathResolver draftPathResolver, string itemExpression)
        {
            var target = Model;
            if (itemExpression.HasValue())
            {
                var draftItem = draftPathResolver.ResolveItem(this, itemExpression);
                if (draftItem.NotExists())
                {
                    throw new AutomateException(
                        ExceptionMessages.DraftDefinition_ItemExpressionNotFound.Substitute(PatternName,
                            itemExpression));
                }
                target = draftItem;
            }

            return target;
        }

        private class AncestryPopulator : IDraftVisitor
        {
            private readonly Stack<DraftItem> ancestry = new();
            private readonly ToolkitDefinition toolkit;

            public AncestryPopulator(ToolkitDefinition toolkit)
            {
                toolkit.GuardAgainstNull(nameof(toolkit));

                this.toolkit = toolkit;
            }

            public bool VisitPatternEnter(DraftItem item)
            {
                this.ancestry.Push(item);

                item.SetAncestry(this.toolkit, null);
                return true;
            }

            public bool VisitPatternExit(DraftItem item)
            {
                this.ancestry.Clear();
                return true;
            }

            public bool VisitElementEnter(DraftItem item)
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

            public bool VisitElementExit(DraftItem item)
            {
                this.ancestry.Pop();
                return true;
            }

            public bool VisitEphemeralCollectionEnter(DraftItem item)
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

            public bool VisitEphemeralCollectionExit(DraftItem item)
            {
                this.ancestry.Pop();
                return true;
            }

            public bool VisitAttributeEnter(DraftItem item)
            {
                var parent = this.ancestry.Peek();
                if (parent.NotExists())
                {
                    throw new InvalidOperationException();
                }

                item.SetAncestry(this.toolkit, parent);
                return true;
            }

            public bool VisitAttributeExit(DraftItem item)
            {
                return true;
            }
        }

        private class CodeTemplateFinder : IDraftVisitor
        {
            private readonly string codeTemplateId;

            public CodeTemplateFinder(string codeTemplateId)
            {
                codeTemplateId.GuardAgainstNullOrEmpty(nameof(codeTemplateId));
                this.codeTemplateId = codeTemplateId;
                DraftItem = null;
            }

            public DraftItem DraftItem { get; private set; }

            public bool VisitPatternEnter(DraftItem item)
            {
                var codeTemplate = item.PatternSchema.FindCodeTemplateById(this.codeTemplateId);
                if (codeTemplate.Exists())
                {
                    DraftItem = item;
                    return false;
                }

                return true;
            }

            public bool VisitPatternExit(DraftItem item)
            {
                return true;
            }

            public bool VisitElementEnter(DraftItem item)
            {
                var codeTemplate = item.ElementSchema.FindCodeTemplateById(this.codeTemplateId);
                if (codeTemplate.Exists())
                {
                    DraftItem = item;
                    return false;
                }

                return true;
            }

            public bool VisitElementExit(DraftItem item)
            {
                return true;
            }
        }

        private class AutomationAggregator : IDraftVisitor
        {
            private readonly string automationId;

            public AutomationAggregator(string automationId)
            {
                automationId.GuardAgainstNullOrEmpty(nameof(automationId));

                this.automationId = automationId;
                Automation = new List<DraftItemCommandPair>();
            }

            public List<DraftItemCommandPair> Automation { get; }

            public bool VisitPatternEnter(DraftItem item)
            {
                var automation = item.PatternSchema.FindAutomationById(this.automationId);
                if (automation.Exists())
                {
                    Automation.Add(new DraftItemCommandPair(automation, item));
                }

                return true;
            }

            public bool VisitPatternExit(DraftItem item)
            {
                return true;
            }

            public bool VisitElementEnter(DraftItem item)
            {
                var automation = item.ElementSchema.Automation.Safe()
                    .FirstOrDefault(auto => auto.Id.EqualsIgnoreCase(this.automationId));
                if (automation.Exists())
                {
                    Automation.Add(new DraftItemCommandPair(automation, item));
                }

                return true;
            }

            public bool VisitElementExit(DraftItem item)
            {
                return true;
            }
        }
    }

    public class CommandsExecuted
    {
    }

    public class DraftItemCommandPair
    {
        public DraftItemCommandPair(IAutomationSchema automation, DraftItem draftItem)
        {
            Automation = automation;
            DraftItem = draftItem;
        }

        public IAutomationSchema Automation { get; }

        public DraftItem DraftItem { get; }
    }
}