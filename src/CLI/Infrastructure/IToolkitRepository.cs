using automate.Domain;

namespace automate.Infrastructure
{
    internal interface IToolkitRepository
    {
        string ToolkitLocation { get; }

        string ExportToolkit(PatternToolkitDefinition toolkit);

        void ImportToolkit(PatternToolkitDefinition toolkit);

        PatternToolkitDefinition GetToolkit(string id);

        PatternToolkitDefinition FindToolkitById(string id);

        void DestroyAll();
    }
}