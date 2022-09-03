using System;
using System.Collections.Generic;
using Automate.Authoring.Domain;
using Automate.CLI.Infrastructure;
using Automate.Common.Domain;
using Automate.Common.Extensions;
using Automate.Runtime.Domain;
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
        public void WhenViewDraftWithTodo_ThenDisplaysConfigurationSchemaAndValidation()
        {
            CreateDraftFromBuiltToolkit();
            this.setup.RunCommand(
                $"{CommandLineApi.ConfigureCommandName} on {{APattern}} --and-set \"AProperty1=avalue1\"");
            this.setup.RunCommand(
                $"{CommandLineApi.ConfigureCommandName} add-one-to {{APattern.AnElement1.ACollection1}}");

            this.setup.RunCommand($"{CommandLineApi.ViewCommandName} draft --todo --output-structured");

            var draft = this.setup.Draft;
            var pattern = this.setup.Pattern;
            var draftConfiguration = new StructuredMessage
            {
                Message = OutputMessages.CommandLine_Output_DraftConfiguration,
                Values = new Dictionary<string, object>
                {
                    { "Name", draft.Name },
                    { "DraftId", draft.Id },
                    {
                        "Configuration", new Dictionary<string, object>
                        {
                            { "Id", draft.Model.Id },
                            { "ConfigurePath", "{APattern}" },
                            {
                                "Schema", new Dictionary<string, object>
                                {
                                    { "Id", draft.Model.Schema.SchemaId },
                                    { "Type", "Pattern" }
                                }
                            },
                            { "AProperty1", "avalue1" },
                            {
                                "AnElement1", new Dictionary<string, object>
                                {
                                    { "Id", draft.Model.Properties["AnElement1"].Id },
                                    { "ConfigurePath", "{APattern.AnElement1}" },
                                    {
                                        "Schema", new Dictionary<string, object>
                                        {
                                            { "Id", draft.Model.Properties["AnElement1"].Schema.SchemaId },
                                            { "Type", "Element" }
                                        }
                                    },
                                    { "AProperty2", "ADefaultValue2" },
                                    { "AProperty3", "ADefaultValue3" },
                                    {
                                        "ACollection1", new Dictionary<string, object>
                                        {
                                            {
                                                "Id", draft.Model.Properties["AnElement1"].Properties["ACollection1"].Id
                                            },
                                            { "ConfigurePath", "{APattern.AnElement1.ACollection1}" },
                                            {
                                                "Schema", new Dictionary<string, object>
                                                {
                                                    {
                                                        "Id",
                                                        draft.Model.Properties["AnElement1"].Properties["ACollection1"]
                                                            .Schema.SchemaId
                                                    },
                                                    { "Type", "EphemeralCollection" }
                                                }
                                            },
                                            {
                                                "Items", new List<Dictionary<string, object>>
                                                {
                                                    new()
                                                    {
                                                        {
                                                            "Id",
                                                            draft.Model.Properties["AnElement1"]
                                                                .Properties["ACollection1"].Items[0].Id
                                                        },
                                                        {
                                                            "ConfigurePath",
                                                            $"{{APattern.AnElement1.ACollection1.{draft.Model.Properties["AnElement1"].Properties["ACollection1"].Items[0].Id}}}"
                                                        },
                                                        {
                                                            "Schema", new Dictionary<string, object>
                                                            {
                                                                {
                                                                    "Id",
                                                                    draft.Model.Properties["AnElement1"]
                                                                        .Properties["ACollection1"].Items[0].Schema
                                                                        .SchemaId
                                                                },
                                                                { "Type", "CollectionItem" }
                                                            }
                                                        },
                                                        { "AProperty4", "ADefaultValue4" },
                                                        {
                                                            "AnElement3", new Dictionary<string, object>
                                                            {
                                                                {
                                                                    "Id",
                                                                    draft.Model.Properties["AnElement1"]
                                                                        .Properties["ACollection1"].Items[0]
                                                                        .Properties["AnElement3"].Id
                                                                },
                                                                {
                                                                    "ConfigurePath",
                                                                    $"{{APattern.AnElement1.ACollection1.{draft.Model.Properties["AnElement1"].Properties["ACollection1"].Items[0].Id}.AnElement3}}"
                                                                },
                                                                {
                                                                    "Schema", new Dictionary<string, object>
                                                                    {
                                                                        {
                                                                            "Id",
                                                                            draft.Model.Properties["AnElement1"]
                                                                                .Properties["ACollection1"].Items[0]
                                                                                .Properties["AnElement3"].Schema
                                                                                .SchemaId
                                                                        },
                                                                        { "Type", "Element" }
                                                                    }
                                                                },
                                                                { "AProperty5", "ADefaultValue5" }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };
            var patternSchema = new StructuredMessage
            {
                Message = OutputMessages.CommandLine_Output_PatternConfiguration,
                Values = new Dictionary<string, object>
                {
                    { "Name", "APattern" },
                    { "PatternId", pattern.Id },
                    { "Version", "0.1.0" },
                    {
                        "Tree", new Dictionary<string, object>
                        {
                            { "Id", pattern.Id },
                            { "EditPath", "{APattern}" },
                            { "Name", "APattern" },
                            { "CodeTemplates", Array.Empty<Dictionary<string, object>>() },
                            { "Automation", Array.Empty<Dictionary<string, object>>() },
                            {
                                "Attributes", new List<Dictionary<string, object>>
                                {
                                    new()
                                    {
                                        { "Id", pattern.Attributes[0].Id },
                                        { "Name", "AProperty1" },
                                        { "DataType", "string" },
                                        { "IsRequired", false },
                                        { "Choices", Array.Empty<string>() },
                                        { "DefaultValue", "ADefaultValue1" }
                                    }
                                }
                            },
                            {
                                "Elements", new List<Dictionary<string, object>>
                                {
                                    new()
                                    {
                                        { "Id", pattern.Elements[0].Id },
                                        { "EditPath", "{APattern.AnElement1}" },
                                        { "Name", "AnElement1" },
                                        { "AutoCreate", true },
                                        { "IsCollection", false },
                                        { "Cardinality", "One" },
                                        { "CodeTemplates", Array.Empty<Dictionary<string, object>>() },
                                        { "Automation", Array.Empty<Dictionary<string, object>>() },
                                        {
                                            "Attributes", new List<Dictionary<string, object>>
                                            {
                                                new()
                                                {
                                                    { "Id", pattern.Elements[0].Attributes[0].Id },
                                                    { "Name", "AProperty2" },
                                                    { "DataType", "string" },
                                                    { "IsRequired", false },
                                                    { "Choices", Array.Empty<string>() },
                                                    { "DefaultValue", "ADefaultValue2" }
                                                },
                                                new()
                                                {
                                                    { "Id", pattern.Elements[0].Attributes[1].Id },
                                                    { "Name", "AProperty3" },
                                                    { "DataType", "string" },
                                                    { "IsRequired", false },
                                                    { "Choices", Array.Empty<string>() },
                                                    { "DefaultValue", "ADefaultValue3" }
                                                }
                                            }
                                        },
                                        {
                                            "Elements", new List<Dictionary<string, object>>
                                            {
                                                new()
                                                {
                                                    { "Id", pattern.Elements[0].Elements[0].Id },
                                                    { "EditPath", "{APattern.AnElement1.ACollection1}" },
                                                    { "Name", "ACollection1" },
                                                    { "AutoCreate", false },
                                                    { "IsCollection", true },
                                                    { "Cardinality", "ZeroOrMany" },
                                                    { "CodeTemplates", Array.Empty<Dictionary<string, object>>() },
                                                    { "Automation", Array.Empty<Dictionary<string, object>>() },
                                                    {
                                                        "Attributes", new List<Dictionary<string, object>>
                                                        {
                                                            new()
                                                            {
                                                                {
                                                                    "Id",
                                                                    pattern.Elements[0].Elements[0].Attributes[0].Id
                                                                },
                                                                { "Name", "AProperty4" },
                                                                { "DataType", "string" },
                                                                { "IsRequired", false },
                                                                { "Choices", Array.Empty<string>() },
                                                                { "DefaultValue", "ADefaultValue4" }
                                                            }
                                                        }
                                                    },
                                                    {
                                                        "Elements", new List<Dictionary<string, object>>
                                                        {
                                                            new()
                                                            {
                                                                {
                                                                    "Id", pattern.Elements[0].Elements[0].Elements[0].Id
                                                                },
                                                                {
                                                                    "EditPath",
                                                                    "{APattern.AnElement1.ACollection1.AnElement3}"
                                                                },
                                                                { "Name", "AnElement3" },
                                                                { "AutoCreate", true },
                                                                { "IsCollection", false },
                                                                { "Cardinality", "One" },
                                                                {
                                                                    "CodeTemplates",
                                                                    Array.Empty<Dictionary<string, object>>()
                                                                },
                                                                {
                                                                    "Automation",
                                                                    Array.Empty<Dictionary<string, object>>()
                                                                },
                                                                {
                                                                    "Attributes", new List<Dictionary<string, object>>
                                                                    {
                                                                        new()
                                                                        {
                                                                            {
                                                                                "Id",
                                                                                pattern.Elements[0].Elements[0]
                                                                                    .Elements[0].Attributes[0].Id
                                                                            },
                                                                            { "Name", "AProperty5" },
                                                                            { "DataType", "string" },
                                                                            { "IsRequired", false },
                                                                            { "Choices", Array.Empty<string>() },
                                                                            { "DefaultValue", "ADefaultValue5" }
                                                                        }
                                                                    }
                                                                },
                                                                {
                                                                    "Elements",
                                                                    Array.Empty<Dictionary<string, object>>()
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    },
                                    new()
                                    {
                                        { "Id", pattern.Elements[1].Id },
                                        { "EditPath", "{APattern.AnElement2}" },
                                        { "Name", "AnElement2" },
                                        { "AutoCreate", false },
                                        { "IsCollection", false },
                                        { "Cardinality", "One" },
                                        { "CodeTemplates", Array.Empty<Dictionary<string, object>>() },
                                        { "Automation", Array.Empty<Dictionary<string, object>>() },
                                        {
                                            "Attributes", new List<Dictionary<string, object>>
                                            {
                                                new()
                                                {
                                                    {
                                                        "Id",
                                                        pattern.Elements[1].Attributes[0].Id
                                                    },
                                                    { "Name", "AProperty6" },
                                                    { "DataType", "string" },
                                                    { "IsRequired", false },
                                                    { "Choices", Array.Empty<string>() },
                                                    { "DefaultValue", "ADefaultValue6" }
                                                }
                                            }
                                        },
                                        { "Elements", new List<Dictionary<string, object>>() }
                                    }
                                }
                            }
                        }
                    }
                }
            };
            var launchPoints = new StructuredMessage
            {
                Message = OutputMessages.CommandLine_Output_PatternLaunchableAutomation,
                Values = new Dictionary<string, object>
                {
                    { "Name", "APattern" },
                    { "PatternId", pattern.Id },
                    { "Version", "0.1.0" },
                    {
                        "LaunchPoints", new Dictionary<string, object>
                        {
                            { "Id", pattern.Id },
                            { "EditPath", "{APattern}" },
                            { "Name", "APattern" }
                        }
                    }
                }
            };
            var validations = new StructuredMessage
            {
                Message = OutputMessages.CommandLine_Output_DraftValidationFailed,
                Values = new Dictionary<string, object>
                {
                    { "Name", draft.Name },
                    { "DraftId", draft.Id },
                    {
                        "Errors", new List<ValidationResult>
                        {
                            new(new ValidationContext("{APattern.AnElement2}"),
                                ValidationMessages.DraftItem_ValidationRule_ElementRequiresAtLeastOneInstance)
                        }
                    }
                }
            };
            var structuredOutput = new StructuredOutput
            {
                Info = new List<string>
                {
                    $"Information: {OutputMessages.CommandLine_Output_Preamble_CurrentDraftInUse.SubstituteTemplate(draft.Name, draft.Id)}"
                },
                Output = new List<StructuredMessage>
                {
                    draftConfiguration,
                    patternSchema,
                    launchPoints,
                    validations
                }
            }.ToJson();
            this.setup.Should().DisplayNoError();
            this.setup.Should().DisplayOutput(structuredOutput);
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
                                    Schema = new
                                    {
                                        Id = draftElement.Schema.SchemaId,
                                        Type = DraftItemSchemaType.Element.ToString()
                                    },
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
                                    Schema = new
                                    {
                                        Id = draftElement.Schema.SchemaId,
                                        Type = DraftItemSchemaType.Element.ToString()
                                    },
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
                                    Schema = new
                                    {
                                        Id = draftCollectionItem.Schema.SchemaId,
                                        Type = DraftItemSchemaType.CollectionItem.ToString()
                                    },
                                    AProperty4 = "anewvalue",
                                    AnElement3 = new
                                    {
                                        draftCollectionItem.Properties["AnElement3"].Id,
                                        ConfigurePath =
                                            $"{{APattern.AnElement1.ACollection1.{draftCollectionItem.Id}.AnElement3}}",
                                        Schema = new
                                        {
                                            Id = draftCollectionItem.Properties["AnElement3"].Schema.SchemaId,
                                            Type = DraftItemSchemaType.Element.ToString()
                                        },
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
                $"{CommandLineApi.EditCommandName} add-element AnElement2 --aschildof {{APattern}} --isrequired --autocreate false");
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