using System.Collections.Generic;
using automate.Extensions;

namespace automate.Domain
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

        public List<CodeTemplate> CodeTemplates { get; set; } = new List<CodeTemplate>();

        public List<IAutomation> Automation { get; set; } = new List<IAutomation>();

        public List<Attribute> Attributes { get; set; } = new List<Attribute>();

        public List<Element> Elements { get; set; } = new List<Element>();

        public string Name { get; set; }

        public string Id { get; set; }

        public ValidationResults Validate(ValidationContext context, object value)
        {
            return ValidationResults.None;
        }
    }
}