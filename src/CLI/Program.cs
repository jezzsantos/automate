using System;
using System.IO;
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
#endif

namespace Automate.CLI
{
    [UsedImplicitly]
    internal class Program
    {
        private const string LoggingCategory = "automate-cli";
        private const string RollingLogFileLevelSettingName = "Logging:RollingFile:LogLevel:Default";
        private static readonly string LocalLogFilePath = Path.Combine("automate", "automate.log");

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
                    logging.AddFile(LocalLogFilePath, options =>
                    {
                        options.Append = true;
                        options.MinLevel = context.Configuration
                            .GetValue<string>(RollingLogFileLevelSettingName)
                            .ToEnumOrDefault(LogLevel.Information);
                        options.MaxRollingFiles = 1;
                        options.FileSizeLimitBytes = 10 * 1000 * 1000;
                    });
#if TESTINGONLY
                    logging.AddConsole();
#else
                    //We actually only want to send unhandled exceptions, crash reports, and events to AI, not logs
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
                recorder.StartSession(LoggingMessages.Program_StartSession);

                host.Start();

                var lifetime = host.Services.GetRequiredService<IHostApplicationLifetime>();
                var container = new DotNetDependencyContainer(host.Services);
                var result = 0;
                try
                {
                    result = CommandLineApi.Execute(container, args);
                    lifetime.StopApplication();
                    host.WaitForShutdownAsync().GetAwaiter().GetResult();
                }
                catch (Exception ex)
                {
                    recorder.CrashFatal(ex, LoggingMessages.Program_Exception);
                    Console.Error.WriteLine(ex.Message);
                    result = 1;
                }
                finally
                {
                    recorder.EndSession(result == 0, LoggingMessages.Program_EndSession);
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
            services.RegisterApplicationInsightsClient(settings, assemblyMetadata);
#endif
            services.AddSingleton(c => CreateRecorder(c, LoggingCategory));
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
                new LoggingMeasurementReporter(logger));
#else
                new ApplicationInsightsCrashReporter(telemetryClient),
                new ApplicationInsightsMeasurementReporter(telemetryClient, logger));
#endif
            return recorder;
        }
    }
}