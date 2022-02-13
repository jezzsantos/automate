using automate.Domain;

namespace automate.Infrastructure
{
    internal interface ISolutionPathResolver
    {
        SolutionItem Resolve(SolutionDefinition solution, string expression);
    }
}