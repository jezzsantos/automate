using Automate.Authoring.Domain;
using Automate.Common.Extensions;

namespace Automate.Runtime.Domain
{
    public static class DraftDefinitionExtensions
    {
        public static DraftToolkitVersionCompatibility GetCompatibility(this DraftDefinition draft,
            ToolkitDefinition installedToolkit)
        {
            draft.GuardAgainstNull(nameof(draft));
            installedToolkit.GuardAgainstNull(nameof(installedToolkit));

            var draftToolkitVersion = draft.Toolkit.Version.ToSemVersion();
            var installedToolkitVersion = installedToolkit.Version.ToSemVersion();
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