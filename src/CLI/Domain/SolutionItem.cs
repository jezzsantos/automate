using System.Collections.Generic;
using System.Linq;
using Automate.CLI.Extensions;
using StringExtensions = ServiceStack.StringExtensions;

namespace Automate.CLI.Domain
{
    internal class SolutionItem : IIdentifiableEntity
    {
        // ReSharper disable once InconsistentNaming
        private object _value;

        public SolutionItem(PatternDefinition pattern)
        {
            Id = IdGenerator.Create();
            Name = pattern.Name;
            PatternSchema = pattern;
            AttributeSchema = null;
            ElementSchema = null;
            IsMaterialised = true;
            Value = null;
            Items = null;

            Properties = new Dictionary<string, SolutionItem>();
            pattern.Attributes
                .ForEach(attr =>
                {
                    Properties.Add(attr.Name,
                        new SolutionItem(attr));
                });
            pattern.Elements
                .ForEach(ele => { Properties.Add(ele.Name, new SolutionItem(ele)); });
        }

        public SolutionItem(Attribute attribute)
        {
            Id = IdGenerator.Create();
            Name = attribute.Name;
            PatternSchema = null;
            ElementSchema = null;
            AttributeSchema = attribute;
            IsMaterialised = attribute.DefaultValue.HasValue();
            SetValue(attribute.DefaultValue, attribute.DataType);
            Properties = null;
            Items = null;
        }

        public SolutionItem(Element element)
        {
            Id = IdGenerator.Create();
            Name = element.Name;
            PatternSchema = null;
            AttributeSchema = null;
            ElementSchema = element;
            IsMaterialised = false;
            Value = null;
            Properties = null;
            Items = null;
        }

        public SolutionItem(object value, string dataType)
        {
            Id = IdGenerator.Create();
            Name = null;
            PatternSchema = null;
            AttributeSchema = null;
            ElementSchema = null;
            IsMaterialised = true;
            SetValue(value, dataType);
            Properties = null;
            Items = null;
        }

        /// <summary>
        ///     For serialization
        /// </summary>
        public SolutionItem()
        {
        }

        public PatternDefinition PatternSchema { get; set; }

        public Element ElementSchema { get; set; }

        public Attribute AttributeSchema { get; set; }

        public object Value
        {
            get => this._value;
            set
            {
                if (IsAttribute)
                {
                    this._value = Attribute.SetValue(AttributeSchema.DataType, value);
                }
                else
                {
                    this._value = value;
                }
            }
        }

        public Dictionary<string, SolutionItem> Properties { get; set; }

        public List<SolutionItem> Items { get; set; }

        public string Name { get; set; }

        public bool IsMaterialised { get; set; }

        public bool IsPattern => PatternSchema.Exists();

        public bool IsElement => ElementSchema.Exists() && !ElementSchema.IsCollection;

        public bool IsCollection => ElementSchema.Exists() && ElementSchema.IsCollection;

        public bool IsAttribute => AttributeSchema.Exists();

        public bool IsValue => PatternSchema.NotExists() && ElementSchema.NotExists() && AttributeSchema.NotExists();

        public List<ArtifactLink> ArtifactLinks { get; set; } = new List<ArtifactLink>();

