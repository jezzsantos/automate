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
                    ? ExceptionMessages.CommandLine_Output_PatternInUse.Format(application.CurrentPatternName,
                        application.CurrentPatternId)
                    : ExceptionMessages.CommandLine_Output_NoPatternSelected);
                Console.WriteLine();

                var command = new RootCommand
                {
                    new Command("create", "To create a new pattern")
                    {
                        new Argument("Name", "The name of the pattern to create")
                    }.WithHandler(nameof(HandleCreate)),
                    new Command("use", "To use an existing pattern")
                    {
                        new Argument("Name", "The name of the existing pattern to use")
                    }.WithHandler(nameof(HandleUse)),
                    new Command("add-codetemplate", "To add a code template to an element")
                    {
                        new Argument("FilePath", "A relative path to the code file, from the current directory"),
                        new Option("--name", "A friendly name for the code template",
                            arity: ArgumentArity.ZeroOrOne)
                    }.WithHandler(nameof(HandleAddCodeTemplate)),
                    new Command("list-codetemplates", "List the code templates for this pattern")
                        .WithHandler(nameof(HandleListCodeTemplate))
                };
                command.Description = "Create and Run automated patterns";

                return command.Invoke(args);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                return 1;
            }
        }

        private static void HandleCreate(string name, IConsole console)
        {
            application.CreateNewPattern(name);
            console.WriteLine(
                ExceptionMessages.CommandLine_Output_PatternCreated.Format(name, application.CurrentPatternId));
        }

        private static void HandleUse(string name, IConsole console)
        {
            application.SwitchCurrentPattern(name);
            console.WriteLine(
                ExceptionMessages.CommandLine_Output_PatternSwitched.Format(name, application.CurrentPatternId));
        }

        private static void HandleAddCodeTemplate(string filepath, string name, IConsole console)
        {
            var currentDirectory = Environment.CurrentDirectory;
            var template = application.AttachCodeTemplate(currentDirectory, filepath, name);
            console.WriteLine(
                ExceptionMessages.CommandLine_Output_CodeTemplatedAdded.Format(template.Name, template.FullPath));
        }

        private static void HandleListCodeTemplate(IConsole console)
        {
            var templates = application.ListCodeTemplates();
            if (templates.Count == 0)
            {
                console.WriteLine(ExceptionMessages.CommandLine_Output_NoCodeTemplates);
            }
            else
            {
                console.WriteLine(string.Format(ExceptionMessages.CommandLine_Output_NoCodeTemplatesListed,
                    templates.Count));
                templates.ForEach(template => console.WriteLine($"{template.Name}"));
            }
        }
    }
}