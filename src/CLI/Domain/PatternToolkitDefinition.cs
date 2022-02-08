using System.Collections.Generic;
using automate.Extensions;

namespace automate.Domain
{
    internal class PatternToolkitDefinition : IIdentifiableEntity
    {
        public PatternToolkitDefinition(PatternDefinition pattern, string version)
        {
            pattern.GuardAgainstNull(nameof(pattern));
            version.GuardAgainstNullOrEmpty(nameof(version));

            Id = IdGenerator.Create();
            Pattern = pattern;
            Version = version;
        }

        /// <summary>
        ///     For serialization
        /// </summary>
        public PatternToolkitDefinition()
        {
        }

        public string Version { get; }

        public string PatternName => Pattern.Name;

        public PatternDefinition Pattern { get; }

        public List<CodeTemplateFile> CodeTemplateFiles { get; set; }

        public string Name { get; set; }

        public string Id { get; set; }
    }

    internal class CodeTemplateFile
    {
        public string Id { get; set; }

        public byte[] Contents { get; set; }
    }
}