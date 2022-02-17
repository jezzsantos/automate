using automate.Domain;

namespace automate.Application
{
    internal interface IPatternPathResolver
    {
        IPatternElement Resolve(PatternDefinition pattern, string expression);
    }
}