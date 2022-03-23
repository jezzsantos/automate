using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Automate.CLI.Extensions;

namespace Automate.CLI.Domain
{
    internal class SolutionItem : IIdentifiableEntity, IPersistable
    {
        private readonly List<ArtifactLink> artifactLinks;

        // ReSharper disable once InconsistentNaming
        private object _value;
        private List<SolutionItem> items;
        private Dictionary<string, SolutionItem> properties;

        public SolutionItem(ToolkitDefinition toolkit, PatternDefinition pattern) : this(toolkit, pattern.ToSchema())
        {
        }

        private SolutionItem(ToolkitDefinition toolkit, IPatternSchema pattern)
        {
            toolkit.GuardAgainstNull(nameof(toolkit));
            pattern.GuardAgainstNull(nameof(pattern));

            Id = IdGenerator.Create();
            Name = pattern.Name;
            Toolkit = toolkit;
            Schema = new SolutionItemSchema(pattern.Id, SolutionItemSchemaType.Pattern);
            IsMaterialised = true;
            Value = null;
            this.properties = new Dictionary<string, SolutionItem>();
            pattern.Attributes.ToListSafe()
                .ForEach(attr =>
                {
                    this.properties.Add(attr.Name,
                        new SolutionItem(toolkit, attr, this));
                });
            pattern.Elements.ToListSafe()
                .ForEach(ele => { this.properties.Add(ele.Name, new SolutionItem(toolkit, ele, this)); });
            this.items = null;
            Parent = null;
            this.artifactLinks = new List<ArtifactLink>();
        }

        public SolutionItem(ToolkitDefinition toolkit, Attribute attribute, SolutionItem parent) : this(toolkit, attribute.ToSchema(), parent)
        {
        }

        private SolutionItem(ToolkitDefinition toolkit, IAttributeSchema attribute, SolutionItem parent)
        {
            toolkit.GuardAgainstNull(nameof(toolkit));
            attribute.GuardAgainstNull(nameof(attribute));

            Id = IdGenerator.Create();
            Name = attribute.Name;
            Toolkit = toolkit;
            Schema = new SolutionItemSchema(attribute.Id, SolutionItemSchemaType.Attribute);
            IsMaterialised = attribute.DefaultValue.HasValue();
            SetValue(attribute.DefaultValue, attribute.DataType);
            this.properties = null;
            this.items = null;
            Parent = parent;
            this.artifactLinks = new List<ArtifactLink>();
        }

        public SolutionItem(ToolkitDefinition toolkit, Element element, SolutionItem parent, bool isCollectionItem = false) : this(toolkit, element.ToSchema(), parent, isCollectionItem)
        {
        }

        private SolutionItem(ToolkitDefinition toolkit, IElementSchema element, SolutionItem parent, bool isCollectionItem = false)
        {
            toolkit.GuardAgainstNull(nameof(toolkit));
            element.GuardAgainstNull(nameof(element));

            Id = IdGenerator.Create();
            Name = element.Name;
            Toolkit = toolkit;
            Schema = new SolutionItemSchema(element.Id, isCollectionItem
                ? SolutionItemSchemaType.CollectionItem
                : element.IsCollection
                    ? SolutionItemSchemaType.EphemeralCollection
                    : SolutionItemSchemaType.Element);
            IsMaterialised = false;
            Value = null;
            this.properties = null;
            this.items = null;
            Parent = parent;
            this.artifactLinks = new List<ArtifactLink>();
        }

        public SolutionItem(object value, string dataType, SolutionItem parent)
        {
            Id = IdGenerator.Create();
            Name = null;
            Toolkit = null;
            Schema = SolutionItemSchema.None;
            IsMaterialised = true;
            SetValue(value, dataType);
            this.properties = null;
            this.items = null;
            Parent = parent;
            this.artifactLinks = new List<ArtifactLink>();
        }

        private SolutionItem(PersistableProperties properties, IPersistableFactory factory)
        {
            Id = properties.Rehydrate<string>(factory, nameof(Id));
            Name = properties.Rehydrate<string>(factory, nameof(Name));
            Schema = properties.Rehydrate<SolutionItemSchema>(factory, nameof(Schema));
            this._value = properties.Rehydrate<object>(factory, nameof(Value));
            this.properties = properties.Rehydrate<Dictionary<string, SolutionItem>>(factory, nameof(Properties));
            this.items = properties.Rehydrate<List<SolutionItem>>(factory, nameof(Items));
            IsMaterialised = properties.Rehydrate<bool>(factory, nameof(IsMaterialised));
            this.artifactLinks = properties.Rehydrate<List<ArtifactLink>>(factory, nameof(ArtifactLinks));
        }

