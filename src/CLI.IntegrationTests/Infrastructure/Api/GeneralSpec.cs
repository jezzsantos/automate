using System;
using System.Collections.Generic;
using System.IO;
using Automate.CLI;
using Automate.CLI.Infrastructure;
using Automate.CLI.Infrastructure.Api;
using Automate.Common.Extensions;
using FluentAssertions;
using Xunit;

namespace CLI.IntegrationTests.Infrastructure.Api
{
    [Trait("Category", "Integration")] [Collection("CLI")]
    public class GeneralSpec : IDisposable
    {
        private readonly string localPath;
        private readonly SystemIoFileSystemReaderWriter readerWriter;
        private readonly CliTestSetup setup;

        public GeneralSpec(CliTestSetup setup)
        {
            this.setup = setup;
            this.setup.ResetRepository();
            this.readerWriter = new SystemIoFileSystemReaderWriter();
            this.localPath = new CliRuntimeMetadata().LocalStateDataPath;
        }

        [Fact]
        public void WhenNoCommands_ThenDisplaysError()
        {
            this.setup.RunCommand("");

            this.setup.Should().DisplayErrorForMissingCommand();
        }

        [Fact]
        public void WhenInfoAndNotCollectingUsage_ThenWontMeasure()
        {
            this.setup.RunCommand($"{CommandLineApi.InfoCommandName} --collect-usage:false");

            var metadata = new CliRuntimeMetadata();
            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayOutput(OutputMessages.CommandLine_Output_Info.SubstituteTemplate(metadata.ProductName,
                    metadata.InstallationPath,
                    metadata.RuntimeVersion, false));
            this.setup.Recordings.IsReportingEnabled.Should().BeFalse();
        }

        [Fact]
        public void WhenInfoAndCollectingUsage_ThenMeasuresWithUserId()
        {
            this.setup.RunCommand($"{CommandLineApi.InfoCommandName}");

            var metadata = new CliRuntimeMetadata();
            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayOutput(OutputMessages.CommandLine_Output_Info.SubstituteTemplate(metadata.ProductName,
                    metadata.InstallationPath,
                    metadata.RuntimeVersion, true));
            this.setup.Recordings.IsReportingEnabled.Should().BeTrue();
            this.setup.Recordings.Session.Should().BeNull();
            this.setup.Recordings.Measurements.Should().ContainSingle(measurement =>
                measurement.EventName == "info" && measurement.MachineId.HasValue() &&
                measurement.CorrelationId == "acorrelationid");
        }

        [Fact]
        public void WhenListAllAndNone_ThenDisplaysNone()
        {
            this.setup.RunCommand($"{CommandLineApi.ListCommandName} all");

            this.setup.Should()
                .DisplayOutput(
                    OutputMessages.CommandLine_Output_NoEditablePatterns);
            this.setup.Should()
                .DisplayOutput(
                    OutputMessages.CommandLine_Output_NoInstalledToolkits);
            this.setup.Should()
                .DisplayOutput(
                    OutputMessages.CommandLine_Output_NoConfiguredDrafts);

            var stateFile =
                Path.GetFullPath(Path.Combine(this.localPath, LocalMachineFileLocalStateRepository.StateFilename));
            this.readerWriter.FileExists(stateFile).Should().BeFalse();
        }

