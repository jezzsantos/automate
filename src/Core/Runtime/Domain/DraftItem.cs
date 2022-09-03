using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Automate.Authoring.Domain;
using Automate.Common;
using Automate.Common.Domain;
using Automate.Common.Extensions;
using JetBrains.Annotations;
using Attribute = Automate.Authoring.Domain.Attribute;

namespace Automate.Runtime.Domain
{
    public class DraftItem : IIdentifiableEntity, IPersistable, IDraftItemVisitable
    {
        public const string SchemaPropertyName = "Schema";
        private readonly List<ArtifactLink> artifactLinks;

        // ReSharper disable once InconsistentNaming
        private object _value;
        private List<DraftItem> items;
        private Dictionary<string, DraftItem> properties;

        internal DraftItem(ToolkitDefinition toolkit, PatternDefinition pattern) : this(toolkit, pattern.ToSchema())
        {
        }

        private DraftItem(ToolkitDefinition toolkit, IPatternSchema pattern)
            : this(pattern.Name, toolkit, null,
                new DraftItemSchema(pattern.Id, DraftItemSchemaType.Pattern))
        {
            toolkit.GuardAgainstNull(nameof(toolkit));
            pattern.GuardAgainstNull(nameof(pattern));

            Init(toolkit);
        }

        public DraftItem(ToolkitDefinition toolkit, Element element) : this(toolkit, element, null)
        {
        }

        internal DraftItem(ToolkitDefinition toolkit, Element element, DraftItem parent,
            bool isCollectionItem = false) : this(toolkit, element.ToSchema(), parent,
            isCollectionItem)
        {
        }

        private DraftItem(ToolkitDefinition toolkit, IElementSchema element,
            DraftItem parent,
            bool isCollectionItem)
            : this(element.Name, toolkit, parent, new DraftItemSchema(element.Id, isCollectionItem
                ? DraftItemSchemaType.CollectionItem
                : element.IsCollection
                    ? DraftItemSchemaType.EphemeralCollection
                    : DraftItemSchemaType.Element))
        {
            toolkit.GuardAgainstNull(nameof(toolkit));
            element.GuardAgainstNull(nameof(element));

            Init(toolkit);
        }

        internal DraftItem(ToolkitDefinition toolkit, Attribute attribute,
            DraftItem parent) : this(toolkit,
            attribute.ToSchema(), parent)
        {
        }

        private DraftItem(ToolkitDefinition toolkit, IAttributeSchema attribute,
            DraftItem parent)
            : this(attribute.Name, toolkit, parent,
                new DraftItemSchema(attribute.Id, DraftItemSchemaType.Attribute))
        {
            toolkit.GuardAgainstNull(nameof(toolkit));
            attribute.GuardAgainstNull(nameof(attribute));

            Init(toolkit);
        }

        private DraftItem(string name, ToolkitDefinition toolkit, DraftItem parent,
            DraftItemSchema schema)
        {
            Id = IdGenerator.Create();
            Name = name;
            Toolkit = toolkit;
            Schema = schema;
            ImmediateParent = parent;
            this.artifactLinks = new List<ArtifactLink>();
        }

        private DraftItem(PersistableProperties properties, IPersistableFactory factory)
        {
            Id = properties.Rehydrate<string>(factory, nameof(Id));
            Name = properties.Rehydrate<string>(factory, nameof(Name));
            Schema = properties.Rehydrate<DraftItemSchema>(factory, nameof(Schema));
            this._value = properties.Rehydrate<object>(factory, nameof(Value));
            this.properties =
                properties.Rehydrate<Dictionary<string, DraftItem>>(factory, nameof(Properties));
            this.items = properties.Rehydrate<List<DraftItem>>(factory, nameof(Items));
            IsMaterialised = properties.Rehydrate<bool>(factory, nameof(IsMaterialised));
            this.artifactLinks = properties.Rehydrate<List<ArtifactLink>>(factory, nameof(ArtifactLinks));
        }

        public DraftItem Parent =>
            IsCollectionItemElement
                ? ImmediateParent?.Parent
                : ImmediateParent;

        internal DraftItem ImmediateParent { get; private set; }

        private ToolkitDefinition Toolkit { get; set; }

        public DraftItemSchema Schema { get; }

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

        public IReadOnlyDictionary<string, DraftItem> Properties => this.properties;

        public IReadOnlyList<DraftItem> Items => this.items;

        public string Name { get; }

        public bool IsMaterialised { get; private set; }

        public bool IsPattern => Schema.SchemaType == DraftItemSchemaType.Pattern;

        public bool IsElement =>
            Schema.SchemaType is DraftItemSchemaType.Element || IsCollectionItemElement;

        public bool IsEphemeralCollection => Schema.SchemaType == DraftItemSchemaType.EphemeralCollection;

        private bool IsCollectionItemElement => Schema.SchemaType == DraftItemSchemaType.CollectionItem;

        public bool IsAttribute => Schema.SchemaType == DraftItemSchemaType.Attribute;

