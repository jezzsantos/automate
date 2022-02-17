using System.Collections.Generic;
using Automate.CLI.Domain;

namespace Automate.CLI.Infrastructure
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