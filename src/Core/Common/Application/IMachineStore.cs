namespace Automate.Common.Application
{
    public interface IMachineStore
    {
        string GetOrCreateInstallationId();

        void SetInstallationId(string installationId);
    }
}