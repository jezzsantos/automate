﻿using System;
using System.Collections.Generic;
using System.Linq;
using Automate.CLI.Extensions;

namespace Automate.CLI.Domain
{
    internal abstract class PatternElement : IPatternElement
    {
        internal const string LaunchPointSelectionWildcard = "*";
        private readonly List<Attribute> attributes;
        private readonly List<Automation> automations;
        private readonly List<CodeTemplate> codeTemplates;
        private readonly List<Element> elements;

        protected PatternElement(string name)
        {
            name.GuardAgainstNullOrEmpty(nameof(name));
            name.GuardAgainstInvalid(Validations.IsNameIdentifier, nameof(name),
                ValidationMessages.InvalidNameIdentifier);

            Id = IdGenerator.Create();
            Name = name;
            Parent = null;
            this.codeTemplates = new List<CodeTemplate>();
            this.automations = new List<Automation>();
            this.attributes = new List<Attribute>();
            this.elements = new List<Element>();
        }

        protected PatternElement(PersistableProperties properties, IPersistableFactory factory)
        {
            Id = properties.Rehydrate<string>(factory, nameof(Id));
            Name = properties.Rehydrate<string>(factory, nameof(Name));
            this.attributes = properties.Rehydrate<List<Attribute>>(factory, nameof(Attributes));
            this.elements = properties.Rehydrate<List<Element>>(factory, nameof(Elements));
            this.automations = properties.Rehydrate<List<Automation>>(factory, nameof(Automation));
            this.codeTemplates = properties.Rehydrate<List<CodeTemplate>>(factory, nameof(CodeTemplates));
        }

        internal PatternElement Parent { get; private set; }

        internal PatternDefinition Pattern => GetRoot();

        public virtual PersistableProperties Dehydrate()
        {
            var properties = new PersistableProperties();
            properties.Dehydrate(nameof(Id), Id);
            properties.Dehydrate(nameof(Name), Name);
            properties.Dehydrate(nameof(Attributes), Attributes);
            properties.Dehydrate(nameof(Elements), Elements);
            properties.Dehydrate(nameof(CodeTemplates), CodeTemplates);
            properties.Dehydrate(nameof(Automation), Automation);

            return properties;
        }

        public void AddAttribute(Attribute attribute)
        {
            this.attributes.Add(attribute);
            RecordChange(VersionChange.NonBreaking, VersionChanges.PatternElement_Attribute_Add, attribute.Id, Id);
        }

        public void DeleteAttribute(Attribute attribute)
        {
            this.attributes.Remove(attribute);
            RecordChange(VersionChange.Breaking, VersionChanges.PatternElement_Attribute_Delete, attribute.Id, Id);
        }

        public void AddElement(Element element)
        {
            element.SetParent(this);
            this.elements.Add(element);
            RecordChange(VersionChange.NonBreaking, VersionChanges.PatternElement_Element_Add, element.Id, Id);
        }

        public void DeleteElement(Element element)
        {
            this.elements.Remove(element);
            RecordChange(VersionChange.Breaking, VersionChanges.PatternElement_Element_Delete, element.Id, Id);
        }

        public void AddCodeTemplate(CodeTemplate codeTemplate)
        {
            this.codeTemplates.Add(codeTemplate);
            RecordChange(VersionChange.NonBreaking, VersionChanges.PatternElement_CodeTemplate_Add, codeTemplate.Id, Id);
        }

        public void DeleteCodeTemplate(CodeTemplate codeTemplate)
        {
            this.codeTemplates.Remove(codeTemplate);
            RecordChange(VersionChange.Breaking, VersionChanges.PatternElement_CodeTemplate_Delete, codeTemplate.Id, Id);
        }

        public void AddAutomation(Automation automation)
        {
            this.automations.Add(automation);
            RecordChange(VersionChange.NonBreaking, VersionChanges.PatternElement_Automation_Add, automation.Id, Id);
        }

        public void DeleteCodeTemplateCommand(CodeTemplateCommand command)
        {
            this.automations.Remove(command.AsAutomation());
            RecordChange(VersionChange.Breaking, VersionChanges.PatternElement_CodeTemplateCommand_Delete, command.Id, Id);
        }

        public void DeleteCommandLaunchPoint(CommandLaunchPoint launchPoint)
        {
            this.automations.Remove(launchPoint.AsAutomation());
            RecordChange(VersionChange.Breaking, VersionChanges.PatternElement_CommandLaunchPoint_Delete, launchPoint.Id, Id);
        }

