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
    }
}