using System;
using Automate.CLI.Infrastructure.Recording;
using Automate.Common;
using Microsoft.ApplicationInsights.DataContracts;
using Moq;
using Xunit;

namespace CLI.UnitTests.Infrastructure.Recording
{
    [Trait("Category", "Unit")]
    public class ApplicationInsightsCrashReporterSpec
    {
        private readonly Mock<ITelemetryClient> telemetryClient;
        private readonly ApplicationInsightsCrashReporter reporter;

        public ApplicationInsightsCrashReporterSpec()
        {
            this.telemetryClient = new Mock<ITelemetryClient>();
            this.telemetryClient.Setup(tc => tc.GetOperationId())
                .Returns("anoperationid");
            this.reporter = new ApplicationInsightsCrashReporter(this.telemetryClient.Object);
        }

        [Fact]
        public void WhenCrashAndReportingNotEnabled_ThenDoesNothing()
        {
            this.reporter.Crash(CrashLevel.Fatal, new Exception("amessage"), "amessagetemplate");

            this.telemetryClient.Verify(tc => tc.TrackException(It.IsAny<ExceptionTelemetry>()), Times.Never);
        }

        [Fact]
        public void WhenCrash_ThenSendsTelemetry()
        {
            this.reporter.EnableReporting("amachineid", "acorrelationid");
            var exception = new Exception("amessage");

            this.reporter.Crash(CrashLevel.Fatal, exception, "amessagetemplate");

            this.telemetryClient.Verify(tc => tc.TrackException(It.Is<ExceptionTelemetry>(et =>
                et.Exception == exception
                && et.SeverityLevel == SeverityLevel.Critical
                && et.Message == "amessagetemplate"
                && et.Properties.Count == 2
                && et.Properties["Message_Template"] == "amessagetemplate"
                && et.Properties["Message_Arguments"] == ""
                && et.Context.Operation.ParentId == "anoperationid"
            )));
        }

        [Fact]
        public void WhenCrashWithMessageArguments_ThenSendsTelemetry()
        {
            this.reporter.EnableReporting("amachineid", "acorrelationid");
            var exception = new Exception("amessage");

            this.reporter.Crash(CrashLevel.Fatal, exception, "amessagetemplate{Arg1}{Arg2}", "anarg1", "anarg2");

            this.telemetryClient.Verify(tc => tc.TrackException(It.Is<ExceptionTelemetry>(et =>
                et.Exception == exception
                && et.SeverityLevel == SeverityLevel.Critical
                && et.Message == "amessagetemplateanarg1anarg2"
                && et.Properties.Count == 2
                && et.Properties["Message_Template"] == "amessagetemplate{Arg1}{Arg2}"
                && et.Properties["Message_Arguments"] == "anarg1, anarg2"
                && et.Context.Operation.ParentId == "anoperationid"
            )));
        }
    }
}