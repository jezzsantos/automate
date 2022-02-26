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