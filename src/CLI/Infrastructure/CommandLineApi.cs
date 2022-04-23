using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Help;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.CommandLine.Parsing;
using System.Linq;
using System.Text;
using Automate.CLI.Application;
using Automate.CLI.Domain;
using Automate.CLI.Extensions;

namespace Automate.CLI.Infrastructure
{
    internal class CommandLineApi
    {
        public const string CreateCommandName = "create";
        public const string EditCommandName = "edit";
        public const string BuildCommandName = "build";
        public const string InstallCommandName = "install";
        public const string RunCommandName = "run";
        public const string TestCommandName = "test";
        public const string ListCommandName = "list";
        public const string ConfigureCommandName = "configure";
        public const string ValidateCommandName = "validate";
        public const string ExecuteCommandName = "execute";
        public const string ViewCommandName = "view";
        public const string UpgradeCommandName = "upgrade";
        private static readonly AuthoringApplication Authoring = new AuthoringApplication(Environment.CurrentDirectory);
        private static readonly RuntimeApplication Runtime = new RuntimeApplication(Environment.CurrentDirectory);

        public static int Execute(string[] args)
        {
            var createCommands = new Command(CreateCommandName, "Creating new patterns")
            {
                new Command("pattern", "Creates a new pattern")
                {
                    new Argument("Name", "The name of the pattern to create")
                }.WithHandler<AuthoringHandlers>(nameof(AuthoringHandlers.HandleCreate))
            };
            var editCommands = new Command(EditCommandName, "Editing patterns")
            {
                new Command("switch", "Switches to configuring another pattern")
                {
                    new Argument("Name", "The name of the existing pattern to edit")
                }.WithHandler<AuthoringHandlers>(nameof(AuthoringHandlers.HandleSwitch)),
                new Command("add-attribute", "Adds an attribute to an element/collection in the pattern")
                {
                    new Argument("Name", "The name of the attribute"),
                    new Option("--isoftype", "The type of the attribute", typeof(string)),
                    new Option("--defaultvalueis", "The default value for the attribute", typeof(string),
                        arity: ArgumentArity.ZeroOrOne),
                    new Option("--isrequired", "Whether an attribute value will be required", typeof(bool),
                        () => false, ArgumentArity.ZeroOrOne),
                    new Option("--isoneof", "A list of semi-colon delimited values", typeof(string),
                        arity: ArgumentArity.ZeroOrOne),
                    new Option("--aschildof", "The expression of the element/collection to add the attribute to",
                        typeof(string), arity: ArgumentArity.ZeroOrOne)
                }.WithHandler<AuthoringHandlers>(nameof(AuthoringHandlers.HandleAddAttribute)),

                //TODO: update-attribute
                new Command("delete-attribute", "Deletes an attribute from an element/collection in the pattern")
                {
                    new Argument("Name", "The name of the attribute"),
                    new Option("--aschildof", "The expression of the element/collection to delete the attribute from",
                        typeof(string), arity: ArgumentArity.ZeroOrOne)
                }.WithHandler<AuthoringHandlers>(nameof(AuthoringHandlers.HandleDeleteAttribute)),
                new Command("add-element", "Adds an element to an element/collection in the pattern")
                {
                    new Argument("Name", "The name of the element"),
                    new Option("--displayedas", "A friendly display name for the element", typeof(string),
                        arity: ArgumentArity.ZeroOrOne),
                    new Option("--describedas", "A description for the element", typeof(string),
                        arity: ArgumentArity.ZeroOrOne),
                    new Option("--aschildof", "The expression of the element/collection to add the element to",
                        typeof(string), arity: ArgumentArity.ZeroOrOne),
                    new Option("--isrequired", "Whether the element will be required or not",
                        typeof(bool), () => true, ArgumentArity.ZeroOrOne)
                }.WithHandler<AuthoringHandlers>(nameof(AuthoringHandlers.HandleAddElement)),

                //TODO: update-element
                new Command("delete-element", "Deletes an element from an element/collection in the pattern")
                {
                    new Argument("Name", "The name of the element"),
                    new Option("--aschildof", "The expression of the element/collection to delete the element from",
                        typeof(string), arity: ArgumentArity.ZeroOrOne)
                }.WithHandler<AuthoringHandlers>(nameof(AuthoringHandlers.HandleDeleteElement)),
                new Command("add-collection", "Adds a collection to an element/collection in the pattern")
                {
                    new Argument("Name", "The name of the collection"),
                    new Option("--displayedas", "A friendly display name for the collection", typeof(string),
                        arity: ArgumentArity.ZeroOrOne),
                    new Option("--describedas", "A description for the collection", typeof(string),
                        arity: ArgumentArity.ZeroOrOne),
                    new Option("--aschildof", "The expression of the element/collection to add the collection to",
                        typeof(string), arity: ArgumentArity.ZeroOrOne),
                    new Option("--isrequired", "Whether at least one item in the collection will be required or not",
                        typeof(bool), () => false, ArgumentArity.ZeroOrOne)
                }.WithHandler<AuthoringHandlers>(nameof(AuthoringHandlers.HandleAddCollection)),

                //TODO: update-collection
                new Command("delete-collection", "Deletes a collection from an element/collection in the pattern")
                {
                    new Argument("Name", "The name of the collection"),
                    new Option("--aschildof", "The expression of the element/collection to delete the collection from",
                        typeof(string), arity: ArgumentArity.ZeroOrOne)
                }.WithHandler<AuthoringHandlers>(nameof(AuthoringHandlers.HandleDeleteCollection)),
                new Command("add-codetemplate", "Adds a code template to an element")
                {
                    new Argument("FilePath", "A relative path to the code file, from the current directory"),
                    new Option("--name", "A friendly name for the code template",
                        arity: ArgumentArity.ZeroOrOne),
                    new Option("--aschildof", "The expression of the element/collection to add the launch point to",
                        typeof(string), arity: ArgumentArity.ZeroOrOne)
                }.WithHandler<AuthoringHandlers>(nameof(AuthoringHandlers.HandleAddCodeTemplate)),

                //TODO: update-codetemplate
                //TODO: edit-codetemplate (open in editor, by name)
                //TODO: delete-codetemplate
                new Command("add-codetemplate-command", "Adds a command that renders a code template")
                {
                    new Argument("CodeTemplateName", "The name of the code template"),
                    new Option("--name", "A name for the command", typeof(string),
                        arity: ArgumentArity.ZeroOrOne),
                    new Option("--astearoff",
                        "Only if you only want to generate the file once, and not overwrite the file if it already exists",
                        typeof(bool), arity: ArgumentArity.ZeroOrOne),
                    new Option("--withpath", "The full path of the generated file, with filename.", typeof(string),
                        arity: ArgumentArity.ExactlyOne),
                    new Option("--aschildof", "The expression of the element/collection to add the command to",
                        typeof(string), arity: ArgumentArity.ZeroOrOne)
                }.WithHandler<AuthoringHandlers>(nameof(AuthoringHandlers.HandleAddCodeTemplateCommand)),
                new Command("update-codetemplate-command", "Updates an existing command")
                {
                    new Argument("CommandName", "The name of the command to update"),
                    new Option("--name", "A new name for the command", typeof(string),
                        arity: ArgumentArity.ZeroOrOne),
                    new Option("--astearoff", "If you only want to generate the file once, and not overwrite the file if it already exists",
                        typeof(bool?), arity: ArgumentArity.ZeroOrOne),
                    new Option("--withpath", "The full path of the generated file, with filename.", typeof(string),
                        arity: ArgumentArity.ZeroOrOne),
                    new Option("--aschildof", "The expression of the element/collection on which the command exists",
                        typeof(string), arity: ArgumentArity.ZeroOrOne)
                }.WithHandler<AuthoringHandlers>(nameof(AuthoringHandlers.HandleUpdateCodeTemplateCommand)),
                new Command("add-cli-command", "Adds a command that executes another command line application")
                    {
                        new Argument("ApplicationName", "The name of the command line application/exe to execute. Include the full path if the application is not in the machine's path variable"),
                        new Option("--arguments", "The arguments to pass to the command line application. (Escape double-quotes with an extra double-quote)",
                            typeof(string), arity: ArgumentArity.ZeroOrOne),
                        new Option("--name", "A name for the command", typeof(string),
                            arity: ArgumentArity.ZeroOrOne),
                        new Option("--aschildof", "The expression of the element/collection on which the command exists",
                            typeof(string), arity: ArgumentArity.ZeroOrOne)
                    }
                    .WithHandler<AuthoringHandlers>(nameof(AuthoringHandlers.HandleAddCliCommand)),

                //TODO: update-cli-command
                //TODO: delete-command (by ID, all kinds)
                new Command("add-command-launchpoint", "Adds a launch point for a command")
                {
                    new Argument("CommandIdentifiers", "A semi-colon delimited list of identifiers of the commands to launch (from anywhere in the pattern), or '*' to launch all commands found on the element/collection"),
                    new Option("--name", "A name for the launch point", typeof(string),
                        arity: ArgumentArity.ZeroOrOne),
                    new Option("--aschildof", "The expression of the element/collection to add the launch point to",
                        typeof(string), arity: ArgumentArity.ZeroOrOne)
                }.WithHandler<AuthoringHandlers>(nameof(AuthoringHandlers.HandleAddCommandLaunchPoint)),
                new Command("update-command-launchpoint", "Updates an existing launch point")
                {
                    new Argument("LaunchPointName", "The name of the launch point to update"),
                    new Option("--name", "A new name for the launch point", typeof(string),
                        arity: ArgumentArity.ZeroOrOne),
                    new Option("--add", "A semi-colon delimited list of identifiers of the commands to add (from anywhere in the pattern), or '*' to add all commands on the from element/collection)",
                        typeof(string), arity: ArgumentArity.ExactlyOne),
                    new Option("--from", "The expression of the element/collection to add commands from",
                        typeof(string), arity: ArgumentArity.ZeroOrOne),
                    new Option("--aschildof", "The expression of the element/collection on which the launch point exists",
                        typeof(string), arity: ArgumentArity.ZeroOrOne)
                }.WithHandler<AuthoringHandlers>(nameof(AuthoringHandlers.HandleUpdateCommandLaunchPoint))

                //TODO: delete-command-launchpoint
            };
            var testCommands = new Command(TestCommandName, "Testing automation of a pattern")
            {
                new Command("codetemplate", "Tests the code template")
                {
                    new Argument("Name", "The name of the code template"),
                    new Option("--aschildof", "The expression of the element/collection on which the code template exists",
                        typeof(string), arity: ArgumentArity.ZeroOrOne),
                    new Option("--import-data", "Import the specified data for the test. A relative path to the JSON file, from the current directory",
                        typeof(string), arity: ArgumentArity.ZeroOrOne),
                    new Option("--export-data", "Export the generated test data to the specified file. A relative path to the JSON file, from the current directory",
                        typeof(string), arity: ArgumentArity.ZeroOrOne)
                }.WithHandler<AuthoringHandlers>(nameof(AuthoringHandlers.HandleTestCodeTemplate))
            };
            var buildCommands = new Command(BuildCommandName, "Building toolkits from patterns")
            {
                new Command("toolkit", "Builds a pattern into a toolkit")
                {
                    new Option("--asversion", "A specific version number (1-2 dot number), or 'auto' to auto-increment the current version",
                        typeof(string), arity: ArgumentArity.ZeroOrOne),
                    new Option("--force", "Force the specified version number even it it violates breaking changes checks",
                        typeof(bool), () => false, ArgumentArity.ZeroOrOne)
                }.WithHandler<AuthoringHandlers>(nameof(AuthoringHandlers.HandleBuild))
            };
            var installCommands = new Command(InstallCommandName, "Installing toolkits")
            {
                new Command("toolkit", "Installs the pattern from a toolkit")
                {
                    new Argument("Location", "The location of the *.toolkit file to install into the current directory")
                }.WithHandler<RuntimeHandlers>(nameof(RuntimeHandlers.HandleInstall))
            };
            var runCommands = new Command(RunCommandName, "Running patterns from toolkits")
            {
                new Command("toolkit", "Creates a new solution from a toolkit")
                {
                    new Argument("PatternName", "The name of the pattern in the toolkit that you want to use"),
                    new Option("--name", "A name for the solution", arity: ArgumentArity.ZeroOrOne)
                }.WithHandler<RuntimeHandlers>(nameof(RuntimeHandlers.HandleNewSolution)),
                new Command("switch", "Switches to configuring another solution")
                {
                    new Argument("SolutionId", "The id of the existing solution to configure")
                }.WithHandler<RuntimeHandlers>(nameof(RuntimeHandlers.HandleSwitch))
            };
            var configureCommands = new Command(ConfigureCommandName, "Configuring solutions to patterns from toolkits")
            {
                new Command("add", "Configure an element in the solution")
                {
                    new Argument("Expression", "The expression of the element to configure"),
                    new Option("--and-set", "A Name=Value pair of a property assignment",
                        arity: ArgumentArity.ZeroOrMore)
                }.WithHandler<RuntimeHandlers>(nameof(RuntimeHandlers.HandleAddTo)),
                new Command("add-one-to", "Add a new item to a collection in the solution")
                {
                    new Argument("Expression", "The expression of the element/collection to add to"),
                    new Option("--and-set", "Additional Name=Value pair of a property assignment",
                        arity: ArgumentArity.ZeroOrMore)
                }.WithHandler<RuntimeHandlers>(nameof(RuntimeHandlers.HandleAddOneTo)),
                new Command("on", "Set the properties of an existing item in the solution")
                {
                    new Argument("Expression", "The expression of the element/collection to assign to"),
                    new Option("--and-set", "Additional Name=Value pair of a property assignment",
                        arity: ArgumentArity.ZeroOrMore)
                }.WithHandler<RuntimeHandlers>(nameof(RuntimeHandlers.HandleSet))
            };
            var validateCommands = new Command(ValidateCommandName, "Validating patterns from toolkits")
            {
                new Command("solution", "Validate the current solution")
                {
                    new Option("--on", "The expression of the element/collection to validate", arity: ArgumentArity.ZeroOrOne)
                }.WithHandler<RuntimeHandlers>(nameof(RuntimeHandlers.HandleValidate))
            };
            var executeCommands = new Command(ExecuteCommandName, "Executing automation on patterns from toolkits")
            {
                new Command("command", "Executes the launch point on the solution")
                {
                    new Argument("Name", "The name of the launch point to execute"),
                    new Option("--on", "The expression of the element/collection containing the launch point to execute", arity: ArgumentArity.ZeroOrOne)
                }.WithHandler<RuntimeHandlers>(nameof(RuntimeHandlers.HandleExecuteCommand))
            };
            var viewCommands = new Command(ViewCommandName, "Viewing patterns and solutions")
            {
                new Command("pattern", "View the configuration of the current pattern")
                {
                    new Option("--all", "Include additional configuration, like automation and code templates", typeof(bool), () => false, ArgumentArity.ZeroOrOne)
                }.WithHandler<AuthoringHandlers>(nameof(AuthoringHandlers.HandleViewPattern)),
                new Command("toolkit", "View the configuration of the current toolkit")
                {
                    new Option("--all", "Include additional configuration, like automation and code templates", typeof(bool), () => false, ArgumentArity.ZeroOrOne)
                }.WithHandler<RuntimeHandlers>(nameof(RuntimeHandlers.HandleViewToolkit)),
                new Command("solution", "View the configuration of the current solution")
                {
                    new Option("--todo", "Displays the details of the pattern, and any validation errors", typeof(bool), () => false, ArgumentArity.ZeroOrOne)
                }.WithHandler<RuntimeHandlers>(nameof(RuntimeHandlers.HandleViewSolution))
            };
            var listCommands = new Command(ListCommandName, "Listing patterns, toolkits and solutions")
            {
                new Command("patterns", "Lists all patterns being edited")
                    .WithHandler<AuthoringHandlers>(nameof(AuthoringHandlers.HandleListPatterns)),
                new Command("toolkits", "Lists all installed toolkits")
                    .WithHandler<RuntimeHandlers>(nameof(RuntimeHandlers.HandleListToolkits)),
                new Command("solutions", "Lists all solutions being configured")
                    .WithHandler<RuntimeHandlers>(nameof(RuntimeHandlers.HandleListSolutions))
            };
            var upgradeCommands = new Command(UpgradeCommandName, "Upgrading toolkits and solutions")
            {
                new Command("solution", "Upgrades a solution from a new version of its toolkit")
                    {
                        new Option("--force", "Force the upgrade despite any compatability errors")
                    }
                    .WithHandler<RuntimeHandlers>(nameof(RuntimeHandlers.HandleSolutionUpgrade))
            };
            var testingOnlyCommands = new Command("testingonly", "For testing only!")
            {
                new Option("--fail", "Throws a general exception", typeof(bool), () => false, ArgumentArity.ZeroOrOne)
            }.WithHandler<TestingOnlyHandlers>(nameof(TestingOnlyHandlers.HandleFail));

            var command =
                new RootCommand(
                    "Templatize patterns from your own codebase, make them programmable, then share them with your team")
                {
                    viewCommands,
                    listCommands,
                    createCommands,
                    editCommands,
                    testCommands,
                    buildCommands,
                    installCommands,
                    runCommands,
                    configureCommands,
                    validateCommands,
                    executeCommands,
                    upgradeCommands,
#if TESTINGONLY
                    testingOnlyCommands
#endif
                };
            command.AddGlobalOption(new Option("--output-structured", "Provide output as structured data",
                typeof(bool), () => false,
                ArgumentArity.ZeroOrOne));
            command.AddGlobalOption(new Option("--debug", "Show more error details when there is an exception",
                typeof(bool), () => false,
                ArgumentArity.ZeroOrOne));

            if (IsAuthoringCommand(args))
            {
                if (Authoring.CurrentPatternId.Exists())
                {
                    ConsoleExtensions.WriteOutput(
                        OutputMessages.CommandLine_Output_CurrentPatternInUse.FormatTemplate(Authoring.CurrentPatternName,
                            Authoring.CurrentPatternVersion), ConsoleColor.Gray);
                }
                else
                {
                    ConsoleExtensions.WriteErrorWarning(OutputMessages.CommandLine_Output_NoPatternSelected);
                }
            }
            if (IsRuntimeCommand(args))
            {
                if (IsRuntimeSolutionCommand(args))
                {
                    if (Runtime.CurrentSolutionId.Exists())
                    {
                        ConsoleExtensions.WriteOutput(
                            OutputMessages.CommandLine_Output_CurrentSolutionInUse.FormatTemplate(Runtime.CurrentSolutionName, Runtime.CurrentSolutionId), ConsoleColor.Gray);
                    }
                    else
                    {
                        ConsoleExtensions.WriteErrorWarning(OutputMessages.CommandLine_Output_NoSolutionSelected);
                    }
                }
            }

            var parser = new CommandLineBuilder(command)
                .UseDefaults()
                .UseExceptionHandler((ex, context) =>
                {
                    var isDebug = IsDebugging(context, ex);

                    var message = ex.InnerException.Exists()
                        ? isDebug ? ex.InnerException.ToString() : ex.InnerException.Message
                        : isDebug
                            ? ex.ToString()
                            : ex.Message;
                    Console.Error.WriteLine();
                    context.Console.WriteError($"Failed Unexpectedly, with: {message}", ConsoleColor.Red);
                }, 1)
                .UseHelp(context =>
                {
                    context.HelpBuilder.CustomizeLayout(_ =>
                    {
                        return HelpBuilder.Default.GetLayout()
                            .Prepend(_ => { WriteBanner(); });
                    });
                })
                .Build();

            var result = parser.Invoke(args);

            Console.WriteLine();

            return result;
        }

