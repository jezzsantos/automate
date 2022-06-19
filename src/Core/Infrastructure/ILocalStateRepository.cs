using Automate.Domain;

namespace Automate.Infrastructure
{
    public interface ILocalStateRepository
    {
        LocalState GetLocalState();

        void SaveLocalState(LocalState state);

        void DestroyAll();
    }
}