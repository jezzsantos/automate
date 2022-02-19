using System.Linq;
using Automate.CLI.Extensions;
using StringExtensions = ServiceStack.StringExtensions;

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
            return Model.GetConfiguration().ToJson();
        }

        public CommandExecutionResult ExecuteCommand(string name)
        {
            var command =
                Toolkit.Pattern.Automation.Safe().FirstOrDefault(
                    automation => StringExtensions.EqualsIgnoreCase(automation.Name, name));
            if (command.NotExists())
            {
                throw new AutomateException(
                    ExceptionMessages.SolutionDefinition_UnknownCommand.Format(name, PatternName));
            }

            return command.Execute(Toolkit, Model);
        }

        private void InitialiseSchema()
        {
            Model = new SolutionItem(Toolkit.Pattern);
        }
    }
}