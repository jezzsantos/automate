using System.Collections.Generic;
using Automate.CLI.Domain;

namespace Automate.CLI.Application
{
    internal interface IToolkitStore
    {
        string Export(ToolkitDefinition toolkit);

        void Import(ToolkitDefinition toolkit);

        ToolkitDefinition GetCurrent();

        ToolkitDefinition ChangeCurrent(string id);

        ToolkitDefinition FindByName(string name);

        ToolkitDefinition FindById(string id);

        List<ToolkitDefinition> ListAll();

        void DestroyAll();
    }
}