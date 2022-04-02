using System.Collections.Generic;

namespace Automate.CLI.Domain
{
    internal static class SolutionItemExtensions
    {
        public static bool HasCardinalityOfAtLeastOne(this Element element)
        {
            return element.Cardinality is ElementCardinality.Single or ElementCardinality.OneOrMany;
        }

        public static bool HasCardinalityOfAtMostOne(this Element element)
        {
            return element.Cardinality is ElementCardinality.Single or ElementCardinality.ZeroOrOne;
        }

        public static bool HasCardinalityOfMany(this Element element)
        {
            return element.Cardinality is ElementCardinality.OneOrMany or ElementCardinality.ZeroOrMany;
        }

        public static void Add(this ValidationResults results, SolutionItem solutionItem, string message)
        {
            results.Add(new ValidationResult(new ValidationContext(solutionItem.PathReference), message));
        }

        public static IReadOnlyList<ValidationResult> Validate(this IAttributeSchema attribute, SolutionItem solutionItem, object value)
        {
            return attribute.Validate(new ValidationContext(solutionItem.PathReference), value);
        }

    }
}