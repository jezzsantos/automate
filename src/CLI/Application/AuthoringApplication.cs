using System.Collections.Generic;
using System.Linq;
using automate.Domain;
using automate.Extensions;
using automate.Infrastructure;

namespace automate.Application
{
    internal class AuthoringApplication
    {
        private readonly IFilePathResolver fileResolver;
        private readonly IPatternToolkitPackager packager;
        private readonly IPatternPathResolver patternResolver;
        private readonly IPatternStore store;

        public AuthoringApplication(string currentDirectory) : this(currentDirectory,
            new PatternStore(currentDirectory), new SystemIoFilePathResolver())
        {
        }

        private AuthoringApplication(string currentDirectory, IPatternStore store, IFilePathResolver fileResolver) :
            this(store, fileResolver, new PatternPathResolver(),
                new PatternToolkitPackager(store, new ToolkitStore(currentDirectory), fileResolver))
        {
            currentDirectory.GuardAgainstNullOrEmpty(nameof(currentDirectory));
        }

        internal AuthoringApplication(IPatternStore store, IFilePathResolver fileResolver,
            IPatternPathResolver patternResolver, IPatternToolkitPackager packager)
        {
            store.GuardAgainstNull(nameof(store));
            fileResolver.GuardAgainstNull(nameof(fileResolver));
            patternResolver.GuardAgainstNull(nameof(patternResolver));
            packager.GuardAgainstNull(nameof(packager));
            this.store = store;
            this.fileResolver = fileResolver;
            this.patternResolver = patternResolver;
            this.packager = packager;
        }

        public string CurrentPatternId => this.store.GetCurrent()?.Id;

        public string CurrentPatternName => this.store.GetCurrent()?.Name;

        public void CreateNewPattern(string name)
        {
            name.GuardAgainstNullOrEmpty(nameof(name));
            this.store.Create(name);
        }

        public void SwitchCurrentPattern(string name)
        {
            name.GuardAgainstNullOrEmpty(nameof(name));
            var current = this.store.Find(name);
            this.store.ChangeCurrent(current.Id);
        }

        public CodeTemplate AttachCodeTemplate(string rootPath, string relativeFilePath, string name,
            string parentExpression)
        {
            rootPath.GuardAgainstNullOrEmpty(nameof(rootPath));
            relativeFilePath.GuardAgainstNullOrEmpty(nameof(relativeFilePath));

            VerifyCurrentPatternExists();
            var pattern = this.store.GetCurrent();

            var absolutePath = this.fileResolver.CreatePath(rootPath, relativeFilePath);
            if (!this.fileResolver.ExistsAtPath(absolutePath))
            {
                throw new AutomateException(
                    ExceptionMessages.AuthoringApplication_CodeTemplate_NotFoundAtLocation.Format(rootPath,
                        relativeFilePath));
            }

            IPatternElement target = pattern;
            if (parentExpression.HasValue())
            {
                target = this.patternResolver.Resolve(pattern, parentExpression);
                if (target.NotExists())
                {
                    throw new AutomateException(
                        ExceptionMessages.AuthoringApplication_NodeExpressionNotFound.Format(parentExpression));
                }
            }

            var templateName = name.HasValue()
                ? name
                : $"CodeTemplate{pattern.CodeTemplates.Count + 1}";
            if (CodeTemplateExistsByName(target, templateName))
            {
                throw new AutomateException(ExceptionMessages.AuthoringApplication_CodeTemplateByNameExists
                    .Format(name));
            }

            var codeTemplate = new CodeTemplate(templateName, absolutePath);
            pattern.CodeTemplates.Add(codeTemplate);
            this.store.Save(pattern);

            var sourceFile = this.fileResolver.GetFileAtPath(absolutePath);
            this.store.UploadCodeTemplate(pattern, codeTemplate.Id, sourceFile);

            return codeTemplate;
        }

        public List<CodeTemplate> ListCodeTemplates()
        {
            VerifyCurrentPatternExists();
            var pattern = this.store.GetCurrent();

            return pattern.CodeTemplates;
        }

        public (IPatternElement parent, Attribute child) AddAttribute(string name, string type, string defaultValue,
            bool isRequired, string isOneOf, string parentExpression)
        {
            name.GuardAgainstNullOrEmpty(nameof(name));

            VerifyCurrentPatternExists();
            var pattern = this.store.GetCurrent();

            var choices = isOneOf.SafeSplit(";");
            if (defaultValue.HasValue()
                && choices.Any()
                && !choices.Contains(defaultValue))
            {
                throw new AutomateException(ExceptionMessages.AuthoringApplication_AttributeDefaultValueIsNotAChoice);
            }

            IPatternElement target = pattern;
            if (parentExpression.HasValue())
            {
                target = this.patternResolver.Resolve(pattern, parentExpression);
                if (target.NotExists())
                {
                    throw new AutomateException(
                        ExceptionMessages.AuthoringApplication_NodeExpressionNotFound.Format(parentExpression));
                }
            }

            if (AttributeNameIsReserved(name))
            {
                throw new AutomateException(ExceptionMessages.AuthoringApplication_AttributeNameReserved.Format(name));
            }

            if (AttributeExistsByName(target, name))
            {
                throw new AutomateException(ExceptionMessages.AuthoringApplication_AttributeByNameExists.Format(name));
            }

            var attribute = new Attribute(name, type, isRequired, defaultValue)
            {
                Choices = choices.ToList()
            };
            target.Attributes.Add(attribute);
            this.store.Save(pattern);

            return (target, attribute);
        }

