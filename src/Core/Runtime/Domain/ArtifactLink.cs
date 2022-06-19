using Automate.Common.Domain;
using Automate.Common.Extensions;

namespace Automate.Runtime.Domain
{
    public class ArtifactLink : IIdentifiableEntity, IPersistable
    {
        public ArtifactLink(string commandId, string path, string tag = null)
        {
            commandId.GuardAgainstNullOrEmpty(nameof(commandId));
            path.GuardAgainstNullOrEmpty(nameof(path));
            Id = IdGenerator.Create();
            CommandId = commandId;
            Path = path;
            Tag = tag;
        }

        private ArtifactLink(PersistableProperties properties,
            IPersistableFactory factory)
        {
            Id = properties.Rehydrate<string>(factory, nameof(Id));
            Tag = properties.Rehydrate<string>(factory, nameof(Tag));
            CommandId = properties.Rehydrate<string>(factory, nameof(CommandId));
            Path = properties.Rehydrate<string>(factory, nameof(Path));
        }

        public string Tag { get; private set; }

        public string CommandId { get; }

        public string Path { get; private set; }

        public PersistableProperties Dehydrate()
        {
            var properties = new PersistableProperties();
            properties.Dehydrate(nameof(Id), Id);
            properties.Dehydrate(nameof(Tag), Tag);
            properties.Dehydrate(nameof(CommandId), CommandId);
            properties.Dehydrate(nameof(Path), Path);

            return properties;
        }

        public void UpdatePathAndTag(string path, string tag)
        {
            path.GuardAgainstNullOrEmpty(nameof(path));
            Path = path;
            Tag = tag;
        }

        public static ArtifactLink Rehydrate(PersistableProperties properties,
            IPersistableFactory factory)
        {
            return new ArtifactLink(properties, factory);
        }

        public string Id { get; }
    }
}