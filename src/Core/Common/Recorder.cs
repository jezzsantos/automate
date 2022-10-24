using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Automate.Common.Extensions;
using Microsoft.Extensions.Logging;

namespace Automate.Common
{
    public class Recorder : IRecorder, IDisposable
    {
        private readonly ILogger logger;
        private readonly ISessionReporter sessioner;
        private readonly ICrashReporter crasher;
        private readonly IMeasurementReporter measurer;
        private bool isReportingEnabled;
        private (string MachineId, string CorrelationId) reportingIds;

        public Recorder(ILogger logger, ISessionReporter sessioner, ICrashReporter crasher,
            IMeasurementReporter measurer)
        {
            logger.GuardAgainstNull(nameof(logger));
            sessioner.GuardAgainstNull(nameof(sessioner));
            crasher.GuardAgainstNull(nameof(crasher));
            measurer.GuardAgainstNull(nameof(measurer));
            this.logger = logger;
            this.sessioner = sessioner;
            this.crasher = crasher;
            this.measurer = measurer;
            this.isReportingEnabled = false;
            this.reportingIds = default;
        }

        [SuppressMessage("ReSharper", "SuspiciousTypeConversion.Global")]
        public void Dispose()
        {
            (this.sessioner as IDisposable)?.Dispose();
            (this.crasher as IDisposable)?.Dispose();
            (this.measurer as IDisposable)?.Dispose();
        }

        public void EnableReporting(string machineId, string correlationId)
        {
            machineId.GuardAgainstNullOrEmpty(nameof(machineId));

            this.isReportingEnabled = true;
            this.reportingIds = (machineId, correlationId);
            this.sessioner.EnableReporting(machineId, correlationId);
            this.measurer.EnableReporting(machineId, correlationId);
            this.crasher.EnableReporting(machineId, correlationId);
        }

        public (string MachineId, string CorrelationId) GetReportingIds()
        {
            return this.reportingIds;
        }

        public void StartSession(string messageTemplate, params object[] args)
        {
            Trace(LogLevel.Information, messageTemplate, args);
            this.sessioner.MeasureStartSession(messageTemplate, args);
        }

        public void EndSession(bool success, string messageTemplate, params object[] args)
        {
            Trace(LogLevel.Information, messageTemplate, args);
            this.sessioner.MeasureEndSession(success, messageTemplate, args);
        }

        public void MeasureEvent(string eventName, Dictionary<string, string> context = null)
        {
            var cleaned = eventName
                .Replace(" ", string.Empty)
                .ToLowerInvariant();
            var formatted = $"cli_{cleaned}";
            Trace(LogLevel.Information, LoggingMessages.Recorder_Measure, formatted);
            if (this.isReportingEnabled)
            {
                this.measurer.MeasureEvent(formatted, context);
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