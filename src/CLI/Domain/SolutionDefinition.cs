using automate.Extensions;

namespace automate.Domain
{
    internal class SolutionDefinition
    {
        public SolutionDefinition(string toolkitId, PatternDefinition pattern)
        {
            toolkitId.GuardAgainstNullOrEmpty(nameof(toolkitId));
            pattern.GuardAgainstNull(nameof(pattern));

            Id = IdGenerator.Create();
            ToolkitId = toolkitId;
            Pattern = pattern;
            InitialiseSchema();
        }

        /// <summary>
        ///     For serialization
        /// </summary>
        public SolutionDefinition()
        {
        }

        public PatternDefinition Pattern { get; set; }

        public string ToolkitId { get; set; }

        public string PatternName => Pattern?.Name;

        public string Id { get; set; }

        public SolutionItem Model { get; set; }

        private void InitialiseSchema()
        {
            Model = new SolutionItem(Pattern);
        }
    }
}