        [Fact]
        public void WhenListAllAndSome_ThenDisplaysLists()
        {
            this.setup.RunCommand($"{CommandLineApi.CreateCommandName} pattern APattern1");
            this.setup.RunCommand($"{CommandLineApi.BuildCommandName} pattern --install");
            this.setup.RunCommand($"{CommandLineApi.RunCommandName} toolkit APattern1");

            this.setup.RunCommand($"{CommandLineApi.ListCommandName} all");

            var pattern = this.setup.Pattern;
            var toolkit = this.setup.Toolkit;
            var draft = this.setup.Draft;

            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayOutput(
                    OutputMessages.CommandLine_Output_EditablePatternsListed.SubstituteTemplate(
                        $"\"Name\": \"{pattern.Name}\", \"Version\": \"{pattern.ToolkitVersion.Current}\", \"ID\": \"{pattern.Id}\", \"IsCurrent\": \"true\""));
            this.setup.Should()
                .DisplayOutput(
                    OutputMessages.CommandLine_Output_InstalledToolkitsListed.SubstituteTemplate(
                        $"\"Name\": \"{toolkit.PatternName}\", \"Version\": \"{toolkit.Version}\", \"ID\": \"{toolkit.Id}\""));
            this.setup.Should()
                .DisplayOutput(
                    OutputMessages.CommandLine_Output_ConfiguredDraftsListed.SubstituteTemplate(
                        $"\"Name\": \"{draft.Name}\", \"ToolkitVersion\": \"{draft.Toolkit.Version}\", \"CurrentToolkitVersion\": \"{toolkit.Version}\", \"ID\": \"{draft.Id}\", \"IsCurrent\": \"true\""));
            var stateFile =
                Path.GetFullPath(Path.Combine(this.localPath, LocalMachineFileLocalStateRepository.StateFilename));
            this.readerWriter.FileExists(stateFile).Should().BeTrue();
        }

        [Fact]
        public void WhenListAllAndToolkitUpgraded_ThenDisplaysLists()
        {
            this.setup.RunCommand($"{CommandLineApi.CreateCommandName} pattern APattern1");
            this.setup.RunCommand($"{CommandLineApi.BuildCommandName} pattern --install");
            this.setup.RunCommand($"{CommandLineApi.RunCommandName} toolkit APattern1");

            this.setup.RunCommand($"{CommandLineApi.PublishCommandName} toolkit --install --asversion 2.0.0");

            this.setup.RunCommand($"{CommandLineApi.ListCommandName} all");

            var pattern = this.setup.Pattern;
            var toolkit = this.setup.Toolkit;
            var draft = this.setup.Draft;

            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayOutput(
                    OutputMessages.CommandLine_Output_EditablePatternsListed.SubstituteTemplate(
                        $"\"Name\": \"{pattern.Name}\", \"Version\": \"{pattern.ToolkitVersion.Current}\", \"ID\": \"{pattern.Id}\", \"IsCurrent\": \"true\""));
            this.setup.Should()
                .DisplayOutput(
                    OutputMessages.CommandLine_Output_InstalledToolkitsListed.SubstituteTemplate(
                        $"\"Name\": \"{toolkit.PatternName}\", \"Version\": \"{toolkit.Version}\", \"ID\": \"{toolkit.Id}\""));
            this.setup.Should()
                .DisplayOutput(
                    OutputMessages.CommandLine_Output_ConfiguredDraftsListed.SubstituteTemplate(
                        $"\"Name\": \"{draft.Name}\", \"ToolkitVersion\": \"{draft.Toolkit.Version}\", \"CurrentToolkitVersion\": \"{toolkit.Version}\", \"ID\": \"{draft.Id}\", \"IsCurrent\": \"true\""));
        }

        public void Dispose()
        {
            this.setup.Reset();
        }
#if TESTINGONLY

        [Fact]
        public void WhenThrowsTopLevelExceptionWithoutDebug_ThenDisplaysDetailedError()
        {
            this.setup.RunCommand(
                $"{CommandLineApi.TestingOnlyCommandName} fail --message \"amessage\" --nested:false --debug:false");

            var thrownMessage = GetThrownMessage(false, false, "amessage");
            this.setup.Should().DisplayError(thrownMessage);
            this.setup.Should().DisplayOutput(OutputMessages.CommandLine_Output_Preamble_TestingOnly);
        }

        [Fact]
        public void WhenThrowsNestedExceptionWithoutDebug_ThenDisplaysDetailedError()
        {
            this.setup.RunCommand(
                $"{CommandLineApi.TestingOnlyCommandName} fail --message \"amessage\" --nested:true --debug:false");

            var thrownMessage = GetThrownMessage(false, true, "amessage");
            this.setup.Should().DisplayError(thrownMessage);
            this.setup.Should().DisplayOutput(OutputMessages.CommandLine_Output_Preamble_TestingOnly);
        }