        public SolutionItem Materialise(object value = null)
        {
            if (IsPattern)
            {
                throw new AutomateException(
                    ExceptionMessages.SolutionItem_PatternAlreadyMaterialised.Format(PatternSchema.Name));
            }

            if (IsElement)
            {
                Properties = new Dictionary<string, SolutionItem>();
                ElementSchema.Attributes.ForEach(
                    attr => { Properties.Add(attr.Name, new SolutionItem(attr)); });
                ElementSchema.Elements.ForEach(ele => { Properties.Add(ele.Name, new SolutionItem(ele)); });
                Items = null;
                IsMaterialised = true;
            }

            if (IsCollection)
            {
                Properties = new Dictionary<string, SolutionItem>();
                Items = new List<SolutionItem>();
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

            var collectedElement = ElementSchema.Clone();
            collectedElement.IsCollection = false;
            collectedElement.Cardinality = ElementCardinality.Single;
            var childItem = new SolutionItem(collectedElement);
            childItem.Materialise();
            Items.Add(childItem);

            return childItem;
        }

        public bool HasAttribute(string name)
        {
            if (IsPattern)
            {
                return PatternSchema.Attributes.Any(attr => attr.Name.EqualsIgnoreCase(name));
            }
            if (IsElement)
            {
                return ElementSchema.Attributes.Any(attr => attr.Name.EqualsIgnoreCase(name));
            }

            return false;
        }

        public SolutionItemProperty GetProperty(string name)
        {
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

            return ValidateInternal(context, false);
        }

        public Dictionary<string, object> GetConfiguration()
        {
            var properties = new Dictionary<string, object>();
            FilterConfiguration(this, properties);
            return properties;

            object FilterConfiguration(SolutionItem solutionItem, IDictionary<string, object> props)
            {
                if (solutionItem.IsAttribute || solutionItem.IsValue)
                {
                    return solutionItem.Value;
                }
                if (solutionItem.IsPattern || solutionItem.IsElement || solutionItem.IsCollection)
                {
                    props.Add(ConvertName(nameof(Id)), solutionItem.Id);
                    if (solutionItem.Properties.HasAny())
                    {
                        foreach (var (key, value) in solutionItem.Properties)
                        {
                            props.Add(ConvertName(key), FilterConfiguration(value, new Dictionary<string, object>()));
                        }
                    }
                    if (solutionItem.Items.HasAny())
                    {
                        var items = new List<object>();
                        solutionItem.Items.ForEach(item =>
                            items.Add(FilterConfiguration(item, new Dictionary<string, object>())));
                        props.Add(ConvertName(nameof(Items)), items);
                    }
                }

                return props;
            }

            string ConvertName(string name)
            {
                return StringExtensions.ToLowercaseUnderscore(name);
            }
        }

        public string Id { get; set; }

        private ValidationResults ValidateInternal(ValidationContext context, bool isItem)
        {
            if (IsPattern)
            {
                var subContext = new ValidationContext(context);
                subContext.Add($"{PatternSchema.Name}");
                return new ValidationResults(
                    Properties.SelectMany(prop => prop.Value.ValidateInternal(subContext, false).Results));
            }

            if (IsElement || IsCollection)
            {
                var subContext = new ValidationContext(context);
                subContext.Add(isItem
                    ? $"{Id}"
                    : $"{ElementSchema.Name}");
                var results = ValidationResults.None;

                if (IsMaterialised)
                {
                    if (IsElement)
                    {
                        if (Properties.HasAny())
                        {
                            results.AddRange(
                                Properties.SelectMany(prop => prop.Value.ValidateInternal(subContext, false).Results));
                        }
                    }

                    if (IsCollection)
                    {
                        if (ElementSchema.HasCardinalityOfAtLeastOne())
                        {
                            if (Items.HasNone())
                            {
                                results.Add(subContext,
                                    ValidationMessages.SolutionItem_ValidationRule_ElementRequiresAtLeastOneInstance
                                        .Format(Name));
                            }
                        }
                        if (ElementSchema.HasCardinalityOfAtMostOne())
                        {
                            if (Items.HasAny() && Items.Count > 1)
                            {
                                results.Add(subContext,
                                    ValidationMessages.SolutionItem_ValidationRule_ElementHasMoreThanOneInstance
                                        .Format(Name));
                            }
                        }
                        if (Items.HasAny())
                        {
                            results.AddRange(Items.SelectMany(item => item.ValidateInternal(subContext, true).Results));
                        }
                    }
                }
                else
                {
                    if (ElementSchema.HasCardinalityOfAtLeastOne())
                    {
                        results.Add(subContext,
                            ValidationMessages.SolutionItem_ValidationRule_ElementRequiresAtLeastOneInstance
                                .Format(Name));
                    }
                    if (ElementSchema.HasCardinalityOfAtMostOne())
                    {
                        if (Items.HasAny() && Items.Count > 1)
                        {
                            results.Add(subContext,
                                ValidationMessages.SolutionItem_ValidationRule_ElementHasMoreThanOneInstance
                                    .Format(Name));
                        }
                    }
                }

                return results;
            }
            
            if (IsAttribute)
            {
                var subContext = new ValidationContext(context);
                subContext.Add($"{AttributeSchema.Name}");
                var results = ValidationResults.None;

                results.AddRange(AttributeSchema.Validate(subContext, Value));

                return results;
            }

            return ValidationResults.None;
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
            if (item.AttributeSchema.NotExists())
            {
                throw new AutomateException(ExceptionMessages.SolutionItem_NotAnAttribute.Format(item.Name));
            }

            this.item = item;
        }

        public bool IsChoice => this.item.AttributeSchema.Choices.Any();

        public List<string> ChoiceValues => this.item.AttributeSchema.Choices;

        public string DataType => this.item.AttributeSchema.DataType;

        public string Name => this.item.AttributeSchema.Name;

        public bool HasChoice(string value)
        {
            return this.item.AttributeSchema.Choices.Any(choice => choice.EqualsIgnoreCase(value));
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
}