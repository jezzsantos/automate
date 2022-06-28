using System;
using System.Collections.Generic;
using System.IO;
using Automate.CLI;
using Automate.CLI.Infrastructure;
using Automate.Common.Extensions;
using Xunit;

namespace CLI.IntegrationTests
{
    [Trait("Category", "Integration")] [Collection("CLI")]
    public class GeneralSpec
    {
        private readonly CliTestSetup setup;

        public GeneralSpec(CliTestSetup setup)
        {
            this.setup = setup;
            this.setup.ResetRepository();
        }

        [Fact]
        public void WhenNoCommands_ThenDisplaysError()
        {
            this.setup.RunCommand("");

            this.setup.Should().DisplayErrorForMissingCommand();
        }

        private static string GetThrownMessage(bool withDebug, bool hasInnerException, string message)
        {
            var assemblyLocation = typeof(CommandLineApi).Assembly.Location;
            var solutionDirectory = GetSolutionDirectory(assemblyLocation, "src");
            var throwingMethodLineNumber = hasInnerException
                ? 61
                : 63;
            var throwingMethodLocation =
                Path.GetFullPath(Path.Combine(solutionDirectory,
                    $@"{nameof(Automate.CLI)}/{nameof(Automate.CLI.Infrastructure)}/CommandLineApiHandlers.cs"));
            var throwingMethodName =
                $"{nameof(Automate)}.{nameof(Automate.CLI)}.{nameof(Automate.CLI.Infrastructure)}.{nameof(CommandLineApi)}.{nameof(CommandLineApi.TestingOnlyHandlers)}.{nameof(CommandLineApi.TestingOnlyHandlers.Fail)}";
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

#endif
    }
}