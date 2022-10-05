using Automate.Authoring.Domain;
using Automate.Common.Extensions;
using Semver;

namespace Automate.Runtime.Domain
{
    public static class DraftDefinitionExtensions
    {
        public static DraftToolkitVersionCompatibility GetCompatibility(this DraftDefinition draft,
            ToolkitDefinition installedToolkit)
        {
            draft.GuardAgainstNull(nameof(draft));
            installedToolkit.GuardAgainstNull(nameof(installedToolkit));

            var draftToolkitVersion = SemVersion.Parse(draft.Toolkit.Version, SemVersionStyles.Any);
            var installedToolkitVersion = SemVersion.Parse(installedToolkit.Version, SemVersionStyles.Any);
            if (draftToolkitVersion > installedToolkitVersion)
            {
                return DraftToolkitVersionCompatibility.DraftAheadOfToolkit;
            }
            if (installedToolkitVersion > draftToolkitVersion)
            {
                return DraftToolkitVersionCompatibility.ToolkitAheadOfDraft;
            }

            return DraftToolkitVersionCompatibility.Compatible;
        }
    }
}