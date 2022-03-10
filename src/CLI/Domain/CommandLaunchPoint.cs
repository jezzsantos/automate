using System;
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

        public override CommandExecutionResult Execute(SolutionDefinition solution, SolutionItem _)
        {
            var outcome = new CommandExecutionResult(Name);

            CommandIds.ToListSafe().ForEach(cmdId =>
            {
                var automation = solution.FindByAutomation(cmdId);
                if (automation.HasNone())
                {
                    throw new AutomateException(ExceptionMessages.CommandLaunchPoint_CommandIdNotFound.Format(cmdId));
                }

                automation.ForEach(auto => ExecuteCommandSafely(auto.Automation, auto.SolutionItem, cmdId));
            });

            return outcome;

            void ExecuteCommandSafely(IAutomation command, SolutionItem solutionItem, string cmdId)
            {
                try
                {
                    var result = command.Execute(solution, solutionItem);
                    outcome.Add(result.Log);
                }
                catch (Exception ex)
                {
                    var message = DomainMessages.CommandLaunchPoint_CommandIdFailedExecution.Format(cmdId, ex.ToMessages(true));
                    outcome.Add(message);
                    outcome.Fail();
                }
            }
        }
    }
}