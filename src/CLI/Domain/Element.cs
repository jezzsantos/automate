using Automate.CLI.Extensions;

namespace Automate.CLI.Domain
{
    internal class Element : PatternElement, IValidateable, IPersistable
    {
        public Element(string name, ElementCardinality cardinality = ElementCardinality.One, string displayName = null,
            string description = null) : base(name)
        {
            DisplayName = displayName;
            Description = description;
            IsCollection = cardinality is ElementCardinality.OneOrMany or ElementCardinality.ZeroOrMany;
            Cardinality = cardinality;
        }

        private Element(PersistableProperties properties, IPersistableFactory factory) : base(properties, factory)
        {
            DisplayName = properties.Rehydrate<string>(factory, nameof(DisplayName));
            Description = properties.Rehydrate<string>(factory, nameof(Description));
            IsCollection = properties.Rehydrate<bool>(factory, nameof(IsCollection));
            Cardinality = properties.Rehydrate<string>(factory, nameof(Cardinality))
                .ToEnumOrDefault(ElementCardinality.One);
        }

        public ElementCardinality Cardinality { get; private set; }

        public string DisplayName { get; private set; }

        public string Description { get; private set; }

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

        public override bool Accept(IPatternVisitor visitor)
        {
            if (visitor.VisitElementEnter(this))
            {
                base.Accept(visitor);
            }

            return visitor.VisitElementExit(this);
        }

        public ValidationResults Validate(ValidationContext context, object value)
        {
            return ValidationResults.None;
        }
    }
}