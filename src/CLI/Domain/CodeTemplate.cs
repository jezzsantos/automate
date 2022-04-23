using System;
using System.Collections.Generic;
using Automate.CLI.Extensions;
using ServiceStack;

namespace Automate.CLI.Domain
{
    internal class CodeTemplate : INamedEntity, IPersistable, IPatternVisitable
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
            LastModifiedUtc = DateTime.UtcNow;
            Metadata = new CodeTemplateMetadata
            {
                OriginalFilePath = fullPath,
                OriginalFileExtension = fileExtension
            };
        }

        private CodeTemplate(PersistableProperties properties, IPersistableFactory factory)
        {
            Id = properties.Rehydrate<string>(factory, nameof(Id));
            Name = properties.Rehydrate<string>(factory, nameof(Name));
            LastModifiedUtc = properties.Rehydrate<DateTime>(factory, nameof(LastModifiedUtc));
            Metadata = properties.Rehydrate<Dictionary<string, object>>(factory, nameof(Metadata)).FromObjectDictionary<CodeTemplateMetadata>();
        }

        public CodeTemplateMetadata Metadata { get; }

        public PersistableProperties Dehydrate()
        {
            var properties = new PersistableProperties();
            properties.Dehydrate(nameof(Id), Id);
            properties.Dehydrate(nameof(Name), Name);
            properties.Dehydrate(nameof(LastModifiedUtc), LastModifiedUtc);
            properties.Dehydrate(nameof(Metadata), Metadata.ToObjectDictionary());

            return properties;
        }

        public static CodeTemplate Rehydrate(PersistableProperties properties, IPersistableFactory factory)
        {
            return new CodeTemplate(properties, factory);
        }

        public string Id { get; }

        public string Name { get; }

        public DateTime LastModifiedUtc { get; private set; }

        public bool Accept(IPatternVisitor visitor)
        {
            return visitor.VisitCodeTemplate(this);
        }

        public void UpdateLastModified(DateTime modifiedUtc)
        {
            LastModifiedUtc = modifiedUtc;
        }
    }

    internal class CodeTemplateMetadata
    {
        public string OriginalFilePath { get; set; }

        public string OriginalFileExtension { get; set; }
    }
}