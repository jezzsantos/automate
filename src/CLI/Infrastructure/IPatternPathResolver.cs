using automate.Domain;

namespace automate.Infrastructure
{
    internal interface IPatternPathResolver
    {
        IPatternElement Resolve(PatternDefinition pattern, string expression);
    }
}