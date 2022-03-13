using System.Collections.Generic;
using Automate.CLI.Extensions;

namespace Automate.CLI.Domain
{
    internal class ToolkitDefinition : IIdentifiableEntity, IPersistable
    {
        public ToolkitDefinition(PatternDefinition pattern, string version)
        {
            pattern.GuardAgainstNull(nameof(pattern));
            version.GuardAgainstNullOrEmpty(nameof(version));

            Id = pattern.Id;
            Pattern = pattern;
            Version = version;
        }

        private ToolkitDefinition(PersistableProperties properties, IPersistableFactory factory)
        {
            Id = properties.Rehydrate<string>(factory, nameof(Id));
            Version = properties.Rehydrate<string>(factory, nameof(Version));
            Pattern = properties.Rehydrate<PatternDefinition>(factory, nameof(Pattern));
            CodeTemplateFiles = properties.Rehydrate<List<CodeTemplateFile>>(factory, nameof(CodeTemplateFiles));
        }

        public string Version { get; }

        public string PatternName => Pattern.Name;

        public PatternDefinition Pattern { get; }

        public List<CodeTemplateFile> CodeTemplateFiles { get; private set; }

        public PersistableProperties Dehydrate()
        {
            var properties = new PersistableProperties();
            properties.Dehydrate(nameof(Id), Id);
            properties.Dehydrate(nameof(Version), Version);
            properties.Dehydrate(nameof(Pattern), Pattern);
            properties.Dehydrate(nameof(CodeTemplateFiles), CodeTemplateFiles);

            return properties;
        }

        public static ToolkitDefinition Rehydrate(PersistableProperties properties, IPersistableFactory factory)
        {
            return new ToolkitDefinition(properties, factory);
        }

        public void AddCodeTemplateFiles(List<CodeTemplateFile> files)
        {
            CodeTemplateFiles = files;
        }

        public string Id { get; }
    }
}