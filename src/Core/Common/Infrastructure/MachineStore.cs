using Automate.Common.Application;
using Automate.Common.Extensions;

namespace Automate.Common.Infrastructure
{
    public class MachineStore : IMachineStore
    {
        private readonly IMachineRepository machineRepository;

        public MachineStore(IMachineRepository machineRepository)
        {
            machineRepository.GuardAgainstNull(nameof(machineRepository));
            this.machineRepository = machineRepository;
        }

        public string GetInstallationId()
        {
            var state = this.machineRepository.GetMachineState();

            return state.InstallationId;
        }

        public void SetInstallationId(string installationId)
        {
            var state = this.machineRepository.GetMachineState();
            state.SetInstallationId(installationId);

            this.machineRepository.SaveMachineState(state);
        }
    }
}