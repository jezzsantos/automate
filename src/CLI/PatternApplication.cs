using System.Collections.Generic;
using System.Linq;
using automate.Extensions;

namespace automate
{
    internal class PatternApplication
    {
        private readonly IFilePathResolver filePathResolver;
        private readonly IPatternPathResolver patternPathResolver;
        private readonly PatternStore store;

        public PatternApplication(string currentDirectory) : this(new PatternStore(currentDirectory),
            new SystemIoFilePathResolver(), new PatternPathResolver())
        {
            currentDirectory.GuardAgainstNullOrEmpty(nameof(currentDirectory));
        }

        internal PatternApplication(PatternStore store, IFilePathResolver filePathResolver,
            IPatternPathResolver patternPathResolver)
        {
            store.GuardAgainstNull(nameof(store));
            filePathResolver.GuardAgainstNull(nameof(filePathResolver));
            patternPathResolver.GuardAgainstNull(nameof(patternPathResolver));
            this.store = store;
            this.filePathResolver = filePathResolver;
            this.patternPathResolver = patternPathResolver;
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

            var absolutePath = this.filePathResolver.CreatePath(rootPath, relativeFilePath);
            if (!this.filePathResolver.ExistsAtPath(absolutePath))
            {
                throw new PatternException(
                    ExceptionMessages.PatternApplication_CodeTemplate_NotFoundAtLocation.Format(rootPath,
                        relativeFilePath));
            }

            IPatternElement target = pattern;
            if (parentExpression.HasValue())
            {
                target = this.patternPathResolver.Resolve(pattern, parentExpression);
                if (target.NotExists())
                {
                    throw new PatternException(
                        ExceptionMessages.PatternApplication_NodeExpressionNotFound.Format(parentExpression));
                }
            }

            var templateName = name.HasValue()
                ? name
                : $"CodeTemplate{pattern.CodeTemplates.Count + 1}";
            if (CodeTemplateExistsByName(target, templateName))
            {
                throw new PatternException(ExceptionMessages.PatternApplication_CodeTemplateByNameExists.Format(name));
            }

            var codeTemplate = new CodeTemplate(templateName, absolutePath);
            pattern.CodeTemplates.Add(codeTemplate);
            this.store.Save(pattern);

            var sourceFile = this.filePathResolver.GetFileAtPath(absolutePath);
            this.store.UploadCodeTemplate(pattern, codeTemplate.Id, sourceFile);

            return codeTemplate;
        }

        public List<CodeTemplate> ListCodeTemplates()
        {
            VerifyCurrentPatternExists();
            var pattern = this.store.GetCurrent();

            return pattern.CodeTemplates;
        }

        public IPatternElement AddAttribute(string name, string type, string defaultValue,
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
                throw new PatternException(ExceptionMessages.PatternApplication_AttributeDefaultValueIsNotAChoice);
            }

            IPatternElement target = pattern;
            if (parentExpression.HasValue())
            {
                target = this.patternPathResolver.Resolve(pattern, parentExpression);
                if (target.NotExists())
                {
                    throw new PatternException(
                        ExceptionMessages.PatternApplication_NodeExpressionNotFound.Format(parentExpression));
                }
            }

            if (AttributeExistsByName(target, name))
            {
                throw new PatternException(ExceptionMessages.PatternApplication_AttributeByNameExists.Format(name));
            }

            var attribute = new Attribute(name, type, isRequired, defaultValue)
            {
                Choices = choices.ToList()
            };
            target.Attributes.Add(attribute);
            this.store.Save(pattern);

            return target;
        }

        public IPatternElement AddElement(string name, string displayName, string description, bool isCollection,
            string parentExpression)
        {
            name.GuardAgainstNullOrEmpty(nameof(name));

            VerifyCurrentPatternExists();
            var pattern = this.store.GetCurrent();

            IPatternElement target = pattern;
            if (parentExpression.HasValue())
            {
                target = this.patternPathResolver.Resolve(pattern, parentExpression);
                if (target.NotExists())
                {
                    throw new PatternException(
                        ExceptionMessages.PatternApplication_NodeExpressionNotFound.Format(parentExpression));
                }
            }

            if (ElementExistsByName(target, name))
            {
                throw new PatternException(ExceptionMessages.PatternApplication_ElementByNameExists.Format(name));
            }

            var element = new Element(name, displayName, description, isCollection);
            target.Elements.Add(element);
            this.store.Save(pattern);

            return target;
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
                target = this.patternPathResolver.Resolve(pattern, parentExpression);
                if (target.NotExists())
                {
                    throw new PatternException(
                        ExceptionMessages.PatternApplication_NodeExpressionNotFound.Format(parentExpression));
                }
            }

            if (AutomationExistsByName(target, name))
            {
                throw new PatternException(ExceptionMessages.PatternApplication_AutomationByNameExists.Format(name));
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
                target = this.patternPathResolver.Resolve(pattern, parentExpression);
                if (target.NotExists())
                {
                    throw new PatternException(
                        ExceptionMessages.PatternApplication_NodeExpressionNotFound.Format(parentExpression));
                }
            }

            var launchPointName = name.HasValue()
                ? name
                : $"LaunchPoint{target.Automation.Count + 1}";
            if (AutomationExistsByName(target, launchPointName))
            {
                throw new PatternException(ExceptionMessages.PatternApplication_AutomationByNameExists.Format(name));
            }

            var commandIdentifiers = commandIds.SafeSplit(";").ToList();
            var automation = new AutomationLaunchPoint(launchPointName, commandIdentifiers);
            target.Automation.Add(automation);
            this.store.Save(pattern);

            return automation;
        }

        private void VerifyCurrentPatternExists()
        {
            if (this.store.GetCurrent().NotExists())
            {
                throw new PatternException(ExceptionMessages.PatternApplication_NoCurrentPattern);
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

        private static bool CodeTemplateExistsByName(IAutomationContainer element, string templateName)
        {
            return element.CodeTemplates.Any(template => template.Name.EqualsIgnoreCase(templateName));
        }
    }
}