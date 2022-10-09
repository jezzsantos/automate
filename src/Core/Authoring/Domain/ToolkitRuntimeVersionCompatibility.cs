namespace Automate.Authoring.Domain
{
    public enum ToolkitRuntimeVersionCompatibility
    {
        Compatible = 0,
        MachineAheadOfToolkit = 1,
        ToolkitAheadOfMachine = 2
    }

    public enum DraftToolkitVersionCompatibility
    {
        Compatible = 0,
        DraftAheadOfToolkit = 1,
        ToolkitAheadOfDraft = 2
    }
}