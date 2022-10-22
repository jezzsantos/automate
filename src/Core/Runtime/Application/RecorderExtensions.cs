using System.Collections.Generic;
using Automate.Authoring.Domain;
using Automate.Common;
using Automate.Common.Extensions;
using Automate.Runtime.Domain;

namespace Automate.Runtime.Application
{
    public static class RecorderExtensions
    {
        public static void CountToolkitInstalled(this IRecorder recorder, ToolkitDefinition toolkit)
        {
            recorder.Count("toolkit.installed", new Dictionary<string, string>
            {
                { "ToolkitId", toolkit.Id.AnonymiseIdentifier() },
                { "RuntimeVersion", toolkit.RuntimeVersion }
            });
        }

        public static void CountToolkitsListed(this IRecorder recorder)
        {
            recorder.Count("toolkits.listed");
        }

        public static void CountToolkitViewed(this IRecorder recorder, ToolkitDefinition toolkit)
        {
            recorder.Count("toolkit.viewed", new Dictionary<string, string>
            {
                { "ToolkitId", toolkit.Id.AnonymiseIdentifier() },
                { "RuntimeVersion", toolkit.RuntimeVersion }
            });
        }

        public static void CountDraftsListed(this IRecorder recorder)
        {
            recorder.Count("drafts.listed");
        }

        public static void CountDraftViewed(this IRecorder recorder, DraftDefinition draft)
        {
            recorder.Count("draft.viewed", new Dictionary<string, string>
            {
                { "DraftId", draft.Id.AnonymiseIdentifier() },
                { "ToolkitId", draft.Toolkit.Id.AnonymiseIdentifier() },
                { "RuntimeVersion", draft.Toolkit.RuntimeVersion }
            });
        }

        public static void CountDraftCreated(this IRecorder recorder, DraftDefinition draft)
        {
            recorder.Count("draft.created", new Dictionary<string, string>
            {
                { "DraftId", draft.Id.AnonymiseIdentifier() },
                { "ToolkitId", draft.Toolkit.Id.AnonymiseIdentifier() },
                { "RuntimeVersion", draft.Toolkit.RuntimeVersion }
            });
        }

        public static void CountDraftSwitched(this IRecorder recorder, DraftDefinition draft)
        {
            recorder.Count("draft.switched", new Dictionary<string, string>
            {
                { "DraftId", draft.Id.AnonymiseIdentifier() },
                { "ToolkitId", draft.Toolkit.Id.AnonymiseIdentifier() },
                { "RuntimeVersion", draft.Toolkit.RuntimeVersion }
            });
        }

        public static void CountDraftConfigured(this IRecorder recorder, DraftDefinition draft, DraftItem target)
        {
            recorder.Count("draft.configured", new Dictionary<string, string>
            {
                { "DraftId", draft.Id.AnonymiseIdentifier() },
                { "ToolkitId", draft.Toolkit.Id.AnonymiseIdentifier() },
                { "RuntimeVersion", draft.Toolkit.RuntimeVersion }
            });
        }

        public static void CountDraftElementReset(this IRecorder recorder, DraftDefinition draft, DraftItem target)
        {
            recorder.Count("draft.element.reset", new Dictionary<string, string>
            {
                { "DraftId", draft.Id.AnonymiseIdentifier() },
                { "ToolkitId", draft.Toolkit.Id.AnonymiseIdentifier() },
                { "RuntimeVersion", draft.Toolkit.RuntimeVersion }
            });
        }

        public static void CountDraftCollectionCleared(this IRecorder recorder, DraftDefinition draft, DraftItem target)
        {
            recorder.Count("draft.collection.cleared", new Dictionary<string, string>
            {
                { "DraftId", draft.Id.AnonymiseIdentifier() },
                { "ToolkitId", draft.Toolkit.Id.AnonymiseIdentifier() },
                { "RuntimeVersion", draft.Toolkit.RuntimeVersion }
            });
        }

        public static void CountDraftItemDeleted(this IRecorder recorder, DraftDefinition draft, DraftItem target)
        {
            recorder.Count("draft.item.deleted", new Dictionary<string, string>
            {
                { "DraftId", draft.Id.AnonymiseIdentifier() },
                { "ToolkitId", draft.Toolkit.Id.AnonymiseIdentifier() },
                { "RuntimeVersion", draft.Toolkit.RuntimeVersion }
            });
        }

        public static void CountDraftValidated(this IRecorder recorder, DraftDefinition draft)
        {
            recorder.Count("draft.validated", new Dictionary<string, string>
            {
                { "DraftId", draft.Id.AnonymiseIdentifier() },
                { "ToolkitId", draft.Toolkit.Id.AnonymiseIdentifier() },
                { "RuntimeVersion", draft.Toolkit.RuntimeVersion }
            });
        }

        public static void CountDraftUpgraded(this IRecorder recorder, DraftDefinition draft)
        {
            recorder.Count("draft.upgraded", new Dictionary<string, string>
            {
                { "DraftId", draft.Id.AnonymiseIdentifier() },
                { "ToolkitId", draft.Toolkit.Id.AnonymiseIdentifier() },
                { "RuntimeVersion", draft.Toolkit.RuntimeVersion }
            });
        }

        public static void CountDraftDeleted(this IRecorder recorder, DraftDefinition draft)
        {
            recorder.Count("draft.deleted", new Dictionary<string, string>
            {
                { "DraftId", draft.Id.AnonymiseIdentifier() },
                { "ToolkitId", draft.Toolkit.Id.AnonymiseIdentifier() },
                { "RuntimeVersion", draft.Toolkit.RuntimeVersion }
            });
        }

        public static void CountLaunchPointExecuted(this IRecorder recorder, DraftDefinition draft)
        {
            recorder.Count("draft.launchpoint.executed", new Dictionary<string, string>
            {
                { "DraftId", draft.Id.AnonymiseIdentifier() },
                { "ToolkitId", draft.Toolkit.Id.AnonymiseIdentifier() },
                { "RuntimeVersion", draft.Toolkit.RuntimeVersion }
            });
        }
    }
}