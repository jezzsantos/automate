using System;
using System.Collections.Generic;
using Automate.Common.Domain;
using Automate.Common.Extensions;

namespace Automate.Authoring.Domain
{
    public class CodeTemplate : INamedEntity, IPersistable, IPatternVisitable
    {
        private static readonly string[] ReservedCodeTemplateNames =
        {
            nameof(INamedEntity.Id)
        };
        public CodeTemplate(string name, string fullPath, string fileExtension)
        {
            name.GuardAgainstNullOrEmpty(nameof(name));
            name.GuardAgainstInvalid(Validations.IsNameIdentifier, nameof(name),
                ValidationMessages.InvalidNameIdentifier);
            name.GuardAgainstInvalid(x => Validations.IsNotReservedName(x, ReservedCodeTemplateNames), nameof(name),
                ValidationMessages.CodeTemplate_ReservedName);
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

        private CodeTemplate(PersistableProperties properties,
            IPersistableFactory factory)
        {
            Id = properties.Rehydrate<string>(factory, nameof(Id));
            Name = properties.Rehydrate<string>(factory, nameof(Name));
            LastModifiedUtc = properties.Rehydrate<DateTime>(factory, nameof(LastModifiedUtc));
            Metadata = properties.Rehydrate<Dictionary<string, object>>(factory, nameof(Metadata))
                .FromObjectDictionary<CodeTemplateMetadata>();
        }

        public CodeTemplateMetadata Metadata { get; }

        public DateTime LastModifiedUtc { get; private set; }

        public PersistableProperties Dehydrate()
        {
            var properties = new PersistableProperties();
            properties.Dehydrate(nameof(Id), Id);
            properties.Dehydrate(nameof(Name), Name);
            properties.Dehydrate(nameof(LastModifiedUtc), LastModifiedUtc);
            properties.Dehydrate(nameof(Metadata), Metadata.ToObjectDictionary());

            return properties;
        }

        public static CodeTemplate Rehydrate(PersistableProperties properties,
            IPersistableFactory factory)
        {
            return new CodeTemplate(properties, factory);
        }

        public void UpdateLastModified(DateTime modifiedUtc)
        {
            LastModifiedUtc = modifiedUtc;
        }

        public string Id { get; }

        public string Name { get; }

        public bool Accept(IPatternVisitor visitor)
        {
            return visitor.VisitCodeTemplate(this);
        }
    }

    public class CodeTemplateMetadata
    {
        public string OriginalFilePath { get; set; }

        public string OriginalFileExtension { get; set; }
    }

    public class CodeTemplateContent
    {
        public byte[] Content { get; set; }

        public DateTime LastModifiedUtc { get; set; }
    }
}