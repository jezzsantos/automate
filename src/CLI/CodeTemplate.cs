using System.Collections.Generic;
using automate.Extensions;

namespace automate
{
    internal class CodeTemplate : INamedEntity
    {
        public const string OriginalPathMetadataName = "OriginalFilePath";

        public CodeTemplate(string name, string fullPath)
        {
            name.GuardAgainstNullOrEmpty(nameof(name));
            name.GuardAgainstInvalid(Validations.IsNameIdentifier, nameof(name),
                ValidationMessages.InvalidNameIdentifier);
            fullPath.GuardAgainstNullOrEmpty(nameof(fullPath));
            Id = IdGenerator.Create();
            Name = name;
            Metadata = new Dictionary<string, string>
            {
                { OriginalPathMetadataName, fullPath }
            };
        }

        /// <summary>
        ///     For serialization
        /// </summary>
        public CodeTemplate()
        {
        }

        public Dictionary<string, string> Metadata { get; set; }

        public string Id { get; set; }

        public string Name { get; set; }
    }
}