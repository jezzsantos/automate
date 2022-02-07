namespace automate
{
    internal interface IPatternToolkitPackager
    {
        PatternToolkitPackage Package(PatternMetaModel pattern, string versionInstruction);
    }
}