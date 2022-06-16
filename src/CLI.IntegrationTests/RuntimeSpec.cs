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
        private readonly string testApplicationName;

        public RuntimeSpec(CliTestSetup setup)
        {
            this.testApplicationName = GetFilePathInOutput("../../../../../tools/TestApp/TestApp.exe");
            this.setup = setup;
            this.setup.ResetRepository();
            DeleteOutputFolders();
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
            ConfigureBuildAndInstallToolkit();

            this.setup.Should()
                .DisplayMessage(
                    OutputMessages.CommandLine_Output_InstalledToolkit.FormatTemplate("APattern", "0.1.0"));
        }

        [Fact]
        public void WhenInstallToolkitWithSameToolkitSameVersionAgain_ThenInstallsToolkit()
        {
            var location = ConfigureBuildAndInstallToolkit();

            this.setup.RunCommand($"{CommandLineApi.InstallCommandName} toolkit {location}");

            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayMessage(
                    OutputMessages.CommandLine_Output_InstalledToolkit.FormatTemplate("APattern", "0.1.0"));
        }

        [Fact]
        public void WhenInstallToolkitWithSameToolkitNextVersionAgain_ThenInstallsToolkit()
        {
            ConfigureBuildAndInstallToolkit();
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-attribute AProperty5");
            this.setup.RunCommand($"{CommandLineApi.BuildCommandName} toolkit");

            var locationV2 = GetFilePathOfExportedToolkit("APattern_0.2.0.toolkit");
            this.setup.RunCommand($"{CommandLineApi.InstallCommandName} toolkit {locationV2}");

            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayMessage(
                    OutputMessages.CommandLine_Output_InstalledToolkit.FormatTemplate("APattern", "0.2.0"));
        }

        [Fact]
        public void WhenInstallToolkitWithMissingRuntimeVersion_ThenDisplaysError()
        {
            var location = GetFilePathInOutput("Assets/Toolkits/BeforeFirstVersionSupportingRuntimeVersion.toolkit");
            this.setup.RunCommand($"{CommandLineApi.InstallCommandName} toolkit {location}");

            var runtimeVersion = ToolkitConstants.GetRuntimeVersion();
            var runtimeName = ToolkitConstants.GetRuntimeProductName();
            this.setup.Should()
                .DisplayError(ExceptionMessages.ToolkitDefinition_CompatabilityToolkitNoVersion.Format(
                    ToolkitConstants.FirstVersionSupportingRuntimeVersion, runtimeVersion, runtimeName));
        }

        [Fact]
        public void WhenInstallToolkitWithOlderIncompatibleRuntimeVersion_ThenDisplaysError()
        {
            var location = GetFilePathInOutput("Assets/Toolkits/OlderRuntimeVersion.toolkit");
            this.setup.RunCommand($"{CommandLineApi.InstallCommandName} toolkit {location}");

            var runtimeVersion = ToolkitConstants.GetRuntimeVersion();
            var runtimeName = ToolkitConstants.GetRuntimeProductName();
            this.setup.Should()
                .DisplayError(ExceptionMessages.ToolkitDefinition_CompatabilityToolkitOutOfDate.Format(
                    "0.1.0-preview", runtimeVersion, runtimeName));
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
            ConfigureBuildAndInstallToolkit();

            this.setup.RunCommand($"{CommandLineApi.ListCommandName} toolkits");

            var toolkit = this.setup.Toolkit;

            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayMessage(
                    OutputMessages.CommandLine_Output_InstalledToolkitsListed.FormatTemplate(
                        $"{{\"Name\": \"{toolkit.PatternName}\", \"Version\": \"{toolkit.Version}\", \"ID\": \"{toolkit.Id}\"}}"));
        }

        [Fact]
        public void WhenCreateDraft_ThenCreatesDraft()
        {
            ConfigureBuildAndInstallToolkit();

            this.setup.RunCommand($"{CommandLineApi.RunCommandName} toolkit APattern");

            var draft = this.setup.Draft;

            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayMessage(
                    OutputMessages.CommandLine_Output_CreateDraftFromToolkit.FormatTemplate(draft.Name,
                        draft.Id, draft.PatternName));
        }

        [Fact]
        public void WhenSwitchDraft_ThenSwitchesDraft()
        {
            ConfigureBuildAndInstallToolkit();

            this.setup.RunCommand($"{CommandLineApi.RunCommandName} toolkit APattern");
            this.setup.RunCommand($"{CommandLineApi.RunCommandName} toolkit APattern");

            var draft1 = this.setup.Drafts.First();

            this.setup.RunCommand($"{CommandLineApi.RunCommandName} switch \"{draft1.Id}\"");

            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayMessage(
                    OutputMessages.CommandLine_Output_DraftSwitched.FormatTemplate(draft1.Name, draft1.Id));
        }

        [Fact]
        public void WhenListInstalledDraftsAndNone_ThenDisplaysNone()
        {
            ConfigureBuildAndInstallToolkit();

            this.setup.RunCommand($"{CommandLineApi.ListCommandName} drafts");

            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayMessage(
                    OutputMessages.CommandLine_Output_NoInstalledDrafts);
        }

        [Fact]
        public void WhenListCreatedDraftsAndOne_ThenDisplaysOne()
        {
            ConfigureBuildAndInstallToolkit();
            this.setup.RunCommand($"{CommandLineApi.RunCommandName} toolkit APattern");

            this.setup.RunCommand($"{CommandLineApi.ListCommandName} drafts");

            var draft = this.setup.Draft;

            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayMessage(
                    OutputMessages.CommandLine_Output_InstalledDraftsListed.FormatTemplate(
                        $"{{\"Name\": \"{draft.Name}\", \"ID\": \"{draft.Id}\", \"Version\": \"{draft.Toolkit.Version}\"}}"));
        }

        [Fact]
        public void WhenConfigureDraftAndSetPropertyOnPattern_ThenDisplaysSuccess()
        {
            CreateDraftFromBuiltToolkit();

            this.setup.RunCommand(
                $"{CommandLineApi.ConfigureCommandName} on {{APattern}} --and-set \"AProperty1=avalue\"");

            var item = this.setup.Draft.Model;
            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayMessage(
                    OutputMessages.CommandLine_Output_DraftConfigured.FormatTemplate("APattern", item.Id));
            item.Properties["AProperty1"].Value.Should().Be("avalue");
        }

        [Fact]
        public void WhenConfigureDraftAndSetPropertyOnNewElement_ThenDisplaysSuccess()
        {
            CreateDraftFromBuiltToolkit();

            this.setup.RunCommand(
                $"{CommandLineApi.ConfigureCommandName} add {{AnElement1}} --and-set \"AProperty3=B\"");

            var item = this.setup.Draft.Model.Properties["AnElement1"];
            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayMessage(
                    OutputMessages.CommandLine_Output_DraftConfigured.FormatTemplate("AnElement1", item.Id));
            item.Properties["AProperty3"].Value.Should().Be("B");
        }

        [Fact]
        public void WhenConfigureDraftAndSetPropertyOnExistingElement_ThenDisplaysSuccess()
        {
            CreateDraftFromBuiltToolkit();
            this.setup.RunCommand($"{CommandLineApi.ConfigureCommandName} add {{AnElement1}}");

            this.setup.RunCommand(
                $"{CommandLineApi.ConfigureCommandName} on {{AnElement1}} --and-set \"AProperty3=C\"");

            var item = this.setup.Draft.Model.Properties["AnElement1"];
            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayMessage(
                    OutputMessages.CommandLine_Output_DraftConfigured.FormatTemplate("AnElement1", item.Id));
            item.Properties["AProperty3"].Value.Should().Be("C");
        }

        [Fact]
        public void WhenConfigureDraftAndReSetPropertyWithNullOnExistingElement_ThenDisplaysSuccess()
        {
            CreateDraftFromBuiltToolkit();
            this.setup.RunCommand(
                $"{CommandLineApi.ConfigureCommandName} on \"\" --and-set \"AProperty1=\"");

            var item = this.setup.Draft.Model;
            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayMessage(
                    OutputMessages.CommandLine_Output_DraftConfigured.FormatTemplate(item.Name, item.Id));
            item.Properties["AProperty1"].Value.Should().BeNull();
        }

        [Fact]
        public void WhenConfigureDraftAndReSetPropertyOnExistingElement_ThenDisplaysSuccess()
        {
            CreateDraftFromBuiltToolkit();
            this.setup.RunCommand(
                $"{CommandLineApi.ConfigureCommandName} add {{AnElement1}} --and-set \"AProperty3=B\"");

            this.setup.RunCommand(
                $"{CommandLineApi.ConfigureCommandName} on {{AnElement1}} --and-set \"AProperty3=C\"");

            var item = this.setup.Draft.Model.Properties["AnElement1"];
            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayMessage(
                    OutputMessages.CommandLine_Output_DraftConfigured.FormatTemplate("AnElement1", item.Id));
            item.Properties["AProperty3"].Value.Should().Be("C");
        }

        [Fact]
        public void WhenConfigureDraftAndSetPropertyOnNewCollectionItem_ThenDisplaysSuccess()
        {
            CreateDraftFromBuiltToolkit();

            this.setup.RunCommand(
                $"{CommandLineApi.ConfigureCommandName} add-one-to {{ACollection2}} --and-set \"AProperty4=anewvalue\"");

            var item = this.setup.Draft.Model.Properties["ACollection2"].Items.Single();
            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayMessage(
                    OutputMessages.CommandLine_Output_DraftConfigured.FormatTemplate("ACollection2", item.Id));
            item.Properties["AProperty4"].Value.Should().Be("anewvalue");
        }

        [Fact]
        public void WhenConfigureDraftAndSetPropertyOnExistingCollectionItem_ThenDisplaysSuccess()
        {
            CreateDraftFromBuiltToolkit();
            this.setup.RunCommand(
                $"{CommandLineApi.ConfigureCommandName} add-one-to {{ACollection2}} --and-set \"AProperty4=avalue\"");
            var item = this.setup.Draft.Model.Properties["ACollection2"].Items.Single();

            this.setup.RunCommand(
                $"{CommandLineApi.ConfigureCommandName} on {{ACollection2.{item.Id}}} --and-set \"AProperty4=anewvalue\"");

            item = this.setup.Draft.Model.Properties["ACollection2"].Items.Single();
            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayMessage(
                    OutputMessages.CommandLine_Output_DraftConfigured.FormatTemplate("ACollection2", item.Id));
            item.Properties["AProperty4"].Value.Should().Be("anewvalue");
        }

        [Fact]
        public void WhenConfigureDraftAndResetElement_ThenDisplaysSuccess()
        {
            CreateDraftFromBuiltToolkit();

            this.setup.RunCommand(
                $"{CommandLineApi.ConfigureCommandName} on {{APatternName.AnElement3}} --and-set \"AProperty5=avalue1\"");
            this.setup.RunCommand(
                $"{CommandLineApi.ConfigureCommandName} on {{APatternName.AnElement3}} --and-set \"AProperty6=99\"");
            this.setup.RunCommand(
                $"{CommandLineApi.ConfigureCommandName} on {{APatternName.AnElement3}} --and-set \"AProperty7=avalue2\"");

            this.setup.RunCommand($"{CommandLineApi.ConfigureCommandName} reset {{AnElement3}}");

            var elementItem = this.setup.Draft.Model.Properties["AnElement3"];
            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayMessage(
                    OutputMessages.CommandLine_Output_DraftResetElement.FormatTemplate("AnElement3",
                        elementItem.Id));
            elementItem.Properties["AProperty5"].Value.Should().Be("ADefaultValue1");
            elementItem.Properties["AProperty6"].Value.Should().Be(25);
            elementItem.Properties["AProperty7"].Value.Should().BeNull();
        }

        [Fact]
        public void WhenConfigureDraftAndClearCollection_ThenDisplaysSuccess()
        {
            CreateDraftFromBuiltToolkit();

            this.setup.RunCommand(
                $"{CommandLineApi.ConfigureCommandName} add-one-to {{ACollection2}} --and-set \"AProperty4=anewvalue1\"");
            this.setup.RunCommand(
                $"{CommandLineApi.ConfigureCommandName} add-one-to {{ACollection2}} --and-set \"AProperty4=anewvalue2\"");
            this.setup.RunCommand(
                $"{CommandLineApi.ConfigureCommandName} add-one-to {{ACollection2}} --and-set \"AProperty4=anewvalue3\"");

            this.setup.RunCommand($"{CommandLineApi.ConfigureCommandName} clear {{ACollection2}}");

            var collectionItem = this.setup.Draft.Model.Properties["ACollection2"];
            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayMessage(
                    OutputMessages.CommandLine_Output_DraftEmptyCollection.FormatTemplate("ACollection2",
                        collectionItem.Id));
            collectionItem.Items.Count.Should().Be(0);
        }

        [Fact]
        public void WhenConfigureDraftAndDeleteElement_ThenDisplaysSuccess()
        {
            CreateDraftFromBuiltToolkit();

            var elementItem = this.setup.Draft.Model.Properties["AnElement3"];
            this.setup.RunCommand($"{CommandLineApi.ConfigureCommandName} delete {{AnElement3}}");

            var draftItem = this.setup.Draft.Model;
            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayMessage(
                    OutputMessages.CommandLine_Output_DraftDelete.FormatTemplate("AnElement3",
                        elementItem.Id));
            draftItem.Properties.Should().NotContainKey("AnElement3");
        }

        [Fact]
        public void WhenViewDraft_ThenDisplaysConfiguration()
        {
            CreateDraftFromBuiltToolkit();
            this.setup.RunCommand(
                $"{CommandLineApi.ConfigureCommandName} on {{APattern}} --and-set \"AProperty1=avalue1\"");
            this.setup.RunCommand(
                $"{CommandLineApi.ConfigureCommandName} add {{AnElement1}} --and-set \"AProperty3=A\"");
            this.setup.RunCommand($"{CommandLineApi.ConfigureCommandName} add-one-to {{ACollection2}}");

            this.setup.RunCommand($"{CommandLineApi.ViewCommandName} draft");

            var draft = this.setup.Draft;
            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayMessage(OutputMessages.CommandLine_Output_DraftConfiguration.FormatTemplate(draft.Name,
                    draft.Id,
                    new
                    {
                        draft.Model.Id,
                        AProperty1 = "avalue1",
                        AnElement1 = new
                        {
                            draft.Model.Properties["AnElement1"].Id,
                            AProperty3 = "A",
                            ACollection1 = new
                            {
                                draft.Model.Properties["AnElement1"].Properties["ACollection1"].Id
                            }
                        },
                        ACollection2 = new
                        {
                            draft.Model.Properties["ACollection2"].Id,
                            Items = new[]
                            {
                                new
                                {
                                    draft.Model.Properties["ACollection2"].Items.Single().Id,
                                    AProperty4 = "ADefaultValue4"
                                }
                            }
                        },
                        AnElement3 = new
                        {
                            draft.Model.Properties["AnElement3"].Id,
                            AProperty5 = "ADefaultValue1",
                            AProperty6 = 25
                        }
                    }.ToJson<dynamic>()));
        }

        [Fact]
        public void WhenViewDraftWithTodo_ThenDisplaysConfigurationSchemaAndValidation()
        {
            CreateDraftFromBuiltToolkit();
            this.setup.RunCommand(
                $"{CommandLineApi.ConfigureCommandName} on {{APattern}} --and-set \"AProperty1=avalue1\"");
            this.setup.RunCommand($"{CommandLineApi.ConfigureCommandName} add-one-to {{ACollection2}}");

            this.setup.RunCommand($"{CommandLineApi.ViewCommandName} draft --todo");

            var draft = this.setup.Draft;
            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayMessage(OutputMessages.CommandLine_Output_DraftConfiguration.FormatTemplate(draft.Name,
                    draft.Id,
                    new
                    {
                        draft.Model.Id,
                        AProperty1 = "avalue1",
                        AnElement1 = new
                        {
                            draft.Model.Properties["AnElement1"].Id
                        },
                        ACollection2 = new
                        {
                            draft.Model.Properties["ACollection2"].Id,
                            Items = new[]
                            {
                                new
                                {
                                    draft.Model.Properties["ACollection2"].Items.Single().Id,
                                    AProperty4 = "ADefaultValue4"
                                }
                            }
                        },
                        AnElement3 = new
                        {
                            draft.Model.Properties["AnElement3"].Id,
                            AProperty5 = "ADefaultValue1",
                            AProperty6 = 25
                        }
                    }.ToJson<dynamic>()));
            var pattern = this.setup.Pattern;
            var codeTemplatePath1 = GetFilePathInOutput("Assets/CodeTemplates/code1.code");
            var codeTemplatePath2 = GetFilePathInOutput("Assets/CodeTemplates/code2.code");
            var element1 = this.setup.Pattern.Elements.First();
            this.setup.Should()
                .DisplayMessage(
                    OutputMessages.CommandLine_Output_PatternConfiguration.FormatTemplate(pattern.Name, pattern.Id,
                        pattern.ToolkitVersion.Current,
                        $"- APattern [{pattern.Id}] (root element){Environment.NewLine}" +
                        $"\t- CodeTemplates:{Environment.NewLine}" +
                        $"\t\t- ACodeTemplate1 [{pattern.CodeTemplates.Single().Id}] (original: {PatternConfigurationVisitor.TruncateCodeTemplatePath(codeTemplatePath1)}){Environment.NewLine}" +
                        $"\t- Automation:{Environment.NewLine}" +
                        $"\t\t- CodeTemplateCommand1 [{pattern.Automation[0].Id}] (CodeTemplateCommand) (template: {pattern.CodeTemplates.Single().Id}, always, path: ~/code/{{{{AnElement1.AProperty3}}}}namingtest.cs){Environment.NewLine}" +
                        $"\t\t- ALaunchPoint1 [{pattern.Automation[1].Id}] (CommandLaunchPoint) (ids: {pattern.Automation[0].Id}){Environment.NewLine}" +
                        $"\t\t- CodeTemplateCommand3 [{pattern.Automation[2].Id}] (CodeTemplateCommand) (template: {pattern.CodeTemplates.Single().Id}, always, path: ~/code/{{{{AnElement1.}}}}invalid.cs){Environment.NewLine}" +
                        $"\t\t- CliCommand4 [{pattern.Automation[3].Id}] (CliCommand) (app: {this.testApplicationName}, args: --succeeds){Environment.NewLine}" +
                        $"\t\t- ALaunchPoint2 [{pattern.Automation[4].Id}] (CommandLaunchPoint) (ids: {pattern.Automation[0].Id};{pattern.Automation[2].Id};{pattern.Automation[3].Id}){Environment.NewLine}" +
                        $"\t- Attributes:{Environment.NewLine}" +
                        $"\t\t- AProperty1 (string, required){Environment.NewLine}" +
                        $"\t\t- AProperty2 (int){Environment.NewLine}" +
                        $"\t- Elements:{Environment.NewLine}" +
                        $"\t\t- AnElement1 [{element1.Id}] (element){Environment.NewLine}" +
                        $"\t\t\t- CodeTemplates:{Environment.NewLine}" +
                        $"\t\t\t\t- ACodeTemplate2 [{element1.CodeTemplates.Single().Id}] (original: {PatternConfigurationVisitor.TruncateCodeTemplatePath(codeTemplatePath2)}){Environment.NewLine}" +
                        $"\t\t\t- Automation:{Environment.NewLine}" +
                        $"\t\t\t\t- CodeTemplateCommand1 [{element1.Automation.First().Id}] (CodeTemplateCommand) (template: {element1.CodeTemplates.Single().Id}, always, path: ~/code/parentsubstitutiontest.cs){Environment.NewLine}" +
                        $"\t\t\t\t- ALaunchPoint3 [{element1.Automation.Last().Id}] (CommandLaunchPoint) (ids: {element1.Automation.First().Id}){Environment.NewLine}" +
                        $"\t\t\t- Attributes:{Environment.NewLine}" +
                        $"\t\t\t\t- AProperty3 (string, oneof: A;B;C){Environment.NewLine}" +
                        $"\t\t\t- Elements:{Environment.NewLine}" +
                        $"\t\t\t\t- ACollection1 [{element1.Elements.First().Id}] (collection){Environment.NewLine}" +
                        $"\t\t- ACollection2 [{pattern.Elements[1].Id}] (collection){Environment.NewLine}" +
                        $"\t\t\t- Attributes:{Environment.NewLine}" +
                        $"\t\t\t\t- AProperty4 (string, default: ADefaultValue4){Environment.NewLine}" +
                        $"\t\t- AnElement3 [{pattern.Elements.Last().Id}] (element){Environment.NewLine}" +
                        $"\t\t\t- Attributes:{Environment.NewLine}" +
                        $"\t\t\t\t- AProperty5 (string, default: ADefaultValue1){Environment.NewLine}" +
                        $"\t\t\t\t- AProperty6 (int, default: 25){Environment.NewLine}" +
                        $"\t\t\t\t- AProperty7 (string){Environment.NewLine}"
                        ,
                        this.setup.Pattern.Id));
            this.setup.Should()
                .DisplayMessage(
                    OutputMessages.CommandLine_Output_PatternLaunchableAutomation.FormatTemplate(pattern.Name,
                        pattern.Id,
                        pattern.ToolkitVersion.Current,
                        $"- APattern (root element){Environment.NewLine}" +
                        $"\t- LaunchPoints:{Environment.NewLine}" +
                        $"\t\t- ALaunchPoint1 [{pattern.Automation[1].Id}] (CommandLaunchPoint){Environment.NewLine}" +
                        $"\t\t- ALaunchPoint2 [{pattern.Automation[4].Id}] (CommandLaunchPoint){Environment.NewLine}" +
                        $"\t- Elements:{Environment.NewLine}" +
                        $"\t\t- AnElement1 (element){Environment.NewLine}" +
                        $"\t\t\t- LaunchPoints:{Environment.NewLine}" +
                        $"\t\t\t\t- ALaunchPoint3 [{element1.Automation.Last().Id}] (CommandLaunchPoint){Environment.NewLine}" +
                        $"\t\t\t- Elements:{Environment.NewLine}" +
                        $"\t\t\t\t- ACollection1 (collection){Environment.NewLine}" +
                        $"\t\t- ACollection2 (collection){Environment.NewLine}" +
                        $"\t\t- AnElement3 (element){Environment.NewLine}"
                        ,
                        this.setup.Pattern.Id));
            this.setup.Should()
                .DisplayMessage(
                    OutputMessages.CommandLine_Output_DraftValidationFailed
                        .FormatTemplate(draft.Name, draft.Id,
                            $"1. {{APattern.AnElement1}} requires at least one instance{Environment.NewLine}{Environment.NewLine}"
                        ));
        }

        [Fact]
        public void WhenValidateAndErrors_ThenDisplaysErrors()
        {
            CreateDraftFromBuiltToolkit();

            this.setup.RunCommand($"{CommandLineApi.ValidateCommandName} draft");

            var draft = this.setup.Draft;
            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayMessage(
                    OutputMessages.CommandLine_Output_DraftValidationFailed
                        .FormatTemplate(draft.Name, draft.Id,
                            $"1. {{APattern.AProperty1}} requires its value to be set{Environment.NewLine}" +
                            $"2. {{APattern.AnElement1}} requires at least one instance{Environment.NewLine}" +
                            $"3. {{APattern.ACollection2}} requires at least one instance{Environment.NewLine}{Environment.NewLine}"
                        ));
        }

        [Fact]
        public void WhenValidateAndNoErrors_ThenDisplaysSuccess()
        {
            CreateDraftFromBuiltToolkit();
            this.setup.RunCommand(
                $"{CommandLineApi.ConfigureCommandName} on {{APattern}} --and-set \"AProperty1=avalue1\"");
            this.setup.RunCommand(
                $"{CommandLineApi.ConfigureCommandName} add {{AnElement1}} --and-set \"AProperty3=A\"");
            this.setup.RunCommand($"{CommandLineApi.ConfigureCommandName} add-one-to {{ACollection2}}");

            this.setup.RunCommand($"{CommandLineApi.ValidateCommandName} draft");

            var draft = this.setup.Draft;
            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayMessage(
                    OutputMessages.CommandLine_Output_DraftValidationSuccess.FormatTemplate(draft.Name,
                        draft.Id));
        }

        [Fact]
        public void WhenExecuteLaunchPointAndHasValidationErrors_ThenDisplaysValidations()
        {
            CreateDraftFromBuiltToolkit();
            this.setup.RunCommand($"{CommandLineApi.ExecuteCommandName} command ALaunchPoint1");

            var draft = this.setup.Draft;
            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayMessage(
                    OutputMessages.CommandLine_Output_DraftValidationFailed
                        .FormatTemplate(draft.Name, draft.Id,
                            $"1. {{APattern.AProperty1}} requires its value to be set{Environment.NewLine}" +
                            $"2. {{APattern.AnElement1}} requires at least one instance{Environment.NewLine}" +
                            $"3. {{APattern.ACollection2}} requires at least one instance{Environment.NewLine}{Environment.NewLine}"
                        ));
        }

        [Fact]
        public void WhenExecuteLaunchPointAndFails_ThenDisplaysResults()
        {
            CreateDraftFromBuiltToolkit();
            this.setup.RunCommand(
                $"{CommandLineApi.ConfigureCommandName} on {{APattern}} --and-set \"AProperty1=avalue1\"");
            this.setup.RunCommand(
                $"{CommandLineApi.ConfigureCommandName} add {{AnElement1}} --and-set \"AProperty3=B\"");
            this.setup.RunCommand($"{CommandLineApi.ConfigureCommandName} add-one-to {{ACollection2}}");

            this.setup.RunCommand($"{CommandLineApi.ExecuteCommandName} command ALaunchPoint2");

            var path = GetFilePathInOutput(@"code/Bnamingtest.cs");
            var commandId = this.setup.Pattern.Automation[2].Id;
            var codeTemplate = this.setup.Pattern.CodeTemplates.Single();
            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayMessage(OutputMessages.CommandLine_Output_CommandExecutionFailed.FormatTemplate("ALaunchPoint2",
                    "* " + DomainMessages.CodeTemplateCommand_Log_GeneratedFile.Format("Bnamingtest.cs",
                        codeTemplate.Id, path) +
                    $"{Environment.NewLine}" +
                    "* " + DomainMessages.CommandLaunchPoint_CommandIdFailedExecution.Format(commandId,
                        ExceptionMessages.TextTemplatingExtensions_HasSyntaxErrors.Format(
                            DomainMessages.CodeTemplateCommand_FilePathExpression_Description.Format(commandId),
                            $"((20:0,20),(21:0,21)): Invalid token `CodeExit`. The dot operator is expected to be followed by a plain identifier{Environment.NewLine}" +
                            $"((19:0,19),(19:0,19)): Invalid token found `.`. Expecting <EOL>/end of line.{Environment.NewLine}" +
                            "* " + InfrastructureMessages.ApplicationExecutor_Succeeded.Format(this.testApplicationName,
                                "--succeeds", "Success") + $"{Environment.NewLine}"))));
        }

        [Fact]
        public void WhenExecuteLaunchPointOnDraft_ThenDisplaysSuccess()
        {
            CreateDraftFromBuiltToolkit();
            this.setup.RunCommand(
                $"{CommandLineApi.ConfigureCommandName} on {{APattern}} --and-set \"AProperty1=avalue1\"");
            this.setup.RunCommand(
                $"{CommandLineApi.ConfigureCommandName} add {{AnElement1}} --and-set \"AProperty3=B\"");
            this.setup.RunCommand($"{CommandLineApi.ConfigureCommandName} add-one-to {{ACollection2}}");

            this.setup.RunCommand($"{CommandLineApi.ExecuteCommandName} command ALaunchPoint1");

            var artifactLink = this.setup.Draft.Model.ArtifactLinks.First().Path;
            var path = GetFilePathInOutput(@"code/Bnamingtest.cs");
            var codeTemplate = this.setup.Pattern.CodeTemplates.Single();
            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayMessage(OutputMessages.CommandLine_Output_CommandExecutionSucceeded.FormatTemplate(
                    "ALaunchPoint1",
                    "* " + DomainMessages.CodeTemplateCommand_Log_GeneratedFile.Format("Bnamingtest.cs",
                        codeTemplate.Id,
                        path) + $"{Environment.NewLine}"));
            artifactLink.Should().Be(path);
            var contents = File.ReadAllText(path);
            contents.Should().Be("some code");
        }

        [Fact]
        public void WhenExecuteLaunchPointOnElement_ThenDisplaysSuccess()
        {
            CreateDraftFromBuiltToolkit();
            this.setup.RunCommand(
                $"{CommandLineApi.ConfigureCommandName} on {{APattern}} --and-set \"AProperty1=avalue1\"");
            this.setup.RunCommand(
                $"{CommandLineApi.ConfigureCommandName} add {{AnElement1}} --and-set \"AProperty3=B\"");
            this.setup.RunCommand($"{CommandLineApi.ConfigureCommandName} add-one-to {{ACollection2}}");

            this.setup.RunCommand($"{CommandLineApi.ExecuteCommandName} command ALaunchPoint3 --on {{AnElement1}}");

            var artifactLink = this.setup.Draft.Model.Properties["AnElement1"].ArtifactLinks.First().Path;
            var path = GetFilePathInOutput(@"code/parentsubstitutiontest.cs");
            var codeTemplate = this.setup.Pattern.Elements.First().CodeTemplates.Single();
            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayMessage(OutputMessages.CommandLine_Output_CommandExecutionSucceeded.FormatTemplate(
                    "ALaunchPoint3",
                    "* " + DomainMessages.CodeTemplateCommand_Log_GeneratedFile.Format("parentsubstitutiontest.cs",
                        codeTemplate.Id,
                        path) + $"{Environment.NewLine}"));
            artifactLink.Should().Be(path);
            var contents = File.ReadAllText(path);
            contents.Should().Be($"B{Environment.NewLine}avalue1{Environment.NewLine}");
        }

        [Fact]
        public void WhenUpgradeDraftAndToolkitNotUpgraded_ThenDisplaysWarning()
        {
            CreateDraftFromBuiltToolkit();
            this.setup.RunCommand(
                $"{CommandLineApi.ConfigureCommandName} on {{APattern}} --and-set \"AProperty1=avalue1\"");
            this.setup.RunCommand(
                $"{CommandLineApi.ConfigureCommandName} add {{AnElement1}} --and-set \"AProperty3=B\"");
            this.setup.RunCommand($"{CommandLineApi.ConfigureCommandName} add-one-to {{ACollection2}}");

            this.setup.RunCommand($"{CommandLineApi.UpgradeCommandName} draft");

            var draft = this.setup.Draft;
            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayMessage(OutputMessages.CommandLine_Output_DraftUpgradeWithWarning.FormatTemplate(
                    draft.Name, draft.Id, draft.PatternName, "0.1.0", "0.1.0",
                    $"* {MigrationChangeType.Abort}: " +
                    MigrationMessages.DraftDefinition_Upgrade_SameToolkitVersion.FormatTemplate(draft.PatternName,
                        draft.Toolkit.Version) + $"{Environment.NewLine}"));
        }

        [Fact]
        public void WhenViewToolkitAndNone_ThenDisplaysError()
        {
            this.setup.RunCommand($"{CommandLineApi.ViewCommandName} toolkit");

            this.setup.Should().DisplayError(ExceptionMessages.RuntimeApplication_NoCurrentToolkit);
        }

        [Fact]
        public void WhenViewToolkitAndNoDraft_ThenDisplaysPatternTree()
        {
            ConfigureBuildAndInstallToolkit();

            this.setup.RunCommand($"{CommandLineApi.ViewCommandName} toolkit");

            var pattern = this.setup.Pattern;
            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayMessage(
                    OutputMessages.CommandLine_Output_ToolkitConfiguration.FormatTemplate(pattern.Name, pattern.Id,
                        pattern.ToolkitVersion.Current,
                        $"- APattern (root element) (attached with 1 code templates){Environment.NewLine}" +
                        $"\t- AProperty1 (attribute) (string, required){Environment.NewLine}" +
                        $"\t- AProperty2 (attribute) (int){Environment.NewLine}" +
                        $"\t- AnElement1 (element) (attached with 1 code templates){Environment.NewLine}" +
                        $"\t\t- AProperty3 (attribute) (string, oneof: A;B;C){Environment.NewLine}" +
                        $"\t\t- ACollection1 (collection){Environment.NewLine}" +
                        $"\t- ACollection2 (collection){Environment.NewLine}" +
                        $"\t\t- AProperty4 (attribute) (string, default: ADefaultValue4){Environment.NewLine}" +
                        $"\t- AnElement3 (element){Environment.NewLine}" +
                        $"\t\t- AProperty5 (attribute) (string, default: ADefaultValue1){Environment.NewLine}" +
                        $"\t\t- AProperty6 (attribute) (int, default: 25){Environment.NewLine}" +
                        $"\t\t- AProperty7 (attribute) (string){Environment.NewLine}"
                        ,
                        this.setup.Pattern.Id));
        }

        [Fact]
        public void WhenViewToolkit_ThenDisplaysPatternTree()
        {
            CreateDraftFromBuiltToolkit();

            this.setup.RunCommand($"{CommandLineApi.ViewCommandName} toolkit");

            var pattern = this.setup.Pattern;
            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayMessage(
                    OutputMessages.CommandLine_Output_ToolkitConfiguration.FormatTemplate(pattern.Name, pattern.Id,
                        pattern.ToolkitVersion.Current,
                        $"- APattern (root element) (attached with 1 code templates){Environment.NewLine}" +
                        $"\t- AProperty1 (attribute) (string, required){Environment.NewLine}" +
                        $"\t- AProperty2 (attribute) (int){Environment.NewLine}" +
                        $"\t- AnElement1 (element) (attached with 1 code templates){Environment.NewLine}" +
                        $"\t\t- AProperty3 (attribute) (string, oneof: A;B;C){Environment.NewLine}" +
                        $"\t\t- ACollection1 (collection){Environment.NewLine}" +
                        $"\t- ACollection2 (collection){Environment.NewLine}" +
                        $"\t\t- AProperty4 (attribute) (string, default: ADefaultValue4){Environment.NewLine}" +
                        $"\t- AnElement3 (element){Environment.NewLine}" +
                        $"\t\t- AProperty5 (attribute) (string, default: ADefaultValue1){Environment.NewLine}" +
                        $"\t\t- AProperty6 (attribute) (int, default: 25){Environment.NewLine}" +
                        $"\t\t- AProperty7 (attribute) (string){Environment.NewLine}"
                        ,
                        this.setup.Pattern.Id));
        }

        private static string GetFilePathInOutput(string filename)
        {
            return Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, filename));
        }

        private static string GetFilePathOfExportedToolkit(string filename)
        {
            return Path.GetFullPath(Path.Combine(InfrastructureConstants.GetExportDirectory(), filename));
        }

        private static void DeleteOutputFolders()
        {
            var directory = new DirectoryInfo(GetFilePathInOutput("code"));
            if (directory.Exists)
            {
                directory.Delete(true);
            }
        }

        private void CreateDraftFromBuiltToolkit()
        {
            ConfigureBuildAndInstallToolkit();
            this.setup.RunCommand($"{CommandLineApi.RunCommandName} toolkit APattern");

            this.setup.Should().DisplayNoError();
        }

        private string ConfigureBuildAndInstallToolkit()
        {
            this.setup.RunCommand($"{CommandLineApi.CreateCommandName} pattern APattern");
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-codetemplate \"Assets/CodeTemplates/code1.code\" --name ACodeTemplate1");
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-codetemplate-command \"ACodeTemplate1\" --targetpath \"~/code/{{{{AnElement1.AProperty3}}}}namingtest.cs\"");
            var commandId1 = this.setup.Pattern.Automation[0].Id;
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-command-launchpoint {commandId1} --name ALaunchPoint1");
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-codetemplate-command \"ACodeTemplate1\" --targetpath \"~/code/{{{{AnElement1.}}}}invalid.cs\"");
            var commandId2 = this.setup.Pattern.Automation[2].Id;
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-cli-command \"{this.testApplicationName}\" --arguments \"--succeeds\"");
            var commandId3 = this.setup.Pattern.Automation[3].Id;
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-command-launchpoint {commandId1};{commandId2};{commandId3} --name ALaunchPoint2");
            this.setup.RunCommand($"{CommandLineApi.EditCommandName} add-attribute AProperty1 --isrequired");
            this.setup.RunCommand($"{CommandLineApi.EditCommandName} add-attribute AProperty2 --isoftype int");
            this.setup.RunCommand($"{CommandLineApi.EditCommandName} add-element AnElement1 --autocreate false");
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-attribute AProperty3 --aschildof {{APattern.AnElement1}} --isoneof \"A;B;C\"");

            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-codetemplate \"Assets/CodeTemplates/code2.code\" --name ACodeTemplate2 --aschildof {{APattern.AnElement1}}");
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-codetemplate-command \"ACodeTemplate2\" --targetpath \"~/code/parentsubstitutiontest.cs\" --aschildof {{APattern.AnElement1}}");
            var commandId4 = this.setup.Pattern.Elements.First().Automation.Single().Id;
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-command-launchpoint {commandId4} --name ALaunchPoint3 --aschildof {{APattern.AnElement1}}");

            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-collection ACollection1 --aschildof {{APattern.AnElement1}}");
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-collection ACollection2 --aschildof {{APattern}} --isrequired");
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-attribute AProperty4 --aschildof {{APattern.ACollection2}} --defaultvalueis ADefaultValue4");

            this.setup.RunCommand($"{CommandLineApi.EditCommandName} add-element AnElement3");
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-attribute AProperty5 --aschildof {{APattern.AnElement3}} --defaultvalueis ADefaultValue1");
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-attribute AProperty6 --aschildof {{APattern.AnElement3}} --isoftype int --defaultvalueis 25");
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-attribute AProperty7 --aschildof {{APattern.AnElement3}}");

            this.setup.Should().DisplayNoError();

            return BuildAndInstallToolkit();
        }

        private string BuildAndInstallToolkit(
            string versionInstruction = ToolkitVersion.AutoIncrementInstruction)
        {
            this.setup.RunCommand($"{CommandLineApi.BuildCommandName} toolkit --asversion {versionInstruction}");
            var latestVersion = this.setup.Pattern.ToolkitVersion.Current;

            var location = GetFilePathOfExportedToolkit($"APattern_{latestVersion}.toolkit");
            this.setup.RunCommand($"{CommandLineApi.InstallCommandName} toolkit {location}");

            this.setup.Should().DisplayNoError();

            return location;
        }
    }
}