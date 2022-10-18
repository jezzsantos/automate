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

        public ApplicationInsightsMetricReporter(TelemetryClient client)
        {
            client.GuardAgainstNull(nameof(client));
            this.client = client;
        }

        public void Dispose()
        {
            if (this.client.Exists())
            {
                this.client.Flush();
            }
        }

        public void Measure(string eventName, Dictionary<string, string> context = null)
        {
            if (this.client.Exists())
            {
                var properties = context ?? new Dictionary<string, string>();
                properties.Add("CallerId", "");
                this.client.TrackEvent(eventName, properties);
            }
        }
    }
}
#endif