        public void SetParent(PatternElement parent)
        {
            Parent = parent;
        }

        public Attribute AddAttribute(string name, string type, bool isRequired, string defaultValue, List<string> choices)
        {
            name.GuardAgainstNullOrEmpty(nameof(name));

            if (AttributeNameIsReserved(name))
            {
                throw new AutomateException(ExceptionMessages.PatternElement_AttributeNameReserved.Format(name));
            }

            if (AttributeExistsByName(this, name))
            {
                throw new AutomateException(ExceptionMessages.PatternElement_AttributeByNameExists.Format(name));
            }

            if (ElementExistsByName(this, name))
            {
                throw new AutomateException(ExceptionMessages.PatternElement_AttributeByNameExistsAsElement.Format(name));
            }

            var attribute = new Attribute(name, type, isRequired, defaultValue, choices);
            AddAttribute(attribute);

            return attribute;
        }

        public Attribute DeleteAttribute(string name)
        {
            name.GuardAgainstNullOrEmpty(nameof(name));

            var attribute = GetAttributeByName(this, name);
            if (attribute.NotExists())
            {
                throw new AutomateException(ExceptionMessages.PatternElement_AttributeByNameNotExists.Format(name));
            }

            DeleteAttribute(attribute);

            return attribute;
        }

        public Element AddElement(string name, string displayName, string description, bool isCollection, ElementCardinality cardinality)
        {
            name.GuardAgainstNullOrEmpty(nameof(name));

            if (ElementExistsByName(this, name))
            {
                throw new AutomateException(ExceptionMessages.PatternElement_ElementByNameExists.Format(name));
            }

            if (AttributeExistsByName(this, name))
            {
                throw new AutomateException(ExceptionMessages.PatternElement_ElementByNameExistsAsAttribute.Format(name));
            }

            var element = new Element(name, displayName, description, isCollection, cardinality);
            AddElement(element);

            return element;
        }

        public Element DeleteElement(string name)
        {
            name.GuardAgainstNullOrEmpty(nameof(name));

            var element = GetElementByName(this, name);
            if (element.NotExists())
            {
                throw new AutomateException(ExceptionMessages.PatternElement_ElementByNameNotExists.Format(name));
            }

            DeleteElement(element);

            return element;
        }

        public CodeTemplate AddCodeTemplate(string name, string fullPath, string extension)
        {
            fullPath.GuardAgainstNullOrEmpty(nameof(fullPath));
            extension.GuardAgainstNullOrEmpty(nameof(extension));

            var templateName = name.HasValue()
                ? name
                : $"CodeTemplate{CodeTemplates.ToListSafe().Count + 1}";
            if (CodeTemplateExistsByName(this, templateName))
            {
                throw new AutomateException(ExceptionMessages.PatternElement_CodeTemplateByNameExists
                    .Format(name));
            }

            var codeTemplate = new CodeTemplate(templateName, fullPath, extension);
            AddCodeTemplate(codeTemplate);

            return codeTemplate;
        }

        public void DeleteCodeTemplate(string id, bool includeReferencingAutomation)
        {
            id.GuardAgainstNullOrEmpty(nameof(id));

            var codeTemplate = this.codeTemplates.FirstOrDefault(temp => temp.Id == id);
            if (codeTemplate.NotExists())
            {
                throw new AutomateException(ExceptionMessages.PatternElement_CodeTemplateNotExistsById.Format(id));
            }

            DeleteCodeTemplate(codeTemplate);

            if (includeReferencingAutomation)
            {
                Automation
                    .Where(auto => auto.Type == AutomationType.CodeTemplateCommand)
                    .Select(CodeTemplateCommand.FromAutomation)
                    .Where(cmd => cmd.CodeTemplateId == codeTemplate.Id)
                    .ToList()
                    .ForEach(cmd => { DeleteCodeTemplateCommand(cmd.Id, true); });
            }
        }

        public Automation AddCodeTemplateCommand(string name, string codeTemplateName, bool isTearOff, string filePath)
        {
            codeTemplateName.GuardAgainstNull(nameof(codeTemplateName));
            filePath.GuardAgainstNull(nameof(filePath));

            var codeTemplate = CodeTemplates.FirstOrDefault(ele => ele.Name.EqualsIgnoreCase(codeTemplateName));
            if (codeTemplate.NotExists())
            {
                throw new AutomateException(ExceptionMessages.PatternElement_CodeTemplateNotFound.Format(codeTemplateName));
            }

            var templateName = name.HasValue()
                ? name
                : $"CodeTemplateCommand{Automation.ToListSafe().Count + 1}";
            if (AutomationExistsByName(this, templateName))
            {
                throw new AutomateException(
                    ExceptionMessages.PatternElement_AutomationByNameExists.Format(templateName));
            }

            var automation = new CodeTemplateCommand(templateName, codeTemplate.Id, isTearOff, filePath).AsAutomation();
            AddAutomation(automation);

            return automation;
        }