        public (IPatternElement parent, IPatternElement child) AddElement(string name, string displayName,
            string description, bool isCollection,
            string parentExpression)
        {
            name.GuardAgainstNullOrEmpty(nameof(name));

            VerifyCurrentPatternExists();
            var pattern = this.store.GetCurrent();

            IPatternElement target = pattern;
            if (parentExpression.HasValue())
            {
                target = this.patternResolver.Resolve(pattern, parentExpression);
                if (target.NotExists())
                {
                    throw new AutomateException(
                        ExceptionMessages.AuthoringApplication_NodeExpressionNotFound.Format(parentExpression));
                }
            }

            if (ElementExistsByName(target, name))
            {
                throw new AutomateException(ExceptionMessages.AuthoringApplication_ElementByNameExists.Format(name));
            }

            var element = new Element(name, displayName, description, isCollection);
            target.Elements.Add(element);
            this.store.Save(pattern);

            return (target, element);
        }

        public AutomationCommand AddCodeTemplateCommand(string name, bool isTearOff, string filePath,
            string parentExpression)
        {
            name.GuardAgainstNullOrEmpty(nameof(name));
            filePath.GuardAgainstNullOrEmpty(nameof(filePath));

            VerifyCurrentPatternExists();
            var pattern = this.store.GetCurrent();

            IPatternElement target = pattern;
            if (parentExpression.HasValue())
            {
                target = this.patternResolver.Resolve(pattern, parentExpression);
                if (target.NotExists())
                {
                    throw new AutomateException(
                        ExceptionMessages.AuthoringApplication_NodeExpressionNotFound.Format(parentExpression));
                }
            }

            if (AutomationExistsByName(target, name))
            {
                throw new AutomateException(ExceptionMessages.AuthoringApplication_AutomationByNameExists.Format(name));
            }

            var automation = new AutomationCommand(name, isTearOff, filePath);
            target.Automation.Add(automation);
            this.store.Save(pattern);

            return automation;
        }

        public AutomationLaunchPoint AddCommandLaunchPoint(string commandIds, string name, string parentExpression)
        {
            commandIds.GuardAgainstNullOrEmpty(nameof(commandIds));

            VerifyCurrentPatternExists();
            var pattern = this.store.GetCurrent();

            IPatternElement target = pattern;
            if (parentExpression.HasValue())
            {
                target = this.patternResolver.Resolve(pattern, parentExpression);
                if (target.NotExists())
                {
                    throw new AutomateException(
                        ExceptionMessages.AuthoringApplication_NodeExpressionNotFound.Format(parentExpression));
                }
            }

            var launchPointName = name.HasValue()
                ? name
                : $"LaunchPoint{target.Automation.Count + 1}";
            if (AutomationExistsByName(target, launchPointName))
            {
                throw new AutomateException(ExceptionMessages.AuthoringApplication_AutomationByNameExists.Format(name));
            }

            var commandIdentifiers = commandIds.SafeSplit(";").ToList();
            var automation = new AutomationLaunchPoint(launchPointName, commandIdentifiers);
            target.Automation.Add(automation);
            this.store.Save(pattern);

            return automation;
        }

        public PatternToolkitPackage PackageToolkit(string version)
        {
            VerifyCurrentPatternExists();
            var pattern = this.store.GetCurrent();

            var package = this.packager.Pack(pattern, version);

            return package;
        }

        public PatternDefinition GetCurrentPattern()
        {
            VerifyCurrentPatternExists();
            return this.store.GetCurrent();
        }

        private void VerifyCurrentPatternExists()
        {
            if (this.store.GetCurrent().NotExists())
            {
                throw new AutomateException(ExceptionMessages.AuthoringApplication_NoCurrentPattern);
            }
        }

        private static bool AutomationExistsByName(IAutomationContainer element, string automationName)
        {
            return element.Automation.Any(ele => ele.Name.EqualsIgnoreCase(automationName));
        }

        private static bool ElementExistsByName(IElementContainer element, string elementName)
        {
            return element.Elements.Any(ele => ele.Name.EqualsIgnoreCase(elementName));
        }

        private static bool AttributeExistsByName(IAttributeContainer element, string attributeName)
        {
            return element.Attributes.Any(attr => attr.Name.EqualsIgnoreCase(attributeName));
        }

        private static bool AttributeNameIsReserved(string attributeName)
        {
            return Attribute.ReservedAttributeNames.Any(reserved => reserved.EqualsIgnoreCase(attributeName));
        }

        private static bool CodeTemplateExistsByName(IAutomationContainer element, string templateName)
        {
            return element.CodeTemplates.Any(template => template.Name.EqualsIgnoreCase(templateName));
        }
    }
}