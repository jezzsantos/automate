using automate.Domain;

namespace automate.Infrastructure
{
    internal interface IToolkitStore
    {
        string Export(PatternToolkitDefinition toolkit);

        void Import(PatternToolkitDefinition toolkit);

        PatternToolkitDefinition GetCurrent();

        void ChangeCurrent(string id);
    }
}