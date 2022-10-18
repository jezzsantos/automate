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
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
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
                    builder
                        .AddJsonFile("appsettings.json", false, false)
                        .AddJsonFile("appsettings.local.json", true, false)
                        .AddEnvironmentVariables();
                })
                .ConfigureLogging((_, logging) =>
                {
                    logging.ClearProviders();
#if TESTINGONLY
                    logging.AddConsole();
#else
                    //We are not sending Traces to AI
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
                    recorder.Measure("Run");
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
            }
        }

        internal static void PopulateContainerForLocalMachineAndCurrentDirectory(IConfiguration settings,
            IServiceCollection services)
        {
#if !TESTINGONLY
            var configuration = TelemetryConfiguration.CreateDefault();
            var connectionString = settings.GetValue<string>(AppInsightsConnectionStringSetting, null);
            configuration.ConnectionString = connectionString;
            services.AddSingleton(new TelemetryClient(configuration));
#endif
            var currentDirectory = Environment.CurrentDirectory;
            services.AddSingleton(c => CreateRecorder(c, "automate-cli"));
            services.AddSingleton(c =>
                new LocalMachineFileRepository(currentDirectory, c.GetRequiredService<IFileSystemReaderWriter>(),
                    c.GetRequiredService<IPersistableFactory>()));
            services.AddSingleton<IPatternRepository>(c => c.GetRequiredService<LocalMachineFileRepository>());
            services.AddSingleton<IToolkitRepository>(c => c.GetRequiredService<LocalMachineFileRepository>());
            services.AddSingleton<IDraftRepository>(c => c.GetRequiredService<LocalMachineFileRepository>());
            services.AddSingleton<ILocalStateRepository>(c => c.GetRequiredService<LocalMachineFileRepository>());
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
            services.AddSingleton<IAssemblyMetadata, CliAssemblyMetadata>();

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
    }
}