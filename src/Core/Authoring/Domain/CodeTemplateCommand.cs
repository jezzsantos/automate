﻿using System.Collections.Generic;
using Automate.Common.Domain;
using Automate.Common.Extensions;

namespace Automate.Authoring.Domain
{
    public class CodeTemplateCommand : IAutomation
    {
        private readonly Automation automation;

        public CodeTemplateCommand(string name, string codeTemplateId, bool isOneOff,
            string filePath) : this(new Automation(name, AutomationType.CodeTemplateCommand,
            new Dictionary<string, object>
            {
                { nameof(CodeTemplateId), codeTemplateId },
                { nameof(IsOneOff), isOneOff },
                { nameof(FilePath), filePath }
            }))
        {
            codeTemplateId.GuardAgainstNullOrEmpty(nameof(codeTemplateId));
            filePath.GuardAgainstNullOrEmpty(nameof(filePath));
            filePath.GuardAgainstInvalid(Validations.IsRuntimeFilePath, nameof(filePath),
                ValidationMessages.Automation_InvalidFilePath);
        }

        private CodeTemplateCommand(Automation automation)
        {
            automation.GuardAgainstNull(nameof(automation));
            this.automation = automation;
        }

        public string CodeTemplateId =>
            this.automation.Metadata.GetValueOrDefault(nameof(CodeTemplateId), string.Empty).ToString();

        public string FilePath => this.automation.Metadata.GetValueOrDefault(nameof(FilePath), string.Empty).ToString();

        public bool IsOneOff => this.automation.Metadata.GetValueOrDefault(nameof(IsOneOff), false).ToString().ToBool();

        public static CodeTemplateCommand FromAutomation(Automation automation)
        {
            return new CodeTemplateCommand(automation);
        }

        public Automation AsAutomation()
        {
            return this.automation;
        }

        public void ChangeName(string name)
        {
            this.automation.Rename(name);
        }

        public void ChangeOneOff(bool isOneOff)
        {
            this.automation.UpdateMetadata(nameof(IsOneOff), isOneOff);
        }

        public void ChangeFilePath(string filePath)
        {
            filePath.GuardAgainstNullOrEmpty(nameof(filePath));
            filePath.GuardAgainstInvalid(Validations.IsRuntimeFilePath, nameof(filePath),
                ValidationMessages.Automation_InvalidFilePath);

            this.automation.UpdateMetadata(nameof(FilePath), filePath);
        }

        public string Id => this.automation.Id;

        public string Name => this.automation.Name;

        public AutomationType Type => this.automation.Type;

        public bool IsLaunchable => this.automation.IsLaunchable;
    }
}