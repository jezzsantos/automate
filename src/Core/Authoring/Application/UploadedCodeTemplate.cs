using Automate.Authoring.Domain;
using Automate.Common.Extensions;

namespace Automate.Authoring.Application
{
    public class UploadedCodeTemplate
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