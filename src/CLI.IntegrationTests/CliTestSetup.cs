using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using Automate.Authoring.Domain;
using Automate.Authoring.Infrastructure;
using Automate.CLI;
using Automate.CLI.Extensions;
using Automate.CLI.Infrastructure;
using Automate.Common;
using Automate.Common.Domain;
using Automate.Common.Extensions;
using Automate.Runtime.Domain;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Xunit;

namespace CLI.IntegrationTests
{
    [CollectionDefinition("CLI", DisableParallelization = true)]
    public class AllCliTests : ICollectionFixture<CliTestSetup>
    {
    }

    [UsedImplicitly]
    public class CliTestSetup : IDisposable
    {
        private readonly IDependencyContainer container;
        private readonly LocalMachineFileRepository repository;
        private readonly TestRecorder testRecorder;

        public CliTestSetup()
        {
            var services = new ServiceCollection();
            services.AddSingleton<ILoggerFactory>(new LoggerFactory());
            Program.PopulateContainerForLocalMachineAndCurrentDirectory(null, services);
            this.testRecorder = new TestRecorder();
            services.AddSingleton<IRecorder>(this.testRecorder);
            var serviceProvider = services.BuildServiceProvider();
            this.container = new DotNetDependencyContainer(serviceProvider);
            this.repository = serviceProvider.GetRequiredService<LocalMachineFileRepository>();
        }

        public StandardResult Value { get; private set; }

        public StandardResult Error { get; private set; }

        internal PatternDefinition Pattern => Patterns.FirstOrDefault();

        internal List<PatternDefinition> Patterns => this.repository.ListPatterns();

        internal LocalState LocalState => this.repository.GetLocalState();

        public string PatternStoreLocation => this.repository.PatternLocation;

        internal DraftDefinition Draft => Drafts.FirstOrDefault();

        internal List<DraftDefinition> Drafts => this.repository.ListDrafts();

        internal ToolkitDefinition Toolkit => this.repository.ListToolkits().FirstOrDefault();

        internal IPatternRepository PatternStore => this.repository;

        public int ExitCode { get; private set; }

        public Recordings Recordings => this.testRecorder.Recordings;

        public void ResetRepository()
        {
            this.repository.DestroyAll();
        }

        public void RunCommand(string arguments)
        {
            var error = Error?.Value?.Trim();
            if (error.HasValue())
            {
                throw new Exception($"Previous call to {nameof(RunCommand)}() failed with error: {error}");
            }

            var host = Host.CreateDefaultBuilder()
                .ConfigureLogging((_, logging) => { logging.ClearProviders(); })
                .Build();
            host.Start();

            var lifetime = host.Services.GetRequiredService<IHostApplicationLifetime>();

            using (var errorStream = new MemoryStream())
            {
                using (var outputStream = new MemoryStream())
                {
                    using (var errorWriter = new StreamWriter(errorStream))
                    {
                        errorWriter.AutoFlush = true;
                        using (var outputWriter = new StreamWriter(outputStream))
                        {
                            outputWriter.AutoFlush = true;

                            var standardOutput = Console.Out;
                            var standardError = Console.Error;
                            Console.SetError(errorWriter);
                            Console.SetOut(outputWriter);

                            try
                            {
                                this.testRecorder.Reset();
                                Value = new StandardResult(string.Empty);
                                Error = new StandardResult(string.Empty);
                                var exitCode = 0;

                                try
                                {
                                    exitCode = CommandLineApi.Execute(this.container,
                                        arguments.SplitToCommandLineArgs());
                                }
                                catch (Exception ex)
                                {
                                    Console.Error.WriteLine(ex.ToString());
                                }

                                ExitCode = exitCode;
                                Value = new StandardResult(outputStream.ReadToEnd());
                                Error = new StandardResult(errorStream.ReadToEnd());
                            }
                            finally
                            {
                                Console.SetOut(standardOutput);
                                Console.SetError(standardError);
                                lifetime.StopApplication();
                                host.WaitForShutdownAsync().GetAwaiter().GetResult();
                                host.StopAsync().GetAwaiter().GetResult();
                                host.Dispose();
                            }
                        }
                    }
                }
            }
        }

        public void Reset()
        {
            Error = null;
            Value = null;
            ExitCode = 0;
        }

        public void Dispose()
        {
        }
    }

