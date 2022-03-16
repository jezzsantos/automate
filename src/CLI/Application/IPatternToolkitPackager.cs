using Automate.CLI.Domain;

namespace Automate.CLI.Application
{
    internal interface IPatternToolkitPackager
    {
        ToolkitPackage Pack(PatternDefinition pattern, VersionInstruction instruction);

        ToolkitDefinition UnPack(IFile installer);
    }
}