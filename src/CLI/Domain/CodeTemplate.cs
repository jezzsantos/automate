using automate.Extensions;

namespace automate.Domain
{
    internal class CodeTemplate : INamedEntity
    {
        public CodeTemplate(string name, string fullPath)
        {
            name.GuardAgainstNullOrEmpty(nameof(name));
            name.GuardAgainstInvalid(Validations.IsNameIdentifier, nameof(name),
                ValidationMessages.InvalidNameIdentifier);
            fullPath.GuardAgainstNullOrEmpty(nameof(fullPath));
            Id = IdGenerator.Create();
            Name = name;
            Metadata = new CodeTemplateMetadata
            {
                OriginalFilePath = fullPath
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
    }
}