using System.Collections.Generic;
using Automate.CLI.Extensions;
using ServiceStack;

namespace Automate.CLI.Domain
{
    internal class Element : IPatternElement, IValidateable, IPersistable
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
            Automation = new List<Automation>();
            Attributes = new List<Attribute>();
            Elements = new List<Element>();
        }

        private Element(PersistableProperties properties, IPersistableFactory factory)
        {
            Id = properties.Rehydrate<string>(factory, nameof(Id));
            Name = properties.Rehydrate<string>(factory, nameof(Name));
            DisplayName = properties.Rehydrate<string>(factory, nameof(DisplayName));
            Description = properties.Rehydrate<string>(factory, nameof(Description));
            IsCollection = properties.Rehydrate<bool>(factory, nameof(IsCollection));
            Cardinality = properties.Rehydrate<string>(factory, nameof(Cardinality)).ToEnumOrDefault(ElementCardinality.Single);
            Attributes = properties.Rehydrate<List<Attribute>>(factory, nameof(Attributes));
            Elements = properties.Rehydrate<List<Element>>(factory, nameof(Elements));
            Automation = properties.Rehydrate<List<Automation>>(factory, nameof(Automation));
            CodeTemplates = properties.Rehydrate<List<CodeTemplate>>(factory, nameof(CodeTemplates));
        }

        public ElementCardinality Cardinality { get; private set; }

        public string DisplayName { get; }

        public string Description { get; }

        public bool IsCollection { get; private set; }

        public PersistableProperties Dehydrate()
        {
            var properties = new PersistableProperties();
            properties.Dehydrate(nameof(Id), Id);
            properties.Dehydrate(nameof(Name), Name);
            properties.Dehydrate(nameof(DisplayName), DisplayName);
            properties.Dehydrate(nameof(Description), Description);
            properties.Dehydrate(nameof(IsCollection), IsCollection);
            properties.Dehydrate(nameof(Cardinality), Cardinality);
            properties.Dehydrate(nameof(Attributes), Attributes);
            properties.Dehydrate(nameof(Elements), Elements);
            properties.Dehydrate(nameof(CodeTemplates), CodeTemplates);
            properties.Dehydrate(nameof(Automation), Automation);

            return properties;
        }

        public static Element Rehydrate(PersistableProperties properties, IPersistableFactory factory)
        {
            return new Element(properties, factory);
        }

        public Element MakeStandalone()
        {
            var element = new Element(Name, DisplayName, Description, false, ElementCardinality.Single);
            Elements.ForEach(ele => element.Elements.Add(ele));
            Attributes.ForEach(attr => element.Attributes.Add(attr));
            Automation.ForEach(auto => element.Automation.Add(auto));
            CodeTemplates.ForEach(temp => element.CodeTemplates.Add(temp));

            return element;
        }

        public List<CodeTemplate> CodeTemplates { get; }

        public List<Automation> Automation { get; }

        public List<Attribute> Attributes { get; }

        public List<Element> Elements { get; }

        public string Id { get; }

        public string Name { get; }

        public ValidationResults Validate(ValidationContext context, object value)
        {
            return ValidationResults.None;
        }
    }
}