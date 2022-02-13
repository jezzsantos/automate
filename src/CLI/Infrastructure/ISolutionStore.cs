using System.Collections.Generic;
using automate.Domain;

namespace automate.Infrastructure
{
    internal interface ISolutionStore
    {
        void Save(SolutionDefinition solution);

        List<SolutionDefinition> ListAll();

        SolutionDefinition FindById(string id);
    }
}