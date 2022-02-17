using System.Collections.Generic;
using Automate.CLI.Extensions;
using ServiceStack;

namespace Automate.CLI.Domain
{
    internal class Element : IPatternElement, IValidateable, ICloneable<Element>
    {
        public Element(string name, string displayName = null, string description = null, bool isCollection = false,
            ElementCardinality cardinality = ElementCardinality.Single)
        {
            name.GuardAgainstNullOrEmpty(nameof(name));
            name.GuardAgainstInvalid(Validations.IsNameIdentifier, nameof(name),
                ValidationMessages.InvalidNameIdentifier);

            Id = IdGenerator.Create();
            Name = name;
            DisplayName = displayName;
            Description = description;
            IsCollection = isCollection;
            Cardinality = cardinality;
        }

        /// <summary>
        ///     For serialization
        /// </summary>
        public Element()
        {
        }

        public ElementCardinality Cardinality { get; set; }

        public string DisplayName { get; set; }

        public string Description { get; set; }

        public bool IsCollection { get; set; }

        public Element Clone()
        {
            return this.CreateCopy();
        }

        public List<CodeTemplate> CodeTemplates { get; set; } = new List<CodeTemplate>();

        public List<IAutomation> Automation { get; set; } = new List<IAutomation>();

        public List<Attribute> Attributes { get; set; } = new List<Attribute>();

        public List<Element> Elements { get; set; } = new List<Element>();

        public string Id { get; set; }

        public string Name { get; set; }

        public ValidationResults Validate(ValidationContext context, object value)
        {
            return ValidationResults.None;
        }
    }
}