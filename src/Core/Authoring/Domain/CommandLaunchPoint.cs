using System.Collections.Generic;
using System.Linq;
using Automate.Common;
using Automate.Common.Domain;
using Automate.Common.Extensions;

namespace Automate.Authoring.Domain
{
    public class CommandLaunchPoint : IAutomation
    {
        public const string CommandIdDelimiter = ";";
        private readonly Automation automation;

        public CommandLaunchPoint(string name, List<string> commandIds)
        {
            commandIds.GuardAgainstNull(nameof(commandIds));
            commandIds.GuardAgainstInvalid(ids => ids.Exists() && ids.Any(), nameof(commandIds),
                ValidationMessages.Automation_EmptyCommandIds);
            commandIds.GuardAgainstInvalid(Validations.IsIdentifiers, nameof(commandIds),
                ValidationMessages.Automation_InvalidCommandIds.Substitute(commandIds.Join(", ")));

            this.automation = new Automation(name, AutomationType.CommandLaunchPoint,
                new Dictionary<string, object>
                {
                    { nameof(CommandIds), commandIds.SafeJoin(CommandIdDelimiter) }
                });
        }

        private CommandLaunchPoint(Automation automation)
        {
            automation.GuardAgainstNull(nameof(automation));
            this.automation = automation;
        }

        public IReadOnlyList<string> CommandIds => this.automation
            .Metadata.GetValueOrDefault(nameof(CommandIds), string.Empty).ToString()
            .SafeSplit(CommandIdDelimiter).ToList();

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
                if (!updated.Contains(commandId))
                {
                    updated.Add(commandId);
                }
            });
            this.automation.UpdateMetadata(nameof(CommandIds), updated.Join(CommandIdDelimiter));
        }

        public void ChangeCommandIds(List<string> commandIdsToAdd, List<string> commandIdsToRemove)
        {
            commandIdsToAdd.GuardAgainstNull(nameof(commandIdsToAdd));
            commandIdsToRemove.GuardAgainstNull(nameof(commandIdsToRemove));

            if (commandIdsToAdd.HasNone()
                && commandIdsToRemove.HasNone())
            {
                return;
            }

            var updated = new List<string>(CommandIds);
            commandIdsToRemove.ForEach(commandId =>
            {
                if (updated.Contains(commandId))
                {
                    updated.Remove(commandId);
                }
            });
            commandIdsToAdd.ForEach(commandId =>
            {
                if (!updated.Contains(commandId))
                {
                    updated.Add(commandId);
                }
            });

            if (updated.Count == 0)
            {
                throw new AutomateException(ExceptionMessages.CommandLaunchPoint_NoCommandIds);
            }

            this.automation.UpdateMetadata(nameof(CommandIds), updated.Join(CommandIdDelimiter));
        }

        public void RemoveCommandId(string commandId)
        {
            commandId.GuardAgainstNullOrEmpty(nameof(commandId));

            var updated = new List<string>(CommandIds);
            if (updated.Contains(commandId))
            {
                updated.Remove(commandId);
            }
            this.automation.UpdateMetadata(nameof(CommandIds), updated.Join(CommandIdDelimiter));
        }

        public void ChangeName(string name)
        {
            this.automation.Rename(name);
        }

        public string Id => this.automation.Id;

        public string Name => this.automation.Name;

        public AutomationType Type => this.automation.Type;

        public bool IsLaunchable => this.automation.IsLaunchable;
    }
}