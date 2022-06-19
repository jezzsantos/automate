using System.Collections.Generic;
using System.Text;
using Automate.Common.Domain;
using Automate.Common.Extensions;

namespace Automate.Authoring.Domain
{
    public class CodeTemplateFile : IPersistable
    {
        public static readonly Encoding Encoding = Encoding.UTF8;

        public CodeTemplateFile(byte[] contents, string id)
        {
            Contents = contents;
            Id = id;
        }

        private CodeTemplateFile(PersistableProperties properties,
            IPersistableFactory factory)
        {
            Id = properties.Rehydrate<string>(factory, nameof(Id));
            Contents = properties.Rehydrate<byte[]>(factory, nameof(Contents));
        }

        public string Id { get; }

        public string CodeTemplateId => Id;

        public IReadOnlyList<byte> Contents { get; private set; }

        public PersistableProperties Dehydrate()
        {
            var properties = new PersistableProperties();
            properties.Dehydrate(nameof(Id), Id);
            properties.Dehydrate(nameof(Contents), Contents);

            return properties;
        }

        public static CodeTemplateFile Rehydrate(PersistableProperties properties,
            IPersistableFactory factory)
        {
            return new CodeTemplateFile(properties, factory);
        }

        public void SetContent(IReadOnlyList<byte> contents)
        {
            contents.GuardAgainstNull(nameof(contents));

            Contents = contents;
        }
    }
}