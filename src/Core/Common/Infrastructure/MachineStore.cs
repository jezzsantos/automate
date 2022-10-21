using System;
using Automate.Common.Application;
using Automate.Common.Extensions;

namespace Automate.Common.Infrastructure
{
    public class MachineStore : IMachineStore
    {
        private readonly Func<string> machineIdFactory;
        private readonly IMachineRepository machineRepository;

        public MachineStore(IMachineRepository machineRepository) : this(machineRepository, CreateMachineId)
        {
        }

        internal MachineStore(IMachineRepository machineRepository, Func<string> machineIdFactory)
        {
            machineRepository.GuardAgainstNull(nameof(machineRepository));
            machineIdFactory.GuardAgainstNull(nameof(machineIdFactory));
            this.machineRepository = machineRepository;
            this.machineIdFactory = machineIdFactory;
        }

        public string GetOrCreateInstallationId()
        {
            var state = this.machineRepository.GetMachineState();

            var installationId = state.InstallationId;
            if (installationId.HasValue())
            {
                return installationId;
            }

            var created = this.machineIdFactory.Invoke();
            SetInstallationId(created);

            return created;
        }

        public void SetInstallationId(string installationId)
        {
            var state = this.machineRepository.GetMachineState();
            state.SetInstallationId(installationId);

            this.machineRepository.SaveMachineState(state);
        }

        private static string CreateMachineId()
        {
            return $"mid_{Guid.NewGuid():N}";
        }
    }
}