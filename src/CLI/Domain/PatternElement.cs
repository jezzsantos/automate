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
            RecordChange(VersionChange.NonBreaking, DomainMessages.PatternElement_VersionChange_Attribute_Add.Format(attribute.Id, Id));
        }

        public void DeleteAttribute(Attribute attribute)
        {
            this.attributes.Remove(attribute);
            RecordChange(VersionChange.Breaking, DomainMessages.PatternElement_VersionChange_Attribute_Delete.Format(attribute.Id, Id));
        }

        public void AddElement(Element element)
        {
            element.SetParent(this);
            this.elements.Add(element);
            RecordChange(VersionChange.NonBreaking, DomainMessages.PatternElement_VersionChange_Element_Add.Format(element.Id, Id));
        }

        public void DeleteElement(Element element)
        {
            this.elements.Remove(element);
            RecordChange(VersionChange.Breaking, DomainMessages.PatternElement_VersionChange_Element_Delete.Format(element.Id, Id));
        }

        public void AddCodeTemplate(CodeTemplate codeTemplate)
        {
            this.codeTemplates.Add(codeTemplate);
            RecordChange(VersionChange.NonBreaking, DomainMessages.PatternElement_VersionChange_CodeTemplate_Add.Format(codeTemplate.Id, Id));
        }

        public void AddAutomation(Automation automation)
        {
            this.automations.Add(automation);
            RecordChange(VersionChange.NonBreaking, DomainMessages.PatternElement_VersionChange_Automation_Add.Format(automation.Id, Id));
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

        public CodeTemplate AttachCodeTemplate(string name, string fullPath, string extension)
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

        public Automation AddCodeTemplateCommand(string name, string codeTemplateName, bool isTearOff, string filePath)
        {
            codeTemplateName.GuardAgainstNull(nameof(codeTemplateName));
            filePath.GuardAgainstNull(nameof(filePath));

            var codeTemplate = CodeTemplates.FirstOrDefault(ele => ele.Name.EqualsIgnoreCase(codeTemplateName));
            if (codeTemplate.NotExists())
            {
                throw new AutomateException(ExceptionMessages.PatternElement_CodeTemplateNoFound.Format(codeTemplateName));
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

        public Automation AddCommandLaunchPoint(string name, List<string> commandIds, PatternDefinition pattern)
        {
            commandIds.GuardAgainstNull(nameof(commandIds));
            pattern.GuardAgainstNull(nameof(pattern));

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
                    var command = pattern.FindAutomation(cmdId);
                    if (command.NotExists() || !command.IsLaunchable())
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

        public Automation UpdateCommandLaunchPoint(string name, List<string> commandIds, IPatternElement sourceElement, PatternDefinition pattern)
        {
            name.GuardAgainstNullOrEmpty(nameof(name));
            commandIds.GuardAgainstNull(nameof(commandIds));
            pattern.GuardAgainstNull(nameof(pattern));

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
                    var command = pattern.FindAutomation(cmdId);
                    if (command.NotExists() || !command.IsLaunchable())
                    {
                        throw new AutomateException(ExceptionMessages.PatternElement_CommandIdNotFound.Format(cmdId));
                    }
                });
            }

            var automation = GetAutomationByName(this, name);
            if (automation.NotExists() || automation.Type != AutomationType.CommandLaunchPoint)
            {
                throw new AutomateException(
                    ExceptionMessages.PatternElement_AutomationNotExistsByName.Format(name));
            }
            var launchPoint = CommandLaunchPoint.FromAutomation(automation);
            launchPoint.AppendCommandIds(commandIds);
            RecordChange(VersionChange.NonBreaking, DomainMessages.PatternElement_VersionChange_Automation_Update.Format(launchPoint.Id, Id));

            return launchPoint.AsAutomation();
        }

        public Automation FindAutomation(string id)
        {
            return FindDescendantAutomation(this);

            Automation FindDescendantAutomation(IPatternElement element)
            {
                var automation = element.Automation.Safe()
                    .FirstOrDefault(auto => auto.Id.EqualsIgnoreCase(id));
                if (automation.Exists())
                {
                    return automation;
                }
                return element.Elements.Safe()
                    .Select(FindDescendantAutomation)
                    .FirstOrDefault(auto => auto.Exists());
            }
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

        private void RecordChange(VersionChange change, string description)
        {
            var pattern = Pattern;
            if (pattern.NotExists())
            {
                return;
            }

            if (this == Pattern)
            {
                pattern.ToolkitVersion.RegisterChange(change, description);
            }
            else
            {
                pattern.RecordChange(change, description);
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

        private static Automation GetAutomationByName(IAutomationContainer element, string name)
        {
            return element.Automation.Safe().FirstOrDefault(auto => auto.Name.EqualsIgnoreCase(name));
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