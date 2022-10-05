using System;
using System.Collections.Generic;
using System.Linq;
using Automate.Authoring.Domain;
using Automate.CLI.Infrastructure;
using Automate.Common.Extensions;
using Xunit;

namespace CLI.IntegrationTests
{
    [Trait("Category", "Integration")] [Collection("CLI")]
    public class AuthoringStructuredSpec : IDisposable
    {
        private readonly CliTestSetup setup;

        public AuthoringStructuredSpec(CliTestSetup setup)
        {
            this.setup = setup;
            this.setup.ResetRepository();
            RuntimeSpec.DeleteOutputFolders();
        }

        
        [Fact]
        public void WhenListAllAndSome_ThenDisplaysLists()
        {
            this.setup.RunCommand($"{CommandLineApi.CreateCommandName} pattern APattern1");
            this.setup.RunCommand($"{CommandLineApi.BuildCommandName} pattern --install");
            this.setup.RunCommand($"{CommandLineApi.RunCommandName} toolkit APattern1");

            this.setup.RunCommand($"{CommandLineApi.ListCommandName} all --output-structured");

            var pattern = this.setup.Pattern;
            var toolkit = this.setup.Toolkit;
            var draft = this.setup.Draft;

            var structuredOutput = new StructuredOutput
            {
                Info = new List<string>(),
                Output = new List<StructuredMessage>
                {
                    new()
                    {
                        Message = OutputMessages.CommandLine_Output_EditablePatternsListed,
                        Values = new Dictionary<string, object>
                        {
                            {
                                "Patterns", new List<Dictionary<string, object>>
                                {
                                    new()
                                    {
                                        { "Id", pattern.Id },
                                        { "Name", pattern.Name },
                                        { "Version", "0.1.0" },
                                        { "IsCurrent", true }
                                    }
                                }
                            }
                        }
                    },
                    new()
                    {
                        Message = OutputMessages.CommandLine_Output_InstalledToolkitsListed,
                        Values = new Dictionary<string, object>
                        {
                            {
                                "Toolkits", new List<Dictionary<string, object>>
                                {
                                    new()
                                    {
                                        { "Id", toolkit.Id },
                                        { "PatternName", "APattern1" },
                                        {
                                            "Version", new Dictionary<string, object>
                                            {
                                                {
                                                    "Toolkit", new Dictionary<string, object>
                                                    {
                                                        { "Created", toolkit.Version },
                                                        { "Installed", toolkit.Version }
                                                    }
                                                },
                                                {
                                                    "Runtime", new Dictionary<string, object>
                                                    {
                                                        { "Created", toolkit.RuntimeVersion },
                                                        { "Installed", toolkit.RuntimeVersion }
                                                    }
                                                },
                                                {
                                                    "Compatibility",
                                                    ToolkitRuntimeVersionCompatibility.Compatible.ToString()
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
                        Message = OutputMessages.CommandLine_Output_ConfiguredDraftsListed,
                        Values = new Dictionary<string, object>
                        {
                            {
                                "Drafts", new List<Dictionary<string, object>>
                                {
                                    new()
                                    {
                                        { "DraftId", draft.Id },
                                        { "DraftName", draft.Name },
                                        { "ToolkitId", toolkit.Id },
                                        {
                                            "ToolkitVersion", new Dictionary<string, object>
                                            {
                                                {
                                                    "DraftCompatibility",
                                                    DraftToolkitVersionCompatibility.Compatible.ToString()
                                                },
                                                {
                                                    "Toolkit", new Dictionary<string, object>
                                                    {
                                                        { "Created", toolkit.Version },
                                                        { "Installed", toolkit.Version }
                                                    }
                                                },
                                                {
                                                    "Runtime", new Dictionary<string, object>
                                                    {
                                                        { "Created", toolkit.RuntimeVersion },
                                                        { "Installed", toolkit.RuntimeVersion }
                                                    }
                                                },
                                                {
                                                    "Compatibility",
                                                    ToolkitRuntimeVersionCompatibility.Compatible.ToString()
                                                }
                                            }
                                        },
                                        { "IsCurrent", true }
                                    }
                                }
                            }
                        }
                    }
                }
            }.ToJson();
            this.setup.Should().DisplayNoError();
            this.setup.Should().DisplayOutput(structuredOutput);
        }

        [Fact]
        public void WhenListAllAndToolkitUpgraded_ThenDisplaysLists()
        {
            this.setup.RunCommand($"{CommandLineApi.CreateCommandName} pattern APattern1");
            this.setup.RunCommand($"{CommandLineApi.BuildCommandName} pattern --install");
            this.setup.RunCommand($"{CommandLineApi.RunCommandName} toolkit APattern1");

            this.setup.RunCommand($"{CommandLineApi.PublishCommandName} toolkit --install --asversion 2.0.0");

            this.setup.RunCommand($"{CommandLineApi.ListCommandName} all --output-structured");

            var pattern = this.setup.Pattern;
            var toolkit = this.setup.Toolkit;
            var draft = this.setup.Draft;

            var structuredOutput = new StructuredOutput
            {
                Info = new List<string>(),
                Output = new List<StructuredMessage>
                {
                    new()
                    {
                        Message = OutputMessages.CommandLine_Output_EditablePatternsListed,
                        Values = new Dictionary<string, object>
                        {
                            {
                                "Patterns", new List<Dictionary<string, object>>
                                {
                                    new()
                                    {
                                        { "Id", pattern.Id },
                                        { "Name", pattern.Name },
                                        { "Version", "2.0.0" },
                                        { "IsCurrent", true }
                                    }
                                }
                            }
                        }
                    },
                    new()
                    {
                        Message = OutputMessages.CommandLine_Output_InstalledToolkitsListed,
                        Values = new Dictionary<string, object>
                        {
                            {
                                "Toolkits", new List<Dictionary<string, object>>
                                {
                                    new()
                                    {
                                        { "Id", toolkit.Id },
                                        { "PatternName", "APattern1" },
                                        {
                                            "Version", new Dictionary<string, object>
                                            {
                                                {
                                                    "Toolkit", new Dictionary<string, object>
                                                    {
                                                        { "Created", "2.0.0" },
                                                        { "Installed", "2.0.0" }
                                                    }
                                                },
                                                {
                                                    "Runtime", new Dictionary<string, object>
                                                    {
                                                        { "Created", toolkit.RuntimeVersion },
                                                        { "Installed", toolkit.RuntimeVersion }
                                                    }
                                                },
                                                {
                                                    "Compatibility",
                                                    ToolkitRuntimeVersionCompatibility.Compatible.ToString()
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
                        Message = OutputMessages.CommandLine_Output_ConfiguredDraftsListed,
                        Values = new Dictionary<string, object>
                        {
                            {
                                "Drafts", new List<Dictionary<string, object>>
                                {
                                    new()
                                    {
                                        { "DraftId", draft.Id },
                                        { "DraftName", draft.Name },
                                        { "ToolkitId", toolkit.Id },
                                        {
                                            "ToolkitVersion", new Dictionary<string, object>
                                            {
                                                {
                                                    "DraftCompatibility",
                                                    DraftToolkitVersionCompatibility.ToolkitAheadOfDraft.ToString()
                                                },
                                                {
                                                    "Toolkit", new Dictionary<string, object>
                                                    {
                                                        { "Created", draft.Toolkit.Version },
                                                        { "Installed", "2.0.0" }
                                                    }
                                                },
                                                {
                                                    "Runtime", new Dictionary<string, object>
                                                    {
                                                        { "Created", toolkit.RuntimeVersion },
                                                        { "Installed", toolkit.RuntimeVersion }
                                                    }
                                                },
                                                {
                                                    "Compatibility",
                                                    ToolkitRuntimeVersionCompatibility.Compatible.ToString()
                                                }
                                            }
                                        },
                                        { "IsCurrent", true }
                                    }
                                }
                            }
                        }
                    }
                }
            }.ToJson();
            this.setup.Should().DisplayNoError();
            this.setup.Should().DisplayOutput(structuredOutput);
        }

        [Fact]
        public void WhenAddAttribute_ThenAddsAttribute()
        {
            this.setup.RunCommand($"{CommandLineApi.CreateCommandName} pattern APattern");
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-attribute AProperty --isrequired true --defaultvalueis achoice2 --isoneof \"achoice1;achoice2;achoice3\"  --output-structured");

            var attribute = this.setup.Pattern.Attributes.Single();
            var structuredOutput = new StructuredOutput
            {
                Info = new List<string>
                {
                    $"Information: {OutputMessages.CommandLine_Output_Preamble_CurrentPatternInUse.SubstituteTemplate("APattern", "0.0.0")}"
                },
                Output = new List<StructuredMessage>
                {
                    new()
                    {
                        Message = OutputMessages.CommandLine_Output_AttributeAdded_ForStructured,
                        Values = new Dictionary<string, object>
                        {
                            { "Name", "AProperty" },
                            { "AttributeId", attribute.Id },
                            { "ParentId", this.setup.Pattern.Id },
                            { "IsRequired", true },
                            { "DataType", "string" },
                            { "DefaultValue", "achoice2" },
                            { "Choices", new List<string> { "achoice1", "achoice2", "achoice3" } }
                        }
                    }
                }
            }.ToJson();
            this.setup.Should().DisplayNoError();
            this.setup.Should().DisplayOutput(structuredOutput);
        }

        [Fact]
        public void WhenUpdateAttribute_ThenUpdatesAttribute()
        {
            this.setup.RunCommand($"{CommandLineApi.CreateCommandName} pattern APattern");
            this.setup.RunCommand($"{CommandLineApi.EditCommandName} add-attribute AProperty1 --isrequired false");

            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} update-attribute AProperty1 --name AProperty2 --isrequired true --isoftype int --defaultvalueis 25 --output-structured");

            var attribute = this.setup.Pattern.Attributes.Single();
            var structuredOutput = new StructuredOutput
            {
                Info = new List<string>
                {
                    $"Information: {OutputMessages.CommandLine_Output_Preamble_CurrentPatternInUse.SubstituteTemplate("APattern", "0.0.0")}"
                },
                Output = new List<StructuredMessage>
                {
                    new()
                    {
                        Message = OutputMessages.CommandLine_Output_AttributeUpdated_ForStructured,
                        Values = new Dictionary<string, object>
                        {
                            { "Name", "AProperty2" },
                            { "AttributeId", attribute.Id },
                            { "ParentId", this.setup.Pattern.Id },
                            { "IsRequired", true },
                            { "DataType", "int" },
                            { "DefaultValue", "25" },
                            { "Choices", new List<string>() }
                        }
                    }
                }
            }.ToJson();
            this.setup.Should().DisplayNoError();
            this.setup.Should().DisplayOutput(structuredOutput);
        }

        [Fact]
        public void WhenAddElement_ThenAddsElement()
        {
            this.setup.RunCommand($"{CommandLineApi.CreateCommandName} pattern APattern");
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-element AnElement --describedas adescription --output-structured");

            var element = this.setup.Pattern.Elements.Single();
            var structuredOutput = new StructuredOutput
            {
                Info = new List<string>
                {
                    $"Information: {OutputMessages.CommandLine_Output_Preamble_CurrentPatternInUse.SubstituteTemplate("APattern", "0.0.0")}"
                },
                Output = new List<StructuredMessage>
                {
                    new()
                    {
                        Message = OutputMessages.CommandLine_Output_ElementAdded_ForStructured,
                        Values = new Dictionary<string, object>
                        {
                            { "Name", "AnElement" },
                            { "ElementId", element.Id },
                            { "ParentId", this.setup.Pattern.Id },
                            { "EditPath", "{APattern.AnElement}" },
                            { "DisplayName", "AnElement" },
                            { "Description", "adescription" },
                            { "AutoCreate", true },
                            { "Cardinality", ElementCardinality.One.ToString() },
                            { "IsCollection", false }
                        }
                    }
                }
            }.ToJson();
            this.setup.Should().DisplayNoError();
            this.setup.Should().DisplayOutput(structuredOutput);
        }

        [Fact]
        public void WhenUpdateElement_ThenUpdatesElement()
        {
            this.setup.RunCommand($"{CommandLineApi.CreateCommandName} pattern APattern");
            this.setup.RunCommand($"{CommandLineApi.EditCommandName} add-element AnElement1 --isrequired false");

            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} update-element AnElement1 --name AnElement2 --isrequired true --displayedas adisplayname --describedas adescription --output-structured");

            var element = this.setup.Pattern.Elements.Single();
            var structuredOutput = new StructuredOutput
            {
                Info = new List<string>
                {
                    $"Information: {OutputMessages.CommandLine_Output_Preamble_CurrentPatternInUse.SubstituteTemplate("APattern", "0.0.0")}"
                },
                Output = new List<StructuredMessage>
                {
                    new()
                    {
                        Message = OutputMessages.CommandLine_Output_ElementUpdated_ForStructured,
                        Values = new Dictionary<string, object>
                        {
                            { "Name", "AnElement2" },
                            { "ElementId", element.Id },
                            { "ParentId", this.setup.Pattern.Id },
                            { "EditPath", "{APattern.AnElement2}" },
                            { "DisplayName", "adisplayname" },
                            { "Description", "adescription" },
                            { "AutoCreate", false },
                            { "Cardinality", ElementCardinality.One.ToString() },
                            { "IsCollection", false }
                        }
                    }
                }
            }.ToJson();
            this.setup.Should().DisplayNoError();
            this.setup.Should().DisplayOutput(structuredOutput);
        }

        public void Dispose()
        {
            this.setup.Reset();
        }
    }
}