        private static void WriteBanner()
        {
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine(@"┌─┐┬ ┬┌┬┐┌─┐┌┬┐┌─┐┌┬┐┌─┐");
            Console.WriteLine(@"├─┤│ │ │ │ ││││├─┤ │ ├┤ ");
            Console.WriteLine(@"┴ ┴└─┘ ┴ └─┘┴ ┴┴ ┴ ┴ └─┘");
            Console.ResetColor();
        }

        private static bool IsDebugging(InvocationContext context, Exception ex)
        {
#if TESTINGONLY
            return true;
#else
            var debugOption = context.Parser.Configuration.RootCommand.Options.FirstOrDefault(opt => opt.Name == "debug");
            if (debugOption.Exists())
            {
                return (bool)context.ParseResult.FindResultFor(debugOption)!.GetValueOrDefault()!;
            }

            return false;
#endif
        }

        private static bool IsRuntimeCommand(IReadOnlyList<string> args)
        {
            if (args.Count < 1)
            {
                return false;
            }

            var isViewSolutionCommand = args[0] == ViewCommandName && args.Count == 2 && args[1] == "solution";
            var isListToolkitsCommand = args[0] == ListCommandName && args.Count == 2 && args[1] == "toolkits";
            var isListSolutionsCommand = args[0] == ListCommandName && args.Count == 2 && args[1] == "solutions";

            return args[0] == InstallCommandName || args[0] == RunCommandName || args[0] == ConfigureCommandName
                   || args[0] == ValidateCommandName || args[0] == ExecuteCommandName
                   || isViewSolutionCommand || isListToolkitsCommand || isListSolutionsCommand;
        }

