using System.Collections.Generic;
using System.Linq;
using Automate.CLI.Extensions;

namespace Automate.CLI.Domain
{
    internal static class PatternElementExtensions
    {
        public static void AddAttributes(this PatternElement element, params Attribute[] attributes)
        {
            attributes.ToListSafe().ForEach(element.AddAttribute);
        }

        public static bool IsLaunchable(this Automation automation)
        {
            return automation.Type != AutomationType.Unknown
                   && automation.Type != AutomationType.TestingOnly
                   && automation.Type != AutomationType.CommandLaunchPoint;
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
            return elements.ToListSafe()
                .Select(ele => ele.ToSchema())
                .ToList();
        }

        public static IReadOnlyList<IAttributeSchema> ToSchema(this IReadOnlyList<Attribute> attributes)
        {
            return attributes.ToListSafe()
                .Select(attr => attr.ToSchema())
                .ToList();
        }

        public static IReadOnlyList<IAutomationSchema> ToSchema(this IReadOnlyList<Automation> automation)
        {
            return automation.ToListSafe()
                .Select(auto => auto.ToSchema())
                .ToList();
        }
    }
}