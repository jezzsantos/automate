using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Automate.Common
{
    public interface IRecorder
    {
        void Trace(LogLevel level, string messageTemplate, params object[] args);

        void EnableReporting(string machineId, string sessionId);

        (string MachineId, string SessionId) GetReportingIds();

        void StartSession(string messageTemplate, params object[] args);

        void EndSession(bool success, string messageTemplate, params object[] args);

        void MeasureEvent(string eventName, Dictionary<string, string> context = null);

        void Crash(CrashLevel level, Exception exception, string messageTemplate, params object[] args);
    }

    public interface ICrashReporter
    {
        void EnableReporting(string machineId, string sessionId);

        void Crash(CrashLevel level, Exception exception, string messageTemplate, params object[] args);
    }

    public interface IMeasurementReporter
    {
        void EnableReporting(string machineId, string sessionId);

        void MeasureEvent(string eventName, Dictionary<string, string> context = null);

        void MeasureStartSession(string messageTemplate, params object[] args);

        void MeasureEndSession(bool success, string messageTemplate, params object[] args);
    }

    public enum CrashLevel
    {
        Recoverable = 0,
        Fatal = 1
    }
}