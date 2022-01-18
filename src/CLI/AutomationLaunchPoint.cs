using System.Collections.Generic;
using System.Linq;
using automate.Extensions;

namespace automate
{
    internal class AutomationLaunchPoint : Automation
    {
        public AutomationLaunchPoint(string name, List<string> commandIds) : base(name)
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
        public AutomationLaunchPoint()
        {
        }

        public List<string> CommandIds { get; set; }
    }
}