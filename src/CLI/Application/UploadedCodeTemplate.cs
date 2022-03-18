using Automate.CLI.Domain;
using Automate.CLI.Extensions;

namespace Automate.CLI.Application
{
    internal class UploadedCodeTemplate
    {
        public UploadedCodeTemplate(CodeTemplate codeTemplate, string location)
        {
            codeTemplate.GuardAgainstNull(nameof(codeTemplate));
            location.GuardAgainstNullOrEmpty(nameof(location));
            Template = codeTemplate;
            Location = location;
        }

        public string Location { get; }

        public CodeTemplate Template { get; }
    }
}