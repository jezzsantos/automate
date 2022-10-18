namespace Automate.Common.Application
{
    public interface IMachineStore
    {
        string GetInstallationId();

        void SetInstallationId(string installationId);
    }
}