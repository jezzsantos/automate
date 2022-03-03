using System.Collections.Generic;
using Automate.CLI.Domain;

namespace Automate.CLI.Application
{
    internal interface IToolkitStore
    {
        string Export(ToolkitDefinition toolkit);

        void Import(ToolkitDefinition toolkit);

        ToolkitDefinition GetCurrent();

        void ChangeCurrent(string id);

        ToolkitDefinition FindByName(string name);

        List<ToolkitDefinition> ListAll();

        void DestroyAll();
    }
}