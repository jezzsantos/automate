using System.Collections.Generic;
using Automate.CLI.Domain;

namespace Automate.CLI.Application
{
    internal interface ISolutionStore
    {
        SolutionDefinition Create(ToolkitDefinition toolkit);

        void Save(SolutionDefinition solution);

        List<SolutionDefinition> ListAll();

        SolutionDefinition FindById(string id);

        void ChangeCurrent(string id);

        SolutionDefinition GetCurrent();

        void DestroyAll();
    }
}