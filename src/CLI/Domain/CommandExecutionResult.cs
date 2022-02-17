using System.Collections.Generic;
using automate.Extensions;

namespace automate.Domain
{
    internal class CommandExecutionResult
    {
        public CommandExecutionResult(string commandName, List<string> log)
        {
            commandName.GuardAgainstNullOrEmpty(nameof(commandName));
            log.GuardAgainstNull(nameof(log));

            CommandName = commandName;
            Log = log;
        }

        public string CommandName { get; }

        public List<string> Log { get; }
    }
}