        public SolutionItem Parent { get; private set; }

        private ToolkitDefinition Toolkit { get; set; }

        public SolutionItemSchema Schema { get; }

        public IPatternSchema PatternSchema => Schema.ResolveSchema<IPatternSchema>(Toolkit);

        public IElementSchema ElementSchema => Schema.ResolveSchema<IElementSchema>(Toolkit);

        public IAttributeSchema AttributeSchema => Schema.ResolveSchema<IAttributeSchema>(Toolkit);

        public object Value
        {
            get => this._value;
            set => this._value = IsAttribute
                ? Attribute.SetValue(AttributeSchema.DataType, value)
                : value;
        }

        public IReadOnlyDictionary<string, SolutionItem> Properties => this.properties;

        public IReadOnlyList<SolutionItem> Items => this.items;

        public string Name { get; }

        public bool IsMaterialised { get; private set; }

        public bool IsPattern => Schema.SchemaType == SolutionItemSchemaType.Pattern;

        public bool IsElement => Schema.SchemaType is SolutionItemSchemaType.Element or SolutionItemSchemaType.CollectionItem;

        public bool IsCollection => Schema.SchemaType == SolutionItemSchemaType.EphemeralCollection;

        public bool IsAttribute => Schema.SchemaType == SolutionItemSchemaType.Attribute;

        public bool IsValue => Schema.SchemaType == SolutionItemSchemaType.None;

        public IReadOnlyList<ArtifactLink> ArtifactLinks => this.artifactLinks;

        public PersistableProperties Dehydrate()
        {
            var props = new PersistableProperties();
            props.Dehydrate(nameof(Id), Id);
            props.Dehydrate(nameof(Name), Name);
            props.Dehydrate(nameof(Schema), Schema);
            props.Dehydrate(nameof(Value), Value);
            props.Dehydrate(nameof(Properties), Properties);
            props.Dehydrate(nameof(Items), Items);
            props.Dehydrate(nameof(IsMaterialised), IsMaterialised);
            props.Dehydrate(nameof(ArtifactLinks), ArtifactLinks);

            return props;
        }

        public static SolutionItem Rehydrate(PersistableProperties properties, IPersistableFactory factory)
        {
            return new SolutionItem(properties, factory);
        }

        public SolutionItem Materialise(object value = null)
        {
            if (IsPattern)
            {
                throw new AutomateException(
                    ExceptionMessages.SolutionItem_PatternAlreadyMaterialised.Format(PatternSchema.Name));
            }

            if (IsElement)
            {
                this.properties = new Dictionary<string, SolutionItem>();
                ElementSchema.Attributes.ToListSafe().ForEach(
                    attr => { this.properties.Add(attr.Name, new SolutionItem(Toolkit, attr, this)); });
                ElementSchema.Elements.ToListSafe().ForEach(ele => { this.properties.Add(ele.Name, new SolutionItem(Toolkit, ele, this)); });
                this.items = null;
                IsMaterialised = true;
            }

            if (IsCollection)
            {
                this.properties = new Dictionary<string, SolutionItem>();
                this.items = new List<SolutionItem>();
                IsMaterialised = true;
            }

            if (IsAttribute)
            {
                SetValue(value, AttributeSchema.DataType);
                IsMaterialised = true;
            }

            if (IsValue)
            {
                throw new AutomateException(ExceptionMessages.SolutionItem_ValueAlreadyMaterialised);
            }

            return this;
        }

        /// <summary>
        ///     Creates a new instance in the <see cref="Items" /> of this "ephemeral" collection.
        ///     This collection is never actually operated upon. Only <see cref="Items" /> in its collection are operated on.
        /// </summary>
        public SolutionItem MaterialiseCollectionItem()
        {
            if (!IsCollection)
            {
                throw new AutomateException(ExceptionMessages.SolutionItem_MaterialiseNotACollection);
            }

            if (!IsMaterialised)
            {
                Materialise();
            }

            var childElementItem = CreateEphemeralCollectionItem();
            this.items.Add(childElementItem);

            return childElementItem;
        }

        public bool HasAttribute(string name)
        {
            name.GuardAgainstNullOrEmpty(nameof(name));

            if (IsPattern)
            {
                return PatternSchema.Attributes.Safe().Any(attr => attr.Name.EqualsIgnoreCase(name));
            }
            if (IsElement)
            {
                return ElementSchema.Attributes.Safe().Any(attr => attr.Name.EqualsIgnoreCase(name));
            }

            return false;
        }

