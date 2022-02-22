using Automate.CLI.Extensions;

namespace Automate.CLI.Domain
{
    internal class SolutionDefinition
    {
        public SolutionDefinition(ToolkitDefinition toolkit)
        {
            toolkit.GuardAgainstNull(nameof(toolkit));

            Id = IdGenerator.Create();
            Toolkit = toolkit;
            InitialiseSchema();
        }

        /// <summary>
        ///     For serialization
        /// </summary>
        public SolutionDefinition()
        {
        }

        public ToolkitDefinition Toolkit { get; set; }

        public string PatternName => Toolkit.Pattern?.Name;

        public string Id { get; set; }

        public SolutionItem Model { get; set; }

        public string GetConfiguration()
        {
            return Model.GetConfiguration(false).ToJson();
        }

        private void InitialiseSchema()
        {
            Model = new SolutionItem(Toolkit.Pattern);
        }
    }
}