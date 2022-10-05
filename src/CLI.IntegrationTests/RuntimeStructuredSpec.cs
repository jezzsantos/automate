using System;
using System.Collections.Generic;
using System.Linq;
using Automate.Authoring.Domain;
using Automate.CLI.Infrastructure;
using Automate.Common;
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
        public void WhenCreateDraft_ThenDisplaysConfiguration()
        {
            ConfigureBuildAndInstallToolkit();
            this.setup.RunCommand($"{CommandLineApi.RunCommandName} toolkit APattern --output-structured");

            var draft = this.setup.Draft;
            var draftConfiguration = new StructuredMessage
            {
                Message = OutputMessages.CommandLine_Output_CreateDraftFromToolkit,
                Values = new Dictionary<string, object>
                {
                    { "DraftName", draft.Name },
                    { "DraftId", draft.Id },
                    { "ToolkitName", draft.Toolkit.PatternName },
                    { "ToolkitId", draft.Toolkit.Id },
                    { "ToolkitVersion", draft.Toolkit.Version },
                    { "RuntimeVersion", draft.Toolkit.RuntimeVersion }
                }
            };

            var structuredOutput = new StructuredOutput
            {
                Info = new List<string>(),
                Output = new List<StructuredMessage>
                {
                    draftConfiguration
                }
            }.ToJson();
            this.setup.Should().DisplayNoError();
            this.setup.Should().DisplayOutput(structuredOutput);
        }

        [Fact]
        public void WhenSwitchDraft_ThenDisplaysConfiguration()
        {
            ConfigureBuildAndInstallToolkit();
            this.setup.RunCommand($"{CommandLineApi.RunCommandName} toolkit APattern");
            this.setup.RunCommand($"{CommandLineApi.RunCommandName} toolkit APattern");

            var draft = this.setup.Drafts.First();

            this.setup.RunCommand($"{CommandLineApi.RunCommandName} switch \"{draft.Id}\" --output-structured");

            var draftConfiguration = new StructuredMessage
            {
                Message = OutputMessages.CommandLine_Output_DraftSwitched,
                Values = new Dictionary<string, object>
                {
                    { "DraftName", draft.Name },
                    { "DraftId", draft.Id },
                    { "ToolkitName", draft.Toolkit.PatternName },
                    { "ToolkitId", draft.Toolkit.Id },
                    { "ToolkitVersion", draft.Toolkit.Version },
                    { "RuntimeVersion", draft.Toolkit.RuntimeVersion }
                }
            };

            var structuredOutput = new StructuredOutput
            {
                Info = new List<string>(),
                Output = new List<StructuredMessage>
                {
                    draftConfiguration
                }
            }.ToJson();
            this.setup.Should().DisplayNoError();
            this.setup.Should().DisplayOutput(structuredOutput);
        }

        [Fact]
        public void WhenViewDraft_ThenDisplaysConfiguration()
        {
            CreateDraftFromBuiltToolkit();
            this.setup.RunCommand(
                $"{CommandLineApi.ConfigureCommandName} on {{APattern}} --and-set \"AProperty1=avalue1\"");
            this.setup.RunCommand(
                $"{CommandLineApi.ConfigureCommandName} add-one-to {{APattern.AnElement1.ACollection1}}");

            this.setup.RunCommand($"{CommandLineApi.ViewCommandName} draft --output-structured");

            var draft = this.setup.Draft;
            var draftConfiguration = new StructuredMessage
            {
                Message = OutputMessages.CommandLine_Output_DraftConfiguration,
                Values = new Dictionary<string, object>
                {
                    { "DraftName", draft.Name },
                    { "DraftId", draft.Id },
                    { "ToolkitVersion", draft.Toolkit.Version },
                    { "RuntimeVersion", draft.Toolkit.RuntimeVersion },
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

            var structuredOutput = new StructuredOutput
            {
                Info = new List<string>
                {
                    $"Information: {OutputMessages.CommandLine_Output_Preamble_CurrentDraftInUse.SubstituteTemplate(draft.Name, draft.Id)}"
                },
                Output = new List<StructuredMessage>
                {
                    draftConfiguration
                }
            }.ToJson();
            this.setup.Should().DisplayNoError();
            this.setup.Should().DisplayOutput(structuredOutput);
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
                    { "DraftName", draft.Name },
                    { "DraftId", draft.Id },
                    { "ToolkitVersion", draft.Toolkit.Version },
                    { "RuntimeVersion", draft.Toolkit.RuntimeVersion },
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
                Message = OutputMessages.CommandLine_Output_PatternSchema,
                Values = new Dictionary<string, object>
                {
                    { "Name", "APattern" },
                    { "PatternId", pattern.Id },
                    { "Version", "0.1.0" },
                    {
                        "Schema", new Dictionary<string, object>
                        {
                            { "Id", pattern.Id },
                            { "EditPath", "{APattern}" },
                            { "Name", "APattern" },
                            { "DisplayName", "APattern" },
                            { "Description", "" },
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
                                        { "DisplayName", "AnElement1" },
                                        { "Description", "" },
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
                                                    { "DisplayName", "ACollection1" },
                                                    { "Description", "" },
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
                                                                { "DisplayName", "AnElement3" },
                                                                { "Description", "" },
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
                                        { "DisplayName", "AnElement2" },
                                        { "Description", "" },
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
                            { "Name", "APattern" },
                            { "DisplayName", "APattern" },
                            { "Description", "" }
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
                    { "Id", draft.Id },
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
                            { "Name", "AnElement1" },
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
                            { "Name", "AnElement2" },
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
                            { "Name", "ACollection1" },
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

        [Fact]
        public void WhenValidateAndHasValidationErrors_ThenDisplaysValidations()
        {
            CreateDraftFromBuiltToolkit(TestCaseSetup.Automation);
            this.setup.RunCommand($"{CommandLineApi.ValidateCommandName} draft --output-structured");

            var draft = this.setup.Draft;
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
                        Message = OutputMessages.CommandLine_Output_DraftValidationFailed,
                        Values = new Dictionary<string, object>
                        {
                            { "Name", draft.Name },
                            { "Id", draft.Id },
                            {
                                "Errors", new[]
                                {
                                    new
                                    {
                                        Context = new
                                        {
                                            Path = "{APattern.AProperty1}"
                                        },
                                        Message = ValidationMessages.Attribute_ValidationRule_RequiredAttributeValue
                                    },
                                    new
                                    {
                                        Context = new
                                        {
                                            Path = "{APattern.AnElement1}"
                                        },
                                        Message = ValidationMessages
                                            .DraftItem_ValidationRule_ElementRequiresAtLeastOneInstance
                                    },
                                    new
                                    {
                                        Context = new
                                        {
                                            Path = "{APattern.AnElement1.ACollection1}"
                                        },
                                        Message = ValidationMessages
                                            .DraftItem_ValidationRule_ElementRequiresAtLeastOneInstance
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

        [Fact]
        public void WhenExecuteLaunchPointAndHasValidationErrors_ThenDisplaysValidations()
        {
            CreateDraftFromBuiltToolkit(TestCaseSetup.Automation);
            this.setup.RunCommand($"{CommandLineApi.ExecuteCommandName} command ALaunchPoint1 --output-structured");

            var draft = this.setup.Draft;
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
                        Message = OutputMessages.CommandLine_Output_CommandExecutionFailed_WithValidation,
                        Values = new Dictionary<string, object>
                        {
                            { "Command", "ALaunchPoint1" },
                            {
                                "ValidationErrors", new[]
                                {
                                    new
                                    {
                                        Context = new
                                        {
                                            Path = "{APattern.AProperty1}"
                                        },
                                        Message = ValidationMessages.Attribute_ValidationRule_RequiredAttributeValue
                                    },
                                    new
                                    {
                                        Context = new
                                        {
                                            Path = "{APattern.AnElement1}"
                                        },
                                        Message = ValidationMessages
                                            .DraftItem_ValidationRule_ElementRequiresAtLeastOneInstance
                                    },
                                    new
                                    {
                                        Context = new
                                        {
                                            Path = "{APattern.AnElement1.ACollection1}"
                                        },
                                        Message = ValidationMessages
                                            .DraftItem_ValidationRule_ElementRequiresAtLeastOneInstance
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

        [Fact]
        public void WhenExecuteLaunchPointAndFails_ThenDisplaysResults()
        {
            CreateDraftFromBuiltToolkit(TestCaseSetup.Automation);
            this.setup.RunCommand(
                $"{CommandLineApi.ConfigureCommandName} on {{APattern}} --and-set \"AProperty1=avalue1\"");
            this.setup.RunCommand(
                $"{CommandLineApi.ConfigureCommandName} add {{AnElement1}}");
            this.setup.RunCommand($"{CommandLineApi.ConfigureCommandName} add-one-to {{AnElement1.ACollection1}}");

            this.setup.RunCommand($"{CommandLineApi.ExecuteCommandName} command ALaunchPoint2 --output-structured");

            var path = RuntimeSpec.GetFilePathInOutput(@"code/namingtest.cs");
            var command = this.setup.Pattern.Automation[2];
            var codeTemplate = this.setup.Pattern.CodeTemplates.Single();
            var draft = this.setup.Draft;
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
                        Message = OutputMessages.CommandLine_Output_CommandExecutionFailed,
                        Values = new Dictionary<string, object>
                        {
                            { "Command", "ALaunchPoint2" },
                            {
                                "Log", new[]
                                {
                                    new
                                    {
                                        Message = InfrastructureMessages.CodeTemplateCommand_Log_GeneratedFile
                                            .Substitute(
                                                "namingtest.cs",
                                                codeTemplate.Id, path),
                                        Type = CommandExecutionLogItemType.Succeeded.ToString()
                                    },
                                    new
                                    {
                                        Message = InfrastructureMessages.CommandLaunchPoint_CommandIdFailedExecution
                                            .Substitute(
                                                command.Id,
                                                ExceptionMessages.TextTemplatingExtensions_HasSyntaxErrors.Substitute(
                                                    InfrastructureMessages
                                                        .CodeTemplateCommand_FilePathExpression_Description
                                                        .Substitute(
                                                            command.Metadata[nameof(CodeTemplateCommand.FilePath)]),
                                                    $"((20:0,20),(21:0,21)): Invalid token `CodeExit`. The dot operator is expected to be followed by a plain identifier{Environment.NewLine}" +
                                                    "((19:0,19),(19:0,19)): Invalid token found `.`. Expecting <EOL>/end of line."
                                                )),
                                        Type = CommandExecutionLogItemType.Failed.ToString()
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

        [Fact]
        public void WhenExecuteLaunchPointAndSucceeds_ThenDisplaysSuccess()
        {
            CreateDraftFromBuiltToolkit(TestCaseSetup.Automation);
            this.setup.RunCommand(
                $"{CommandLineApi.ConfigureCommandName} on {{APattern}} --and-set \"AProperty1=avalue1\"");
            this.setup.RunCommand(
                $"{CommandLineApi.ConfigureCommandName} add {{AnElement1}}");
            this.setup.RunCommand($"{CommandLineApi.ConfigureCommandName} add-one-to {{AnElement1.ACollection1}}");

            this.setup.RunCommand($"{CommandLineApi.ExecuteCommandName} command ALaunchPoint1 --output-structured");

            var path = RuntimeSpec.GetFilePathInOutput(@"code/namingtest.cs");
            var codeTemplate = this.setup.Pattern.CodeTemplates.Single();
            var draft = this.setup.Draft;
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
                        Message = OutputMessages.CommandLine_Output_CommandExecutionSucceeded,
                        Values = new Dictionary<string, object>
                        {
                            { "Command", "ALaunchPoint1" },
                            {
                                "Log", new[]
                                {
                                    new
                                    {
                                        Message = InfrastructureMessages.CodeTemplateCommand_Log_GeneratedFile
                                            .Substitute(
                                                "namingtest.cs",
                                                codeTemplate.Id, path),
                                        Type = CommandExecutionLogItemType.Succeeded.ToString()
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

        [Fact]
        public void WhenUpgradeDraftAndToolkitNotUpgraded_ThenDisplaysWarning()
        {
            CreateDraftFromBuiltToolkit();
            this.setup.RunCommand(
                $"{CommandLineApi.ConfigureCommandName} on {{APattern}} --and-set \"AProperty1=avalue1\"");
            this.setup.RunCommand(
                $"{CommandLineApi.ConfigureCommandName} add-one-to {{APattern.AnElement1.ACollection1}}");

            this.setup.RunCommand($"{CommandLineApi.UpgradeCommandName} draft --output-structured");

            var draft = this.setup.Draft;
            var upgrade = new StructuredMessage
            {
                Message = OutputMessages.CommandLine_Output_DraftUpgradeWithWarning,
                Values = new Dictionary<string, object>
                {
                    { "DraftName", draft.Name },
                    { "DraftId", draft.Id },
                    { "ToolkitName", draft.Toolkit.PatternName },
                    { "OldVersion", draft.Toolkit.Version },
                    { "NewVersion", draft.Toolkit.Version },
                    {
                        "Log", new[]
                        {
                            new
                            {
                                Type = MigrationChangeType.Abort.ToString(),
                                MessageTemplate = MigrationMessages.DraftDefinition_Upgrade_SameToolkitVersion,
                                Arguments = new Dictionary<string, object>
                                {
                                    { "Name", draft.Toolkit.PatternName },
                                    { "Version", draft.Toolkit.Version }
                                }
                            }
                        }
                    }
                }
            };

            var structuredOutput = new StructuredOutput
            {
                Info = new List<string>(),
                Output = new List<StructuredMessage>
                {
                    upgrade
                }
            }.ToJson();
            this.setup.Should().DisplayNoError();
            this.setup.Should().DisplayOutput(structuredOutput);
        }

        [Fact]
        public void WhenViewToolkit_ThenDisplaysSchema()
        {
            CreateDraftFromBuiltToolkit();

            this.setup.RunCommand(
                $"{CommandLineApi.ViewCommandName} toolkit --output-structured");

            var pattern = this.setup.Pattern;
            var toolkit = this.setup.Toolkit;
            var patternSchema = new StructuredMessage
            {
                Message = OutputMessages.CommandLine_Output_ToolkitSchema,
                Values = new Dictionary<string, object>
                {
                    { "Name", "APattern" },
                    { "Id", toolkit.Id },
                    { "Version", toolkit.Version },
                    { "RuntimeVersion", toolkit.RuntimeVersion },
                    {
                        "Schema", new Dictionary<string, object>
                        {
                            { "Id", pattern.Id },
                            { "EditPath", "{APattern}" },
                            { "Name", "APattern" },
                            { "DisplayName", "APattern" },
                            { "Description", "" },
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
                                        { "DisplayName", "AnElement1" },
                                        { "Description", "" },
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
                                                    { "DisplayName", "ACollection1" },
                                                    { "Description", "" },
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
                                                                { "DisplayName", "AnElement3" },
                                                                { "Description", "" },
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
                                        { "DisplayName", "AnElement2" },
                                        { "Description", "" },
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
            var structuredOutput = new StructuredOutput
            {
                Info = new List<string>(),
                Output = new List<StructuredMessage>
                {
                    patternSchema
                }
            }.ToJson();
            this.setup.Should().DisplayNoError();
            this.setup.Should().DisplayOutput(structuredOutput);
        }

        public void Dispose()
        {
            this.setup.Reset();
        }

        private void CreateDraftFromBuiltToolkit(TestCaseSetup testCase = TestCaseSetup.Normal)
        {
            ConfigureBuildAndInstallToolkit(testCase);
            this.setup.RunCommand($"{CommandLineApi.RunCommandName} toolkit APattern");
        }

        private void ConfigureBuildAndInstallToolkit(TestCaseSetup testCase = TestCaseSetup.Normal)
        {
            this.setup.RunCommand($"{CommandLineApi.CreateCommandName} pattern APattern");
            if (testCase == TestCaseSetup.Normal)
            {
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
            }
            if (testCase == TestCaseSetup.Automation)
            {
                this.setup.RunCommand(
                    $"{CommandLineApi.EditCommandName} add-attribute AProperty1 --isrequired");
                this.setup.RunCommand($"{CommandLineApi.EditCommandName} add-element AnElement1 --autocreate false");
                this.setup.RunCommand(
                    $"{CommandLineApi.EditCommandName} add-collection ACollection1 --isrequired --aschildof {{APattern.AnElement1}}");

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
                    $"{CommandLineApi.EditCommandName} add-command-launchpoint {commandId1};{commandId2} --name ALaunchPoint2");
            }

            BuildAndInstallToolkit();
        }

        private void BuildAndInstallToolkit()
        {
            this.setup.RunCommand($"{CommandLineApi.BuildCommandName} toolkit");
            var latestVersion = this.setup.Pattern.ToolkitVersion.Current;

            var location = RuntimeSpec.GetFilePathOfExportedToolkit($"APattern_{latestVersion}.toolkit");
            this.setup.RunCommand($"{CommandLineApi.InstallCommandName} toolkit {location}");
        }

        private enum TestCaseSetup
        {
            Normal = 0,
            Automation = 1
        }
    }
}