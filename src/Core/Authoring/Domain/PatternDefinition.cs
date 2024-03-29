﻿using System;
using System.Collections.Generic;
using System.Linq;
using Automate.Common.Domain;
using Automate.Common.Extensions;
using Automate.Runtime.Domain;

namespace Automate.Authoring.Domain
{
    public class PatternDefinition : PatternElement, IValidateable, IPersistable
    {
        public PatternDefinition(string name, string displayName = null,
            string description = null) : base(name, displayName, description)
        {
            Description = description;
            ToolkitVersion = new PatternVersioningHistory();
            RuntimeVersion = MachineConstants.GetRuntimeVersion().ToString();
        }

        private PatternDefinition(PersistableProperties properties,
            IPersistableFactory factory) : base(properties,
            factory)
        {
            ToolkitVersion = properties.Rehydrate<PatternVersioningHistory>(factory, nameof(ToolkitVersion));
            RuntimeVersion = properties.Rehydrate<string>(factory, nameof(RuntimeVersion));
        }

        public PatternVersioningHistory ToolkitVersion { get; private set; }

        public string RuntimeVersion { get; private set; }

        public override PersistableProperties Dehydrate()
        {
            var properties = base.Dehydrate();
            properties.Dehydrate(nameof(ToolkitVersion), ToolkitVersion);
            properties.Dehydrate(nameof(RuntimeVersion), RuntimeVersion);

            return properties;
        }

        public static PatternDefinition Rehydrate(PersistableProperties properties,
            IPersistableFactory factory)
        {
            var instance = new PatternDefinition(properties, factory);
            instance.PopulateAncestry();

            return instance;
        }

        public List<(CodeTemplate Template, IPatternElement Parent)> GetAllCodeTemplates()
        {
            var aggregator = new CodeTemplateAggregator();
            TraverseDescendants(aggregator);
            return aggregator.CodeTemplates;
        }

        public List<(CommandLaunchPoint LaunchPoint, IPatternElement Parent)> GetAllLaunchPoints()
        {
            var aggregator = new LaunchPointAggregator();
            TraverseDescendants(aggregator);
            return aggregator.LaunchPoints;
        }

        public Automation FindAutomation(string id, Predicate<Automation> where)
        {
            var finder = new AutomationFinder(id, where);
            TraverseDescendants(finder);
            return finder.Automation;
        }

        public DraftDefinition CreateTestDraft()
        {
            const int maxNumberInstances = 3;

            var draft = new DraftDefinition(new ToolkitDefinition(this));
            PopulateDescendants(draft.Model, 1);
            return draft;

            void PopulateDescendants(DraftItem draftItem, int instanceCountAtThisLevel)
            {
                if (draftItem.IsPattern)
                {
                    PopulatePatternElement(draftItem, draftItem.PatternSchema, instanceCountAtThisLevel);
                }
                if (draftItem.IsElement)
                {
                    if (!draftItem.IsMaterialised)
                    {
                        draftItem.Materialise();
                    }
                    PopulatePatternElement(draftItem, draftItem.ElementSchema, instanceCountAtThisLevel);
                }
                if (draftItem.IsEphemeralCollection)
                {
                    draftItem.MaterialiseCollectionItem();
                    var existingCount = draftItem.Items.Safe().Count();
                    if (draftItem.ElementSchema.HasCardinalityOfMany() && existingCount < maxNumberInstances)
                    {
                        Repeat.Times(() => { draftItem.MaterialiseCollectionItem(); },
                            maxNumberInstances - existingCount);
                    }

                    var counter = 0;
                    draftItem.Items.ToListSafe().ForEach(itm => { PopulateDescendants(itm, ++counter); });
                }
            }

            void PopulatePatternElement(DraftItem draftItem, IPatternElementSchema schema,
                int instanceCountAtThisLevel)
            {
                schema.Attributes.ToListSafe().ForEach(attr =>
                {
                    PopulateAttribute(draftItem, attr, instanceCountAtThisLevel);
                });
                schema.Elements.ToListSafe().ForEach(ele =>
                {
                    PopulateDescendants(draftItem.Properties[ele.Name], instanceCountAtThisLevel);
                });
            }

            void PopulateAttribute(DraftItem draftItem, IAttributeSchema schema, int instanceCountAtThisLevel)
            {
                var prop = draftItem.GetProperty(schema.Name);
                if (!prop.HasDefaultValue)
                {
                    object testValue;
                    if (prop.ChoiceValues.HasAny())
                    {
                        var choiceIndex = (instanceCountAtThisLevel - 1) % schema.Choices.Count;
                        testValue = prop.ChoiceValues[choiceIndex];
                    }
                    else
                    {
                        testValue = prop.DataType switch
                        {
                            Attribute.DefaultType => $"{schema.Name.ToLower()}{instanceCountAtThisLevel}",
                            "bool" => instanceCountAtThisLevel % 2 != 0,
                            "int" => instanceCountAtThisLevel,
                            "float" => Convert.ToDouble($"{instanceCountAtThisLevel}.{instanceCountAtThisLevel}"),
                            "datetime" => DateTime.Today.AddHours(instanceCountAtThisLevel).ToUniversalTime(),
                            _ => null
                        };
                    }
                    prop.SetValue(testValue);
                }
            }
        }