        private static bool IsRuntimeSolutionCommand(IReadOnlyList<string> args)
        {
            if (args.Count < 1)
            {
                return false;
            }

            var isListToolkitsCommand = args[0] == ListCommandName && args.Count == 2 && args[1] == "toolkits";
            var isListSolutionsCommand = args[0] == ListCommandName && args.Count == 2 && args[1] == "solutions";

            return args[0] != InstallCommandName && args[0] != RunCommandName && !isListToolkitsCommand && !isListSolutionsCommand;
        }

        private static bool IsAuthoringCommand(IReadOnlyList<string> args)
        {
            if (args.Count < 1)
            {
                return false;
            }

            var isViewPatternCommand = args[0] == ViewCommandName && args.Count == 2 && args[1] == "pattern";

            return args[0] == CreateCommandName || args[0] == EditCommandName || args[0] == BuildCommandName
                   || args[0] == TestCommandName || isViewPatternCommand;
        }

        private class TestingOnlyHandlers
        {
            internal static void HandleFail(bool outputStructured, IConsole console)
            {
                throw new Exception("testingonly");
            }
        }

        private class AuthoringHandlers
        {
            private static readonly IPersistableFactory PersistenceFactory = new AutomatePersistableFactory();

            internal static void HandleAddCliCommand(string applicationName, string arguments, string name, string asChildOf, bool outputStructured, IConsole console)
            {
                var command = Authoring.AddCliCommand(applicationName, arguments, name, asChildOf);
                console.WriteOutput(outputStructured, OutputMessages.CommandLine_Output_CliCommandAdded,
                    command.Name, command.Id);
            }

