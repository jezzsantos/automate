using Automate.Authoring.Domain;
using Automate.Common.Domain;

namespace Automate.Authoring.Application
{
    public interface IPatternPathResolver
    {
        IPatternElement Resolve(PatternDefinition pattern, string expression);
    }
}