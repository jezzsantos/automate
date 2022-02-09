using automate.Domain;

namespace automate.Infrastructure
{
    internal interface IToolkitStore
    {
        string Export(ToolkitDefinition toolkit);

        void Import(ToolkitDefinition toolkit);

        ToolkitDefinition GetCurrent();

        void ChangeCurrent(string id);

        ToolkitDefinition FindByName(string name);
    }
}