        public SolutionItemProperty GetProperty(string name)
        {
            name.GuardAgainstNullOrEmpty(nameof(name));

            if (!IsMaterialised)
            {
                throw new AutomateException(ExceptionMessages.SolutionItem_NotMaterialised);
            }

            if (Properties.NotExists()
                || !Properties.ContainsKey(name))
            {
                throw new AutomateException(ExceptionMessages.SolutionItem_NotAProperty.Format(name));
            }

            return new SolutionItemProperty(Properties[name]);
        }

        public ValidationResults Validate(ValidationContext context)
        {
            context.GuardAgainstNull(nameof(context));

            return ValidateDescendants(this, context, false);

            ValidationResults ValidateDescendants(SolutionItem solutionItem, ValidationContext validationContext, bool isItem)
            {
                if (solutionItem.IsPattern)
                {
                    var subContext = new ValidationContext(validationContext);
                    subContext.Add($"{solutionItem.PatternSchema.Name}");
                    return new ValidationResults(
                        solutionItem.Properties.SelectMany(prop => ValidateDescendants(prop.Value, subContext, false).Results));
                }

                if (solutionItem.IsElement || solutionItem.IsCollection)
                {
                    var subContext = new ValidationContext(validationContext);
                    subContext.Add(isItem
                        ? $"{solutionItem.Id}"
                        : $"{solutionItem.ElementSchema.Name}");
                    var results = ValidationResults.None;

                    if (solutionItem.IsMaterialised)
                    {
                        if (solutionItem.IsElement)
                        {
                            if (solutionItem.Properties.HasAny())
                            {
                                results.AddRange(
                                    solutionItem.Properties.SelectMany(prop => ValidateDescendants(prop.Value, subContext, false).Results));
                            }
                        }

                        if (solutionItem.IsCollection)
                        {
                            if (solutionItem.ElementSchema.HasCardinalityOfAtLeastOne())
                            {
                                if (solutionItem.Items.HasNone())
                                {
                                    results.Add(subContext,
                                        ValidationMessages.SolutionItem_ValidationRule_ElementRequiresAtLeastOneInstance
                                            .Format(solutionItem.Name));
                                }
                            }
                            if (solutionItem.ElementSchema.HasCardinalityOfAtMostOne())
                            {
                                if (solutionItem.Items.HasAny() && solutionItem.Items.Count > 1)
                                {
                                    results.Add(subContext,
                                        ValidationMessages.SolutionItem_ValidationRule_ElementHasMoreThanOneInstance
                                            .Format(solutionItem.Name));
                                }
                            }
                            if (solutionItem.Items.HasAny())
                            {
                                results.AddRange(solutionItem.Items.SelectMany(item => ValidateDescendants(item, subContext, true).Results));
                            }
                        }
                    }
                    else
                    {
                        if (solutionItem.ElementSchema.HasCardinalityOfAtLeastOne())
                        {
                            results.Add(subContext,
                                ValidationMessages.SolutionItem_ValidationRule_ElementRequiresAtLeastOneInstance
                                    .Format(solutionItem.Name));
                        }
                        if (solutionItem.ElementSchema.HasCardinalityOfAtMostOne())
                        {
                            if (solutionItem.Items.HasAny() && solutionItem.Items.Count > 1)
                            {
                                results.Add(subContext,
                                    ValidationMessages.SolutionItem_ValidationRule_ElementHasMoreThanOneInstance
                                        .Format(solutionItem.Name));
                            }
                        }
                    }

                    return results;
                }

                if (solutionItem.IsAttribute)
                {
                    var subContext = new ValidationContext(validationContext);
                    subContext.Add($"{solutionItem.AttributeSchema.Name}");
                    var results = ValidationResults.None;

                    results.AddRange(solutionItem.AttributeSchema.Validate(subContext, solutionItem.Value));

                    return results;
                }

                return ValidationResults.None;
            }
        }

