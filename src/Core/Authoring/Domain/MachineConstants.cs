using System.Reflection;
using Automate.Common.Extensions;
using Semver;

namespace Automate.Authoring.Domain
{
    internal static class MachineConstants
    {
        internal const string FirstVersionSupportingRuntimeVersion = "0.2.0-preview";

        internal static SemVersion GetRuntimeVersion()
        {
            return Assembly.GetExecutingAssembly()
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()!
                .InformationalVersion.ToSemVersion();
        }
    }
}