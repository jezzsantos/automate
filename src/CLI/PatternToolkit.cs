using System.Collections.Generic;
using automate.Extensions;

namespace automate
{
    internal class PatternToolkit
    {
        public PatternToolkit(PatternMetaModel pattern, string version)
        {
            pattern.GuardAgainstNull(nameof(pattern));
            version.GuardAgainstNullOrEmpty(nameof(version));

            Pattern = pattern;
            Version = version;
        }

        /// <summary>
        ///     For serialization
        /// </summary>
        public PatternToolkit()
        {
        }

        public string Version { get; }

        public string PatternName => Pattern.Name;

        public PatternMetaModel Pattern { get; }

        public List<CodeTemplateFile> CodeTemplateFiles { get; set; }
    }

    internal class CodeTemplateFile
    {
        public string Id { get; set; }

        public byte[] Contents { get; set; }
    }
}