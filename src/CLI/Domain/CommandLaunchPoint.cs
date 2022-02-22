using System.Collections.Generic;
using System.Linq;
using Automate.CLI.Extensions;

namespace Automate.CLI.Domain
{
    internal class CommandLaunchPoint : Automation
    {
        public CommandLaunchPoint(string name, List<string> commandIds) : base(name)
        {
            commandIds.GuardAgainstNull(nameof(commandIds));
            commandIds.GuardAgainstInvalid(ids => ids.Exists() && ids.Any(), nameof(commandIds),
                ValidationMessages.Automation_EmptyCommandIds);
            commandIds.GuardAgainstInvalid(Validations.IsIdentifiers, nameof(commandIds),
                ValidationMessages.Automation_InvalidCommandIds.Format(commandIds.Join(", ")));
            CommandIds = commandIds;
        }

        /// <summary>
        ///     For serialization
        /// </summary>
        public CommandLaunchPoint()
        {
        }

        public List<string> CommandIds { get; set; }

        public override CommandExecutionResult Execute(ToolkitDefinition toolkit, SolutionItem item)
        {
            var logs = new List<string>();

            var automations = item.GetAutomation();
            automations
                .Where(auto => CommandIds.Contains(auto.Id))
                .ToList()
                .ForEach(cmd =>
                {
                    var result = cmd.Execute(toolkit, item);
                    logs.AddRange(result.Log);
                });

            return new CommandExecutionResult(Name, logs);
        }
    }
}