        [Fact]
        public void WhenThrowsTopLevelExceptionWithDebug_ThenDisplaysDetailedError()
        {
            this.setup.RunCommand(
                $"{CommandLineApi.TestingOnlyCommandName} fail --message \"amessage\" --nested:false --debug:true");

            var thrownMessage = GetThrownMessage(true, false, "amessage");
            this.setup.Should().DisplayError(thrownMessage);
            this.setup.Should().DisplayOutput(OutputMessages.CommandLine_Output_Preamble_TestingOnly);
        }

        [Fact]
        public void WhenThrowsNestedExceptionWithDebug_ThenDisplaysDetailedError()
        {
            this.setup.RunCommand(
                $"{CommandLineApi.TestingOnlyCommandName} fail --message \"amessage\" --nested:true --debug:true");

            var thrownMessage = GetThrownMessage(true, true, "amessage");
            this.setup.Should().DisplayError(thrownMessage);
            this.setup.Should().DisplayOutput(OutputMessages.CommandLine_Output_Preamble_TestingOnly);
        }

        [Fact]
        public void WhenSucceeds_ThenDisplaysOutput()
        {
            this.setup.RunCommand($"{CommandLineApi.TestingOnlyCommandName} succeed --message \"amessage\"");

            this.setup.Should().DisplayNoError();
            this.setup.Should().DisplayOutput(
                $"{OutputMessages.CommandLine_Output_Preamble_TestingOnly}{Environment.NewLine}" +
                $"{Environment.NewLine}" +
                "amessage");
        }

        [Fact]
        public void WhenThrowsWithStructuredOutput_ThenDisplaysStructuredOutput()
        {
            this.setup.RunCommand(
                $"{CommandLineApi.TestingOnlyCommandName} fail --message \"amessage\" --nested:true --output-structured --debug:false");

            var structuredOutput = new StructuredOutput
            {
                Info = new List<string> { $"Information: {OutputMessages.CommandLine_Output_Preamble_TestingOnly}" },
                Output = new List<StructuredMessage>
                {
                    new()
                    {
                        Message = "amessagetemplate {aname}",
                        Values = new Dictionary<string, object>
                        {
                            { "aname", "avalue" }
                        }
                    },
                    new()
                    {
                        Message = "amessagetemplate {aname}",
                        Values = new Dictionary<string, object>
                        {
                            {
                                "aname", new
                                {
                                    AProperty1 = new
                                    {
                                        AChildProperty1 = "avalue1"
                                    },
                                    AProperty2 = "avalue2"
                                }
                            }
                        }
                    }
                }
            }.ToJson();
            var structuredError = new StructuredOutput
            {
                Info = new List<string> { $"Information: {OutputMessages.CommandLine_Output_Preamble_TestingOnly}" },
                Error = new StructuredOutputError
                {
                    Message = GetThrownMessage(false, true, "amessage")
                },
                Output = new List<StructuredMessage>
                {
                    new()
                    {
                        Message = "amessagetemplate {aname}",
                        Values = new Dictionary<string, object>
                        {
                            { "aname", "avalue" }
                        }
                    },
                    new()
                    {
                        Message = "amessagetemplate {aname}",
                        Values = new Dictionary<string, object>
                        {
                            {
                                "aname", new
                                {
                                    AProperty1 = new
                                    {
                                        AChildProperty1 = "avalue1"
                                    },
                                    AProperty2 = "avalue2"
                                }
                            }
                        }
                    }
                }
            }.ToJson();

            this.setup.Should().DisplayError(structuredError);
            this.setup.Should().DisplayOutput(structuredOutput);
        }

