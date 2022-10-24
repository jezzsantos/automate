using Microsoft.ApplicationInsights.DataContracts;

namespace Automate.CLI.Infrastructure.Recording
{
    public interface ITelemetryClient
    {
        void SetRoleInstance(string name);

        void SetDeviceId(string id);

        void SetUserId(string id);

        void SetSessionId(string id);

        void SetOperationId(string id);

        string GetOperationId();

        void TrackException(ExceptionTelemetry telemetry);

        void TrackEvent(EventTelemetry telemetry);

        void TrackRequest(RequestTelemetry telemetry);

        void SendAllTelemetry();
    }
}