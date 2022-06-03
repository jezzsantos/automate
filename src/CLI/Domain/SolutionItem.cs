using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Automate.CLI.Extensions;

namespace Automate.CLI.Domain
{
    internal class SolutionItem : IIdentifiableEntity, IPersistable, ISolutionItemVisitable
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
            : this(pattern.Name, toolkit, null, new SolutionItemSchema(pattern.Id, SolutionItemSchemaType.Pattern))
        {
            toolkit.GuardAgainstNull(nameof(toolkit));
            pattern.GuardAgainstNull(nameof(pattern));

            MaterialisePatternElement(toolkit, pattern, false);
        }

        public SolutionItem(ToolkitDefinition toolkit, Element element, SolutionItem parent,
            bool isCollectionItem = false) : this(toolkit, element.ToSchema(), parent, isCollectionItem)
        {
        }

        private SolutionItem(ToolkitDefinition toolkit, IElementSchema element, SolutionItem parent,
            bool isCollectionItem)
            : this(element.Name, toolkit, parent, new SolutionItemSchema(element.Id, isCollectionItem
                ? SolutionItemSchemaType.CollectionItem
                : element.IsCollection
                    ? SolutionItemSchemaType.EphemeralCollection
                    : SolutionItemSchemaType.Element))
        {
            toolkit.GuardAgainstNull(nameof(toolkit));
            element.GuardAgainstNull(nameof(element));

            UnMaterialise();
        }

        public SolutionItem(ToolkitDefinition toolkit, Attribute attribute, SolutionItem parent) : this(toolkit,
            attribute.ToSchema(), parent)
        {
        }

        private SolutionItem(ToolkitDefinition toolkit, IAttributeSchema attribute, SolutionItem parent)
            : this(attribute.Name, toolkit, parent,
                new SolutionItemSchema(attribute.Id, SolutionItemSchemaType.Attribute))
        {
            toolkit.GuardAgainstNull(nameof(toolkit));
            attribute.GuardAgainstNull(nameof(attribute));

            MaterialiseAttribute(attribute);
        }

        private SolutionItem(string name, ToolkitDefinition toolkit, SolutionItem parent, SolutionItemSchema schema)
        {
            Id = IdGenerator.Create();
            Name = name;
            Toolkit = toolkit;
            Schema = schema;
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

        public bool IsElement =>
            Schema.SchemaType is SolutionItemSchemaType.Element or SolutionItemSchemaType.CollectionItem;

        public bool IsEphemeralCollection => Schema.SchemaType == SolutionItemSchemaType.EphemeralCollection;

        public bool IsAttribute => Schema.SchemaType == SolutionItemSchemaType.Attribute;

        public IReadOnlyList<ArtifactLink> ArtifactLinks => this.artifactLinks;

        public string FullyQualifiedPath => GetPath(false);

        public string PathReference => GetPath(true);

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

            if (IsElement || IsEphemeralCollection)
            {
                MaterialisePatternElement(Toolkit, ElementSchema, IsEphemeralCollection);
            }
            else if (IsAttribute)
            {
                MaterialiseAttribute(AttributeSchema, value);
            }
            else
            {
                throw new AutomateException(ExceptionMessages.SolutionItem_ValueAlreadyMaterialised);
            }

            return this;
        }

