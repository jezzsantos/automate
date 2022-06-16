using System.Collections.Generic;
using System.Linq;
using Automate.CLI.Extensions;

namespace Automate.CLI.Domain
{
    internal class PatternSchema : PatternElementSchema, IPatternSchema
    {
        public PatternSchema(PatternElement patternElement) : base(patternElement)
        {
        }

        public IAutomationSchema FindAutomationById(string id)
        {
            return Automation.Safe()
                .FirstOrDefault(auto => auto.Id.EqualsIgnoreCase(id));
        }
    }

    internal class PatternElementSchema : IPatternElementSchema
    {
        private readonly PatternElement patternElement;

        public PatternElementSchema(PatternElement patternElement)
        {
            this.patternElement = patternElement;
        }

        public string Id => this.patternElement.Id;

        public string Name => this.patternElement.Name;

        public IReadOnlyList<IAutomationSchema> Automation => this.patternElement.Automation.ToSchema();

        public IReadOnlyList<IAttributeSchema> Attributes => this.patternElement.Attributes.ToSchema();

        public IReadOnlyList<IElementSchema> Elements => this.patternElement.Elements.ToSchema();

        public ICodeTemplateSchema FindCodeTemplateById(string id)
        {
            return this.patternElement.CodeTemplates.Safe()
                .FirstOrDefault(auto => auto.Id.EqualsIgnoreCase(id))?.ToSchema();
        }
    }

    internal class ElementSchema : PatternElementSchema, IElementSchema
    {
        public ElementSchema(Element element) : base(element)
        {
            element.GuardAgainstNull(nameof(element));
            Element = element;
        }

        public virtual bool IsCollection => Element.IsCollection;

        public Element Element { get; }

        public virtual bool HasCardinalityOfAtLeastOne()
        {
            return Element.HasCardinalityOfAtLeastOne();
        }

        public virtual bool HasCardinalityOfAtMostOne()
        {
            return Element.HasCardinalityOfAtMostOne();
        }

        public virtual bool HasCardinalityOfMany()
        {
            return Element.HasCardinalityOfMany();
        }

        public bool ShouldAutoCreate()
        {
            if (IsCollection)
            {
                return Element.AutoCreate;
            }
            return Element.AutoCreate && Element.Cardinality is ElementCardinality.One;
        }
    }

    internal class CollectionItemSchema : ElementSchema
    {
        public CollectionItemSchema(Element element) : base(element)
        {
        }

        public override bool IsCollection => false;

        public override bool HasCardinalityOfAtLeastOne()
        {
            return true;
        }

        public override bool HasCardinalityOfAtMostOne()
        {
            return true;
        }

        public override bool HasCardinalityOfMany()
        {
            return false;
        }
    }

    internal class AttributeSchema : IAttributeSchema
    {
        public AttributeSchema(Attribute attribute)
        {
            attribute.GuardAgainstNull(nameof(attribute));
            Attribute = attribute;
        }

        public string DataType => Attribute.DataType;

        public string Name => Attribute.Name;

        public IReadOnlyList<string> Choices => Attribute.Choices;

        public string DefaultValue => Attribute.DefaultValue;

        public string Id => Attribute.Id;

        public Attribute Attribute { get; }

        public bool IsValidDataType(string value)
        {
            return Attribute.IsValidDataType(value);
        }

        public IReadOnlyList<ValidationResult> Validate(ValidationContext context, object value)
        {
            return Attribute.Validate(context, value);
        }
    }

    internal class AutomationSchema : IAutomationSchema
    {
        public AutomationSchema(Automation automation)
        {
            automation.GuardAgainstNull(nameof(automation));

            Automation = automation;
        }

        public string Name => Automation.Name;

        public string Id => Automation.Id;

        public Automation Automation { get; }

        public CommandExecutionResult Execute(DraftDefinition draft, DraftItem draftItem)
        {
            return Automation.Execute(draft, draftItem);
        }
    }

    internal class CodeTemplateSchema : ICodeTemplateSchema
    {
        private readonly CodeTemplate template;

        public CodeTemplateSchema(CodeTemplate template)
        {
            this.template = template;
        }

        public string Id => this.template.Id;
    }
}