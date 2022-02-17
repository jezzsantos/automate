using System.Collections.Generic;
using Automate.CLI.Domain;

namespace Automate.CLI.Application
{
    internal interface ISolutionStore
    {
        void Save(SolutionDefinition solution);

        List<SolutionDefinition> ListAll();

        SolutionDefinition FindById(string id);
    }
}