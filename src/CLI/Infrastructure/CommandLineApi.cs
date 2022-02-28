using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.IO;
using System.CommandLine.Parsing;
using System.Linq;
using System.Text;
using Automate.CLI.Application;
using Automate.CLI.Domain;
using Automate.CLI.Extensions;
using Attribute = Automate.CLI.Domain.Attribute;

namespace Automate.CLI.Infrastructure
{
    internal class CommandLineApi
    {
        public const string CreateCommandName = "create";
        public const string EditCommandName = "edit";
        public const string BuildCommandName = "build";
        public const string InstallCommandName = "install";
        public const string RunCommandName = "run";
        public const string UsingCommandName = "using";
        public const string ValidateCommandName = "validate";
        public const string ExecuteCommandName = "execute";
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
                new Command("view-pattern", "Views the current configuration of this pattern")
                    {
                        new Option("--full", "Include additional configuration, like automation and code templates", typeof(bool), () => false, ArgumentArity.ZeroOrOne)
                    }
                    .WithHandler<AuthoringHandlers>(nameof(AuthoringHandlers.HandleViewPattern)),
                new Command("use", "Uses an existing pattern")
                {
                    new Argument("Name", "The name of the existing pattern to use")
                }.WithHandler<AuthoringHandlers>(nameof(AuthoringHandlers.HandleUse)),
                new Command("add-codetemplate", "Adds a code template to an element")
                {
                    new Argument("FilePath", "A relative path to the code file, from the current directory"),
                    new Option("--name", "A friendly name for the code template",
                        arity: ArgumentArity.ZeroOrOne),
                    new Option("--aschildof", "The expression of the element/collection to add the launch point to",
                        typeof(string),
                        arity: ArgumentArity.ZeroOrOne)
                }.WithHandler<AuthoringHandlers>(nameof(AuthoringHandlers.HandleAddCodeTemplate)),
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
                        typeof(string),
                        arity: ArgumentArity.ZeroOrOne)
                }.WithHandler<AuthoringHandlers>(nameof(AuthoringHandlers.HandleAddAttribute)),
                new Command("add-element", "Adds an element to an element/collection in the pattern")
                {
                    new Argument("Name", "The name of the element"),
                    new Option("--displayedas", "A friendly display name for the element", typeof(string),
                        arity: ArgumentArity.ZeroOrOne),
                    new Option("--describedas", "A description for the element", typeof(string),
                        arity: ArgumentArity.ZeroOrOne),
                    new Option("--aschildof", "The expression of the element/collection to add the element to",
                        typeof(string), arity: ArgumentArity.ZeroOrOne)
                }.WithHandler<AuthoringHandlers>(nameof(AuthoringHandlers.HandleAddElement)),
                new Command("add-collection", "Adds a collection to an element/collection in the pattern")
                {
                    new Argument("Name", "The name of the collection"),
                    new Option("--displayedas", "A friendly display name for the collection", typeof(string),
                        arity: ArgumentArity.ZeroOrOne),
                    new Option("--describedas", "A description for the collection", typeof(string),
                        arity: ArgumentArity.ZeroOrOne),
                    new Option("--aschildof", "The expression of the element/collection to add the collection to",
                        typeof(string),
                        arity: ArgumentArity.ZeroOrOne),
                    new Option("--ality",
                        "The number of instances of this element in this collection that must exist",
                        typeof(ElementCardinality), () => ElementCardinality.ZeroOrMany, ArgumentArity.ZeroOrOne)
                }.WithHandler<AuthoringHandlers>(nameof(AuthoringHandlers.HandleAddCollection)),
                new Command("add-codetemplate-command", "Adds a command that renders a code template")
                {
                    new Argument("CodeTemplateName", "The name of the code template"),
                    new Option("--name", "A name for the command", typeof(string),
                        arity: ArgumentArity.ZeroOrOne),
                    new Option("--astearoff",
                        "Only if you only want to generate the file once, and not overwrite the file if it already exists",
                        typeof(bool),
                        arity: ArgumentArity.ZeroOrOne),
                    new Option("--withpath", "The full path of the generated file, with filename.", typeof(string),
                        arity: ArgumentArity.ExactlyOne),
                    new Option("--aschildof", "The expression of the element/collection to add the launch point to",
                        typeof(string),
                        arity: ArgumentArity.ZeroOrOne)
                }.WithHandler<AuthoringHandlers>(nameof(AuthoringHandlers.HandleAddCodeTemplateCommand)),
                new Command("add-command-launchpoint", "Adds a launch point for a command")
                {
                    new Argument("CommandIdentifiers", "A semi-colon delimited list of identifiers of the commands to launch"),
                    new Option("--name", "A name for the launch point", typeof(string),
                        arity: ArgumentArity.ZeroOrOne),
                    new Option("--aschildof", "The expression of the element/collection to add the launch point to",
                        typeof(string),
                        arity: ArgumentArity.ZeroOrOne)
                }.WithHandler<AuthoringHandlers>(nameof(AuthoringHandlers.HandleAddCommandLaunchPoint))
            };
            var buildCommands = new Command(BuildCommandName, "Building toolkits from patterns")
            {
                new Command("toolkit", "Builds a pattern into a toolkit")
                {
                    new Option("--versionas", "A specific version number, or 'auto' to auto-increment the current version",
                        typeof(string),
                        arity: ArgumentArity.ZeroOrOne)
                }.WithHandler<AuthoringHandlers>(nameof(AuthoringHandlers.HandleBuild))
            };
            var installCommands = new Command(InstallCommandName, "Installing toolkits")
            {
                new Command("toolkit", "Installs the pattern from a toolkit")
                {
                    new Argument("Location",
                        "The location of the *.toolkit file to install into the current directory")
                }.WithHandler<RuntimeHandlers>(nameof(RuntimeHandlers.HandleInstall))
            };
            var runCommands = new Command(RunCommandName, "Running patterns from toolkits")
            {
                new Command("list-toolkits", "Lists all installed toolkits")
                    .WithHandler<RuntimeHandlers>(nameof(RuntimeHandlers.HandleListToolkits)),
                new Command("toolkit", "Creates a new solution from a toolkit")
                {
                    new Argument("Name", "The name of the toolkit that you want to use")
                }.WithHandler<RuntimeHandlers>(nameof(RuntimeHandlers.HandleNewSolution)),
                new Command("list-solutions", "Lists all created solutions")
                    .WithHandler<RuntimeHandlers>(nameof(RuntimeHandlers.HandleListSolutions))
            };
            var usingCommands = new Command(UsingCommandName, "Using patterns from toolkits")
            {
                new Argument("SolutionID", "The identifier of the current solution that you are using"),
                new Option("--add", "The expression of the element to add", arity: ArgumentArity.ZeroOrOne),
                new Option("--add-one-to", "The expression of the collection to add a new element to",
                    arity: ArgumentArity.ZeroOrOne),
                new Option("--set", "A name=value pair of properties to assign", arity: ArgumentArity.ZeroOrOne),
                new Option("--and-set", "A name=value pair of properties to assign",
                    arity: ArgumentArity.ZeroOrMore),
                new Option("--on", "The expression of the element/collection to assign properties to", arity: ArgumentArity.ZeroOrOne),
                new Option("--view-configuration", "View the current configuration of this solution", typeof(bool),
                    () => false, ArgumentArity.ZeroOrOne)
            }.WithHandler<RuntimeHandlers>(nameof(RuntimeHandlers.HandleUsing));
            var validateCommands = new Command(ValidateCommandName, "Validating patterns from toolkits")
            {
                new Argument("SolutionID", "The identifier of the current solution that you are validating"),
                new Option("--on", "The expression of the element/collection to validate", arity: ArgumentArity.ZeroOrOne)
            }.WithHandler<RuntimeHandlers>(nameof(RuntimeHandlers.HandleValidate));
            var executeCommands = new Command(ExecuteCommandName, "Executing commands on patterns from toolkits")
            {
                new Argument("SolutionID", "The identifier of the current solution containing the command"),
                new Option("--command", "The command name to execute", arity: ArgumentArity.ExactlyOne),
                new Option("--on", "The expression of the element/collection containing the command to execute", arity: ArgumentArity.ZeroOrOne)
            }.WithHandler<RuntimeHandlers>(nameof(RuntimeHandlers.HandleExecuteCommand));

            var command =
                new RootCommand(
                    "Templatize patterns from your own codebase, make them programmable, then share them with your team")
                {
                    createCommands,
                    editCommands,
                    buildCommands,
                    installCommands,
                    runCommands,
                    usingCommands,
                    validateCommands,
                    executeCommands
                };
            command.AddGlobalOption(new Option("--output-structured", "Provide output as structured data",
                typeof(bool), () => false,
                ArgumentArity.ZeroOrOne));

            if (IsAuthoringCommand(args))
            {
                if (Authoring.CurrentPatternId.Exists())
                {
                    ConsoleExtensions.WriteOutput(
                        OutputMessages.CommandLine_Output_PatternInUse.FormatTemplate(Authoring.CurrentPatternName,
                            Authoring.CurrentPatternId), ConsoleColor.Gray);
                }
                else
                {
                    ConsoleExtensions.WriteErrorWarning(OutputMessages.CommandLine_Output_NoPatternSelected);
                }
            }
            if (IsRuntimeCommand(args))
            {
            }

            var parser = new CommandLineBuilder(command)
                .UseDefaults()
                .UseExceptionHandler((ex, context) =>
                {
                    var message = ex.InnerException.Exists()
                        ? ex.InnerException.Message
                        : ex.Message;
                    Console.Error.WriteLine();
                    context.Console.WriteError($"Failed Unexpectedly, reason: {message}", ConsoleColor.Red);
#if DEBUG
                    throw ex;
#endif
                }, 1)
                .Build();

            return parser.Invoke(args);
        }

        private static bool IsRuntimeCommand(IReadOnlyList<string> args)
        {
            return args.Count > 0
                   && (args[0] == InstallCommandName || args[0] == RunCommandName || args[0] == UsingCommandName
                       || args[0] == ValidateCommandName || args[0] == ExecuteCommandName);
        }

        private static bool IsAuthoringCommand(IReadOnlyList<string> args)
        {
            return args.Count > 0
                   && (args[0] == CreateCommandName || args[0] == EditCommandName || args[0] == BuildCommandName);
        }

        private class AuthoringHandlers
        {
            internal static void HandleBuild(string versionas, bool outputStructured, IConsole console)
            {
                var package = Authoring.PackageToolkit(versionas);
                console.WriteOutput(outputStructured, OutputMessages.CommandLine_Output_BuiltToolkit,
                    package.Toolkit.PatternName, package.Toolkit.Version, package.BuiltLocation);
            }

            internal static void HandleAddCodeTemplateCommand(string codeTemplateName, string name, bool asTearOff,
                string withPath,
                string asChildOf, bool outputStructured, IConsole console)
            {
                var command = Authoring.AddCodeTemplateCommand(codeTemplateName, name, asTearOff, withPath, asChildOf);
                console.WriteOutput(outputStructured, OutputMessages.CommandLine_Output_CodeTemplateCommandAdded,
                    command.Name,
                    command.Id);
            }

            internal static void HandleAddCommandLaunchPoint(string commandIdentifiers, string name, string asChildOf,
                bool outputStructured, IConsole console)
            {
                var launchPoint = Authoring.AddCommandLaunchPoint(commandIdentifiers, name, asChildOf);
                console.WriteOutput(outputStructured, OutputMessages.CommandLine_Output_LaunchPointAdded,
                    launchPoint.Name);
            }

            internal static void HandleAddElement(string name, string displayedAs, string describedAs, string asChildOf,
                bool outputStructured, IConsole console)
            {
                var (parent, element) = Authoring.AddElement(name, displayedAs, describedAs, false,
                    ElementCardinality.Single, asChildOf);
                console.WriteOutput(outputStructured, OutputMessages.CommandLine_Output_ElementAdded, name, parent.Id,
                    element.Id);
            }

            internal static void HandleAddCollection(string name, string displayedAs, string describedAs,
                string asChildOf, ElementCardinality ality, bool outputStructured, IConsole console)
            {
                var (parent, collection) = Authoring.AddElement(name, displayedAs, describedAs, true, ality, asChildOf);
                console.WriteOutput(outputStructured, OutputMessages.CommandLine_Output_CollectionAdded, name,
                    parent.Id, collection.Id);
            }

            internal static void HandleAddAttribute(string name, string isOfType, string defaultValueIs,
                bool isRequired,
                string isOneOf, string asChildOf, bool outputStructured, IConsole console)
            {
                var (parent, attribute) =
                    Authoring.AddAttribute(name, isOfType, defaultValueIs, isRequired, isOneOf, asChildOf);
                console.WriteOutput(outputStructured, OutputMessages.CommandLine_Output_AttributeAdded, name,
                    parent.Id, attribute.Id);
            }

            internal static void HandleCreate(string name, bool outputStructured, IConsole console)
            {
                Authoring.CreateNewPattern(name);
                console.WriteOutput(outputStructured,
                    OutputMessages.CommandLine_Output_PatternCreated, name, Authoring.CurrentPatternId);
            }

            internal static void HandleViewPattern(bool full, bool outputStructured, IConsole console)
            {
                var pattern = Authoring.GetCurrentPattern();

                console.WriteOutput(outputStructured,
                    OutputMessages.CommandLine_Output_ElementsListed, FormatPatternConfiguration(outputStructured, pattern, full));
            }

            internal static void HandleUse(string name, bool outputStructured, IConsole console)
            {
                Authoring.SwitchCurrentPattern(name);
                console.WriteOutput(outputStructured,
                    OutputMessages.CommandLine_Output_PatternSwitched, name, Authoring.CurrentPatternId);
            }

            internal static void HandleAddCodeTemplate(string filepath, string name, string asChildOf,
                bool outputStructured, IConsole console)
            {
                var currentDirectory = Environment.CurrentDirectory;
                var template = Authoring.AttachCodeTemplate(currentDirectory, filepath, name, asChildOf);
                console.WriteOutput(outputStructured,
                    OutputMessages.CommandLine_Output_CodeTemplatedAdded, template.Name,
                    template.Metadata.OriginalFilePath);
            }

            private static string FormatPatternConfiguration(bool outputStructured, PatternDefinition pattern, bool includeAll)
            {
                if (outputStructured)
                {
                    return pattern.ToJson();
                }

                var output = new StringBuilder();
                DisplayDescendantConfiguration(pattern, 0);

                return output.ToString();

                void DisplayDescendantConfiguration(IPatternElement element, int indentLevel)
                {
                    output.Append(indentLevel > 0
                        ? new string('\t', indentLevel)
                        : "");
                    DisplayElement(element, indentLevel);
                }

                void DisplayElement(IPatternElement element, int indentLevel)
                {
                    var subHeadingLevel = indentLevel + 1;
                    var subItemLevel = indentLevel + (includeAll
                        ? 2
                        : 1);

                    output.Append($"- {element.Name}");
                    if (includeAll)
                    {
                        output.Append($" [{element.Id}]");
                    }
                    output.Append(
                        $" ({(element is PatternDefinition ? "root element" : ((Element)element).IsCollection ? "collection" : "element")})");

                    if (element.CodeTemplates.HasAny())
                    {
                        if (includeAll)
                        {
                            output.Append("\n");
                            output.Append(new string('\t', subHeadingLevel));
                            output.Append("- CodeTemplates:\n");
                            element.CodeTemplates.ToListSafe().ForEach(ct => DisplayCodeTemplate(ct, subItemLevel));
                        }
                        else
                        {
                            output.Append($" (attached with {element.CodeTemplates.ToListSafe().Count} code templates)\n");
                        }
                    }
                    else
                    {
                        output.Append("\n");
                    }

                    if (element.Automation.HasAny())
                    {
                        if (includeAll)
                        {
                            output.Append(new string('\t', subHeadingLevel));
                            output.Append("- Automation:\n");
                            element.Automation.ToListSafe().ForEach(auto => DisplayAutomation(auto, subItemLevel));
                        }
                    }

                    if (element.Attributes.HasAny())
                    {
                        if (includeAll)
                        {
                            output.Append(new string('\t', subHeadingLevel));
                            output.Append("- Attributes:\n");
                        }
                        element.Attributes.ToListSafe().ForEach(a => DisplayAttribute(a, subItemLevel));
                    }
                    if (element.Elements.HasAny())
                    {
                        if (includeAll)
                        {
                            output.Append(new string('\t', subHeadingLevel));
                            output.Append("- Elements:\n");
                        }
                        element.Elements.ToListSafe()
                            .ForEach(e => DisplayDescendantConfiguration(e, subItemLevel));
                    }
                }

                void DisplayAttribute(Attribute attribute, int indentLevel)
                {
                    output.Append(new string('\t', indentLevel));
                    output.Append(
                        $"- {attribute.Name}{(includeAll ? "" : " (attribute)")} ({attribute.DataType}{(attribute.IsRequired ? ", required" : "")}{(attribute.Choices.HasAny() ? ", oneof: " + $"{attribute.Choices.ToListSafe().Join(";")}" : "")}{(attribute.DefaultValue.HasValue() ? ", default:" + $"{attribute.DefaultValue}" : "")})\n");
                }

                void DisplayCodeTemplate(CodeTemplate template, int indentLevel)
                {
                    output.Append(new string('\t', indentLevel));
                    output.Append(
                        $"- {template.Name} [{template.Id}] (file: {template.Metadata.OriginalFilePath}, ext: {template.Metadata.OriginalFileExtension})\n");
                }

                void DisplayAutomation(IAutomation automation, int indentLevel)
                {
                    output.Append(new string('\t', indentLevel));
                    output.Append(
                        $"- {automation.Name} [{automation.Id}] ({automation.GetType().Name})");
                    if (automation is CodeTemplateCommand command)
                    {
                        output.Append($" (template: {command.CodeTemplateId}, tearOff: {command.IsTearOff.ToString().ToLower()}, path: {command.FilePath})\n");
                    }
                    else if (automation is CommandLaunchPoint launchPoint)
                    {
                        output.Append($" (ids: {launchPoint.CommandIds.SafeJoin(";")})\n");
                    }
                    else
                    {
                        output.Append("\n");
                    }
                }
            }
        }

        private class RuntimeHandlers
        {
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
                        toolkits.Select(toolkit =>
                            $"{{\"Name\": \"{toolkit.PatternName}\", \"ID\": \"{toolkit.Id}\"}}\n").Join());
                }
                else
                {
                    console.WriteOutput(outputStructured, OutputMessages.CommandLine_Output_NoInstalledToolkits);
                }
            }

            internal static void HandleNewSolution(string name, bool outputStructured, IConsole console)
            {
                var solution = Runtime.CreateSolution(name);
                console.WriteOutput(outputStructured, OutputMessages.CommandLine_Output_CreateSolutionFromToolkit
                    , solution.PatternName, solution.Id);
            }

            internal static void HandleListSolutions(bool outputStructured, IConsole console)
            {
                var solutions = Runtime.ListCreatedSolutions();
                if (solutions.Any())
                {
                    console.WriteOutput(outputStructured, OutputMessages.CommandLine_Output_InstalledSolutionsListed,
                        solutions.Select(solution =>
                            $"{{\"Name\": \"{solution.PatternName}\", \"ID\": \"{solution.Id}\"}}\n").Join());
                }
                else
                {
                    console.WriteOutput(outputStructured, OutputMessages.CommandLine_Output_NoInstalledSolutions);
                }
            }

            internal static void HandleUsing(string solutionId, string add, string addOneTo, string set,
                string[] andSet, string on, bool viewConfiguration,
                bool outputStructured, IConsole console)
            {
                if (!viewConfiguration)
                {
                    var sets = new List<string>();
                    if (set.HasValue())
                    {
                        sets.Add(set);
                    }
                    if (andSet.HasAny())
                    {
                        sets.AddRange(andSet);
                    }

                    var solutionItem = Runtime.ConfigureSolution(solutionId, add, addOneTo, on, sets);
                    console.WriteOutput(outputStructured, OutputMessages.CommandLine_Output_SolutionConfigured, solutionItem.Name, solutionItem.Id);
                }
                else
                {
                    var configuration = Runtime.GetSolutionConfiguration(solutionId);
                    console.WriteOutput(outputStructured, OutputMessages.CommandLine_Output_SolutionConfiguration,
                        configuration);
                }
            }

            internal static void HandleValidate(string solutionId, string on,
                bool outputStructured, IConsole console)
            {
                var errors = Runtime.Validate(solutionId, on);

                if (errors.Count > 0)
                {
                    console.WriteOutputWarning(outputStructured, OutputMessages.CommandLine_Output_SolutionValidationFailed,
                        FormatValidationErrors(errors));
                }
                else
                {
                    console.WriteOutput(outputStructured, OutputMessages.CommandLine_Output_SolutionValidationSuccess);
                }
            }

            internal static void HandleExecuteCommand(string solutionId, string command, string on, bool outputStructured,
                IConsole console)
            {
                var execution = Runtime.ExecuteLaunchPoint(solutionId, command, on);
                if (execution.IsSuccess)
                {
                    console.WriteOutput(outputStructured, OutputMessages.CommandLine_Output_CommandExecuted,
                        execution.CommandName, FormatExecutionLog(execution.Log));
                }
                else
                {
                    console.WriteOutputWarning(outputStructured, OutputMessages.CommandLine_Output_SolutionValidationFailed,
                        FormatValidationErrors(execution.Errors));
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

            private static string FormatExecutionLog(List<string> items)
            {
                var builder = new StringBuilder();
                items
                    .ForEach(item => { builder.AppendLine($"* {item}"); });

                return builder.ToString();
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