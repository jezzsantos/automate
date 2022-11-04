using System.Collections.Generic;
using Automate.Authoring.Domain;

namespace Automate.Authoring.Infrastructure
{
    public interface IToolkitRepository
    {
        string ToolkitsLocation { get; }

        string ExportToolkit(ToolkitDefinition toolkit);

        void ImportToolkit(ToolkitDefinition toolkit);

        ToolkitDefinition GetToolkit(string id);

        ToolkitDefinition FindToolkitById(string id);

        ToolkitDefinition FindToolkitByName(string name);

        List<ToolkitDefinition> ListToolkits();

        void DestroyAll();
    }
}