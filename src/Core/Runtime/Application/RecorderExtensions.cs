using System.Collections.Generic;
using Automate.Authoring.Domain;
using Automate.Common;
using Automate.Common.Extensions;
using Automate.Runtime.Domain;

namespace Automate.Runtime.Application
{
    public static class RecorderExtensions
    {
        public static void MeasureToolkitInstalled(this IRecorder recorder, ToolkitDefinition toolkit)
        {
            recorder.MeasureEvent("toolkit.installed", new Dictionary<string, string>
            {
                { "ToolkitId", toolkit.Id.AnonymiseIdentifier() },
                { "RuntimeVersion", toolkit.RuntimeVersion }
            });
        }

        public static void MeasureToolkitsListed(this IRecorder recorder)
        {
            recorder.MeasureEvent("toolkits.listed");
        }

        public static void MeasureToolkitViewed(this IRecorder recorder, ToolkitDefinition toolkit)
        {
            recorder.MeasureEvent("toolkit.viewed", new Dictionary<string, string>
            {
                { "ToolkitId", toolkit.Id.AnonymiseIdentifier() },
                { "RuntimeVersion", toolkit.RuntimeVersion }
            });
        }

        public static void MeasureDraftsListed(this IRecorder recorder)
        {
            recorder.MeasureEvent("drafts.listed");
        }

        public static void MeasureDraftViewed(this IRecorder recorder, DraftDefinition draft)
        {
            recorder.MeasureEvent("draft.viewed", new Dictionary<string, string>
            {
                { "DraftId", draft.Id.AnonymiseIdentifier() },
                { "ToolkitId", draft.Toolkit.Id.AnonymiseIdentifier() },
                { "RuntimeVersion", draft.Toolkit.RuntimeVersion }
            });
        }

        public static void MeasureDraftCreated(this IRecorder recorder, DraftDefinition draft)
        {
            recorder.MeasureEvent("draft.created", new Dictionary<string, string>
            {
                { "DraftId", draft.Id.AnonymiseIdentifier() },
                { "ToolkitId", draft.Toolkit.Id.AnonymiseIdentifier() },
                { "RuntimeVersion", draft.Toolkit.RuntimeVersion }
            });
        }

        public static void MeasureDraftSwitched(this IRecorder recorder, DraftDefinition draft)
        {
            recorder.MeasureEvent("draft.switched", new Dictionary<string, string>
            {
                { "DraftId", draft.Id.AnonymiseIdentifier() },
                { "ToolkitId", draft.Toolkit.Id.AnonymiseIdentifier() },
                { "RuntimeVersion", draft.Toolkit.RuntimeVersion }
            });
        }

        public static void MeasureDraftConfigured(this IRecorder recorder, DraftDefinition draft, DraftItem target)
        {
            recorder.MeasureEvent("draft.configured", new Dictionary<string, string>
            {
                { "DraftId", draft.Id.AnonymiseIdentifier() },
                { "ToolkitId", draft.Toolkit.Id.AnonymiseIdentifier() },
                { "RuntimeVersion", draft.Toolkit.RuntimeVersion }
            });
        }

        public static void MeasureDraftElementReset(this IRecorder recorder, DraftDefinition draft, DraftItem target)
        {
            recorder.MeasureEvent("draft.element.reset", new Dictionary<string, string>
            {
                { "DraftId", draft.Id.AnonymiseIdentifier() },
                { "ToolkitId", draft.Toolkit.Id.AnonymiseIdentifier() },
                { "RuntimeVersion", draft.Toolkit.RuntimeVersion }
            });
        }

        public static void MeasureDraftCollectionCleared(this IRecorder recorder, DraftDefinition draft,
            DraftItem target)
        {
            recorder.MeasureEvent("draft.collection.cleared", new Dictionary<string, string>
            {
                { "DraftId", draft.Id.AnonymiseIdentifier() },
                { "ToolkitId", draft.Toolkit.Id.AnonymiseIdentifier() },
                { "RuntimeVersion", draft.Toolkit.RuntimeVersion }
            });
        }

        public static void MeasureDraftItemDeleted(this IRecorder recorder, DraftDefinition draft, DraftItem target)
        {
            recorder.MeasureEvent("draft.item.deleted", new Dictionary<string, string>
            {
                { "DraftId", draft.Id.AnonymiseIdentifier() },
                { "ToolkitId", draft.Toolkit.Id.AnonymiseIdentifier() },
                { "RuntimeVersion", draft.Toolkit.RuntimeVersion }
            });
        }

        public static void MeasureDraftValidated(this IRecorder recorder, DraftDefinition draft)
        {
            recorder.MeasureEvent("draft.validated", new Dictionary<string, string>
            {
                { "DraftId", draft.Id.AnonymiseIdentifier() },
                { "ToolkitId", draft.Toolkit.Id.AnonymiseIdentifier() },
                { "RuntimeVersion", draft.Toolkit.RuntimeVersion }
            });
        }

        public static void MeasureDraftUpgraded(this IRecorder recorder, DraftDefinition draft)
        {
            recorder.MeasureEvent("draft.upgraded", new Dictionary<string, string>
            {
                { "DraftId", draft.Id.AnonymiseIdentifier() },
                { "ToolkitId", draft.Toolkit.Id.AnonymiseIdentifier() },
                { "RuntimeVersion", draft.Toolkit.RuntimeVersion }
            });
        }

        public static void MeasureDraftDeleted(this IRecorder recorder, DraftDefinition draft)
        {
            recorder.MeasureEvent("draft.deleted", new Dictionary<string, string>
            {
                { "DraftId", draft.Id.AnonymiseIdentifier() },
                { "ToolkitId", draft.Toolkit.Id.AnonymiseIdentifier() },
                { "RuntimeVersion", draft.Toolkit.RuntimeVersion }
            });
        }

        public static void MeasureLaunchPointExecuted(this IRecorder recorder, DraftDefinition draft)
        {
            recorder.MeasureEvent("draft.launchpoint.executed", new Dictionary<string, string>
            {
                { "DraftId", draft.Id.AnonymiseIdentifier() },
                { "ToolkitId", draft.Toolkit.Id.AnonymiseIdentifier() },
                { "RuntimeVersion", draft.Toolkit.RuntimeVersion }
            });
        }
    }
}