        public VersionUpdateResult UpdateToolkitVersion(VersionInstruction instruction)
        {
            return ToolkitVersion.UpdateVersion(instruction);
        }

        public TSchema FindSchema<TSchema>(string id) where TSchema : IIdentifiableEntity
        {
            var finder = new SchemaFinder<TSchema>(id);
            TraverseDescendants(finder);
            return finder.Schema;
        }

        public override bool Accept(IPatternVisitor visitor)
        {
            if (visitor.VisitPatternEnter(this))
            {
                base.Accept(visitor);
            }

            return visitor.VisitPatternExit(this);
        }

        public void SyncChanges(IRuntimeMetadata metadata,
            Func<PatternDefinition, CodeTemplate, CodeTemplateContent> getTemplateContent)
        {
            SyncRuntimeChanges(metadata);
            SyncCodeTemplatesChanges(getTemplateContent);
        }

        /// <summary>
        ///     This [hierarchical] traversal requires that we enter and exit composite nodes (i.e. the
        ///     <see cref="PatternDefinition" /> and <see cref="PatternElement" />) that themselves are composed of other nodes or
        ///     composite nodes.
        ///     Once 'entered' a specific composite node, we traverse each of its child nodes (and their child nodes), before
        ///     'exiting' this specific composite node.
        ///     In this way we can tell the difference between a sibling node and a child node (of the same type).
        ///     Furthermore, each visit returns to us whether we are to abort visiting that branch of the graph, and we return up
        ///     to the root node, or continue down that branch, and on to sibling branches.
        ///     In other words, an aborted 'enter', must result in an 'exit', but then followed by returning up that branch of
        ///     nodes to the root of the graph, before exiting the traversal.
        /// </summary>
        public void TraverseDescendants(IPatternVisitor visitor)
        {
            Accept(visitor);
        }

#if TESTINGONLY
        public void ResetRuntimeVersion()
        {
            RuntimeVersion = null;
        }
#endif

        public ValidationResults Validate(ValidationContext context, object value)
        {
            return ValidationResults.None;
        }

        internal void SyncRuntimeChanges(IRuntimeMetadata metadata)
        {
            if (RuntimeVersion.HasNoValue()) //introduced at 1.0.5
            {
                return;
            }

            var installed = metadata.RuntimeVersion;
            var published = RuntimeVersion.ToSemVersion();
            if (installed.Major > published.Major)
            {
                ToolkitVersion.RegisterChange(VersionChange.NonBreaking,
                    VersionChanges.Pattern_RuntimeVersion_Update, Id, published.ToString(), installed.ToString());
            }
            RuntimeVersion = installed.ToString();
        }

        private void SyncCodeTemplatesChanges(
            Func<PatternDefinition, CodeTemplate, CodeTemplateContent> getTemplateContent)
        {
            GetAllCodeTemplates()
                .ForEach(template =>
                {
                    var lastModified = template.Template.LastModifiedUtc;
                    var lastChangedInStore = getTemplateContent(this, template.Template).LastModifiedUtc;

                    if (!lastModified.IsNear(lastChangedInStore, TimeSpan.FromSeconds(1)))
                    {
                        template.Template.UpdateLastModified(lastChangedInStore);
                        ToolkitVersion.RegisterChange(VersionChange.NonBreaking,
                            VersionChanges.Pattern_CodeTemplates_Update, template.Template.Id, template.Parent.Id);
                    }
                });
        }

        private void PopulateAncestry()
        {
            var populator = new AncestryPopulator();
            TraverseDescendants(populator);
        }

        private class AncestryPopulator : IPatternVisitor
        {
            private readonly Stack<PatternElement> ancestry = new();

            public bool VisitPatternEnter(PatternDefinition pattern)
            {
                this.ancestry.Push(pattern);
                return true;
            }

            public bool VisitPatternExit(PatternDefinition pattern)
            {
                this.ancestry.Clear();
                return true;
            }

