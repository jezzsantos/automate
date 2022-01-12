using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using automate;
using automate.Extensions;
using FluentAssertions;
using Xunit;

namespace CLI.IntegrationTests
{
    [CollectionDefinition("CLI", DisableParallelization = true)]
    public class AllCliTests : ICollectionFixture<CliTestSetup>
    {
    }

    public class CliTestSetup : IDisposable
    {
        private const string ProcessName = "automate";
        private readonly JsonFilePatternRepository repository;
        private StandardOutput error;
        private StandardOutput output;
        private Process process;

        public CliTestSetup()
        {
            Process.GetProcessesByName(ProcessName).ToList().ForEach(p => p.Kill());
            this.repository = new JsonFilePatternRepository(Environment.CurrentDirectory);
            ResetRepository();
            ResetCommandLineSession();
        }

        public StandardOutput Output
        {
            get
            {
                if (this.process.NotExists())
                {
                    return new StandardOutput(string.Empty);
                }

                if (!this.output.Exists())
                {
                    var standardOutput = this.process.StandardOutput.ReadToEnd();
                    this.output = new StandardOutput(standardOutput);
                }

                return this.output;
            }
        }

        public StandardOutput Error
        {
            get
            {
                if (this.process.NotExists())
                {
                    return new StandardOutput(string.Empty);
                }

                if (!this.error.Exists())
                {
                    var standardError = this.process.StandardError.ReadToEnd();
                    this.error = new StandardOutput(standardError);
                }

                return this.error;
            }
        }

        internal List<PatternMetaModel> Patterns => this.repository.List();

        internal PatternState PatternState => this.repository.GetState();

        public string Location => this.repository.Location;

        public void ResetRepository()
        {
            this.repository.DestroyAll();
        }

        public void RunCommand(string arguments)
        {
            KillProcess();
            ResetCommandLineSession();
            this.process = Process.Start(new ProcessStartInfo
            {
                FileName = Path.Combine(Environment.CurrentDirectory, ProcessName) + ".exe",
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            });
            if (this.process.Exists())
            {
                this.process.WaitForExit();
            }
        }

        public void Dispose()
        {
            KillProcess();
        }

        private void ResetCommandLineSession()
        {
            this.output = null;
            this.error = null;
        }

        private void KillProcess()
        {
            if (this.process.Exists())
            {
                if (!this.process.HasExited)
                {
                    this.process.Kill();
                }
            }
        }
    }

    [Trait("Category", "Integration")] [Collection("CLI")]
    public class AuthoringSpec
    {
        private readonly CliTestSetup setup;

        public AuthoringSpec(CliTestSetup setup)
        {
            this.setup = setup;
            this.setup.ResetRepository();
        }

        [Fact]
        public void WhenNoCommands_ThenDisplaysError()
        {
            this.setup.RunCommand("");

            this.setup.Should().DisplayErrorForMissingCommand();
        }

        [Fact]
        public void WhenCreateWithNameAndExists_ThenDisplaysError()
        {
            this.setup.RunCommand("create aname");
            this.setup.RunCommand("create aname");

            this.setup.Should().DisplayError(ExceptionMessages.PatternStore_FoundNamed, "aname");
        }

        [Fact]
        public void WhenCreateWithName_ThenCreatesNewPattern()
        {
            this.setup.RunCommand("create aname");

            this.setup.Should().DisplayNoError();
            this.setup.Patterns.Single().Name.Should().Be("aname");
            this.setup.PatternState.Current.Should().Be(this.setup.Patterns.Single().Id);
        }

        [Fact]
        public void WhenCreateMultipleTimes_ThenCreatesNewPatterns()
        {
            this.setup.RunCommand("create aname1");
            this.setup.RunCommand("create aname2");
            this.setup.RunCommand("create aname3");

            this.setup.Should().DisplayNoError();
            this.setup.Patterns.Should().Contain(x => x.Name == "aname1");
            this.setup.Patterns.Should().Contain(x => x.Name == "aname2");
            this.setup.Patterns.Should().Contain(x => x.Name == "aname3");
            this.setup.PatternState.Current.Should().Be(this.setup.Patterns.Find(x => x.Name == "aname3")!.Id);
        }

        [Fact]
        public void WhenUseWithoutName_ThenDisplaysError()
        {
            this.setup.RunCommand("use");

            this.setup.Should().DisplayErrorForMissingArgument("use");
        }

        [Fact]
        public void WhenUseWithNameAndNotExists_ThenDisplaysError()
        {
            this.setup.RunCommand("use aname");

            this.setup.Should()
                .DisplayError(ExceptionMessages.PatternStore_NotFoundAtLocationWithId, "aname", this.setup.Location);
        }

        [Fact]
        public void WhenUseWithNameAndExists_ThenUsesPattern()
        {
            this.setup.RunCommand("create aname");
            this.setup.RunCommand("use aname");

            this.setup.Should().DisplayNoError();
            this.setup.PatternState.Current.Should().Be(this.setup.Patterns.Single().Id);
        }

        [Fact]
        public void WhenAddCodeTemplateAndNoCurrentPattern_ThenDisplaysError()
        {
            var template = Path.Combine(Environment.CurrentDirectory, "Assets/CodeTemplates/code1.code");

            this.setup.RunCommand($"add-codetemplate {template}");

            this.setup.Should()
                .DisplayError(ExceptionMessages.PatternApplication_NoCurrentPattern);
        }

        [Fact]
        public void WhenAddCodeTemplateAndFileMissing_ThenDisplaysHelp()
        {
            this.setup.RunCommand("create aname");
            this.setup.RunCommand("add-codetemplate");

            this.setup.Should().DisplayErrorForMissingArgument("add-codetemplate");
        }

        [Fact]
        public void WhenAddCodeTemplateAndUnnamed_ThenAddsCodeTemplateWithDefaultName()
        {
            var template = Path.Combine(Environment.CurrentDirectory, "Assets/CodeTemplates/code1.code");

            this.setup.RunCommand("create aname");
            this.setup.RunCommand($"add-codetemplate \"{template}\"");

            this.setup.Should().DisplayNoError();
            this.setup.Patterns.Single().CodeTemplates.First().Name.Should().Be("CodeTemplate1");
        }

        [Fact]
        public void WhenAddCodeTemplateAndNamed_ThenAddsCodeTemplate()
        {
            var template = Path.Combine(Environment.CurrentDirectory, "Assets/CodeTemplates/code1.code");

            this.setup.RunCommand("create aname");
            this.setup.RunCommand($"add-codetemplate \"{template}\" --name aname");

            this.setup.Should().DisplayNoError();
            this.setup.Patterns.Single().CodeTemplates.First().Name.Should().Be("aname");
        }

        [Fact]
        public void WhenListCodeTemplatesAndNone_ThenDisplaysNone()
        {
            this.setup.RunCommand("create aname");

            this.setup.RunCommand("list-codetemplates");

            this.setup.Should().DisplayNoError();
            this.setup.Should().DisplayMessage(ExceptionMessages.CommandLine_Output_NoCodeTemplates);
        }
    }
}