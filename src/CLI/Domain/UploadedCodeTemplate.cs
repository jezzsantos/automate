using Automate.CLI.Extensions;

namespace Automate.CLI.Domain
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