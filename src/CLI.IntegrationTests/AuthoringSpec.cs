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
            this.setup.RunCommand($"{Program.AuthoringCommandName}");

            this.setup.Should().DisplayErrorForMissingCommand();
        }

        [Fact]
        public void WhenCreateWithNameAndExists_ThenDisplaysError()
        {
            this.setup.RunCommand($"{Program.AuthoringCommandName} create apattern");
            this.setup.RunCommand($"{Program.AuthoringCommandName} create apattern");

            this.setup.Should().DisplayError(ExceptionMessages.PatternStore_FoundNamed, "apattern");
        }

        [Fact]
        public void WhenCreateWithName_ThenCreatesNewPattern()
        {
            this.setup.RunCommand($"{Program.AuthoringCommandName} create apattern");

            this.setup.Should().DisplayNoError();
            this.setup.Patterns.Single().Name.Should().Be("apattern");
            this.setup.PatternState.Current.Should().Be(this.setup.Patterns.Single().Id);
        }

        [Fact]
        public void WhenCreateMultipleTimes_ThenCreatesNewPatterns()
        {
            this.setup.RunCommand($"{Program.AuthoringCommandName} create apattern1");
            this.setup.RunCommand($"{Program.AuthoringCommandName} create apattern2");
            this.setup.RunCommand($"{Program.AuthoringCommandName} create apattern3");

            this.setup.Should().DisplayNoError();
            this.setup.Patterns.Should().Contain(x => x.Name == "apattern1");
            this.setup.Patterns.Should().Contain(x => x.Name == "apattern2");
            this.setup.Patterns.Should().Contain(x => x.Name == "apattern3");
            this.setup.PatternState.Current.Should().Be(this.setup.Patterns.Find(x => x.Name == "apattern3")!.Id);
        }

        [Fact]
        public void WhenUseWithoutName_ThenDisplaysError()
        {
            this.setup.RunCommand($"{Program.AuthoringCommandName} use");

            this.setup.Should().DisplayErrorForMissingArgument("use");
        }

        [Fact]
        public void WhenUseWithNameAndNotExists_ThenDisplaysError()
        {
            this.setup.RunCommand($"{Program.AuthoringCommandName} use apattern");

            this.setup.Should()
                .DisplayError(ExceptionMessages.PatternStore_NotFoundAtLocationWithId, "apattern", this.setup.Location);
        }

        [Fact]
        public void WhenUseWithNameAndExists_ThenUsesPattern()
        {
            this.setup.RunCommand($"{Program.AuthoringCommandName} create apattern");
            this.setup.RunCommand($"{Program.AuthoringCommandName} use apattern");

            this.setup.Should().DisplayNoError();
            this.setup.PatternState.Current.Should().Be(this.setup.Patterns.Single().Id);
        }

        [Fact]
        public void WhenAddCodeTemplateAndNoCurrentPattern_ThenDisplaysError()
        {
            var template = Path.Combine(Environment.CurrentDirectory, "Assets/CodeTemplates/code1.code");

            this.setup.RunCommand($"{Program.AuthoringCommandName} add-codetemplate {template}");

            this.setup.Should()
                .DisplayError(ExceptionMessages.PatternApplication_NoCurrentPattern);
        }

        [Fact]
        public void WhenAddCodeTemplateAndFileMissing_ThenDisplaysHelp()
        {
            this.setup.RunCommand($"{Program.AuthoringCommandName} create apattern");
            this.setup.RunCommand($"{Program.AuthoringCommandName} add-codetemplate");

            this.setup.Should().DisplayErrorForMissingArgument("add-codetemplate");
        }

        [Fact]
        public void WhenAddCodeTemplateAndUnnamed_ThenAddsCodeTemplateWithDefaultName()
        {
            var template = Path.Combine(Environment.CurrentDirectory, "Assets/CodeTemplates/code1.code");

            this.setup.RunCommand($"{Program.AuthoringCommandName} create apattern");
            this.setup.RunCommand($"{Program.AuthoringCommandName} add-codetemplate \"{template}\"");

            this.setup.Should().DisplayNoError();
            this.setup.Patterns.Single().CodeTemplates.First().Name.Should().Be("CodeTemplate1");
        }

        [Fact]
        public void WhenAddCodeTemplateAndNamed_ThenAddsCodeTemplate()
        {
            var template = Path.Combine(Environment.CurrentDirectory, "Assets/CodeTemplates/code1.code");

            this.setup.RunCommand($"{Program.AuthoringCommandName} create apattern");
            this.setup.RunCommand(
                $"{Program.AuthoringCommandName} add-codetemplate \"{template}\" --name atemplatename");

            this.setup.Should().DisplayNoError();
            this.setup.Patterns.Single().CodeTemplates.First().Name.Should().Be("atemplatename");
        }

        [Fact]
        public void WhenListCodeTemplatesAndNone_ThenDisplaysNone()
        {
            this.setup.RunCommand($"{Program.AuthoringCommandName} create apattern");

            this.setup.RunCommand($"{Program.AuthoringCommandName} list-codetemplates");

            this.setup.Should().DisplayNoError();
            this.setup.Should().DisplayMessage(OutputMessages.CommandLine_Output_NoCodeTemplates);
        }

        [Fact]
        public void WhenAddAttributeAndNoCurrentPattern_ThenDisplaysError()
        {
            this.setup.RunCommand($"{Program.AuthoringCommandName} add-attribute anattribute");

            this.setup.Should()
                .DisplayError(ExceptionMessages.PatternApplication_NoCurrentPattern);
        }

        [Fact]
        public void WhenAddAttribute_ThenAddsAttribute()
        {
            this.setup.RunCommand($"{Program.AuthoringCommandName} create apattern");
            this.setup.RunCommand($"{Program.AuthoringCommandName} add-attribute anattribute");

            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayMessage(
                    OutputMessages.CommandLine_Output_AttributeAdded.FormatTemplate("anattribute",
                        this.setup.Patterns.Single().Id));
            this.setup.Patterns.Single().Attributes.Single().IsRequired.Should().BeFalse();
        }

        [Fact]
        public void WhenAddAttributeWithIsRequired_ThenAddsAttribute()
        {
            this.setup.RunCommand($"{Program.AuthoringCommandName} create apattern");
            this.setup.RunCommand($"{Program.AuthoringCommandName} add-attribute anattribute --isrequired");

            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayMessage(
                    OutputMessages.CommandLine_Output_AttributeAdded.FormatTemplate("anattribute",
                        this.setup.Patterns.Single().Id));
            this.setup.Patterns.Single().Attributes.Single().IsRequired.Should().BeTrue();
        }

        [Fact]
        public void WhenAddAttributeWithIsRequiredFalse_ThenAddsAttribute()
        {
            this.setup.RunCommand($"{Program.AuthoringCommandName} create apattern");
            this.setup.RunCommand($"{Program.AuthoringCommandName} add-attribute anattribute --isrequired false");

            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayMessage(
                    OutputMessages.CommandLine_Output_AttributeAdded.FormatTemplate("anattribute",
                        this.setup.Patterns.Single().Id));
            this.setup.Patterns.Single().Attributes.Single().IsRequired.Should().BeFalse();
        }

        [Fact]
        public void WhenAddAttributeAsChildOfDeepElement_ThenAddsAttribute()
        {
            this.setup.RunCommand($"{Program.AuthoringCommandName} create apattern");
            this.setup.RunCommand($"{Program.AuthoringCommandName} add-element anelementname1");
            this.setup.RunCommand(
                $"{Program.AuthoringCommandName} add-element anelementname2 --aschildof {{apattern.anelementname1}}");
            this.setup.RunCommand(
                $"{Program.AuthoringCommandName} add-attribute anattribute --aschildof {{apattern.anelementname1.anelementname2}}");

            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayMessage(
                    OutputMessages.CommandLine_Output_AttributeAdded.FormatTemplate("anattribute",
                        this.setup.Patterns.Single().Elements.Single().Elements.Single().Id));
        }

        [Fact]
        public void WhenAddElementAndNoCurrentPattern_ThenDisplaysError()
        {
            this.setup.RunCommand($"{Program.AuthoringCommandName} add-element anelement");

            this.setup.Should()
                .DisplayError(ExceptionMessages.PatternApplication_NoCurrentPattern);
        }

        [Fact]
        public void WhenAddElement_ThenAddsAttribute()
        {
            this.setup.RunCommand($"{Program.AuthoringCommandName} create apattern");
            this.setup.RunCommand($"{Program.AuthoringCommandName} add-element anelement");

            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayMessage(
                    OutputMessages.CommandLine_Output_ElementAdded.FormatTemplate("anelement",
                        this.setup.Patterns.Single().Id));
        }

        [Fact]
        public void WhenAddCollectionAndNoCurrentPattern_ThenDisplaysError()
        {
            this.setup.RunCommand($"{Program.AuthoringCommandName} add-collection acollection");

            this.setup.Should()
                .DisplayError(ExceptionMessages.PatternApplication_NoCurrentPattern);
        }

        [Fact]
        public void WhenAddCollection_ThenAddsAttribute()
        {
            this.setup.RunCommand($"{Program.AuthoringCommandName} create apattern");
            this.setup.RunCommand(
                $"{Program.AuthoringCommandName} add-collection acollection --displayedas adisplayname --describedas adescription");

            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayMessage(
                    OutputMessages.CommandLine_Output_CollectionAdded.FormatTemplate("acollection",
                        this.setup.Patterns.Single().Id));
        }

        [Fact]
        public void WhenAddCodeTemplateCommand_ThenAddsCommand()
        {
            this.setup.RunCommand($"{Program.AuthoringCommandName} create apattern");
            this.setup.RunCommand(
                $"{Program.AuthoringCommandName} add-codetemplate-command \"CodeTemplate1\" --withpath ~/afilepath");

            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayMessage(
                    OutputMessages.CommandLine_Output_CodeTemplateCommandAdded.FormatTemplate("CodeTemplate1",
                        this.setup.Patterns.Single().Automation.Single().Id));
        }

        [Fact]
        public void WhenAddCommandLaunchPoint_ThenAddsLaunchPoint()
        {
            this.setup.RunCommand($"{Program.AuthoringCommandName} create apattern");
            this.setup.RunCommand(
                $"{Program.AuthoringCommandName} add-codetemplate-command \"CodeTemplate1\" --withpath ~/afilepath");
            var commandId = this.setup.Patterns.Single().Automation.Single().Id;

            this.setup.RunCommand(
                $"{Program.AuthoringCommandName} add-command-launchpoint {commandId} --name alaunchpoint");

            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayMessage(
                    OutputMessages.CommandLine_Output_LaunchPointAdded.FormatTemplate("alaunchpoint"));
        }

        [Fact]
        public void WhenBuildToolkit_ThenBuildsToolkitOnDesktop()
        {
            var template = Path.Combine(Environment.CurrentDirectory, "Assets/CodeTemplates/code1.code");

            this.setup.RunCommand($"{Program.AuthoringCommandName} create apattern");
            this.setup.RunCommand(
                $"{Program.AuthoringCommandName} add-codetemplate \"{template}\" --name atemplatename");

            this.setup.RunCommand($"{Program.AuthoringCommandName} build");

            var desktopFolder = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            var location = Path.Combine(desktopFolder, "apattern_1.0.toolkit");
            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayMessage(
                    OutputMessages.CommandLine_Output_BuiltToolkit.FormatTemplate("apattern", "1.0.0", location));
        }
    }
}