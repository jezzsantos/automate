using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Automate.Common
{
    public interface IRecorder : IMetricReporter, ICrashReporter
    {
        void Trace(LogLevel level, string messageTemplate, params object[] args);

        new void DisableUsageCollection();

        new void SetUserId(string id);

        string GetUserId();
    }

    public interface ICrashReporter
    {
        void Crash(CrashLevel level, Exception exception, string messageTemplate, params object[] args);

        void DisableUsageCollection();

        void SetUserId(string id);
    }

    public interface IMetricReporter
    {
        void Count(string eventName, Dictionary<string, string> context = null);

        void DisableUsageCollection();

        void SetUserId(string id);
    }

    public enum CrashLevel
    {
        Recoverable = 0,
        Fatal = 1
    }
}