    internal static class CommandLineExtensions
    {
        public static string[] SplitToCommandLineArgs(this string commandLine)
        {
            if (!commandLine.HasValue())
            {
                return Array.Empty<string>();
            }

            return Parse().ToArray();

            IEnumerable<string> Parse()
            {
                var result = new StringBuilder();

                var quoted = false;
                var escaped = false;
                var started = false;
                var allowCaret = false;
                for (var index = 0; index < commandLine.Length; index++)
                {
                    var character = commandLine[index];
                    if (character == '^' && !quoted)
                    {
                        if (allowCaret)
                        {
                            result.Append(character);
                            started = true;
                            escaped = false;
                            allowCaret = false;
                        }
                        else if (index + 1 < commandLine.Length && commandLine[index + 1] == '^')
                        {
                            allowCaret = true;
                        }
                        else if (index + 1 == commandLine.Length)
                        {
                            result.Append(character);
                            started = true;
                            escaped = false;
                        }
                    }
                    else if (escaped)
                    {
                        result.Append(character);
                        started = true;
                        escaped = false;
                    }
                    else if (character == '"')
                    {
                        quoted = !quoted;
                        started = true;
                    }
                    else if (character == '\\' && index + 1 < commandLine.Length && commandLine[index + 1] == '"')
                    {
                        escaped = true;
                    }
                    else if (character == ' ' && !quoted)
                    {
                        if (started)
                        {
                            yield return result.ToString();
                        }
                        result.Clear();
                        started = false;
                    }
                    else
                    {
                        result.Append(character);
                        started = true;
                    }
                }

                if (started)
                {
                    yield return result.ToString();
                }
            }
        }
    }

    public class TestRecorder : IRecorder
    {
        public TestRecorder()
        {
            Recordings = new Recordings();
        }

        public Recordings Recordings { get; }

        public void Reset()
        {
            Recordings.Reset();
        }

        public void EnableReporting(string machineId, string sessionId)
        {
            Recordings.EnableReporting(machineId, sessionId);
        }

        public (string MachineId, string SessionId) GetReportingIds()
        {
            return Recordings.GetReportingIds();
        }

        public void Count(string eventName, Dictionary<string, string> context = null)
        {
            Recordings.Measurements.Add(new TestMeasurement(eventName, Recordings.MachineId, Recordings.SessionId));
        }

        public void Crash(CrashLevel level, Exception exception, string messageTemplate, params object[] args)
        {
            Recordings.Crashes.Add(new TestCrash(level, exception, messageTemplate, args));
        }

        public void Trace(LogLevel level, string messageTemplate, params object[] args)
        {
            Recordings.Traces.Add(new TestTrace(level, messageTemplate, args));
        }
    }

    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    [SuppressMessage("ReSharper", "CollectionNeverQueried.Global")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public class Recordings
    {
        public List<TestMeasurement> Measurements { get; } = new();

        public bool IsReportingEnabled { get; set; }

        public List<TestCrash> Crashes { get; } = new();

        public List<TestTrace> Traces { get; } = new();

        public string MachineId { get; set; }

        public string SessionId { get; set; }

        public void Reset()
        {
            MachineId = null;
            SessionId = null;
            IsReportingEnabled = false;
            Measurements.Clear();
            Crashes.Clear();
            Traces.Clear();
        }

        public void EnableReporting(string machineId, string sessionId)
        {
            IsReportingEnabled = true;
            MachineId = machineId;
            SessionId = sessionId;
        }

        public (string MachineId, string SessionId) GetReportingIds()
        {
            return (MachineId, SessionId);
        }
    }

    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public class TestCrash
    {
        public TestCrash(CrashLevel level, Exception exception, string messageTemplate, object[] args)
        {
            Level = level;
            Exception = exception;
            MessageTemplate = messageTemplate;
            Args = args;
        }

        public object[] Args { get; }

        public string MessageTemplate { get; }

        public CrashLevel Level { get; }

        public Exception Exception { get; }
    }

    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public class TestTrace
    {
        public TestTrace(LogLevel level, string messageTemplate, object[] args)
        {
            Level = level;
            MessageTemplate = messageTemplate;
            Args = args;
        }

        public object[] Args { get; }

        public string MessageTemplate { get; }

        public LogLevel Level { get; }
    }

    public class TestMeasurement
    {
        public TestMeasurement(string eventName, string machineId, string sessionId)
        {
            EventName = eventName;
            MachineId = machineId;
            SessionId = sessionId;
        }

        public string EventName { get; }

        public string MachineId { get; }

        public string SessionId { get; }
    }
}