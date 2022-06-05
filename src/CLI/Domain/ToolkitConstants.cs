using System.Reflection;

namespace Automate.CLI.Domain
{
    internal static class ToolkitConstants
    {
        internal const string FirstVersionSupportingRuntimeVersion = "0.2.0-preview";

        internal static string GetRuntimeVersion()
        {
            return Assembly.GetExecutingAssembly()
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()!
                .InformationalVersion;
        }

        internal static string GetRuntimeProductName()
        {
            return Assembly.GetExecutingAssembly()
                .GetCustomAttribute<AssemblyProductAttribute>()!
                .Product;
        }
    }
}