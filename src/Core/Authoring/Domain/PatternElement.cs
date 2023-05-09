using System;
using System.Collections.Generic;
using System.Linq;
using Automate.Common;
using Automate.Common.Domain;
using Automate.Common.Extensions;

namespace Automate.Authoring.Domain
{
    public abstract class PatternElement : IPatternElement, IPatternVisitable
    {
        internal const string LaunchPointSelectionWildcard = "*";
        private readonly List<Attribute> attributes;
        private readonly List<Automation> automations;
        private readonly ICollectionAutoNamer autoNamer;
        private readonly List<CodeTemplate> codeTemplates;
        private readonly List<Element> elements;

        protected PatternElement(string name, string displayName, string description)
        {
            name.GuardAgainstNullOrEmpty(nameof(name));
            name.GuardAgainstInvalid(Validations.IsNameIdentifier, nameof(name),
                ValidationMessages.InvalidNameIdentifier);

            Id = IdGenerator.Create();
            Name = name;
            DisplayName = displayName ?? name;
            Description = description;
            Parent = null;
            this.codeTemplates = new List<CodeTemplate>();
            this.automations = new List<Automation>();
            this.attributes = new List<Attribute>();
            this.elements = new List<Element>();
            this.autoNamer = new CollectionAutoNamer();
        }

        protected PatternElement(PersistableProperties properties,
            IPersistableFactory factory)
        {
            Id = properties.Rehydrate<string>(factory, nameof(Id));
            Name = properties.Rehydrate<string>(factory, nameof(Name));
            DisplayName = properties.Rehydrate<string>(factory, nameof(DisplayName));
            Description = properties.Rehydrate<string>(factory, nameof(Description));
            this.attributes = properties.Rehydrate<List<Attribute>>(factory, nameof(Attributes));
            this.elements = properties.Rehydrate<List<Element>>(factory, nameof(Elements));
            this.automations = properties.Rehydrate<List<Automation>>(factory, nameof(Automation));
            this.codeTemplates = properties.Rehydrate<List<CodeTemplate>>(factory, nameof(CodeTemplates));
            this.autoNamer = new CollectionAutoNamer();
        }

        internal PatternElement Parent { get; private set; }

        internal PatternDefinition Pattern => GetRoot();

        public string DisplayName { get; protected set; }

        public string Description { get; protected set; }

        public string EditPath => GetEditPath();

        public virtual PersistableProperties Dehydrate()
        {
            var properties = new PersistableProperties();
            properties.Dehydrate(nameof(Id), Id);
            properties.Dehydrate(nameof(Name), Name);
            properties.Dehydrate(nameof(DisplayName), DisplayName);
            properties.Dehydrate(nameof(Description), Description);
            properties.Dehydrate(nameof(Attributes), Attributes);
            properties.Dehydrate(nameof(Elements), Elements);
            properties.Dehydrate(nameof(CodeTemplates), CodeTemplates);
            properties.Dehydrate(nameof(Automation), Automation);

            return properties;
        }

        public void AddAttribute(Attribute attribute)
        {
            attribute.SetParent(this);
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
            RecordChange(VersionChange.NonBreaking, VersionChanges.PatternElement_CodeTemplate_Add, codeTemplate.Id,
                Id);
        }

        public void DeleteCodeTemplate(CodeTemplate codeTemplate)
        {
            this.codeTemplates.Remove(codeTemplate);
            RecordChange(VersionChange.Breaking, VersionChanges.PatternElement_CodeTemplate_Delete, codeTemplate.Id,
                Id);
        }

        public void AddAutomation(Automation automation)
        {
            automation.SetParent(this);
            this.automations.Add(automation);
            RecordChange(VersionChange.NonBreaking, VersionChanges.PatternElement_Automation_Add, automation.Id, Id);
        }

        public void DeleteCodeTemplateCommand(CodeTemplateCommand command)
        {
            this.automations.Remove(command.AsAutomation());
            RecordChange(VersionChange.Breaking, VersionChanges.PatternElement_CodeTemplateCommand_Delete, command.Id,
                Id);
        }

