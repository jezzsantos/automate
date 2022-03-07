using System;
using System.Collections.Generic;
using System.Linq;
using Automate.CLI.Domain;
using Automate.CLI.Extensions;
using Automate.CLI.Infrastructure;
using Attribute = Automate.CLI.Domain.Attribute;

namespace Automate.CLI.Application
{
    internal class AuthoringApplication
    {
        private readonly IFilePathResolver fileResolver;
        private readonly IPatternToolkitPackager packager;
        private readonly IPatternPathResolver patternResolver;
        private readonly IPatternStore store;
        private readonly ITextTemplatingEngine textTemplatingEngine;

        public AuthoringApplication(string currentDirectory) : this(currentDirectory,
            new PatternStore(currentDirectory), new SystemIoFilePathResolver())
        {
        }

        private AuthoringApplication(string currentDirectory, IPatternStore store, IFilePathResolver fileResolver) :
            this(store, fileResolver, new PatternPathResolver(),
                new PatternToolkitPackager(store, new ToolkitStore(currentDirectory)), new TextTemplatingEngine())
        {
            currentDirectory.GuardAgainstNullOrEmpty(nameof(currentDirectory));
        }

        internal AuthoringApplication(IPatternStore store, IFilePathResolver fileResolver,
            IPatternPathResolver patternResolver, IPatternToolkitPackager packager, ITextTemplatingEngine textTemplatingEngine)
        {
            store.GuardAgainstNull(nameof(store));
            fileResolver.GuardAgainstNull(nameof(fileResolver));
            patternResolver.GuardAgainstNull(nameof(patternResolver));
            packager.GuardAgainstNull(nameof(packager));
            textTemplatingEngine.GuardAgainstNull(nameof(textTemplatingEngine));
            this.store = store;
            this.fileResolver = fileResolver;
            this.patternResolver = patternResolver;
            this.packager = packager;
            this.textTemplatingEngine = textTemplatingEngine;
        }

        public string CurrentPatternId => this.store.GetCurrent()?.Id;

        public string CurrentPatternName => this.store.GetCurrent()?.Name;

        public string CurrentPatternVersion => this.store.GetCurrent()?.ToolkitVersion;

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

            var fullPath = this.fileResolver.CreatePath(rootPath, relativeFilePath);
            if (!this.fileResolver.ExistsAtPath(fullPath))
            {
                throw new AutomateException(
                    ExceptionMessages.AuthoringApplication_CodeTemplate_NotFoundAtLocation.Format(rootPath,
                        relativeFilePath));
            }
            var extension = this.fileResolver.GetFileExtension(fullPath);

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
                : $"CodeTemplate{target.CodeTemplates.ToListSafe().Count + 1}";
            if (CodeTemplateExistsByName(target, templateName))
            {
                throw new AutomateException(ExceptionMessages.AuthoringApplication_CodeTemplateByNameExists
                    .Format(name));
            }

            var codeTemplate = new CodeTemplate(templateName, fullPath, extension);
            target.CodeTemplates.Add(codeTemplate);
            this.store.Save(pattern);

            var sourceFile = this.fileResolver.GetFileAtPath(fullPath);
            this.store.UploadCodeTemplate(pattern, codeTemplate.Id, sourceFile);

            return codeTemplate;
        }

        public List<CodeTemplate> ListCodeTemplates(string parentExpression)
        {
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

            return target.CodeTemplates.ToListSafe();
        }

        public (IPatternElement parent, Attribute child) AddAttribute(string name, string type, string defaultValue,
            bool isRequired, string isOneOf, string parentExpression)
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

            if (AttributeNameIsReserved(name))
            {
                throw new AutomateException(ExceptionMessages.AuthoringApplication_AttributeNameReserved.Format(name));
            }

            if (AttributeExistsByName(target, name))
            {
                throw new AutomateException(ExceptionMessages.AuthoringApplication_AttributeByNameExists.Format(name));
            }

            if (ElementExistsByName(target, name))
            {
                throw new AutomateException(ExceptionMessages.AuthoringApplication_AttributeByNameExistsAsElement.Format(name));
            }

            var choices = isOneOf.SafeSplit(";").ToList();
            var attribute = new Attribute(name, type, isRequired, defaultValue, choices);
            target.Attributes.Add(attribute);
            this.store.Save(pattern);

