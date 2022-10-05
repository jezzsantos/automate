using Automate.Authoring.Domain;
using Automate.Common.Domain;

namespace Automate.Authoring.Application
{
    public interface IPatternToolkitPackager
    {
        ToolkitPackage PackAndExport(PatternDefinition pattern, VersionInstruction instruction);

        ToolkitDefinition UnPack(IAssemblyMetadata metadata, IFile installer);
    }
}