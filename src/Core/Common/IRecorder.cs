using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Automate.Common
{
    public interface IRecorder : IMetricReporter, ICrashReporter
    {
        void Trace(LogLevel level, string messageTemplate, params object[] args);
    }

    public interface ICrashReporter
    {
        void Crash(CrashLevel level, Exception exception, string messageTemplate, params object[] args);
    }

    public interface IMetricReporter
    {
        void Measure(string eventName, Dictionary<string, string> context = null);
    }

    public enum CrashLevel
    {
        Recoverable = 0,
        Fatal = 1
    }
}