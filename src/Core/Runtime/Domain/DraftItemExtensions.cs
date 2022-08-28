using System.Collections.Generic;
using Automate.Authoring.Domain;
using Automate.Common.Domain;

namespace Automate.Runtime.Domain
{
    internal static class DraftItemExtensions
    {
        public static bool HasCardinalityOfAtLeastOne(this Element element)
        {
            return element.Cardinality is ElementCardinality.One or ElementCardinality.OneOrMany;
        }

        public static bool HasCardinalityOfAtMostOne(this Element element)
        {
            return element.Cardinality is ElementCardinality.One or ElementCardinality.ZeroOrOne;
        }

        public static bool HasCardinalityOfMany(this Element element)
        {
            return element.Cardinality is ElementCardinality.OneOrMany or ElementCardinality.ZeroOrMany;
        }

        public static void Add(this ValidationResults results, DraftItem draftItem, string message)
        {
            results.Add(new ValidationResult(new ValidationContext(draftItem.ConfigurationPath), message));
        }

        public static IReadOnlyList<ValidationResult> Validate(this IAttributeSchema attribute, DraftItem draftItem,
            object value)
        {
            return attribute.Validate(new ValidationContext(draftItem.ConfigurationPath), value);
        }
    }
}