using Automate.CLI.Domain;

namespace Automate.CLI.Infrastructure
{
    internal interface ILocalStateRepository
    {
        LocalState GetLocalState();

        void SaveLocalState(LocalState state);

        void DestroyAll();
    }
}