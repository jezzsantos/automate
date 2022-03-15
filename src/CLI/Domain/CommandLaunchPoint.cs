using System;
using System.Collections.Generic;
using System.Linq;
using Automate.CLI.Extensions;

namespace Automate.CLI.Domain
{
    internal class CommandLaunchPoint : IAutomation
    {
        public CommandLaunchPoint(string id, string name, IReadOnlyDictionary<string, object> metadata) : this(id, name, metadata[nameof(CommandIds)].ToString().SafeSplit(";").ToList())
        {
        }

        public CommandLaunchPoint(string id, string name, List<string> commandIds)
        {
            id.GuardAgainstNullOrEmpty(nameof(id));
            id.GuardAgainstInvalid(IdGenerator.IsValid, nameof(id),
                ValidationMessages.InvalidIdentifier);
            name.GuardAgainstNullOrEmpty(nameof(name));
            name.GuardAgainstInvalid(Validations.IsNameIdentifier, nameof(name),
                ValidationMessages.InvalidNameIdentifier);
            commandIds.GuardAgainstNull(nameof(commandIds));
            commandIds.GuardAgainstInvalid(ids => ids.Exists() && ids.Any(), nameof(commandIds),
                ValidationMessages.Automation_EmptyCommandIds);
            commandIds.GuardAgainstInvalid(Validations.IsIdentifiers, nameof(commandIds),
                ValidationMessages.Automation_InvalidCommandIds.Format(commandIds.Join(", ")));
            Id = id;
            Name = name;
            CommandIds = commandIds;
        }

        public List<string> CommandIds { get; }

        public string Id { get; }

        public string Name { get; }

        public CommandExecutionResult Execute(SolutionDefinition solution, SolutionItem _)
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