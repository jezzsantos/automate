using System.Linq;
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

        public (IAutomation Automation, SolutionItem SolutionItem) FindAutomation(string automationId)
        {
            return FindDescendantAutomation(Model);

            (IAutomation Automation, SolutionItem SolutionItem) FindDescendantAutomation(SolutionItem item)
            {
                if (item.IsPattern)
                {
                    var automation = item.PatternSchema.Automation.Safe()
                        .FirstOrDefault(auto => auto.Id.EqualsIgnoreCase(automationId));
                    if (automation.Exists())
                    {
                        return (automation, item);
                    }
                }
                if (item.IsElement || item.IsCollection)
                {
                    var automation = item.ElementSchema.Automation.Safe()
                        .FirstOrDefault(auto => auto.Id.EqualsIgnoreCase(automationId));
                    if (automation.Exists())
                    {
                        return (automation, item);
                    }
                }

                foreach (var (_, value) in item.Properties.Safe())
                {
                    var result = FindDescendantAutomation(value);
                    if (result.Automation.Exists())
                    {
                        return result;
                    }
                }

                return default;
            }
        }

        private void InitialiseSchema()
        {
            Model = new SolutionItem(Toolkit.Pattern);
        }
    }
}