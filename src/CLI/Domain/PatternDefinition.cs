using System;
using System.Collections.Generic;
using System.Linq;
using Automate.CLI.Extensions;

namespace Automate.CLI.Domain
{
    internal class PatternDefinition : PatternElement, IValidateable, IPersistable
    {
        public PatternDefinition(string name) : base(name)
        {
            DisplayName = name;
            Description = null;
            ToolkitVersion = new ToolkitVersion();
        }

        private PatternDefinition(PersistableProperties properties, IPersistableFactory factory) : base(properties, factory)
        {
            DisplayName = properties.Rehydrate<string>(factory, nameof(DisplayName));
            Description = properties.Rehydrate<string>(factory, nameof(Description));
            ToolkitVersion = properties.Rehydrate<ToolkitVersion>(factory, nameof(ToolkitVersion));
        }

        public string DisplayName { get; }

        public string Description { get; }

        public ToolkitVersion ToolkitVersion { get; private set; }

        public override PersistableProperties Dehydrate()
        {
            var properties = base.Dehydrate();
            properties.Dehydrate(nameof(DisplayName), DisplayName);
            properties.Dehydrate(nameof(Description), Description);
            properties.Dehydrate(nameof(ToolkitVersion), ToolkitVersion);

            return properties;
        }

        public static PatternDefinition Rehydrate(PersistableProperties properties, IPersistableFactory factory)
        {
            var instance = new PatternDefinition(properties, factory);
            instance.PopulateAncestry();

            return instance;
        }

        public List<(CodeTemplate Template, IPatternElement Parent)> GetAllCodeTemplates()
        {
            var aggregator = new CodeTemplateAggregator();
            TraversePattern(aggregator);
            return aggregator.CodeTemplates;
        }

        public List<(CommandLaunchPoint LaunchPoint, IPatternElement Parent)> GetAllLaunchPoints()
        {
            var aggregator = new LaunchPointAggregator();
            TraversePattern(aggregator);
            return aggregator.LaunchPoints;
        }

        public Automation FindAutomation(string id, Predicate<Automation> where)
        {
            var finder = new AutomationFinder(id, where);
            TraversePattern(finder);
            return finder.Automation;
        }

        public SolutionDefinition CreateTestSolution()
        {
            var creator = new TestDataCreator(this);
            TraversePattern(creator);
            return creator.Solution;
        }

        public VersionUpdateResult UpdateToolkitVersion(VersionInstruction instruction)
        {
            return ToolkitVersion.UpdateVersion(instruction);
        }

        public TSchema FindSchema<TSchema>(string id) where TSchema : IIdentifiableEntity
        {
            var finder = new SchemaFinder<TSchema>(id);
            TraversePattern(finder);
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

        public ValidationResults Validate(ValidationContext context, object value)
        {
            return ValidationResults.None;
        }

        internal void TraversePattern(IPatternVisitor visitor)
        {
            Accept(visitor);
        }

        private void PopulateAncestry()
        {
            var populator = new AncestryPopulator();
            TraversePattern(populator);
        }

        private class TestDataCreator : IPatternVisitor
        {
            private const int MaxNumberInstances = 3;

            public TestDataCreator(PatternDefinition pattern)
            {
                pattern.GuardAgainstNull(nameof(pattern));
                Solution = new SolutionDefinition(new ToolkitDefinition(pattern));

                PopulateDescendants(Solution.Model, 1);
            }

            public SolutionDefinition Solution { get; }

            private void PopulateDescendants(SolutionItem solutionItem, int instanceCountAtThisLevel)
            {
                if (solutionItem.IsPattern)
                {
                    PopulatePatternElement(solutionItem, solutionItem.PatternSchema, instanceCountAtThisLevel);
                }
                if (solutionItem.IsElement)
                {
                    if (!solutionItem.IsMaterialised)
                    {
                        solutionItem.Materialise();
                    }
                    PopulatePatternElement(solutionItem, solutionItem.ElementSchema, instanceCountAtThisLevel);
                }
                if (solutionItem.IsEphemeralCollection)
                {
                    solutionItem.MaterialiseCollectionItem();
                    var existingCount = solutionItem.Items.Safe().Count();
                    if (solutionItem.ElementSchema.HasCardinalityOfMany() && existingCount < MaxNumberInstances)
                    {
                        Repeat.Times(() => { solutionItem.MaterialiseCollectionItem(); }, MaxNumberInstances - existingCount);
                    }

                    var counter = 0;
                    solutionItem.Items.ToListSafe().ForEach(itm => { PopulateDescendants(itm, ++counter); });
                }
            }

            private void PopulatePatternElement(SolutionItem solutionItem, IPatternElementSchema schema, int instanceCountAtThisLevel)
            {
                schema.Attributes.ToListSafe().ForEach(attr => { PopulateAttribute(solutionItem, attr, instanceCountAtThisLevel); });
                schema.Elements.ToListSafe().ForEach(ele => { PopulateDescendants(solutionItem.Properties[ele.Name], instanceCountAtThisLevel); });
            }

            private void PopulateAttribute(SolutionItem solutionItem, IAttributeSchema schema, int instanceCountAtThisLevel)
            {
                var prop = solutionItem.GetProperty(schema.Name);
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
                            "decimal" => Convert.ToDecimal($"{instanceCountAtThisLevel}.{instanceCountAtThisLevel}"),
                            "DateTime" => DateTime.Today.AddHours(instanceCountAtThisLevel).ToUniversalTime(),
                            _ => null
                        };
                    }
                    prop.SetProperty(testValue);
                }
            }
        }

        private class AncestryPopulator : IPatternVisitor
        {
            private readonly Queue<PatternElement> ancestry = new Queue<PatternElement>();

            public bool VisitPatternEnter(PatternDefinition pattern)
            {
                this.ancestry.Enqueue(pattern);
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
                this.ancestry.Enqueue(element);

                element.SetParent(parent);

                return true;
            }

            public bool VisitElementExit(Element element)
            {
                this.ancestry.Dequeue();
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