using System;
using System.Collections.Generic;
using System.CommandLine;
using automate.Extensions;
using JetBrains.Annotations;

namespace automate
{
    internal class Program
    {
        public const string AuthoringCommandName = "pattern";
        public const string RuntimeCommandName = "toolkit";
        private static readonly AuthoringApplication authoring = new AuthoringApplication(Environment.CurrentDirectory);
        private static readonly RuntimeApplication runtime = new RuntimeApplication(Environment.CurrentDirectory);

        [UsedImplicitly]
        private static int Main(string[] args)
        {
            try
            {
                var authoringCommands = new Command(AuthoringCommandName, "Creating patterns")
                {
                    new Command("create", "Creates a new pattern")
                    {
                        new Argument("Name", "The name of the pattern to create")
                    }.WithHandler<AuthoringHandlers>(nameof(AuthoringHandlers.HandleCreate)),
                    new Command("use", "Uses an existing pattern")
                    {
                        new Argument("Name", "The name of the existing pattern to use")
                    }.WithHandler<AuthoringHandlers>(nameof(AuthoringHandlers.HandleUse)),
                    new Command("add-codetemplate", "Adds a code template to an element")
                    {
                        new Argument("FilePath", "A relative path to the code file, from the current directory"),
                        new Option("--name", "A friendly name for the code template",
                            arity: ArgumentArity.ZeroOrOne),
                        new Option("--aschildof", "The element/collection to add the launch point to", typeof(string),
                            arity: ArgumentArity.ZeroOrOne)
                    }.WithHandler<AuthoringHandlers>(nameof(AuthoringHandlers.HandleAddCodeTemplate)),
                    new Command("list-codetemplates", "Lists the code templates for this pattern")
                        .WithHandler<AuthoringHandlers>(nameof(AuthoringHandlers.HandleListCodeTemplate)),
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
                        new Option("--aschildof", "The element/collection to add the attribute to", typeof(string),
                            arity: ArgumentArity.ZeroOrOne)
                    }.WithHandler<AuthoringHandlers>(nameof(AuthoringHandlers.HandleAddAttribute)),
                    new Command("add-element", "Adds an element to an element/collection in the pattern")
                    {
                        new Argument("Name", "The name of the element"),
                        new Option("--displayedas", "A friendly display name for the element", typeof(string),
                            arity: ArgumentArity.ZeroOrOne),
                        new Option("--describedas", "A description for the element", typeof(string),
                            arity: ArgumentArity.ZeroOrOne),
                        new Option("--aschildof", "The element/collection to add the element to", typeof(string),
                            arity: ArgumentArity.ZeroOrOne)
                    }.WithHandler<AuthoringHandlers>(nameof(AuthoringHandlers.HandleAddElement)),
                    new Command("add-collection", "Adds a collection to an element/collection in the pattern")
                    {
                        new Argument("Name", "The name of the collection"),
                        new Option("--displayedas", "A friendly display name for the collection", typeof(string),
                            arity: ArgumentArity.ZeroOrOne),
                        new Option("--describedas", "A description for the collection", typeof(string),
                            arity: ArgumentArity.ZeroOrOne),
                        new Option("--aschildof", "The element/collection to add the collection to", typeof(string),
                            arity: ArgumentArity.ZeroOrOne)
                    }.WithHandler<AuthoringHandlers>(nameof(AuthoringHandlers.HandleAddCollection)),
                    new Command("add-codetemplate-command", "Adds a command that renders a code template")
                    {
                        new Argument("Name", "The name of the code template"),
                        new Option("--astearoff",
                            "Only if you only want to generate the file once, and not overwrite if already exists",
                            typeof(bool),
                            arity: ArgumentArity.ZeroOrOne),
                        new Option("--withpath", "The full path of the generated file, with filename.", typeof(string),
                            arity: ArgumentArity.ExactlyOne),
                        new Option("--aschildof", "The element/collection to add the launch point to", typeof(string),
                            arity: ArgumentArity.ZeroOrOne)
                    }.WithHandler<AuthoringHandlers>(nameof(AuthoringHandlers.HandleAddCodeTemplateCommand)),
                    new Command("add-command-launchpoint", "Adds a launch point for a command")
                    {
                        new Argument("CommandIdentifiers", "The identifiers of the commands to launch"),
                        new Option("--name", "A name for the launch point", typeof(string),
                            arity: ArgumentArity.ZeroOrOne),
                        new Option("--aschildof", "The element/collection to add the launch point to", typeof(string),
                            arity: ArgumentArity.ZeroOrOne)
                    }.WithHandler<AuthoringHandlers>(nameof(AuthoringHandlers.HandleAddCommandLaunchPoint))
                };
                var runtimeCommands = new Command(RuntimeCommandName, "Running toolkits");

                var command = new RootCommand
                {
                    authoringCommands,
                    runtimeCommands
                };
                command.Description = "Create automated patterns as toolkits";
                command.AddGlobalOption(new Option("--output-structured", "Provide output as structured data",
                    typeof(bool), () => false,
                    ArgumentArity.ZeroOrOne));

                if (IsAuthoringCommand(args))
                {
                    Console.WriteLine(authoring.CurrentPatternId.Exists()
                        ? OutputMessages.CommandLine_Output_PatternInUse.Format(authoring.CurrentPatternName,
                            authoring.CurrentPatternId)
                        : OutputMessages.CommandLine_Output_NoPatternSelected);
                    Console.WriteLine();
                }
                if (IsRuntimeCommand(args))
                {
                    Console.WriteLine(runtime.CurrentToolkitId.Exists()
                        ? OutputMessages.CommandLine_Output_ToolkitInUse.Format(runtime.CurrentToolkitName,
                            runtime.CurrentToolkitId)
                        : OutputMessages.CommandLine_Output_NoToolkitSelected);
                    Console.WriteLine();
                }

                return command.Invoke(args);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                return 1;
            }
        }

