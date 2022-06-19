using System.Collections.Generic;
using System.Linq;
using Automate.Common.Extensions;

namespace Automate.Authoring.Domain
{
    internal static class PatternElementExtensions
    {
        public static void AddAttributes(this PatternElement element, params Attribute[] attributes)
        {
            attributes.ToListSafe().ForEach(element.AddAttribute);
        }

        public static IPatternSchema ToSchema(this PatternDefinition pattern)
        {
            return new PatternSchema(pattern);
        }

        public static IElementSchema ToSchema(this Element element)
        {
            return new ElementSchema(element);
        }

        public static IAttributeSchema ToSchema(this Attribute attribute)
        {
            return new AttributeSchema(attribute);
        }

        public static IAutomationSchema ToSchema(this Automation automation)
        {
            return new AutomationSchema(automation);
        }

        public static ICodeTemplateSchema ToSchema(this CodeTemplate template)
        {
            return new CodeTemplateSchema(template);
        }

        public static IReadOnlyList<IElementSchema> ToSchema(this IReadOnlyList<Element> elements)
        {
            return elements.ToListSafe().Select(ToSchema)
                .ToList();
        }

        public static IReadOnlyList<IAttributeSchema> ToSchema(this IReadOnlyList<Attribute> attributes)
        {
            return attributes.ToListSafe().Select(ToSchema)
                .ToList();
        }

        public static IReadOnlyList<IAutomationSchema> ToSchema(this IReadOnlyList<Automation> automation)
        {
            return automation.ToListSafe().Select(ToSchema)
                .ToList();
        }
    }
}