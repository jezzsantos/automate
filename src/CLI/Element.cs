using System.Collections.Generic;
using automate.Extensions;

namespace automate
{
    internal class Element : INamedEntity, IElementContainer, IAutomationContainer, ICustomizableEntity
    {
        public Element(string name, string displayName, string description, bool isCollection)
        {
            name.GuardAgainstNullOrEmpty(nameof(name));
            name.GuardAgainstInvalid(Validations.IsIdentifier, nameof(name),
                ExceptionMessages.Validations_InvalidIdentifier);

            Id = IdGenerator.Create();
            Name = name;
            DisplayName = displayName;
            Description = description;
            IsCollection = isCollection;
            Attributes = new List<Attribute>();
            Elements = new List<Element>();
            CodeTemplates = new List<CodeTemplate>();
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

        public List<CodeTemplate> CodeTemplates { get; set; }

        public List<Attribute> Attributes { get; set; }

        public List<Element> Elements { get; set; }

        public string Id { get; set; }

        public string Name { get; set; }
    }
}