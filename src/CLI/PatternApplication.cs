using System.Collections.Generic;
using System.Linq;
using automate.Extensions;

namespace automate
{
    internal class PatternApplication
    {
        private readonly IPathResolver pathResolver;
        private readonly PatternStore store;

        public PatternApplication(string currentDirectory) : this(new PatternStore(currentDirectory),
            new FilePathResolver())
        {
            currentDirectory.GuardAgainstNullOrEmpty(nameof(currentDirectory));
        }

        internal PatternApplication(PatternStore store, IPathResolver pathResolver)
        {
            store.GuardAgainstNull(nameof(store));
            pathResolver.GuardAgainstNull(nameof(pathResolver));
            this.store = store;
            this.pathResolver = pathResolver;
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

            var fullPath = this.pathResolver.CreatePath(rootPath, relativeFilePath);
            if (!this.pathResolver.ExistsAtPath(fullPath))
            {
                throw new PatternException(
                    ExceptionMessages.PatternApplication_CodeTemplate_NotFoundAtLocation.Format(rootPath,
                        relativeFilePath));
            }

            var templateName = string.IsNullOrEmpty(name)
                ? $"CodeTemplate{pattern.CodeTemplates.Count + 1}"
                : name;
            if (CodeTemplateExistsByName(templateName))
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

        private void VerifyCurrentPatternExists()
        {
            if (this.store.GetCurrent().NotExists())
            {
                throw new PatternException(ExceptionMessages.PatternApplication_NoCurrentPattern);
            }
        }

        private bool CodeTemplateExistsByName(string templateName)
        {
            templateName.GuardAgainstNullOrEmpty(nameof(templateName));

            var pattern = this.store.GetCurrent();
            if (pattern.Exists())
            {
                return pattern.CodeTemplates.Any(template => template.Name == templateName);
            }

            return false;
        }
    }
}