            return (target, attribute);
        }

        public (IPatternElement parent, IPatternElement child) AddElement(string name, string displayName,
            string description, bool isCollection, ElementCardinality cardinality,
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

            if (AttributeExistsByName(target, name))
            {
                throw new AutomateException(ExceptionMessages.AuthoringApplication_ElementByNameExistsAsAttribute.Format(name));
            }

            var element = new Element(name, displayName, description, isCollection, cardinality);
            target.Elements.Add(element);
            this.store.Save(pattern);

            return (target, element);
        }

        public CodeTemplateCommand AddCodeTemplateCommand(string codeTemplateName, string name, bool isTearOff,
            string filePath, string parentExpression)
        {
            codeTemplateName.GuardAgainstNullOrEmpty(nameof(codeTemplateName));
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

            var codeTemplate = target.CodeTemplates.Safe().FirstOrDefault(ele => ele.Name.EqualsIgnoreCase(codeTemplateName));
            if (codeTemplate.NotExists())
            {
                throw new AutomateException(parentExpression.HasValue()
                    ? ExceptionMessages.AuthoringApplication_CodeTemplateNotExistsElement.Format(codeTemplateName, parentExpression)
                    : ExceptionMessages.AuthoringApplication_CodeTemplateNotExistsRoot.Format(codeTemplateName, parentExpression));
            }
            var commandName = name.HasValue()
                ? name
                : $"CodeTemplateCommand{target.Automation.ToListSafe().Count + 1}";
            if (AutomationExistsByName(target, commandName))
            {
                throw new AutomateException(
                    ExceptionMessages.AuthoringApplication_AutomationByNameExists.Format(commandName));
            }

            var automation = new CodeTemplateCommand(commandName, codeTemplate.Id, isTearOff, filePath);
            target.Automation.Add(automation);
            this.store.Save(pattern);

            return automation;
        }

        public CommandLaunchPoint AddCommandLaunchPoint(string commandIds, string name, string parentExpression)
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
                : $"LaunchPoint{target.Automation.ToListSafe().Count + 1}";
            if (AutomationExistsByName(target, launchPointName))
            {
                throw new AutomateException(
                    ExceptionMessages.AuthoringApplication_AutomationByNameExists.Format(launchPointName));
            }

            var commandIdentifiers = commandIds.SafeSplit(";", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).ToList();
            commandIdentifiers.ForEach(cmdId =>
            {
                var codeTemplate = pattern.FindAutomation(cmdId);
                if (codeTemplate.NotExists())
                {
                    throw new AutomateException(ExceptionMessages.AuthoringApplication_CommandIdNotFound.Format(cmdId));
                }
            });

            var automation = new CommandLaunchPoint(launchPointName, commandIdentifiers);
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

        public string TestCodeTemplate(string codeTemplateName, string parentExpression, string rootPath, string importedRelativeFilePath, string exportedRelativeFilePath)
        {
            codeTemplateName.GuardAgainstNullOrEmpty(nameof(codeTemplateName));

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

            var codeTemplate = target.CodeTemplates.Safe().FirstOrDefault(ele => ele.Name.EqualsIgnoreCase(codeTemplateName));
            if (codeTemplate.NotExists())
            {
                throw new AutomateException(parentExpression.HasValue()
                    ? ExceptionMessages.AuthoringApplication_CodeTemplateNotExistsElement.Format(codeTemplateName, parentExpression)
                    : ExceptionMessages.AuthoringApplication_CodeTemplateNotExistsRoot.Format(codeTemplateName, parentExpression));
            }

            if (importedRelativeFilePath.HasValue())
            {
                var fullPath = this.fileResolver.CreatePath(rootPath, importedRelativeFilePath);
                if (!this.fileResolver.ExistsAtPath(fullPath))
                {
                    throw new AutomateException(
                        ExceptionMessages.AuthoringApplication_TestDataImport_NotFoundAtLocation.Format(fullPath));
                }

                var importedJson = this.fileResolver.GetFileAtPath(fullPath);
                Dictionary<string, object> importedData;
                try
                {
                    var json = SystemIoFileConstants.Encoding.GetString(importedJson.GetContents());
                    importedData = json.FromJson();
                }
                catch (Exception ex)
                {
                    throw new AutomateException(ExceptionMessages.AuthoringApplication_TestDataImport_NotValidJson, ex);
                }

                var contents = GetTemplateContent();
                return this.textTemplatingEngine.Transform(contents, importedData);
            }
            else
            {
                var solution = pattern.CreateTestSolution();
                var solutionItem = solution.FindByCodeTemplate(codeTemplate.Id);
                if (solutionItem.NotExists())
                {
                    throw new AutomateException(ExceptionMessages.AuthoringApplication_CodeTemplateNotExistsTestSolution.Format(codeTemplate.Id));
                }
                var generatedData = solutionItem.GetConfiguration(true);

                var contents = GetTemplateContent();
                var generatedCode = this.textTemplatingEngine.Transform(contents, generatedData);

                if (exportedRelativeFilePath.HasValue())
                {
                    var fullPath = this.fileResolver.CreatePath(rootPath, exportedRelativeFilePath);
                    try
                    {
                        this.fileResolver.CreateFileAtPath(fullPath, SystemIoFileConstants.Encoding.GetBytes(generatedData.ToJson()));
                    }
                    catch (Exception ex)
                    {
                        throw new AutomateException(ExceptionMessages.AuthoringApplication_TestDataExport_NotValidFile.Format(fullPath, ex.Message));
                    }
                }

                return generatedCode;
            }

            string GetTemplateContent()
            {
                var byteContents = this.store.DownloadCodeTemplate(pattern, codeTemplate);
                var contents = CodeTemplateFile.Encoding.GetString(byteContents);
                return contents;
            }
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
            return element.Automation.Safe().Any(ele => ele.Name.EqualsIgnoreCase(automationName));
        }

        private static bool ElementExistsByName(IElementContainer element, string elementName)
        {
            return element.Elements.Safe().Any(ele => ele.Name.EqualsIgnoreCase(elementName));
        }

        private static bool AttributeExistsByName(IAttributeContainer element, string attributeName)
        {
            return element.Attributes.Safe().Any(attr => attr.Name.EqualsIgnoreCase(attributeName));
        }

        private static bool AttributeNameIsReserved(string attributeName)
        {
            return Attribute.ReservedAttributeNames.Any(reserved => reserved.EqualsIgnoreCase(attributeName));
        }

        private static bool CodeTemplateExistsByName(IAutomationContainer element, string templateName)
        {
            return element.CodeTemplates.Safe().Any(template => template.Name.EqualsIgnoreCase(templateName));
        }
    }
}