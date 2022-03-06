using System;
using System.Collections.Generic;
using System.Linq;
using Automate.CLI.Extensions;

namespace Automate.CLI.Domain
{
    internal class PatternDefinition : IPatternElement, IValidateable
    {
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
            Automation = new List<IAutomation>();
            Attributes = new List<Attribute>();
            Elements = new List<Element>();
        }

        /// <summary>
        ///     For serialization
        /// </summary>
        public PatternDefinition()
        {
        }

        public string DisplayName { get; set; }

        public string Description { get; set; }

        public string ToolkitVersion { get; set; }

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

        public IAutomation FindAutomation(string commandId)
        {
            return FindDescendantAutomation(this);

            IAutomation FindDescendantAutomation(IPatternElement element)
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
            var solution = new SolutionDefinition(new ToolkitDefinition(this, "0.0"));

            PopulateDescendants(solution.Model, 1);

            void PopulateDescendants(SolutionItem solutionItem, int instanceCountAtThisLevel)
            {
                if (solutionItem.IsPattern)
                {
                    PopulateParent(solutionItem, solutionItem.PatternSchema, instanceCountAtThisLevel);
                }
                if (solutionItem.IsElement)
                {
                    if (!solutionItem.IsMaterialised)
                    {
                        solutionItem.Materialise();
                    }
                    PopulateParent(solutionItem, solutionItem.ElementSchema, instanceCountAtThisLevel);
                }
                if (solutionItem.IsCollection)
                {
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

            void PopulateParent(SolutionItem solutionItem, IPatternElement element, int instanceCountAtThisLevel)
            {
                element.Attributes.ToListSafe().ForEach(attr => { PopulateAttribute(solutionItem, attr, instanceCountAtThisLevel); });
                element.Elements.ToListSafe().ForEach(ele => { PopulateDescendants(solutionItem.Properties[ele.Name], instanceCountAtThisLevel); });
            }

            void PopulateAttribute(SolutionItem solutionItem, Attribute attribute, int instanceCountAtThisLevel)
            {
                var hasProperty = solutionItem.Properties.ContainsKey(attribute.Name);
                if (!hasProperty || !solutionItem.Properties[attribute.Name].IsAttribute)
                {
                    solutionItem.Properties.Add(attribute.Name,
                        new SolutionItem(attribute, solutionItem));
                }

                if (!attribute.DefaultValue.HasValue())
                {
                    object testValue;
                    if (attribute.Choices.HasAny())
                    {
                        var choiceIndex = (instanceCountAtThisLevel - 1) % attribute.Choices.Count;
                        testValue = attribute.Choices[choiceIndex];
                    }
                    else
                    {
                        testValue = attribute.DataType switch
                        {
                            Attribute.DefaultType => $"{attribute.Name.ToLower()}{instanceCountAtThisLevel}",
                            "bool" => instanceCountAtThisLevel % 2 != 0,
                            "int" => instanceCountAtThisLevel,
                            "decimal" => Convert.ToDecimal($"{instanceCountAtThisLevel}.{instanceCountAtThisLevel}"),
                            "DateTime" => DateTime.Today.AddHours(instanceCountAtThisLevel).ToUniversalTime(),
                            _ => null
                        };
                    }

                    solutionItem.Properties[attribute.Name].Value = testValue;
                }
            }
        }

        public List<CodeTemplate> CodeTemplates { get; set; }

        public List<IAutomation> Automation { get; set; }

        public List<Attribute> Attributes { get; set; }

        public List<Element> Elements { get; set; }

        public string Name { get; set; }

        public string Id { get; set; }

        public ValidationResults Validate(ValidationContext context, object value)
        {
            return ValidationResults.None;
        }
    }
}