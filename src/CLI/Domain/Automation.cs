﻿using automate.Extensions;

namespace automate.Domain
{
    internal abstract class Automation : IAutomation
    {
        protected Automation(string name)
        {
            name.GuardAgainstNullOrEmpty(nameof(name));
            name.GuardAgainstInvalid(Validations.IsNameIdentifier, nameof(name),
                ValidationMessages.InvalidNameIdentifier);

            Id = IdGenerator.Create();
            Name = name;
        }

        /// <summary>
        ///     For serialization
        /// </summary>
        protected Automation()
        {
        }

        public string Name { get; set; }

        public abstract CommandExecutionResult Execute(ToolkitDefinition toolkit, SolutionItem ownerSolution);

        public string Id { get; set; }
    }
}