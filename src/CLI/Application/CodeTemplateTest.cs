using Automate.CLI.Domain;
using Automate.CLI.Extensions;

namespace Automate.CLI.Application
{
    internal class CodeTemplateTest
    {
        public CodeTemplateTest(CodeTemplate template, string output)
        {
            template.GuardAgainstNull(nameof(template));
            output.GuardAgainstNullOrEmpty(nameof(output));

            Template = template;
            Output = output;
        }

        public CodeTemplate Template { get; }

        public string Output { get; }
    }
}