using System.CommandLine;
using Automate.CLI.Extensions;

namespace Automate.CLI.Infrastructure
{
    internal static partial class CommandLineApi
    {
        public const string InfoCommandName = "info";
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
        internal const string DeleteCommandName = "delete";
        public const string TestingOnlyCommandName = "testingonly";
        private const string DebugOption = "debug";
        private const string StructuredOutputOption = "output-structured";
        private const string PatternSubCommandName = "pattern";
        private const string DraftSubCommandName = "draft";
        private const string DraftsSubCommandName = "drafts";
        private const string ToolkitSubCommandName = "toolkit";
        private const string ToolkitsSubCommandName = "toolkits";
        private const string PatternsSubCommandName = "patterns";
        private const string CollectUsageEnabledOption = "collect-usage";
        private const string CollectUsageCorrelationOption = "usage-correlation";

        private static RootCommand DefineCommands()
        {
            var administrativeCommands =
                new Command(InfoCommandName, "Information about this CLI")
                    .AsHidden()
                    .WithHandler<AdministrativeApiHandlers>(nameof(AdministrativeApiHandlers.Info));
            var createCommands = new Command(CreateCommandName, "Creating new patterns")
            {
                new Command(PatternSubCommandName, "Creates a new pattern")
                {
                    new Argument("Name", "The name of the pattern to create"),
                    new Option("--displayedas", "A friendly display name for the pattern", typeof(string),
                        arity: ArgumentArity.ZeroOrOne),
                    new Option("--describedas", "A description for the pattern", typeof(string),
                        arity: ArgumentArity.ZeroOrOne)
                }.WithHandler<AuthoringApiHandlers>(nameof(AuthoringApiHandlers.CreatePattern))
            };
            var editCommands = new Command(EditCommandName, "Editing patterns")
            {
                new Command("switch", "Switches to configuring another pattern")
                {
                    new Argument("Id", "The ID of the existing pattern to edit")
                }.WithHandler<AuthoringApiHandlers>(nameof(AuthoringApiHandlers.SwitchPattern)),
                new Command("update-pattern", "Updates the pattern")
                {
                    new Option("--name", "A new name for the pattern", typeof(string),
                        arity: ArgumentArity.ZeroOrOne),
                    new Option("--displayedas", "A new friendly display name for the pattern", typeof(string),
                        arity: ArgumentArity.ZeroOrOne),
                    new Option("--describedas", "A new description for the pattern", typeof(string),
                        arity: ArgumentArity.ZeroOrOne)
                }.WithHandler<AuthoringApiHandlers>(nameof(AuthoringApiHandlers.UpdatePattern)),
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
                }.WithHandler<AuthoringApiHandlers>(nameof(AuthoringApiHandlers.AddAttribute)),
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
                }.WithHandler<AuthoringApiHandlers>(nameof(AuthoringApiHandlers.UpdateAttribute)),
                new Command("delete-attribute", "Deletes an attribute from an element/collection in the pattern")
                {
                    new Argument("Name", "The name of the attribute"),
                    new Option("--aschildof", "The expression of the element/collection to delete the attribute from",
                        typeof(string), arity: ArgumentArity.ZeroOrOne)
                }.WithHandler<AuthoringApiHandlers>(nameof(AuthoringApiHandlers.DeleteAttribute)),
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
                }.WithHandler<AuthoringApiHandlers>(nameof(AuthoringApiHandlers.AddElement)),
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
                }.WithHandler<AuthoringApiHandlers>(nameof(AuthoringApiHandlers.UpdateElement)),
                new Command("delete-element", "Deletes an element from an element/collection in the pattern")
                {
                    new Argument("Name", "The name of the element"),
                    new Option("--aschildof", "The expression of the element/collection to delete the element from",
                        typeof(string), arity: ArgumentArity.ZeroOrOne)
                }.WithHandler<AuthoringApiHandlers>(nameof(AuthoringApiHandlers.DeleteElement)),
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
                }.WithHandler<AuthoringApiHandlers>(nameof(AuthoringApiHandlers.AddCollection)),
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
                }.WithHandler<AuthoringApiHandlers>(nameof(AuthoringApiHandlers.UpdateCollection)),
                new Command("delete-collection", "Deletes a collection from an element/collection in the pattern")
                {
                    new Argument("Name", "The name of the collection"),
                    new Option("--aschildof", "The expression of the element/collection to delete the collection from",
                        typeof(string), arity: ArgumentArity.ZeroOrOne)
                }.WithHandler<AuthoringApiHandlers>(nameof(AuthoringApiHandlers.DeleteCollection)),
                new Command("add-codetemplate", "Adds a code template to an element")
                {
                    new Argument("FilePath", "A relative path to the code file, from the current directory"),
                    new Option("--name", "A friendly name for the code template",
                        arity: ArgumentArity.ZeroOrOne),
                    new Option("--aschildof", "The expression of the element/collection to add the code template to",
                        typeof(string), arity: ArgumentArity.ZeroOrOne)
                }.WithHandler<AuthoringApiHandlers>(nameof(AuthoringApiHandlers.AddCodeTemplate)),
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
                }.WithHandler<AuthoringApiHandlers>(nameof(AuthoringApiHandlers.AddCodeTemplateWithCommand)),
                new Command("codetemplate", "Edits a code template in an editor")
                {
                    new Argument("TemplateName", "The name of the code template"),
                    new Option("--with", "Name of or full path to an application to edit the code template",
                        typeof(string), arity: ArgumentArity.ExactlyOne),
                    new Option("--args", "An program arguments",
                        typeof(string), arity: ArgumentArity.ZeroOrOne),
                    new Option("--aschildof", "The expression of the element/collection to edit the code template from",
                        typeof(string), arity: ArgumentArity.ZeroOrOne)
                }.WithHandler<AuthoringApiHandlers>(nameof(AuthoringApiHandlers.EditCodeTemplate)),
                new Command("delete-codetemplate", "Deletes a code template from an element/collection in the pattern")
                {
                    new Argument("TemplateName", "The name of the code template"),
                    new Option("--aschildof",
                        "The expression of the element/collection to delete the code template from",
                        typeof(string), arity: ArgumentArity.ZeroOrOne)
                }.WithHandler<AuthoringApiHandlers>(nameof(AuthoringApiHandlers.DeleteCodeTemplate)),
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
                }.WithHandler<AuthoringApiHandlers>(nameof(AuthoringApiHandlers.AddCodeTemplateCommand)),
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
                }.WithHandler<AuthoringApiHandlers>(nameof(AuthoringApiHandlers.UpdateCodeTemplateCommand)),
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
                    .WithHandler<AuthoringApiHandlers>(nameof(AuthoringApiHandlers.AddCliCommand)),
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
                }.WithHandler<AuthoringApiHandlers>(nameof(AuthoringApiHandlers.UpdateCliCommand)),
                new Command("delete-command", "Deletes any command from an element/collection in the pattern")
                {
                    new Argument("CommandName", "The name of the command"),
                    new Option("--aschildof", "The expression of the element/collection to delete the command from",
                        typeof(string), arity: ArgumentArity.ZeroOrOne)
                }.WithHandler<AuthoringApiHandlers>(nameof(AuthoringApiHandlers.DeleteCommand)),
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
                }.WithHandler<AuthoringApiHandlers>(nameof(AuthoringApiHandlers.AddCommandLaunchPoint)),
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
                }.WithHandler<AuthoringApiHandlers>(nameof(AuthoringApiHandlers.UpdateLaunchPoint)),
                new Command("delete-command-launchpoint",
                    "Deletes a launch point from an element/collection in the pattern")
                {
                    new Argument("LaunchPointName", "The name of the launch point"),
                    new Option("--aschildof",
                        "The expression of the element/collection to delete the launch point from",
                        typeof(string), arity: ArgumentArity.ZeroOrOne)
                }.WithHandler<AuthoringApiHandlers>(nameof(AuthoringApiHandlers.DeleteLaunchPoint))
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
                }.WithHandler<AuthoringApiHandlers>(nameof(AuthoringApiHandlers.TestCodeTemplate)),
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
                }.WithHandler<AuthoringApiHandlers>(nameof(AuthoringApiHandlers.TestCodeTemplateCommand))
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
                    }.WithHandler<AuthoringApiHandlers>(nameof(AuthoringApiHandlers.Publish))
                    .WithAlias(PatternSubCommandName)
            }.WithAlias(BuildCommandName);
            var installCommands = new Command(InstallCommandName, "Installing toolkits")
            {
                new Command(ToolkitSubCommandName, "Installs the pattern from a toolkit")
                {
                    new Argument("Location", "The location of the *.toolkit file to install into the current directory")
                }.WithHandler<RuntimeApiHandlers>(nameof(RuntimeApiHandlers.Install))
            };
            var runCommands = new Command(RunCommandName, "Running patterns from toolkits")
            {
                new Command(ToolkitSubCommandName, "Creates a new draft from a toolkit")
                {
                    new Argument("PatternName", "The name of the pattern in the toolkit that you want to use"),
                    new Option("--name", "A name for the draft", arity: ArgumentArity.ZeroOrOne)
                }.WithHandler<RuntimeApiHandlers>(nameof(RuntimeApiHandlers.NewDraft)),
                new Command("switch", "Switches to configuring another draft")
                {
                    new Argument("DraftId", "The id of the existing draft to configure")
                }.WithHandler<RuntimeApiHandlers>(nameof(RuntimeApiHandlers.SwitchDraft))
            };
            var configureCommands = new Command(ConfigureCommandName, "Configuring drafts to patterns from toolkits")
            {
                new Command("add", "Configure an element in the draft")
                {
                    new Argument("Expression", "The expression of the element to configure"),
                    new Option("--and-set", "A Name=Value pair of a property assignment",
                        arity: ArgumentArity.ZeroOrMore)
                }.WithHandler<RuntimeApiHandlers>(nameof(RuntimeApiHandlers.ConfigureDraftAddTo)),
                new Command("add-one-to", "Add a new item to a collection in the draft")
                {
                    new Argument("Expression", "The expression of the element/collection to add to"),
                    new Option("--and-set", "Additional Name=Value pair of a property assignment",
                        arity: ArgumentArity.ZeroOrMore)
                }.WithHandler<RuntimeApiHandlers>(nameof(RuntimeApiHandlers.ConfigureDraftAddOneTo)),
                new Command("on", "Set the properties of an existing item in the draft")
                {
                    new Argument("Expression", "The expression of the element/collection to assign to"),
                    new Option("--and-set", "Additional Name=Value pair of a property assignment",
                        arity: ArgumentArity.ZeroOrMore)
                }.WithHandler<RuntimeApiHandlers>(nameof(RuntimeApiHandlers.ConfigureDraftOn)),
                new Command("reset", "Reset all the properties of an existing item in the draft")
                {
                    new Argument("Expression", "The expression of the element to reset")
                }.WithHandler<RuntimeApiHandlers>(nameof(RuntimeApiHandlers.ConfigureDraftResetElement)),
                new Command("clear", "Deletes all the items from an existing collection in the draft")
                {
                    new Argument("Expression", "The expression of the collection to clear")
                }.WithHandler<RuntimeApiHandlers>(nameof(RuntimeApiHandlers.ConfigureDraftClearCollection)),
                new Command("delete", "Deletes an existing item in the draft")
                {
                    new Argument("Expression", "The expression of the element/collection to delete")
                }.WithHandler<RuntimeApiHandlers>(nameof(RuntimeApiHandlers.ConfigureDraftDeleteElement))
            };
            var validateCommands = new Command(ValidateCommandName, "Validating patterns from toolkits")
            {
                new Command(DraftSubCommandName, "Validate the current draft")
                {
                    new Option("--on", "The expression of the element/collection to validate",
                        arity: ArgumentArity.ZeroOrOne)
                }.WithHandler<RuntimeApiHandlers>(nameof(RuntimeApiHandlers.ValidateDraft))
            };
            var executeCommands = new Command(ExecuteCommandName, "Executing automation on patterns from toolkits")
            {
                new Command("command", "Executes the launch point on the draft")
                {
                    new Argument("Name", "The name of the launch point to execute"),
                    new Option("--on",
                        "The expression of the element/collection containing the launch point to execute",
                        arity: ArgumentArity.ZeroOrOne)
                }.WithHandler<RuntimeApiHandlers>(nameof(RuntimeApiHandlers.ExecuteLaunchPoint))
            };
            var viewCommands = new Command(ViewCommandName, "Viewing patterns and drafts")
            {
                new Command(PatternSubCommandName, "View the configuration of the current pattern")
                {
                    new Option("--all", "Include additional configuration, like automation and code templates",
                        typeof(bool), () => false, ArgumentArity.ZeroOrOne)
                }.WithHandler<AuthoringApiHandlers>(nameof(AuthoringApiHandlers.ViewPattern)),
                new Command(ToolkitSubCommandName, "View the configuration of the current toolkit")
                {
                    new Option("--all", "Include additional configuration, like automation and code templates",
                        typeof(bool), () => false, ArgumentArity.ZeroOrOne)
                }.WithHandler<RuntimeApiHandlers>(nameof(RuntimeApiHandlers.ViewToolkit)),
                new Command(DraftSubCommandName, "View the configuration of the current draft")
                {
                    new Option("--todo", "Displays the details of the pattern, and any validation errors", typeof(bool),
                        () => false, ArgumentArity.ZeroOrOne)
                }.WithHandler<RuntimeApiHandlers>(nameof(RuntimeApiHandlers.ViewDraft))
            };
            var listCommands = new Command(ListCommandName, "Listing patterns, toolkits and drafts")
            {
                new Command("all", "Lists all patterns, toolkits and drafts")
                    .WithAlias("everything")
                    .WithHandler<AuthoringApiHandlers>(nameof(AuthoringApiHandlers.ListEverything)),
                new Command(PatternsSubCommandName, "Lists all patterns being edited")
                    .WithHandler<AuthoringApiHandlers>(nameof(AuthoringApiHandlers.ListPatterns)),
                new Command(ToolkitsSubCommandName, "Lists all installed toolkits")
                    .WithHandler<RuntimeApiHandlers>(nameof(RuntimeApiHandlers.ListToolkits)),
                new Command(DraftsSubCommandName, "Lists all drafts being configured")
                    .WithHandler<RuntimeApiHandlers>(nameof(RuntimeApiHandlers.ListDrafts))
            };
            var upgradeCommands = new Command(UpgradeCommandName, "Upgrading toolkits and drafts")
            {
                new Command(DraftSubCommandName, "Upgrades a draft from a new version of its toolkit")
                    {
                        new Option("--force", "Force the upgrade despite any compatability errors")
                    }
                    .WithHandler<RuntimeApiHandlers>(nameof(RuntimeApiHandlers.UpgradeDraft))
            };
            var deleteCommands = new Command(DeleteCommandName, "Deleting patterns, toolkits and drafts")
            {
                new Command(DraftSubCommandName, "Deletes the current draft")
                    .WithHandler<RuntimeApiHandlers>(nameof(RuntimeApiHandlers.DeleteDraft))
            };
