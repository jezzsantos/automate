using automate.Domain;

namespace automate.Infrastructure
{
    internal interface ILocalStateRepository
    {
        LocalState GetLocalState();

        void SaveLocalState(LocalState state);

        void DestroyAll();
    }
}