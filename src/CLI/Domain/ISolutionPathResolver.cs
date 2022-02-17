namespace Automate.CLI.Domain
{
    internal interface ISolutionPathResolver
    {
        SolutionItem ResolveItem(SolutionDefinition solution, string expression);

        string ResolveExpression(string expression, SolutionItem solutionItem);
    }
}