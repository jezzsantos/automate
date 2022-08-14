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
using Automate.CLI.Extensions;
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
        public const string CreateCommandName = "create";
        public const string EditCommandName = "edit";
        public const string BuildCommandName = "build";
        public const string PublishCommandName = "publish";
        public const string InstallCommandName = "install";
        public const string RunCommandName = "run";
        public const string TestCommandName = "test";
        public const string ListCommandName = "list";
        public const string ConfigureCommandName = "configure";
        public const string ValidateCommandName = "validate";
        public const string ExecuteCommandName = "execute";
        public const string ViewCommandName = "view";
        public const string UpgradeCommandName = "upgrade";
        public const string TestingOnlyCommandName = "testingonly";
        private const string DebugOption = "debug";
        private const string StructuredOutputOption = "output-structured";
        private const string PatternSubCommandName = "pattern";
        private const string DraftSubCommandName = "draft";
        private const string DraftsSubCommandName = "drafts";
        private const string ToolkitSubCommandName = "toolkit";
        private const string ToolkitsSubCommandName = "toolkits";
        private const string PatternsSubCommandName = "patterns";
        private static AuthoringApplication authoring;
        private static RuntimeApplication runtime;

        public static int Execute(IDependencyContainer container, string[] args)
        {
            authoring = CreateAuthoringApplication(container);
            runtime = CreateRuntimeApplication(container);

            var outputMessages = new List<OutputMessage>();
            var contextualMessages = new List<ContextualMessage>();
            HandlerBase.Initialise(outputMessages);

            var createCommands = new Command(CreateCommandName, "Creating new patterns")
            {
                new Command(PatternSubCommandName, "Creates a new pattern")
                {
                    new Argument("Name", "The name of the pattern to create"),
                    new Option("--displayedas", "A friendly display name for the pattern", typeof(string),
                        arity: ArgumentArity.ZeroOrOne),
                    new Option("--describedas", "A description for the pattern", typeof(string),
                        arity: ArgumentArity.ZeroOrOne)
                }.WithHandler<AuthoringHandlers>(nameof(AuthoringHandlers.CreatePattern))
            };
            var editCommands = new Command(EditCommandName, "Editing patterns")
            {
                new Command("switch", "Switches to configuring another pattern")
                {
                    new Argument("Name", "The name of the existing pattern to edit")
                }.WithHandler<AuthoringHandlers>(nameof(AuthoringHandlers.SwitchPattern)),
                new Command("update-pattern", "Updates the pattern")
                {
                    new Option("--name", "A new name for the pattern", typeof(string),
                        arity: ArgumentArity.ZeroOrOne),
                    new Option("--displayedas", "A new friendly display name for the pattern", typeof(string),
                        arity: ArgumentArity.ZeroOrOne),
                    new Option("--describedas", "A new description for the pattern", typeof(string),
                        arity: ArgumentArity.ZeroOrOne)
                }.WithHandler<AuthoringHandlers>(nameof(AuthoringHandlers.UpdatePattern)),
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
                }.WithHandler<AuthoringHandlers>(nameof(AuthoringHandlers.AddAttribute)),
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
                }.WithHandler<AuthoringHandlers>(nameof(AuthoringHandlers.UpdateAttribute)),
                new Command("delete-attribute", "Deletes an attribute from an element/collection in the pattern")
                {
                    new Argument("Name", "The name of the attribute"),
                    new Option("--aschildof", "The expression of the element/collection to delete the attribute from",
                        typeof(string), arity: ArgumentArity.ZeroOrOne)
                }.WithHandler<AuthoringHandlers>(nameof(AuthoringHandlers.DeleteAttribute)),
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
                }.WithHandler<AuthoringHandlers>(nameof(AuthoringHandlers.AddElement)),
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
                }.WithHandler<AuthoringHandlers>(nameof(AuthoringHandlers.UpdateElement)),
                new Command("delete-element", "Deletes an element from an element/collection in the pattern")
                {
                    new Argument("Name", "The name of the element"),
                    new Option("--aschildof", "The expression of the element/collection to delete the element from",
                        typeof(string), arity: ArgumentArity.ZeroOrOne)
                }.WithHandler<AuthoringHandlers>(nameof(AuthoringHandlers.DeleteElement)),
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
                }.WithHandler<AuthoringHandlers>(nameof(AuthoringHandlers.AddCollection)),
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
                }.WithHandler<AuthoringHandlers>(nameof(AuthoringHandlers.UpdateCollection)),
                new Command("delete-collection", "Deletes a collection from an element/collection in the pattern")
                {
                    new Argument("Name", "The name of the collection"),
                    new Option("--aschildof", "The expression of the element/collection to delete the collection from",
                        typeof(string), arity: ArgumentArity.ZeroOrOne)
                }.WithHandler<AuthoringHandlers>(nameof(AuthoringHandlers.DeleteCollection)),
                new Command("add-codetemplate", "Adds a code template to an element")
                {
                    new Argument("FilePath", "A relative path to the code file, from the current directory"),
                    new Option("--name", "A friendly name for the code template",
                        arity: ArgumentArity.ZeroOrOne),
                    new Option("--aschildof", "The expression of the element/collection to add the code template to",
                        typeof(string), arity: ArgumentArity.ZeroOrOne)
                }.WithHandler<AuthoringHandlers>(nameof(AuthoringHandlers.AddCodeTemplate)),
                new Command("add-codetemplate-with-command",
                    "Adds a code template to an element, with a command to render it")
                {
                    new Argument("FilePath", "A relative path to the code file, from the current directory"),
                    new Option("--targetpath", "The full path of the generated file, with filename.", typeof(string),
                        arity: ArgumentArity.ExactlyOne),
                    new Option("--isoneoff",
                        "Only generate the file the first time if it does not already exist",
                        typeof(bool), () => false, ArgumentArity.ZeroOrOne),
                    new Option("--name", "A friendly name for the code template",
                        arity: ArgumentArity.ZeroOrOne),
                    new Option("--aschildof", "The expression of the element/collection to add the code template to",
                        typeof(string), arity: ArgumentArity.ZeroOrOne)
                }.WithHandler<AuthoringHandlers>(nameof(AuthoringHandlers.AddCodeTemplateWithCommand)),
                new Command("codetemplate", "Edits a code template in an editor")
                {
                    new Argument("TemplateName", "The name of the code template"),
                    new Option("--with", "Name of or full path to an application to edit the code template",
                        typeof(string), arity: ArgumentArity.ExactlyOne),
                    new Option("--args", "An program arguments",
                        typeof(string), arity: ArgumentArity.ZeroOrOne),
                    new Option("--aschildof", "The expression of the element/collection to edit the code template from",
                        typeof(string), arity: ArgumentArity.ZeroOrOne)
                }.WithHandler<AuthoringHandlers>(nameof(AuthoringHandlers.EditCodeTemplate)),
                new Command("delete-codetemplate", "Deletes a code template from an element/collection in the pattern")
                {
                    new Argument("TemplateName", "The name of the code template"),
                    new Option("--aschildof",
                        "The expression of the element/collection to delete the code template from",
                        typeof(string), arity: ArgumentArity.ZeroOrOne)
                }.WithHandler<AuthoringHandlers>(nameof(AuthoringHandlers.DeleteCodeTemplate)),
                new Command("add-codetemplate-command", "Adds a command that renders a code template")
                {
                    new Argument("CodeTemplateName", "The name of the code template"),
                    new Option("--name", "A name for the command", typeof(string),
                        arity: ArgumentArity.ZeroOrOne),
                    new Option("--targetpath", "The full path of the generated file, with filename.", typeof(string),
                        arity: ArgumentArity.ExactlyOne),
                    new Option("--isoneoff",
                        "Only generate the file the first time if it does not already exist",
                        typeof(bool), () => false, ArgumentArity.ZeroOrOne),
                    new Option("--aschildof", "The expression of the element/collection to add the command to",
                        typeof(string), arity: ArgumentArity.ZeroOrOne)
                }.WithHandler<AuthoringHandlers>(nameof(AuthoringHandlers.AddCodeTemplateCommand)),
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
                }.WithHandler<AuthoringHandlers>(nameof(AuthoringHandlers.UpdateCodeTemplateCommand)),
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
                    .WithHandler<AuthoringHandlers>(nameof(AuthoringHandlers.AddCliCommand)),
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
                }.WithHandler<AuthoringHandlers>(nameof(AuthoringHandlers.UpdateCliCommand)),
                new Command("delete-command", "Deletes any command from an element/collection in the pattern")
                {
                    new Argument("CommandName", "The name of the command"),
                    new Option("--aschildof", "The expression of the element/collection to delete the command from",
                        typeof(string), arity: ArgumentArity.ZeroOrOne)
                }.WithHandler<AuthoringHandlers>(nameof(AuthoringHandlers.DeleteCommand)),
                new Command("add-command-launchpoint", "Adds a launch point for a command")
                {
                    new Argument("CommandIdentifiers",
                        "A semi-colon delimited list of identifiers of the commands to launch (from anywhere in the pattern), or '*' to launch all commands found on the element/collection"),
                    new Option("--name", "A name for the launch point", typeof(string),
                        arity: ArgumentArity.ZeroOrOne),
                    new Option("--aschildof", "The expression of the element/collection to add the launch point to",
                        typeof(string), arity: ArgumentArity.ZeroOrOne),
                    new Option("--from", "The expression of the element/collection to add commands from",
                        typeof(string), arity: ArgumentArity.ZeroOrOne)
                }.WithHandler<AuthoringHandlers>(nameof(AuthoringHandlers.AddCommandLaunchPoint)),
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
                }.WithHandler<AuthoringHandlers>(nameof(AuthoringHandlers.UpdateLaunchPoint)),
                new Command("delete-command-launchpoint",
                    "Deletes a launch point from an element/collection in the pattern")
                {
                    new Argument("LaunchPointName", "The name of the launch point"),
                    new Option("--aschildof",
                        "The expression of the element/collection to delete the launch point from",
                        typeof(string), arity: ArgumentArity.ZeroOrOne)
                }.WithHandler<AuthoringHandlers>(nameof(AuthoringHandlers.DeleteLaunchPoint))
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
                }.WithHandler<AuthoringHandlers>(nameof(AuthoringHandlers.TestCodeTemplate)),
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
                }.WithHandler<AuthoringHandlers>(nameof(AuthoringHandlers.TestCodeTemplateCommand))
            };
            var publishCommands = new Command(PublishCommandName, "Publishing toolkits from patterns")
            {
                new Command(ToolkitSubCommandName, "Builds a pattern into a toolkit and publishes the toolkit")
                    {
                        new Option("--asversion",
                            "A specific version number (1-2 dot number), or 'auto' to auto-increment the current version",
                            typeof(string), arity: ArgumentArity.ZeroOrOne),
                        new Option("--force",
                            "Force the specified version number even it it violates breaking changes checks",
                            typeof(bool), () => false, ArgumentArity.ZeroOrOne),
                        new Option("--install", "Install the built toolkit locally",
                            typeof(bool), () => false, ArgumentArity.ZeroOrOne)
                    }.WithHandler<AuthoringHandlers>(nameof(AuthoringHandlers.Publish))
                    .WithAlias(PatternSubCommandName)
            }.WithAlias(BuildCommandName);
            var installCommands = new Command(InstallCommandName, "Installing toolkits")
            {
                new Command(ToolkitSubCommandName, "Installs the pattern from a toolkit")
                {
                    new Argument("Location", "The location of the *.toolkit file to install into the current directory")
                }.WithHandler<RuntimeHandlers>(nameof(RuntimeHandlers.Install))
            };
            var runCommands = new Command(RunCommandName, "Running patterns from toolkits")
            {
                new Command(ToolkitSubCommandName, "Creates a new draft from a toolkit")
                {
                    new Argument("PatternName", "The name of the pattern in the toolkit that you want to use"),
                    new Option("--name", "A name for the draft", arity: ArgumentArity.ZeroOrOne)
                }.WithHandler<RuntimeHandlers>(nameof(RuntimeHandlers.NewDraft)),
                new Command("switch", "Switches to configuring another draft")
                {
                    new Argument("DraftId", "The id of the existing draft to configure")
                }.WithHandler<RuntimeHandlers>(nameof(RuntimeHandlers.SwitchDraft))
            };
            var configureCommands = new Command(ConfigureCommandName, "Configuring drafts to patterns from toolkits")
            {
                new Command("add", "Configure an element in the draft")
                {
                    new Argument("Expression", "The expression of the element to configure"),
                    new Option("--and-set", "A Name=Value pair of a property assignment",
                        arity: ArgumentArity.ZeroOrMore)
                }.WithHandler<RuntimeHandlers>(nameof(RuntimeHandlers.ConfigureDraftAddTo)),
                new Command("add-one-to", "Add a new item to a collection in the draft")
                {
                    new Argument("Expression", "The expression of the element/collection to add to"),
                    new Option("--and-set", "Additional Name=Value pair of a property assignment",
                        arity: ArgumentArity.ZeroOrMore)
                }.WithHandler<RuntimeHandlers>(nameof(RuntimeHandlers.ConfigureDraftAddOneTo)),
                new Command("on", "Set the properties of an existing item in the draft")
                {
                    new Argument("Expression", "The expression of the element/collection to assign to"),
                    new Option("--and-set", "Additional Name=Value pair of a property assignment",
                        arity: ArgumentArity.ZeroOrMore)
                }.WithHandler<RuntimeHandlers>(nameof(RuntimeHandlers.ConfigureDraftOn)),
                new Command("reset", "Reset all the properties of an existing item in the draft")
                {
                    new Argument("Expression", "The expression of the element to reset")
                }.WithHandler<RuntimeHandlers>(nameof(RuntimeHandlers.ConfigureDraftResetElement)),
                new Command("clear", "Deletes all the items from an existing collection in the draft")
                {
                    new Argument("Expression", "The expression of the collection to clear")
                }.WithHandler<RuntimeHandlers>(nameof(RuntimeHandlers.ConfigureDraftClearCollection)),
                new Command("delete", "Deletes an existing item in the draft")
                {
                    new Argument("Expression", "The expression of the element/collection to delete")
                }.WithHandler<RuntimeHandlers>(nameof(RuntimeHandlers.ConfigureDraftDeleteElement))
            };
            var validateCommands = new Command(ValidateCommandName, "Validating patterns from toolkits")
            {
                new Command(DraftSubCommandName, "Validate the current draft")
                {
                    new Option("--on", "The expression of the element/collection to validate",
                        arity: ArgumentArity.ZeroOrOne)
                }.WithHandler<RuntimeHandlers>(nameof(RuntimeHandlers.ValidateDraft))
            };
            var executeCommands = new Command(ExecuteCommandName, "Executing automation on patterns from toolkits")
            {
                new Command("command", "Executes the launch point on the draft")
                {
                    new Argument("Name", "The name of the launch point to execute"),
                    new Option("--on",
                        "The expression of the element/collection containing the launch point to execute",
                        arity: ArgumentArity.ZeroOrOne)
                }.WithHandler<RuntimeHandlers>(nameof(RuntimeHandlers.ExecuteLaunchPoint))
            };
            var viewCommands = new Command(ViewCommandName, "Viewing patterns and drafts")
            {
                new Command(PatternSubCommandName, "View the configuration of the current pattern")
                {
                    new Option("--all", "Include additional configuration, like automation and code templates",
                        typeof(bool), () => false, ArgumentArity.ZeroOrOne)
                }.WithHandler<AuthoringHandlers>(nameof(AuthoringHandlers.ViewPattern)),
                new Command(ToolkitSubCommandName, "View the configuration of the current toolkit")
                {
                    new Option("--all", "Include additional configuration, like automation and code templates",
                        typeof(bool), () => false, ArgumentArity.ZeroOrOne)
                }.WithHandler<RuntimeHandlers>(nameof(RuntimeHandlers.ViewToolkit)),
                new Command(DraftSubCommandName, "View the configuration of the current draft")
                {
                    new Option("--todo", "Displays the details of the pattern, and any validation errors", typeof(bool),
                        () => false, ArgumentArity.ZeroOrOne)
                }.WithHandler<RuntimeHandlers>(nameof(RuntimeHandlers.ViewDraft))
            };
            var listCommands = new Command(ListCommandName, "Listing patterns, toolkits and drafts")
            {
                new Command(PatternsSubCommandName, "Lists all patterns being edited")
                    .WithHandler<AuthoringHandlers>(nameof(AuthoringHandlers.ListPatterns)),
                new Command(ToolkitsSubCommandName, "Lists all installed toolkits")
                    .WithHandler<RuntimeHandlers>(nameof(RuntimeHandlers.ListToolkits)),
                new Command(DraftsSubCommandName, "Lists all drafts being configured")
                    .WithHandler<RuntimeHandlers>(nameof(RuntimeHandlers.ListDrafts))
            };
            var upgradeCommands = new Command(UpgradeCommandName, "Upgrading toolkits and drafts")
            {
                new Command(DraftSubCommandName, "Upgrades a draft from a new version of its toolkit")
                    {
                        new Option("--force", "Force the upgrade despite any compatability errors")
                    }
                    .WithHandler<RuntimeHandlers>(nameof(RuntimeHandlers.UpgradeDraft))
            };
