using System.Collections.Generic;
using System.Linq;
using Automate.CLI.Extensions;

namespace Automate.CLI.Domain
{
    internal abstract class PatternElement : IPatternElement
    {
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

        public void AddElement(Element element)
        {
            this.elements.Add(element);
        }

        public void AddAttribute(Attribute attribute)
        {
            this.attributes.Add(attribute);
        }

        public void AddCodeTemplate(CodeTemplate codeTemplate)
        {
            this.codeTemplates.Add(codeTemplate);
        }

        public void AddAutomation(Automation automation)
        {
            this.automations.Add(automation);
        }

        public Attribute AddAttribute(string name, string type, bool isRequired, string defaultValue, List<string> choices)
        {
            name.GuardAgainstNullOrEmpty(nameof(name));

            if (AttributeNameIsReserved(name))
            {
                throw new AutomateException(ExceptionMessages.AuthoringApplication_AttributeNameReserved.Format(name));
            }

            if (AttributeExistsByName(this, name))
            {
                throw new AutomateException(ExceptionMessages.PatternElement_AttributeByNameExists.Format(name));
            }

            if (ElementExistsByName(this, name))
            {
                throw new AutomateException(ExceptionMessages.AuthoringApplication_AttributeByNameExistsAsElement.Format(name));
            }

            var attribute = new Attribute(name, type, isRequired, defaultValue, choices);
            this.attributes.Add(attribute);

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
            this.elements.Add(element);

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
            this.codeTemplates.Add(codeTemplate);

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

            var automationName = name.HasValue()
                ? name
                : $"CodeTemplateCommand{Automation.ToListSafe().Count + 1}";
            if (AutomationExistsByName(this, automationName))
            {
                throw new AutomateException(
                    ExceptionMessages.AuthoringApplication_AutomationByNameExists.Format(automationName));
            }

            var automation = new Automation(automationName, AutomationType.CodeTemplateCommand, new Dictionary<string, object>
            {
                { nameof(CodeTemplateCommand.CodeTemplateId), codeTemplate.Id },
                { nameof(CodeTemplateCommand.IsTearOff), isTearOff },
                { nameof(CodeTemplateCommand.FilePath), filePath }
            });
            this.automations.Add(automation);

            return automation;
        }

        public Automation AddCommandLaunchPoint(string name, List<string> commandIds, IPatternElement pattern)
        {
            commandIds.GuardAgainstNull(nameof(commandIds));
            pattern.GuardAgainstNull(nameof(pattern));

            commandIds.ForEach(cmdId =>
            {
                if (!pattern.FindAutomation(cmdId).Exists())
                {
                    throw new AutomateException(ExceptionMessages.PatternElement_CommandIdNotFound.Format(cmdId));
                }
            });

            var launchPointName = name.HasValue()
                ? name
                : $"LaunchPoint{Automation.ToListSafe().Count + 1}";
            if (AutomationExistsByName(this, launchPointName))
            {
                throw new AutomateException(
                    ExceptionMessages.AuthoringApplication_AutomationByNameExists.Format(launchPointName));
            }

            var automation = new Automation(launchPointName, AutomationType.CommandLaunchPoint, new Dictionary<string, object>
            {
                { nameof(CommandLaunchPoint.CommandIds), commandIds.Join(";") }
            });
            this.automations.Add(automation);

            return automation;
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

        private static bool AttributeExistsByName(IAttributeContainer element, string attributeName)
        {
            return element.Attributes.Safe().Any(attr => attr.Name.EqualsIgnoreCase(attributeName));
        }

        private static bool AttributeNameIsReserved(string attributeName)
        {
            return Attribute.ReservedAttributeNames.Any(reserved => reserved.EqualsIgnoreCase(attributeName));
        }

        private static bool ElementExistsByName(IElementContainer element, string elementName)
        {
            return element.Elements.Safe().Any(ele => ele.Name.EqualsIgnoreCase(elementName));
        }

        private static bool AutomationExistsByName(IAutomationContainer element, string automationName)
        {
            return element.Automation.Safe().Any(ele => ele.Name.EqualsIgnoreCase(automationName));
        }

        private static bool CodeTemplateExistsByName(IAutomationContainer element, string templateName)
        {
            return element.CodeTemplates.Safe().Any(template => template.Name.EqualsIgnoreCase(templateName));
        }
    }
}