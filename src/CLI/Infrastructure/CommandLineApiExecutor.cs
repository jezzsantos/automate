using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Help;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.CommandLine.Parsing;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Automate.Authoring.Application;
using Automate.Common;
using Automate.Common.Application;
using Automate.Common.Domain;
using Automate.Common.Extensions;
using Automate.Runtime.Application;
using Automate.Runtime.Domain;

namespace Automate.CLI.Infrastructure
{
    internal static partial class CommandLineApi
    {
        private static AuthoringApplication authoring;
        private static RuntimeApplication runtime;

        public static int Execute(IDependencyContainer container, string[] args)
        {
            var recorder = container.Resolve<IRecorder>();

            var outputMessages = new List<OutputMessage>();
            var contextualMessages = new List<ContextualMessage>();
            var commandLine = new CommandLineBuilder(DefineCommands())
                .UseDefaults()
                .UseExceptionHandler((ex, context) => { HandleException(context, ex); })
                .UseHelp(context =>
                {
                    context.HelpBuilder.CustomizeLayout(_ =>
                    {
                        return HelpBuilder.Default.GetLayout()
                            .Prepend(_ => { WriteBanner(); })
                            .Append(__ =>
                            {
                                __.Output.WriteLine();
                                __.Output.WriteLine(OutputMessages.CommandLine_Output_HelpLink);
                            });
                    });
                })
                .Build();

            var parseResult = commandLine.Parse(args);

            var allowUsageCollection = IsOptionEnabled(parseResult, CollectUsageEnabledOption);
            if (allowUsageCollection)
            {
                var machineId = container.Resolve<IMachineStore>().GetOrCreateInstallationId();
                var correlationId = GetOptionValue(parseResult, CollectUsageCorrelationOption);
                recorder.EnableReporting(machineId, correlationId);
            }
            else
            {
                recorder.TraceInformation(LoggingMessages.CommandLineApi_UsageDisabled);
            }

            authoring = CreateAuthoringApplication(container);
            runtime = CreateRuntimeApplication(container);
            HandlerBase.Initialise(outputMessages, recorder,
                container.Resolve<IAssemblyMetadata>());

            if (IsAuthoringCommand(parseResult))
            {
                if (authoring.CurrentPatternId.Exists())
                {
                    contextualMessages.Add(new ContextualMessage(
                        OutputMessages.CommandLine_Output_Preamble_CurrentPatternInUse.SubstituteTemplate(
                            authoring.CurrentPatternName,
                            authoring.CurrentPatternVersion)));
                }
                else
                {
                    contextualMessages.Add(new ContextualMessage(
                        OutputMessages.CommandLine_Output_Preamble_NoPatternSelected, MessageLevel.ErrorWarning));
                }
            }
            if (IsRuntimeCommand(parseResult))
            {
                if (IsRuntimeDraftCommand(parseResult))
                {
                    if (runtime.CurrentDraftId.Exists())
                    {
                        contextualMessages.Add(new ContextualMessage(
                            OutputMessages.CommandLine_Output_Preamble_CurrentDraftInUse.SubstituteTemplate(
                                runtime.CurrentDraftName, runtime.CurrentDraftId)));
                    }
                    else
                    {
                        contextualMessages.Add(new ContextualMessage(
                            OutputMessages.CommandLine_Output_Preamble_NoDraftSelected, MessageLevel.ErrorWarning));
                    }
                }
            }
            if (IsTestingOnlyCommand(parseResult))
            {
                contextualMessages.Add(new ContextualMessage(OutputMessages.CommandLine_Output_Preamble_TestingOnly));
            }

            var result = commandLine.Invoke(args);

            if (IsStructuredOutput(parseResult))
            {
                WriteStructuredOutput();
            }
            else
            {
                WriteUnstructuredOutput();
                Console.WriteLine();
            }

            return result;

            void HandleException(InvocationContext context, Exception ex)
            {
                context.ExitCode = 1;

                var isDebug = IsDebugging(context.ParseResult);
                var message = ex.InnerException.Exists()
                    ? isDebug
                        ? ex.InnerException.ToString()
                        : ex.InnerException.Message
                    : isDebug
                        ? ex.ToString()
                        : ex.Message;
                var errorMessage = ExceptionMessages.CommandLineApi_UnexpectedError.Substitute(message);
                if (IsStructuredOutput(context.ParseResult))
                {
                    WriteStructuredError(errorMessage, context);
                }
                else
                {
                    WriteUnstructuredOutput();
                    WriteUnstructuredError(context, errorMessage);
                }
                if (IsUnexpectedException(ex))
                {
                    recorder.CrashRecoverable(ex, errorMessage);
                }
            }

            StructuredOutput CreateStructuredOutput()
            {
                return new StructuredOutput
                {
                    Info = contextualMessages.Select(cm => $"{cm.Level}: {cm.Message}").ToList(),
                    Output = outputMessages
                        .Select(om => om.MessageTemplate.SubstituteTemplateStructured(om.Arguments)).ToList()
                };
            }

            void WriteStructuredOutput()
            {
                var structuredOutput = CreateStructuredOutput().ToJson();
                Console.WriteLine(structuredOutput);
            }

            void WriteUnstructuredOutput()
            {
                contextualMessages.ForEach(cm =>
                {
                    switch (cm.Level)
                    {
                        case MessageLevel.Information:
                            ConsoleExtensions.WriteOutput(cm.Message);
                            ConsoleExtensions.WriteOutputLine();
                            break;

                        case MessageLevel.ErrorWarning:
                            ConsoleExtensions.WriteErrorWarning(cm.Message);
                            ConsoleExtensions.WriteOutputLine();
                            break;

                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                });
                outputMessages.ForEach(om =>
                {
                    var message = om.MessageTemplate.SubstituteTemplate(om.Arguments);
                    switch (om.Level)
                    {
                        case OutputMessageLevel.Information:
                            ConsoleExtensions.WriteOutput(message);
                            ConsoleExtensions.WriteOutputLine();
                            break;

                        case OutputMessageLevel.Warning:
                            ConsoleExtensions.WriteOutputWarning(message);
                            ConsoleExtensions.WriteOutputLine();
                            break;

                        case OutputMessageLevel.Error:
                            ConsoleExtensions.WriteError(message);
                            ConsoleExtensions.WriteOutputLine();
                            break;

                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                });
            }

            void WriteStructuredError(string errorMessage, InvocationContext context)
            {
                var structuredError = CreateStructuredOutput();
                structuredError.Error = new StructuredOutputError
                {
                    Message = errorMessage
                };
                var error = structuredError.ToJson();
                context.Console.WriteError(error);
            }

            void WriteUnstructuredError(InvocationContext context, string errorMessage)
            {
                context.Console.Error.WriteLine();
                context.Console.WriteError(errorMessage);
            }

            bool IsUnexpectedException(Exception ex)
            {
                return !(ex is AutomateException);
            }
        }

        private static AuthoringApplication CreateAuthoringApplication(IDependencyContainer container)
        {
            return new AuthoringApplication(container.Resolve<IRecorder>(),
                container.Resolve<IPatternStore>(), container.Resolve<IFilePathResolver>(),
                container.Resolve<IPatternPathResolver>(), container.Resolve<IPatternToolkitPackager>(),
                container.Resolve<ITextTemplatingEngine>(), container.Resolve<IApplicationExecutor>(),
                container.Resolve<IAssemblyMetadata>());
        }

        private static RuntimeApplication CreateRuntimeApplication(IDependencyContainer container)
        {
            return new RuntimeApplication(container.Resolve<IRecorder>(),
                container.Resolve<IToolkitStore>(), container.Resolve<IDraftStore>(),
                container.Resolve<IFilePathResolver>(), container.Resolve<IPatternToolkitPackager>(),
                container.Resolve<IDraftPathResolver>(), container.Resolve<IAutomationExecutor>(),
                container.Resolve<IAssemblyMetadata>());
        }

        private static void WriteBanner()
        {
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine(@"┌─┐┬ ┬┌┬┐┌─┐┌┬┐┌─┐┌┬┐┌─┐");
            Console.WriteLine(@"├─┤│ │ │ │ ││││├─┤ │ ├┤ ");
            Console.WriteLine(@"┴ ┴└─┘ ┴ └─┘┴ ┴┴ ┴ ┴ └─┘");
            Console.ResetColor();
        }

        [SuppressMessage("ReSharper", "UnusedParameter.Local")]
        private static bool IsDebugging(ParseResult parsedResult)
        {
            return IsOptionEnabled(parsedResult, DebugOption);
        }

        private static bool IsStructuredOutput(ParseResult parsedCommandLine)
        {
            return IsOptionEnabled(parsedCommandLine, StructuredOutputOption);
        }

        private static bool IsRuntimeCommand(ParseResult parsedCommandLine)
        {
            var matches = new List<CommandLineCommands>
            {
                new(InstallCommandName),
                new(RunCommandName),
                new(ConfigureCommandName),
                new(ValidateCommandName),
                new(ExecuteCommandName),
                new(ViewCommandName, DraftSubCommandName),
                new(ListCommandName, ToolkitsSubCommandName),
                new(ListCommandName, DraftsSubCommandName)
            };

            return IsMatchingCommand(parsedCommandLine, matches);
        }

        private static bool IsRuntimeDraftCommand(ParseResult parsedCommandLine)
        {
            var matches = new List<CommandLineCommands>
            {
                new(ExecuteCommandName),
                new(UpgradeCommandName, DraftSubCommandName),
                new(ViewCommandName, DraftSubCommandName),
                new(ValidateCommandName, DraftSubCommandName),
                new(ConfigureCommandName)
            };

            return IsMatchingCommand(parsedCommandLine, matches);
        }

        private static bool IsAuthoringCommand(ParseResult parsedCommandLine)
        {
            var matches = new List<CommandLineCommands>
            {
                new(CreateCommandName),
                new(EditCommandName),
                new(BuildCommandName),
                new(PublishCommandName),
                new(TestCommandName),
                new(ViewCommandName, PatternSubCommandName)
            };

            return IsMatchingCommand(parsedCommandLine, matches);
        }

        private static bool IsTestingOnlyCommand(ParseResult parsedCommandLine)
        {
            var matches = new List<CommandLineCommands>
            {
                new(TestingOnlyCommandName)
            };

            return IsMatchingCommand(parsedCommandLine, matches);
        }

        private static bool IsOptionEnabled(ParseResult parsedCommandLine, string optionName, bool defaultValue = false)
        {
            var option = parsedCommandLine.Parser.Configuration.RootCommand.Options
                .FirstOrDefault(opt => opt.Name.EqualsOrdinal(optionName));
            if (option.NotExists())
            {
                return defaultValue;
            }

            return (bool)parsedCommandLine.FindResultFor(option)!.GetValueOrDefault()!;
        }

        private static string GetOptionValue(ParseResult parsedCommandLine, string optionName)
        {
            var option = parsedCommandLine.Parser.Configuration.RootCommand.Options
                .FirstOrDefault(opt => opt.Name.EqualsOrdinal(optionName));
            if (option.NotExists())
            {
                return null;
            }

            var value = parsedCommandLine.FindResultFor(option)!
                .GetValueOrDefault()!;

            return value.Exists()
                ? value.ToString()
                : null;
        }

        private static bool IsMatchingCommand(ParseResult parsedCommandLine, List<CommandLineCommands> matches)
        {
            var allCommands = GetCommandLineCommands(parsedCommandLine);
            var commandSet = new CommandLineCommands(allCommands.FirstOrDefault(), allCommands.ElementAtOrDefault(1),
                allCommands.ElementAtOrDefault(2));

            foreach (var match in matches)
            {
                if (match.Matches(commandSet))
                {
                    return true;
                }
            }

            return false;

            List<string> GetCommandLineCommands(ParseResult result)
            {
                var commandResult = result.CommandResult;
                var commands = new List<string>
                {
                    commandResult.Symbol.Name
                };

                var currentCommandResult = commandResult as SymbolResult;
                while (currentCommandResult.Parent.Exists())
                {
                    var parent = currentCommandResult.Parent;
                    commands.Add(parent.Symbol.Name);
                    currentCommandResult = parent;
                }

                commands.Reverse();

                return commands
                    .Skip(1)
                    .ToListSafe();
            }
        }

        private enum MessageLevel
        {
            Information,
            ErrorWarning
        }

        internal enum OutputMessageLevel
        {
            Information,
            Error,
            Warning
        }

        private class ContextualMessage
        {
            public ContextualMessage(string message, MessageLevel level = MessageLevel.Information)
            {
                Message = message;
                Level = level;
            }

            public string Message { get; }

            public MessageLevel Level { get; }
        }

        internal class OutputMessage
        {
            public OutputMessage(OutputMessageLevel level, string messageTemplate, params object[] args)
            {
                MessageTemplate = messageTemplate;
                Arguments = args;
                Level = level;
            }

            public string MessageTemplate { get; }

            public object[] Arguments { get; }

            public OutputMessageLevel Level { get; }
        }

        private class CommandLineCommands
        {
            private readonly string bottomLevel;
            private readonly string midLevel;
            private readonly string topLevel;

            public CommandLineCommands(string topLevel, string midLevel = null, string bottomLevel = null)
            {
                this.topLevel = topLevel;
                this.midLevel = midLevel;
                this.bottomLevel = bottomLevel;
            }

            public bool Matches(CommandLineCommands target)
            {
                return this.topLevel.EqualsIgnoreCase(target.topLevel)
                       && (this.midLevel.NotExists() || this.midLevel.EqualsIgnoreCase(target.midLevel))
                       && (this.bottomLevel.NotExists() || this.bottomLevel.EqualsIgnoreCase(target.bottomLevel));
            }
        }
    }

    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    internal class StructuredOutput
    {
        public List<string> Info { get; set; }

        public StructuredOutputError Error { get; set; }

        public List<StructuredMessage> Output { get; set; }
    }

    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    internal class StructuredOutputError
    {
        public string Message { get; set; }
    }

    internal static class CommandLineExtensions
    {
        private const string Delimiter = "=";

        public static (string Name, string Value) SplitPropertyAssignment(this string expression)
        {
            expression.GuardAgainstNullOrEmpty(nameof(expression));

            var indexOfDelimiter = expression.IndexOf(Delimiter, StringComparison.Ordinal);
            var hasDelimiter = indexOfDelimiter > -1;
            if (!hasDelimiter)
            {
                return (expression, null);
            }

            var name = expression.Substring(0, indexOfDelimiter);
            if (name.HasNoValue())
            {
                throw new AutomateException(ExceptionMessages
                    .CommandLineApiExtensions_SplitPropertyAssignment_ValueWithoutName.Substitute(expression));
            }

            var value = expression.Substring(indexOfDelimiter + 1);
            if (value.HasNoValue())
            {
                return (name, null);
            }

            return (name, value);
        }
    }

    internal static class ConsoleExtensions
    {
        public static void WriteError(this IConsole console, string message)
        {
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Red;
            console.Error.WriteLine(message);
            console.Error.WriteLine();
            Console.ResetColor();
        }

        public static void WriteError(string message)
        {
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine(message);
            Console.Error.WriteLine();
            Console.ResetColor();
        }

        public static void WriteErrorWarning(string message)
        {
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        public static void WriteOutputLine()
        {
            Console.ResetColor();
            Console.WriteLine(string.Empty);
        }

        public static void WriteOutput(string message)
        {
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        public static void WriteOutputWarning(string message)
        {
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine(message);
            Console.ResetColor();
        }
    }
}