using System.Collections.Generic;
using automate.Domain;

namespace automate.Infrastructure
{
    internal interface ISolutionRepository
    {
        string SolutionLocation { get; }

        void NewSolution(SolutionDefinition solution);

        void UpsertSolution(SolutionDefinition solution);

        SolutionDefinition GetSolution(string id);

        SolutionDefinition FindSolutionById(string id);

        void DestroyAll();

        List<SolutionDefinition> ListSolutions();
    }
}