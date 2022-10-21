using System;
using Automate.Common;
using Automate.Common.Extensions;
using Microsoft.Extensions.Logging;

namespace Automate.CLI.Infrastructure
{
    public class LoggingCrashReporter : ICrashReporter
    {
        private readonly ILogger logger;
        private string machineId;
        private bool reportingEnabled;
        private string sessionId;

        public LoggingCrashReporter(ILogger logger)
        {
            logger.GuardAgainstNull(nameof(logger));
            this.logger = logger;
            this.reportingEnabled = false;
            this.machineId = null;
            this.sessionId = null;
        }

        public void Crash(CrashLevel level, Exception exception, string messageTemplate, params object[] args)
        {
            if (this.reportingEnabled)
            {
                this.logger.Log(LogLevel.Error,
                    $"Crashed: for '{this.machineId}:{this.sessionId}' with message {messageTemplate}, and exception: {exception}",
                    args);
            }
        }

        public void EnableReporting(string machineId, string sessionId)
        {
            this.reportingEnabled = true;
            this.machineId = machineId;
            this.sessionId = sessionId;
        }
    }
}