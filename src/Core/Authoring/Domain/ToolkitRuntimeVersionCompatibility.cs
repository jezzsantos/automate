namespace Automate.Authoring.Domain
{
    public enum ToolkitRuntimeVersionCompatibility
    {
        Compatible = 0,
        RuntimeAheadOfToolkit = 1,
        ToolkitAheadOfRuntime = 2
    }

    public enum DraftToolkitVersionCompatibility
    {
        Compatible = 0,
        DraftAheadOfToolkit = 1,
        ToolkitAheadOfDraft = 2
    }
}