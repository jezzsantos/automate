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
        public const string TestCommandName = "test";
        public const string ListCommandName = "list";
        public const string ConfigureCommandName = "configure";
        public const string ValidateCommandName = "validate";
        public const string ExecuteCommandName = "execute";
        public const string ViewCommandName = "view";
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
            var testCommands = new Command(TestCommandName, "Testing automation of a pattern")
            {
                new Command("codetemplate", "Tests the code template")
                {
                    new Argument("Name", "The name of the code template"),
                    new Option("--aschildof", "The expression of the element/collection on which the code template exists",
                        typeof(string),
                        arity: ArgumentArity.ZeroOrOne)
                }.WithHandler<AuthoringHandlers>(nameof(AuthoringHandlers.HandleTestCodeTemplate))
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
                new Command("toolkit", "Creates a new solution from a toolkit")
                {
                    new Argument("PatternName", "The name of the pattern in the toolkit that you want to use")
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
                new Command("command", "Executes the command on the solution")
                {
                    new Argument("Name", "The name of the command to execute"),
                    new Option("--on", "The expression of the element/collection containing the command to execute", arity: ArgumentArity.ZeroOrOne)
                }.WithHandler<RuntimeHandlers>(nameof(RuntimeHandlers.HandleExecuteCommand))
            };
            var viewCommands = new Command(ViewCommandName, "Viewing patterns and solutions")
            {
                new Command("pattern", "View the configuration of the pattern")
                {
                    new Option("--all", "Include additional configuration, like automation and code templates", typeof(bool), () => false, ArgumentArity.ZeroOrOne)
                }.WithHandler<AuthoringHandlers>(nameof(AuthoringHandlers.HandleViewPattern)),
                new Command("solution", "View the configuration of the solution")
                {
                    new Option("--todo", "Displays the details of the pattern, and any validation errors", typeof(bool), () => false, ArgumentArity.ZeroOrOne)
                }.WithHandler<RuntimeHandlers>(nameof(RuntimeHandlers.HandleViewConfiguration))
            };
            var listCommands = new Command(ListCommandName, "Listing patterns, toolkits and solutions")
            {
                new Command("toolkits", "Lists all installed toolkits")
                    .WithHandler<RuntimeHandlers>(nameof(RuntimeHandlers.HandleListToolkits)),
                new Command("solutions", "Lists all created solutions")
                    .WithHandler<RuntimeHandlers>(nameof(RuntimeHandlers.HandleListSolutions))
            };

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
                        OutputMessages.CommandLine_Output_CurrentPatternInUse.FormatTemplate(Authoring.CurrentPatternName,
                            Authoring.CurrentPatternId), ConsoleColor.Gray);
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
                            OutputMessages.CommandLine_Output_CurrentSolutionInUse.FormatTemplate(Runtime.CurrentSolutionId), ConsoleColor.Gray);
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

            internal static void HandleViewPattern(bool all, bool outputStructured, IConsole console)
            {
                var pattern = Authoring.GetCurrentPattern();

                console.WriteOutput(outputStructured,
                    OutputMessages.CommandLine_Output_ElementsListed, FormatPatternConfiguration(outputStructured, pattern, all));
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
                var template = Authoring.AttachCodeTemplate(currentDirectory, filepath, name, asChildOf);
                console.WriteOutput(outputStructured,
                    OutputMessages.CommandLine_Output_CodeTemplatedAdded, template.Name,
                    template.Metadata.OriginalFilePath);
            }

            internal static void HandleTestCodeTemplate(string name, string asChildOf, bool outputStructured, IConsole console)
            {
                var output = Authoring.TestCodeTemplate(name, asChildOf);
                console.WriteOutput(outputStructured, OutputMessages.CommandLine_Output_CodeTemplateTested, name, output);
            }

            internal static string FormatPatternConfiguration(bool outputStructured, PatternDefinition pattern, bool includeAll)
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
                        $"- {attribute.Name}{(includeAll ? "" : " (attribute)")} ({attribute.DataType}{(attribute.IsRequired ? ", required" : "")}{(attribute.Choices.HasAny() ? ", oneof: " + $"{attribute.Choices.ToListSafe().Join(";")}" : "")}{(attribute.DefaultValue.HasValue() ? ", default: " + $"{attribute.DefaultValue}" : "")})\n");
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

            internal static void HandleNewSolution(string patternName, bool outputStructured, IConsole console)
            {
                var solution = Runtime.CreateSolution(patternName);
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

            internal static void HandleSwitch(string solutionId, bool outputStructured, IConsole console)
            {
                Runtime.SwitchCurrentSolution(solutionId);
                console.WriteOutput(outputStructured,
                    OutputMessages.CommandLine_Output_SolutionSwitched, solutionId, Runtime.CurrentSolutionId);
            }

            internal static void HandleAddTo(string expression, string[] andSet, bool outputStructured, IConsole console)
            {
                var sets = new List<string>();
                if (andSet.HasAny())
                {
                    sets.AddRange(andSet);
                }

                var solutionItem = Runtime.ConfigureSolution(expression, null, null, sets);
                console.WriteOutput(outputStructured, OutputMessages.CommandLine_Output_SolutionConfigured, solutionItem.Name, solutionItem.Id);
            }

            internal static void HandleAddOneTo(string expression, string[] andSet, bool outputStructured, IConsole console)
            {
                var sets = new List<string>();
                if (andSet.HasAny())
                {
                    sets.AddRange(andSet);
                }

                var solutionItem = Runtime.ConfigureSolution(null, expression, null, sets);
                console.WriteOutput(outputStructured, OutputMessages.CommandLine_Output_SolutionConfigured, solutionItem.Name, solutionItem.Id);
            }

            internal static void HandleSet(string expression, string[] andSet, bool outputStructured, IConsole console)
            {
                var sets = new List<string>();
                if (andSet.HasAny())
                {
                    sets.AddRange(andSet);
                }

                var solutionItem = Runtime.ConfigureSolution(null, null, expression, sets);
                console.WriteOutput(outputStructured, OutputMessages.CommandLine_Output_SolutionConfigured, solutionItem.Name, solutionItem.Id);
            }

            internal static void HandleViewConfiguration(bool todo, bool outputStructured, IConsole console)
            {
                var (configuration, pattern, validation) = Runtime.GetConfiguration(todo, todo);

                console.WriteOutput(outputStructured, OutputMessages.CommandLine_Output_SolutionConfiguration,
                    configuration);

                if (todo)
                {
                    console.WriteOutputLine();
                    console.WriteOutput(outputStructured,
                        OutputMessages.CommandLine_Output_ElementsListed, AuthoringHandlers.FormatPatternConfiguration(outputStructured, pattern, true));
                }

                if (todo)
                {
                    console.WriteOutputLine();
                    if (validation.HasAny())
                    {
                        console.WriteOutputWarning(outputStructured, OutputMessages.CommandLine_Output_SolutionValidationFailed,
                            FormatValidationErrors(validation));
                    }
                    else
                    {
                        console.WriteOutput(outputStructured, OutputMessages.CommandLine_Output_SolutionValidationSuccess);
                    }
                }
            }

            internal static void HandleValidate(string on,
                bool outputStructured, IConsole console)
            {
                var results = Runtime.Validate(on);

                if (results.HasAny())
                {
                    console.WriteOutputWarning(outputStructured, OutputMessages.CommandLine_Output_SolutionValidationFailed,
                        FormatValidationErrors(results));
                }
                else
                {
                    console.WriteOutput(outputStructured, OutputMessages.CommandLine_Output_SolutionValidationSuccess);
                }
            }

            internal static void HandleExecuteCommand(string name, string on, bool outputStructured,
                IConsole console)
            {
                var execution = Runtime.ExecuteLaunchPoint(name, on);
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