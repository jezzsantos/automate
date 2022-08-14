using System;
using System.Collections.Generic;
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

        public CliTestSetup()
        {
            var services = new ServiceCollection();
            Program.PopulateContainerForLocalMachineAndCurrentDirectory(services);
            this.container = new DotNetDependencyContainer(services);
            this.repository = this.container.Resolve<LocalMachineFileRepository>();
        }

        public StandardOutput Output { get; private set; }

        public StandardOutput Error { get; private set; }

        internal PatternDefinition Pattern => Patterns.FirstOrDefault();

        internal List<PatternDefinition> Patterns => this.repository.ListPatterns();

        internal LocalState LocalState => this.repository.GetLocalState();

        public string PatternStoreLocation => this.repository.PatternLocation;

        internal DraftDefinition Draft => Drafts.FirstOrDefault();

        internal List<DraftDefinition> Drafts => this.repository.ListDrafts();

        internal ToolkitDefinition Toolkit => this.repository.ListToolkits().FirstOrDefault();

        internal IPatternRepository PatternStore => this.repository;

        public int ExitCode { get; private set; }

        public void ResetRepository()
        {
            this.repository.DestroyAll();
        }

        public void RunCommand(string arguments)
        {
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
                                Output = new StandardOutput(string.Empty);
                                Error = new StandardOutput(string.Empty);
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
                                Output = new StandardOutput(outputStream.ReadToEnd());
                                Error = new StandardOutput(errorStream.ReadToEnd());
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
}