        public Automation UpdateCodeTemplateCommand(string commandName, string name, bool? isTearOff, string filePath)
        {
            var automation = GetAutomationByName(this, commandName, auto => auto.Type == AutomationType.CodeTemplateCommand);
            if (automation.NotExists())
            {
                throw new AutomateException(
                    ExceptionMessages.PatternElement_AutomationNotExistsByName.Format(AutomationType.CodeTemplateCommand, commandName));
            }
            var command = CodeTemplateCommand.FromAutomation(automation);
            if (name.HasValue())
            {
                command.ChangeName(name);
            }
            if (isTearOff.HasValue)
            {
                command.ChangeTearOff(isTearOff.Value);
            }
            if (filePath.HasValue())
            {
                command.ChangeFilePath(filePath);
            }
            RecordChange(VersionChange.NonBreaking, VersionChanges.PatternElement_Automation_Update, command.Id, Id);

            return command.AsAutomation();
        }

        public void DeleteCodeTemplateCommand(string id, bool includeReferencingAutomation)
        {
            id.GuardAgainstNullOrEmpty(nameof(id));

            var command = FindCodeTemplateCommand(id);
            if (command.NotExists())
            {
                throw new AutomateException(ExceptionMessages.PatternElement_AutomationNotExistsById.Format(AutomationType.CodeTemplateCommand, id));
            }

            DeleteCodeTemplateCommand(CodeTemplateCommand.FromAutomation(command));

            if (includeReferencingAutomation)
            {
                Pattern.GetAllLaunchPoints()
                    .Where(pair => pair.LaunchPoint.CommandIds.Contains(command.Id))
                    .ToList().ForEach(pair =>
                    {
                        pair.LaunchPoint.RemoveCommandId(id);
                        if (pair.LaunchPoint.CommandIds.HasNone())
                        {
                            pair.Parent.DeleteCommandLaunchPoint(pair.LaunchPoint.Id);
                        }
                    });
            }
        }

        public Automation AddCommandLaunchPoint(string name, List<string> commandIds)
        {
            commandIds.GuardAgainstNull(nameof(commandIds));

            if (commandIds.HasNone())
            {
                throw new AutomateException(ExceptionMessages.PatternElement_NoCommandIds);
            }
            if (commandIds.Count == 1
                && commandIds.First() == LaunchPointSelectionWildcard)
            {
                commandIds.Clear();
                commandIds.AddRange(GetAllLaunchableAutomation(Automation));
            }
            else
            {
                commandIds.ForEach(cmdId =>
                {
                    var command = Pattern.FindAutomation(cmdId, auto => auto.IsLaunchable());
                    if (command.NotExists())
                    {
                        throw new AutomateException(ExceptionMessages.PatternElement_CommandIdNotFound.Format(cmdId));
                    }
                });
            }

            var launchPointName = name.HasValue()
                ? name
                : $"LaunchPoint{Automation.ToListSafe().Count + 1}";
            if (AutomationExistsByName(this, launchPointName))
            {
                throw new AutomateException(
                    ExceptionMessages.PatternElement_AutomationByNameExists.Format(launchPointName));
            }

            var automation = new CommandLaunchPoint(launchPointName, commandIds).AsAutomation();
            AddAutomation(automation);

            return automation;
        }