        /// <summary>
        ///     Returns the populated configuration of the current <see cref="SolutionItem" />.
        ///     Entries in this <see cref="Dictionary{K,V}" /> will only contain the values (at the leaves of the hierarchy)
        /// </summary>
        /// <param name="includeAncestry">
        ///     Whether to include ancestry (<see cref="Parent" /> references) in this configuration graph.
        ///     Warning: Will cause cyclic overflows if attempt is made to render a physical representation of this graph (i.e
        ///     serialization).
        /// </param>
        public LazySolutionItemDictionary GetConfiguration(bool includeAncestry)
        {
            if (!IsPattern && !IsElement && !IsCollection)
            {
                throw new AutomateException(ExceptionMessages.SolutionItem_ConfigurationForNonElement);
            }

            return new LazySolutionItemDictionary(this, includeAncestry);
        }

        public CommandExecutionResult ExecuteCommand(SolutionDefinition solution, string name)
        {
            solution.GuardAgainstNull(nameof(solution));
            name.GuardAgainstNullOrEmpty(nameof(name));

            var command = GetAutomationByName(name);
            if (command.NotExists())
            {
                throw new AutomateException(
                    ExceptionMessages.SolutionItem_UnknownAutomation.Format(name));
            }

            return command.Execute(solution, this);
        }

        public ArtifactLink AddArtifactLink(string commandId, string path, string tag)
        {
            var link = new ArtifactLink(commandId, path, tag);
            this.artifactLinks.Add(link);

            return link;
        }

        public void SetAncestry(ToolkitDefinition toolkit, SolutionItem parent)
        {
            Toolkit = toolkit;
            Parent = Schema.SchemaType == SolutionItemSchemaType.CollectionItem
                ? parent?.Parent
                : parent;
        }

        public void SetProperties(Dictionary<string, string> propertyAssignments)
        {
            propertyAssignments.GuardAgainstNull(nameof(propertyAssignments));

            foreach (var (name, value) in propertyAssignments)
            {
                var assignment = $"{name}={value}";
                assignment.GuardAgainstInvalid(_ => Validations.IsPropertyAssignment(name, value), nameof(assignment),
                    ExceptionMessages.SolutionItem_ConfigureSolution_PropertyAssignmentInvalid, name, Id);

                if (!HasAttribute(name))
                {
                    throw new AutomateException(
                        ExceptionMessages.SolutionItem_ConfigureSolution_ElementPropertyNotExists.Format(Name, name));
                }

                var property = GetProperty(name);
                if (property.IsChoice)
                {
                    if (!property.HasChoice(value))
                    {
                        throw new AutomateException(
                            ExceptionMessages.SolutionItem_ConfigureSolution_ElementPropertyValueIsNotOneOf
                                .Format(Name, name, property.ChoiceValues.Join(";"), value));
                    }
                }
                else
                {
                    if (!property.DataTypeMatches(value))
                    {
                        throw new AutomateException(
                            ExceptionMessages.SolutionItem_ConfigureSolution_ElementPropertyValueNotCompatible
                                .Format(Name, name, property.DataType, value));
                    }
                }

                property.SetProperty(value);
            }
        }

        public string Id { get; }

        private SolutionItem CreateEphemeralCollectionItem()
        {
            var childElementItem = new SolutionItem(Toolkit, ElementSchema, Parent, true);
            childElementItem.Materialise();

            return childElementItem;
        }

        private IAutomationSchema GetAutomationByName(string name)
        {
            var automations = new List<IAutomationSchema>();
            if (IsPattern)
            {
                automations = PatternSchema.Automation.ToListSafe();
            }

            if (IsElement)
            {
                automations = ElementSchema.Automation.ToListSafe();
            }

            if (IsCollection || IsAttribute || IsValue)
            {
                throw new AutomateException(ExceptionMessages.SolutionItem_HasNoAutomations);
            }

            return automations.FirstOrDefault(auto => auto.Name.EqualsIgnoreCase(name));
        }

        private void SetValue(object value, string dataType)
        {
            Value = Attribute.SetValue(dataType, value);
        }
    }

    internal class SolutionItemProperty
    {
        private readonly SolutionItem item;

        public SolutionItemProperty(SolutionItem item)
        {
            if (!item.IsAttribute)
            {
                throw new AutomateException(ExceptionMessages.SolutionItem_NotAnAttribute.Format(item.Name));
            }

            this.item = item;
        }

        public bool IsChoice => this.item.AttributeSchema.Choices.HasAny();

        public List<string> ChoiceValues => this.item.AttributeSchema.Choices.ToListSafe();

        public string DataType => this.item.AttributeSchema.DataType;

        public string Name => this.item.AttributeSchema.Name;

        public bool HasDefaultValue => this.item.AttributeSchema.DefaultValue.HasValue();

