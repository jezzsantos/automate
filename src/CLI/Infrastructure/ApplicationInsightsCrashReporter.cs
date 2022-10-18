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

        public ApplicationInsightsCrashReporter(TelemetryClient client)
        {
            client.GuardAgainstNull(nameof(client));
            this.client = client;
        }

        public void Crash(CrashLevel level, Exception exception, string messageTemplate, params object[] args)
        {
            if (this.client.Exists())
            {
                var properties = args?
                    .Select(arg => arg.ToString())
                    .SafeJoin(", ");

                this.client.TrackException(exception, new Dictionary<string, string>
                {
                    { "Level", level.ToString() },
                    { "CallerId", "" },
                    { "Message_Template", messageTemplate },
                    { "Message_Properties", properties }
                });
            }
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