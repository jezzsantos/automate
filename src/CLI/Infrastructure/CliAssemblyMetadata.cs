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
                var assembly = Assembly.GetEntryAssembly();
                if (assembly.NotExists())
                {
                    throw new InvalidOperationException("Assembly cannot be determined");
                }
                var location = assembly.GetName().CodeBase;
                if (location.NotExists())
                {
                    throw new InvalidOperationException("Assembly is not running in from a location on disk");
                }

                var uri = new Uri(location);
                return new FileInfo(uri.AbsolutePath).Directory!.FullName;
            }
        }
    }
}