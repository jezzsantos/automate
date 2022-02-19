using System.Collections.Generic;
using Automate.CLI.Extensions;

namespace Automate.CLI.Domain
{
    internal class CommandExecutionResult
    {
        public CommandExecutionResult(string commandName, List<string> log)
        {
            commandName.GuardAgainstNullOrEmpty(nameof(commandName));
            log.GuardAgainstNull(nameof(log));

            IsSuccess = true;
            CommandName = commandName;
            Log = log;
            Errors = new ValidationResults();
        }

        public CommandExecutionResult(string commandName, ValidationResults validations)
        {
            commandName.GuardAgainstNullOrEmpty(nameof(commandName));
            validations.GuardAgainstNull(nameof(validations));

            IsSuccess = false;
            CommandName = commandName;
            Log = new List<string>();
            Errors = validations;
        }

        public bool IsSuccess { get; }

        public string CommandName { get; }

        public List<string> Log { get; }

        public ValidationResults Errors { get; }
    }
}