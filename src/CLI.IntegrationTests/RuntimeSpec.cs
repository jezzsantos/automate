using System;
using System.IO;
using System.Linq;
using Automate.CLI;
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
            this.setup.RunCommand($"{CommandLineApi.ConfigureCommandName}");

            this.setup.Should().DisplayErrorForMissingCommand();
        }

        [Fact]
        public void WhenInstallToolkit_ThenInstallsToolkit()
        {
            BuildAndInstallToolkit();

            this.setup.Should()
                .DisplayMessage(
                    OutputMessages.CommandLine_Output_InstalledToolkit.FormatTemplate("APattern", "0.1.0"));
        }

        [Fact]
        public void WhenInstallToolkitWithSameToolkitSameVersionAgain_ThenInstallsToolkit()
        {
            var location = BuildAndInstallToolkit();

            this.setup.RunCommand($"{CommandLineApi.InstallCommandName} toolkit {location}");

            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayMessage(
                    OutputMessages.CommandLine_Output_InstalledToolkit.FormatTemplate("APattern", "0.1.0"));
        }

        [Fact]
        public void WhenInstallToolkitWithSameToolkitNextVersionAgain_ThenInstallsToolkit()
        {
            BuildAndInstallToolkit();
            var desktopFolder = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            var locationV2 = Path.Combine(desktopFolder, "APattern_0.2.0.toolkit");
            this.setup.RunCommand($"{CommandLineApi.BuildCommandName} toolkit");

            this.setup.RunCommand($"{CommandLineApi.InstallCommandName} toolkit {locationV2}");

            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayMessage(
                    OutputMessages.CommandLine_Output_InstalledToolkit.FormatTemplate("APattern", "0.2.0"));
        }

        [Fact]
        public void WhenListInstalledToolkitsAndNone_ThenDisplaysNone()
        {
            this.setup.RunCommand($"{CommandLineApi.ListCommandName} toolkits");

            this.setup.Should()
                .DisplayMessage(
                    OutputMessages.CommandLine_Output_NoInstalledToolkits);
        }

        [Fact]
        public void WhenListInstalledToolkitsAndOne_ThenDisplaysOne()
        {
            BuildAndInstallToolkit();

            this.setup.RunCommand($"{CommandLineApi.ListCommandName} toolkits");

            var toolkit = this.setup.Toolkits.Single();

            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayMessage(
                    OutputMessages.CommandLine_Output_InstalledToolkitsListed.FormatTemplate(
                        $"{{\"Name\": \"{toolkit.PatternName}\", \"Version\": \"{toolkit.Version}\", \"ID\": \"{toolkit.Id}\"}}"));
        }

        [Fact]
        public void WhenCreateSolution_ThenCreatesSolution()
        {
            BuildAndInstallToolkit();

            this.setup.RunCommand($"{CommandLineApi.RunCommandName} toolkit APattern");

            var solution = this.setup.Solutions.Single();

            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayMessage(
                    OutputMessages.CommandLine_Output_CreateSolutionFromToolkit.FormatTemplate(solution.Name,
                        solution.Id, solution.PatternName));
        }

        [Fact]
        public void WhenSwitchSolution_ThenSwitchesSolution()
        {
            BuildAndInstallToolkit();

            this.setup.RunCommand($"{CommandLineApi.RunCommandName} toolkit APattern");
            this.setup.RunCommand($"{CommandLineApi.RunCommandName} toolkit APattern");

            var solution1 = this.setup.Solutions.First();

            this.setup.RunCommand($"{CommandLineApi.RunCommandName} switch \"{solution1.Id}\"");

            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayMessage(
                    OutputMessages.CommandLine_Output_SolutionSwitched.FormatTemplate(solution1.Name, solution1.Id));
        }

        [Fact]
        public void WhenListInstalledSolutionsAndNone_ThenDisplaysNone()
        {
            BuildAndInstallToolkit();

            this.setup.RunCommand($"{CommandLineApi.ListCommandName} solutions");

            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayMessage(
                    OutputMessages.CommandLine_Output_NoInstalledSolutions);
        }

        [Fact]
        public void WhenListCreatedSolutionsAndOne_ThenDisplaysOne()
        {
            BuildAndInstallToolkit();
            this.setup.RunCommand($"{CommandLineApi.RunCommandName} toolkit APattern");

            this.setup.RunCommand($"{CommandLineApi.ListCommandName} solutions");

            var solution = this.setup.Solutions.Single();

            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayMessage(
                    OutputMessages.CommandLine_Output_InstalledSolutionsListed.FormatTemplate(
                        $"{{\"Name\": \"{solution.Name}\", \"ID\": \"{solution.Id}\", \"Version\": \"{solution.Toolkit.Version}\"}}"));
        }

        [Fact]
        public void WhenConfigureSolutionAndSetPropertyOnPattern_ThenDisplaysSuccess()
        {
            BuildInstallAndCreateSolution();

            this.setup.RunCommand($"{CommandLineApi.ConfigureCommandName} on {{APattern}} --and-set \"AProperty1=avalue\"");

            var item = this.setup.Solutions.Single().Model;
            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayMessage(OutputMessages.CommandLine_Output_SolutionConfigured.FormatTemplate("APattern", item.Id));
            item.Properties["AProperty1"].Value.Should().Be("avalue");
        }

        [Fact]
        public void WhenConfigureSolutionAndSetPropertyOnNewElement_ThenDisplaysSuccess()
        {
            BuildInstallAndCreateSolution();

            this.setup.RunCommand($"{CommandLineApi.ConfigureCommandName} add {{AnElement1}} --and-set \"AProperty3=B\"");

            var item = this.setup.Solutions.Single().Model.Properties["AnElement1"];
            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayMessage(OutputMessages.CommandLine_Output_SolutionConfigured.FormatTemplate("AnElement1", item.Id));
            item.Properties["AProperty3"].Value.Should().Be("B");
        }

        [Fact]
        public void WhenConfigureSolutionAndSetPropertyOnExistingElement_ThenDisplaysSuccess()
        {
            BuildInstallAndCreateSolution();
            this.setup.RunCommand($"{CommandLineApi.ConfigureCommandName} add {{AnElement1}}");

            this.setup.RunCommand($"{CommandLineApi.ConfigureCommandName} on {{AnElement1}} --and-set \"AProperty3=C\"");

            var item = this.setup.Solutions.Single().Model.Properties["AnElement1"];
            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayMessage(OutputMessages.CommandLine_Output_SolutionConfigured.FormatTemplate("AnElement1", item.Id));
            item.Properties["AProperty3"].Value.Should().Be("C");
        }

        [Fact]
        public void WhenConfigureSolutionAndReSetPropertyOnExistingElement_ThenDisplaysSuccess()
        {
            BuildInstallAndCreateSolution();
            this.setup.RunCommand($"{CommandLineApi.ConfigureCommandName} add {{AnElement1}} --and-set \"AProperty3=B\"");

            this.setup.RunCommand($"{CommandLineApi.ConfigureCommandName} on {{AnElement1}} --and-set \"AProperty3=C\"");

            var item = this.setup.Solutions.Single().Model.Properties["AnElement1"];
            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayMessage(OutputMessages.CommandLine_Output_SolutionConfigured.FormatTemplate("AnElement1", item.Id));
            item.Properties["AProperty3"].Value.Should().Be("C");
        }

        [Fact]
        public void WhenConfigureSolutionAndSetPropertyOnNewCollectionItem_ThenDisplaysSuccess()
        {
            BuildInstallAndCreateSolution();

            this.setup.RunCommand($"{CommandLineApi.ConfigureCommandName} add-one-to {{ACollection2}} --and-set \"AProperty4=anewvalue\"");

            var item = this.setup.Solutions.Single().Model.Properties["ACollection2"].Items.Single();
            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayMessage(OutputMessages.CommandLine_Output_SolutionConfigured.FormatTemplate("ACollection2", item.Id));
            item.Properties["AProperty4"].Value.Should().Be("anewvalue");
        }

        [Fact]
        public void WhenConfigureSolutionAndSetPropertyOnExistingCollectionItem_ThenDisplaysSuccess()
        {
            BuildInstallAndCreateSolution();
            this.setup.RunCommand($"{CommandLineApi.ConfigureCommandName} add-one-to {{ACollection2}} --and-set \"AProperty4=avalue\"");
            var item = this.setup.Solutions.Single().Model.Properties["ACollection2"].Items.Single();

            this.setup.RunCommand($"{CommandLineApi.ConfigureCommandName} on {{ACollection2.{item.Id}}} --and-set \"AProperty4=anewvalue\"");

            item = this.setup.Solutions.Single().Model.Properties["ACollection2"].Items.Single();
            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayMessage(OutputMessages.CommandLine_Output_SolutionConfigured.FormatTemplate("ACollection2", item.Id));
            item.Properties["AProperty4"].Value.Should().Be("anewvalue");
        }

        [Fact]
        public void WhenViewSolution_ThenDisplaysConfiguration()
        {
            BuildInstallAndCreateSolution();
            this.setup.RunCommand($"{CommandLineApi.ConfigureCommandName} on {{APattern}} --and-set \"AProperty1=avalue1\"");
            this.setup.RunCommand(
                $"{CommandLineApi.ConfigureCommandName} add {{AnElement1}} --and-set \"AProperty3=A\"");
            this.setup.RunCommand($"{CommandLineApi.ConfigureCommandName} add-one-to {{ACollection2}}");

            this.setup.RunCommand($"{CommandLineApi.ViewCommandName} solution");

            var solution = this.setup.Solutions.Single();
            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayMessage(OutputMessages.CommandLine_Output_SolutionConfiguration.FormatTemplate(solution.Name, solution.Id,
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
                                    a_property4 = "ADefaultValue4"
                                }
                            }
                        }
                    }.ToJson<dynamic>()));
        }

        [Fact]
        public void WhenViewSolutionWithTodo_ThenDisplaysConfigurationSchemaAndValidation()
        {
            BuildInstallAndCreateSolution();
            this.setup.RunCommand($"{CommandLineApi.ConfigureCommandName} on {{APattern}} --and-set \"AProperty1=avalue1\"");
            this.setup.RunCommand($"{CommandLineApi.ConfigureCommandName} add-one-to {{ACollection2}}");

            this.setup.RunCommand($"{CommandLineApi.ViewCommandName} solution --todo");

            var solution = this.setup.Solutions.Single();
            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayMessage(OutputMessages.CommandLine_Output_SolutionConfiguration.FormatTemplate(solution.Name, solution.Id,
                    new
                    {
                        id = solution.Model.Id,
                        a_property1 = "avalue1",
                        an_element1 = new
                        {
                            id = solution.Model.Properties["AnElement1"].Id
                        },
                        a_collection2 = new
                        {
                            id = solution.Model.Properties["ACollection2"].Id,
                            items = new[]
                            {
                                new
                                {
                                    id = solution.Model.Properties["ACollection2"].Items.Single().Id,
                                    a_property4 = "ADefaultValue4"
                                }
                            }
                        }
                    }.ToJson<dynamic>()));
            var pattern = this.setup.Patterns.Single();
            var codeTemplatePath1 = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "Assets/CodeTemplates/code1.code"));
            var codeTemplatePath2 = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "Assets/CodeTemplates/code2.code"));
            var element1 = this.setup.Patterns.Single().Elements.First();
            this.setup.Should()
                .DisplayMessage(
                    OutputMessages.CommandLine_Output_PatternTree.FormatTemplate(
                        $"- APattern [{pattern.Id}] (root element){Environment.NewLine}" +
                        $"\t- CodeTemplates:{Environment.NewLine}" +
                        $"\t\t- ACodeTemplate1 [{pattern.CodeTemplates.Single().Id}] (file: {codeTemplatePath1}, ext: .code){Environment.NewLine}" +
                        $"\t- Automation:{Environment.NewLine}" +
                        $"\t\t- CodeTemplateCommand1 [{pattern.Automation[0].Id}] (CodeTemplateCommand) (template: {pattern.CodeTemplates.Single().Id}, tearOff: false, path: ~/code/{{{{an_element1.a_property3}}}}namingtest.cs){Environment.NewLine}" +
                        $"\t\t- ALaunchPoint1 [{pattern.Automation[1].Id}] (CommandLaunchPoint) (ids: {pattern.Automation[0].Id}){Environment.NewLine}" +
                        $"\t\t- CodeTemplateCommand3 [{pattern.Automation[2].Id}] (CodeTemplateCommand) (template: {pattern.CodeTemplates.Single().Id}, tearOff: false, path: ~/code/{{{{an_element1.}}}}invalid.cs){Environment.NewLine}" +
                        $"\t\t- ALaunchPoint2 [{pattern.Automation[3].Id}] (CommandLaunchPoint) (ids: {pattern.Automation[0].Id};{pattern.Automation[2].Id}){Environment.NewLine}" +
                        $"\t- Attributes:{Environment.NewLine}" +
                        $"\t\t- AProperty1 (string, required){Environment.NewLine}" +
                        $"\t- Elements:{Environment.NewLine}" +
                        $"\t\t- AnElement1 [{element1.Id}] (element){Environment.NewLine}" +
                        $"\t\t\t- CodeTemplates:{Environment.NewLine}" +
                        $"\t\t\t\t- ACodeTemplate2 [{element1.CodeTemplates.Single().Id}] (file: {codeTemplatePath2}, ext: .code){Environment.NewLine}" +
                        $"\t\t\t- Automation:{Environment.NewLine}" +
                        $"\t\t\t\t- CodeTemplateCommand1 [{element1.Automation.First().Id}] (CodeTemplateCommand) (template: {element1.CodeTemplates.Single().Id}, tearOff: false, path: ~/code/parentsubstitutiontest.cs){Environment.NewLine}" +
                        $"\t\t\t\t- ALaunchPoint3 [{element1.Automation.Last().Id}] (CommandLaunchPoint) (ids: {element1.Automation.First().Id}){Environment.NewLine}" +
                        $"\t\t\t- Attributes:{Environment.NewLine}" +
                        $"\t\t\t\t- AProperty3 (string, oneof: A;B;C){Environment.NewLine}" +
                        $"\t\t\t- Elements:{Environment.NewLine}" +
                        $"\t\t\t\t- ACollection1 [{element1.Elements.First().Id}] (collection){Environment.NewLine}" +
                        $"\t\t- ACollection2 [{pattern.Elements.Last().Id}] (collection){Environment.NewLine}" +
                        $"\t\t\t- Attributes:{Environment.NewLine}" +
                        $"\t\t\t\t- AProperty4 (string, default: ADefaultValue4){Environment.NewLine}"
                        ,
                        this.setup.Patterns.Single().Id));
            this.setup.Should()
                .DisplayMessage(
                    OutputMessages.CommandLine_Output_SolutionValidationFailed
                        .FormatTemplate(solution.Name, solution.Id,
                            $"1. {{APattern.AnElement1}} requires at least one instance{Environment.NewLine}{Environment.NewLine}"
                        ));
        }

        [Fact]
        public void WhenValidateAndErrors_ThenDisplaysErrors()
        {
            BuildInstallAndCreateSolution();

            this.setup.RunCommand($"{CommandLineApi.ValidateCommandName} solution");

            var solution = this.setup.Solutions.Single();
            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayMessage(
                    OutputMessages.CommandLine_Output_SolutionValidationFailed
                        .FormatTemplate(solution.Name, solution.Id,
                            $"1. {{APattern.AProperty1}} requires its value to be set{Environment.NewLine}" +
                            $"2. {{APattern.AnElement1}} requires at least one instance{Environment.NewLine}" +
                            $"3. {{APattern.ACollection2}} requires at least one instance{Environment.NewLine}{Environment.NewLine}"
                        ));
        }

        [Fact]
        public void WhenValidateAndNoErrors_ThenDisplaysSuccess()
        {
            BuildInstallAndCreateSolution();
            this.setup.RunCommand($"{CommandLineApi.ConfigureCommandName} on {{APattern}} --and-set \"AProperty1=avalue1\"");
            this.setup.RunCommand(
                $"{CommandLineApi.ConfigureCommandName} add {{AnElement1}} --and-set \"AProperty3=A\"");
            this.setup.RunCommand($"{CommandLineApi.ConfigureCommandName} add-one-to {{ACollection2}}");

            this.setup.RunCommand($"{CommandLineApi.ValidateCommandName} solution");

            var solution = this.setup.Solutions.Single();
            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayMessage(
                    OutputMessages.CommandLine_Output_SolutionValidationSuccess.FormatTemplate(solution.Name, solution.Id));
        }

        [Fact]
        public void WhenExecuteLaunchPointAndHasValidationErrors_ThenDisplaysValidations()
        {
            BuildInstallAndCreateSolution();
            this.setup.RunCommand($"{CommandLineApi.ExecuteCommandName} command ALaunchPoint1");

            var solution = this.setup.Solutions.Single();
            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayMessage(
                    OutputMessages.CommandLine_Output_SolutionValidationFailed
                        .FormatTemplate(solution.Name, solution.Id,
                            $"1. {{APattern.AProperty1}} requires its value to be set{Environment.NewLine}" +
                            $"2. {{APattern.AnElement1}} requires at least one instance{Environment.NewLine}" +
                            $"3. {{APattern.ACollection2}} requires at least one instance{Environment.NewLine}{Environment.NewLine}"
                        ));
        }

        [Fact]
        public void WhenExecuteLaunchPointAndHasFails_ThenDisplaysResults()
        {
            var testDirectory = Environment.CurrentDirectory;
            BuildInstallAndCreateSolution();
            this.setup.RunCommand($"{CommandLineApi.ConfigureCommandName} on {{APattern}} --and-set \"AProperty1=avalue1\"");
            this.setup.RunCommand(
                $"{CommandLineApi.ConfigureCommandName} add {{AnElement1}} --and-set \"AProperty3=B\"");
            this.setup.RunCommand($"{CommandLineApi.ConfigureCommandName} add-one-to {{ACollection2}}");

            this.setup.RunCommand($"{CommandLineApi.ExecuteCommandName} command ALaunchPoint2");

            var path = Path.Combine(testDirectory, @"code\Bnamingtest.cs");
            var commandId = this.setup.Patterns.Single().Automation[2].Id;
            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayMessage(OutputMessages.CommandLine_Output_CommandExecutionFailed.FormatTemplate("ALaunchPoint2",
                    "* " + DomainMessages.CodeTemplateCommand_Log_GeneratedFile.Format("Bnamingtest.cs", path) + $"{Environment.NewLine}" +
                    "* " + DomainMessages.CommandLaunchPoint_CommandIdFailedExecution.Format(commandId,
                        ExceptionMessages.TextTemplatingExtensions_HasSyntaxErrors.Format(DomainMessages.CodeTemplateCommand_FilePathExpression_Description.Format(commandId),
                            $"((21:0,21),(22:0,22)): Invalid token `CodeExit`. The dot operator is expected to be followed by a plain identifier{Environment.NewLine}" +
                            $"((20:0,20),(20:0,20)): Invalid token found `.`. Expecting <EOL>/end of line.{Environment.NewLine}{Environment.NewLine}"))));
        }

        [Fact]
        public void WhenExecuteLaunchPointOnSolution_ThenDisplaysSuccess()
        {
            var testDirectory = Environment.CurrentDirectory;
            BuildInstallAndCreateSolution();
            this.setup.RunCommand($"{CommandLineApi.ConfigureCommandName} on {{APattern}} --and-set \"AProperty1=avalue1\"");
            this.setup.RunCommand(
                $"{CommandLineApi.ConfigureCommandName} add {{AnElement1}} --and-set \"AProperty3=B\"");
            this.setup.RunCommand($"{CommandLineApi.ConfigureCommandName} add-one-to {{ACollection2}}");

            this.setup.RunCommand($"{CommandLineApi.ExecuteCommandName} command ALaunchPoint1");

            var artifactLink = this.setup.Solutions.Single().Model.ArtifactLinks.First().Path;
            var path = Path.Combine(testDirectory, @"code\Bnamingtest.cs");
            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayMessage(OutputMessages.CommandLine_Output_CommandExecutionSucceeded.FormatTemplate("ALaunchPoint1",
                    "* " + DomainMessages.CodeTemplateCommand_Log_GeneratedFile.Format("Bnamingtest.cs",
                        path) + $"{Environment.NewLine}"));
            artifactLink.Should().Be(path);
            var contents = File.ReadAllText(path);
            contents.Should().Be("some code");
        }

        [Fact]
        public void WhenExecuteLaunchPointOnElement_ThenDisplaysSuccess()
        {
            var testDirectory = Environment.CurrentDirectory;
            BuildInstallAndCreateSolution();
            this.setup.RunCommand($"{CommandLineApi.ConfigureCommandName} on {{APattern}} --and-set \"AProperty1=avalue1\"");
            this.setup.RunCommand(
                $"{CommandLineApi.ConfigureCommandName} add {{AnElement1}} --and-set \"AProperty3=B\"");
            this.setup.RunCommand($"{CommandLineApi.ConfigureCommandName} add-one-to {{ACollection2}}");

            this.setup.RunCommand($"{CommandLineApi.ExecuteCommandName} command ALaunchPoint3 --on {{AnElement1}}");

            var artifactLink = this.setup.Solutions.Single().Model.Properties["AnElement1"].ArtifactLinks.First().Path;
            var path = Path.Combine(testDirectory, @"code\parentsubstitutiontest.cs");
            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayMessage(OutputMessages.CommandLine_Output_CommandExecutionSucceeded.FormatTemplate("ALaunchPoint3",
                    "* " + DomainMessages.CodeTemplateCommand_Log_GeneratedFile.Format("parentsubstitutiontest.cs",
                        path) + $"{Environment.NewLine}"));
            artifactLink.Should().Be(path);
            var contents = File.ReadAllText(path);
            contents.Should().Be($"B{Environment.NewLine}avalue1{Environment.NewLine}B");
        }

        private static void DeleteCodeFolder()
        {
            var directory = new DirectoryInfo(Path.Combine(Environment.CurrentDirectory, "code"));
            if (directory.Exists)
            {
                directory.Delete(true);
            }
        }

        private void BuildInstallAndCreateSolution()
        {
            BuildAndInstallToolkit();
            this.setup.RunCommand($"{CommandLineApi.RunCommandName} toolkit APattern");
        }

        private string BuildAndInstallToolkit()
        {
            this.setup.RunCommand($"{CommandLineApi.CreateCommandName} pattern APattern");
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-codetemplate \"Assets/CodeTemplates/code1.code\" --name ACodeTemplate1");
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-codetemplate-command \"ACodeTemplate1\" --withpath \"~/code/{{{{an_element1.a_property3}}}}namingtest.cs\"");
            var commandId1 = this.setup.Patterns.Single().Automation[0].Id;
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-command-launchpoint {commandId1} --name ALaunchPoint1");
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-codetemplate-command \"ACodeTemplate1\" --withpath \"~/code/{{{{an_element1.}}}}invalid.cs\"");
            var commandId2 = this.setup.Patterns.Single().Automation[2].Id;
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-command-launchpoint {commandId1};{commandId2} --name ALaunchPoint2");
            this.setup.RunCommand($"{CommandLineApi.EditCommandName} add-attribute AProperty1 --isrequired");
            this.setup.RunCommand($"{CommandLineApi.EditCommandName} add-attribute AProperty2 --typeis int");
            this.setup.RunCommand($"{CommandLineApi.EditCommandName} add-element AnElement1");
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-attribute AProperty3 --aschildof {{APattern.AnElement1}} --isoneof \"A;B;C\"");

            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-codetemplate \"Assets/CodeTemplates/code2.code\" --name ACodeTemplate2 --aschildof {{APattern.AnElement1}}");
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-codetemplate-command \"ACodeTemplate2\" --withpath \"~/code/parentsubstitutiontest.cs\" --aschildof {{APattern.AnElement1}}");
            var commandId3 = this.setup.Patterns.Single().Elements.First().Automation.Single().Id;
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-command-launchpoint {commandId3} --name ALaunchPoint3 --aschildof {{APattern.AnElement1}}");

            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-collection ACollection1 --aschildof {{APattern.AnElement1}}");
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-collection ACollection2 --aschildof {{APattern}} --ality OneOrMany");
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-attribute AProperty4 --aschildof {{APattern.ACollection2}} --defaultvalueis ADefaultValue4");

            this.setup.RunCommand($"{CommandLineApi.BuildCommandName} toolkit");

            var desktopFolder = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            var location = Path.Combine(desktopFolder, "APattern_0.1.0.toolkit");
            this.setup.RunCommand($"{CommandLineApi.InstallCommandName} toolkit {location}");

            this.setup.Should().DisplayNoError();

            return location;
        }
    }
}