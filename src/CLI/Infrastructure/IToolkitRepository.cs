using automate.Domain;

namespace automate.Infrastructure
{
    internal interface IToolkitRepository
    {
        string ToolkitLocation { get; }

        string ExportToolkit(ToolkitDefinition toolkit);

        void ImportToolkit(ToolkitDefinition toolkit);

        ToolkitDefinition GetToolkit(string id);

        ToolkitDefinition FindToolkitById(string id);

        ToolkitDefinition FindToolkitByName(string name);

        void DestroyAll();
    }
}