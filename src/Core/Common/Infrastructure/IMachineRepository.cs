using Automate.Common.Domain;

namespace Automate.Common.Infrastructure
{
    public interface IMachineRepository
    {
        MachineState GetMachineState();

        void SaveMachineState(MachineState state);

        void DestroyAll();
    }
}