        public IReadOnlyList<ArtifactLink> ArtifactLinks => this.artifactLinks;

        private string FullyQualifiedPath => GetPath(false);

        public string ConfigurePath => GetPath(true);

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

        public static DraftItem Rehydrate(PersistableProperties properties,
            IPersistableFactory factory)
        {
            return new DraftItem(properties, factory);
        }

        public DraftItem Materialise(object value = null)
        {
            if (IsPattern)
            {
                throw new AutomateException(
                    ExceptionMessages.DraftItem_PatternAlreadyMaterialised.Substitute(PatternSchema.Name));
            }

            SetMaterialised(true, value);

            return this;
        }

        /// <summary>
        ///     Creates a new instance in the <see cref="Items" /> of this "ephemeral" collection.
        ///     This ephemeral collection is never actually operated upon. Only <see cref="Items" /> in its collection are operated
        ///     on.
        /// </summary>
        public DraftItem MaterialiseCollectionItem()
        {
            if (!IsEphemeralCollection)
            {
                throw new AutomateException(ExceptionMessages.DraftItem_MaterialiseNotACollection);
            }

            if (!IsMaterialised)
            {
                Materialise();
            }

            var childElementItem = CreateEphemeralCollectionItem();
            this.items.Add(childElementItem);

            return childElementItem;
        }

