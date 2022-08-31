using System;
using System.Collections.Generic;
using Automate.CLI.Infrastructure;
using Automate.Common.Extensions;
using Xunit;

namespace CLI.IntegrationTests
{
    [Trait("Category", "Integration")] [Collection("CLI")]
    public class RuntimeStructuredSpec : IDisposable
    {
        private readonly CliTestSetup setup;

        public RuntimeStructuredSpec(CliTestSetup setup)
        {
            this.setup = setup;
            this.setup.ResetRepository();
            RuntimeSpec.DeleteOutputFolders();
        }

        [Fact]
        public void WhenConfigureDraftAndSetPropertyOnElement_ThenDisplaysSuccess()
        {
            CreateDraftFromBuiltToolkit();

            this.setup.RunCommand(
                $"{CommandLineApi.ConfigureCommandName} on {{AnElement1}} --and-set \"AProperty2=anewvalue\" --output-structured");

            var draft = this.setup.Draft;
            var draftElement = draft.Model.Properties["AnElement1"];

            var structuredOutput = new StructuredOutput
            {
                Info = new List<string>
                {
                    $"Information: {OutputMessages.CommandLine_Output_Preamble_CurrentDraftInUse.SubstituteTemplate(draft.Name, draft.Id)}"
                },
                Output = new List<StructuredMessage>
                {
                    new()
                    {
                        Message = OutputMessages.CommandLine_Output_DraftConfigured,
                        Values = new Dictionary<string, object>
                        {
                            { "DraftName", "AnElement1" },
                            { "DraftItemId", draftElement.Id },
                            {
                                "Configuration", new
                                {
                                    draftElement.Id,
                                    ConfigurePath = "{APattern.AnElement1}",
                                    AProperty2 = "anewvalue",
                                    AProperty3 = "ADefaultValue3"
                                }
                            }
                        }
                    }
                }
            }.ToJson();
            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayOutput(structuredOutput);
        }

        [Fact]
        public void WhenConfigureDraftAndAddNewElement_ThenDisplaysSuccess()
        {
            CreateDraftFromBuiltToolkit();

            this.setup.RunCommand(
                $"{CommandLineApi.ConfigureCommandName} add {{AnElement2}} --and-set \"AProperty6=anewvalue\" --output-structured");

            var draft = this.setup.Draft;
            var draftElement = draft.Model.Properties["AnElement2"];

            var structuredOutput = new StructuredOutput
            {
                Info = new List<string>
                {
                    $"Information: {OutputMessages.CommandLine_Output_Preamble_CurrentDraftInUse.SubstituteTemplate(draft.Name, draft.Id)}"
                },
                Output = new List<StructuredMessage>
                {
                    new()
                    {
                        Message = OutputMessages.CommandLine_Output_DraftConfigured,
                        Values = new Dictionary<string, object>
                        {
                            { "DraftName", "AnElement2" },
                            { "DraftItemId", draftElement.Id },
                            {
                                "Configuration", new
                                {
                                    draftElement.Id,
                                    ConfigurePath = "{APattern.AnElement2}",
                                    AProperty6 = "anewvalue"
                                }
                            }
                        }
                    }
                }
            }.ToJson();
            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayOutput(structuredOutput);
        }

        [Fact]
        public void WhenConfigureDraftAndAddNewCollectionItem_ThenDisplaysSuccess()
        {
            CreateDraftFromBuiltToolkit();

            this.setup.RunCommand(
                $"{CommandLineApi.ConfigureCommandName} add-one-to {{AnElement1.ACollection1}} --and-set \"AProperty4=anewvalue\" --output-structured");

            var draft = this.setup.Draft;
            var draftCollection = draft.Model.Properties["AnElement1"].Properties["ACollection1"];
            var draftCollectionItem = draftCollection.Items[0];

            var structuredOutput = new StructuredOutput
            {
                Info = new List<string>
                {
                    $"Information: {OutputMessages.CommandLine_Output_Preamble_CurrentDraftInUse.SubstituteTemplate(draft.Name, draft.Id)}"
                },
                Output = new List<StructuredMessage>
                {
                    new()
                    {
                        Message = OutputMessages.CommandLine_Output_DraftConfigured,
                        Values = new Dictionary<string, object>
                        {
                            { "DraftName", "ACollection1" },
                            { "DraftItemId", draftCollectionItem.Id },
                            {
                                "Configuration", new
                                {
                                    draftCollectionItem.Id,
                                    ConfigurePath = $"{{APattern.AnElement1.ACollection1.{draftCollectionItem.Id}}}",
                                    AProperty4 = "anewvalue",
                                    AnElement3 = new
                                    {
                                        draftCollectionItem.Properties["AnElement3"].Id,
                                        ConfigurePath =
                                            $"{{APattern.AnElement1.ACollection1.{draftCollectionItem.Id}.AnElement3}}",
                                        AProperty5 = "ADefaultValue5"
                                    }
                                }
                            }
                        }
                    }
                }
            }.ToJson();
            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayOutput(structuredOutput);
        }

        public void Dispose()
        {
            this.setup.Reset();
        }

        private void CreateDraftFromBuiltToolkit()
        {
            ConfigureBuildAndInstallToolkit();
            this.setup.RunCommand($"{CommandLineApi.RunCommandName} toolkit APattern");
        }

        private void ConfigureBuildAndInstallToolkit()
        {
            this.setup.RunCommand($"{CommandLineApi.CreateCommandName} pattern APattern");
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-attribute AProperty1 --defaultvalueis ADefaultValue1");
            this.setup.RunCommand($"{CommandLineApi.EditCommandName} add-element AnElement1");
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-attribute AProperty2 --aschildof {{APattern.AnElement1}} --defaultvalueis ADefaultValue2");
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-attribute AProperty3 --aschildof {{APattern.AnElement1}} --defaultvalueis ADefaultValue3");
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-collection ACollection1 --aschildof {{APattern.AnElement1}}");
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-attribute AProperty4 --aschildof {{APattern.AnElement1.ACollection1}} --defaultvalueis ADefaultValue4");
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-element AnElement3 --aschildof {{APattern.AnElement1.ACollection1}}");
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-attribute AProperty5 --aschildof {{APattern.AnElement1.ACollection1.AnElement3}} --defaultvalueis ADefaultValue5");
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-element AnElement2 --aschildof {{APattern}} --autocreate false");
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-attribute AProperty6 --aschildof {{APattern.AnElement2}} --defaultvalueis ADefaultValue6");

            BuildAndInstallToolkit();
        }

        private void BuildAndInstallToolkit()
        {
            this.setup.RunCommand($"{CommandLineApi.BuildCommandName} toolkit");
            var latestVersion = this.setup.Pattern.ToolkitVersion.Current;

            var location = RuntimeSpec.GetFilePathOfExportedToolkit($"APattern_{latestVersion}.toolkit");
            this.setup.RunCommand($"{CommandLineApi.InstallCommandName} toolkit {location}");
        }
    }
}