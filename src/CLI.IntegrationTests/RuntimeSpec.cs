using System;
using System.IO;
using System.Linq;
using automate;
using automate.Domain;
using automate.Extensions;
using automate.Infrastructure;
using FluentAssertions;
using Xunit;

namespace CLI.IntegrationTests
{
    [Trait("Category", "Integration")] [Collection("CLI")]
    public class RuntimeSpec
    {
        private readonly CliTestSetup setup;

        public RuntimeSpec(CliTestSetup setup)
        {
            this.setup = setup;
            this.setup.ResetRepository();
        }

        [Fact]
        public void WhenInstallNoCommands_ThenDisplaysError()
        {
            this.setup.RunCommand($"{Program.InstallCommandName}");

            this.setup.Should().DisplayErrorForMissingCommand();
        }

        [Fact]
        public void WhenRunNoCommands_ThenDisplaysError()
        {
            this.setup.RunCommand($"{Program.RunCommandName}");

            this.setup.Should().DisplayErrorForMissingCommand();
        }

        [Fact]
        public void WhenUsingNoCommands_ThenDisplaysError()
        {
            this.setup.RunCommand($"{Program.UsingCommandName}");

            this.setup.Should().DisplayErrorForMissingArgument(Program.UsingCommandName);
        }

        [Fact]
        public void WhenInstallToolkit_ThenInstallsToolkit()
        {
            BuildAndInstallToolkit();

            this.setup.Should()
                .DisplayMessage(
                    OutputMessages.CommandLine_Output_InstalledToolkit.FormatTemplate("apattern", "1.0.0"));
        }

        [Fact]
        public void WhenInstallToolkitWithSameToolkitSameVersionAgain_ThenInstallsToolkit()
        {
            var location = BuildAndInstallToolkit();

            this.setup.RunCommand($"{Program.InstallCommandName} toolkit {location}");

            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayMessage(
                    OutputMessages.CommandLine_Output_InstalledToolkit.FormatTemplate("apattern", "1.0.0"));
        }

        [Fact]
        public void WhenInstallToolkitWithSameToolkitNextVersionAgain_ThenInstallsToolkit()
        {
            BuildAndInstallToolkit();
            var desktopFolder = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            var locationV2 = Path.Combine(desktopFolder, "apattern_2.0.toolkit");
            this.setup.RunCommand($"{Program.BuildCommandName} toolkit");

            this.setup.RunCommand($"{Program.InstallCommandName} toolkit {locationV2}");

            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayMessage(
                    OutputMessages.CommandLine_Output_InstalledToolkit.FormatTemplate("apattern", "2.0.0"));
        }

        [Fact]
        public void WhenListInstalledToolkitsAndNone_ThenDisplaysNone()
        {
            this.setup.RunCommand($"{Program.RunCommandName} list-toolkits");

            this.setup.Should()
                .DisplayMessage(
                    OutputMessages.CommandLine_Output_NoInstalledToolkits);
        }

        [Fact]
        public void WhenListInstalledToolkitsAndOne_ThenDisplaysOne()
        {
            BuildAndInstallToolkit();

            this.setup.RunCommand($"{Program.RunCommandName} list-toolkits");

            var toolkit = this.setup.Toolkits.Single();

            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayMessage(
                    OutputMessages.CommandLine_Output_InstalledToolkitsListed.FormatTemplate(
                        $"{{\"Name\": \"{toolkit.PatternName}\", \"ID\": \"{toolkit.Id}\"}}"));
        }

        [Fact]
        public void WhenCreateSolution_ThenCreatesSolution()
        {
            BuildAndInstallToolkit();

            this.setup.RunCommand($"{Program.RunCommandName} toolkit apattern");

            var solution = this.setup.Solutions.Single();

            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayMessage(
                    OutputMessages.CommandLine_Output_CreateSolutionFromToolkit.FormatTemplate(solution.PatternName,
                        solution.Id));
        }

        [Fact]
        public void WhenListInstalledSolutionsAndNone_ThenDisplaysNone()
        {
            BuildAndInstallToolkit();

            this.setup.RunCommand($"{Program.RunCommandName} list-solutions");

            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayMessage(
                    OutputMessages.CommandLine_Output_NoInstalledSolutions);
        }

