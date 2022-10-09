using Automate.Common.Domain;
using Automate.Common.Extensions;
using Semver;

namespace Automate.Authoring.Domain
{
    public static class ToolkitDefinitionExtensions
    {
        public static ToolkitRuntimeVersionCompatibility GetCompatibility(this ToolkitDefinition toolkit,
            IAssemblyMetadata metadata)
        {
            if (toolkit.IsMachineAheadOfToolkit(metadata))
            {
                return ToolkitRuntimeVersionCompatibility.MachineAheadOfToolkit;
            }
            if (toolkit.IsToolkitAheadOfMachine(metadata))
            {
                return ToolkitRuntimeVersionCompatibility.ToolkitAheadOfMachine;
            }

            return ToolkitRuntimeVersionCompatibility.Compatible;
        }

        private static bool IsMachineAheadOfToolkit(this ToolkitDefinition toolkit, IAssemblyMetadata metadata)
        {
            toolkit.GuardAgainstNull(nameof(toolkit));
            metadata.GuardAgainstNull(nameof(metadata));

            var machineRuntimeVersion = SemVersion.Parse(metadata.RuntimeVersion, SemVersionStyles.Any);
            var toolkitRuntimeVersion = SemVersion.Parse(toolkit.RuntimeVersion, SemVersionStyles.Any);

            if (machineRuntimeVersion.IsPrerelease && toolkitRuntimeVersion.IsPrerelease)
            {
                return machineRuntimeVersion.Minor > toolkitRuntimeVersion.Minor;
            }
            if (!machineRuntimeVersion.IsPrerelease && toolkitRuntimeVersion.IsPrerelease)
            {
                return machineRuntimeVersion > toolkitRuntimeVersion.WithoutPrerelease();
            }
            if (machineRuntimeVersion.IsPrerelease && !toolkitRuntimeVersion.IsPrerelease)
            {
                return machineRuntimeVersion.WithoutPrerelease() > toolkitRuntimeVersion;
            }

            return machineRuntimeVersion.Major > toolkitRuntimeVersion.Major;
        }

        private static bool IsToolkitAheadOfMachine(this ToolkitDefinition toolkit, IAssemblyMetadata metadata)
        {
            toolkit.GuardAgainstNull(nameof(toolkit));
            metadata.GuardAgainstNull(nameof(metadata));

            var machineRuntimeVersion = SemVersion.Parse(metadata.RuntimeVersion, SemVersionStyles.Any);
            var toolkitRuntimeVersion = SemVersion.Parse(toolkit.RuntimeVersion, SemVersionStyles.Any);

            if (machineRuntimeVersion.IsPrerelease && toolkitRuntimeVersion.IsPrerelease)
            {
                return toolkitRuntimeVersion.Minor > machineRuntimeVersion.Minor;
            }
            if (!machineRuntimeVersion.IsPrerelease && toolkitRuntimeVersion.IsPrerelease)
            {
                return toolkitRuntimeVersion.WithoutPrerelease() > machineRuntimeVersion;
            }
            if (machineRuntimeVersion.IsPrerelease && !toolkitRuntimeVersion.IsPrerelease)
            {
                return toolkitRuntimeVersion > machineRuntimeVersion.WithoutPrerelease();
            }

            return toolkitRuntimeVersion.Major > machineRuntimeVersion.Major;
        }
    }
}