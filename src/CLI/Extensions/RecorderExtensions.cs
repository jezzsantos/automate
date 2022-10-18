using System.Collections.Generic;
using Automate.Common;

namespace Automate.CLI.Extensions
{
    public static class RecorderExtensions
    {
        public static void CountUsage(this IRecorder recorder)
        {
            recorder.Count("use");
        }

        public static void CountUsageException(this IRecorder recorder, string errorMessage)
        {
            recorder.Count("use", new Dictionary<string, string>
            {
                { "Error", errorMessage }
            });
        }
    }
}