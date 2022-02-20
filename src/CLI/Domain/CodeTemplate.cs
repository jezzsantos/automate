using Automate.CLI.Extensions;

namespace Automate.CLI.Domain
{
    internal class CodeTemplate : INamedEntity
    {
        public CodeTemplate(string name, string fullPath, string fileExtension)
        {
            name.GuardAgainstNullOrEmpty(nameof(name));
            name.GuardAgainstInvalid(Validations.IsNameIdentifier, nameof(name),
                ValidationMessages.InvalidNameIdentifier);
            fullPath.GuardAgainstNullOrEmpty(nameof(fullPath));
            fileExtension.GuardAgainstNullOrEmpty(nameof(fileExtension));
            Id = IdGenerator.Create();
            Name = name;
            Metadata = new CodeTemplateMetadata
            {
                OriginalFilePath = fullPath,
                OriginalFileExtension = fileExtension
            };
        }

        /// <summary>
        ///     For serialization
        /// </summary>
        public CodeTemplate()
        {
        }

        public CodeTemplateMetadata Metadata { get; set; }

        public string Id { get; set; }

        public string Name { get; set; }
    }

    internal class CodeTemplateMetadata
    {
        public string OriginalFilePath { get; set; }

        public string OriginalFileExtension { get; set; }
    }
}