        public bool HasChoice(string value)
        {
            return this.item.AttributeSchema.Choices.Safe().Any(choice => choice.EqualsIgnoreCase(value));
        }

        public bool DataTypeMatches(string value)
        {
            return this.item.AttributeSchema.IsValidDataType(value);
        }

        public void SetProperty(object value)
        {
            this.item.Value = Attribute.SetValue(DataType, value);
        }
    }

    internal class SolutionItemSchema : IPersistable
    {
        public static readonly SolutionItemSchema None = new SolutionItemSchema();

        public SolutionItemSchema(string id, SolutionItemSchemaType schemaType)
        {
            id.GuardAgainstNullOrEmpty(nameof(id));
            SchemaId = id;
            SchemaType = schemaType;
        }

        private SolutionItemSchema(PersistableProperties properties, IPersistableFactory factory)
        {
            SchemaId = properties.Rehydrate<string>(factory, nameof(SchemaId));
            SchemaType = properties.Rehydrate<SolutionItemSchemaType>(factory, nameof(SchemaType));
        }

        private SolutionItemSchema()
        {
            SchemaId = null;
            SchemaType = SolutionItemSchemaType.None;
        }

        public string SchemaId { get; }

        public SolutionItemSchemaType SchemaType { get; }

        public PersistableProperties Dehydrate()
        {
            var properties = new PersistableProperties();
            properties.Dehydrate(nameof(SchemaId), SchemaId);
            properties.Dehydrate(nameof(System.Data.SchemaType), SchemaType);

            return properties;
        }

        public static SolutionItemSchema Rehydrate(PersistableProperties properties, IPersistableFactory factory)
        {
            return new SolutionItemSchema(properties, factory);
        }

        public TSchema ResolveSchema<TSchema>(ToolkitDefinition toolkit) where TSchema : ISchema
        {
            toolkit.GuardAgainstNull(nameof(toolkit));

            if (SchemaType == SolutionItemSchemaType.None)
            {
                throw new AutomateException(ExceptionMessages.SolutionItem_InvalidSolutionItemSchema);
            }

            object target;
            if (typeof(TSchema) == typeof(IPatternSchema))
            {
                target = toolkit.Pattern.FindSchema<PatternDefinition>(SchemaId);
                if (target.Exists())
                {
                    return (TSchema)((PatternDefinition)target).ToSchema();
                }
            }
            if (typeof(TSchema) == typeof(IElementSchema))
            {
                target = toolkit.Pattern.FindSchema<Element>(SchemaId);
                if (target.Exists())
                {
                    return SchemaType == SolutionItemSchemaType.CollectionItem
                        ? (TSchema)(ISchema)new CollectionItemSchema((Element)target)
                        : (TSchema)((Element)target).ToSchema();
                }
            }
            if (typeof(TSchema) == typeof(IAttributeSchema))
            {
                target = toolkit.Pattern.FindSchema<Attribute>(SchemaId);
                if (target.Exists())
                {
                    return (TSchema)((Attribute)target).ToSchema();
                }
            }

            throw new AutomateException(ExceptionMessages.SolutionItem_UnknownSchema.Format(SchemaId, SchemaType));
        }
    }

    internal enum SolutionItemSchemaType
    {
        None = 0,
        Pattern = 1,
        Element = 2,
        EphemeralCollection = 3,
        CollectionItem = 4,
        Attribute = 5
    }

    /// <summary>
    ///     This dictionary implementation is required to use an <see cref="IEnumerator" /> that evaluates lazy-ly (deferred
    ///     execution),
    ///     so that we can populate cyclic pointers in memory for ancestor properties (i.e. <see cref="SolutionItem.Parent" />
    ///     which would otherwise cause infinite recursion, if each <see cref="DictionaryEntry" /> was evaluated when this
    ///     dictionary was being constructed.
    ///     This dictionary type must implement <see cref="IDictionary" /> so that it can be used in text transformation using
    ///     Scriban.
    /// </summary>
    internal class LazySolutionItemDictionary : IDictionary
    {
        private readonly bool includeAncestry;
        private readonly SolutionItem solutionItem;

        /// <summary>
        ///     For serialization
        /// </summary>
        public LazySolutionItemDictionary()
        {
        }

        public LazySolutionItemDictionary(SolutionItem solutionItem, bool includeAncestry)
        {
            solutionItem.GuardAgainstNull(nameof(solutionItem));
            this.solutionItem = solutionItem;
            this.includeAncestry = includeAncestry;
        }

