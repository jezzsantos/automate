using Automate.Authoring.Domain;
using Automate.Common.Domain;

namespace Automate.Authoring.Application
{
    public interface IPatternToolkitPackager
    {
        ToolkitPackage PackAndExport(IRuntimeMetadata metadata, PatternDefinition pattern,
            VersionInstruction instruction);

        ToolkitDefinition UnPack(IRuntimeMetadata metadata, IFile installer);
    }
}