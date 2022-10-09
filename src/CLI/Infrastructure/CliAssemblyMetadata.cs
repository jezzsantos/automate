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
    }
}