            internal static void HandleBuild(string asversion, bool force, bool outputStructured, IConsole console)
            {
                var package = Authoring.BuildAndExportToolkit(asversion, force);
                console.WriteOutput(outputStructured, OutputMessages.CommandLine_Output_BuiltToolkit,
                    package.Toolkit.PatternName, package.Toolkit.Version, package.ExportedLocation);
                if (package.Message.HasValue())
                {
                    console.WriteOutputWarning(outputStructured, OutputMessages.CommandLine_Output_BuiltToolkit_Warning, package.Message);
                }
            }

            internal static void HandleAddCodeTemplateCommand(string codeTemplateName, string name, bool asTearOff,
                string withPath, string asChildOf, bool outputStructured, IConsole console)
            {
                var command = Authoring.AddCodeTemplateCommand(codeTemplateName, name, asTearOff, withPath, asChildOf);
                console.WriteOutput(outputStructured, OutputMessages.CommandLine_Output_CodeTemplateCommandAdded,
                    command.Name,
                    command.Id);
            }

            internal static void HandleUpdateCodeTemplateCommand(string commandName, string name, bool? asTearOff,
                string withPath, string asChildOf, bool outputStructured, IConsole console)
            {
                var command = Authoring.UpdateCodeTemplateCommand(commandName, name, asTearOff, withPath, asChildOf);
                console.WriteOutput(outputStructured, OutputMessages.CommandLine_Output_CodeTemplateCommandUpdated,
                    command.Name, command.Id, command.Metadata[nameof(CodeTemplateCommand.FilePath)], command.Metadata[nameof(CodeTemplateCommand.IsTearOff)]);
            }

