using automate.Domain;

namespace automate.Infrastructure
{
    internal interface ISolutionStore
    {
        void Save(SolutionDefinition solution);
    }
}