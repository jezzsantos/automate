using System.Collections.Generic;
using Automate.CLI.Extensions;
using ServiceStack;

namespace Automate.CLI.Domain
{
    internal class Element : IPatternElement, IValidateable, ICloneable<Element>
    {
        public Element(string name, string displayName = null, string description = null, bool isCollection = false,
            ElementCardinality? cardinality = null)
        {
            name.GuardAgainstNullOrEmpty(nameof(name));
            name.GuardAgainstInvalid(Validations.IsNameIdentifier, nameof(name),
                ValidationMessages.InvalidNameIdentifier);

            Id = IdGenerator.Create();
            Name = name;
            DisplayName = displayName;
            Description = description;
            IsCollection = isCollection;
            Cardinality = cardinality.HasValue ? cardinality.Value : isCollection ? ElementCardinality.OneOrMany : ElementCardinality.Single;
            CodeTemplates = new List<CodeTemplate>();
            Automation = new List<IAutomation>();
            Attributes = new List<Attribute>();
            Elements = new List<Element>();
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

        public List<CodeTemplate> CodeTemplates { get; set; }

        public List<IAutomation> Automation { get; set; }

        public List<Attribute> Attributes { get; set; }

        public List<Element> Elements { get; set; }

        public string Id { get; set; }

        public string Name { get; set; }

        public ValidationResults Validate(ValidationContext context, object value)
        {
            return ValidationResults.None;
        }
    }
}