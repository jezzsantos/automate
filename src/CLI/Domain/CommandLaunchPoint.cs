using System;
using System.Collections.Generic;
using System.Linq;
using Automate.CLI.Extensions;

namespace Automate.CLI.Domain
{
    internal class CommandLaunchPoint : IAutomation
    {
        internal const string CommandIdDelimiter = ";";
        private readonly Automation automation;

        public CommandLaunchPoint(string name, List<string> commandIds)
        {
            commandIds.GuardAgainstNull(nameof(commandIds));
            commandIds.GuardAgainstInvalid(ids => ids.Exists() && ids.Any(), nameof(commandIds),
                ValidationMessages.Automation_EmptyCommandIds);
            commandIds.GuardAgainstInvalid(Validations.IsIdentifiers, nameof(commandIds),
                ValidationMessages.Automation_InvalidCommandIds.Format(commandIds.Join(", ")));

            this.automation = new Automation(name, AutomationType.CommandLaunchPoint, new Dictionary<string, object>
            {
                { nameof(CommandIds), commandIds.SafeJoin(CommandIdDelimiter) }
            });
        }

        private CommandLaunchPoint(Automation automation)
        {
            automation.GuardAgainstNull(nameof(automation));
            this.automation = automation;
        }

        public IReadOnlyList<string> CommandIds => this.automation.Metadata[nameof(CommandIds)].ToString().SafeSplit(CommandIdDelimiter).ToList();

        public static CommandLaunchPoint FromAutomation(Automation automation)
        {
            return new CommandLaunchPoint(automation);
        }

        public Automation AsAutomation()
        {
            return this.automation;
        }

        public void AppendCommandIds(List<string> commandIds)
        {
            commandIds.GuardAgainstNull(nameof(commandIds));

            if (commandIds.HasNone())
            {
                return;
            }

            var updated = new List<string>(CommandIds);
            commandIds.ForEach(commandId =>
            {
                if (!CommandIds.Contains(commandId))
                {
                    updated.Add(commandId);
                }
            });
            this.automation.UpdateMetadata(nameof(CommandIds), updated.Join(CommandIdDelimiter));
        }

        public void RemoveCommandId(string commandId)
        {
            commandId.GuardAgainstNullOrEmpty(nameof(commandId));

            var updated = new List<string>(CommandIds);
            if (CommandIds.Contains(commandId))
            {
                updated.Remove(commandId);
            }
            this.automation.UpdateMetadata(nameof(CommandIds), updated.Join(CommandIdDelimiter));
        }

        public string Id => this.automation.Id;

        public string Name => this.automation.Name;

        public CommandExecutionResult Execute(SolutionDefinition solution, SolutionItem _)
        {
            var outcome = new CommandExecutionResult(Name);

            CommandIds.ToListSafe().ForEach(cmdId =>
            {
                var command = solution.FindByAutomation(cmdId);
                if (command.HasNone())
                {
                    throw new AutomateException(ExceptionMessages.CommandLaunchPoint_CommandIdNotFound.Format(cmdId));
                }

                command.ForEach(auto => ExecuteCommandSafely(auto.Automation, auto.SolutionItem, cmdId));
            });

            return outcome;

            void ExecuteCommandSafely(Automation command, SolutionItem solutionItem, string cmdId)
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