        public void UnMaterialise()
        {
            if (!IsElement && !IsEphemeralCollection)
            {
                throw new AutomateException(ExceptionMessages.DraftItem_DeleteForNonElementChild);
            }

            var parent = ImmediateParent;
            if (IsCollectionItemElement)
            {
                if (parent.items.All(item => item != this))
                {
                    throw new AutomateException(ExceptionMessages.DraftItem_DeleteWithUnknownChild);
                }

                parent.items.Remove(this);
            }
            else
            {
                if (IsPattern || IsElement)
                {
                    SetMaterialised(false, force: true);
                }

                if (IsEphemeralCollection)
                {
                    this.items.Clear();
                    SetMaterialised(false, force: true);
                }
            }
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

        public DraftItemProperty GetProperty(string name)
        {
            name.GuardAgainstNullOrEmpty(nameof(name));

            if (!IsMaterialised)
            {
                throw new AutomateException(ExceptionMessages.DraftItem_NotMaterialised);
            }

            if (Properties.NotExists()
                || !Properties.ContainsKey(name))
            {
                throw new AutomateException(ExceptionMessages.DraftItem_NotAProperty.Substitute(name));
            }

            return new DraftItemProperty(Properties[name]);
        }

        public ValidationResults Validate()
        {
            var validator = new SchemaValidator();
            TraverseDescendants(validator);
            return validator.Validations;
        }

        /// <summary>
        ///     Returns the populated configuration of the current <see cref="DraftItem" />.
        ///     Entries in this <see cref="Dictionary{K,V}" /> will only contain the values (at the leaves of the hierarchy)
        /// </summary>
        /// <param name="includeAncestry">
        ///     Whether to include ancestry (<see cref="Parent" /> references) in this configuration graph.
        ///     Warning: Will cause cyclic overflows if attempt is made to render a physical representation of this graph (i.e
        ///     serialization).
        /// </param>
        /// <param name="includeSchema">
        ///     Whether to include the schema information of the <see cref="DraftItem" /> in this configuration
        ///     graph.
        /// </param>
        public LazyDraftItemDictionary GetConfiguration(bool includeAncestry, bool includeSchema)
        {
            if (!IsPattern && !IsElement && !IsEphemeralCollection)
            {
                throw new AutomateException(ExceptionMessages.DraftItem_ConfigurationForNonElement);
            }

            return new LazyDraftItemDictionary(this, includeAncestry, includeSchema);
        }

        public CommandExecutionResult ExecuteCommand(DraftDefinition draft, string name)
        {
            draft.GuardAgainstNull(nameof(draft));
            name.GuardAgainstNullOrEmpty(nameof(name));

            var command = GetAutomationByName(name);
            if (command.NotExists())
            {
                throw new AutomateException(
                    ExceptionMessages.DraftItem_UnknownAutomation.Substitute(name));
            }
            if (!command.IsLaunching())
            {
                throw new AutomateException(ExceptionMessages.DraftItem_NotLaunching.Substitute(name));
            }

            var executable = command.GetExecutable(draft, this);
            return new CommandExecutionResult(executable.Name, new CommandExecutableContext(executable, draft, this));
        }

        public void AddArtifactLink(string commandId, string path, string tag)
        {
            var link = new ArtifactLink(commandId, path, tag);
            this.artifactLinks.Add(link);
        }

        public void SetAncestry(ToolkitDefinition toolkit, DraftItem parent)
        {
            Toolkit = toolkit;
            ImmediateParent = parent;
        }

        public void SetProperties(Dictionary<string, string> propertyAssignments)
        {
            propertyAssignments.GuardAgainstNull(nameof(propertyAssignments));

            foreach (var (name, value) in propertyAssignments)
            {
                var assignment = $"{name}={value}";
                assignment.GuardAgainstInvalid(_ => Validations.IsPropertyAssignment(name, value), nameof(assignment),
                    ExceptionMessages.DraftItem_Configure_PropertyAssignmentInvalid, name, Id);

                if (!HasAttribute(name))
                {
                    throw new AutomateException(
                        ExceptionMessages.DraftItem_Configure_ElementPropertyNotExists.Substitute(Name, name));
                }

                var property = GetProperty(name);
                if (property.IsChoice)
                {
                    if (!property.HasChoice(value))
                    {
                        throw new AutomateException(
                            ExceptionMessages.DraftItem_Configure_ElementPropertyValueIsNotOneOf
                                .Substitute(Name, name, property.ChoiceValues.Join(";"), value));
                    }
                }
                else
                {
                    if (!property.DataTypeMatches(value))
                    {
                        throw new AutomateException(
                            ExceptionMessages.DraftItem_Configure_ElementPropertyValueNotCompatible
                                .Substitute(Name, name, property.DataType, value));
                    }
                }

                property.SetValue(value);
            }
        }

        public void Migrate(ToolkitDefinition latestToolkit, DraftUpgradeResult result)
        {
            var migrator = new SchemaMigrator(latestToolkit, result);
            TraverseDescendants(migrator);
        }

        public void ResetAllProperties()
        {
            if (!IsPattern && !IsElement)
            {
                throw new AutomateException(ExceptionMessages.DraftItem_ResetPropertiesForNonElement);
            }

            List<IAttributeSchema> attributes = null;
            if (IsPattern)
            {
                attributes = PatternSchema.Attributes.ToListSafe();
            }
            if (IsElement)
            {
                attributes = ElementSchema.Attributes.ToListSafe();
            }

            attributes?.ForEach(attr =>
            {
                var property = GetProperty(attr.Name);
                property.ResetValue();
            });
        }

        public void ClearCollectionItems()
        {
            if (!IsEphemeralCollection)
            {
                throw new AutomateException(ExceptionMessages.DraftItem_ClearItemsForNonCollection);
            }

            this.items.Clear();
        }

        public bool Accept(IDraftItemVisitor visitor)
        {
            if (IsPattern)
            {
                if (visitor.VisitPatternEnter(this))
                {
                    foreach (var (_, value) in Properties.ToListSafe())
                    {
                        if (!value.Accept(visitor))
                        {
                            break;
                        }
                    }
                }

                return visitor.VisitPatternExit(this);
            }

            if (IsElement)
            {
                if (visitor.VisitElementEnter(this))
                {
                    foreach (var (_, value) in Properties.ToListSafe())
                    {
                        if (!value.Accept(visitor))
                        {
                            break;
                        }
                    }
                }

                return visitor.VisitElementExit(this);
            }

            if (IsEphemeralCollection)
            {
                if (visitor.VisitEphemeralCollectionEnter(this))
                {
                    var abort = false;
                    foreach (var (_, value) in Properties.ToListSafe())
                    {
                        if (!value.Accept(visitor))
                        {
                            abort = true;
                            break;
                        }
                    }

                    if (!abort)
                    {
                        foreach (var item in Items.ToListSafe())
                        {
                            if (!item.Accept(visitor))
                            {
                                break;
                            }
                        }
                    }
                }

                return visitor.VisitEphemeralCollectionExit(this);
            }

            if (IsAttribute)
            {
                if (visitor.VisitAttributeEnter(this))
                {
                    visitor.Visit(Value);
                }

                return visitor.VisitAttributeExit(this);
            }

            return true;
        }

        public string Id { get; }

        private void Init(ToolkitDefinition toolkit)
        {
            InitDescendantStructure();
            InitMaterialisationState();

            void InitDescendantStructure()
            {
                if (IsPattern || IsElement || IsEphemeralCollection)
                {
                    if (IsEphemeralCollection)
                    {
                        this.properties = new Dictionary<string, DraftItem>();
                        this.items = new List<DraftItem>();
                    }
                    else
                    {
                        this.properties = new Dictionary<string, DraftItem>();
                        IPatternElementSchema patternElementSchema = IsPattern
                            ? PatternSchema
                            : ElementSchema;
                        patternElementSchema.Attributes.ToListSafe()
                            .ForEach(attr => { this.properties.Add(attr.Name, new DraftItem(toolkit, attr, this)); });
                        patternElementSchema.Elements.ToListSafe()
                            .ForEach(ele =>
                            {
                                this.properties.Add(ele.Name, new DraftItem(toolkit, ele, this, false));
                            });
                        this.items = null;
                    }
                    Value = null;
                }
                else if (IsAttribute)
                {
                    this.properties = null;
                    this.items = null;
                    Value = null;
                }
                else
                {
                    throw new AutomateException(ExceptionMessages.DraftItem_UnknownSchema);
                }
            }

            void InitMaterialisationState()
            {
                if (IsPattern || IsElement || IsEphemeralCollection)
                {
                    IPatternElementSchema patternElementSchema = IsPattern
                        ? PatternSchema
                        : ElementSchema;
                    var shouldMaterialise = patternElementSchema.ShouldAutoCreate();
                    SetMaterialised(shouldMaterialise);
                }
                else if (IsAttribute)
                {
                    var shouldMaterialise = Parent.IsPattern
                        ? Parent.PatternSchema.ShouldAutoCreate()
                        : Parent.ElementSchema.ShouldAutoCreate();
                    SetMaterialised(shouldMaterialise);
                }
                else
                {
                    throw new AutomateException(ExceptionMessages.DraftItem_UnknownSchema);
                }
            }
        }

        /// <summary>
        ///     This [hierarchical] traversal requires that we enter and exit composite nodes (i.e. the <see cref="DraftItem" />
        ///     that themselves are composed of other nodes or composite nodes.
        ///     Once 'entered' a specific composite node, we traverse each of its child nodes (and their child nodes), before
        ///     'exiting' this specific composite node.
        ///     In this way we can tell the difference between a sibling node and a child node (of the same type).
        ///     Furthermore, each visit returns to us whether we are to abort visiting that branch of the graph, and we return up
        ///     to the root node, or continue down that branch, and on to sibling branches.
        ///     In other words, an aborted 'enter', must result in an 'exit', but then followed by returning up that branch of
        ///     nodes to the root of the graph, before exiting the traversal.
        /// </summary>
        private void TraverseDescendants(IDraftItemVisitor visitor)
        {
            Accept(visitor);
        }

        private DraftItem CreateProperty(ToolkitDefinition toolkit, IAttributeSchema schema)
        {
            var property = new DraftItem(toolkit, schema, this);
            this.properties.Add(schema.Name, property);

            return property;
        }

        private void DestroyProperty(string propertyName)
        {
            if (this.properties.HasAny())
            {
                if (this.properties.Any(prop => prop.Key.EqualsIgnoreCase(propertyName)))
                {
                    this.properties.Remove(propertyName);
                }
            }
        }

        /// <summary>
        ///     Cannot use the <see cref="IDraftItemVisitor" /> to traverse up the hierarchy,
        ///     so we have to traverse up using a custom iterator
        /// </summary>
        private string GetPath(bool asConfigurePath)
        {
            var path = GetAncestorPath(this);
            return asConfigurePath
                ? $"{{{path}}}"
                : path;

            string GetAncestorPath(DraftItem draftItem)
            {
                if (draftItem.IsPattern)
                {
                    return draftItem.Name;
                }

                if (draftItem.IsElement || draftItem.IsEphemeralCollection)
                {
                    if (draftItem.Parent.Exists())
                    {
                        return draftItem.IsCollectionItemElement
                            ? $"{GetAncestorPath(draftItem.Parent)}.{draftItem.ElementSchema.Name}.{draftItem.Id}"
                            : $"{GetAncestorPath(draftItem.Parent)}.{draftItem.Name}";
                    }
                    throw new AutomateException(ExceptionMessages.DraftItem_UnknownPath);
                }
                if (draftItem.IsAttribute)
                {
                    if (draftItem.Parent.Exists())
                    {
                        return $"{GetAncestorPath(draftItem.Parent)}.{draftItem.Name}";
                    }
                    throw new AutomateException(ExceptionMessages.DraftItem_UnknownPath);
                }
                throw new AutomateException(ExceptionMessages.DraftItem_UnknownPath);
            }
        }

        private void SetMaterialised(bool isMaterialised, object value = null, bool force = false)
        {
            if (IsPattern || IsElement || IsEphemeralCollection)
            {
                if (IsEphemeralCollection)
                {
                    this.items = new List<DraftItem>();
                }
                else
                {
                    this.properties
                        .Select(prop => prop.Value)
                        .ToList().ForEach(prop =>
                        {
                            if (force)
                            {
                                prop.SetMaterialised(isMaterialised, force: true);
                            }
                            else
                            {
                                if (prop.IsElement || prop.IsEphemeralCollection)
                                {
                                    prop.SetMaterialised(prop.ElementSchema.ShouldAutoCreate());
                                }
                                if (prop.IsAttribute)
                                {
                                    prop.SetMaterialised(isMaterialised);
                                }
                            }
                        });
                    this.items = null;
                }
                Value = null;
                IsMaterialised = isMaterialised;
            }
            else if (IsAttribute)
            {
                this.properties = null;
                this.items = null;
                if (isMaterialised)
                {
                    var schema = AttributeSchema;
                    SetValue(value.Exists()
                            ? schema.IsValidDataType(value.ToString())
                                ? value
                                : null
                            : schema.IsValidDataType(schema.DefaultValue)
                                ? schema.DefaultValue
                                : null,
                        schema.DataType);
                    IsMaterialised = true;
                }
                else
                {
                    SetValue(null, AttributeSchema.DataType);
                    IsMaterialised = false;
                }
            }
            else
            {
                throw new AutomateException(ExceptionMessages.DraftItem_UnknownSchema);
            }
        }

        private DraftItem CreateEphemeralCollectionItem()
        {
            return new DraftItem(Toolkit, ElementSchema, this, true);
        }

        private IAutomationSchema GetAutomationByName(string name)
        {
            List<IAutomationSchema> automations;
            if (IsPattern)
            {
                automations = PatternSchema.Automation.ToListSafe();
            }
            else if (IsElement)
            {
                automations = ElementSchema.Automation.ToListSafe();
            }
            else
            {
                throw new AutomateException(ExceptionMessages.DraftItem_HasNoAutomations);
            }

            return automations.FirstOrDefault(auto => auto.Name.EqualsIgnoreCase(name));
        }

        private void SetValue(object value, string dataType)
        {
            this._value = Attribute.SetValue(dataType, value);
        }

        private class SchemaMigrator : IDraftItemVisitor
        {
            private readonly Stack<List<string>> deletedAttributes;
            private readonly Stack<List<string>> deletedElements;
            private readonly ToolkitDefinition latestToolkit;
            private readonly DraftUpgradeResult result;

            public SchemaMigrator(ToolkitDefinition latestToolkit, DraftUpgradeResult result)
            {
                latestToolkit.GuardAgainstNull(nameof(latestToolkit));
                result.GuardAgainstNull(nameof(result));
                this.latestToolkit = latestToolkit;
                this.result = result;
                this.deletedAttributes = new Stack<List<string>>();
                this.deletedElements = new Stack<List<string>>();
            }

            public bool VisitPatternEnter(DraftItem item)
            {
                this.deletedAttributes.Push(new List<string>());
                this.deletedElements.Push(new List<string>());
                return true;
            }

            public bool VisitPatternExit(DraftItem item)
            {
                DeleteAttributes(item, this.deletedAttributes.Pop());
                DeleteElements(item, this.deletedElements.Pop());
                AddNewAttributes(item, item.PatternSchema);

                return true;
            }

            public bool VisitElementEnter(DraftItem item)
            {
                var elementToDelete = string.Empty;
                if (item.IsMaterialised)
                {
                    var latestSchema = item.Schema.ResolveSchema<IElementSchema>(this.latestToolkit, false);
                    if (latestSchema.NotExists())
                    {
                        elementToDelete = item.ElementSchema.Name;
                        this.result.Add(MigrationChangeType.Breaking, MigrationMessages.DraftItem_ElementDeleted,
                            item.FullyQualifiedPath);
                    }
                }

                if (elementToDelete.HasValue())
                {
                    this.deletedElements.Peek().Add(item.ElementSchema.Name);
                }

                this.deletedAttributes.Push(new List<string>());
                this.deletedElements.Push(new List<string>());

                return true;
            }

            public bool VisitElementExit(DraftItem item)
            {
                DeleteAttributes(item, this.deletedAttributes.Pop());
                DeleteElements(item, this.deletedElements.Pop());
                AddNewAttributes(item, item.ElementSchema);

                return true;
            }

            public bool VisitEphemeralCollectionEnter(DraftItem item)
            {
                var elementToDelete = string.Empty;
                if (item.IsMaterialised)
                {
                    var latestSchema = item.Schema.ResolveSchema<IElementSchema>(this.latestToolkit, false);
                    if (latestSchema.NotExists())
                    {
                        elementToDelete = item.ElementSchema.Name;
                        this.result.Add(MigrationChangeType.Breaking, MigrationMessages.DraftItem_ElementDeleted,
                            item.FullyQualifiedPath);
                    }
                }

                if (elementToDelete.HasValue())
                {
                    this.deletedElements.Peek().Add(item.ElementSchema.Name);
                }
                this.deletedAttributes.Push(new List<string>());
                this.deletedElements.Push(new List<string>());

                return true;
            }

            public bool VisitEphemeralCollectionExit(DraftItem item)
            {
                DeleteAttributes(item, this.deletedAttributes.Pop());
                DeleteElements(item, this.deletedElements.Pop());
                AddNewAttributes(item, item.ElementSchema);

                return true;
            }

            public bool VisitAttributeEnter(DraftItem item)
            {
                var attributeToDelete = string.Empty;

                var latestSchema = item.Schema.ResolveSchema<IAttributeSchema>(this.latestToolkit, false);
                if (latestSchema.NotExists())
                {
                    attributeToDelete = item.AttributeSchema.Name;
                    this.result.Add(MigrationChangeType.Breaking, MigrationMessages.DraftItem_AttributeDeleted,
                        item.FullyQualifiedPath);
                }
                else
                {
                    var newName = latestSchema.Name;
                    var oldName = item.AttributeSchema.Name;
                    var newDefaultValue = latestSchema.DefaultValue;
                    var oldDefaultValue = item.AttributeSchema.DefaultValue;
                    var oldDataType = item.AttributeSchema.DataType;
                    var newDataType = latestSchema.DataType;
                    var oldChoices = item.AttributeSchema.Choices;
                    var newChoices = latestSchema.Choices;
                    var value = item.Value;

                    if (oldName.NotEqualsOrdinal(newName))
                    {
                        attributeToDelete = item.AttributeSchema.Name;
                        var property = item.Parent.CreateProperty(this.latestToolkit, latestSchema);
                        property.SetValue(value, newDataType);
                        this.result.Add(MigrationChangeType.Breaking,
                            MigrationMessages.DraftItem_AttributeNameChanged, item.FullyQualifiedPath, oldName,
                            newName);
                    }

                    if (oldDataType.NotEqualsOrdinal(newDataType))
                    {
                        if (value.Exists())
                        {
                            if (!latestSchema.IsValidDataType(value.ToString()))
                            {
                                if (newDefaultValue.HasValue())
                                {
                                    item.SetValue(latestSchema.IsValidDataType(newDefaultValue)
                                        ? newDefaultValue
                                        : null, newDataType);
                                }
                                else
                                {
                                    item.SetValue(null, newDataType);
                                }
                            }
                        }
                        this.result.Add(MigrationChangeType.Breaking,
                            MigrationMessages.DraftItem_AttributeDataTypeChanged, item.FullyQualifiedPath,
                            oldDataType, newDataType);
                    }

                    if (oldChoices.HasNone() && newChoices.HasAny())
                    {
                        if (value.Exists())
                        {
                            if (!latestSchema.Choices.Contains(value.ToString()))
                            {
                                item.SetValue(null, newDataType);
                            }
                            this.result.Add(MigrationChangeType.NonBreaking,
                                MigrationMessages.DraftItem_AttributeChoicesAdded, item.FullyQualifiedPath,
                                item.Value);
                        }
                    }
                    if (oldChoices.HasAny() && newChoices.HasNone())
                    {
                        this.result.Add(MigrationChangeType.Breaking,
                            MigrationMessages.DraftItem_AttributeChoicesDeleted, item.FullyQualifiedPath);
                    }
                    if (oldChoices.HasAny() && newChoices.HasAny())
                    {
                        if (!oldChoices.SequenceEqual(newChoices))
                        {
                            if (value.Exists())
                            {
                                if (!latestSchema.Choices.Contains(value.ToString()))
                                {
                                    if (latestSchema.Choices.Contains(newDefaultValue))
                                    {
                                        item.SetValue(newDefaultValue, newDataType);
                                        this.result.Add(MigrationChangeType.Breaking,
                                            MigrationMessages.DraftItem_AttributeChoicesChanged,
                                            item.FullyQualifiedPath, value, newDefaultValue);
                                    }
                                    else
                                    {
                                        item.SetValue(null, newDataType);
                                        this.result.Add(MigrationChangeType.Breaking,
                                            MigrationMessages.DraftItem_AttributeChoicesChanged,
                                            item.FullyQualifiedPath, value, null);
                                    }
                                }
                            }
                        }
                    }

                    if (newDefaultValue.NotEqualsOrdinal(oldDefaultValue))
                    {
                        if (value.Exists())
                        {
                            if (value.ToString().EqualsIgnoreCase(oldDefaultValue))
                            {
                                item.SetValue(newDefaultValue, newDataType);
                                this.result.Add(MigrationChangeType.NonBreaking,
                                    MigrationMessages.DraftItem_AttributeDefaultValueChanged,
                                    item.FullyQualifiedPath, value, newDefaultValue);
                            }
                        }
                        else
                        {
                            item.SetValue(newDefaultValue, newDataType);
                            this.result.Add(MigrationChangeType.NonBreaking,
                                MigrationMessages.DraftItem_AttributeDefaultValueChanged, item.FullyQualifiedPath,
                                value, newDefaultValue);
                        }
                    }
                }

                if (attributeToDelete.HasValue())
                {
                    this.deletedAttributes.Peek().Add(attributeToDelete);
                }

                return true;
            }

            private static void DeleteElements(DraftItem item, IEnumerable<string> elementNames)
            {
                elementNames.ToListSafe()
                    .ForEach(item.DestroyProperty);
            }

            private static void DeleteAttributes(DraftItem item, IEnumerable<string> attributeNames)
            {
                attributeNames.ToListSafe()
                    .ForEach(item.DestroyProperty);
            }

            private void AddNewAttributes<TSchema>(DraftItem item, TSchema existingSchema)
                where TSchema : IPatternElementSchema
            {
                var latestSchema = item.Schema.ResolveSchema<TSchema>(this.latestToolkit, false);
                if (latestSchema.Exists())
                {
                    latestSchema.Attributes
                        .Remainder(existingSchema.Attributes, schema => schema.Id).ToList()
                        .ForEach(attr =>
                        {
                            var property = item.CreateProperty(this.latestToolkit, attr);
                            this.result.Add(MigrationChangeType.NonBreaking,
                                MigrationMessages.DraftItem_AttributeAdded, property.FullyQualifiedPath,
                                property.Value);
                        });
                }
            }
        }

        private class SchemaValidator : IDraftItemVisitor
        {
            public SchemaValidator()
            {
                Validations = new ValidationResults();
            }

            public ValidationResults Validations { get; }

            public bool VisitElementEnter(DraftItem item)
            {
                if (!item.IsMaterialised)
                {
                    if (item.ElementSchema.HasCardinalityOfAtLeastOne())
                    {
                        Validations.Add(item,
                            ValidationMessages.DraftItem_ValidationRule_ElementRequiresAtLeastOneInstance
                                .Substitute(item.Name));
                    }
                    if (item.ElementSchema.HasCardinalityOfAtMostOne())
                    {
                        if (item.Items.HasAny() && item.Items.Count > 1)
                        {
                            Validations.Add(item,
                                ValidationMessages.DraftItem_ValidationRule_ElementHasMoreThanOneInstance
                                    .Substitute(item.Name));
                        }
                    }
                }

                return true;
            }

            public bool VisitEphemeralCollectionEnter(DraftItem item)
            {
                if (item.IsMaterialised)
                {
                    if (item.ElementSchema.HasCardinalityOfAtLeastOne())
                    {
                        if (item.Items.HasNone())
                        {
                            Validations.Add(item,
                                ValidationMessages.DraftItem_ValidationRule_ElementRequiresAtLeastOneInstance
                                    .Substitute(item.Name));
                        }
                    }
                    if (item.ElementSchema.HasCardinalityOfAtMostOne())
                    {
                        if (item.Items.HasAny() && item.Items.Count > 1)
                        {
                            Validations.Add(item,
                                ValidationMessages.DraftItem_ValidationRule_ElementHasMoreThanOneInstance
                                    .Substitute(item.Name));
                        }
                    }
                }
                else
                {
                    if (item.ElementSchema.HasCardinalityOfAtLeastOne())
                    {
                        Validations.Add(item,
                            ValidationMessages.DraftItem_ValidationRule_ElementRequiresAtLeastOneInstance
                                .Substitute(item.Name));
                    }
                    if (item.ElementSchema.HasCardinalityOfAtMostOne())
                    {
                        if (item.Items.HasAny() && item.Items.Count > 1)
                        {
                            Validations.Add(item,
                                ValidationMessages.DraftItem_ValidationRule_ElementHasMoreThanOneInstance
                                    .Substitute(item.Name));
                        }
                    }
                }

                return true;
            }

            public bool VisitAttributeEnter(DraftItem item)
            {
                if (item.IsMaterialised)
                {
                    Validations.AddRange(item.AttributeSchema.Validate(item, item.Value));
                }
                return true;
            }
        }
    }

