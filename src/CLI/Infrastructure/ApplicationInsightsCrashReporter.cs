#if !TESTINGONLY
using System;
using System.Collections.Generic;
using System.Linq;
using Automate.Common;
using Automate.Common.Extensions;
using Microsoft.ApplicationInsights;

namespace Automate.CLI.Infrastructure
{
    public class ApplicationInsightsCrashReporter : ICrashReporter, IDisposable
    {
        private readonly TelemetryClient client;
        private bool usageCollectionEnabled;

        public ApplicationInsightsCrashReporter(TelemetryClient client)
        {
            client.GuardAgainstNull(nameof(client));
            this.client = client;
            this.usageCollectionEnabled = true;
        }

        public void Crash(CrashLevel level, Exception exception, string messageTemplate, params object[] args)
        {
            if (this.usageCollectionEnabled)
            {
                if (this.client.Exists())
                {
                    var properties = args?
                        .Select(arg => arg.ToString())
                        .SafeJoin(", ");

                    this.client.TrackException(exception, new Dictionary<string, string>
                    {
                        { "Level", level.ToString() },
                        { "Message_Template", messageTemplate },
                        { "Message_Properties", properties }
                    });
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

        public void Dispose()
        {
            if (this.client.Exists())
            {
                this.client.Flush();
            }
        }
    }
}
#endif