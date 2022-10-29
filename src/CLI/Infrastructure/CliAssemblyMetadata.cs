using System;
using System.IO;
using System.Reflection;
using Automate.Common.Domain;
using Automate.Common.Extensions;
using Semver;

namespace Automate.CLI.Infrastructure
{
    public class CliAssemblyMetadata : IAssemblyMetadata
    {
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
    }
}