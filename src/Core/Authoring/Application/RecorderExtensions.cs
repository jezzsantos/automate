using System.Collections.Generic;
using Automate.Authoring.Domain;
using Automate.Common;
using Automate.Common.Extensions;

namespace Automate.Authoring.Application
{
    public static class RecorderExtensions
    {
        public static void CountPatternCreated(this IRecorder recorder, PatternDefinition pattern)
        {
            recorder.Count("pattern.created", new Dictionary<string, string>
            {
                { "RuntimeVersion", pattern.RuntimeVersion },
                { "Id", pattern.Id.AnonymiseMeasure() }
            });
        }
    }
}