        public Automation UpdateCommandLaunchPoint(string launchPointName, List<string> commandIds, IPatternElement sourceElement)
        {
            launchPointName.GuardAgainstNullOrEmpty(nameof(launchPointName));
            commandIds.GuardAgainstNull(nameof(commandIds));

            if (commandIds.HasNone())
            {
                throw new AutomateException(ExceptionMessages.PatternElement_NoCommandIds);
            }
            if (commandIds.Count == 1
                && commandIds.First() == LaunchPointSelectionWildcard)
            {
                commandIds.Clear();
                commandIds.AddRange(GetAllLaunchableAutomation(sourceElement.Automation));
            }
            else
            {
                commandIds.ForEach(cmdId =>
                {
                    var command = Pattern.FindAutomation(cmdId, auto => auto.IsLaunchable());
                    if (command.NotExists())
                    {
                        throw new AutomateException(ExceptionMessages.PatternElement_CommandIdNotFound.Format(cmdId));
                    }
                });
            }

            var automation = GetAutomationByName(this, launchPointName, auto => auto.Type == AutomationType.CommandLaunchPoint);
            if (automation.NotExists())
            {
                throw new AutomateException(
                    ExceptionMessages.PatternElement_AutomationNotExistsByName.Format(AutomationType.CommandLaunchPoint, launchPointName));
            }
            var launchPoint = CommandLaunchPoint.FromAutomation(automation);
            launchPoint.AppendCommandIds(commandIds);
            RecordChange(VersionChange.NonBreaking, VersionChanges.PatternElement_Automation_Update, launchPoint.Id, Id);

            return launchPoint.AsAutomation();
        }

        public void DeleteCommandLaunchPoint(string id)
        {
            id.GuardAgainstNullOrEmpty(nameof(id));

            var launchPoint = FindLaunchPoint(id);
            if (launchPoint.NotExists())
            {
                throw new AutomateException(ExceptionMessages.PatternElement_AutomationNotExistsById.Format(AutomationType.CommandLaunchPoint, id));
            }

            DeleteCommandLaunchPoint(CommandLaunchPoint.FromAutomation(launchPoint));
        }

        public string Id { get; }

        public string Name { get; }

        public IReadOnlyList<Element> Elements => this.elements;

        public IReadOnlyList<Attribute> Attributes => this.attributes;

        public IReadOnlyList<CodeTemplate> CodeTemplates => this.codeTemplates;

        public IReadOnlyList<Automation> Automation => this.automations;

        private static List<string> GetAllLaunchableAutomation(IReadOnlyList<Automation> automation)
        {
            return automation
                .Where(auto => auto.IsLaunchable())
                .Select(auto => auto.Id)
                .ToList();
        }

        private Automation FindCodeTemplateCommand(string id)
        {
            return Automation.FirstOrDefault(cmd => cmd.Id == id && cmd.Type == AutomationType.CodeTemplateCommand);
        }

        private Automation FindLaunchPoint(string id)
        {
            return Automation.FirstOrDefault(cmd => cmd.Id == id && cmd.Type == AutomationType.CommandLaunchPoint);
        }

        private void RecordChange(VersionChange change, string description, params object[] args)
        {
            var pattern = Pattern;
            if (pattern.NotExists())
            {
                return;
            }

            if (this == Pattern)
            {
                pattern.ToolkitVersion.RegisterChange(change, description, args);
            }
            else
            {
                pattern.RecordChange(change, description, args);
            }
        }

        private static bool AttributeExistsByName(IAttributeContainer element, string attributeName)
        {
            return GetAttributeByName(element, attributeName).Exists();
        }

        private static Attribute GetAttributeByName(IAttributeContainer element, string attributeName)
        {
            return element.Attributes.Safe().FirstOrDefault(attr => attr.Name.EqualsIgnoreCase(attributeName));
        }

        private static bool AttributeNameIsReserved(string attributeName)
        {
            return Attribute.ReservedAttributeNames.Any(reserved => reserved.EqualsIgnoreCase(attributeName));
        }

        private static bool ElementExistsByName(IElementContainer element, string elementName)
        {
            return element.Elements.Safe().Any(ele => ele.Name.EqualsIgnoreCase(elementName));
        }

        private static Element GetElementByName(IElementContainer element, string elementName)
        {
            return element.Elements.Safe().FirstOrDefault(ele => ele.Name.EqualsIgnoreCase(elementName));
        }

        private static bool AutomationExistsByName(IAutomationContainer element, string automationName)
        {
            return element.Automation.Safe().Any(ele => ele.Name.EqualsIgnoreCase(automationName));
        }

        private static Automation GetAutomationByName(IAutomationContainer element, string name, Predicate<Automation> where)
        {
            return element.Automation.Safe()
                .FirstOrDefault(auto => auto.Name.EqualsIgnoreCase(name) && where(auto));
        }

        private static bool CodeTemplateExistsByName(IAutomationContainer element, string templateName)
        {
            return element.CodeTemplates.Safe().Any(template => template.Name.EqualsIgnoreCase(templateName));
        }

        private PatternDefinition GetRoot()
        {
            return Parent.NotExists()
                ? this as PatternDefinition
                : Parent.GetRoot();
        }
    }
}