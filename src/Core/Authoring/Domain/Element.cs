using Automate.Common.Domain;
using Automate.Common.Extensions;

namespace Automate.Authoring.Domain
{
    public class Element : PatternElement, IValidateable, IPersistable
    {
        public static readonly string[] ReservedElementNames = Attribute.ReservedAttributeNames;

        public Element(string name,
            ElementCardinality cardinality = ElementCardinality.One, bool autoCreate = true,
            string displayName = null, string description = null) : base(name, displayName, description)
        {
            IsCollection = cardinality is ElementCardinality.OneOrMany or ElementCardinality.ZeroOrMany;
            Cardinality = cardinality;
            AutoCreate = autoCreate;
        }

        private Element(PersistableProperties properties, IPersistableFactory factory) :
            base(properties, factory)
        {
            IsCollection = properties.Rehydrate<bool>(factory, nameof(IsCollection));
            Cardinality = properties.Rehydrate<string>(factory, nameof(Cardinality))
                .ToEnumOrDefault(ElementCardinality.One);
            AutoCreate = properties.Rehydrate(factory, nameof(AutoCreate), true);
        }

        public ElementCardinality Cardinality { get; private set; }

        public bool IsCollection { get; private set; }

        public bool AutoCreate { get; private set; }

        public override PersistableProperties Dehydrate()
        {
            var properties = base.Dehydrate();
            properties.Dehydrate(nameof(IsCollection), IsCollection);
            properties.Dehydrate(nameof(Cardinality), Cardinality);
            properties.Dehydrate(nameof(AutoCreate), AutoCreate);

            return properties;
        }

        public static Element Rehydrate(PersistableProperties properties,
            IPersistableFactory factory)
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

        public void SetCardinality(ElementCardinality cardinality)
        {
            if (cardinality != Cardinality)
            {
                Cardinality = cardinality;
                RecordChange(VersionChange.Breaking, VersionChanges.PatternElement_Element_Update_Cardinality,
                    Id, Parent.Id);
            }
        }

        public void SetCreation(bool autoCreate)
        {
            if (autoCreate != AutoCreate)
            {
                AutoCreate = autoCreate;
                RecordChange(VersionChange.Breaking, VersionChanges.PatternElement_Element_Update_AutoCreate,
                    Id, Parent.Id);
            }
        }

        public ValidationResults Validate(ValidationContext context, object value)
        {
            return ValidationResults.None;
        }
    }
}