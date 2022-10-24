using System;
using System.Linq;
using Automate.Common;
using Automate.Common.Extensions;
using Microsoft.ApplicationInsights.DataContracts;

namespace Automate.CLI.Infrastructure.Recording
{
    public class ApplicationInsightsCrashReporter : ICrashReporter
    {
        private readonly ITelemetryClient client;
        private bool reportingEnabled;

        public ApplicationInsightsCrashReporter(ITelemetryClient client)
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

        public void Crash(CrashLevel level, Exception exception, string messageTemplate, params object[] args)
        {
            if (this.reportingEnabled)
            {
                var argsString = args?
                    .Select(arg => arg.ToString())
                    .SafeJoin(", ");

                var telemetry = new ExceptionTelemetry(exception)
                {
                    SeverityLevel = level == CrashLevel.Fatal
                        ? SeverityLevel.Critical
                        : SeverityLevel.Error,
                    Message = messageTemplate.SubstituteTemplate(args),
                    Properties =
                    {
                        { "Message_Template", messageTemplate },
                        { "Message_Arguments", argsString }
                    }
                };
                telemetry.Context.Operation.ParentId = this.client.GetOperationId();

                this.client.TrackException(telemetry);
            }
        }
    }
}