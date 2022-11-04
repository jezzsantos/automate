using System;
using System.Threading;
using Automate.Common.Application;
using Automate.Common.Domain;
using Automate.Common.Extensions;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.WindowsServer.TelemetryChannel;
using Microsoft.Extensions.Configuration;

namespace Automate.CLI.Infrastructure.Recording
{
    public class ApplicationInsightsTelemetryClient : ITelemetryClient
    {
        private const string AppInsightsConnectionSettingName = "ApplicationInsights:ConnectionString";
        private const string ApplicationInsightsMapNameFormat = "{0} CLI";
        private const string LocalTelemetryCachePath = "usage-cache";
        private readonly TelemetryClient client;

        public ApplicationInsightsTelemetryClient(IConfiguration settings, IRuntimeMetadata metadata,
            IFileSystemReaderWriter readerWriter) : this(
            CreateClient(settings, metadata, readerWriter))
        {
        }

        internal ApplicationInsightsTelemetryClient(TelemetryClient client)
        {
            client.GuardAgainstNull(nameof(client));
            this.client = client;
        }

        public void SetRoleInstance(string name)
        {
            this.client.Context.Cloud.RoleInstance = name;
        }

        public void SetDeviceId(string id)
        {
            this.client.Context.Device.Id = id;
        }

        public void SetUserId(string id)
        {
            this.client.Context.User.Id = id;
        }

        public void SetSessionId(string id)
        {
            this.client.Context.Session.Id = id;
        }

        public void SetOperationId(string id)
        {
            this.client.Context.Operation.Id = id;
        }

        public string GetOperationId()
        {
            return this.client.Context.Operation.Id;
        }

        public void TrackException(ExceptionTelemetry telemetry)
        {
            this.client.TrackException(telemetry);
        }

        public void TrackEvent(EventTelemetry telemetry)
        {
            this.client.TrackEvent(telemetry);
        }

        public void TrackRequest(RequestTelemetry telemetry)
        {
            this.client.TrackRequest(telemetry);
        }

        public void SendAllTelemetry()
        {
            this.client.FlushAsync(CancellationToken.None)
                .GetAwaiter()
                .GetResult(); //We use the Async version here since it should block until all telemetry is transmitted
        }

        private static TelemetryClient CreateClient(IConfiguration settings,
            IRuntimeMetadata metadata, IFileSystemReaderWriter readerWriter)
        {
            var connectionString = settings.GetValue<string>(AppInsightsConnectionSettingName, null);

            var channel = new ServerTelemetryChannel();
            channel.StorageFolder = GetStoragePath();
            channel.MaxTelemetryBufferCapacity = 1; // send immediately to disk (default 500)
            channel.MaxTelemetryBufferDelay = TimeSpan.FromMilliseconds(1); // (default 30secs)
            channel.MaxTransmissionSenderCapacity = 100; // (default 10)
            var configuration = TelemetryConfiguration.CreateDefault();
            configuration.ConnectionString = connectionString;
            channel.Initialize(configuration);
            configuration.TelemetryChannel = channel;

            var telemetryClient = new TelemetryClient(configuration);
            telemetryClient.Context.Component.Version = metadata.RuntimeVersion.ToString();
            telemetryClient.Context.Cloud.RoleName = CreateCloudRoleName(metadata);
            telemetryClient.Context.Device.OperatingSystem = Environment.OSVersion.ToString();

            return telemetryClient;

            string GetStoragePath()
            {
                var path = readerWriter.MakeAbsolutePath(metadata.UserDataPath, LocalTelemetryCachePath);
                readerWriter.EnsureDirectoryExists(path);

                return path;
            }
        }

        private static string CreateCloudRoleName(IRuntimeMetadata metadata)
        {
            return string.Format(ApplicationInsightsMapNameFormat, metadata.ProductName);
        }
    }
}