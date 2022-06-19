using Automate.Common.Domain;

namespace Automate.Common.Infrastructure
{
    public interface ILocalStateRepository
    {
        LocalState GetLocalState();

        void SaveLocalState(LocalState state);

        void DestroyAll();
    }
}