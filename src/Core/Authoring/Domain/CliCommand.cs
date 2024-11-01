﻿using System.Collections.Generic;
using Automate.Common.Domain;
using Automate.Common.Extensions;

namespace Automate.Authoring.Domain
{
    public class CliCommand : IAutomation
    {
        private readonly Automation automation;

        public CliCommand(string name, string applicationName,
            string arguments) : this(
            new Automation(name, AutomationType.CliCommand, new Dictionary<string, object>
            {
                { nameof(ApplicationName), applicationName },
                { nameof(Arguments), arguments }
            }))
        {
            applicationName.GuardAgainstNull(nameof(applicationName));
        }

        public CliCommand(Automation automation)
        {
            automation.GuardAgainstNull(nameof(automation));

            this.automation = automation;
        }

        public string ApplicationName => this.automation.Metadata
            .GetValueOrDefault(nameof(ApplicationName), string.Empty)?.ToString();

        public string Arguments =>
            this.automation.Metadata.GetValueOrDefault(nameof(Arguments), string.Empty)?.ToString();

        public static CliCommand FromAutomation(Automation automation)
        {
            return new CliCommand(automation);
        }

        public Automation AsAutomation()
        {
            return this.automation;
        }

        public void ChangeArguments(string arguments)
        {
            this.automation.UpdateMetadata(nameof(Arguments), arguments);
        }

        public void ChangeApplicationName(string applicationName)
        {
            this.automation.UpdateMetadata(nameof(ApplicationName), applicationName);
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