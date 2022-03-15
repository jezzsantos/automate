using System.Linq;
using ServiceStack;

namespace Automate.CLI.Domain
{
    internal class Element : PatternElement, IValidateable, IPersistable
    {
        public Element(string name, string displayName = null, string description = null, bool isCollection = false,
            ElementCardinality? cardinality = null) : base(name)
        {
            DisplayName = displayName;
            Description = description;
            IsCollection = isCollection;
            Cardinality = cardinality.HasValue ? cardinality.Value : isCollection ? ElementCardinality.OneOrMany : ElementCardinality.Single;
        }

        private Element(PersistableProperties properties, IPersistableFactory factory) : base(properties, factory)
        {
            DisplayName = properties.Rehydrate<string>(factory, nameof(DisplayName));
            Description = properties.Rehydrate<string>(factory, nameof(Description));
            IsCollection = properties.Rehydrate<bool>(factory, nameof(IsCollection));
            Cardinality = properties.Rehydrate<string>(factory, nameof(Cardinality)).ToEnumOrDefault(ElementCardinality.Single);
        }

        public ElementCardinality Cardinality { get; private set; }

        public string DisplayName { get; }

        public string Description { get; }

        public bool IsCollection { get; private set; }

        public override PersistableProperties Dehydrate()
        {
            var properties = base.Dehydrate();
            properties.Dehydrate(nameof(DisplayName), DisplayName);
            properties.Dehydrate(nameof(Description), Description);
            properties.Dehydrate(nameof(IsCollection), IsCollection);
            properties.Dehydrate(nameof(Cardinality), Cardinality);

            return properties;
        }

        public static Element Rehydrate(PersistableProperties properties, IPersistableFactory factory)
        {
            return new Element(properties, factory);
        }

        public Element MakeStandalone()
        {
            var element = new Element(Name, DisplayName, Description, false, ElementCardinality.Single);
            Elements.ToList().ForEach(ele => element.AddElement(ele));
            Attributes.ToList().ForEach(attr => element.AddAttribute(attr));
            Automation.ToList().ForEach(auto => element.AddAutomation(auto));
            CodeTemplates.ToList().ForEach(temp => element.AddCodeTemplate(temp));

            return element;
        }

        public ValidationResults Validate(ValidationContext context, object value)
        {
            return ValidationResults.None;
        }
    }
}