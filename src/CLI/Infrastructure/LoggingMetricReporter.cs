using System.Collections.Generic;
using Automate.Common;
using Automate.Common.Extensions;
using Microsoft.Extensions.Logging;

namespace Automate.CLI.Infrastructure
{
    public class LoggingMetricReporter : IMetricReporter
    {
        private readonly ILogger logger;
        private string machineId;
        private bool reportingEnabled;
        private string sessionId;

        public LoggingMetricReporter(ILogger logger)
        {
            logger.GuardAgainstNull(nameof(logger));
            this.logger = logger;
            this.reportingEnabled = false;
            this.machineId = null;
            this.sessionId = null;
        }

        public void Count(string eventName, Dictionary<string, string> context = null)
        {
            if (this.reportingEnabled)
            {
                this.logger.Log(LogLevel.Information,
                    "Measured event: '{EventName}' for '{MachineId}:{SessionId}', with context: {Context}",
                    eventName, this.machineId, this.sessionId,
                    context.ToJson());
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