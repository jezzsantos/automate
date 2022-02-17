using Automate.CLI.Domain;

namespace Automate.CLI.Application
{
    internal interface IPatternToolkitPackager
    {
        PatternToolkitPackage Pack(PatternDefinition pattern, string versionInstruction);

        ToolkitDefinition UnPack(IFile installer);
    }
}