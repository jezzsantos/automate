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
            Object = element;
        }

        public virtual bool IsCollection => Object.IsCollection;

        public Element Object { get; }

        public virtual bool HasCardinalityOfAtLeastOne()
        {
            return Object.HasCardinalityOfAtLeastOne();
        }

        public virtual bool HasCardinalityOfAtMostOne()
        {
            return Object.HasCardinalityOfAtMostOne();
        }

        public virtual bool HasCardinalityOfMany()
        {
            return Object.HasCardinalityOfMany();
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
            Object = attribute;
        }

        public string DataType => Object.DataType;

        public string Name => Object.Name;

        public IReadOnlyList<string> Choices => Object.Choices;

        public string DefaultValue => Object.DefaultValue;

        public string Id => Object.Id;

        public Attribute Object { get; }

        public bool IsValidDataType(string value)
        {
            return Object.IsValidDataType(value);
        }

        public IReadOnlyList<ValidationResult> Validate(ValidationContext context, object value)
        {
            return Object.Validate(context, value);
        }
    }

    internal class AutomationSchema : IAutomationSchema
    {
        public AutomationSchema(Automation automation)
        {
            automation.GuardAgainstNull(nameof(automation));

            Object = automation;
        }

        public string Name => Object.Name;

        public string Id => Object.Id;

        public Automation Object { get; }

        public CommandExecutionResult Execute(SolutionDefinition solution, SolutionItem solutionItem)
        {
            return Object.Execute(solution, solutionItem);
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