#if TESTINGONLY
            var testingOnlyCommands = new Command(TestingOnlyCommandName, "For testing only!")
            {
                new Command("fail", "Forces the command to fail with an exception")
                {
                    new Option("--nested", "Whether to nest the exception", typeof(bool), () => false,
                        ArgumentArity.ZeroOrOne),
                    new Option("--message", "A message for the thrown exception", arity: ArgumentArity.ZeroOrOne)
                }.WithHandler<TestingOnlyHandlers>(nameof(TestingOnlyHandlers.Fail)),
                new Command("succeed", "Runs a command successfully")
                {
                    new Option("--message", "A message to output", arity: ArgumentArity.ZeroOrOne),
                    new Option("--value", "A value to substitute into the message", arity: ArgumentArity.ZeroOrOne)
                }.WithHandler<TestingOnlyHandlers>(nameof(TestingOnlyHandlers.Succeed))
            };
#endif
            var command =
                new RootCommand(
                    "Templatize patterns from your own codebase, make them programmable, then share them with your team")
                {
                    viewCommands,
                    listCommands,
                    createCommands,
                    editCommands,
                    testCommands,
                    publishCommands,
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
            command.AddGlobalOption(new Option($"--{StructuredOutputOption}", "Provide output as structured data",
                typeof(bool), () => false,
                ArgumentArity.ZeroOrOne));
            command.AddGlobalOption(new Option($"--{DebugOption}", "Show more error details when there is an exception",
                typeof(bool),
#if TESTINGONLY
                () => true,
#else
                () => false,
#endif
                ArgumentArity.ZeroOrOne));

            var parser = new CommandLineBuilder(command)
                .UseDefaults()
                .UseExceptionHandler((ex, context) => { HandleException(context, ex); })
                .UseHelp(context =>
                {
                    context.HelpBuilder.CustomizeLayout(_ =>
                    {
                        return HelpBuilder.Default.GetLayout()
                            .Prepend(_ => { WriteBanner(); });
                    });
                })
                .Build();

            var parseResult = parser.Parse(args);

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

            var result = parser.Invoke(args);

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
        }

        private static AuthoringApplication CreateAuthoringApplication(IDependencyContainer container)
        {
            return new AuthoringApplication(container.Resolve<IPatternStore>(),
                container.Resolve<IFilePathResolver>(), container.Resolve<IPatternPathResolver>(),
                container.Resolve<IPatternToolkitPackager>(), container.Resolve<ITextTemplatingEngine>(),
                container.Resolve<IApplicationExecutor>());
        }

        private static RuntimeApplication CreateRuntimeApplication(IDependencyContainer container)
        {
            return new RuntimeApplication(container.Resolve<IToolkitStore>(),
                container.Resolve<IDraftStore>(),
                container.Resolve<IFilePathResolver>(), container.Resolve<IPatternToolkitPackager>(),
                container.Resolve<IDraftPathResolver>(), container.Resolve<IAutomationExecutor>(),
                container.Resolve<IRuntimeMetadata>());
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

        private static bool IsOptionEnabled(ParseResult parsedCommandLine, string optionName)
        {
            var option = parsedCommandLine.Parser.Configuration.RootCommand.Options.FirstOrDefault(opt =>
                opt.Name == optionName);
            return option.Exists()
                   && (bool)parsedCommandLine.FindResultFor(option)!.GetValueOrDefault()!;
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
            Console.Error.WriteLine(message);
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