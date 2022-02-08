using System.Collections.Generic;
using automate.Extensions;

namespace automate.Domain
{
    internal class Element : IPatternElement
    {
        public Element(string name, string displayName, string description, bool isCollection)
        {
            name.GuardAgainstNullOrEmpty(nameof(name));
            name.GuardAgainstInvalid(Validations.IsNameIdentifier, nameof(name),
                ValidationMessages.InvalidNameIdentifier);

            Id = IdGenerator.Create();
            Name = name;
            DisplayName = displayName;
            Description = description;
            IsCollection = isCollection;
        }

        /// <summary>
        ///     For serialization
        /// </summary>
        public Element()
        {
        }

        public string DisplayName { get; set; }

        public string Description { get; set; }

        public bool IsCollection { get; set; }

        public List<CodeTemplate> CodeTemplates { get; set; } = new List<CodeTemplate>();

        public List<IAutomation> Automation { get; set; } = new List<IAutomation>();

        public List<Attribute> Attributes { get; set; } = new List<Attribute>();

        public List<Element> Elements { get; set; } = new List<Element>();

        public string Id { get; set; }

        public string Name { get; set; }
    }
}