            public bool VisitElementEnter(Element element)
            {
                var parent = this.ancestry.Peek();
                if (parent.NotExists())
                {
                    throw new InvalidOperationException();
                }
                this.ancestry.Push(element);

                element.SetParent(parent);

                return true;
            }

            public bool VisitElementExit(Element element)
            {
                this.ancestry.Pop();
                return true;
            }

            public bool VisitAttribute(Attribute attribute)
            {
                var parent = this.ancestry.Peek();
                if (parent.NotExists())
                {
                    throw new InvalidOperationException();
                }

                attribute.SetParent(parent);

                return true;
            }

            public bool VisitAutomation(Automation automation)
            {
                var parent = this.ancestry.Peek();
                if (parent.NotExists())
                {
                    throw new InvalidOperationException();
                }

                automation.SetParent(parent);

                return true;
            }
        }

        private class AutomationFinder : IPatternVisitor
        {
            private readonly string id;
            private readonly Predicate<Automation> where;

            public AutomationFinder(string id, Predicate<Automation> where)
            {
                id.GuardAgainstNullOrEmpty(nameof(id));
                where.GuardAgainstNull(nameof(where));
                this.id = id;
                this.where = where;
                Automation = null;
            }

            public Automation Automation { get; private set; }

            public bool VisitAutomation(Automation automation)
            {
                if (automation.Id.EqualsIgnoreCase(this.id) && this.where(automation))
                {
                    Automation = automation;
                    return false;
                }

                return true;
            }
        }

        private class SchemaFinder<TSchema> : IPatternVisitor
        {
            private readonly string id;

            public SchemaFinder(string id)
            {
                id.GuardAgainstNullOrEmpty(nameof(id));

                this.id = id;
                Schema = default;
            }

            public TSchema Schema { get; private set; }

            public bool VisitPatternEnter(PatternDefinition pattern)
            {
                return VisitNode(pattern);
            }

            public bool VisitElementEnter(Element element)
            {
                return VisitNode(element);
            }

            public bool VisitAttribute(Attribute attribute)
            {
                return VisitNode(attribute);
            }

            private bool VisitNode(IIdentifiableEntity node)
            {
                if (node.Id.EqualsIgnoreCase(this.id))
                {
                    if (node is TSchema schema)
                    {
                        Schema = schema;
                    }
                    else
                    {
                        Schema = default;
                    }

                    return false;
                }

                return true;
            }
        }

        private class LaunchPointAggregator : IPatternVisitor
        {
            private IPatternElement parent;

            public LaunchPointAggregator()
            {
                LaunchPoints = new List<(CommandLaunchPoint LaunchPoint, IPatternElement Parent)>();
            }

            public List<(CommandLaunchPoint LaunchPoint, IPatternElement Parent)> LaunchPoints { get; }

            public bool VisitAutomation(Automation automation)
            {
                if (this.parent.NotExists())
                {
                    throw new InvalidOperationException();
                }

                if (automation.Type == AutomationType.CommandLaunchPoint)
                {
                    LaunchPoints.Add((CommandLaunchPoint.FromAutomation(automation), this.parent));
                }

                return true;
            }

            public bool VisitPatternEnter(PatternDefinition pattern)
            {
                this.parent = pattern;
                return true;
            }

            public bool VisitPatternExit(PatternDefinition pattern)
            {
                this.parent = null;
                return true;
            }

            public bool VisitElementEnter(Element element)
            {
                this.parent = element;
                return true;
            }

            public bool VisitElementExit(Element element)
            {
                this.parent = null;
                return true;
            }
        }

        private class CodeTemplateAggregator : IPatternVisitor
        {
            private IPatternElement parent;

            public CodeTemplateAggregator()
            {
                CodeTemplates = new List<(CodeTemplate Template, IPatternElement Parent)>();
            }

            public List<(CodeTemplate Template, IPatternElement Parent)> CodeTemplates { get; }

            public bool VisitCodeTemplate(CodeTemplate codeTemplate)
            {
                if (this.parent.NotExists())
                {
                    throw new InvalidOperationException();
                }

                CodeTemplates.Add((codeTemplate, this.parent));
                return true;
            }

            public bool VisitPatternEnter(PatternDefinition pattern)
            {
                this.parent = pattern;
                return true;
            }

            public bool VisitPatternExit(PatternDefinition pattern)
            {
                this.parent = null;
                return true;
            }

            public bool VisitElementEnter(Element element)
            {
                this.parent = element;
                return true;
            }

            public bool VisitElementExit(Element element)
            {
                this.parent = null;
                return true;
            }
        }
    }
}