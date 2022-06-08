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
    internal static class CommandLineApi
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
        private static readonly AuthoringApplication Authoring = new(Environment.CurrentDirectory);
        private static readonly RuntimeApplication Runtime = new(Environment.CurrentDirectory);

        public static int Execute(string[] args)
        {
            var createCommands = new Command(CreateCommandName, "Creating new patterns")
            {
                new Command("pattern", "Creates a new pattern")
                {
                    new Argument("Name", "The name of the pattern to create"),
                    new Option("--displayedas", "A friendly display name for the pattern", typeof(string),
                        arity: ArgumentArity.ZeroOrOne),
                    new Option("--describedas", "A description for the pattern", typeof(string),
                        arity: ArgumentArity.ZeroOrOne)
                }.WithHandler<AuthoringHandlers>(nameof(AuthoringHandlers.HandleCreatePattern))
            };
            var editCommands = new Command(EditCommandName, "Editing patterns")
            {
                new Command("switch", "Switches to configuring another pattern")
                {
                    new Argument("Name", "The name of the existing pattern to edit")
                }.WithHandler<AuthoringHandlers>(nameof(AuthoringHandlers.HandleSwitch)),
                new Command("update-pattern", "Updates the pattern")
                {
                    new Option("--name", "A new name for the pattern", typeof(string),
                        arity: ArgumentArity.ZeroOrOne),
                    new Option("--displayedas", "A new friendly display name for the pattern", typeof(string),
                        arity: ArgumentArity.ZeroOrOne),
                    new Option("--describedas", "A new description for the pattern", typeof(string),
                        arity: ArgumentArity.ZeroOrOne)
                }.WithHandler<AuthoringHandlers>(nameof(AuthoringHandlers.HandleUpdatePattern)),
                new Command("add-attribute", "Adds an attribute to an element/collection in the pattern")
                {
                    new Argument("Name", "The name of the attribute"),
                    new Option("--isoftype", "The type of the attribute", typeof(string)),
                    new Option("--defaultvalueis", "The default value for the attribute", typeof(string),
                        arity: ArgumentArity.ZeroOrOne),
                    new Option("--isrequired", "Whether an attribute value will be required", typeof(bool),
                        () => false, ArgumentArity.ZeroOrOne),
                    new Option("--isoneof", "A list of semi-colon delimited choices", typeof(string),
                        arity: ArgumentArity.ZeroOrOne),
                    new Option("--aschildof", "The expression of the element/collection to add the attribute to",
                        typeof(string), arity: ArgumentArity.ZeroOrOne)
                }.WithHandler<AuthoringHandlers>(nameof(AuthoringHandlers.HandleAddAttribute)),
                new Command("update-attribute", "Updates an attribute on an element/collection in the pattern")
                {
                    new Argument("AttributeName", "The name of the attribute"),
                    new Option("--name", "A new name for the attribute", typeof(string),
                        arity: ArgumentArity.ZeroOrOne),
                    new Option("--isoftype", "The new type of the attribute", typeof(string),
                        arity: ArgumentArity.ZeroOrOne),
                    new Option("--defaultvalueis", "The new default value for the attribute", typeof(string),
                        arity: ArgumentArity.ZeroOrOne),
                    new Option("--isrequired", "Whether an attribute value will be required or not", typeof(bool?),
                        () => null, ArgumentArity.ZeroOrOne),
                    new Option("--isoneof", "A new list of semi-colon delimited choices", typeof(string),
                        arity: ArgumentArity.ZeroOrOne),
                    new Option("--aschildof", "The expression of the element/collection to update the attribute on",
                        typeof(string), arity: ArgumentArity.ZeroOrOne)
                }.WithHandler<AuthoringHandlers>(nameof(AuthoringHandlers.HandleUpdateAttribute)),
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
                        typeof(bool), () => true, ArgumentArity.ZeroOrOne),
                    new Option("--autocreate", "Whether the element will be created automatically or not",
                        typeof(bool?), () => null, ArgumentArity.ZeroOrOne)
                }.WithHandler<AuthoringHandlers>(nameof(AuthoringHandlers.HandleAddElement)),
                new Command("update-element", "Updates an element on an element/collection in the pattern")
                {
                    new Argument("ElementName", "The name of the element"),
                    new Option("--name", "A new name for the element", typeof(string),
                        arity: ArgumentArity.ZeroOrOne),
                    new Option("--displayedas", "A new friendly display name for the element", typeof(string),
                        arity: ArgumentArity.ZeroOrOne),
                    new Option("--describedas", "A new description for the element", typeof(string),
                        arity: ArgumentArity.ZeroOrOne),
                    new Option("--aschildof", "The expression of the element/collection to update the element on",
                        typeof(string), arity: ArgumentArity.ZeroOrOne),
                    new Option("--isrequired", "Whether the element will now be required or not",
                        typeof(bool?), () => null, ArgumentArity.ZeroOrOne),
                    new Option("--autocreate", "Whether the element will now be created automatically or not",
                        typeof(bool?), () => null, ArgumentArity.ZeroOrOne)
                }.WithHandler<AuthoringHandlers>(nameof(AuthoringHandlers.HandleUpdateElement)),
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
                        typeof(bool), () => false, ArgumentArity.ZeroOrOne),
                    new Option("--autocreate", "Whether the collection will be created automatically or not",
                        typeof(bool?), () => null, ArgumentArity.ZeroOrOne)
                }.WithHandler<AuthoringHandlers>(nameof(AuthoringHandlers.HandleAddCollection)),
                new Command("update-collection", "Updates a collection on an element/collection in the pattern")
                {
                    new Argument("CollectionName", "The name of the collection"),
                    new Option("--name", "A new name for the element", typeof(string),
                        arity: ArgumentArity.ZeroOrOne),
                    new Option("--displayedas", "A new friendly display name for the collection", typeof(string),
                        arity: ArgumentArity.ZeroOrOne),
                    new Option("--describedas", "A new description for the collection", typeof(string),
                        arity: ArgumentArity.ZeroOrOne),
                    new Option("--aschildof", "The expression of the element/collection to update the collection on",
                        typeof(string), arity: ArgumentArity.ZeroOrOne),
                    new Option("--isrequired", "Whether the collection will now be required or not",
                        typeof(bool?), () => null, ArgumentArity.ZeroOrOne),
                    new Option("--autocreate", "Whether the collection will now be created automatically or not",
                        typeof(bool?), () => null, ArgumentArity.ZeroOrOne)
                }.WithHandler<AuthoringHandlers>(nameof(AuthoringHandlers.HandleUpdateCollection)),
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
                    new Option("--aschildof", "The expression of the element/collection to add the code template to",
                        typeof(string), arity: ArgumentArity.ZeroOrOne)
                }.WithHandler<AuthoringHandlers>(nameof(AuthoringHandlers.HandleAddCodeTemplate)),
                new Command("codetemplate", "Edits a code template in an editor")
                {
                    new Argument("TemplateName", "The name of the code template"),
                    new Option("--with", "Name of or full path to an application to edit the code template",
                        typeof(string), arity: ArgumentArity.ExactlyOne),
                    new Option("--args", "An program arguments",
                        typeof(string), arity: ArgumentArity.ZeroOrOne),
                    new Option("--aschildof", "The expression of the element/collection to edit the code template from",
                        typeof(string), arity: ArgumentArity.ZeroOrOne)
                }.WithHandler<AuthoringHandlers>(nameof(AuthoringHandlers.HandleEditCodeTemplate)),
                new Command("delete-codetemplate", "Deletes a code template from an element/collection in the pattern")
                {
                    new Argument("TemplateName", "The name of the code template"),
                    new Option("--aschildof",
                        "The expression of the element/collection to delete the code template from",
                        typeof(string), arity: ArgumentArity.ZeroOrOne)
                }.WithHandler<AuthoringHandlers>(nameof(AuthoringHandlers.HandleDeleteCodeTemplate)),
                new Command("add-codetemplate-command", "Adds a command that renders a code template")
                {
                    new Argument("CodeTemplateName", "The name of the code template"),
                    new Option("--name", "A name for the command", typeof(string),
                        arity: ArgumentArity.ZeroOrOne),
                    new Option("--isoneoff",
                        "Only generate the file the first time if it does not already exist",
                        typeof(bool), () => false, ArgumentArity.ZeroOrOne),
                    new Option("--targetpath", "The full path of the generated file, with filename.", typeof(string),
                        arity: ArgumentArity.ExactlyOne),
                    new Option("--aschildof", "The expression of the element/collection to add the command to",
                        typeof(string), arity: ArgumentArity.ZeroOrOne)
                }.WithHandler<AuthoringHandlers>(nameof(AuthoringHandlers.HandleAddCodeTemplateCommand)),
                new Command("update-codetemplate-command", "Updates an existing code template command")
                {
                    new Argument("CommandName", "The name of the command to update"),
                    new Option("--name", "A new name for the command", typeof(string),
                        arity: ArgumentArity.ZeroOrOne),
                    new Option("--isoneoff",
                        "Only generate the file the first time, if it does not already exist",
                        typeof(bool?), () => null, ArgumentArity.ZeroOrOne),
                    new Option("--targetpath", "A new full path of the generated file, with filename.", typeof(string),
                        arity: ArgumentArity.ZeroOrOne),
                    new Option("--aschildof", "The expression of the element/collection on which the command exists",
                        typeof(string), arity: ArgumentArity.ZeroOrOne)
                }.WithHandler<AuthoringHandlers>(nameof(AuthoringHandlers.HandleUpdateCodeTemplateCommand)),
                new Command("add-cli-command", "Adds a command that executes another command line application")
                    {
                        new Argument("ApplicationName",
                            "The name of the command line application/exe to execute. Include the full path if the application is not in the machine's path variable"),
                        new Option("--arguments",
                            "The arguments to pass to the command line application. (Escape double-quotes with an extra double-quote)",
                            typeof(string), arity: ArgumentArity.ZeroOrOne),
                        new Option("--name", "A name for the command", typeof(string),
                            arity: ArgumentArity.ZeroOrOne),
                        new Option("--aschildof",
                            "The expression of the element/collection on which the command exists",
                            typeof(string), arity: ArgumentArity.ZeroOrOne)
                    }
                    .WithHandler<AuthoringHandlers>(nameof(AuthoringHandlers.HandleAddCliCommand)),
                new Command("update-cli-command", "Updates an existing CLI command")
                {
                    new Argument("CommandName", "The name of the command to update"),
                    new Option("--app",
                        "The new name of the command line application/exe to execute. Include the full path if the application is not in the machine's path variable",
                        typeof(string), arity: ArgumentArity.ZeroOrOne),
                    new Option("--arguments",
                        "The new arguments to pass to the command line application. (Escape double-quotes with an extra double-quote)",
                        typeof(string), arity: ArgumentArity.ZeroOrOne),
                    new Option("--name", "A new name for the command", typeof(string),
                        arity: ArgumentArity.ZeroOrOne),
                    new Option("--aschildof",
                        "The expression of the element/collection on which the command exists",
                        typeof(string), arity: ArgumentArity.ZeroOrOne)
                }.WithHandler<AuthoringHandlers>(nameof(AuthoringHandlers.HandleUpdateCliCommand)),
                new Command("delete-command", "Deletes any command from an element/collection in the pattern")
                {
                    new Argument("CommandName", "The name of the command"),
                    new Option("--aschildof", "The expression of the element/collection to delete the command from",
                        typeof(string), arity: ArgumentArity.ZeroOrOne)
                }.WithHandler<AuthoringHandlers>(nameof(AuthoringHandlers.HandleDeleteCommand)),
                new Command("add-command-launchpoint", "Adds a launch point for a command")
                {
                    new Argument("CommandIdentifiers",
                        "A semi-colon delimited list of identifiers of the commands to launch (from anywhere in the pattern), or '*' to launch all commands found on the element/collection"),
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
                    new Option("--add",
                        "A semi-colon delimited list of identifiers of the commands to add (from anywhere in the pattern), or '*' to add all commands on the from element/collection)",
                        typeof(string), arity: ArgumentArity.ExactlyOne),
                    new Option("--from", "The expression of the element/collection to add commands from",
                        typeof(string), arity: ArgumentArity.ZeroOrOne),
                    new Option("--aschildof",
                        "The expression of the element/collection on which the launch point exists",
                        typeof(string), arity: ArgumentArity.ZeroOrOne)
                }.WithHandler<AuthoringHandlers>(nameof(AuthoringHandlers.HandleUpdateLaunchPoint)),
                new Command("delete-command-launchpoint",
                    "Deletes a launch point from an element/collection in the pattern")
                {
                    new Argument("LaunchPointName", "The name of the launch point"),
                    new Option("--aschildof",
                        "The expression of the element/collection to delete the launch point from",
                        typeof(string), arity: ArgumentArity.ZeroOrOne)
                }.WithHandler<AuthoringHandlers>(nameof(AuthoringHandlers.HandleDeleteLaunchPoint))
            };
            var testCommands = new Command(TestCommandName, "Testing automation of a pattern")
            {
                new Command("codetemplate", "Tests a code template")
                {
                    new Argument("TemplateName", "The name of the code template"),
                    new Option("--aschildof",
                        "The expression of the element/collection on which the code template exists",
                        typeof(string), arity: ArgumentArity.ZeroOrOne),
                    new Option("--import-data",
                        "Import the specified data for the test. A relative path to the JSON file, from the current directory",
                        typeof(string), arity: ArgumentArity.ZeroOrOne),
                    new Option("--export-data",
                        "Export the generated test data to the specified file. A relative path to the JSON file, from the current directory",
                        typeof(string), arity: ArgumentArity.ZeroOrOne)
                }.WithHandler<AuthoringHandlers>(nameof(AuthoringHandlers.HandleTestCodeTemplate)),
                new Command("codetemplate-command", "Tests a code template command")
                {
                    new Argument("CommandName", "The name of the command"),
                    new Option("--aschildof",
                        "The expression of the element/collection on which the command exists",
                        typeof(string), arity: ArgumentArity.ZeroOrOne),
                    new Option("--import-data",
                        "Import the specified data for the test. A relative path to the JSON file, from the current directory",
                        typeof(string), arity: ArgumentArity.ZeroOrOne),
                    new Option("--export-data",
                        "Export the generated test data to the specified file. A relative path to the JSON file, from the current directory",
                        typeof(string), arity: ArgumentArity.ZeroOrOne)
                }.WithHandler<AuthoringHandlers>(nameof(AuthoringHandlers.HandleTestCodeTemplateCommand))
            };
            var buildCommands = new Command(BuildCommandName, "Building toolkits from patterns")
            {
                new Command("toolkit", "Builds a pattern into a toolkit")
                {
                    new Option("--asversion",
                        "A specific version number (1-2 dot number), or 'auto' to auto-increment the current version",
                        typeof(string), arity: ArgumentArity.ZeroOrOne),
                    new Option("--force",
                        "Force the specified version number even it it violates breaking changes checks",
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
                new Command("toolkit", "Creates a new draft from a toolkit")
                {
                    new Argument("PatternName", "The name of the pattern in the toolkit that you want to use"),
                    new Option("--name", "A name for the draft", arity: ArgumentArity.ZeroOrOne)
                }.WithHandler<RuntimeHandlers>(nameof(RuntimeHandlers.HandleNewDraft)),
                new Command("switch", "Switches to configuring another draft")
                {
                    new Argument("DraftId", "The id of the existing draft to configure")
                }.WithHandler<RuntimeHandlers>(nameof(RuntimeHandlers.HandleSwitch))
            };
            var configureCommands = new Command(ConfigureCommandName, "Configuring drafts to patterns from toolkits")
            {
                new Command("add", "Configure an element in the draft")
                {
                    new Argument("Expression", "The expression of the element to configure"),
                    new Option("--and-set", "A Name=Value pair of a property assignment",
                        arity: ArgumentArity.ZeroOrMore)
                }.WithHandler<RuntimeHandlers>(nameof(RuntimeHandlers.HandleConfigureAddTo)),
                new Command("add-one-to", "Add a new item to a collection in the draft")
                {
                    new Argument("Expression", "The expression of the element/collection to add to"),
                    new Option("--and-set", "Additional Name=Value pair of a property assignment",
                        arity: ArgumentArity.ZeroOrMore)
                }.WithHandler<RuntimeHandlers>(nameof(RuntimeHandlers.HandleConfigureAddOneTo)),
                new Command("on", "Set the properties of an existing item in the draft")
                {
                    new Argument("Expression", "The expression of the element/collection to assign to"),
                    new Option("--and-set", "Additional Name=Value pair of a property assignment",
                        arity: ArgumentArity.ZeroOrMore)
                }.WithHandler<RuntimeHandlers>(nameof(RuntimeHandlers.HandleConfigureOn)),
                new Command("reset", "Reset all the properties of an existing item in the draft")
                {
                    new Argument("Expression", "The expression of the element to reset")
                }.WithHandler<RuntimeHandlers>(nameof(RuntimeHandlers.HandleConfigureResetElement)),
                new Command("clear", "Deletes all the items from an existing collection in the draft")
                {
                    new Argument("Expression", "The expression of the collection to clear")
                }.WithHandler<RuntimeHandlers>(nameof(RuntimeHandlers.HandleConfigureClearCollection)),
                new Command("delete", "Deletes an existing item in the draft")
                {
                    new Argument("Expression", "The expression of the element/collection to delete")
                }.WithHandler<RuntimeHandlers>(nameof(RuntimeHandlers.HandleConfigureDelete))
            };
            var validateCommands = new Command(ValidateCommandName, "Validating patterns from toolkits")
            {
                new Command("draft", "Validate the current draft")
                {
                    new Option("--on", "The expression of the element/collection to validate",
                        arity: ArgumentArity.ZeroOrOne)
                }.WithHandler<RuntimeHandlers>(nameof(RuntimeHandlers.HandleValidate))
            };
            var executeCommands = new Command(ExecuteCommandName, "Executing automation on patterns from toolkits")
            {
                new Command("command", "Executes the launch point on the draft")
                {
                    new Argument("Name", "The name of the launch point to execute"),
                    new Option("--on",
                        "The expression of the element/collection containing the launch point to execute",
                        arity: ArgumentArity.ZeroOrOne)
                }.WithHandler<RuntimeHandlers>(nameof(RuntimeHandlers.HandleExecuteCommand))
            };
            var viewCommands = new Command(ViewCommandName, "Viewing patterns and drafts")
            {
                new Command("pattern", "View the configuration of the current pattern")
                {
                    new Option("--all", "Include additional configuration, like automation and code templates",
                        typeof(bool), () => false, ArgumentArity.ZeroOrOne)
                }.WithHandler<AuthoringHandlers>(nameof(AuthoringHandlers.HandleViewPattern)),
                new Command("toolkit", "View the configuration of the current toolkit")
                {
                    new Option("--all", "Include additional configuration, like automation and code templates",
                        typeof(bool), () => false, ArgumentArity.ZeroOrOne)
                }.WithHandler<RuntimeHandlers>(nameof(RuntimeHandlers.HandleViewToolkit)),
                new Command("draft", "View the configuration of the current draft")
                {
                    new Option("--todo", "Displays the details of the pattern, and any validation errors", typeof(bool),
                        () => false, ArgumentArity.ZeroOrOne)
                }.WithHandler<RuntimeHandlers>(nameof(RuntimeHandlers.HandleViewDraft))
            };
            var listCommands = new Command(ListCommandName, "Listing patterns, toolkits and drafts")
            {
                new Command("patterns", "Lists all patterns being edited")
                    .WithHandler<AuthoringHandlers>(nameof(AuthoringHandlers.HandleListPatterns)),
                new Command("toolkits", "Lists all installed toolkits")
                    .WithHandler<RuntimeHandlers>(nameof(RuntimeHandlers.HandleListToolkits)),
                new Command("drafts", "Lists all drafts being configured")
                    .WithHandler<RuntimeHandlers>(nameof(RuntimeHandlers.HandleListDrafts))
            };
            var upgradeCommands = new Command(UpgradeCommandName, "Upgrading toolkits and drafts")
            {
                new Command("draft", "Upgrades a draft from a new version of its toolkit")
                    {
                        new Option("--force", "Force the upgrade despite any compatability errors")
                    }
                    .WithHandler<RuntimeHandlers>(nameof(RuntimeHandlers.HandleDraftUpgrade))
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
                        OutputMessages.CommandLine_Output_CurrentPatternInUse.FormatTemplate(
                            Authoring.CurrentPatternName,
                            Authoring.CurrentPatternVersion), ConsoleColor.Gray);
                }
                else
                {
                    ConsoleExtensions.WriteErrorWarning(OutputMessages.CommandLine_Output_NoPatternSelected);
                }
            }
            if (IsRuntimeCommand(args))
            {
                if (IsRuntimeDraftCommand(args))
                {
                    if (Runtime.CurrentDraftId.Exists())
                    {
                        ConsoleExtensions.WriteOutput(
                            OutputMessages.CommandLine_Output_CurrentDraftInUse.FormatTemplate(
                                Runtime.CurrentDraftName, Runtime.CurrentDraftId), ConsoleColor.Gray);
                    }
                    else
                    {
                        ConsoleExtensions.WriteErrorWarning(OutputMessages.CommandLine_Output_NoDraftSelected);
                    }
                }
            }

            var parser = new CommandLineBuilder(command)
                .UseDefaults()
                .UseExceptionHandler((ex, context) =>
                {
                    var isDebug = IsDebugging(context, ex);

                    var message = ex.InnerException.Exists()
                        ? isDebug
                            ? ex.InnerException.ToString()
                            : ex.InnerException.Message
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

        private static bool IsDebugging(

            // ReSharper disable once UnusedParameter.Local
            InvocationContext context,

            // ReSharper disable once UnusedParameter.Local
            Exception ex)
        {
#if TESTINGONLY
            return true;
#else
            var debugOption =
 context.Parser.Configuration.RootCommand.Options.FirstOrDefault(opt => opt.Name == "debug");
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

            var isViewDraftCommand = args[0] == ViewCommandName && args.Count == 2 && args[1] == "draft";
            var isListToolkitsCommand = args[0] == ListCommandName && args.Count == 2 && args[1] == "toolkits";
            var isListDraftsCommand = args[0] == ListCommandName && args.Count == 2 && args[1] == "drafts";

            return args[0] == InstallCommandName || args[0] == RunCommandName || args[0] == ConfigureCommandName
                   || args[0] == ValidateCommandName || args[0] == ExecuteCommandName
                   || isViewDraftCommand || isListToolkitsCommand || isListDraftsCommand;
        }

        private static bool IsRuntimeDraftCommand(IReadOnlyList<string> args)
        {
            if (args.Count < 1)
            {
                return false;
            }

            var isListToolkitsCommand = args[0] == ListCommandName && args.Count == 2 && args[1] == "toolkits";
            var isListDraftsCommand = args[0] == ListCommandName && args.Count == 2 && args[1] == "drafts";

            return args[0] != InstallCommandName && args[0] != RunCommandName && !isListToolkitsCommand &&
                   !isListDraftsCommand;
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
            internal static void HandleFail(

                // ReSharper disable once UnusedParameter.Local
                bool outputStructured,

                // ReSharper disable once UnusedParameter.Local
                IConsole console)
            {
                throw new Exception("testingonly");
            }
        }

        private class AuthoringHandlers
        {
            private static readonly IPersistableFactory PersistenceFactory = new AutomatePersistableFactory();

            internal static void HandleBuild(string asversion, bool force, bool outputStructured, IConsole console)
            {
                var package = Authoring.BuildAndExportToolkit(asversion, force);
                console.WriteOutput(outputStructured, OutputMessages.CommandLine_Output_BuiltToolkit,
                    package.Toolkit.PatternName, package.Toolkit.Version, package.ExportedLocation);
                if (package.Message.HasValue())
                {
                    console.WriteOutputWarning(outputStructured, OutputMessages.CommandLine_Output_BuiltToolkit_Warning,
                        package.Message);
                }
            }

            internal static void HandleAddCodeTemplateCommand(string codeTemplateName, string name, bool isOneOff,
                string targetPath, string asChildOf, bool outputStructured, IConsole console)
            {
                var (parent, command) =
                    Authoring.AddCodeTemplateCommand(codeTemplateName, name, isOneOff, targetPath, asChildOf);
                console.WriteOutput(outputStructured, OutputMessages.CommandLine_Output_CodeTemplateCommandAdded,
                    command.Name, command.Id, parent.Id);
            }

            internal static void HandleUpdateCodeTemplateCommand(string commandName, string name, bool? isOneOff,
                string targetPath, string asChildOf, bool outputStructured, IConsole console)
            {
                var (parent, command) =
                    Authoring.UpdateCodeTemplateCommand(commandName, name, isOneOff, targetPath, asChildOf);
                console.WriteOutput(outputStructured, OutputMessages.CommandLine_Output_CodeTemplateCommandUpdated,
                    command.Name, command.Id, parent.Id, command.Metadata[nameof(CodeTemplateCommand.FilePath)],
                    command.Metadata[nameof(CodeTemplateCommand.IsOneOff)]);
            }

            internal static void HandleAddCliCommand(string applicationName, string arguments, string name,
                string asChildOf, bool outputStructured, IConsole console)
            {
                var (parent, command) = Authoring.AddCliCommand(applicationName, arguments, name, asChildOf);
                console.WriteOutput(outputStructured, OutputMessages.CommandLine_Output_CliCommandAdded,
                    command.Name, command.Id, parent.Id);
            }

            internal static void HandleUpdateCliCommand(string commandName, string app, string arguments,
                string name, string asChildOf, bool outputStructured, IConsole console)
            {
                var (parent, command) = Authoring.UpdateCliCommand(commandName, name, app, arguments, asChildOf);
                console.WriteOutput(outputStructured, OutputMessages.CommandLine_Output_CliCommandUpdated,
                    command.Name, command.Id, parent.Id, command.Metadata[nameof(CliCommand.ApplicationName)],
                    command.Metadata[nameof(CliCommand.Arguments)]);
            }

            internal static void HandleDeleteCommand(string commandName, string asChildOf, bool outputStructured,
                IConsole console)
            {
                var (parent, command) = Authoring.DeleteCommand(commandName, asChildOf);
                console.WriteOutput(outputStructured, OutputMessages.CommandLine_Output_CommandDeleted,
                    command.Name, command.Id, parent.Id);
            }

            internal static void HandleAddCommandLaunchPoint(string commandIdentifiers, string name, string asChildOf,
                bool outputStructured, IConsole console)
            {
                var cmdIds = commandIdentifiers.SafeSplit(CommandLaunchPoint.CommandIdDelimiter,
                    StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).ToList();
                var (parent, launchPoint) = Authoring.AddCommandLaunchPoint(name, cmdIds, asChildOf);
                console.WriteOutput(outputStructured, OutputMessages.CommandLine_Output_LaunchPointAdded,
                    launchPoint.Name, launchPoint.Id, parent.Id,
                    launchPoint.Metadata[nameof(CommandLaunchPoint.CommandIds)]);
            }

            internal static void HandleUpdateLaunchPoint(string launchPointName, string name, string add,
                string from, string asChildOf,
                bool outputStructured, IConsole console)
            {
                var cmdIds = add.SafeSplit(CommandLaunchPoint.CommandIdDelimiter,
                    StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).ToList();
                var (parent, launchPoint) =
                    Authoring.UpdateCommandLaunchPoint(launchPointName, name, cmdIds, from, asChildOf);
                console.WriteOutput(outputStructured, OutputMessages.CommandLine_Output_LaunchPointUpdated,
                    launchPoint.Name, launchPoint.Id, parent.Id,
                    launchPoint.Metadata[nameof(CommandLaunchPoint.CommandIds)]);
            }

            internal static void HandleDeleteLaunchPoint(string launchPointName, string asChildOf,
                bool outputStructured, IConsole console)
            {
                var (parent, launchPoint) = Authoring.DeleteCommandLaunchPoint(launchPointName, asChildOf);
                console.WriteOutput(outputStructured, OutputMessages.CommandLine_Output_LaunchPointDeleted,
                    launchPoint.Name, launchPoint.Id, parent.Id);
            }

            internal static void HandleAddElement(string name, bool? autoCreate, string displayedAs, string describedAs,
                string asChildOf,
                bool isRequired, bool outputStructured, IConsole console)
            {
                var (parent, element) = Authoring.AddElement(name,
                    isRequired
                        ? ElementCardinality.One
                        : ElementCardinality.ZeroOrOne, autoCreate ?? isRequired, displayedAs, describedAs, asChildOf);
                console.WriteOutput(outputStructured, OutputMessages.CommandLine_Output_ElementAdded, name, element.Id,
                    parent.Id);
            }

            internal static void HandleUpdateElement(string elementName, string name, bool? autoCreate,
                string displayedAs, string describedAs, string asChildOf, bool? isRequired, bool outputStructured,
                IConsole console)
            {
                var (parent, element) = Authoring.UpdateElement(elementName, name,
                    isRequired, autoCreate, displayedAs, describedAs, asChildOf);
                console.WriteOutput(outputStructured, OutputMessages.CommandLine_Output_ElementUpdated, element.Name,
                    element.Id, parent.Id);
            }

            internal static void HandleAddCollection(string name, bool? autoCreate, string displayedAs,
                string describedAs,
                string asChildOf, bool isRequired, bool outputStructured, IConsole console)
            {
                var (parent, collection) = Authoring.AddElement(name, isRequired
                    ? ElementCardinality.OneOrMany
                    : ElementCardinality.ZeroOrMany, autoCreate ?? isRequired, displayedAs, describedAs, asChildOf);
                console.WriteOutput(outputStructured, OutputMessages.CommandLine_Output_CollectionAdded, name,
                    collection.Id, parent.Id);
            }

            internal static void HandleUpdateCollection(string collectionName, string name, bool? autoCreate,
                string displayedAs, string describedAs, string asChildOf, bool? isRequired, bool outputStructured,
                IConsole console)
            {
                var (parent, element) = Authoring.UpdateElement(collectionName, name,
                    isRequired, autoCreate, displayedAs, describedAs, asChildOf);
                console.WriteOutput(outputStructured, OutputMessages.CommandLine_Output_CollectionUpdated, element.Name,
                    element.Id, parent.Id);
            }

            internal static void HandleAddAttribute(string name, string isOfType, string defaultValueIs,
                bool isRequired, string isOneOf, string asChildOf, bool outputStructured, IConsole console)
            {
                var choices = isOneOf.SafeSplit(";").ToList();
                var (parent, attribute) =
                    Authoring.AddAttribute(name, isOfType, defaultValueIs, isRequired, choices, asChildOf);
                console.WriteOutput(outputStructured, OutputMessages.CommandLine_Output_AttributeAdded, name,
                    attribute.Id, parent.Id);
            }

            internal static void HandleUpdatePattern(string name, string displayedAs, string describedAs,
                bool outputStructured, IConsole console)
            {
                var pattern = Authoring.UpdatePattern(name, displayedAs, describedAs);
                console.WriteOutput(outputStructured, OutputMessages.CommandLine_Output_PatternUpdated, pattern.Name);
            }

            internal static void HandleUpdateAttribute(string attributeName, string name, string isOfType,
                string defaultValueIs, bool? isRequired, string isOneOf, string asChildOf, bool outputStructured,
                IConsole console)
            {
                var choices = isOneOf.SafeSplit(";").ToList();
                var (parent, attribute) =
                    Authoring.UpdateAttribute(attributeName, name, isOfType, defaultValueIs, isRequired, choices,
                        asChildOf);
                console.WriteOutput(outputStructured, OutputMessages.CommandLine_Output_AttributeUpdated,
                    attribute.Name, attribute.Id, parent.Id);
            }

            internal static void HandleDeleteAttribute(string name, string asChildOf, bool outputStructured,
                IConsole console)
            {
                var (parent, attribute) =
                    Authoring.DeleteAttribute(name, asChildOf);
                console.WriteOutput(outputStructured, OutputMessages.CommandLine_Output_AttributeDeleted, name,
                    attribute.Id, parent.Id);
            }

            internal static void HandleDeleteElement(string name, string asChildOf, bool outputStructured,
                IConsole console)
            {
                var (parent, element) =
                    Authoring.DeleteElement(name, asChildOf);
                console.WriteOutput(outputStructured, OutputMessages.CommandLine_Output_ElementDeleted, name,
                    element.Id, parent.Id);
            }

            internal static void HandleDeleteCollection(string name, string asChildOf, bool outputStructured,
                IConsole console)
            {
                var (parent, element) =
                    Authoring.DeleteElement(name, asChildOf);
                console.WriteOutput(outputStructured, OutputMessages.CommandLine_Output_CollectionDeleted, name,
                    element.Id, parent.Id);
            }

            internal static void HandleCreatePattern(string name, string displayedAs,
                string describedAs, bool outputStructured, IConsole console)
            {
                Authoring.CreateNewPattern(name, displayedAs, describedAs);
                console.WriteOutput(outputStructured,
                    OutputMessages.CommandLine_Output_PatternCreated, name, Authoring.CurrentPatternId);
            }

            internal static void HandleViewPattern(bool all, bool outputStructured, IConsole console)
            {
                var pattern = Authoring.GetCurrentPattern();

                console.WriteOutput(outputStructured,
                    OutputMessages.CommandLine_Output_PatternConfiguration, pattern.Name, pattern.Id,
                    pattern.ToolkitVersion.Current,
                    FormatPatternConfiguration(outputStructured, pattern, all));
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
                var (parent, template) = Authoring.AttachCodeTemplate(currentDirectory, filepath, name, asChildOf);
                console.WriteOutput(outputStructured,
                    OutputMessages.CommandLine_Output_CodeTemplatedAdded, template.Template.Name, template.Template.Id,
                    parent.Id, template.Template.Metadata.OriginalFilePath, template.Location);
            }

            internal static void HandleEditCodeTemplate(string templateName, string with, string args, string asChildOf,
                bool outputStructured, IConsole console)
            {
                var (parent, template, location) = Authoring.EditCodeTemplate(templateName, with, args, asChildOf);
                console.WriteOutput(outputStructured,
                    OutputMessages.CommandLine_Output_CodeTemplatedEdited, template.Name, template.Id,
                    parent.Id, with, location);
            }

            internal static void HandleDeleteCodeTemplate(string templateName, string asChildOf,
                bool outputStructured, IConsole console)
            {
                var (parent, template) = Authoring.DeleteCodeTemplate(templateName, asChildOf);
                console.WriteOutput(outputStructured,
                    OutputMessages.CommandLine_Output_CodeTemplateDeleted, template.Name, template.Id, parent.Id);
            }

            internal static void HandleTestCodeTemplate(string templateName, string asChildOf, string importData,
                string exportData, bool outputStructured, IConsole console)
            {
                var currentDirectory = Environment.CurrentDirectory;
                var test =
                    Authoring.TestCodeTemplate(templateName, asChildOf, currentDirectory, importData, exportData);
                if (exportData.HasValue())
                {
                    console.WriteOutput(outputStructured, OutputMessages.CommandLine_Output_CodeTemplateTestExported,
                        templateName, test.Template.Id, test.ExportedFilePath);
                    console.WriteOutputLine();
                }

                if (importData.HasValue())
                {
                    console.WriteOutput(outputStructured, OutputMessages.CommandLine_Output_CodeTemplateTestImported,
                        templateName, test.Template.Id, importData);
                    console.WriteOutputLine();
                }

                console.WriteOutput(outputStructured,
                    OutputMessages.CommandLine_Output_TextTemplatingExpressionReference);
                console.WriteOutput(outputStructured, OutputMessages.CommandLine_Output_CodeTemplateTested,
                    templateName,
                    test.Template.Id, test.Output);
            }

            internal static void HandleTestCodeTemplateCommand(string commandName, string asChildOf, string importData,
                string exportData, bool outputStructured, IConsole console)
            {
                var currentDirectory = Environment.CurrentDirectory;
                var test = Authoring.TestCodeTemplateCommand(commandName, asChildOf, currentDirectory, importData,
                    exportData);
                if (exportData.HasValue())
                {
                    console.WriteOutput(outputStructured,
                        OutputMessages.CommandLine_Output_CodeTemplateCommandTestExported,
                        commandName, test.Command.Id, test.ExportedFilePath);
                    console.WriteOutputLine();
                }

                if (importData.HasValue())
                {
                    console.WriteOutput(outputStructured,
                        OutputMessages.CommandLine_Output_CodeTemplateCommandTestImported,
                        commandName, test.Command.Id, importData);
                    console.WriteOutputLine();
                }

                console.WriteOutput(outputStructured,
                    OutputMessages.CommandLine_Output_TextTemplatingExpressionReference);
                console.WriteOutput(outputStructured, OutputMessages.CommandLine_Output_CodeTemplateCommandTested,
                    commandName,
                    test.Command.Id, test.Output);
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

            internal static string FormatPatternConfiguration(bool outputStructured, PatternDefinition pattern,
                bool isDetailed)
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
            internal static void HandleDraftUpgrade(bool force, bool outputStructured, IConsole console)
            {
                var upgrade = Runtime.UpgradeDraft(force);
                if (upgrade.IsSuccess)
                {
                    if (upgrade.Log.Any(entry => entry.Type == MigrationChangeType.Abort))
                    {
                        console.WriteOutputWarning(outputStructured,
                            OutputMessages.CommandLine_Output_DraftUpgradeWithWarning,
                            upgrade.Draft.Name, upgrade.Draft.Id, upgrade.Draft.PatternName,
                            upgrade.FromVersion, upgrade.ToVersion, FormatUpgradeLog(upgrade.Log));
                    }
                    else
                    {
                        console.WriteOutput(outputStructured,
                            OutputMessages.CommandLine_Output_DraftUpgradeSucceeded,
                            upgrade.Draft.Name, upgrade.Draft.Id, upgrade.Draft.PatternName,
                            upgrade.FromVersion, upgrade.ToVersion, FormatUpgradeLog(upgrade.Log));
                    }
                }
                else
                {
                    console.WriteError(outputStructured, OutputMessages.CommandLine_Output_DraftUpgradeFailed,
                        upgrade.Draft.Name, upgrade.Draft.Id, upgrade.Draft.PatternName, upgrade.FromVersion,
                        upgrade.ToVersion, FormatUpgradeLog(upgrade.Log));
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

            internal static void HandleNewDraft(string patternName, string name, bool outputStructured,
                IConsole console)
            {
                var draft = Runtime.CreateDraft(patternName, name);
                console.WriteOutput(outputStructured, OutputMessages.CommandLine_Output_CreateDraftFromToolkit,
                    draft.Name, draft.Id, draft.PatternName);
            }

            internal static void HandleListDrafts(bool outputStructured, IConsole console)
            {
                var drafts = Runtime.ListCreatedDrafts();
                if (drafts.Any())
                {
                    console.WriteOutput(outputStructured, OutputMessages.CommandLine_Output_InstalledDraftsListed,
                        drafts.ToMultiLineText(draft =>
                            $"{{\"Name\": \"{draft.Name}\", \"ID\": \"{draft.Id}\", \"Version\": \"{draft.Toolkit.Version}\"}}"));
                }
                else
                {
                    console.WriteOutput(outputStructured, OutputMessages.CommandLine_Output_NoInstalledDrafts);
                }
            }

            internal static void HandleSwitch(string draftId, bool outputStructured, IConsole console)
            {
                Runtime.SwitchCurrentDraft(draftId);
                console.WriteOutput(outputStructured,
                    OutputMessages.CommandLine_Output_DraftSwitched, Runtime.CurrentDraftName,
                    Runtime.CurrentDraftId);
            }

            internal static void HandleConfigureAddTo(string expression, string[] andSet, bool outputStructured,
                IConsole console)
            {
                var sets = new List<string>();
                if (andSet.HasAny())
                {
                    sets.AddRange(andSet);
                }

                var nameValues = sets
                    .Select(set => set.SplitPropertyAssignment())
                    .ToDictionary(pair => pair.Name, pair => pair.Value);

                var draftItem = Runtime.ConfigureDraft(expression, null, null, nameValues);
                console.WriteOutput(outputStructured, OutputMessages.CommandLine_Output_DraftConfigured,
                    draftItem.Name, draftItem.Id);
            }

            internal static void HandleConfigureAddOneTo(string expression, string[] andSet, bool outputStructured,
                IConsole console)
            {
                var sets = new List<string>();
                if (andSet.HasAny())
                {
                    sets.AddRange(andSet);
                }
                var nameValues = sets
                    .Select(set => set.SplitPropertyAssignment())
                    .ToDictionary(pair => pair.Name, pair => pair.Value);

                var draftItem = Runtime.ConfigureDraft(null, expression, null, nameValues);
                console.WriteOutput(outputStructured, OutputMessages.CommandLine_Output_DraftConfigured,
                    draftItem.Name, draftItem.Id);
            }

            internal static void HandleConfigureOn(string expression, string[] andSet, bool outputStructured,
                IConsole console)
            {
                var sets = new List<string>();
                if (andSet.HasAny())
                {
                    sets.AddRange(andSet);
                }
                var nameValues = sets
                    .Select(set => set.SplitPropertyAssignment())
                    .ToDictionary(pair => pair.Name, pair => pair.Value);

                var draftItem = Runtime.ConfigureDraft(null, null, expression, nameValues);
                console.WriteOutput(outputStructured, OutputMessages.CommandLine_Output_DraftConfigured,
                    draftItem.Name, draftItem.Id);
            }

            internal static void HandleConfigureResetElement(string expression, bool outputStructured, IConsole console)
            {
                var draftItem = Runtime.ConfigureDraftAndResetElement(expression);
                console.WriteOutput(outputStructured, OutputMessages.CommandLine_Output_DraftResetElement,
                    draftItem.Name, draftItem.Id);
            }

            internal static void HandleConfigureClearCollection(string expression, bool outputStructured,
                IConsole console)
            {
                var draftItem = Runtime.ConfigureDraftAndClearCollection(expression);
                console.WriteOutput(outputStructured, OutputMessages.CommandLine_Output_DraftEmptyCollection,
                    draftItem.Name, draftItem.Id);
            }

            internal static void HandleConfigureDelete(string expression, bool outputStructured, IConsole console)
            {
                var draftItem = Runtime.ConfigureDraftAndDelete(expression);
                console.WriteOutput(outputStructured, OutputMessages.CommandLine_Output_DraftDelete,
                    draftItem.Name, draftItem.Id);
            }

            internal static void HandleViewDraft(bool todo, bool outputStructured, IConsole console)
            {
                var (configuration, pattern, validation) = Runtime.GetDraftConfiguration(todo, todo);

                var draftId = Runtime.CurrentDraftId;
                var draftName = Runtime.CurrentDraftName;
                console.WriteOutput(outputStructured, OutputMessages.CommandLine_Output_DraftConfiguration,
                    draftName, draftId, configuration);

                if (todo)
                {
                    console.WriteOutputLine();
                    console.WriteOutput(outputStructured,
                        OutputMessages.CommandLine_Output_PatternConfiguration, pattern.Name, pattern.Id,
                        pattern.ToolkitVersion.Current,
                        AuthoringHandlers.FormatPatternConfiguration(outputStructured, pattern, true));
                }

                if (todo)
                {
                    console.WriteOutputLine();
                    if (validation.HasAny())
                    {
                        console.WriteOutputWarning(outputStructured,
                            OutputMessages.CommandLine_Output_DraftValidationFailed,
                            draftName, draftId, FormatValidationErrors(validation));
                    }
                    else
                    {
                        console.WriteOutput(outputStructured,
                            OutputMessages.CommandLine_Output_DraftValidationSuccess, draftName, draftId);
                    }
                }
            }

            internal static void HandleViewToolkit(bool all, bool outputStructured, IConsole console)
            {
                var pattern = Runtime.ViewCurrentToolkit().Pattern;

                console.WriteOutput(outputStructured,
                    OutputMessages.CommandLine_Output_ToolkitConfiguration, pattern.Name, pattern.Id,
                    pattern.ToolkitVersion.Current,
                    AuthoringHandlers.FormatPatternConfiguration(outputStructured, pattern, all));
            }

            internal static void HandleValidate(string on,
                bool outputStructured, IConsole console)
            {
                var results = Runtime.Validate(on);

                var draftId = Runtime.CurrentDraftId;
                var draftName = Runtime.CurrentDraftName;
                if (results.HasAny())
                {
                    console.WriteOutputWarning(outputStructured,
                        OutputMessages.CommandLine_Output_DraftValidationFailed,
                        draftName, draftId, FormatValidationErrors(results));
                }
                else
                {
                    console.WriteOutput(outputStructured, OutputMessages.CommandLine_Output_DraftValidationSuccess,
                        draftName, draftId);
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
                        console.WriteOutputWarning(outputStructured,
                            OutputMessages.CommandLine_Output_DraftValidationFailed,
                            Runtime.CurrentDraftName, Runtime.CurrentDraftId,
                            FormatValidationErrors(execution.ValidationErrors));
                    }
                    else
                    {
                        console.WriteOutputWarning(outputStructured,
                            OutputMessages.CommandLine_Output_CommandExecutionFailed,
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
                    .ForEach(item =>
                    {
                        builder.AppendLine(
                            $"* {item.Type}: {item.MessageTemplate.FormatTemplate(item.Arguments.ToArray())}");
                    });

                return builder.ToString();
            }
        }
    }

    internal static class CommandLineExtensions
    {
        public const string Delimiter = "=";

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
                    .CommandLineApiExtensions_SplitPropertyAssignment_ValueWithoutName.Format(expression));
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