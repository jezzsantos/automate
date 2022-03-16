using System;
using System.Collections.Generic;
using Automate.CLI.Extensions;

namespace Automate.CLI.Domain
{
    internal class ToolkitDefinition : IIdentifiableEntity, IPersistable
    {
        private List<CodeTemplateFile> codeTemplateFiles;

        public ToolkitDefinition(PatternDefinition pattern, Version version = null)
        {
            pattern.GuardAgainstNull(nameof(pattern));

            Id = pattern.Id;
            Pattern = pattern;
            Version = version.Exists()
                ? version.ToString(ToolkitVersion.VersionFieldCount)
                : ToolkitVersion.InitialVersionNumber.ToString(ToolkitVersion.VersionFieldCount);
        }

        private ToolkitDefinition(PersistableProperties properties, IPersistableFactory factory)
        {
            Id = properties.Rehydrate<string>(factory, nameof(Id));
            Version = properties.Rehydrate<string>(factory, nameof(Version));
            Pattern = properties.Rehydrate<PatternDefinition>(factory, nameof(Pattern));
            this.codeTemplateFiles = properties.Rehydrate<List<CodeTemplateFile>>(factory, nameof(CodeTemplateFiles));
        }

        public string Version { get; }

        public string PatternName => Pattern.Name;

        public PatternDefinition Pattern { get; }

        public IReadOnlyList<CodeTemplateFile> CodeTemplateFiles => this.codeTemplateFiles;

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
            this.codeTemplateFiles = files;
        }

        public string Id { get; }
    }
}