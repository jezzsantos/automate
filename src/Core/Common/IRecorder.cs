using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Automate.Common
{
    public interface IRecorder : IMetricReporter, ICrashReporter
    {
        void Trace(LogLevel level, string messageTemplate, params object[] args);

        new void EnableReporting(string machineId, string sessionId);

        (string MachineId, string SessionId) GetReportingIds();
    }

    public interface IOperationReporter
    {
        void BeginOperation(string messageTemplate, params object[] args);

        void EndOperation(bool success, string messageTemplate, params object[] args);
    }

    public interface ICrashReporter
    {
        void Crash(CrashLevel level, Exception exception, string messageTemplate, params object[] args);

        void EnableReporting(string machineId, string sessionId);
    }

    public interface IMetricReporter : IOperationReporter
    {
        void Count(string eventName, Dictionary<string, string> context = null);

        void EnableReporting(string machineId, string sessionId);
    }

    public enum CrashLevel
    {
        Recoverable = 0,
        Fatal = 1
    }
}