    public class DraftItemProperty
    {
        private readonly DraftItem item;

        public DraftItemProperty(DraftItem item)
        {
            if (!item.IsAttribute)
            {
                throw new AutomateException(ExceptionMessages.DraftItem_NotAnAttribute.Substitute(item.Name));
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

        public void SetValue(object value)
        {
            this.item.Value = value;
        }

        public void ResetValue()
        {
            SetValue(HasDefaultValue
                ? this.item.AttributeSchema.DefaultValue
                : null);
        }
    }

    public class DraftItemSchema : IPersistable
    {
        public DraftItemSchema(string id, DraftItemSchemaType schemaType)
        {
            id.GuardAgainstNullOrEmpty(nameof(id));
            SchemaId = id;
            SchemaType = schemaType;
        }

        private DraftItemSchema(PersistableProperties properties,
            IPersistableFactory factory)
        {
            SchemaId = properties.Rehydrate<string>(factory, nameof(SchemaId));
            SchemaType = properties.Rehydrate<DraftItemSchemaType>(factory, nameof(SchemaType));
        }

        public string SchemaId { get; }

        public DraftItemSchemaType SchemaType { get; }

        public PersistableProperties Dehydrate()
        {
            var properties = new PersistableProperties();
            properties.Dehydrate(nameof(SchemaId), SchemaId);
            properties.Dehydrate(nameof(System.Data.SchemaType), SchemaType);

            return properties;
        }

        public static DraftItemSchema Rehydrate(PersistableProperties properties,
            IPersistableFactory factory)
        {
            return new DraftItemSchema(properties, factory);
        }

        public TSchema ResolveSchema<TSchema>(ToolkitDefinition toolkit, bool throwOnNotFound = true)
            where TSchema : ISchema
        {
            toolkit.GuardAgainstNull(nameof(toolkit));

            if (SchemaType == DraftItemSchemaType.None)
            {
                throw new AutomateException(ExceptionMessages.DraftItem_InvalidDraftItemSchema);
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
            else if (typeof(TSchema) == typeof(IElementSchema))
            {
                target = toolkit.Pattern.FindSchema<Element>(SchemaId);
                if (target.Exists())
                {
                    return SchemaType == DraftItemSchemaType.CollectionItem
                        ? (TSchema)(ISchema)new CollectionItemSchema((Element)target)
                        : (TSchema)((Element)target).ToSchema();
                }
            }
            else if (typeof(TSchema) == typeof(IAttributeSchema))
            {
                target = toolkit.Pattern.FindSchema<Attribute>(SchemaId);
                if (target.Exists())
                {
                    return (TSchema)((Attribute)target).ToSchema();
                }
            }
            else
            {
                throw new NotSupportedException();
            }

            if (throwOnNotFound)
            {
                throw new AutomateException(ExceptionMessages.DraftItem_UnknownSchema.Substitute(SchemaId, SchemaType));
            }

            return default;
        }
    }

    public enum DraftItemSchemaType
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
    ///     so that we can populate cyclic pointers in memory for ancestor properties (i.e. <see cref="DraftItem.Parent" />
    ///     which would otherwise cause infinite recursion, if each <see cref="DictionaryEntry" /> was evaluated when this
    ///     dictionary was being constructed.
    ///     This dictionary type must implement <see cref="IDictionary" /> so that it can be used in text transformation using
    ///     Scriban.
    /// </summary>
    public class LazyDraftItemDictionary : IDictionary
    {
        private readonly DraftItem draftItem;
        private readonly bool includeAncestry;
        private readonly bool includeSchema;

        /// <summary>
        ///     For serialization
        /// </summary>
        [UsedImplicitly]
        public LazyDraftItemDictionary()
        {
        }

        public LazyDraftItemDictionary(DraftItem draftItem, bool includeAncestry, bool includeSchema)
        {
            draftItem.GuardAgainstNull(nameof(draftItem));
            this.draftItem = draftItem;
            this.includeAncestry = includeAncestry;
            this.includeSchema = includeSchema;
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

                throw new KeyNotFoundException(ExceptionMessages.LazyDraftItemDictionary_NotFound.Substitute(key));
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
            if (this.draftItem.IsMaterialised)
            {
                if (this.draftItem.IsPattern || this.draftItem.IsElement || this.draftItem.IsEphemeralCollection)
                {
                    yield return new DictionaryEntry(AsIsMemberName(nameof(DraftItem.Id)), this.draftItem.Id);

                    yield return new DictionaryEntry(AsIsMemberName(nameof(DraftItem.ConfigurePath)),
                        this.draftItem.ConfigurePath);

                    if (this.includeSchema)
                    {
                        var schema = new Dictionary<string, object>
                        {
                            { AsIsMemberName("Id"), this.draftItem.Schema.SchemaId },
                            { AsIsMemberName("Type"), this.draftItem.Schema.SchemaType.ToString() }
                        };
                        yield return new DictionaryEntry(AsIsMemberName(DraftItem.SchemaPropertyName), schema);
                    }
                }
                if (this.includeAncestry)
                {
                    if (this.draftItem.IsPattern || this.draftItem.IsElement ||
                        this.draftItem.IsEphemeralCollection || this.draftItem.IsAttribute)
                    {
                        if (this.draftItem.Parent.Exists())
                        {
                            yield return new DictionaryEntry(AsIsMemberName(nameof(DraftItem.Parent)),
                                new LazyDraftItemDictionary(this.draftItem.Parent, this.includeAncestry,
                                    this.includeSchema));
                        }
                    }
                }
                if (this.draftItem.IsPattern || this.draftItem.IsElement || this.draftItem.IsEphemeralCollection)
                {
                    if (this.draftItem.Properties.HasAny())
                    {
                        foreach (var prop in this.draftItem.Properties.Safe())
                        {
                            var (name, value) = prop;
                            if (value.IsMaterialised)
                            {
                                if (value.IsAttribute)
                                {
                                    if (value.Value.Exists())
                                    {
                                        yield return new DictionaryEntry(AsIsMemberName(name), value.Value);
                                    }
                                }
                                if (value.IsElement || value.IsEphemeralCollection)
                                {
                                    yield return new DictionaryEntry(AsIsMemberName(name),
                                        new LazyDraftItemDictionary(value, this.includeAncestry, this.includeSchema));
                                }
                            }
                        }
                    }
                }
                if (this.draftItem.IsEphemeralCollection)
                {
                    if (this.draftItem.Items.HasAny())
                    {
                        var items = this.draftItem.Items.Select(item =>
                            new LazyDraftItemDictionary(item, this.includeAncestry, this.includeSchema));
                        yield return new DictionaryEntry(AsIsMemberName(nameof(DraftItem.Items)), items);
                    }
                }
                if (this.draftItem.IsAttribute)
                {
                    if (this.draftItem.Value.Exists())
                    {
                        yield return new DictionaryEntry(AsIsMemberName(this.draftItem.Name), this.draftItem.Value);
                    }
                }
            }
        }

        private static string AsIsMemberName(string memberName)
        {
            return memberName;
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
                    throw new InvalidOperationException(ExceptionMessages.DictionaryEntryEnumerator_OutOfRange);
                }
            }
        }
    }
}