        /// <summary>
        ///     Creates a new instance in the <see cref="Items" /> of this "ephemeral" collection.
        ///     This ephemeral collection is never actually operated upon. Only <see cref="Items" /> in its collection are operated
        ///     on.
        /// </summary>
        public SolutionItem MaterialiseCollectionItem()
        {
            if (!IsEphemeralCollection)
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

        public ValidationResults Validate()
        {
            var validator = new SchemaValidator();
            TraverseDescendants(validator);
            return validator.Validations;
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
            if (!IsPattern && !IsElement && !IsEphemeralCollection)
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

        public void AddArtifactLink(string commandId, string path, string tag)
        {
            var link = new ArtifactLink(commandId, path, tag);
            this.artifactLinks.Add(link);
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

                property.SetValue(value);
            }
        }

        public void Migrate(ToolkitDefinition latestToolkit, SolutionUpgradeResult result)
        {
            var migrator = new SchemaMigrator(latestToolkit, result);
            TraverseDescendants(migrator);
        }

        public void ResetAllProperties()
        {
            if (!IsPattern && !IsElement)
            {
                throw new AutomateException(ExceptionMessages.SolutionItem_ResetPropertiesForNonElement);
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
                throw new AutomateException(ExceptionMessages.SolutionItem_ClearItemsForNonCollection);
            }

            this.items.Clear();
        }

        public void Delete(SolutionItem childItem)
        {
            if (!IsPattern && !IsElement && !IsEphemeralCollection)
            {
                throw new AutomateException(ExceptionMessages.SolutionItem_DeleteForNonElement);
            }

            if (!childItem.IsElement && !childItem.IsEphemeralCollection)
            {
                throw new AutomateException(ExceptionMessages.SolutionItem_DeleteForNonElementChild);
            }

            if (IsPattern || IsElement)
            {
                if (Properties.All(prop => prop.Value != childItem))
                {
                    throw new AutomateException(ExceptionMessages.SolutionItem_DeleteWithUnknownChild);
                }

                this.properties.Remove(childItem.Name);
            }

            if (IsEphemeralCollection)
            {
                if (Items.All(item => item != childItem))
                {
                    throw new AutomateException(ExceptionMessages.SolutionItem_DeleteWithUnknownChild);
                }

                this.items.Remove(childItem);
            }
        }

        public string Id { get; }

        public bool Accept(ISolutionItemVisitor visitor)
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

        /// <summary>
        ///     This [hierarchical] traversal requires that we enter and exit composite nodes (i.e. the <see cref="SolutionItem" />
        ///     that themselves are composed of other nodes or composite nodes.
        ///     Once 'entered' a specific composite node, we traverse each of its child nodes (and their child nodes), before
        ///     'exiting' this specific composite node.
        ///     In this way we can tell the difference between a sibling node and a child node (of the same type).
        ///     Furthermore, each visit returns to us whether we are to abort visiting that branch of the graph, and we return up
        ///     to the root node, or continue down that branch, and on to sibling branches.
        ///     In other words, an aborted 'enter', must result in an 'exit', but then followed by returning up that branch of
        ///     nodes to the root of the graph, before exiting the traversal.
        /// </summary>
        private void TraverseDescendants(ISolutionItemVisitor visitor)
        {
            Accept(visitor);
        }

        private SolutionItem AddProperty(ToolkitDefinition toolkit, IAttributeSchema schema)
        {
            var property = new SolutionItem(toolkit, schema, this);
            this.properties.Add(schema.Name, property);

            return property;
        }

        /// <summary>
        ///     Cannot use the <see cref="ISolutionItemVisitor" /> to traverse up the hierarchy,
        ///     so we have to traverse up using a custom iterator
        /// </summary>
        private string GetPath(bool asReference)
        {
            var path = GetAncestorPath(this);
            return asReference
                ? $"{{{path}}}"
                : path;

            string GetAncestorPath(SolutionItem solutionItem)
            {
                if (solutionItem.IsPattern)
                {
                    return solutionItem.Name;
                }

                if (solutionItem.IsElement || solutionItem.IsEphemeralCollection)
                {
                    if (solutionItem.Parent.Exists())
                    {
                        return solutionItem.Schema.SchemaType == SolutionItemSchemaType.CollectionItem
                            ? $"{GetAncestorPath(solutionItem.Parent)}.{solutionItem.ElementSchema.Name}.{solutionItem.Id}"
                            : $"{GetAncestorPath(solutionItem.Parent)}.{solutionItem.Name}";
                    }
                    throw new AutomateException(ExceptionMessages.SolutionItem_UnknownPath);
                }
                if (solutionItem.IsAttribute)
                {
                    if (solutionItem.Parent.Exists())
                    {
                        return $"{GetAncestorPath(solutionItem.Parent)}.{solutionItem.Name}";
                    }
                    throw new AutomateException(ExceptionMessages.SolutionItem_UnknownPath);
                }
                throw new AutomateException(ExceptionMessages.SolutionItem_UnknownPath);
            }
        }

        private void RemoveProperty(string propertyName)
        {
            if (Properties.HasAny())
            {
                if (Properties.Any(prop => prop.Key.EqualsIgnoreCase(propertyName)))
                {
                    this.properties.Remove(propertyName);
                }
            }
        }

        private void MaterialisePatternElement(ToolkitDefinition toolkit, IPatternElementSchema schema,
            bool isCollection)
        {
            this.properties = new Dictionary<string, SolutionItem>();
            if (isCollection)
            {
                this.items = new List<SolutionItem>();
            }
            else
            {
                schema.Attributes.ToListSafe()
                    .ForEach(attr => { this.properties.Add(attr.Name, new SolutionItem(toolkit, attr, this)); });
                schema.Elements.ToListSafe()
                    .ForEach(ele =>
                    {
                        var element = new SolutionItem(toolkit, ele, this, false);
                        if (ele.ShouldAutoCreate())
                        {
                            element.Materialise();
                        }
                        this.properties.Add(ele.Name, element);
                    });
                this.items = null;
            }
            IsMaterialised = true;
            Value = null;
        }

        private void MaterialiseAttribute(IAttributeSchema schema, object value = null)
        {
            this.properties = null;
            this.items = null;
            IsMaterialised = value.Exists()
                ? true
                : schema.DefaultValue.HasValue();
            SetValue(value.Exists()
                    ? schema.IsValidDataType(value.ToString())
                        ? value
                        : null
                    : schema.IsValidDataType(schema.DefaultValue)
                        ? schema.DefaultValue
                        : null,
                schema.DataType);
        }

        private void UnMaterialise()
        {
            if (IsPattern)
            {
                throw new AutomateException(
                    ExceptionMessages.SolutionItem_UnMaterialisationOnPatternForbidden.Format(PatternSchema.Name));
            }

            if (IsElement || IsEphemeralCollection)
            {
                Value = null;
            }
            else if (IsAttribute)
            {
                SetValue(null, AttributeSchema.DataType);
            }
            else
            {
                throw new AutomateException(ExceptionMessages.SolutionItem_UnMaterialisationOnValueForbidden);
            }

            this.properties = null;
            this.items = null;
            IsMaterialised = false;
        }

        private SolutionItem CreateEphemeralCollectionItem()
        {
            var childElementItem = new SolutionItem(Toolkit, ElementSchema, Parent, true);
            childElementItem.Materialise();

            return childElementItem;
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
                throw new AutomateException(ExceptionMessages.SolutionItem_HasNoAutomations);
            }

            return automations.FirstOrDefault(auto => auto.Name.EqualsIgnoreCase(name));
        }

