using System;
using Automate.Authoring.Application;
using Automate.Authoring.Infrastructure;
using Automate.CLI.Infrastructure;
using Automate.Common;
using Automate.Common.Application;
using Automate.Common.Domain;
using Automate.Common.Extensions;
using Automate.Common.Infrastructure;
using Automate.Runtime.Application;
using Automate.Runtime.Domain;
using Automate.Runtime.Infrastructure;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
#if !TESTINGONLY
using System.IO;
using System.Threading;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.WindowsServer.TelemetryChannel;
#endif

namespace Automate.CLI
{
    [UsedImplicitly]
    internal class Program
    {
#if !TESTINGONLY
        private static readonly TimeSpan TelemetryDeliveryWindow = TimeSpan.FromSeconds(2);
        private const string AppInsightsConnectionStringSetting = "ApplicationInsights:ConnectionString";
#endif

        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(builder =>
                {
                    builder
                        .AddJsonFile("appsettings.json", false, false)
                        .AddJsonFile("appsettings.local.json", true, false);
                })
                .ConfigureLogging((context, logging) =>
                {
                    logging.ClearProviders();
                    logging.AddFile(context.Configuration.GetSection("Logging"));
#if TESTINGONLY
                    logging.AddConsole();
#else
                    //We are not sending Traces to AI, only crash reports and events
#endif
                })
                .ConfigureServices((context, services) =>
                    PopulateContainerForLocalMachineAndCurrentDirectory(context.Configuration, services));
        }

        [UsedImplicitly]
        private static int Main(string[] args)
        {
            using (var host = CreateHostBuilder(args).Build())
            {
                var recorder = host.Services.GetRequiredService<IRecorder>();
                recorder.TraceInformation("Starting up CLI host");

                host.Start();

                var lifetime = host.Services.GetRequiredService<IHostApplicationLifetime>();
                var container = host.Services.GetRequiredService<IDependencyContainer>();

                try
                {
                    recorder.TraceInformation("Running CLI");

                    var result = CommandLineApi.Execute(container, args);
                    lifetime.StopApplication();
                    host.WaitForShutdownAsync().GetAwaiter().GetResult();

                    recorder.TraceInformation("Shutting down CLI host");
                    return result;
                }
                catch (Exception ex)
                {
                    recorder.CrashFatal(ex, "CLI failed to run");
                    Console.Error.WriteLine(ex.Message);
                    return 1;
                }
#if !TESTINGONLY
                finally
                {
                    DelayToSendTelemetry(recorder);
                }
#endif
            }
        }

        internal static void PopulateContainerForLocalMachineAndCurrentDirectory(IConfiguration settings,
            IServiceCollection services)
        {
            var currentDirectory = Environment.CurrentDirectory;
            var assemblyMetadata = new CliAssemblyMetadata();
#if !TESTINGONLY
            RegisterApplicationInsightsTelemetryClient(settings, services, assemblyMetadata);
#endif
            services.AddSingleton(c => CreateRecorder(c, "automate-cli"));
            services.AddSingleton(c => new LocalMachineUserRepository(assemblyMetadata.InstallationPath,
                c.GetRequiredService<IFileSystemReaderWriter>(),
                c.GetRequiredService<IPersistableFactory>()));
            services.AddSingleton(c =>
                new LocalMachineFileRepository(currentDirectory, c.GetRequiredService<IFileSystemReaderWriter>(),
                    c.GetRequiredService<IPersistableFactory>()));
            services.AddSingleton<IMachineRepository>(c => c.GetRequiredService<LocalMachineUserRepository>());
            services.AddSingleton<IPatternRepository>(c => c.GetRequiredService<LocalMachineFileRepository>());
            services.AddSingleton<IToolkitRepository>(c => c.GetRequiredService<LocalMachineFileRepository>());
            services.AddSingleton<IDraftRepository>(c => c.GetRequiredService<LocalMachineFileRepository>());
            services.AddSingleton<ILocalStateRepository>(c => c.GetRequiredService<LocalMachineFileRepository>());
            services.AddSingleton<IMachineStore, MachineStore>();
            services.AddSingleton<IPatternStore, PatternStore>();
            services.AddSingleton<IToolkitStore, ToolkitStore>();
            services.AddSingleton<IDraftStore, DraftStore>();
            services.AddSingleton<IFilePathResolver, SystemIoFilePathResolver>();
            services.AddSingleton<IPatternPathResolver, PatternPathResolver>();
            services.AddSingleton<IDraftPathResolver, DraftPathResolver>();
            services.AddSingleton<IPatternToolkitPackager, PatternToolkitPackager>();
            services.AddSingleton<ITextTemplatingEngine, TextTemplatingEngine>();
            services.AddSingleton<IApplicationExecutor, ApplicationExecutor>();
            services.AddSingleton<IFileSystemReaderWriter, SystemIoFileSystemReaderWriter>();
            services.AddSingleton<IAutomationExecutor, AutomationExecutor>();
            services.AddSingleton<IPersistableFactory, AutomatePersistableFactory>();
            services.AddSingleton<IAssemblyMetadata>(assemblyMetadata);

            services.AddSingleton<IDependencyContainer>(new DotNetDependencyContainer(services));
        }

