using Semver;

namespace Automate.Common.Domain
{
    public interface IAssemblyMetadata
    {
        SemVersion RuntimeVersion { get; }

        string ProductName { get; }

        string InstallationPath { get; }
    }
}