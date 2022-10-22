#if !TESTINGONLY
using System;
using System.Collections.Generic;
using Automate.Common;
using Automate.Common.Extensions;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;

namespace Automate.CLI.Infrastructure
{
    public class ApplicationInsightsMetricReporter : IMetricReporter, IDisposable
    {
        private const string OperationName = "cli-use";
        private readonly TelemetryClient client;
        private RequestTelemetry operation;
        private bool reportingEnabled;

        public ApplicationInsightsMetricReporter(TelemetryClient client)
        {
            client.GuardAgainstNull(nameof(client));
            this.client = client;
            this.reportingEnabled = false;
        }

        public void Dispose()
        {
            if (this.client.Exists())
            {
                this.client.Flush();
            }
        }

        public void Count(string eventName, Dictionary<string, string> context = null)
        {
            if (this.reportingEnabled)
            {
                this.client.TrackEvent(eventName.ToLower(), context);
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

        public void BeginOperation(string messageTemplate, params object[] args)
        {
            this.operation = new RequestTelemetry
            {
                Name = OperationName
            };
            this.operation.Start();
            this.operation.GenerateOperationId();
        }

        public void EndOperation(bool success, string messageTemplate, params object[] args)
        {
            if (this.reportingEnabled)
            {
                if (this.operation.Exists())
                {
                    this.operation.Success = success;
                    this.operation.Stop();
                    this.client.TrackRequest(this.operation);
                }
            }
        }
    }
}
#endif