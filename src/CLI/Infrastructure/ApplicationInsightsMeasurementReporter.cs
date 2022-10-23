#if !TESTINGONLY
using System;
using System.Collections.Generic;
using System.Threading;
using Automate.Common;
using Automate.Common.Extensions;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.Logging;

namespace Automate.CLI.Infrastructure
{
    public class ApplicationInsightsMeasurementReporter : IMeasurementReporter, IDisposable
    {
        private const string SessionRequestName = "cli-use";
        private readonly TelemetryClient client;
        private readonly ILogger logger;
        private RequestTelemetry request;
        private bool reportingEnabled;

        public ApplicationInsightsMeasurementReporter(TelemetryClient client, ILogger logger)
        {
            client.GuardAgainstNull(nameof(client));
            logger.GuardAgainstNull(nameof(logger));
            this.client = client;
            this.logger = logger;
            this.reportingEnabled = false;
        }

        public void Dispose()
        {
            if (this.client.Exists())
            {
                this.client.Flush();
            }
        }

        public void EnableReporting(string machineId, string sessionId)
        {
            this.reportingEnabled = true;
            this.client.Context.Cloud.RoleInstance = machineId;
            this.client.Context.Device.Id = machineId;
            this.client.Context.User.Id = machineId;
            this.client.Context.Session.Id = sessionId;
        }

        public void MeasureEvent(string eventName, Dictionary<string, string> context = null)
        {
            if (this.reportingEnabled)
            {
                this.client.TrackEvent(eventName.ToLower(), context);
            }
        }

        public void MeasureStartSession(string messageTemplate, params object[] args)
        {
            this.request = new RequestTelemetry
            {
                Name = SessionRequestName
            };
            this.request.GenerateOperationId();
            this.request.Start();
        }

        public void MeasureEndSession(bool success, string messageTemplate, params object[] args)
        {
            if (this.reportingEnabled)
            {
                if (this.request.Exists())
                {
                    this.request.Success = success;
                    this.request.Stop();
                    this.client.TrackRequest(this.request);
                }
            }
            
            this.logger.LogDebug(LoggingMessages.Program_FlushTelemetry);
            this.client.FlushAsync(CancellationToken.None)
                .GetAwaiter()
                .GetResult(); //We use the Async version here since it should block until all telemetry is transmitted
        }
    }
}
#endif