using System;
using Automate.Common;
using Automate.Common.Extensions;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.Logging;

namespace Automate.CLI.Infrastructure.Recording
{
    public class ApplicationInsightsSessionReporter : ISessionReporter, IDisposable
    {
        internal const string SessionOperationName = "cli-operation-session";
        private readonly ITelemetryClient client;
        private readonly ILogger logger;
        private string parentOperationId;
        private bool reportingEnabled;
        private string operationId;

        public ApplicationInsightsSessionReporter(ILogger logger, ITelemetryClient client)
        {
            logger.GuardAgainstNull(nameof(logger));
            client.GuardAgainstNull(nameof(client));
            this.logger = logger;
            this.client = client;
            this.reportingEnabled = false;
            this.operationId = null;
            this.parentOperationId = null;
        }

        internal RequestTelemetry Telemetry { get; private set; }

        public void Dispose()
        {
            if (this.client.Exists())
            {
                this.client.SendAllTelemetry();
            }
        }

        public void EnableReporting(string machineId, string correlationId)
        {
            machineId.GuardAgainstNull(nameof(machineId));

            if (Telemetry.NotExists())
            {
                throw new InvalidOperationException(ExceptionMessages
                    .ApplicationInsightsSessionReporter_ExpectEnableReportingAfterSessionStart);
            }

            var correlation = ParseCorrelationId(correlationId);

            this.reportingEnabled = true;
            this.client.SetRoleInstance(machineId);
            this.client.SetDeviceId(machineId);
            this.client.SetUserId(machineId);
            this.client.SetSessionId(correlation.SessionId);
            this.operationId = correlation.OperationId;
            this.parentOperationId = correlation.OperationParentId;
        }

        public void MeasureStartSession(string messageTemplate, params object[] args)
        {
            if (this.reportingEnabled)
            {
                throw new InvalidOperationException(ExceptionMessages
                    .ApplicationInsightsSessionReporter_ExpectEnableReportingAfterSessionStart);
            }

            var id = CreateRandomId("opr");
            Telemetry = new RequestTelemetry
            {
                Name = SessionOperationName,
                Id = id
            };
            this.client.SetOperationId(id);

            Telemetry.Start();
        }

        public void MeasureEndSession(bool success, string messageTemplate, params object[] args)
        {
            if (this.reportingEnabled)
            {
                if (Telemetry.Exists())
                {
                    Telemetry.Success = success;
                    Telemetry.Context.Operation.Id = this.operationId;
                    Telemetry.Context.Operation.ParentId = this.parentOperationId;
                    Telemetry.Stop();

                    this.client.TrackRequest(Telemetry);
                    Telemetry = null;
                }

                this.logger.LogDebug(LoggingMessages.Program_FlushTelemetry);
                this.client.SendAllTelemetry();
            }
        }

        private static string CreateRandomId(string prefix)
        {
            return $"cli_{prefix}_{Guid.NewGuid():N}";
        }

        private static (string SessionId, string OperationId, string OperationParentId) ParseCorrelationId(
            string correlationId)
        {
            if (correlationId.HasNoValue())
            {
                var fallBackSessionId = CreateRandomId("ses");
                return (fallBackSessionId, null, null);
            }

            var parts = correlationId
                .Split('|', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 3)
            {
                return (correlationId, null, null);
            }

            return (parts[0], parts[1], parts[2]);
        }

#if TESTINGONLY
        public void ResetTelemetry()
        {
            Telemetry = null;
        }
#endif
    }
}