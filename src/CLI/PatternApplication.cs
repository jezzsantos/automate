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

        public CodeTemplate AttachCodeTemplate(string rootPath, string relativeFilePath, string name)
        {
            rootPath.GuardAgainstNullOrEmpty(nameof(rootPath));
            relativeFilePath.GuardAgainstNullOrEmpty(nameof(relativeFilePath));

            VerifyCurrentPatternExists();
            var pattern = this.store.GetCurrent();

            var fullPath = this.filePathResolver.CreatePath(rootPath, relativeFilePath);
            if (!this.filePathResolver.ExistsAtPath(fullPath))
            {
                throw new PatternException(
                    ExceptionMessages.PatternApplication_CodeTemplate_NotFoundAtLocation.Format(rootPath,
                        relativeFilePath));
            }

            var templateName = string.IsNullOrEmpty(name)
                ? $"CodeTemplate{pattern.CodeTemplates.Count + 1}"
                : name;
            if (CodeTemplateExistsByName(pattern, templateName))
            {
                throw new PatternException(ExceptionMessages.PatternApplication_CodeTemplateByNameExists.Format(name));
            }

            var codeTemplate = new CodeTemplate(templateName, fullPath);
            pattern.CodeTemplates.Add(codeTemplate);
            this.store.Save(pattern);

            return codeTemplate;
        }

        public List<CodeTemplate> ListCodeTemplates()
        {
            VerifyCurrentPatternExists();
            var pattern = this.store.GetCurrent();

            return pattern.CodeTemplates;
        }

        public IElementContainer AddAttribute(string name, string type, string defaultValue,
            bool isRequired, string isOneOf, string parentExpression)
        {
            name.GuardAgainstNullOrEmpty(nameof(name));

            VerifyCurrentPatternExists();
            var pattern = this.store.GetCurrent();

            if (AttributeExistsByName(pattern, name))
            {
                throw new PatternException(ExceptionMessages.PatternApplication_AttributeByNameExists.Format(name));
            }

            var choices = isOneOf.SafeSplit(";");
            if (defaultValue.HasValue()
                && choices.Any()
                && !choices.Contains(defaultValue))
            {
                throw new PatternException(ExceptionMessages.PatternApplication_AttributeDefaultValueIsNotAChoice);
            }

            IElementContainer target = pattern;
            if (parentExpression.HasValue())
            {
                target = this.patternPathResolver.Resolve(pattern, parentExpression);
                if (target.NotExists())
                {
                    throw new PatternException(
                        ExceptionMessages.PatternApplication_NodeExpressionNotFound.Format(parentExpression));
                }
            }

            var attribute = new Attribute(name, type, isRequired, defaultValue)
            {
                Choices = choices.ToList()
            };
            target.Attributes.Add(attribute);
            this.store.Save(pattern);

            return target;
        }

        public IElementContainer AddElement(string name, string displayName, string description, bool isCollection,
            string parentExpression)
        {
            name.GuardAgainstNullOrEmpty(nameof(name));

            VerifyCurrentPatternExists();
            var pattern = this.store.GetCurrent();

            if (ElementExistsByName(pattern, name))
            {
                throw new PatternException(ExceptionMessages.PatternApplication_ElementByNameExists.Format(name));
            }

            IElementContainer target = pattern;
            if (parentExpression.HasValue())
            {
                target = this.patternPathResolver.Resolve(pattern, parentExpression);
                if (target.NotExists())
                {
                    throw new PatternException(
                        ExceptionMessages.PatternApplication_NodeExpressionNotFound.Format(parentExpression));
                }
            }

            var element = new Element(name, displayName, description, isCollection);
            target.Elements.Add(element);
            this.store.Save(pattern);

            return target;
        }

        private void VerifyCurrentPatternExists()
        {
            if (this.store.GetCurrent().NotExists())
            {
                throw new PatternException(ExceptionMessages.PatternApplication_NoCurrentPattern);
            }
        }

        private static bool ElementExistsByName(PatternMetaModel pattern, string elementName)
        {
            return pattern.Elements.Any(ele => ele.Name.EqualsIgnoreCase(elementName));
        }

        private static bool AttributeExistsByName(PatternMetaModel pattern, string attributeName)
        {
            return pattern.Attributes.Any(attr => attr.Name.EqualsIgnoreCase(attributeName));
        }

        private static bool CodeTemplateExistsByName(PatternMetaModel pattern, string templateName)
        {
            return pattern.CodeTemplates.Any(template => template.Name.EqualsIgnoreCase(templateName));
        }
    }
}