        [Fact]
        public void WhenListCreatedSolutionsAndOne_ThenDisplaysOne()
        {
            BuildAndInstallToolkit();
            this.setup.RunCommand($"{Program.RunCommandName} toolkit apattern");

            this.setup.RunCommand($"{Program.RunCommandName} list-solutions");

            var solution = this.setup.Solutions.Single();

            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayMessage(
                    OutputMessages.CommandLine_Output_InstalledSolutionsListed.FormatTemplate(
                        $"{{\"Name\": \"{solution.PatternName}\", \"ID\": \"{solution.Id}\"}}"));
        }

        [Fact]
        public void WhenConfigureSolutionAndSetProperty_ThenDisplaysSuccess()
        {
            var solution = BuildInstallAndCreateSolution();

            this.setup.RunCommand($"{Program.UsingCommandName} {solution.Id} --set \"AProperty1=avalue\"");

            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayMessage(OutputMessages.CommandLine_Output_SolutionConfigured);
            this.setup.Solutions.Single().Model.Properties["AProperty1"].Value.Should().Be("avalue");
        }

        [Fact]
        public void WhenViewConfiguration_ThenDisplaysConfiguration()
        {
            var solution = BuildInstallAndCreateSolution();
            this.setup.RunCommand($"{Program.UsingCommandName} {solution.Id} --set \"AProperty1=avalue1\"");
            this.setup.RunCommand(
                $"{Program.UsingCommandName} {solution.Id} --add {{AnElement1}} --and-set \"AProperty3=A\"");

            this.setup.RunCommand($"{Program.UsingCommandName} {solution.Id} --view-configuration");

            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayMessage(OutputMessages.CommandLine_Output_SolutionConfiguration.FormatTemplate(
                    new
                    {
                        a_property1 = "avalue1",
                        an_element1 = new
                        {
                            a_property3 = "A"
                        },
                        a_collection2 = new { }
                    }.ToJson<dynamic>()));
        }

        [Fact]
        public void WhenValidate_ThenDisplaysErrors()
        {
            var solution = BuildInstallAndCreateSolution();

            this.setup.RunCommand($"{Program.ValidateCommandName} {solution.Id}");

            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayMessage(
                    OutputMessages.CommandLine_Output_SolutionValidationFailed
                        .FormatTemplate(
                            "1. {apattern.AProperty1} requires its value to be set\r\n" +
                            "2. {apattern.AnElement1} requires at least one instance\r\n" +
                            "3. {apattern.ACollection2} requires at least one instance\r\n\r\n"
                        ));
        }

        private SolutionDefinition BuildInstallAndCreateSolution()
        {
            BuildAndInstallToolkit();
            this.setup.RunCommand($"{Program.RunCommandName} toolkit apattern");

            return this.setup.Solutions.Single();
        }

        private string BuildAndInstallToolkit()
        {
            var desktopFolder = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            var location = Path.Combine(desktopFolder, "apattern_1.0.toolkit");
            this.setup.RunCommand($"{Program.CreateCommandName} pattern apattern");
            this.setup.RunCommand($"{Program.EditCommandName} add-attribute AProperty1 --isrequired");
            this.setup.RunCommand($"{Program.EditCommandName} add-attribute AProperty2 --typeis int");
            this.setup.RunCommand($"{Program.EditCommandName} add-element AnElement1");
            this.setup.RunCommand(
                $"{Program.EditCommandName} add-attribute AProperty3 --aschildof {{apattern.AnElement1}} --isoneof \"A;B;C\"");
            this.setup.RunCommand(
                $"{Program.EditCommandName} add-collection ACollection1 --aschildof {{apattern.AnElement}}");
            this.setup.RunCommand(
                $"{Program.EditCommandName} add-collection ACollection2 --aschildof {{apattern}} --ality OneOrMany");
            this.setup.RunCommand($"{Program.BuildCommandName} toolkit");

            this.setup.RunCommand($"{Program.InstallCommandName} toolkit {location}");

            this.setup.Should().DisplayNoError();

            return location;
        }
    }
}