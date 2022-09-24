using System.Collections.Generic;
using System.Linq;
using Automate.Common.Domain;
using Automate.Common.Extensions;
using Automate.Runtime.Domain;

namespace Automate.Authoring.Domain
{
    public enum CommandExecutionLogItemType
    {
        Succeeded = 0,
        Warning = 1,
        Failed = 2
    }

    public class CommandExecutionLogItem
    {
        public CommandExecutionLogItem(string message) : this(message, CommandExecutionLogItemType.Succeeded)
        {
        }

        public CommandExecutionLogItem(string message, CommandExecutionLogItemType type)
        {
            Message = message;
            Type = type;
        }

        public string Message { get; }

        public CommandExecutionLogItemType Type { get; }
    }

    public class CommandExecutionResult
    {
        private readonly List<CommandExecutionLogItem> log;

        public CommandExecutionResult(string commandName, CommandExecutableContext executableContext)
        {
            commandName.GuardAgainstNullOrEmpty(nameof(commandName));

            IsSuccess = true;
            CommandName = commandName;
            ExecutableContext = executableContext;
            this.log = new List<CommandExecutionLogItem>();
            ValidationErrors = new ValidationResults();
        }

        public CommandExecutionResult(string commandName, ValidationResults validations)
        {
            commandName.GuardAgainstNullOrEmpty(nameof(commandName));
            validations.GuardAgainstNull(nameof(validations));

            IsSuccess = false;
            CommandName = commandName;
            ExecutableContext = null;
            this.log = new List<CommandExecutionLogItem>();
            ValidationErrors = validations;
        }

        public bool IsSuccess { get; private set; }

        public string CommandName { get; }

        public IReadOnlyList<CommandExecutionLogItem> Log => this.log;

        public ValidationResults ValidationErrors { get; }

        public bool IsInvalid => !IsSuccess && ValidationErrors.HasAny();

        public CommandExecutableContext ExecutableContext { get; }

        public void Fail(string message)
        {
            Fail();
            this.log.Add(new CommandExecutionLogItem(message, CommandExecutionLogItemType.Failed));
        }

        public void RecordSuccess(string message)
        {
            message.GuardAgainstNullOrEmpty(nameof(message));

            this.log.Add(new CommandExecutionLogItem(message, CommandExecutionLogItemType.Succeeded));
        }

        public void Record(IReadOnlyList<CommandExecutionLogItem> items)
        {
            items.GuardAgainstNull(nameof(items));

            this.log.AddRange(items);
        }

        public void Merge(CommandExecutionResult source)
        {
            Record(source.Log);
            if (!source.IsSuccess)
            {
                Fail();
            }
            if (source.ValidationErrors.Any())
            {
                ValidationErrors.AddRange(source.ValidationErrors);
            }
        }

        private void Fail()
        {
            IsSuccess = false;
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