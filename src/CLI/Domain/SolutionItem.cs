using System.Collections.Generic;
using System.Linq;
using automate.Extensions;

namespace automate.Domain
{
    internal class SolutionItem
    {
        public SolutionItem(PatternDefinition pattern)
        {
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

        public object Value { get; set; }

        public Dictionary<string, SolutionItem> Properties { get; set; }

        public List<SolutionItem> Items { get; set; }

        public string Name { get; set; }

        public bool IsMaterialised { get; set; }

        public bool IsPattern => PatternSchema.Exists();

        public bool IsElement => ElementSchema.Exists();

        public bool IsAttribute => AttributeSchema.Exists();

        public bool IsValue => PatternSchema.NotExists() && ElementSchema.NotExists() && AttributeSchema.NotExists();

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
                if (!ElementSchema.IsCollection)
                {
                    ElementSchema.Attributes.ForEach(
                        attr => { Properties.Add(attr.Name, new SolutionItem(attr)); });
                }
                ElementSchema.Elements.ForEach(ele => { Properties.Add(ele.Name, new SolutionItem(ele)); });
                Items = ElementSchema.IsCollection
                    ? new List<SolutionItem>()
                    : null;
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
            if (!IsElement
                || !ElementSchema.IsCollection)
            {
                throw new AutomateException(ExceptionMessages.SolutionItem_MaterialiseNotACollection);
            }

            if (!IsMaterialised)
            {
                Materialise();
            }

            var item = new SolutionItem(ElementSchema);
            item.Materialise();
            item.ElementSchema.Attributes.ForEach(
                attr => { item.Properties.Add(attr.Name, new SolutionItem(attr)); });
            item.Items = null;
            Items.Add(item);

            return item;
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

        private void SetValue(object value, string dataType)
        {
            Value = Attribute.SetValue(value, dataType);
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
            return this.item.AttributeSchema.IsValidDataTye(value);
        }

        public void SetProperty(object value)
        {
            this.item.Value = Attribute.SetValue(value, DataType);
        }
    }
}