        private void SetValue(object value, string dataType)
        {
            this._value = Attribute.SetValue(dataType, value);
        }

        private class SchemaMigrator : ISolutionItemVisitor
        {
            private readonly Stack<List<string>> deletedAttributes;
            private readonly Stack<List<string>> deletedElements;
            private readonly ToolkitDefinition latestToolkit;
            private readonly SolutionUpgradeResult result;

            public SchemaMigrator(ToolkitDefinition latestToolkit, SolutionUpgradeResult result)
            {
                latestToolkit.GuardAgainstNull(nameof(latestToolkit));
                result.GuardAgainstNull(nameof(result));
                this.latestToolkit = latestToolkit;
                this.result = result;
                this.deletedAttributes = new Stack<List<string>>();
                this.deletedElements = new Stack<List<string>>();
            }

            public bool VisitPatternEnter(SolutionItem item)
            {
                this.deletedAttributes.Push(new List<string>());
                this.deletedElements.Push(new List<string>());
                return true;
            }

            public bool VisitPatternExit(SolutionItem item)
            {
                DeleteAttributes(item, this.deletedAttributes.Pop());
                DeleteElements(item, this.deletedElements.Pop());
                AddNewAttributes(item, item.PatternSchema);

                return true;
            }

            public bool VisitElementEnter(SolutionItem item)
            {
                var elementToDelete = string.Empty;
                if (item.IsMaterialised)
                {
                    var latestSchema = item.Schema.ResolveSchema<IElementSchema>(this.latestToolkit, false);
                    if (latestSchema.NotExists())
                    {
                        elementToDelete = item.ElementSchema.Name;
                        this.result.Add(MigrationChangeType.Breaking, MigrationMessages.SolutionItem_ElementDeleted,
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

            public bool VisitElementExit(SolutionItem item)
            {
                DeleteAttributes(item, this.deletedAttributes.Pop());
                DeleteElements(item, this.deletedElements.Pop());
                AddNewAttributes(item, item.ElementSchema);

                return true;
            }

            public bool VisitEphemeralCollectionEnter(SolutionItem item)
            {
                var elementToDelete = string.Empty;
                if (item.IsMaterialised)
                {
                    var latestSchema = item.Schema.ResolveSchema<IElementSchema>(this.latestToolkit, false);
                    if (latestSchema.NotExists())
                    {
                        elementToDelete = item.ElementSchema.Name;
                        this.result.Add(MigrationChangeType.Breaking, MigrationMessages.SolutionItem_ElementDeleted,
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

            public bool VisitEphemeralCollectionExit(SolutionItem item)
            {
                DeleteAttributes(item, this.deletedAttributes.Pop());
                DeleteElements(item, this.deletedElements.Pop());
                AddNewAttributes(item, item.ElementSchema);

                return true;
            }

            public bool VisitAttributeEnter(SolutionItem item)
            {
                var attributeToDelete = string.Empty;

                var latestSchema = item.Schema.ResolveSchema<IAttributeSchema>(this.latestToolkit, false);
                if (latestSchema.NotExists())
                {
                    attributeToDelete = item.AttributeSchema.Name;
                    this.result.Add(MigrationChangeType.Breaking, MigrationMessages.SolutionItem_AttributeDeleted,
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
                        var property = item.Parent.AddProperty(this.latestToolkit, latestSchema);
                        property.SetValue(value, newDataType);
                        this.result.Add(MigrationChangeType.Breaking,
                            MigrationMessages.SolutionItem_AttributeNameChanged, item.FullyQualifiedPath, oldName,
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
                                    if (latestSchema.IsValidDataType(newDefaultValue))
                                    {
                                        item.SetValue(newDefaultValue, newDataType);
                                    }
                                    else
                                    {
                                        item.SetValue(null, newDataType);
                                    }
                                }
                                else
                                {
                                    item.SetValue(null, newDataType);
                                }
                            }
                        }
                        this.result.Add(MigrationChangeType.Breaking,
                            MigrationMessages.SolutionItem_AttributeDataTypeChanged, item.FullyQualifiedPath,
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
                                MigrationMessages.SolutionItem_AttributeChoicesAdded, item.FullyQualifiedPath,
                                item.Value);
                        }
                    }
                    if (oldChoices.HasAny() && newChoices.HasNone())
                    {
                        this.result.Add(MigrationChangeType.Breaking,
                            MigrationMessages.SolutionItem_AttributeChoicesDeleted, item.FullyQualifiedPath);
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
                                            MigrationMessages.SolutionItem_AttributeChoicesChanged,
                                            item.FullyQualifiedPath, value, newDefaultValue);
                                    }
                                    else
                                    {
                                        item.SetValue(null, newDataType);
                                        this.result.Add(MigrationChangeType.Breaking,
                                            MigrationMessages.SolutionItem_AttributeChoicesChanged,
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
                                    MigrationMessages.SolutionItem_AttributeDefaultValueChanged,
                                    item.FullyQualifiedPath, value, newDefaultValue);
                            }
                        }
                        else
                        {
                            item.SetValue(newDefaultValue, newDataType);
                            this.result.Add(MigrationChangeType.NonBreaking,
                                MigrationMessages.SolutionItem_AttributeDefaultValueChanged, item.FullyQualifiedPath,
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

            private static void DeleteElements(SolutionItem item, IEnumerable<string> elementNames)
            {
                elementNames.ToListSafe()
                    .ForEach(item.RemoveProperty);
            }

            private static void DeleteAttributes(SolutionItem item, IEnumerable<string> attributeNames)
            {
                attributeNames.ToListSafe()
                    .ForEach(item.RemoveProperty);
            }

            private void AddNewAttributes<TSchema>(SolutionItem item, TSchema existingSchema)
                where TSchema : IPatternElementSchema
            {
                var latestSchema = item.Schema.ResolveSchema<TSchema>(this.latestToolkit, false);
                if (latestSchema.Exists())
                {
                    latestSchema.Attributes
                        .Remainder(existingSchema.Attributes, schema => schema.Id)
                        .ToList()
                        .ForEach(attr =>
                        {
                            var property = item.AddProperty(this.latestToolkit, attr);
                            this.result.Add(MigrationChangeType.NonBreaking,
                                MigrationMessages.SolutionItem_AttributeAdded, property.FullyQualifiedPath,
                                property.Value);
                        });
                }
            }
        }

        private class SchemaValidator : ISolutionItemVisitor
        {
            public SchemaValidator()
            {
                Validations = new ValidationResults();
            }

            public ValidationResults Validations { get; }

            public bool VisitElementEnter(SolutionItem item)
            {
                if (!item.IsMaterialised)
                {
                    if (item.ElementSchema.HasCardinalityOfAtLeastOne())
                    {
                        Validations.Add(item,
                            ValidationMessages.SolutionItem_ValidationRule_ElementRequiresAtLeastOneInstance
                                .Format(item.Name));
                    }
                    if (item.ElementSchema.HasCardinalityOfAtMostOne())
                    {
                        if (item.Items.HasAny() && item.Items.Count > 1)
                        {
                            Validations.Add(item,
                                ValidationMessages.SolutionItem_ValidationRule_ElementHasMoreThanOneInstance
                                    .Format(item.Name));
                        }
                    }
                }

                return true;
            }

            public bool VisitEphemeralCollectionEnter(SolutionItem item)
            {
                if (item.IsMaterialised)
                {
                    if (item.ElementSchema.HasCardinalityOfAtLeastOne())
                    {
                        if (item.Items.HasNone())
                        {
                            Validations.Add(item,
                                ValidationMessages.SolutionItem_ValidationRule_ElementRequiresAtLeastOneInstance
                                    .Format(item.Name));
                        }
                    }
                    if (item.ElementSchema.HasCardinalityOfAtMostOne())
                    {
                        if (item.Items.HasAny() && item.Items.Count > 1)
                        {
                            Validations.Add(item,
                                ValidationMessages.SolutionItem_ValidationRule_ElementHasMoreThanOneInstance
                                    .Format(item.Name));
                        }
                    }
                }
                else
                {
                    if (item.ElementSchema.HasCardinalityOfAtLeastOne())
                    {
                        Validations.Add(item,
                            ValidationMessages.SolutionItem_ValidationRule_ElementRequiresAtLeastOneInstance
                                .Format(item.Name));
                    }
                    if (item.ElementSchema.HasCardinalityOfAtMostOne())
                    {
                        if (item.Items.HasAny() && item.Items.Count > 1)
                        {
                            Validations.Add(item,
                                ValidationMessages.SolutionItem_ValidationRule_ElementHasMoreThanOneInstance
                                    .Format(item.Name));
                        }
                    }
                }

                return true;
            }

            public bool VisitAttributeEnter(SolutionItem item)
            {
                Validations.AddRange(item.AttributeSchema.Validate(item, item.Value));

                return true;
            }
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

        public void SetValue(object value)
        {
            this.item.Value = value;
        }

        public void ResetValue()
        {
            if (HasDefaultValue)
            {
                SetValue(this.item.AttributeSchema.DefaultValue);
            }
            else
            {
                SetValue(null);
            }
        }
    }

    internal class SolutionItemSchema : IPersistable
    {
        public static readonly SolutionItemSchema None = new();

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

        public TSchema ResolveSchema<TSchema>(ToolkitDefinition toolkit, bool throwOnNotFound = true)
            where TSchema : ISchema
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
            else if (typeof(TSchema) == typeof(IElementSchema))
            {
                target = toolkit.Pattern.FindSchema<Element>(SchemaId);
                if (target.Exists())
                {
                    return SchemaType == SolutionItemSchemaType.CollectionItem
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
                throw new AutomateException(ExceptionMessages.SolutionItem_UnknownSchema.Format(SchemaId, SchemaType));
            }

            return default;
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
            if (this.solutionItem.IsPattern || this.solutionItem.IsElement || this.solutionItem.IsEphemeralCollection)
            {
                yield return new DictionaryEntry(AsIsMemberName(nameof(SolutionItem.Id)), this.solutionItem.Id);
            }
            if (this.includeAncestry)
            {
                if (this.solutionItem.IsPattern || this.solutionItem.IsElement ||
                    this.solutionItem.IsEphemeralCollection || this.solutionItem.IsAttribute)
                {
                    if (this.solutionItem.Parent.Exists())
                    {
                        yield return new DictionaryEntry(AsIsMemberName(nameof(SolutionItem.Parent)),
                            new LazySolutionItemDictionary(this.solutionItem.Parent, true));
                    }
                }
            }
            if (this.solutionItem.IsPattern || this.solutionItem.IsElement || this.solutionItem.IsEphemeralCollection)
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
                                yield return new DictionaryEntry(AsIsMemberName(name), item.Value);
                            }
                        }
                        if (item.IsElement || item.IsEphemeralCollection)
                        {
                            yield return new DictionaryEntry(AsIsMemberName(name),
                                new LazySolutionItemDictionary(item, this.includeAncestry));
                        }
                    }
                }
            }
            if (this.solutionItem.IsEphemeralCollection)
            {
                if (this.solutionItem.Items.HasAny())
                {
                    var items = this.solutionItem.Items.Select(item =>
                        new LazySolutionItemDictionary(item, this.includeAncestry));
                    yield return new DictionaryEntry(AsIsMemberName(nameof(SolutionItem.Items)), items);
                }
            }
            if (this.solutionItem.IsAttribute)
            {
                if (this.solutionItem.Value.Exists())
                {
                    yield return new DictionaryEntry(AsIsMemberName(this.solutionItem.Name), this.solutionItem.Value);
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
                    throw new InvalidOperationException("Enumerator is before or after the collection.");
                }
            }
        }
    }
}