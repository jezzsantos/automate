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
        private bool reportingEnabled;

        public ApplicationInsightsCrashReporter(TelemetryClient client)
        {
            client.GuardAgainstNull(nameof(client));
            this.client = client;
            this.reportingEnabled = false;
        }

        public void Crash(CrashLevel level, Exception exception, string messageTemplate, params object[] args)
        {
            if (this.reportingEnabled)
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

        public void EnableReporting(string machineId, string sessionId)
        {
            this.reportingEnabled = true;
            this.client.Context.Cloud.RoleInstance = machineId;
            this.client.Context.Device.Id = machineId;
            this.client.Context.User.Id = machineId;
            this.client.Context.Session.Id = sessionId;
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