        public void DeleteCliCommand(CliCommand command)
        {
            this.automations.Remove(command.AsAutomation());
            RecordChange(VersionChange.Breaking, VersionChanges.PatternElement_CliCommand_Delete, command.Id, Id);
        }

        public void DeleteCommandLaunchPoint(CommandLaunchPoint service)
        {
            this.automations.Remove(service.AsAutomation());
            RecordChange(VersionChange.Breaking, VersionChanges.PatternElement_CommandLaunchPoint_Delete,
                service.Id, Id);
        }

        public void SetParent(PatternElement parent)
        {
            Parent = parent;
        }

        public void RecordChange(VersionChange change, string description, params object[] args)
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

        public void RenameAndDescribe(string name, string displayName = null, string description = null)
        {
            if (name.IsNotNull())
            {
                SetName(name);
                if (displayName.HasNoValue())
                {
                    SetDisplayName(name.ToCapitalizedWords());
                }
            }

            if (displayName.IsNotNull())
            {
                SetDisplayName(displayName);
            }

            if (description.IsNotNull())
            {
                SetDescription(description);
            }

            void SetName(string value)
            {
                value.GuardAgainstInvalid(Validations.IsNameIdentifier, nameof(name),
                    ValidationMessages.InvalidNameIdentifier);
                if (value.NotEqualsOrdinal(Name))
                {
                    Name = value;
                    RecordChange(VersionChange.Breaking, VersionChanges.Pattern_Name_Updated, Id);
                }
            }

            void SetDisplayName(string value)
            {
                value.GuardAgainstNullOrEmpty(nameof(displayName));
                if (value.NotEqualsOrdinal(DisplayName))
                {
                    DisplayName = value;
                    RecordChange(VersionChange.NonBreaking, VersionChanges.Pattern_DisplayName_Updated,
                        Id);
                }
            }

            void SetDescription(string value)
            {
                value.GuardAgainstNullOrEmpty(nameof(description));
                if (value.NotEqualsOrdinal(Description))
                {
                    Description = value;
                    RecordChange(VersionChange.NonBreaking, VersionChanges.Pattern_Description_Updated,
                        Id);
                }
            }
        }

        public Attribute AddAttribute(string name, string type = Attribute.DefaultType, bool isRequired = false,
            string defaultValue = null, List<string> choices = null)
        {
            name.GuardAgainstNullOrEmpty(nameof(name));

            if (AttributeNameIsReserved(name))
            {
                throw new AutomateException(ExceptionMessages.PatternElement_AttributeNameReserved.Substitute(name));
            }

            if (AttributeExistsByName(this, name))
            {
                throw new AutomateException(ExceptionMessages.PatternElement_AttributeByNameExists.Substitute(name));
            }

            if (ElementExistsByName(this, name))
            {
                throw new AutomateException(
                    ExceptionMessages.PatternElement_AttributeByNameExistsAsElement.Substitute(name));
            }

            var attribute = new Attribute(name, type, isRequired, defaultValue, choices);
            AddAttribute(attribute);

            return attribute;
        }

        public Attribute UpdateAttribute(string attributeName, string name = null, string type = null,
            bool? isRequired = false, string defaultValue = null, List<string> choices = null)
        {
            attributeName.GuardAgainstNullOrEmpty(nameof(attributeName));

            var attribute = GetAttributeByName(this, attributeName);
            if (attribute.NotExists())
            {
                throw new AutomateException(
                    ExceptionMessages.PatternElement_AttributeByNameNotExists.Substitute(attributeName));
            }

            if (name.HasValue())
            {
                if (name.NotEqualsIgnoreCase(attribute.Name))
                {
                    if (AttributeNameIsReserved(name))
                    {
                        throw new AutomateException(ExceptionMessages.PatternElement_AttributeNameReserved
                            .Substitute(name));
                    }

                    if (AttributeExistsByName(this, name))
                    {
                        throw new AutomateException(ExceptionMessages.PatternElement_AttributeByNameExists
                            .Substitute(name));
                    }

                    if (ElementExistsByName(this, name))
                    {
                        throw new AutomateException(ExceptionMessages.PatternElement_AttributeByNameExistsAsElement
                            .Substitute(name));
                    }
                }

                attribute.Rename(name);
            }

            if (type.HasValue())
            {
                attribute.ResetDataType(type);
            }

            if (choices.Exists())
            {
                attribute.SetChoices(choices);
            }

            if (isRequired.HasValue)
            {
                attribute.SetRequired(isRequired.Value);
            }

            if (defaultValue.HasValue())
            {
                attribute.SetDefaultValue(defaultValue);
            }

            return attribute;
        }

