using Automate.Domain;

namespace Automate.Application
{
    public interface IPatternPathResolver
    {
        IPatternElement Resolve(PatternDefinition pattern, string expression);
    }
}