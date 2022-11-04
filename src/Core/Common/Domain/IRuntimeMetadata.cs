using Semver;

namespace Automate.Common.Domain
{
    public interface IRuntimeMetadata
    {
        SemVersion RuntimeVersion { get; }

        string ProductName { get; }

        string InstallationPath { get; }

        string CurrentExecutionPath { get; }

        string UserDataPath { get; }

        string LocalStateDataPath { get; }
    }
}