        [Fact]
        public void WhenSucceedsWithStructuredOutput_ThenDisplaysStructuredOutput()
        {
            this.setup.RunCommand(
                $"{CommandLineApi.TestingOnlyCommandName} succeed --message \"amessagetemplate {{aname}}\" --value \"avalue\" --output-structured");

            var structuredOutput = new StructuredOutput
            {
                Info = new List<string> { $"Information: {OutputMessages.CommandLine_Output_Preamble_TestingOnly}" },
                Output = new List<StructuredMessage>
                {
                    new()
                    {
                        Message = "amessagetemplate {aname}",
                        Values = new Dictionary<string, object>
                        {
                            { "aname", "avalue" }
                        }
                    },
                    new()
                    {
                        Message = "amessagetemplate {aname}",
                        Values = new Dictionary<string, object>
                        {
                            {
                                "aname", new
                                {
                                    AProperty1 = new
                                    {
                                        AChildProperty1 = "avalue1"
                                    },
                                    AProperty2 = "avalue2"
                                }
                            }
                        }
                    }
                }
            }.ToJson();
            this.setup.Should().DisplayNoError();
            this.setup.Should().DisplayOutput(structuredOutput);
        }

        private static string GetThrownMessage(bool withDebug, bool hasInnerException, string message)
        {
            var assemblyLocation = typeof(CommandLineApi).Assembly.Location;
            var solutionDirectory = GetSolutionDirectory(assemblyLocation, "src");
            var throwingMethodLineNumber = hasInnerException
                ? 30
                : 32;
            var throwingMethodLocation =
                Path.GetFullPath(Path.Combine(solutionDirectory,
                    $@"{nameof(Automate.CLI)}/{nameof(Automate.CLI.Infrastructure)}/{nameof(Automate.CLI.Infrastructure.Api)}/{nameof(CommandLineApi.TestingOnlyApiHandlers)}.cs"));
            var throwingMethodName =
                $"{nameof(Automate)}.{nameof(Automate.CLI)}.{nameof(Automate.CLI.Infrastructure)}.{nameof(Automate.CLI.Infrastructure.Api)}.{nameof(CommandLineApi)}.{nameof(CommandLineApi.TestingOnlyApiHandlers)}.{nameof(CommandLineApi.TestingOnlyApiHandlers.Fail)}";
            var throwingMethodSignature = "String message, Boolean nested";

            return hasInnerException
                ? withDebug
                    ? ExceptionMessages.CommandLineApi_UnexpectedError.Substitute(
                        $"System.Exception: {message}{Environment.NewLine}" +
                        $" ---> System.Exception: {message}{Environment.NewLine}" +
                        $"   --- End of inner exception stack trace ---{Environment.NewLine}" +
                        $"   at {throwingMethodName}({throwingMethodSignature}) in {throwingMethodLocation}:line {throwingMethodLineNumber}")
                    : ExceptionMessages.CommandLineApi_UnexpectedError.Substitute(
                        $"{message}")
                : withDebug
                    ? ExceptionMessages.CommandLineApi_UnexpectedError.Substitute(
                        $"System.Exception: {message}{Environment.NewLine}" +
                        $"   at {throwingMethodName}({throwingMethodSignature}) in {throwingMethodLocation}:line {throwingMethodLineNumber}")
                    : ExceptionMessages.CommandLineApi_UnexpectedError.Substitute(
                        $"{message}");
        }

        private static string GetSolutionDirectory(string path, string solutionDirectoryName)
        {
            solutionDirectoryName.GuardAgainstNull(nameof(solutionDirectoryName));

            var currentDirectory = path;
            while (true)
            {
                var parentDirectory = Directory.GetParent(currentDirectory!);
                if (parentDirectory.NotExists())
                {
                    throw new Exception(
                        $"Could not find the solution directory called: {solutionDirectoryName} in: {path}");
                }
                if (parentDirectory.Name.EqualsIgnoreCase(solutionDirectoryName))
                {
                    return parentDirectory.FullName;
                }

                currentDirectory = parentDirectory.FullName;
            }
        }

#endif
    }
}