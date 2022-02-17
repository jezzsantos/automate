using automate.Extensions;

namespace automate.Domain
{
    internal class ArtifactLink : IIdentifiableEntity
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

        /// <summary>
        ///     For serialization
        /// </summary>
        public ArtifactLink()
        {
        }

        public string Tag { get; set; }

        public string CommandId { get; set; }

        public string Path { get; set; }

        public void UpdatePathAndTag(string path, string tag)
        {
            path.GuardAgainstNullOrEmpty(nameof(path));
            Path = path;
            Tag = tag;
        }

        public string Id { get; set; }
    }
}