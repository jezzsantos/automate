using automate.Domain;

namespace automate.Infrastructure
{
    internal interface IToolkitStore
    {
        string Save(PatternToolkitDefinition toolkit);

        PatternToolkitDefinition GetCurrent();
    }
}