            internal static void HandleAddCommandLaunchPoint(string commandIdentifiers, string name, string asChildOf,
                bool outputStructured, IConsole console)
            {
                var cmdIds = commandIdentifiers.SafeSplit(CommandLaunchPoint.CommandIdDelimiter, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).ToList();
                var launchPoint = Authoring.AddCommandLaunchPoint(name, cmdIds, asChildOf);
                console.WriteOutput(outputStructured, OutputMessages.CommandLine_Output_LaunchPointAdded,
                    launchPoint.Name, launchPoint.Id, launchPoint.Metadata[nameof(CommandLaunchPoint.CommandIds)]);
            }

            internal static void HandleUpdateCommandLaunchPoint(string launchPointName, string name, string add, string from, string asChildOf,
                bool outputStructured, IConsole console)
            {
                var cmdIds = add.SafeSplit(CommandLaunchPoint.CommandIdDelimiter, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).ToList();
                var launchPoint = Authoring.UpdateCommandLaunchPoint(launchPointName, name, cmdIds, from, asChildOf);
                console.WriteOutput(outputStructured, OutputMessages.CommandLine_Output_LaunchPointUpdated,
                    launchPoint.Name, launchPoint.Id, launchPoint.Metadata[nameof(CommandLaunchPoint.CommandIds)]);
            }

            internal static void HandleAddElement(string name, string displayedAs, string describedAs, string asChildOf,
                bool isRequired, bool outputStructured, IConsole console)
            {
                var (parent, element) = Authoring.AddElement(name,
                    isRequired
                        ? ElementCardinality.One
                        : ElementCardinality.ZeroOrOne, displayedAs, describedAs, asChildOf);
                console.WriteOutput(outputStructured, OutputMessages.CommandLine_Output_ElementAdded, name, parent.Id,
                    element.Id);
            }

            internal static void HandleAddCollection(string name, string displayedAs, string describedAs,
                string asChildOf, bool isRequired, bool outputStructured, IConsole console)
            {
                var (parent, collection) = Authoring.AddElement(name, isRequired
                    ? ElementCardinality.OneOrMany
                    : ElementCardinality.ZeroOrMany, displayedAs, describedAs, asChildOf);
                console.WriteOutput(outputStructured, OutputMessages.CommandLine_Output_CollectionAdded, name,
                    parent.Id, collection.Id);
            }

