using automate.Domain;

namespace automate.Application
{
    internal interface IPatternToolkitPackager
    {
        PatternToolkitPackage Pack(PatternDefinition pattern, string versionInstruction);

        ToolkitDefinition UnPack(IFile installer);
    }
}