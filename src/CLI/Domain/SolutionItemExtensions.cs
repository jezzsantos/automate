namespace Automate.CLI.Domain
{
    internal static class SolutionItemExtensions
    {
        public static bool HasCardinalityOfAtLeastOne(this Element element)
        {
            return element.Cardinality == ElementCardinality.Single ||
                   element.Cardinality == ElementCardinality.OneOrMany;
        }

        public static bool HasCardinalityOfAtMostOne(this Element element)
        {
            return element.Cardinality == ElementCardinality.Single ||
                   element.Cardinality == ElementCardinality.ZeroOrOne;
        }
    }
}