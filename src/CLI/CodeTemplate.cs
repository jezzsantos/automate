using automate.Extensions;

namespace automate
{
    internal class CodeTemplate : INamedEntity
    {
        public CodeTemplate(string name, string fullPath)
        {
            name.GuardAgainstNullOrEmpty(nameof(name));
            name.GuardAgainstInvalid(Validations.IsIdentifier, nameof(name),
                ExceptionMessages.Validations_InvalidIdentifier);
            fullPath.GuardAgainstNullOrEmpty(nameof(fullPath));
            Id = IdGenerator.Create();
            Name = name;
            FullPath = fullPath;
        }

        /// <summary>
        ///     For serialization
        /// </summary>
        public CodeTemplate()
        {
        }

        public string FullPath { get; set; }

        public string Id { get; set; }

        public string Name { get; set; }
    }
}