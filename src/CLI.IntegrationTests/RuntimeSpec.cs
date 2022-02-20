using System;
using System.IO;
using System.Linq;
using Automate.CLI.Domain;
using Automate.CLI.Extensions;
using Automate.CLI.Infrastructure;
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
            DeleteCodeFolder();
        }

        [Fact]
        public void WhenInstallNoCommands_ThenDisplaysError()
        {
            this.setup.RunCommand($"{CommandLineApi.InstallCommandName}");

            this.setup.Should().DisplayErrorForMissingCommand();
        }

        [Fact]
        public void WhenRunNoCommands_ThenDisplaysError()
        {
            this.setup.RunCommand($"{CommandLineApi.RunCommandName}");

            this.setup.Should().DisplayErrorForMissingCommand();
        }

        [Fact]
        public void WhenUsingNoCommands_ThenDisplaysError()
        {
            this.setup.RunCommand($"{CommandLineApi.UsingCommandName}");

            this.setup.Should().DisplayErrorForMissingArgument(CommandLineApi.UsingCommandName);
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

            this.setup.RunCommand($"{CommandLineApi.InstallCommandName} toolkit {location}");

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
            this.setup.RunCommand($"{CommandLineApi.BuildCommandName} toolkit");

            this.setup.RunCommand($"{CommandLineApi.InstallCommandName} toolkit {locationV2}");

            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayMessage(
                    OutputMessages.CommandLine_Output_InstalledToolkit.FormatTemplate("apattern", "2.0.0"));
        }

        [Fact]
        public void WhenListInstalledToolkitsAndNone_ThenDisplaysNone()
        {
            this.setup.RunCommand($"{CommandLineApi.RunCommandName} list-toolkits");

            this.setup.Should()
                .DisplayMessage(
                    OutputMessages.CommandLine_Output_NoInstalledToolkits);
        }

        [Fact]
        public void WhenListInstalledToolkitsAndOne_ThenDisplaysOne()
        {
            BuildAndInstallToolkit();

            this.setup.RunCommand($"{CommandLineApi.RunCommandName} list-toolkits");

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

            this.setup.RunCommand($"{CommandLineApi.RunCommandName} toolkit apattern");

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

            this.setup.RunCommand($"{CommandLineApi.RunCommandName} list-solutions");

            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayMessage(
                    OutputMessages.CommandLine_Output_NoInstalledSolutions);
        }

        [Fact]
        public void WhenListCreatedSolutionsAndOne_ThenDisplaysOne()
        {
            BuildAndInstallToolkit();
            this.setup.RunCommand($"{CommandLineApi.RunCommandName} toolkit apattern");

            this.setup.RunCommand($"{CommandLineApi.RunCommandName} list-solutions");

            var solution = this.setup.Solutions.Single();

            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayMessage(
                    OutputMessages.CommandLine_Output_InstalledSolutionsListed.FormatTemplate(
                        $"{{\"Name\": \"{solution.PatternName}\", \"ID\": \"{solution.Id}\"}}"));
        }

        [Fact]
        public void WhenConfigureSolutionAndSetPropertyOnPattern_ThenDisplaysSuccess()
        {
            var solution = BuildInstallAndCreateSolution();

            this.setup.RunCommand($"{CommandLineApi.UsingCommandName} {solution.Id} --set \"AProperty1=avalue\"");

            var item = this.setup.Solutions.Single().Model;
            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayMessage(OutputMessages.CommandLine_Output_SolutionConfigured.FormatTemplate("apattern", item.Id));
            item.Properties["AProperty1"].Value.Should().Be("avalue");
        }

        [Fact]
        public void WhenConfigureSolutionAndSetPropertyOnElement_ThenDisplaysSuccess()
        {
            var solution = BuildInstallAndCreateSolution();

            this.setup.RunCommand($"{CommandLineApi.UsingCommandName} {solution.Id} --add {{AnElement1}} --set \"AProperty3=B\"");

            var item = this.setup.Solutions.Single().Model.Properties["AnElement1"];
            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayMessage(OutputMessages.CommandLine_Output_SolutionConfigured.FormatTemplate("AnElement1", item.Id));
            item.Properties["AProperty3"].Value.Should().Be("B");
        }

        [Fact]
        public void WhenConfigureSolutionAndSetPropertyOnCollection_ThenDisplaysSuccess()
        {
            var solution = BuildInstallAndCreateSolution();

            this.setup.RunCommand($"{CommandLineApi.UsingCommandName} {solution.Id} --add-one-to {{ACollection2}}");

            var item = this.setup.Solutions.Single().Model.Properties["ACollection2"].Items.Single();
            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayMessage(OutputMessages.CommandLine_Output_SolutionConfigured.FormatTemplate("ACollection2", item.Id));
            item.Properties["AProperty4"].Value.Should().Be("adefaultvalue4");
        }

        [Fact]
        public void WhenViewConfiguration_ThenDisplaysConfiguration()
        {
            var solution = BuildInstallAndCreateSolution();
            this.setup.RunCommand($"{CommandLineApi.UsingCommandName} {solution.Id} --set \"AProperty1=avalue1\"");
            this.setup.RunCommand(
                $"{CommandLineApi.UsingCommandName} {solution.Id} --add {{AnElement1}} --and-set \"AProperty3=A\"");
            this.setup.RunCommand($"{CommandLineApi.UsingCommandName} {solution.Id} --add-one-to {{ACollection2}}");

            this.setup.RunCommand($"{CommandLineApi.UsingCommandName} {solution.Id} --view-configuration");

            solution = this.setup.Solutions.Single();
            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayMessage(OutputMessages.CommandLine_Output_SolutionConfiguration.FormatTemplate(
                    new
                    {
                        id = solution.Model.Id,
                        a_property1 = "avalue1",
                        an_element1 = new
                        {
                            id = solution.Model.Properties["AnElement1"].Id,
                            a_property3 = "A",
                            a_collection1 = new
                            {
                                id = solution.Model.Properties["AnElement1"].Properties["ACollection1"].Id
                            }
                        },
                        a_collection2 = new
                        {
                            id = solution.Model.Properties["ACollection2"].Id,
                            items = new[]
                            {
                                new
                                {
                                    id = solution.Model.Properties["ACollection2"].Items.Single().Id,
                                    a_property4 = "adefaultvalue4"
                                }
                            }
                        }
                    }.ToJson<dynamic>()));
        }

        [Fact]
        public void WhenValidateAndErrors_ThenDisplaysErrors()
        {
            var solution = BuildInstallAndCreateSolution();

            this.setup.RunCommand($"{CommandLineApi.ValidateCommandName} {solution.Id}");

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

        [Fact]
        public void WhenValidateAndNoErrors_ThenDisplaysSuccess()
        {
            var solution = BuildInstallAndCreateSolution();
            this.setup.RunCommand($"{CommandLineApi.UsingCommandName} {solution.Id} --set \"AProperty1=avalue1\"");
            this.setup.RunCommand(
                $"{CommandLineApi.UsingCommandName} {solution.Id} --add {{AnElement1}} --and-set \"AProperty3=A\"");
            this.setup.RunCommand($"{CommandLineApi.UsingCommandName} {solution.Id} --add-one-to {{ACollection2}}");

            this.setup.RunCommand($"{CommandLineApi.ValidateCommandName} {solution.Id}");

            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayMessage(
                    OutputMessages.CommandLine_Output_SolutionValidationSuccess);
        }

        [Fact]
        public void WhenExecuteLaunchPointAndHasValidationErrors_ThenDisplaysValidations()
        {
            var solution = BuildInstallAndCreateSolution();
            this.setup.RunCommand($"{CommandLineApi.ExecuteCommandName} {solution.Id} --command alaunchpoint");

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

        [Fact]
        public void WhenExecuteLaunchPoint_ThenDisplaysSuccess()
        {
            var testDirectory = Environment.CurrentDirectory;
            var solution = BuildInstallAndCreateSolution();
            this.setup.RunCommand($"{CommandLineApi.UsingCommandName} {solution.Id} --set \"AProperty1=avalue1\"");
            this.setup.RunCommand(
                $"{CommandLineApi.UsingCommandName} {solution.Id} --add {{AnElement1}} --and-set \"AProperty3=B\"");
            this.setup.RunCommand($"{CommandLineApi.UsingCommandName} {solution.Id} --add-one-to {{ACollection2}}");

            this.setup.RunCommand($"{CommandLineApi.ExecuteCommandName} {solution.Id} --command alaunchpoint");

            var artifactLink = this.setup.Solutions.Single().Model.ArtifactLinks.First().Path;
            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayMessage(OutputMessages.CommandLine_Output_CommandExecuted.FormatTemplate("alaunchpoint",
                    "* " + DomainMessages.CodeTemplateCommand_Log_GeneratedFile.Format("Bfile.cs",
                        Path.Combine(testDirectory, @"code\Bfile.cs")) +
                    "\r\n"));
            artifactLink.Should()
                .Be(Path.Combine(testDirectory, @"code\Bfile.cs"));
        }

        private static void DeleteCodeFolder()
        {
            var directory = new DirectoryInfo(Path.Combine(Environment.CurrentDirectory, "code"));
            if (directory.Exists)
            {
                directory.Delete(true);
            }
        }

        private SolutionDefinition BuildInstallAndCreateSolution()
        {
            BuildAndInstallToolkit();
            this.setup.RunCommand($"{CommandLineApi.RunCommandName} toolkit apattern");

            return this.setup.Solutions.Single();
        }

        private string BuildAndInstallToolkit()
        {
            var desktopFolder = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            var location = Path.Combine(desktopFolder, "apattern_1.0.toolkit");
            this.setup.RunCommand($"{CommandLineApi.CreateCommandName} pattern apattern");
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-codetemplate \"Assets/CodeTemplates/code1.code\"");
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-codetemplate-command \"CodeTemplate1\" --withpath \"~/code/{{{{an_element1.a_property3}}}}file.cs\"");
            var commandId = this.setup.Patterns.Single().Automation.Single().Id;
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-command-launchpoint {commandId} --name alaunchpoint");
            this.setup.RunCommand($"{CommandLineApi.EditCommandName} add-attribute AProperty1 --isrequired");
            this.setup.RunCommand($"{CommandLineApi.EditCommandName} add-attribute AProperty2 --typeis int");
            this.setup.RunCommand($"{CommandLineApi.EditCommandName} add-element AnElement1");
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-attribute AProperty3 --aschildof {{apattern.AnElement1}} --isoneof \"A;B;C\"");
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-collection ACollection1 --aschildof {{apattern.AnElement1}}");
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-collection ACollection2 --aschildof {{apattern}} --ality OneOrMany");
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-attribute AProperty4 --aschildof {{apattern.ACollection2}} --defaultvalueis adefaultvalue4");

            this.setup.RunCommand($"{CommandLineApi.BuildCommandName} toolkit");

            this.setup.RunCommand($"{CommandLineApi.InstallCommandName} toolkit {location}");

            this.setup.Should().DisplayNoError();

            return location;
        }
    }
}