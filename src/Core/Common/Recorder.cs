using System;
using System.Collections.Generic;
using Automate.Common.Extensions;
using Microsoft.Extensions.Logging;

namespace Automate.Common
{
    public class Recorder : IRecorder, IDisposable
    {
        private readonly ICrashReporter crasher;
        private readonly ILogger logger;
        private readonly IMetricReporter measurer;
        private bool isReportingEnabled;
        private string machineId;
        private string sessionId;

        public Recorder(ILogger logger, ICrashReporter crasher, IMetricReporter measurer)
        {
            logger.GuardAgainstNull(nameof(logger));
            crasher.GuardAgainstNull(nameof(crasher));
            measurer.GuardAgainstNull(nameof(measurer));
            this.logger = logger;
            this.crasher = crasher;
            this.measurer = measurer;
            this.isReportingEnabled = false;
            this.machineId = null;
            this.sessionId = null;
        }

        public static string CreateSessionId()
        {
            return $"cli_{Guid.NewGuid():N}";
        }

        public void Dispose()
        {
            (this.crasher as IDisposable)?.Dispose();
            (this.measurer as IDisposable)?.Dispose();
        }

        public void EnableReporting(string machineId, string sessionId)
        {
            this.isReportingEnabled = true;
            this.machineId = machineId;
            this.sessionId = sessionId;
            this.measurer.EnableReporting(machineId, sessionId);
            this.crasher.EnableReporting(machineId, sessionId);
        }

        public (string MachineId, string SessionId) GetReportingIds()
        {
            return (this.machineId, this.sessionId);
        }

        public void BeginOperation(string messageTemplate, params object[] args)
        {
            Trace(LogLevel.Information, messageTemplate, args);
            this.measurer.BeginOperation(messageTemplate, args);
        }

        public void EndOperation(bool success, string messageTemplate, params object[] args)
        {
            Trace(LogLevel.Information, messageTemplate, args);
            this.measurer.EndOperation(success, messageTemplate, args);
        }

        public void Count(string eventName, Dictionary<string, string> context = null)
        {
            var cleaned = eventName
                .Replace(" ", string.Empty)
                .ToLowerInvariant();
            var formatted = $"cli_{cleaned}";
            Trace(LogLevel.Information, LoggingMessages.Recorder_Measure, formatted);
            if (this.isReportingEnabled)
            {
                this.measurer.Count(formatted, context);
            }
        }

        public void Crash(CrashLevel level, Exception exception, string messageTemplate, params object[] args)
        {
            Trace(LogLevel.Error, exception, LoggingMessages.Recorder_Crash.Substitute(messageTemplate), args);
            if (this.isReportingEnabled)
            {
                this.crasher.Crash(level, exception, messageTemplate, args);
            }
        }

        public void Trace(LogLevel level, string messageTemplate, params object[] args)
        {
            Trace(level, null, messageTemplate, args);
        }

        private void Trace(LogLevel level, Exception exception, string messageTemplate, params object[] args)
        {
            if (this.logger.IsEnabled(level))
            {
                this.logger.Log(level, exception, messageTemplate, args);
            }
        }
    }
}