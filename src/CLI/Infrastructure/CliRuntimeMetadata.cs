using System;
using System.IO;
using System.Reflection;
using Automate.Common.Domain;
using Automate.Common.Extensions;
using Semver;

namespace Automate.CLI.Infrastructure
{
    public class CliRuntimeMetadata : IRuntimeMetadata
    {
        internal const string LocalSubDirectoryDataPath = "automate";
        internal static readonly string DotNetToolsInstallationPath =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".dotnet/tools");

        public SemVersion RuntimeVersion
        {
            get
            {
                var version = Assembly.GetExecutingAssembly()
                    .GetCustomAttribute<AssemblyInformationalVersionAttribute>()!
                    .InformationalVersion;
                return version.HasNoValue()
                    ? null
                    : version.ToSemVersion();
            }
        }

        public string ProductName => Assembly.GetExecutingAssembly()
            .GetCustomAttribute<AssemblyProductAttribute>()!
            .Product;

        public string InstallationPath
        {
            get
            {
                var location = AppContext.BaseDirectory;
                if (location.HasNoValue())
                {
                    throw new InvalidOperationException(ExceptionMessages.CliAssemblyMetadata_InstallationPathNotExist);
                }

                var uri = new Uri(location);
                return new FileInfo(uri.AbsolutePath).Directory!.FullName;
            }
        }

        public string CurrentExecutionPath => Environment.CurrentDirectory;

        public string UserDataPath =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                LocalSubDirectoryDataPath);

        public string LocalStateDataPath => CalculateLocalStatePath(CurrentExecutionPath);

        internal static string CalculateLocalStatePath(string currentDirectory)
        {
            var currentDirectoryPath = Path.GetFullPath(currentDirectory);
            var dotnetToolsPath = Path.GetFullPath(DotNetToolsInstallationPath);

            return currentDirectoryPath.EqualsIgnoreCase(dotnetToolsPath)
                ? Path.Combine(dotnetToolsPath, $"_{LocalSubDirectoryDataPath}")
                : Path.Combine(currentDirectoryPath, LocalSubDirectoryDataPath);
        }
    }
}