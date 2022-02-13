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

            Properties = new Dictionary<string, SolutionItem>
            {
                { nameof(PatternDefinition.DisplayName), new SolutionItem(pattern.DisplayName) },
                { nameof(PatternDefinition.Description), new SolutionItem(pattern.Description) }
            };
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
            Value = attribute.DefaultValue;
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

        public SolutionItem(string value)
        {
            Name = null;
            PatternSchema = null;
            AttributeSchema = null;
            ElementSchema = null;
            IsMaterialised = true;
            Value = value;
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

        public object Name { get; set; }

        public bool IsMaterialised { get; set; }

        public SolutionItem Materialise(object value = null)
        {
            if (PatternSchema.Exists())
            {
                throw new AutomateException(
                    ExceptionMessages.SolutionItem_PatternAlreadyMaterialised.Format(PatternSchema.Name));
            }

            if (ElementSchema.Exists())
            {
                Properties = new Dictionary<string, SolutionItem>
                {
                    { nameof(Element.DisplayName), new SolutionItem(ElementSchema.DisplayName) },
                    { nameof(Element.Description), new SolutionItem(ElementSchema.Description) }
                };
                if (!ElementSchema.IsCollection)
                {
                    ElementSchema.Attributes.ForEach(
                        attribute => { Properties.Add(attribute.Name, new SolutionItem(attribute)); });
                }
                ElementSchema.Elements.ForEach(ele => { Properties.Add(ele.Name, new SolutionItem(ele)); });
                Items = ElementSchema.IsCollection
                    ? new List<SolutionItem>()
                    : null;
                IsMaterialised = true;
            }

            if (AttributeSchema.Exists())
            {
                Value = value;
                IsMaterialised = true;
            }

            if (PatternSchema.NotExists()
                && ElementSchema.NotExists()
                && AttributeSchema.NotExists())
            {
                Value = value;
                IsMaterialised = true;
            }

            return this;
        }

        public SolutionItem MaterialiseCollectionItem()
        {
            if (ElementSchema.NotExists()
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
            item.Items = null;
            Items.Add(item);

            return item;
        }

        public bool HasAttribute(string name)
        {
            if (PatternSchema.Exists())
            {
                return PatternSchema.Attributes.Any(attr => attr.Name.EqualsIgnoreCase(name));
            }
            if (ElementSchema.Exists())
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

        public void SetProperty(string value)
        {
            this.item.Value = value;
        }
    }
}