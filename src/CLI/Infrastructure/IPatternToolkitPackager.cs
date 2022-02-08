using automate.Domain;

namespace automate.Infrastructure
{
    internal interface IPatternToolkitPackager
    {
        PatternToolkitPackage Package(PatternDefinition pattern, string versionInstruction);
    }
}