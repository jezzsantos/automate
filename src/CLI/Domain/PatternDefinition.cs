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

        public List<CodeTemplate> GetAllCodeTemplates()
        {
            var templates = new List<CodeTemplate>();
            AggregateDescendantTemplates(this);

            void AggregateDescendantTemplates(IPatternElement element)
            {
                element.CodeTemplates.ToListSafe().ForEach(tem => templates.Add(tem));
                element.Elements.ToListSafe().ForEach(AggregateDescendantTemplates);
            }

            return templates;
        }

        public SolutionDefinition CreateTestSolution()
        {
            const int maxNumberInstances = 3;
            var solution = new SolutionDefinition(new ToolkitDefinition(this));

            PopulateDescendants(solution.Model, 1);

            void PopulateDescendants(SolutionItem solutionItem, int instanceCountAtThisLevel)
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
                if (solutionItem.IsCollection)
                {
                    solutionItem.MaterialiseCollectionItem();
                    var existingCount = solutionItem.Items.Safe().Count();
                    if (solutionItem.ElementSchema.HasCardinalityOfMany() && existingCount < maxNumberInstances)
                    {
                        Repeat.Times(() => { solutionItem.MaterialiseCollectionItem(); }, maxNumberInstances - existingCount);
                    }

                    var counter = 0;
                    solutionItem.Items.ToListSafe().ForEach(itm => { PopulateDescendants(itm, ++counter); });
                }
            }

            return solution;

            void PopulatePatternElement(SolutionItem solutionItem, IPatternElement element, int instanceCountAtThisLevel)
            {
                element.Attributes.ToListSafe().ForEach(attr => { PopulateAttribute(solutionItem, attr, instanceCountAtThisLevel); });
                element.Elements.ToListSafe().ForEach(ele => { PopulateDescendants(solutionItem.Properties[ele.Name], instanceCountAtThisLevel); });
            }

            void PopulateAttribute(SolutionItem solutionItem, Attribute attribute, int instanceCountAtThisLevel)
            {
                var prop = solutionItem.GetProperty(attribute.Name);
                if (!prop.HasDefaultValue)
                {
                    object testValue;
                    if (prop.ChoiceValues.HasAny())
                    {
                        var choiceIndex = (instanceCountAtThisLevel - 1) % attribute.Choices.Count;
                        testValue = prop.ChoiceValues[choiceIndex];
                    }
                    else
                    {
                        testValue = prop.DataType switch
                        {
                            Attribute.DefaultType => $"{attribute.Name.ToLower()}{instanceCountAtThisLevel}",
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

        public VersionUpdateResult UpdateToolkitVersion(string versionInstruction)
        {
            return ToolkitVersion.UpdateVersion(versionInstruction);
        }

        public ValidationResults Validate(ValidationContext context, object value)
        {
            return ValidationResults.None;
        }

        private void PopulateAncestry()
        {
            PopulateDescendantParents(this, null);

            void PopulateDescendantParents(PatternElement element, PatternElement parent)
            {
                element.SetParent(parent);
                var elements = element.Elements.Safe();
                foreach (var ele in elements)
                {
                    PopulateDescendantParents(ele, element);
                }
            }
        }
    }
}