        public Attribute DeleteAttribute(string name)
        {
            name.GuardAgainstNullOrEmpty(nameof(name));

            var attribute = GetAttributeByName(this, name);
            if (attribute.NotExists())
            {
                throw new AutomateException(ExceptionMessages.PatternElement_AttributeByNameNotExists.Substitute(name));
            }

            DeleteAttribute(attribute);

            return attribute;
        }

        public Element AddElement(string name, ElementCardinality cardinality = ElementCardinality.One,
            bool autoCreate = false, string displayName = null, string description = null)
        {
            name.GuardAgainstNullOrEmpty(nameof(name));

            if (ElementNameIsReserved(name))
            {
                throw new AutomateException(ExceptionMessages.PatternElement_ElementNameReserved.Substitute(name));
            }

            if (ElementExistsByName(this, name))
            {
                throw new AutomateException(ExceptionMessages.PatternElement_ElementByNameExists.Substitute(name));
            }

            if (AttributeExistsByName(this, name))
            {
                throw new AutomateException(
                    ExceptionMessages.PatternElement_ElementByNameExistsAsAttribute.Substitute(name));
            }

            var element = new Element(name, cardinality, autoCreate, displayName, description);
            AddElement(element);

            return element;
        }

        public Element UpdateElement(string elementName, string name = null, bool? isRequired = null,
            bool? autoCreate = null, string displayName = null, string description = null)
        {
            elementName.GuardAgainstNullOrEmpty(nameof(elementName));

            var element = GetElementByName(this, elementName);
            if (element.NotExists())
            {
                throw new AutomateException(
                    ExceptionMessages.PatternElement_ElementByNameNotExists.Substitute(elementName));
            }

            if (name.HasValue())
            {
                if (name.NotEqualsIgnoreCase(element.Name))
                {
                    if (ElementNameIsReserved(name))
                    {
                        throw new AutomateException(
                            ExceptionMessages.PatternElement_ElementNameReserved.Substitute(name));
                    }

                    if (ElementExistsByName(this, name))
                    {
                        throw new AutomateException(
                            ExceptionMessages.PatternElement_ElementByNameExists.Substitute(name));
                    }

                    if (AttributeExistsByName(this, name))
                    {
                        throw new AutomateException(ExceptionMessages.PatternElement_ElementByNameExistsAsAttribute
                            .Substitute(name));
                    }
                }
            }
            element.RenameAndDescribe(name, displayName, description);

            if (isRequired.HasValue)
            {
                if (element.IsCollection)
                {
                    if (isRequired.Value
                        && element.Cardinality == ElementCardinality.ZeroOrMany)
                    {
                        element.SetCardinality(ElementCardinality.OneOrMany);
                    }
                    if (!isRequired.Value
                        && element.Cardinality == ElementCardinality.OneOrMany)
                    {
                        element.SetCardinality(ElementCardinality.ZeroOrMany);
                    }
                }
                else
                {
                    if (isRequired.Value
                        && element.Cardinality == ElementCardinality.ZeroOrOne)
                    {
                        element.SetCardinality(ElementCardinality.One);
                    }
                    if (!isRequired.Value
                        && element.Cardinality == ElementCardinality.One)
                    {
                        element.SetCardinality(ElementCardinality.ZeroOrOne);
                    }
                }
            }

            if (autoCreate.HasValue)
            {
                element.SetCreation(autoCreate.Value);
            }

            return element;
        }

