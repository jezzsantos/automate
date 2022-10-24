using System.Collections.Generic;
using Automate.Common;
using Automate.Common.Extensions;
using Microsoft.Extensions.Logging;

namespace Automate.CLI.Infrastructure.Recording
{
    public class LoggingMeasurementReporter : IMeasurementReporter
    {
        private readonly ILogger logger;
        private bool reportingEnabled;
        private (string MachineId, string CorrelationId) reportingIds;

        public LoggingMeasurementReporter(ILogger logger)
        {
            logger.GuardAgainstNull(nameof(logger));
            this.logger = logger;
            this.reportingEnabled = false;
            this.reportingIds = default;
        }

        public void EnableReporting(string machineId, string correlationId)
        {
            this.reportingEnabled = true;
            this.reportingIds = (machineId, correlationId);
        }

        public void MeasureEvent(string eventName, Dictionary<string, string> context = null)
        {
            if (this.reportingEnabled)
            {
                this.logger.Log(LogLevel.Information,
                    "Measured event: '{EventName}' for '{MachineId}:{CorrelationId}', with context: {Context}",
                    eventName, this.reportingIds.MachineId, this.reportingIds.CorrelationId,
                    context.ToJson());
            }
        }
    }
}