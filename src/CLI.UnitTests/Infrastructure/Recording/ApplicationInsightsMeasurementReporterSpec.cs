using System.Collections.Generic;
using Automate.CLI.Infrastructure.Recording;
using Automate.Common.Extensions;
using Microsoft.ApplicationInsights.DataContracts;
using Moq;
using Xunit;

namespace CLI.UnitTests.Infrastructure.Recording
{
    [Trait("Category", "Unit")]
    public class ApplicationInsightsMeasurementReporterSpec
    {
        private readonly ApplicationInsightsMeasurementReporter reporter;
        private readonly Mock<ITelemetryClient> telemetryClient;

        public ApplicationInsightsMeasurementReporterSpec()
        {
            this.telemetryClient = new Mock<ITelemetryClient>();
            this.telemetryClient.Setup(tc => tc.GetOperationId())
                .Returns("anoperationid");
            this.reporter = new ApplicationInsightsMeasurementReporter(this.telemetryClient.Object);
        }

        [Fact]
        public void WhenMeasureAndReportingNotEnabled_ThenDoesNothing()
        {
            this.reporter.MeasureEvent("aneventname");

            this.telemetryClient.Verify(tc => tc.TrackEvent(It.IsAny<EventTelemetry>()), Times.Never);
        }

        [Fact]
        public void WhenMeasureAndNoContext_ThenReports()
        {
            this.reporter.EnableReporting("amachineid", "acorrelationid");

            this.reporter.MeasureEvent("An.Event.Name");

            this.telemetryClient.Verify(tc => tc.TrackEvent(It.Is<EventTelemetry>(et =>
                et.Name == "an.event.name"
                && et.Properties.HasNone()
                && et.Context.Operation.ParentId == "anoperationid"
            )));
        }

        [Fact]
        public void WhenMeasureAndHasContext_ThenReports()
        {
            this.reporter.EnableReporting("amachineid", "acorrelationid");

            this.reporter.MeasureEvent("An.Event.Name", new Dictionary<string, string>
            {
                { "aname1", "avalue1" },
                { "aname2", "avalue2" }
            });

            this.telemetryClient.Verify(tc => tc.TrackEvent(It.Is<EventTelemetry>(et =>
                et.Name == "an.event.name"
                && et.Properties.Count == 2
                && et.Properties["aname1"] == "avalue1"
                && et.Properties["aname2"] == "avalue2"
                && et.Context.Operation.ParentId == "anoperationid"
            )));
        }
    }
}