        public Element DeleteElement(string name)
        {
            name.GuardAgainstNullOrEmpty(nameof(name));

            var element = GetElementByName(this, name);
            if (element.NotExists())
            {
                throw new AutomateException(ExceptionMessages.PatternElement_ElementByNameNotExists.Substitute(name));
            }

            DeleteElement(element);

            return element;
        }

        public CodeTemplate AddCodeTemplate(string name, string fullPath, string extension)
        {
            fullPath.GuardAgainstNullOrEmpty(nameof(fullPath));
            extension.GuardAgainstNullOrEmpty(nameof(extension));

            var templateName = this.autoNamer.GetNextCodeTemplateName(name, this);
            if (CodeTemplateExistsByName(this, templateName))
            {
                throw new AutomateException(ExceptionMessages.PatternElement_CodeTemplateByNameExists
                    .Substitute(name));
            }

            var codeTemplate = new CodeTemplate(templateName, fullPath, extension);
            AddCodeTemplate(codeTemplate);

            return codeTemplate;
        }

        public CodeTemplate DeleteCodeTemplate(string name, bool includeReferencingAutomation)
        {
            name.GuardAgainstNullOrEmpty(nameof(name));

            var codeTemplate = this.codeTemplates.FirstOrDefault(temp => temp.Name == name);
            if (codeTemplate.NotExists())
            {
                throw new AutomateException(ExceptionMessages.PatternElement_CodeTemplateNotExistsById
                    .Substitute(name));
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

            return codeTemplate;
        }

        public Automation AddCodeTemplateCommand(string name, string codeTemplateName, bool isOneOff, string filePath)
        {
            codeTemplateName.GuardAgainstNull(nameof(codeTemplateName));
            filePath.GuardAgainstNull(nameof(filePath));

            var codeTemplate = CodeTemplates.FirstOrDefault(ele => ele.Name.EqualsIgnoreCase(codeTemplateName));
            if (codeTemplate.NotExists())
            {
                throw new AutomateException(
                    ExceptionMessages.PatternElement_CodeTemplateNotFound.Substitute(codeTemplateName));
            }

            var commandName = this.autoNamer.GetNextAutomationName(AutomationType.CodeTemplateCommand, name, this);
            if (AutomationExistsByName(this, commandName))
            {
                throw new AutomateException(
                    ExceptionMessages.PatternElement_AutomationByNameExists.Substitute(commandName));
            }

            var automation = new CodeTemplateCommand(commandName, codeTemplate.Id, isOneOff, filePath)
                .AsAutomation();
            AddAutomation(automation);

            return automation;
        }

        public Automation UpdateCodeTemplateCommand(string commandName, string name, bool? isOneOff, string filePath)
        {
            var automation =
                FindAutomationByName(this, commandName, auto => auto.Type == AutomationType.CodeTemplateCommand);
            if (automation.NotExists())
            {
                throw new AutomateException(
                    ExceptionMessages.PatternElement_AutomationNotExistsByName.Substitute(
                        AutomationType.CodeTemplateCommand, commandName));
            }
            var command = CodeTemplateCommand.FromAutomation(automation);
            if (name.HasValue())
            {
                command.ChangeName(name);
            }
            if (isOneOff.HasValue)
            {
                command.ChangeOneOff(isOneOff.Value);
            }
            if (filePath.HasValue())
            {
                command.ChangeFilePath(filePath);
            }

            return command.AsAutomation();
        }

        public void DeleteCodeTemplateCommand(string id, bool includeReferencingAutomation)
        {
            id.GuardAgainstNullOrEmpty(nameof(id));

            var command = FindAutomationById(this, id, auto => auto.Type == AutomationType.CodeTemplateCommand);
            if (command.NotExists())
            {
                throw new AutomateException(
                    ExceptionMessages.PatternElement_AutomationNotExistsById.Substitute(
                        AutomationType.CodeTemplateCommand,
                        id));
            }

            DeleteCodeTemplateCommand(CodeTemplateCommand.FromAutomation(command));

            if (includeReferencingAutomation)
            {
                RemoveLaunchPointReferences(command.Id);
            }
        }

        public Automation AddCliCommand(string name, string applicationName, string arguments)
        {
            applicationName.GuardAgainstNull(nameof(applicationName));

            var commandName = this.autoNamer.GetNextAutomationName(AutomationType.CliCommand, name, this);
            if (AutomationExistsByName(this, commandName))
            {
                throw new AutomateException(
                    ExceptionMessages.PatternElement_AutomationByNameExists.Substitute(commandName));
            }

            var automation = new CliCommand(commandName, applicationName, arguments).AsAutomation();
            AddAutomation(automation);

            return automation;
        }

        public Automation UpdateCliCommand(string commandName, string name, string applicationName, string arguments)
        {
            var automation = FindAutomationByName(this, commandName, auto => auto.Type == AutomationType.CliCommand);
            if (automation.NotExists())
            {
                throw new AutomateException(
                    ExceptionMessages.PatternElement_AutomationNotExistsByName.Substitute(AutomationType.CliCommand,
                        commandName));
            }

            var command = CliCommand.FromAutomation(automation);
            if (name.HasValue())
            {
                command.ChangeName(name);
            }
            if (applicationName.HasValue() && name.NotEqualsOrdinal(command.ApplicationName))
            {
                command.ChangeApplicationName(applicationName);
            }
            if (arguments.HasValue() && name.NotEqualsOrdinal(command.Arguments))
            {
                command.ChangeArguments(arguments);
            }

            return command.AsAutomation();
        }

        public void DeleteCliCommand(string id, bool includeReferencingAutomation)
        {
            id.GuardAgainstNullOrEmpty(nameof(id));

            var command = FindAutomationById(this, id, auto => auto.Type == AutomationType.CliCommand);
            if (command.NotExists())
            {
                throw new AutomateException(
                    ExceptionMessages.PatternElement_AutomationNotExistsById.Substitute(AutomationType.CliCommand, id));
            }

            DeleteCliCommand(CliCommand.FromAutomation(command));

            if (includeReferencingAutomation)
            {
                RemoveLaunchPointReferences(command.Id);
            }
        }

        public Automation AddCommandLaunchPoint(string name, List<string> commandIds, IPatternElement sourceElement)
        {
            commandIds.GuardAgainstNull(nameof(commandIds));
            sourceElement.GuardAgainstNull(nameof(sourceElement));

            if (commandIds.HasNone())
            {
                throw new AutomateException(ExceptionMessages.PatternElement_NoCommandIds);
            }
            if (commandIds.Count == 1
                && commandIds.First() == LaunchPointSelectionWildcard)
            {
                commandIds.Clear();
                var launchables = GetAllLaunchableAutomation(sourceElement.Automation);
                if (launchables.HasNone())
                {
                    throw new AutomateException(ExceptionMessages.PatternElement_NoCommandsToLaunch);
                }
                commandIds.AddRange(launchables);
            }
            else
            {
                commandIds.ForEach(cmdId =>
                {
                    var command = Pattern.FindAutomation(cmdId, auto => auto.IsLaunchable);
                    if (command.NotExists())
                    {
                        throw new AutomateException(
                            ExceptionMessages.PatternElement_CommandIdNotFound.Substitute(cmdId));
                    }
                });
            }

            var launchPointName = this.autoNamer.GetNextAutomationName(AutomationType.CommandLaunchPoint, name, this);
            if (AutomationExistsByName(this, launchPointName))
            {
                throw new AutomateException(
                    ExceptionMessages.PatternElement_AutomationByNameExists.Substitute(launchPointName));
            }

            var automation = new CommandLaunchPoint(launchPointName, commandIds).AsAutomation();
            AddAutomation(automation);

            return automation;
        }

        public Automation UpdateCommandLaunchPoint(string launchPointName, string name, List<string> addCommandIds,
            List<string> removeCommandIds, IPatternElement sourceElement)
        {
            launchPointName.GuardAgainstNullOrEmpty(nameof(launchPointName));
            addCommandIds.GuardAgainstNull(nameof(addCommandIds));
            removeCommandIds.GuardAgainstNull(nameof(removeCommandIds));
            sourceElement.GuardAgainstNull(nameof(sourceElement));

            var launchables = GetAllLaunchableAutomation(sourceElement.Automation);
            if (addCommandIds.Count == 1
                && addCommandIds.First() == LaunchPointSelectionWildcard)
            {
                addCommandIds.Clear();
                if (launchables.HasNone())
                {
                    throw new AutomateException(ExceptionMessages.PatternElement_NoCommandsToLaunch);
                }
                addCommandIds.AddRange(launchables);
            }
            else
            {
                addCommandIds.ForEach(cmdId =>
                {
                    var command = Pattern.FindAutomation(cmdId, auto => auto.IsLaunchable);
                    if (command.NotExists())
                    {
                        throw new AutomateException(
                            ExceptionMessages.PatternElement_CommandIdNotFound.Substitute(cmdId));
                    }
                });
            }

            if (removeCommandIds.Count == 1
                && removeCommandIds.First() == LaunchPointSelectionWildcard)
            {
                removeCommandIds.Clear();
                removeCommandIds.AddRange(launchables);
            }

            var automation = FindAutomationByName(this, launchPointName,
                auto => auto.Type == AutomationType.CommandLaunchPoint);
            if (automation.NotExists())
            {
                throw new AutomateException(
                    ExceptionMessages.PatternElement_AutomationNotExistsByName.Substitute(
                        AutomationType.CommandLaunchPoint,
                        launchPointName));
            }

            var launchPoint = CommandLaunchPoint.FromAutomation(automation);
            launchPoint.ChangeCommandIds(addCommandIds, removeCommandIds);
            if (name.HasValue())
            {
                launchPoint.ChangeName(name);
            }

            return launchPoint.AsAutomation();
        }

        public void DeleteCommandLaunchPoint(string id)
        {
            id.GuardAgainstNullOrEmpty(nameof(id));

            var launchPoint = FindAutomationById(this, id, auto => auto.Type == AutomationType.CommandLaunchPoint);
            if (launchPoint.NotExists())
            {
                throw new AutomateException(
                    ExceptionMessages.PatternElement_AutomationNotExistsById.Substitute(
                        AutomationType.CommandLaunchPoint,
                        id));
            }

            DeleteCommandLaunchPoint(CommandLaunchPoint.FromAutomation(launchPoint));
        }

        public Automation DeleteAutomation(string name)
        {
            name.GuardAgainstNullOrEmpty(nameof(name));

            var automation = FindAutomationByName(this, name, _ => true);
            if (automation.NotExists())
            {
                throw new AutomateException(
                    ExceptionMessages.PatternElement_AutomationNotExistsByName.Substitute(AutomationType.Unknown,
                        name));
            }

            switch (automation.Type)
            {
                case AutomationType.CodeTemplateCommand:
                    DeleteCodeTemplateCommand(automation.Id, true);
                    break;

                case AutomationType.CliCommand:
                    DeleteCliCommand(automation.Id, true);
                    break;

                case AutomationType.CommandLaunchPoint:
                    DeleteCommandLaunchPoint(automation.Id);
                    break;

                case AutomationType.Unknown:
                default:
                    throw new AutomateException(
                        ExceptionMessages.PatternElement_UnknownAutomationType.Substitute(automation.Type));
            }

            return automation;
        }

        public string Id { get; }

        public string Name { get; protected set; }

        public IReadOnlyList<Element> Elements => this.elements;

        public IReadOnlyList<Attribute> Attributes => this.attributes;

        public IReadOnlyList<CodeTemplate> CodeTemplates => this.codeTemplates;

        public IReadOnlyList<Automation> Automation => this.automations;

        public CodeTemplate FindCodeTemplateByName(string name)
        {
            var codeTemplate = CodeTemplates.FirstOrDefault(template => template.Name.EqualsIgnoreCase(name));
            if (codeTemplate.NotExists())
            {
                throw new AutomateException(ExceptionMessages.PatternElement_CodeTemplateNotFound.Substitute(name));
            }

            return codeTemplate;
        }

        public CodeTemplateCommand FindCodeTemplateCommandByName(string name)
        {
            var automation = Automation.FirstOrDefault(auto =>
                auto.Type == AutomationType.CodeTemplateCommand && auto.Name.EqualsIgnoreCase(name));
            if (automation.NotExists())
            {
                throw new AutomateException(
                    ExceptionMessages.PatternElement_CodeTemplateCommandNotFound.Substitute(name));
            }

            return CodeTemplateCommand.FromAutomation(automation);
        }

        public virtual bool Accept(IPatternVisitor visitor)
        {
            var abort = false;

            if (!visitor.VisitCodeTemplatesEnter(CodeTemplates))
            {
                return false;
            }
            foreach (var template in CodeTemplates)
            {
                if (!template.Accept(visitor))
                {
                    abort = true;
                    break;
                }
            }
            visitor.VisitCodeTemplatesExit(CodeTemplates);
            if (abort)
            {
                return false;
            }

            if (!visitor.VisitAutomationsEnter(Automation))
            {
                return false;
            }
            foreach (var automation in Automation)
            {
                if (!automation.Accept(visitor))
                {
                    abort = true;
                    break;
                }
            }
            visitor.VisitAutomationsExit(Automation);
            if (abort)
            {
                return false;
            }

            if (!visitor.VisitAttributesEnter(Attributes))
            {
                return false;
            }
            foreach (var attribute in Attributes)
            {
                if (!attribute.Accept(visitor))
                {
                    abort = true;
                    break;
                }
            }
            visitor.VisitAttributesExit(Attributes);
            if (abort)
            {
                return false;
            }

            if (!visitor.VisitElementsEnter(Elements))
            {
                return false;
            }
            foreach (var element in Elements)
            {
                if (!element.Accept(visitor))
                {
                    abort = true;
                    break;
                }
            }
            visitor.VisitElementsExit(Elements);

            return !abort;
        }

        private string GetEditPath()
        {
            var path = GetAncestorPath(this);
            return $"{{{path}}}";

            string GetAncestorPath(PatternElement element)
            {
                return element.Parent.Exists()
                    ? $"{GetAncestorPath(element.Parent)}.{element.Name}"
                    : $"{element.Name}";
            }
        }

        private void RemoveLaunchPointReferences(string automationId)
        {
            Pattern.GetAllLaunchPoints()
                .Where(pair => pair.LaunchPoint.CommandIds.Contains(automationId))
                .ToList().ForEach(pair =>
                {
                    pair.LaunchPoint.RemoveCommandId(automationId);
                    if (pair.LaunchPoint.CommandIds.HasNone())
                    {
                        pair.Parent.DeleteCommandLaunchPoint(pair.LaunchPoint.Id);
                    }
                });
        }

        private static List<string> GetAllLaunchableAutomation(IReadOnlyList<Automation> automation)
        {
            return automation
                .Where(auto => auto.IsLaunchable)
                .Select(auto => auto.Id)
                .ToList();
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

        private static bool ElementNameIsReserved(string attributeName)
        {
            return Element.ReservedElementNames.Any(reserved => reserved.EqualsIgnoreCase(attributeName));
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

        private static Automation FindAutomationByName(IAutomationContainer element, string name,
            Predicate<Automation> where)
        {
            return element.Automation.Safe().FirstOrDefault(auto => auto.Name.EqualsIgnoreCase(name) && where(auto));
        }

        private static Automation FindAutomationById(IAutomationContainer element, string name,
            Predicate<Automation> where)
        {
            return element.Automation.Safe().FirstOrDefault(auto => auto.Id.EqualsIgnoreCase(name) && where(auto));
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