            internal static void HandleAddAttribute(string name, string isOfType, string defaultValueIs,
                bool isRequired,
                string isOneOf, string asChildOf, bool outputStructured, IConsole console)
            {
                var choices = isOneOf.SafeSplit(";").ToList();
                var (parent, attribute) =
                    Authoring.AddAttribute(name, isOfType, defaultValueIs, isRequired, choices, asChildOf);
                console.WriteOutput(outputStructured, OutputMessages.CommandLine_Output_AttributeAdded, name,
                    parent.Id, attribute.Id);
            }

            internal static void HandleDeleteAttribute(string name, string asChildOf, bool outputStructured, IConsole console)
            {
                var (parent, attribute) =
                    Authoring.DeleteAttribute(name, asChildOf);
                console.WriteOutput(outputStructured, OutputMessages.CommandLine_Output_AttributeDeleted, name,
                    parent.Id, attribute.Id);
            }

            internal static void HandleDeleteElement(string name, string asChildOf, bool outputStructured, IConsole console)
            {
                var (parent, element) =
                    Authoring.DeleteElement(name, asChildOf);
                console.WriteOutput(outputStructured, OutputMessages.CommandLine_Output_ElementDeleted, name,
                    parent.Id, element.Id);
            }

            internal static void HandleDeleteCollection(string name, string asChildOf, bool outputStructured, IConsole console)
            {
                var (parent, element) =
                    Authoring.DeleteElement(name, asChildOf);
                console.WriteOutput(outputStructured, OutputMessages.CommandLine_Output_CollectionDeleted, name,
                    parent.Id, element.Id);
            }

            internal static void HandleCreate(string name, bool outputStructured, IConsole console)
            {
                Authoring.CreateNewPattern(name);
                console.WriteOutput(outputStructured,
                    OutputMessages.CommandLine_Output_PatternCreated, name, Authoring.CurrentPatternId);
            }

            internal static void HandleViewPattern(bool all, bool outputStructured, IConsole console)
            {
                var pattern = Authoring.GetCurrentPattern();

                console.WriteOutput(outputStructured,
                    OutputMessages.CommandLine_Output_PatternConfiguration, FormatPatternConfiguration(outputStructured, pattern, all));
            }

            internal static void HandleSwitch(string name, bool outputStructured, IConsole console)
            {
                Authoring.SwitchCurrentPattern(name);
                console.WriteOutput(outputStructured,
                    OutputMessages.CommandLine_Output_PatternSwitched, name, Authoring.CurrentPatternId);
            }

            internal static void HandleAddCodeTemplate(string filepath, string name, string asChildOf,
                bool outputStructured, IConsole console)
            {
                var currentDirectory = Environment.CurrentDirectory;
                var uploaded = Authoring.AttachCodeTemplate(currentDirectory, filepath, name, asChildOf);
                console.WriteOutput(outputStructured,
                    OutputMessages.CommandLine_Output_CodeTemplatedAdded, uploaded.Template.Name, uploaded.Template.Id,
                    uploaded.Template.Metadata.OriginalFilePath, uploaded.Location);
            }

            internal static void HandleTestCodeTemplate(string name, string asChildOf, string importData, string exportData, bool outputStructured, IConsole console)
            {
                var currentDirectory = Environment.CurrentDirectory;
                var result = Authoring.TestCodeTemplate(name, asChildOf, currentDirectory, importData, exportData);
                if (exportData.HasValue())
                {
                    console.WriteOutput(outputStructured, OutputMessages.CommandLine_Output_CodeTemplateTestExported, name, result.Template.Id, exportData);
                    console.WriteOutputLine();
                }

                if (importData.HasValue())
                {
                    console.WriteOutput(outputStructured, OutputMessages.CommandLine_Output_CodeTemplateTestImported, name, result.Template.Id, importData);
                    console.WriteOutputLine();
                }

                console.WriteOutput(outputStructured, OutputMessages.CommandLine_Output_CodeTemplateTested, name, result.Template.Id, result.Output);
            }

            internal static void HandleListPatterns(bool outputStructured, IConsole console)
            {
                var patterns = Authoring.ListPatterns();
                if (patterns.Any())
                {
                    console.WriteOutput(outputStructured, OutputMessages.CommandLine_Output_EditablePatternsListed,
                        patterns.ToMultiLineText(pattern =>
                            $"{{\"Name\": \"{pattern.Name}\", \"Version\": \"{pattern.ToolkitVersion.Current}\", \"ID\": \"{pattern.Id}\"}}"));
                }
                else
                {
                    console.WriteOutput(outputStructured, OutputMessages.CommandLine_Output_NoEditablePatterns);
                }
            }

            internal static string FormatPatternConfiguration(bool outputStructured, PatternDefinition pattern, bool isDetailed)
            {
                if (outputStructured)
                {
                    return pattern.ToJson(PersistenceFactory);
                }

                var configuration = new PatternConfigurationVisitor(isDetailed);
                pattern.TraverseDescendants(configuration);
                return configuration.ToString();
            }
        }

        private class RuntimeHandlers
        {
            internal static void HandleSolutionUpgrade(bool force, bool outputStructured, IConsole console)
            {
                var upgrade = Runtime.UpgradeSolution(force);
                if (upgrade.IsSuccess)
                {
                    if (upgrade.Log.Any(entry => entry.Type == MigrationChangeType.Abort))
                    {
                        console.WriteOutputWarning(outputStructured, OutputMessages.CommandLine_Output_SolutionUpgradeWithWarning,
                            upgrade.Solution.Name, upgrade.Solution.Id, upgrade.Solution.PatternName, upgrade.FromVersion, upgrade.ToVersion, FormatUpgradeLog(upgrade.Log));
                    }
                    else
                    {
                        console.WriteOutput(outputStructured, OutputMessages.CommandLine_Output_SolutionUpgradeSucceeded,
                            upgrade.Solution.Name, upgrade.Solution.Id, upgrade.Solution.PatternName, upgrade.FromVersion, upgrade.ToVersion, FormatUpgradeLog(upgrade.Log));
                    }
                }
                else
                {
                    console.WriteError(outputStructured, OutputMessages.CommandLine_Output_SolutionUpgradeFailed,
                        upgrade.Solution.Name, upgrade.Solution.Id, upgrade.Solution.PatternName, upgrade.FromVersion, upgrade.ToVersion, FormatUpgradeLog(upgrade.Log));
                }
            }

