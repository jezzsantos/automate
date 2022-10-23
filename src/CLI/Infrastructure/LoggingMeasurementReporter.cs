using System.Collections.Generic;
using Automate.Common;
using Automate.Common.Extensions;
using Microsoft.Extensions.Logging;

namespace Automate.CLI.Infrastructure
{
    public class LoggingMeasurementReporter : IMeasurementReporter
    {
        private readonly ILogger logger;
        private string machineId;
        private bool reportingEnabled;
        private string sessionId;

        public LoggingMeasurementReporter(ILogger logger)
        {
            logger.GuardAgainstNull(nameof(logger));
            this.logger = logger;
            this.reportingEnabled = false;
            this.machineId = null;
            this.sessionId = null;
        }

        public void EnableReporting(string machineId, string sessionId)
        {
            this.reportingEnabled = true;
            this.machineId = machineId;
            this.sessionId = sessionId;
        }

        public void MeasureEvent(string eventName, Dictionary<string, string> context = null)
        {
            if (this.reportingEnabled)
            {
                this.logger.Log(LogLevel.Information,
                    "Measured event: '{EventName}' for '{MachineId}:{SessionId}', with context: {Context}",
                    eventName, this.machineId, this.sessionId,
                    context.ToJson());
            }
        }

        public void MeasureStartSession(string messageTemplate, params object[] args)
        {
            this.logger.Log(LogLevel.Information, null, messageTemplate, args);
        }

        public void MeasureEndSession(bool success, string messageTemplate, params object[] args)
        {
            this.logger.Log(LogLevel.Information, null, messageTemplate, args);
        }
    }
}