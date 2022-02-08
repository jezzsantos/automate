using automate.Domain;

namespace automate.Infrastructure
{
    internal interface IPatternToolkitPackager
    {
        PatternToolkitPackage Pack(PatternDefinition pattern, string versionInstruction);

        PatternToolkitDefinition UnPack(IFile installer);
    }
}