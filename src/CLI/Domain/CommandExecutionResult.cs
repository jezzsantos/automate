using System.Collections.Generic;
using Automate.CLI.Extensions;

namespace Automate.CLI.Domain
{
    internal class CommandExecutionResult
    {
        private readonly List<string> log;

        public CommandExecutionResult(string commandName) : this(commandName, new List<string>())
        {
        }

        public CommandExecutionResult(string commandName, List<string> log)
        {
            commandName.GuardAgainstNullOrEmpty(nameof(commandName));
            log.GuardAgainstNull(nameof(log));

            IsSuccess = true;
            CommandName = commandName;
            this.log = log;
            ValidationErrors = new ValidationResults();
        }

        public CommandExecutionResult(string commandName, ValidationResults validations)
        {
            commandName.GuardAgainstNullOrEmpty(nameof(commandName));
            validations.GuardAgainstNull(nameof(validations));

            IsSuccess = false;
            CommandName = commandName;
            this.log = new List<string>();
            ValidationErrors = validations;
        }

        public bool IsSuccess { get; private set; }

        public string CommandName { get; }

        public IReadOnlyList<string> Log => this.log;

        public ValidationResults ValidationErrors { get; }

        public bool IsInvalid => !IsSuccess && ValidationErrors.HasAny();

        public void Fail()
        {
            IsSuccess = false;
        }

        public void Add(string message)
        {
            message.GuardAgainstNullOrEmpty(nameof(message));

            this.log.Add(message);
        }

        public void Add(IReadOnlyList<string> messages)
        {
            messages.GuardAgainstNull(nameof(messages));

            this.log.AddRange(messages);
        }
    }
}