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
            var templates = new List<(CodeTemplate Template, IPatternElement Parent)>();
            AggregateDescendantTemplates(this);

            void AggregateDescendantTemplates(IPatternElement element)
            {
                element.CodeTemplates.ToListSafe().ForEach(tem => templates.Add((tem, element)));
                element.Elements.ToListSafe().ForEach(AggregateDescendantTemplates);
            }

            return templates;
        }

        public List<(CommandLaunchPoint LaunchPoint, IPatternElement Parent)> GetAllLaunchPoints()
        {
            var launchPoints = new List<(CommandLaunchPoint LaunchPoint, IPatternElement Parent)>();
            AggregateDescendantLaunchPoints(this);

            void AggregateDescendantLaunchPoints(IPatternElement element)
            {
                element.Automation
                    .Where(auto => auto.Type == AutomationType.CommandLaunchPoint)
                    .ToListSafe().ForEach(auto => { launchPoints.Add((CommandLaunchPoint.FromAutomation(auto), element)); });
                element.Elements.ToListSafe().ForEach(AggregateDescendantLaunchPoints);
            }

            return launchPoints;
        }

        public Automation FindAutomation(string id, Predicate<Automation> where)
        {
            return FindDescendantAutomation(this);

            Automation FindDescendantAutomation(IPatternElement element)
            {
                var automation = element.Automation.Safe()
                    .FirstOrDefault(auto => auto.Id.EqualsIgnoreCase(id) && where(auto));
                if (automation.Exists())
                {
                    return automation;
                }
                return element.Elements.Safe()
                    .Select(FindDescendantAutomation)
                    .FirstOrDefault(auto => auto.Exists());
            }
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

            void PopulatePatternElement(SolutionItem solutionItem, IPatternElementSchema schema, int instanceCountAtThisLevel)
            {
                schema.Attributes.ToListSafe().ForEach(attr => { PopulateAttribute(solutionItem, attr, instanceCountAtThisLevel); });
                schema.Elements.ToListSafe().ForEach(ele => { PopulateDescendants(solutionItem.Properties[ele.Name], instanceCountAtThisLevel); });
            }

            void PopulateAttribute(SolutionItem solutionItem, IAttributeSchema schema, int instanceCountAtThisLevel)
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

        public VersionUpdateResult UpdateToolkitVersion(VersionInstruction instruction)
        {
            return ToolkitVersion.UpdateVersion(instruction);
        }

        public TSchema FindSchema<TSchema>(string id) where TSchema : IIdentifiableEntity
        {
            return FindDescendantSchema(this);

            TSchema FindDescendantSchema(object descendant)
            {
                if (descendant is PatternDefinition pattern)
                {
                    if (pattern.Id.EqualsIgnoreCase(id))
                    {
                        return pattern is TSchema schema
                            ? schema
                            : default;
                    }
                    var elementOrAttribute = FindDescendantPatternElementSchema(pattern);
                    if (elementOrAttribute.Exists())
                    {
                        return elementOrAttribute;
                    }
                }
                else if (descendant is Element element)
                {
                    if (element.Id.EqualsIgnoreCase(id))
                    {
                        return element is TSchema schema
                            ? schema
                            : default;
                    }

                    var elementOrAttribute = FindDescendantPatternElementSchema(element);
                    if (elementOrAttribute.Exists())
                    {
                        return elementOrAttribute;
                    }
                }
                else if (descendant is Attribute attribute)
                {
                    if (attribute.Id.EqualsIgnoreCase(id))
                    {
                        return attribute is TSchema schema
                            ? schema
                            : default;
                    }
                }

                return default;
            }

            TSchema FindDescendantPatternElementSchema(IPatternElement patternElement)
            {
                var element = patternElement.Elements.Safe()
                    .Select(FindDescendantSchema)
                    .FirstOrDefault(schema => schema.Exists());
                if (element.Exists())
                {
                    return element;
                }
                var attribute = patternElement.Attributes.Safe()
                    .Select(FindDescendantSchema)
                    .FirstOrDefault(schema => schema.Exists());
                if (attribute.Exists())
                {
                    return attribute;
                }

                return default;
            }
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