        private static IRecorder CreateRecorder(IServiceProvider services, string categoryName)
        {
            var logger = new DotNetCoreLogger(services.GetRequiredService<ILoggerFactory>(), categoryName);
            var recorder = new Recorder(logger,
#if TESTINGONLY
                new LoggingCrashReporter(logger),
                new LoggingMetricReporter(logger));
#else
                new ApplicationInsightsCrashReporter(services.GetRequiredService<TelemetryClient>()),
                new ApplicationInsightsMetricReporter(services.GetRequiredService<TelemetryClient>()));
#endif
            return recorder;
        }

#if !TESTINGONLY
        private static void DelayToSendTelemetry(IRecorder recorder)
        {
            recorder.TraceInformation("Delaying for telemetry delivery");
            Thread.Sleep(TelemetryDeliveryWindow);
        }

        private static void RegisterApplicationInsightsTelemetryClient(IConfiguration settings,
            IServiceCollection services, CliAssemblyMetadata assemblyMetadata)
        {
            var connectionString = settings.GetValue<string>(AppInsightsConnectionStringSetting, null);

            var channel = new ServerTelemetryChannel();
            channel.StorageFolder = GetStoragePath();
            channel.MaxBacklogSize = 10 * 1000; // (default 1,000,000)
            channel.MaxTelemetryBufferCapacity = 1; // nothing stored in memory, send immediately to disk (default 500)
            channel.MaxTelemetryBufferDelay = TimeSpan.FromMilliseconds(1); // (default 30secs)

            // channel.MaxTransmissionSenderCapacity = 100; // blast 100 at a time to cloud (default 10)
            // channel.MaxTransmissionStorageCapacity = 10 * 1000 * 1000; //store as much as needed 10MB (default 52,428,800)
            // channel.MaxTransmissionBufferCapacity = 2; // nothing stored in memory, send to disk? (default 5,242,880)
            var configuration = TelemetryConfiguration.CreateDefault();
            configuration.ConnectionString = connectionString;
            channel.Initialize(configuration);
            configuration.TelemetryChannel = channel;

            var telemetryClient = new TelemetryClient(configuration);
            telemetryClient.Context.Component.Version = assemblyMetadata.RuntimeVersion.ToString();
            telemetryClient.Context.GlobalProperties["Application"] = assemblyMetadata.ProductName;
            telemetryClient.Context.Device.OperatingSystem = Environment.OSVersion.ToString();
            services.AddSingleton(telemetryClient);

            string GetStoragePath()
            {
                var path = Path.Combine(assemblyMetadata.InstallationPath, Path.Combine("automate", "usage-cache"));
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                return path;
            }
        }
#endif
    }
}