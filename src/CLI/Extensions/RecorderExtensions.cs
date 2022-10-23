using System.Collections.Generic;
using Automate.Common;

namespace Automate.CLI.Extensions
{
    public static class RecorderExtensions
    {
        public static void MeasureInfo(this IRecorder recorder, bool collectUsage, string usageSession)
        {
            recorder.MeasureEvent("info", new Dictionary<string, string>
            {
                { "AllowCollection", collectUsage.ToString() },
                { "SessionId", usageSession }
            });
        }

        public static void MeasureListAll(this IRecorder recorder)
        {
            recorder.MeasureEvent("all-list");
        }
    }
}