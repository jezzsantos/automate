using System;
using System.Collections.Generic;
using System.Linq;
using Automate.CLI.Extensions;

namespace Automate.CLI.Domain
{
    internal class PatternDefinition : IPatternElement, IValidateable, IPersistable
    {
        internal static readonly Version DefaultVersionNumber = new Version(0, 0, 0);

        public PatternDefinition(string name)
        {
            name.GuardAgainstNullOrEmpty(nameof(name));
            name.GuardAgainstInvalid(Validations.IsNameIdentifier, nameof(name),
                ValidationMessages.InvalidNameIdentifier);

            Name = name;
            Id = IdGenerator.Create();
            DisplayName = name;
            Description = null;
            CodeTemplates = new List<CodeTemplate>();
            Automation = new List<Automation>();
            Attributes = new List<Attribute>();
            Elements = new List<Element>();
            ToolkitVersion = DefaultVersionNumber.ToString(2);
        }

        private PatternDefinition(PersistableProperties properties, IPersistableFactory factory)
        {
            Id = properties.Rehydrate<string>(factory, nameof(Id));
            Name = properties.Rehydrate<string>(factory, nameof(Name));
            DisplayName = properties.Rehydrate<string>(factory, nameof(DisplayName));
            Description = properties.Rehydrate<string>(factory, nameof(Description));
            ToolkitVersion = properties.Rehydrate<string>(factory, nameof(ToolkitVersion));
            Attributes = properties.Rehydrate<List<Attribute>>(factory, nameof(Attributes));
            Elements = properties.Rehydrate<List<Element>>(factory, nameof(Elements));
            CodeTemplates = properties.Rehydrate<List<CodeTemplate>>(factory, nameof(CodeTemplates));
            Automation = properties.Rehydrate<List<Automation>>(factory, nameof(Automation));
        }

        public string DisplayName { get; }

        public string Description { get; }

        public string ToolkitVersion { get; private set; }

        public PersistableProperties Dehydrate()
        {
            var properties = new PersistableProperties();
            properties.Dehydrate(nameof(Id), Id);
            properties.Dehydrate(nameof(Name), Name);
            properties.Dehydrate(nameof(DisplayName), DisplayName);
            properties.Dehydrate(nameof(Description), Description);
            properties.Dehydrate(nameof(ToolkitVersion), ToolkitVersion);
            properties.Dehydrate(nameof(Attributes), Attributes);
            properties.Dehydrate(nameof(Elements), Elements);
            properties.Dehydrate(nameof(CodeTemplates), CodeTemplates);
            properties.Dehydrate(nameof(Automation), Automation);

            return properties;
        }

        public static PatternDefinition Rehydrate(PersistableProperties properties, IPersistableFactory factory)
        {
            return new PatternDefinition(properties, factory);
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

        public Automation FindAutomation(string commandId)
        {
            return FindDescendantAutomation(this);

            Automation FindDescendantAutomation(IPatternElement element)
            {
                var automation = element.Automation.Safe()
                    .FirstOrDefault(auto => auto.Id.EqualsIgnoreCase(commandId));
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
            var solution = new SolutionDefinition(new ToolkitDefinition(this, new Version(0, 0, 0, 0).ToString(2)));

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

            solution.PopulateAncestry();
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

        public void UpdateToolkitVersion(string version)
        {
            ToolkitVersion = version;
        }

        public List<CodeTemplate> CodeTemplates { get; }

        public List<Automation> Automation { get; }

        public List<Attribute> Attributes { get; }

        public List<Element> Elements { get; }

        public string Name { get; }

        public string Id { get; }

        public ValidationResults Validate(ValidationContext context, object value)
        {
            return ValidationResults.None;
        }
    }
}