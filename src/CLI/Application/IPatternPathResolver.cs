using Automate.CLI.Domain;

namespace Automate.CLI.Application
{
    internal interface IPatternPathResolver
    {
        IPatternElement Resolve(PatternDefinition pattern, string expression);
    }
}