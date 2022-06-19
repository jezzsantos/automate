using System.Reflection;

namespace Automate.Domain
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
    }
}