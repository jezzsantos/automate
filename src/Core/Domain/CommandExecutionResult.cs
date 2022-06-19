using System.Collections.Generic;
using Automate.Extensions;

namespace Automate.Domain
{
    public class CommandExecutionResult
    {
        private readonly List<string> log;

        public CommandExecutionResult(string commandName) : this(commandName, null as CommandExecutableContext)
        {
        }

        public CommandExecutionResult(string commandName, CommandExecutableContext executableContext)
        {
            commandName.GuardAgainstNullOrEmpty(nameof(commandName));

            IsSuccess = true;
            CommandName = commandName;
            ExecutableContext = executableContext;
            this.log = new List<string>();
            ValidationErrors = new ValidationResults();
        }

        public CommandExecutionResult(string commandName, ValidationResults validations)
        {
            commandName.GuardAgainstNullOrEmpty(nameof(commandName));
            validations.GuardAgainstNull(nameof(validations));

            IsSuccess = false;
            CommandName = commandName;
            ExecutableContext = null;
            this.log = new List<string>();
            ValidationErrors = validations;
        }

        public bool IsSuccess { get; private set; }

        public string CommandName { get; }

        public IReadOnlyList<string> Log => this.log;

        public ValidationResults ValidationErrors { get; }

        public bool IsInvalid => !IsSuccess && ValidationErrors.HasAny();

        public CommandExecutableContext ExecutableContext { get; }

        public void Fail()
        {
            IsSuccess = false;
        }

        public void Fail(string message)
        {
            Fail();
            Record(message);
        }

        public void Record(string message)
        {
            message.GuardAgainstNullOrEmpty(nameof(message));

            this.log.Add(message);
        }

        public void Record(IReadOnlyList<string> messages)
        {
            messages.GuardAgainstNull(nameof(messages));

            this.log.AddRange(messages);
        }
    }

    public class CommandExecutableContext
    {
        public CommandExecutableContext(IAutomation executable, DraftDefinition draft, DraftItem item)
        {
            executable.GuardAgainstNull(nameof(executable));
            draft.GuardAgainstNull(nameof(draft));
            item.GuardAgainstNull(nameof(item));
            Executable = executable;
            Draft = draft;
            Item = item;
        }

        public IAutomation Executable { get; }

        public DraftDefinition Draft { get; }

        public DraftItem Item { get; }
    }
}