#if !TESTINGONLY
using System;
using System.Collections.Generic;
using Automate.Common;
using Automate.Common.Extensions;
using Microsoft.ApplicationInsights;

namespace Automate.CLI.Infrastructure
{
    public class ApplicationInsightsMetricReporter : IMetricReporter, IDisposable
    {
        private readonly TelemetryClient client;
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
            this.client.Context.User.Id = machineId;
            this.client.Context.Session.Id = sessionId;
        }
    }
}
#endif