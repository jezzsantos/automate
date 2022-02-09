using System.Collections.Generic;
using automate.Extensions;

namespace automate.Domain
{
    internal class ToolkitDefinition : IIdentifiableEntity
    {
        public ToolkitDefinition(PatternDefinition pattern, string version)
        {
            pattern.GuardAgainstNull(nameof(pattern));
            version.GuardAgainstNullOrEmpty(nameof(version));

            Id = pattern.Id;
            Pattern = pattern;
            Version = version;
        }

        /// <summary>
        ///     For serialization
        /// </summary>
        public ToolkitDefinition()
        {
        }

        public string Version { get; set; }

        public string PatternName => Pattern.Name;

        public PatternDefinition Pattern { get; set; }

        public List<CodeTemplateFile> CodeTemplateFiles { get; set; }

        public string Id { get; set; }
    }

    internal class CodeTemplateFile
    {
        public string Id { get; set; }

        public byte[] Contents { get; set; }
    }
}