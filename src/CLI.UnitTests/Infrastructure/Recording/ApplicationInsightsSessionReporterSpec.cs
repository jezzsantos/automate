using System;
using System.Threading;
using Automate.CLI;
using Automate.CLI.Infrastructure.Recording;
using FluentAssertions;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CLI.UnitTests.Infrastructure.Recording
{
    [Trait("Category", "Unit")]
    public class ApplicationInsightsSessionReporterSpec
    {
        private readonly ApplicationInsightsSessionReporter reporter;
        private readonly Mock<ITelemetryClient> telemetryClient;

        public ApplicationInsightsSessionReporterSpec()
        {
            var logger = new Mock<ILogger>();
            this.telemetryClient = new Mock<ITelemetryClient>();
            this.reporter = new ApplicationInsightsSessionReporter(logger.Object, this.telemetryClient.Object);
        }

        [Fact]
        public void WhenEnableReportingBeforeMeasureStartSession_ThenThrows()
        {
            this.reporter
                .Invoking(x => x.EnableReporting("amachineid", null))
                .Should().Throw<InvalidOperationException>()
                .WithMessage(
                    ExceptionMessages.ApplicationInsightsSessionReporter_ExpectEnableReportingAfterSessionStart);
        }

        [Fact]
        public void WhenEnableReportingAndCorrelationIdIsNull_ThenSetsNewSessionAndLeavesOperationId()
        {
            this.reporter.MeasureStartSession("amessagetemplate");
            this.telemetryClient.Reset();

            this.reporter.EnableReporting("amachineid", null);

            this.telemetryClient.Verify(tc => tc.SetRoleInstance("amachineid"));
            this.telemetryClient.Verify(tc => tc.SetDeviceId("amachineid"));
            this.telemetryClient.Verify(tc => tc.SetUserId("amachineid"));
            this.telemetryClient.Verify(tc => tc.SetSessionId(It.Is<string>(s => s.Contains("_ses_"))));
            this.telemetryClient.Verify(tc => tc.SetOperationId(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void WhenEnableReportingAndCorrelationIdIsNotCompositeValue_ThenSetsSessionAndLeavesOperationId()
        {
            this.reporter.MeasureStartSession("amessagetemplate");
            this.telemetryClient.Reset();

            this.reporter.EnableReporting("amachineid", "avalue");

            this.telemetryClient.Verify(tc => tc.SetRoleInstance("amachineid"));
            this.telemetryClient.Verify(tc => tc.SetDeviceId("amachineid"));
            this.telemetryClient.Verify(tc => tc.SetUserId("amachineid"));
            this.telemetryClient.Verify(tc => tc.SetSessionId("avalue"));
            this.telemetryClient.Verify(tc => tc.SetOperationId(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void WhenEnableReportingAndCorrelationIdIsNotCompositeValue2_ThenSetsSessionAndLeavesOperationId()
        {
            this.reporter.MeasureStartSession("amessagetemplate");
            this.telemetryClient.Reset();

            this.reporter.EnableReporting("amachineid", "avalue1|avalue2");

            this.telemetryClient.Verify(tc => tc.SetRoleInstance("amachineid"));
            this.telemetryClient.Verify(tc => tc.SetDeviceId("amachineid"));
            this.telemetryClient.Verify(tc => tc.SetUserId("amachineid"));
            this.telemetryClient.Verify(tc => tc.SetSessionId("avalue1|avalue2"));
            this.telemetryClient.Verify(tc => tc.SetOperationId(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void WhenEnableReportingAndCorrelationIdIsCompositeValue_ThenSetsSessionAndCorrelation()
        {
            this.reporter.MeasureStartSession("amessagetemplate");
            this.telemetryClient.Reset();

            this.reporter.EnableReporting("amachineid", "avalue1|avalue2|avalue3");

            this.telemetryClient.Verify(tc => tc.SetRoleInstance("amachineid"));
            this.telemetryClient.Verify(tc => tc.SetDeviceId("amachineid"));
            this.telemetryClient.Verify(tc => tc.SetUserId("amachineid"));
            this.telemetryClient.Verify(tc => tc.SetSessionId("avalue1"));
            this.telemetryClient.Verify(tc => tc.SetOperationId(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void WhenMeasureStartSession_ThenStartsTelemetryAndSetsOperationId()
        {
            this.reporter.MeasureStartSession("amessagetemplate");

            var telemetry = this.reporter.Telemetry;

            telemetry.Name.Should().Be(ApplicationInsightsSessionReporter.SessionOperationName);
            telemetry.Id.Should().Contain("_opr_");
            telemetry.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
            this.telemetryClient.Verify(tc => tc.SetOperationId(telemetry.Id));
        }

        [Fact]
        public void WhenMeasureEndSessionAndReportingNotEnabled_ThenDoesNothing()
        {
            this.reporter.MeasureEndSession(true, "amessagetemplate");

            this.telemetryClient.Verify(tc => tc.TrackRequest(It.IsAny<RequestTelemetry>()), Times.Never);
            this.telemetryClient.Verify(tc => tc.SendAllTelemetry(), Times.Never);
        }

        [Fact]
        public void WhenMeasureEndSessionAndStartNotCalledFirst_ThenOnlySendsAllTelemetry()
        {
            this.reporter.MeasureStartSession("amessagetemplate");
            this.reporter.EnableReporting("amachineid", "avalue1|avalue2|avalue3");
#if TESTINGONLY
            this.reporter.ResetTelemetry();
#endif

            this.reporter.MeasureEndSession(true, "amessagetemplate");

            this.telemetryClient.Verify(tc => tc.TrackRequest(It.IsAny<RequestTelemetry>()), Times.Never);
            this.telemetryClient.Verify(tc => tc.SendAllTelemetry());
        }

        [Fact]
        public void WhenMeasureEndSession_ThenSendsTelemetry()
        {
            this.reporter.MeasureStartSession("amessagetemplate");
            this.reporter.EnableReporting("amachineid", "avalue1|avalue2|avalue3");
            Thread.Sleep(50);

            this.reporter.MeasureEndSession(true, "amessagetemplate");

            this.telemetryClient.Verify(tc => tc.TrackRequest(It.Is<RequestTelemetry>(rt =>
                rt.Name == ApplicationInsightsSessionReporter.SessionOperationName
                && rt.Id.Contains("_opr_")
                && rt.Context.Operation.Id == "avalue2"
                && rt.Context.Operation.ParentId == "avalue3"
                && rt.Timestamp < DateTimeOffset.UtcNow
                && rt.Success == true
                && rt.Duration.TotalMilliseconds > 50
            )));
            this.telemetryClient.Verify(tc => tc.SendAllTelemetry());
        }
    }
}