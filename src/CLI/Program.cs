using System;
using System.CommandLine;
using automate.Extensions;

namespace automate
{
    internal class Program
    {
        private static readonly PatternApplication application = new PatternApplication(Environment.CurrentDirectory);

        // ReSharper disable once UnusedMember.Local

        private static int Main(string[] args)
        {
            try
            {
                Console.WriteLine(application.CurrentPatternId.Exists()
                    ? OutputMessages.CommandLine_Output_PatternInUse.Format(application.CurrentPatternName,
                        application.CurrentPatternId)
                    : OutputMessages.CommandLine_Output_NoPatternSelected);
                Console.WriteLine();

                var command = new RootCommand
                {
                    new Command("create", "Creates a new pattern")
                    {
                        new Argument("Name", "The name of the pattern to create")
                    }.WithHandler(nameof(HandleCreate)),
                    new Command("use", "Uses an existing pattern")
                    {
                        new Argument("Name", "The name of the existing pattern to use")
                    }.WithHandler(nameof(HandleUse)),
                    new Command("add-codetemplate", "Adds a code template to an element")
                    {
                        new Argument("FilePath", "A relative path to the code file, from the current directory"),
                        new Option("--name", "A friendly name for the code template",
                            arity: ArgumentArity.ZeroOrOne),
                        new Option("--aschildof", "The element/collection to add the launch point to", typeof(string),
                            arity: ArgumentArity.ZeroOrOne)
                    }.WithHandler(nameof(HandleAddCodeTemplate)),
                    new Command("list-codetemplates", "Lists the code templates for this pattern")
                        .WithHandler(nameof(HandleListCodeTemplate)),
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
                    }.WithHandler(nameof(HandleAddAttribute)),
                    new Command("add-element", "Adds an element to an element/collection in the pattern")
                    {
                        new Argument("Name", "The name of the element"),
                        new Option("--displayedas", "A friendly display name for the element", typeof(string),
                            arity: ArgumentArity.ZeroOrOne),
                        new Option("--describedas", "A description for the element", typeof(string),
                            arity: ArgumentArity.ZeroOrOne),
                        new Option("--aschildof", "The element/collection to add the element to", typeof(string),
                            arity: ArgumentArity.ZeroOrOne)
                    }.WithHandler(nameof(HandleAddElement)),
                    new Command("add-collection", "Adds a collection to an element/collection in the pattern")
                    {
                        new Argument("Name", "The name of the collection"),
                        new Option("--displayedas", "A friendly display name for the collection", typeof(string),
                            arity: ArgumentArity.ZeroOrOne),
                        new Option("--describedas", "A description for the collection", typeof(string),
                            arity: ArgumentArity.ZeroOrOne),
                        new Option("--aschildof", "The element/collection to add the collection to", typeof(string),
                            arity: ArgumentArity.ZeroOrOne)
                    }.WithHandler(nameof(HandleAddCollection)),
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
                    }.WithHandler(nameof(HandleAddCodeTemplateCommand)),
                    new Command("add-command-launchpoint", "Adds a launch point for a command")
                    {
                        new Argument("CommandIdentifiers", "The identifiers of the commands to launch"),
                        new Option("--name", "A name for the launch point", typeof(string),
                            arity: ArgumentArity.ZeroOrOne),
                        new Option("--aschildof", "The element/collection to add the launch point to", typeof(string),
                            arity: ArgumentArity.ZeroOrOne)
                    }.WithHandler(nameof(HandleAddCommandLaunchPoint))
                };
                command.Description = "Create and Run automated patterns";
                command.AddGlobalOption(new Option("--output-structured", "Provide output as structured data",
                    typeof(bool), () => false,
                    ArgumentArity.ZeroOrOne));

                return command.Invoke(args);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                return 1;
            }
        }

        private static void HandleAddCodeTemplateCommand(string name, bool asTearOff, string withPath, string asChildOf,
            bool outputStructured, IConsole console)
        {
            var command = application.AddCodeTemplateCommand(name, asTearOff, withPath, asChildOf);
            WriteOutput(console, outputStructured, OutputMessages.CommandLine_Output_CodeTemplateCommandAdded, name,
                command.Id);
        }

        private static void HandleAddCommandLaunchPoint(string commandIdentifiers, string name, string asChildOf,
            bool outputStructured, IConsole console)
        {
            var launchPoint = application.AddCommandLaunchPoint(commandIdentifiers, name, asChildOf);
            WriteOutput(console, outputStructured, OutputMessages.CommandLine_Output_LaunchPointAdded,
                launchPoint.Name);
        }

        private static void HandleAddElement(string name, string displayedAs, string describedAs, string asChildOf,
            bool outputStructured, IConsole console)
        {
            var parent = application.AddElement(name, displayedAs, describedAs, false, asChildOf);
            WriteOutput(console, outputStructured, OutputMessages.CommandLine_Output_ElementAdded, name, parent.Id);
        }

        private static void HandleAddCollection(string name, string displayedAs, string describedAs, string asChildOf,
            bool outputStructured, IConsole console)
        {
            var parent = application.AddElement(name, displayedAs, describedAs, true, asChildOf);
            WriteOutput(console, outputStructured, OutputMessages.CommandLine_Output_CollectionAdded, name, parent.Id);
        }

        private static void HandleAddAttribute(string name, string isOfType, string defaultValue, bool isRequired,
            string isOneOf, string asChildOf, bool outputStructured, IConsole console)
        {
            var parent = application.AddAttribute(name, isOfType, defaultValue, isRequired, isOneOf, asChildOf);
            WriteOutput(console, outputStructured, OutputMessages.CommandLine_Output_AttributeAdded, name, parent.Id);
        }

        private static void HandleCreate(string name, bool outputStructured, IConsole console)
        {
            application.CreateNewPattern(name);
            WriteOutput(console, outputStructured,
                OutputMessages.CommandLine_Output_PatternCreated, name, application.CurrentPatternId);
        }

        private static void HandleUse(string name, bool outputStructured, IConsole console)
        {
            application.SwitchCurrentPattern(name);
            WriteOutput(console, outputStructured,
                OutputMessages.CommandLine_Output_PatternSwitched, name, application.CurrentPatternId);
        }

        private static void HandleAddCodeTemplate(string filepath, string name, string asChildOf, bool outputStructured,
            IConsole console)
        {
            var currentDirectory = Environment.CurrentDirectory;
            var template = application.AttachCodeTemplate(currentDirectory, filepath, name, asChildOf);
            WriteOutput(console, outputStructured,
                OutputMessages.CommandLine_Output_CodeTemplatedAdded, template.Name, template.FullPath);
        }

        private static void HandleListCodeTemplate(bool outputStructured, IConsole console)
        {
            var templates = application.ListCodeTemplates();
            if (templates.Count == 0)
            {
                WriteOutput(console, outputStructured, OutputMessages.CommandLine_Output_NoCodeTemplates);
            }
            else
            {
                WriteOutput(console, outputStructured, OutputMessages.CommandLine_Output_NoCodeTemplates,
                    templates.Count);
                templates.ForEach(template => WriteOutput(console, outputStructured, $"{template.Name}"));
            }
        }

        private static void WriteOutput(IConsole console, bool outputStructured, string messageTemplate,
            params object[] args)
        {
            console.WriteLine(outputStructured
                ? messageTemplate.FormatTemplateStructured(args)
                : messageTemplate.FormatTemplate(args));
        }
    }
}