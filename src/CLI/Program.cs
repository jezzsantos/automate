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
        private const string AppInsightsConnectionStringSetting = "ApplicationInsights:ConnectionString";
#endif

        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(builder =>
                {
                    var assembly = typeof(Program).Assembly;
                    var ns = typeof(Program).Namespace;
                    builder
                        .AddJsonStream(assembly.GetManifestResourceStream($"{ns}.appsettings.json"))
                        .AddJsonFile("appsettings.local.json", true, false);
                })
                .ConfigureLogging((context, logging) =>
                {
                    logging.ClearProviders();
                    logging.AddFile("automate/automate.log", options =>
                    {
                        options.Append = true;
                        options.MinLevel = context.Configuration
                            .GetValue<string>("Logging:RollingFile:LogLevel:Default")
                            .ToEnumOrDefault(LogLevel.Information);
                        options.MaxRollingFiles = 1;
                        options.FileSizeLimitBytes = 10 * 1000 * 1000;
                    });
#if TESTINGONLY
                    logging.AddConsole();
#else
                    //We only want to send unhandled exceptions, crash reports, and events to AI
                    logging.AddApplicationInsights();
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
                recorder.BeginOperation("Starting CLI");

                host.Start();

                var lifetime = host.Services.GetRequiredService<IHostApplicationLifetime>();
                var container = new DotNetDependencyContainer(host.Services);
#if !TESTINGONLY
                var telemetryClient = host.Services.GetRequiredService<TelemetryClient>();
#endif
                var result = 0;
                try
                {
                    result = CommandLineApi.Execute(container, args);
                    lifetime.StopApplication();
                    host.WaitForShutdownAsync().GetAwaiter().GetResult();
                }
                catch (Exception ex)
                {
                    recorder.CrashFatal(ex, "CLI run failed");
                    Console.Error.WriteLine(ex.Message);
                    result = 1;
                }
                finally
                {
                    recorder.EndOperation(result == 0, "Shutting down CLI");
#if !TESTINGONLY
                    FlushTelemetry(recorder, telemetryClient);
#endif
                }

                return result;
            }
        }

        internal static void PopulateContainerForLocalMachineAndCurrentDirectory(IConfiguration settings,
            IServiceCollection services)
        {
            var currentDirectory = Environment.CurrentDirectory;
            var assemblyMetadata = new CliAssemblyMetadata();
#if !TESTINGONLY
            var telemetryClient = CreateApplicationInsightsTelemetryClient(settings, assemblyMetadata);
            services.AddSingleton(telemetryClient);
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
        }

        private static IRecorder CreateRecorder(IServiceProvider services, string categoryName)
        {
            var logger = new DotNetCoreLogger(services.GetRequiredService<ILoggerFactory>(), categoryName);
#if !TESTINGONLY
            var telemetryClient = services.GetRequiredService<TelemetryClient>();
#endif
            var recorder = new Recorder(logger,
#if TESTINGONLY
                new LoggingCrashReporter(logger),
                new LoggingMetricReporter(logger));
#else
                new ApplicationInsightsCrashReporter(telemetryClient),
                new ApplicationInsightsMetricReporter(telemetryClient));
#endif
            return recorder;
        }

#if !TESTINGONLY
        private static void FlushTelemetry(IRecorder recorder, TelemetryClient telemetryClient)
        {
            recorder.TraceDebug("Flushing any usage telemetry");
            telemetryClient.FlushAsync(CancellationToken.None)
                .GetAwaiter().GetResult(); //We use the Async version here since it should block until all telemetry is transmitted
        }

        private static TelemetryClient CreateApplicationInsightsTelemetryClient(IConfiguration settings, IAssemblyMetadata assemblyMetadata)
        {
            var connectionString = settings.GetValue<string>(AppInsightsConnectionStringSetting, null);

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
            telemetryClient.Context.Cloud.RoleName = $"{assemblyMetadata.ProductName} CLI";
            telemetryClient.Context.Device.OperatingSystem = Environment.OSVersion.ToString();
                
            return telemetryClient;
            
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