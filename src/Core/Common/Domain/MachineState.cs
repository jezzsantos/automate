namespace Automate.Common.Domain
{
    public class MachineState : IPersistable
    {
        public MachineState()
        {
        }

        private MachineState(PersistableProperties properties,
            IPersistableFactory factory)
        {
            InstallationId = properties.Rehydrate<string>(factory, nameof(InstallationId));
        }

        public string InstallationId { get; private set; } = null!;

        public PersistableProperties Dehydrate()
        {
            var properties = new PersistableProperties();
            properties.Dehydrate(nameof(InstallationId), InstallationId);

            return properties;
        }

        public static MachineState Rehydrate(PersistableProperties properties,
            IPersistableFactory factory)
        {
            return new MachineState(properties, factory);
        }

        public void SetInstallationId(string installationId)
        {
            InstallationId = installationId;
        }
    }
}