#if TESTINGONLY
            var testingOnlyCommands = new Command(TestingOnlyCommandName, "For testing only!")
            {
                new Command("fail", "Forces the command to fail with an exception")
                {
                    new Option("--nested", "Whether to nest the exception", typeof(bool), () => false,
                        ArgumentArity.ZeroOrOne),
                    new Option("--message", "A message for the thrown exception", arity: ArgumentArity.ZeroOrOne)
                }.WithHandler<TestingOnlyApiHandlers>(nameof(TestingOnlyApiHandlers.Fail)),
                new Command("succeed", "Runs a command successfully")
                {
                    new Option("--message", "A message to output", arity: ArgumentArity.ZeroOrOne),
                    new Option("--value", "A value to substitute into the message", arity: ArgumentArity.ZeroOrOne)
                }.WithHandler<TestingOnlyApiHandlers>(nameof(TestingOnlyApiHandlers.Succeed))
            };
#endif
            var command =
                new RootCommand(
                    "Templatize patterns from your own codebase, make them programmable, then share them with your team")
                {
                    administrativeCommands,
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
                    deleteCommands,
#if TESTINGONLY
                    testingOnlyCommands
#endif
                };
            command.AddGlobalOption(new Option($"--{CollectUsageEnabledOption}",
                "Allow collection of usage data",
                typeof(bool), () => true,
                ArgumentArity.ZeroOrOne));
            command.AddGlobalOption(new Option($"--{CollectUsageCorrelationOption}",
                "Request ID to be used in collection of usage data",
                typeof(string), () => null,
                ArgumentArity.ZeroOrOne));
            command.AddGlobalOption(new Option($"--{StructuredOutputOption}",
                    "Provide output as structured data in json",
                    typeof(bool), () => false,
                    ArgumentArity.ZeroOrOne)
                .WithAlias("--os"));
            command.AddGlobalOption(new Option($"--{DebugOption}", "Show more error details when there is an exception",
                typeof(bool),
#if TESTINGONLY
                () => true,
#else
                () => false,
#endif
                ArgumentArity.ZeroOrOne));

            return command;
        }
    }
}