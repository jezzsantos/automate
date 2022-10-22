using System.Collections.Generic;
using Automate.Common;

namespace Automate.CLI.Extensions
{
    public static class RecorderExtensions
    {
        public static void CountInfo(this IRecorder recorder, bool collectUsage, string usageSession)
        {
            recorder.Count("info", new Dictionary<string, string>
            {
                { "AllowCollection", collectUsage.ToString() },
                { "SessionId", usageSession }
            });
        }

        public static void CountUsage(this IRecorder recorder)
        {
            recorder.Count("use");
        }

        public static void CountUsageException(this IRecorder recorder, string errorMessage)
        {
            recorder.Count("exception", new Dictionary<string, string>
            {
                { "Error", errorMessage }
            });
        }

        public static void CountAll(this IRecorder recorder)
        {
            recorder.Count("all-list");
        }
    }
}