using System.Collections.Generic;
using Automate.Authoring.Domain;
using Automate.Common;
using Automate.Common.Extensions;
using Automate.Runtime.Domain;

namespace Automate.Runtime.Application
{
    public static class RecorderExtensions
    {
        public static void CountDraftCreated(this IRecorder recorder, DraftDefinition draft)
        {
            recorder.Count("pattern.created", new Dictionary<string, string>
            {
                { "RuntimeVersion", draft.Toolkit.RuntimeVersion },
                { "ToolkitId", draft.Toolkit.Id.AnonymiseMeasure() },
                { "Id", draft.Id.AnonymiseMeasure() }
            });
        }

        public static void CountToolkitInstalled(this IRecorder recorder, ToolkitDefinition toolkit)
        {
            recorder.Count("pattern.created", new Dictionary<string, string>
            {
                { "RuntimeVersion", toolkit.RuntimeVersion },
                { "Id", toolkit.Id.AnonymiseMeasure() }
            });
        }
    }
}