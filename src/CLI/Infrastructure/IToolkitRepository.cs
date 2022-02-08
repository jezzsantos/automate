using automate.Domain;

namespace automate.Infrastructure
{
    internal interface IToolkitRepository
    {
        string ToolkitLocation { get; }

        string SaveToolkit(PatternToolkitDefinition toolkit);

        PatternToolkitDefinition GetToolkit(string id);

        void DestroyAll();
    }
}