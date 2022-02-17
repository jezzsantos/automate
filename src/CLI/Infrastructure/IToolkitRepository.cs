using System.Collections.Generic;
using Automate.CLI.Domain;

namespace Automate.CLI.Infrastructure
{
    internal interface IToolkitRepository
    {
        string ToolkitLocation { get; }

        string ExportToolkit(ToolkitDefinition toolkit);

        void ImportToolkit(ToolkitDefinition toolkit);

        ToolkitDefinition GetToolkit(string id);

        ToolkitDefinition FindToolkitById(string id);

        ToolkitDefinition FindToolkitByName(string name);

        List<ToolkitDefinition> ListToolkits();

        void DestroyAll();
    }
}