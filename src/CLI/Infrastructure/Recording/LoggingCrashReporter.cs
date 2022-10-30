using System;
using Automate.Common;
using Automate.Common.Extensions;
using Microsoft.Extensions.Logging;

namespace Automate.CLI.Infrastructure.Recording
{
    public class LoggingCrashReporter : ICrashReporter
    {
        private readonly ILogger logger;
        private bool reportingEnabled;
        private (string MachineId, string CorrelationId) reportingIds;

        public LoggingCrashReporter(ILogger logger)
        {
            logger.GuardAgainstNull(nameof(logger));
            this.logger = logger;
            this.reportingEnabled = false;
            this.reportingIds = default;
        }

        public void Crash(CrashLevel level, Exception exception, string messageTemplate, params object[] args)
        {
            if (this.reportingEnabled)
            {
                this.logger.Log(LogLevel.Error,
                    $"Crashed: for '{this.reportingIds.MachineId}:{this.reportingIds.CorrelationId}' with message {messageTemplate}, and exception: {exception}",
                    args);
            }
        }

        public void EnableReporting(string machineId, string correlationId)
        {
            this.reportingEnabled = true;
            this.reportingIds = (machineId, correlationId);
        }
    }
}