        private static bool IsRuntimeCommand(IReadOnlyList<string> args)
        {
            return args.Count > 0 && args[0] == RuntimeCommandName;
        }

        private static bool IsAuthoringCommand(IReadOnlyList<string> args)
        {
            return args.Count > 0 && args[0] == AuthoringCommandName;
        }

        private class AuthoringHandlers
        {
            internal static void HandleAddCodeTemplateCommand(string name, bool asTearOff, string withPath,
                string asChildOf,
                bool outputStructured, IConsole console)
            {
                var command = authoring.AddCodeTemplateCommand(name, asTearOff, withPath, asChildOf);
                console.WriteOutput(outputStructured, OutputMessages.CommandLine_Output_CodeTemplateCommandAdded, name,
                    command.Id);
            }

            internal static void HandleAddCommandLaunchPoint(string commandIdentifiers, string name, string asChildOf,
                bool outputStructured, IConsole console)
            {
                var launchPoint = authoring.AddCommandLaunchPoint(commandIdentifiers, name, asChildOf);
                console.WriteOutput(outputStructured, OutputMessages.CommandLine_Output_LaunchPointAdded,
                    launchPoint.Name);
            }

            internal static void HandleAddElement(string name, string displayedAs, string describedAs, string asChildOf,
                bool outputStructured, IConsole console)
            {
                var parent = authoring.AddElement(name, displayedAs, describedAs, false, asChildOf);
                console.WriteOutput(outputStructured, OutputMessages.CommandLine_Output_ElementAdded, name, parent.Id);
            }

            internal static void HandleAddCollection(string name, string displayedAs, string describedAs,
                string asChildOf,
                bool outputStructured, IConsole console)
            {
                var parent = authoring.AddElement(name, displayedAs, describedAs, true, asChildOf);
                console.WriteOutput(outputStructured, OutputMessages.CommandLine_Output_CollectionAdded, name,
                    parent.Id);
            }

            internal static void HandleAddAttribute(string name, string isOfType, string defaultValue, bool isRequired,
                string isOneOf, string asChildOf, bool outputStructured, IConsole console)
            {
                var parent = authoring.AddAttribute(name, isOfType, defaultValue, isRequired, isOneOf, asChildOf);
                console.WriteOutput(outputStructured, OutputMessages.CommandLine_Output_AttributeAdded, name,
                    parent.Id);
            }

            internal static void HandleCreate(string name, bool outputStructured, IConsole console)
            {
                authoring.CreateNewPattern(name);
                console.WriteOutput(outputStructured,
                    OutputMessages.CommandLine_Output_PatternCreated, name, authoring.CurrentPatternId);
            }

            internal static void HandleUse(string name, bool outputStructured, IConsole console)
            {
                authoring.SwitchCurrentPattern(name);
                console.WriteOutput(outputStructured,
                    OutputMessages.CommandLine_Output_PatternSwitched, name, authoring.CurrentPatternId);
            }

            internal static void HandleAddCodeTemplate(string filepath, string name, string asChildOf,
                bool outputStructured,
                IConsole console)
            {
                var currentDirectory = Environment.CurrentDirectory;
                var template = authoring.AttachCodeTemplate(currentDirectory, filepath, name, asChildOf);
                console.WriteOutput(outputStructured,
                    OutputMessages.CommandLine_Output_CodeTemplatedAdded, template.Name,
                    template.Metadata[CodeTemplate.OriginalPathMetadataName]);
            }

            internal static void HandleListCodeTemplate(bool outputStructured, IConsole console)
            {
                var templates = authoring.ListCodeTemplates();
                if (templates.Count == 0)
                {
                    console.WriteOutput(outputStructured, OutputMessages.CommandLine_Output_NoCodeTemplates);
                }
                else
                {
                    console.WriteOutput(outputStructured, OutputMessages.CommandLine_Output_NoCodeTemplates,
                        templates.Count);
                    templates.ForEach(template => console.WriteOutput(outputStructured, $"{template.Name}"));
                }
            }
        }
    }

    internal static class ConsoleExtensions
    {
        internal static void WriteOutput(this IConsole console, bool outputStructured, string messageTemplate,
            params object[] args)
        {
            console.WriteLine(outputStructured
                ? messageTemplate.FormatTemplateStructured(args)
                : messageTemplate.FormatTemplate(args));
        }
    }
}