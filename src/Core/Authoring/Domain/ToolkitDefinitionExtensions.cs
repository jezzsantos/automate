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
            if (toolkit.IsToolkitOutOfDate(metadata))
            {
                return ToolkitRuntimeVersionCompatibility.RuntimeAheadOfToolkit;
            }
            if (toolkit.IsRuntimeOutOfDate(metadata))
            {
                return ToolkitRuntimeVersionCompatibility.ToolkitAheadOfRuntime;
            }

            return ToolkitRuntimeVersionCompatibility.Compatible;
        }

        public static bool IsToolkitOutOfDate(this ToolkitDefinition toolkit, IAssemblyMetadata metadata)
        {
            toolkit.GuardAgainstNull(nameof(toolkit));
            metadata.GuardAgainstNull(nameof(metadata));

            var assemblyRuntimeVersion = SemVersion.Parse(metadata.RuntimeVersion, SemVersionStyles.Any);
            var toolkitRuntimeVersion = SemVersion.Parse(toolkit.RuntimeVersion, SemVersionStyles.Any);

            if (assemblyRuntimeVersion.IsPrerelease && toolkitRuntimeVersion.IsPrerelease)
            {
                return assemblyRuntimeVersion.Minor > toolkitRuntimeVersion.Minor;
            }
            if (!assemblyRuntimeVersion.IsPrerelease && toolkitRuntimeVersion.IsPrerelease)
            {
                return assemblyRuntimeVersion > toolkitRuntimeVersion.WithoutPrerelease();
            }
            if (assemblyRuntimeVersion.IsPrerelease && !toolkitRuntimeVersion.IsPrerelease)
            {
                return assemblyRuntimeVersion.WithoutPrerelease() > toolkitRuntimeVersion;
            }

            return assemblyRuntimeVersion.Major > toolkitRuntimeVersion.Major;
        }

        public static bool IsRuntimeOutOfDate(this ToolkitDefinition toolkit, IAssemblyMetadata metadata)
        {
            toolkit.GuardAgainstNull(nameof(toolkit));
            metadata.GuardAgainstNull(nameof(metadata));

            var assemblyRuntimeVersion = SemVersion.Parse(metadata.RuntimeVersion, SemVersionStyles.Any);
            var toolkitRuntimeVersion = SemVersion.Parse(toolkit.RuntimeVersion, SemVersionStyles.Any);

            if (assemblyRuntimeVersion.IsPrerelease && toolkitRuntimeVersion.IsPrerelease)
            {
                return assemblyRuntimeVersion.Minor < toolkitRuntimeVersion.Minor;
            }
            if (!assemblyRuntimeVersion.IsPrerelease && toolkitRuntimeVersion.IsPrerelease)
            {
                return assemblyRuntimeVersion < toolkitRuntimeVersion.WithoutPrerelease();
            }
            if (assemblyRuntimeVersion.IsPrerelease && !toolkitRuntimeVersion.IsPrerelease)
            {
                return assemblyRuntimeVersion.WithoutPrerelease() < toolkitRuntimeVersion;
            }

            return assemblyRuntimeVersion.Major < toolkitRuntimeVersion.Major;
        }
    }
}