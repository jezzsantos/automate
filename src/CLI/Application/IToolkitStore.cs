using System.Collections.Generic;
using automate.Domain;

namespace automate.Application
{
    internal interface IToolkitStore
    {
        string Export(ToolkitDefinition toolkit);

        void Import(ToolkitDefinition toolkit);

        ToolkitDefinition GetCurrent();

        void ChangeCurrent(string id);

        ToolkitDefinition FindByName(string name);

        List<ToolkitDefinition> ListAll();
    }
}