            internal static void HandleInstall(string location, bool outputStructured, IConsole console)
            {
                var toolkit = Runtime.InstallToolkit(location);
                console.WriteOutput(outputStructured, OutputMessages.CommandLine_Output_InstalledToolkit,
                    toolkit.PatternName, toolkit.Version);
            }

            internal static void HandleListToolkits(bool outputStructured, IConsole console)
            {
                var toolkits = Runtime.ListInstalledToolkits();
                if (toolkits.Any())
                {
                    console.WriteOutput(outputStructured, OutputMessages.CommandLine_Output_InstalledToolkitsListed,
                        toolkits.ToMultiLineText(toolkit =>
                            $"{{\"Name\": \"{toolkit.PatternName}\", \"Version\": \"{toolkit.Version}\", \"ID\": \"{toolkit.Id}\"}}"));
                }
                else
                {
                    console.WriteOutput(outputStructured, OutputMessages.CommandLine_Output_NoInstalledToolkits);
                }
            }

            internal static void HandleNewSolution(string patternName, string name, bool outputStructured, IConsole console)
            {
                var solution = Runtime.CreateSolution(patternName, name);
                console.WriteOutput(outputStructured, OutputMessages.CommandLine_Output_CreateSolutionFromToolkit, solution.Name, solution.Id, solution.PatternName);
            }

            internal static void HandleListSolutions(bool outputStructured, IConsole console)
            {
                var solutions = Runtime.ListCreatedSolutions();
                if (solutions.Any())
                {
                    console.WriteOutput(outputStructured, OutputMessages.CommandLine_Output_InstalledSolutionsListed,
                        solutions.ToMultiLineText(solution =>
                            $"{{\"Name\": \"{solution.Name}\", \"ID\": \"{solution.Id}\", \"Version\": \"{solution.Toolkit.Version}\"}}"));
                }
                else
                {
                    console.WriteOutput(outputStructured, OutputMessages.CommandLine_Output_NoInstalledSolutions);
                }
            }

            internal static void HandleSwitch(string solutionId, bool outputStructured, IConsole console)
            {
                Runtime.SwitchCurrentSolution(solutionId);
                console.WriteOutput(outputStructured,
                    OutputMessages.CommandLine_Output_SolutionSwitched, Runtime.CurrentSolutionName, Runtime.CurrentSolutionId);
            }

            internal static void HandleAddTo(string expression, string[] andSet, bool outputStructured, IConsole console)
            {
                var sets = new List<string>();
                if (andSet.HasAny())
                {
                    sets.AddRange(andSet);
                }

                var nameValues = sets
                    .Select(ParsePropertyAssignment)
                    .ToDictionary(pair => pair.Name, pair => pair.Value);

                var solutionItem = Runtime.ConfigureSolution(expression, null, null, nameValues);
                console.WriteOutput(outputStructured, OutputMessages.CommandLine_Output_SolutionConfigured, solutionItem.Name, solutionItem.Id);
            }

            internal static void HandleAddOneTo(string expression, string[] andSet, bool outputStructured, IConsole console)
            {
                var sets = new List<string>();
                if (andSet.HasAny())
                {
                    sets.AddRange(andSet);
                }
                var nameValues = sets
                    .Select(ParsePropertyAssignment)
                    .ToDictionary(pair => pair.Name, pair => pair.Value);

                var solutionItem = Runtime.ConfigureSolution(null, expression, null, nameValues);
                console.WriteOutput(outputStructured, OutputMessages.CommandLine_Output_SolutionConfigured, solutionItem.Name, solutionItem.Id);
            }

            internal static void HandleSet(string expression, string[] andSet, bool outputStructured, IConsole console)
            {
                var sets = new List<string>();
                if (andSet.HasAny())
                {
                    sets.AddRange(andSet);
                }
                var nameValues = sets
                    .Select(ParsePropertyAssignment)
                    .ToDictionary(pair => pair.Name, pair => pair.Value);

                var solutionItem = Runtime.ConfigureSolution(null, null, expression, nameValues);
                console.WriteOutput(outputStructured, OutputMessages.CommandLine_Output_SolutionConfigured, solutionItem.Name, solutionItem.Id);
            }

            internal static void HandleViewSolution(bool todo, bool outputStructured, IConsole console)
            {
                var (configuration, pattern, validation) = Runtime.GetSolutionConfiguration(todo, todo);

                var solutionId = Runtime.CurrentSolutionId;
                var solutionName = Runtime.CurrentSolutionName;
                console.WriteOutput(outputStructured, OutputMessages.CommandLine_Output_SolutionConfiguration,
                    solutionName, solutionId, configuration);

                if (todo)
                {
                    console.WriteOutputLine();
                    console.WriteOutput(outputStructured,
                        OutputMessages.CommandLine_Output_PatternConfiguration, AuthoringHandlers.FormatPatternConfiguration(outputStructured, pattern, true));
                }

                if (todo)
                {
                    console.WriteOutputLine();
                    if (validation.HasAny())
                    {
                        console.WriteOutputWarning(outputStructured, OutputMessages.CommandLine_Output_SolutionValidationFailed,
                            solutionName, solutionId, FormatValidationErrors(validation));
                    }
                    else
                    {
                        console.WriteOutput(outputStructured, OutputMessages.CommandLine_Output_SolutionValidationSuccess, solutionName, solutionId);
                    }
                }
            }

