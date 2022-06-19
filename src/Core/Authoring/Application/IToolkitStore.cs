using System.Collections.Generic;
using Automate.Authoring.Domain;

namespace Automate.Authoring.Application
{
    public interface IToolkitStore
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