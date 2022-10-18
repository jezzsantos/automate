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
        private bool usageCollectionEnabled;

        public ApplicationInsightsMetricReporter(TelemetryClient client)
        {
            client.GuardAgainstNull(nameof(client));
            this.client = client;
            this.usageCollectionEnabled = true;
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
            if (this.usageCollectionEnabled)
            {
                if (this.client.Exists())
                {
                    this.client.TrackEvent(eventName.ToLower(), context);
                }
            }
        }

        public void DisableUsageCollection()
        {
            this.usageCollectionEnabled = false;
        }

        public void SetUserId(string id)
        {
            this.client.Context.User.Id = id;
        }
    }
}
#endif