            internal static void HandleViewToolkit(bool all, bool outputStructured, IConsole console)
            {
                var pattern = Runtime.GetCurrentToolkit().Pattern;

                console.WriteOutput(outputStructured,
                    OutputMessages.CommandLine_Output_ToolkitConfiguration, AuthoringHandlers.FormatPatternConfiguration(outputStructured, pattern, all));
            }

            internal static void HandleValidate(string on,
                bool outputStructured, IConsole console)
            {
                var results = Runtime.Validate(on);

                var solutionId = Runtime.CurrentSolutionId;
                var solutionName = Runtime.CurrentSolutionName;
                if (results.HasAny())
                {
                    console.WriteOutputWarning(outputStructured, OutputMessages.CommandLine_Output_SolutionValidationFailed,
                        solutionName, solutionId, FormatValidationErrors(results));
                }
                else
                {
                    console.WriteOutput(outputStructured, OutputMessages.CommandLine_Output_SolutionValidationSuccess, solutionName, solutionId);
                }
            }

            internal static void HandleExecuteCommand(string name, string on, bool outputStructured,
                IConsole console)
            {
                var execution = Runtime.ExecuteLaunchPoint(name, on);
                if (execution.IsSuccess)
                {
                    console.WriteOutput(outputStructured, OutputMessages.CommandLine_Output_CommandExecutionSucceeded,
                        execution.CommandName, FormatExecutionLog(execution.Log));
                }
                else
                {
                    if (execution.IsInvalid)
                    {
                        console.WriteOutputWarning(outputStructured, OutputMessages.CommandLine_Output_SolutionValidationFailed,
                            Runtime.CurrentSolutionName, Runtime.CurrentSolutionId, FormatValidationErrors(execution.ValidationErrors));
                    }
                    else
                    {
                        console.WriteOutputWarning(outputStructured, OutputMessages.CommandLine_Output_CommandExecutionFailed,
                            execution.CommandName, FormatExecutionLog(execution.Log));
                    }
                }
            }

            private static string FormatValidationErrors(ValidationResults results)
            {
                var builder = new StringBuilder();
                var counter = 1;
                results.Results.ToList()
                    .ForEach(result => { builder.AppendLine($"{counter++}. {result.Context.Path} {result.Message}"); });

                return builder.ToString();
            }

            private static string FormatExecutionLog(IReadOnlyList<string> items)
            {
                var builder = new StringBuilder();
                items.ToList()
                    .ForEach(item => { builder.AppendLine($"* {item}"); });

                return builder.ToString();
            }

            private static string FormatUpgradeLog(IReadOnlyList<MigrationChange> items)
            {
                var builder = new StringBuilder();
                items.ToList()
                    .ForEach(item => { builder.AppendLine($"* {item.Type}: {item.MessageTemplate.FormatTemplate(item.Arguments.ToArray())}"); });

                return builder.ToString();
            }

            private static (string Name, string Value) ParsePropertyAssignment(string expression)
            {
                var parts = expression.Split('=');
                return (parts.First(), parts.Last());
            }
        }
    }

    internal static class ConsoleExtensions
    {
        public static void WriteOutput(this IConsole console, bool outputStructured, string messageTemplate,
            params object[] args)
        {
            console.WriteLine(string.Empty);
            console.WriteLine(outputStructured
                ? messageTemplate.FormatTemplateStructured(args)
                : messageTemplate.FormatTemplate(args));
        }

        public static void WriteOutputWarning(this IConsole console, bool outputStructured, string messageTemplate,
            params object[] args)
        {
            console.WriteLine(string.Empty);
            console.WriteOutput(outputStructured
                ? messageTemplate.FormatTemplateStructured(args)
                : messageTemplate.FormatTemplate(args), ConsoleColor.DarkYellow);
        }

        public static void WriteError(this IConsole console, bool outputStructured, string messageTemplate,
            params object[] args)
        {
            console.WriteError(string.Empty, ConsoleColor.Red);
            console.WriteError(outputStructured
                ? messageTemplate.FormatTemplateStructured(args)
                : messageTemplate.FormatTemplate(args), ConsoleColor.Red);
        }

        public static void WriteError(this IConsole console, string message, ConsoleColor color)
        {
            Console.ResetColor();
            Console.ForegroundColor = color;
            console.Error.WriteLine(message);
            Console.Error.WriteLine();
            Console.ResetColor();
        }

        public static void WriteErrorWarning(string message)
        {
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.Error.WriteLine(message);
            Console.ResetColor();
        }

        public static void WriteOutputLine(this IConsole console)
        {
            Console.ResetColor();
            console.WriteLine(string.Empty);
        }

        public static void WriteOutput(this IConsole console, string message, ConsoleColor color)
        {
            Console.ResetColor();
            Console.ForegroundColor = color;
            console.WriteLine(message);
            Console.ResetColor();
        }

        public static void WriteOutput(string message, ConsoleColor color)
        {
            Console.ResetColor();
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ResetColor();
        }
    }
}