using System.Collections.Generic;
using Automate.Common;
using Automate.Common.Extensions;
using Microsoft.ApplicationInsights.DataContracts;

namespace Automate.CLI.Infrastructure.Recording
{
    public class ApplicationInsightsMeasurementReporter : IMeasurementReporter
    {
        private readonly ITelemetryClient client;
        private bool reportingEnabled;

        public ApplicationInsightsMeasurementReporter(ITelemetryClient client)
        {
            client.GuardAgainstNull(nameof(client));
            this.client = client;
            this.reportingEnabled = false;
        }

        public void EnableReporting(string machineId, string correlationId)
        {
            machineId.GuardAgainstNull(nameof(machineId));

            this.reportingEnabled = true;
        }

        public void MeasureEvent(string eventName, Dictionary<string, string> context = null)
        {
            if (this.reportingEnabled)
            {
                var telemetry = new EventTelemetry(eventName.ToLower());
                if (context.Exists())
                {
                    foreach (var (key, value) in context)
                    {
                        telemetry.Properties.Add(key, value);
                    }
                }
                telemetry.Context.Operation.ParentId = this.client.GetOperationId();

                this.client.TrackEvent(telemetry);
            }
        }
    }
}