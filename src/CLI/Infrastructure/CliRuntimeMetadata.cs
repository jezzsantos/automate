using System.Reflection;
using Automate.Common.Domain;

namespace Automate.CLI.Infrastructure
{
    public class CliRuntimeMetadata : IRuntimeMetadata
    {
        public string RuntimeVersion => Assembly.GetExecutingAssembly()
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()!
            .InformationalVersion;

        public string ProductName => Assembly.GetExecutingAssembly()
            .GetCustomAttribute<AssemblyProductAttribute>()!
            .Product;
    }
}