using automate.Extensions;

namespace automate.Domain
{
    internal class SolutionDefinition
    {
        public SolutionDefinition(string toolkitId, string toolkitName)
        {
            toolkitId.GuardAgainstNullOrEmpty(nameof(toolkitId));
            toolkitName.GuardAgainstNullOrEmpty(nameof(toolkitName));

            Id = IdGenerator.Create();
            ToolkitId = toolkitId;
            PatternName = toolkitName;
        }

        /// <summary>
        ///     For serialization
        /// </summary>
        public SolutionDefinition()
        {
        }

        public string ToolkitId { get; set; }

        public string PatternName { get; set; }

        public string Id { get; set; }
    }
}