        public object this[object key]
        {
            get
            {
                var value = GetPairs().FirstOrDefault(pair => pair.Key.Equals(key));

                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                if (value.Key.Exists())
                {
                    return value.Value;
                }

                throw new KeyNotFoundException($"The key: {key} could not be found on the current solution item");
            }
            set => throw new NotImplementedException();
        }

        public ICollection Keys => GetPairs().Select(pair => pair.Key).ToArray();

        public ICollection Values => GetPairs().Select(pair => pair.Value).ToArray();

        public bool Contains(object key)
        {
            return GetPairs().Select(pair => pair.Key).Contains(key);
        }

        public IDictionaryEnumerator GetEnumerator()
        {
            return new DictionaryEntryEnumerator(GetPairs().ToList());
        }

        public void Remove(object key)
        {
            throw new NotImplementedException();
        }

        public bool IsFixedSize => false;

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetPairs().GetEnumerator();
        }

        public void Add(object key, object value)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public void CopyTo(Array array, int index)
        {
            foreach (var (_, value) in GetPairs())
            {
                array.SetValue(value, index);
            }
        }

        public int Count => Keys.Count;

        public bool IsSynchronized => Keys.IsSynchronized;

        public object SyncRoot => Keys.SyncRoot;

        public bool IsReadOnly => true;

        private IEnumerable<DictionaryEntry> GetPairs()
        {
            if (this.solutionItem.IsPattern || this.solutionItem.IsElement || this.solutionItem.IsCollection)
            {
                yield return new DictionaryEntry(FormatName(nameof(SolutionItem.Id)), this.solutionItem.Id);
            }
            if (this.includeAncestry)
            {
                if (this.solutionItem.IsPattern || this.solutionItem.IsElement || this.solutionItem.IsCollection || this.solutionItem.IsAttribute)
                {
                    if (this.solutionItem.Parent.Exists())
                    {
                        yield return new DictionaryEntry(FormatName(nameof(SolutionItem.Parent)), new LazySolutionItemDictionary(this.solutionItem.Parent, true));
                    }
                }
            }
            if (this.solutionItem.IsPattern || this.solutionItem.IsElement || this.solutionItem.IsCollection)
            {
                if (this.solutionItem.Properties.HasAny())
                {
                    foreach (var prop in this.solutionItem.Properties.Safe())
                    {
                        var (name, item) = prop;
                        if (item.IsAttribute)
                        {
                            if (item.Value.Exists())
                            {
                                yield return new DictionaryEntry(FormatName(name), item.Value);
                            }
                        }
                        if (item.IsElement || item.IsCollection)
                        {
                            yield return new DictionaryEntry(FormatName(name), new LazySolutionItemDictionary(item, this.includeAncestry));
                        }
                    }
                }
            }
            if (this.solutionItem.IsCollection)
            {
                if (this.solutionItem.Items.HasAny())
                {
                    var items = this.solutionItem.Items.Select(item => new LazySolutionItemDictionary(item, this.includeAncestry));
                    yield return new DictionaryEntry(FormatName(nameof(SolutionItem.Items)), items);
                }
            }
            if (this.solutionItem.IsAttribute)
            {
                if (this.solutionItem.Value.Exists())
                {
                    yield return new DictionaryEntry(FormatName(this.solutionItem.Name), this.solutionItem.Value);
                }
            }
        }

        private static string FormatName(string name)
        {
            return name.ToSnakeCase();
        }

        private class DictionaryEntryEnumerator : IDictionaryEnumerator
        {
            private readonly IList<DictionaryEntry> entries;
            private int index = -1;

            public DictionaryEntryEnumerator(IList<DictionaryEntry> entries)
            {
                this.entries = entries;
            }

            public object Current
            {
                get
                {
                    ValidateIndex();
                    return this.entries[this.index];
                }
            }

            public DictionaryEntry Entry => (DictionaryEntry)Current!;

            public object Key
            {
                get
                {
                    ValidateIndex();
                    return this.entries[this.index].Key;
                }
            }

            public object Value
            {
                get
                {
                    ValidateIndex();
                    return this.entries[this.index].Value;
                }
            }

            public bool MoveNext()
            {
                if (this.index < this.entries.Count - 1)
                {
                    this.index++;
                    return true;
                }
                return false;
            }

            public void Reset()
            {
                this.index = -1;
            }

            private void ValidateIndex()
            {
                if (this.index < 0 || this.index >= this.entries.Count)
                {
                    throw new InvalidOperationException("Enumerator is before or after the collection.");
                }
            }
        }
    }
}