using Automate.Authoring.Domain;
using Automate.Common.Domain;

namespace Automate.Authoring.Application
{
    public interface IPatternToolkitPackager
    {
        ToolkitPackage PackAndExport(IAssemblyMetadata metadata, PatternDefinition pattern,
            VersionInstruction instruction);

        ToolkitDefinition UnPack(IAssemblyMetadata metadata, IFile installer);
    }
}