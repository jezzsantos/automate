#if !TESTINGONLY
using System;
using System.IO;
using Automate.Common.Domain;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.WindowsServer.TelemetryChannel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Automate.CLI.Infrastructure
{
    public static class ApplicationInsightsExtensions
    {
        private const string AppInsightsConnectionSettingName = "ApplicationInsights:ConnectionString";
        private static readonly string LocalTelemetryCachePath = Path.Combine("automate", "usage-cache");
        private const string ApplicationInsightsMapNameFormat = "{0} CLI";

        public static void RegisterApplicationInsightsClient(this IServiceCollection services, IConfiguration settings,
            IAssemblyMetadata assemblyMetadata)
        {
            var telemetryClient = CreateApplicationInsightsTelemetryClient(settings, assemblyMetadata);
            services.AddSingleton(telemetryClient);
        }

        private static TelemetryClient CreateApplicationInsightsTelemetryClient(IConfiguration settings,
            IAssemblyMetadata assemblyMetadata)
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
            telemetryClient.Context.Component.Version = assemblyMetadata.RuntimeVersion.ToString();
            telemetryClient.Context.Cloud.RoleName = CreateCloudRoleName(assemblyMetadata);
            telemetryClient.Context.Device.OperatingSystem = Environment.OSVersion.ToString();

            return telemetryClient;

            string GetStoragePath()
            {
                var path = Path.Combine(assemblyMetadata.InstallationPath, LocalTelemetryCachePath);
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                return path;
            }
        }

        private static string CreateCloudRoleName(IAssemblyMetadata assemblyMetadata)
        {
            return string.Format(ApplicationInsightsMapNameFormat, assemblyMetadata.ProductName);
        }
    }
}
#endif