using System.Collections.Generic;
using Automate.CLI.Extensions;

namespace Automate.CLI.Domain
{
    internal class CommandExecutionResult
    {
        public CommandExecutionResult(string commandName) : this(commandName, new List<string>())
        {
        }

        public CommandExecutionResult(string commandName, List<string> log)
        {
            commandName.GuardAgainstNullOrEmpty(nameof(commandName));
            log.GuardAgainstNull(nameof(log));

            IsSuccess = true;
            CommandName = commandName;
            Log = log;
            ValidationErrors = new ValidationResults();
        }

        public CommandExecutionResult(string commandName, ValidationResults validations)
        {
            commandName.GuardAgainstNullOrEmpty(nameof(commandName));
            validations.GuardAgainstNull(nameof(validations));

            IsSuccess = false;
            CommandName = commandName;
            Log = new List<string>();
            ValidationErrors = validations;
        }

        public bool IsSuccess { get; private set; }

        public string CommandName { get; }

        public List<string> Log { get; }

        public ValidationResults ValidationErrors { get; }

        public bool IsInvalid => !IsSuccess && ValidationErrors.HasAny();

        public void Fail()
        {
            IsSuccess = false;
        }

        public void Add(string message)
        {
            message.GuardAgainstNullOrEmpty(nameof(message));

            Log.Add(message);
        }

        public void Add(List<string> messages)
        {
            messages.GuardAgainstNull(nameof(messages));

            Log.AddRange(messages);
        }
    }
}