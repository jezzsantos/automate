using Automate.Domain;
using Automate.Extensions;

namespace Automate.Application
{
    public class CodeTemplateTest
    {
        public CodeTemplateTest(CodeTemplate template, string output, string exportedFilePath = null)
        {
            template.GuardAgainstNull(nameof(template));
            output.GuardAgainstNullOrEmpty(nameof(output));

            Template = template;
            Output = output;
            ExportedFilePath = exportedFilePath;
        }

        public CodeTemplate Template { get; }

        public string Output { get; }

        public string ExportedFilePath { get; }
    }

    public class CodeTemplateCommandTest
    {
        public CodeTemplateCommandTest(CodeTemplateCommand command, string output, string exportedFilePath = null)
        {
            command.GuardAgainstNull(nameof(command));
            output.GuardAgainstNullOrEmpty(nameof(output));

            Command = command;
            Output = output;
            ExportedFilePath = exportedFilePath;
        }

        public CodeTemplateCommand Command { get; }

        public string Output { get; }

        public string ExportedFilePath { get; }
    }
}