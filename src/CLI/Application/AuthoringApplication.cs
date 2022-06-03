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
        private readonly IApplicationExecutor applictionExecutor;
        private readonly IFilePathResolver fileResolver;
        private readonly IPatternToolkitPackager packager;
        private readonly IPatternPathResolver patternResolver;
        private readonly IPatternStore store;
        private readonly ITextTemplatingEngine textTemplatingEngine;

        public AuthoringApplication(string currentDirectory) : this(currentDirectory,
            new PatternStore(currentDirectory), new SystemIoFilePathResolver(), new ApplicationExecutor())
        {
        }

        private AuthoringApplication(string currentDirectory, IPatternStore store, IFilePathResolver fileResolver,
            IApplicationExecutor applicationExecutor) :
            this(store, fileResolver, new PatternPathResolver(),
                new PatternToolkitPackager(store, new ToolkitStore(currentDirectory)), new TextTemplatingEngine(),
                applicationExecutor)
        {
            currentDirectory.GuardAgainstNullOrEmpty(nameof(currentDirectory));
        }

        internal AuthoringApplication(IPatternStore store, IFilePathResolver fileResolver,
            IPatternPathResolver patternResolver, IPatternToolkitPackager packager,
            ITextTemplatingEngine textTemplatingEngine, IApplicationExecutor applicationExecutor)
        {
            store.GuardAgainstNull(nameof(store));
            fileResolver.GuardAgainstNull(nameof(fileResolver));
            patternResolver.GuardAgainstNull(nameof(patternResolver));
            packager.GuardAgainstNull(nameof(packager));
            textTemplatingEngine.GuardAgainstNull(nameof(textTemplatingEngine));
            applicationExecutor.GuardAgainstNull(nameof(applicationExecutor));
            this.store = store;
            this.fileResolver = fileResolver;
            this.patternResolver = patternResolver;
            this.packager = packager;
            this.textTemplatingEngine = textTemplatingEngine;
            this.applictionExecutor = applicationExecutor;
        }

        public string CurrentPatternId => this.store.GetCurrent()?.Id;

        public string CurrentPatternName => this.store.GetCurrent()?.Name;

        public string CurrentPatternVersion => this.store.GetCurrent()?.ToolkitVersion.Current;

        public PatternDefinition GetCurrentPattern()
        {
            return EnsureCurrentPatternExists();
        }

        public void CreateNewPattern(string name, string displayName, string description)
        {
            name.GuardAgainstNullOrEmpty(nameof(name));

            var pattern = new PatternDefinition(name, displayName, description);
            this.store.Create(pattern);
        }

        public PatternDefinition UpdatePattern(string name, string displayName, string description)
        {
            name.GuardAgainstNullOrEmpty(nameof(name));

            var pattern = EnsureCurrentPatternExists();

            pattern.RenameAndDescribe(name, displayName, description);

            this.store.Save(pattern);

            return pattern;
        }

        public void SwitchCurrentPattern(string name)
        {
            name.GuardAgainstNullOrEmpty(nameof(name));

            var current = this.store.Find(name);
            this.store.ChangeCurrent(current.Id);
        }

        public List<PatternDefinition> ListPatterns()
        {
            return this.store.ListAll();
        }

        public (IPatternElement Parent, Attribute Attribute) AddAttribute(string name, string type, string defaultValue,
            bool isRequired, List<string> choices, string parentExpression)
        {
            name.GuardAgainstNullOrEmpty(nameof(name));

            var pattern = EnsureCurrentPatternExists();
            var target = ResolveTargetElement(pattern, parentExpression);

            var attribute = target.AddAttribute(name, type, isRequired, defaultValue, choices);
            this.store.Save(pattern);

            return (target, attribute);
        }

        public (IPatternElement Parent, Attribute Attribute) UpdateAttribute(string attributeName, string name,
            string type, string defaultValue, bool? isRequired, List<string> choices, string parentExpression)
        {
            attributeName.GuardAgainstNullOrEmpty(nameof(attributeName));

            var pattern = EnsureCurrentPatternExists();
            var target = ResolveTargetElement(pattern, parentExpression);

            var attribute = target.UpdateAttribute(attributeName, name, type, isRequired, defaultValue, choices);
            this.store.Save(pattern);

            return (target, attribute);
        }

        public (IPatternElement Parent, Attribute Attribute) DeleteAttribute(string name, string parentExpression)
        {
            name.GuardAgainstNullOrEmpty(nameof(name));

            var pattern = EnsureCurrentPatternExists();
            var target = ResolveTargetElement(pattern, parentExpression);

            var attribute = target.DeleteAttribute(name);
            this.store.Save(pattern);

            return (target, attribute);
        }

        public (IPatternElement Parent, Element Element) AddElement(string name, ElementCardinality cardinality,
            bool autoCreate, string displayName, string description, string parentExpression)
        {
            name.GuardAgainstNullOrEmpty(nameof(name));

            var pattern = EnsureCurrentPatternExists();
            var target = ResolveTargetElement(pattern, parentExpression);

            var element = target.AddElement(name, cardinality, autoCreate, displayName, description);
            this.store.Save(pattern);

            return (target, element);
        }

        public (IPatternElement Parent, Element Element) UpdateElement(string elementName, string name,
            bool? isRequired, bool? autoCreate, string displayName, string description, string parentExpression)
        {
            elementName.GuardAgainstNullOrEmpty(nameof(elementName));

            var pattern = EnsureCurrentPatternExists();
            var target = ResolveTargetElement(pattern, parentExpression);

            var element = target.UpdateElement(elementName, name, isRequired, autoCreate, displayName, description);
            this.store.Save(pattern);

            return (target, element);
        }

        public (IPatternElement Parent, Element Element) DeleteElement(string name, string parentExpression)
        {
            name.GuardAgainstNullOrEmpty(nameof(name));

            var pattern = EnsureCurrentPatternExists();
            var target = ResolveTargetElement(pattern, parentExpression);

            var attribute = target.DeleteElement(name);
            this.store.Save(pattern);

            return (target, attribute);
        }

        public (IPatternElement Parent, UploadedCodeTemplate Template) AttachCodeTemplate(string rootPath,
            string relativeFilePath, string name,
            string parentExpression)
        {
            rootPath.GuardAgainstNullOrEmpty(nameof(rootPath));
            relativeFilePath.GuardAgainstNullOrEmpty(nameof(relativeFilePath));

            var pattern = EnsureCurrentPatternExists();

            var fullPath = this.fileResolver.CreatePath(rootPath, relativeFilePath);
            if (!this.fileResolver.ExistsAtPath(fullPath))
            {
                throw new AutomateException(
                    ExceptionMessages.AuthoringApplication_CodeTemplate_NotFoundAtLocation.Format(rootPath,
                        relativeFilePath));
            }
            var extension = this.fileResolver.GetFileExtension(fullPath);

            var target = ResolveTargetElement(pattern, parentExpression);

            var codeTemplate = target.AddCodeTemplate(name, fullPath, extension);
            this.store.Save(pattern);

            var sourceFile = this.fileResolver.GetFileAtPath(fullPath);
            var location = this.store.UploadCodeTemplate(pattern, codeTemplate.Id, sourceFile);

            return (target, new UploadedCodeTemplate(codeTemplate, location));
        }

        public (IPatternElement Parent, CodeTemplate Template, string Location) EditCodeTemplate(string templateName,
            string editorPath, string arguments,
            string parentExpression)
        {
            templateName.GuardAgainstNullOrEmpty(nameof(templateName));
            editorPath.GuardAgainstNullOrEmpty(nameof(editorPath));

            var pattern = EnsureCurrentPatternExists();
            var target = ResolveTargetElement(pattern, parentExpression);

            var codeTemplate = target.FindCodeTemplateByName(templateName);
            var location = this.store.GetCodeTemplateLocation(pattern, codeTemplate);
            var args = arguments.HasValue()
                ? $"{arguments} {location}"
                : location;

            var result = this.applictionExecutor.RunApplicationProcess(false, editorPath, args);
            if (!result.IsSuccess)
            {
                throw new AutomateException(
                    ExceptionMessages.AuthoringApplication_EditingCodeTemplateError.Format(editorPath, result.Error));
            }

            return (target, codeTemplate, location);
        }

        public (IPatternElement Parent, CodeTemplate Template) DeleteCodeTemplate(string templateName,
            string parentExpression)
        {
            templateName.GuardAgainstNullOrEmpty(nameof(templateName));

            var pattern = EnsureCurrentPatternExists();
            var target = ResolveTargetElement(pattern, parentExpression);

            var template = target.DeleteCodeTemplate(templateName, true);
            this.store.Save(pattern);

            return (target, template);
        }

        public List<CodeTemplate> ListCodeTemplates(string parentExpression)
        {
            var pattern = EnsureCurrentPatternExists();
            var target = ResolveTargetElement(pattern, parentExpression);

            return target.CodeTemplates.ToListSafe();
        }

        public Automation AddCodeTemplateCommand(string templateName, string name, bool isOneOff,
            string filePath, string parentExpression)
        {
            templateName.GuardAgainstNullOrEmpty(nameof(templateName));
            filePath.GuardAgainstNullOrEmpty(nameof(filePath));

            var pattern = EnsureCurrentPatternExists();
            var target = ResolveTargetElement(pattern, parentExpression);

            try
            {
                var command = target.AddCodeTemplateCommand(name, templateName, isOneOff, filePath);
                this.store.Save(pattern);

                return command;
            }
            catch (AutomateException ex)
            {
                if (ex.Message == ExceptionMessages.PatternElement_CodeTemplateNotFound.Format(templateName))
                {
                    throw new AutomateException(parentExpression.HasValue()
                        ? ExceptionMessages.AuthoringApplication_CodeTemplateNotExistsElement.Format(templateName,
                            parentExpression)
                        : ExceptionMessages.AuthoringApplication_CodeTemplateNotExistsRoot.Format(templateName,
                            parentExpression));
                }

                throw;
            }
        }

        public Automation UpdateCodeTemplateCommand(string commandName, string name, bool? isOneOff, string filePath,
            string parentExpression)
        {
            commandName.GuardAgainstNullOrEmpty(nameof(commandName));

            var pattern = EnsureCurrentPatternExists();
            var target = ResolveTargetElement(pattern, parentExpression);

            var command = target.UpdateCodeTemplateCommand(commandName, name, isOneOff, filePath);
            this.store.Save(pattern);

            return command;
        }

        public Automation AddCliCommand(string applicationName, string arguments, string name, string parentExpression)
        {
            applicationName.GuardAgainstNull(nameof(applicationName));

            var pattern = EnsureCurrentPatternExists();
            var target = ResolveTargetElement(pattern, parentExpression);

            var command = target.AddCliCommand(name, applicationName, arguments);
            this.store.Save(pattern);

            return command;
        }

        public Automation UpdateCliCommand(string commandName, string name, string applicationName, string arguments,
            string parentExpression)
        {
            commandName.GuardAgainstNull(nameof(commandName));

            var pattern = EnsureCurrentPatternExists();
            var target = ResolveTargetElement(pattern, parentExpression);

            var command = target.UpdateCliCommand(commandName, name, applicationName, arguments);
            this.store.Save(pattern);

            return command;
        }

        public (IPatternElement Parent, Automation Command) DeleteCommand(string commandName, string parentExpression)
        {
            commandName.GuardAgainstNull(nameof(commandName));

            var pattern = EnsureCurrentPatternExists();
            var target = ResolveTargetElement(pattern, parentExpression);

            var command = target.DeleteAutomation(commandName);
            this.store.Save(pattern);

            return (target, command);
        }

        public Automation AddCommandLaunchPoint(string name, List<string> commandIds, string parentExpression)
        {
            commandIds.GuardAgainstNull(nameof(commandIds));

            var pattern = EnsureCurrentPatternExists();
            var target = ResolveTargetElement(pattern, parentExpression);

            var launchPoint = target.AddCommandLaunchPoint(name, commandIds);
            this.store.Save(pattern);

            return launchPoint;
        }

        public Automation UpdateCommandLaunchPoint(string launchPointName, string name, List<string> commandIds,
            string sourceExpression, string parentExpression)
        {
            launchPointName.GuardAgainstNullOrEmpty(nameof(launchPointName));
            commandIds.GuardAgainstNull(nameof(commandIds));

            var pattern = EnsureCurrentPatternExists();
            var target = ResolveTargetElement(pattern, parentExpression);
            var source = ResolveTargetElement(pattern, sourceExpression);

            var launchPoint = target.UpdateCommandLaunchPoint(launchPointName, name, commandIds, source);
            this.store.Save(pattern);

            return launchPoint;
        }

        public (IPatternElement Parent, Automation LaunchPoint) DeleteCommandLaunchPoint(string launchPointName,
            string parentExpression)
        {
            launchPointName.GuardAgainstNull(nameof(launchPointName));

            var pattern = EnsureCurrentPatternExists();
            var target = ResolveTargetElement(pattern, parentExpression);

            var launchPoint = target.DeleteAutomation(launchPointName);
            this.store.Save(pattern);

            return (target, launchPoint);
        }

        public CodeTemplateTest TestCodeTemplate(string codeTemplateName, string parentExpression, string rootPath,
            string importedRelativeFilePath, string exportedRelativeFilePath)
        {
            codeTemplateName.GuardAgainstNullOrEmpty(nameof(codeTemplateName));

            var pattern = EnsureCurrentPatternExists();
            var target = ResolveTargetElement(pattern, parentExpression);

            var codeTemplate = target.CodeTemplates.Safe()
                .FirstOrDefault(template => template.Name.EqualsIgnoreCase(codeTemplateName));
            if (codeTemplate.NotExists())
            {
                throw new AutomateException(parentExpression.HasValue()
                    ? ExceptionMessages.AuthoringApplication_CodeTemplateNotExistsElement.Format(codeTemplateName,
                        parentExpression)
                    : ExceptionMessages.AuthoringApplication_CodeTemplateNotExistsRoot.Format(codeTemplateName,
                        parentExpression));
            }

            var textTemplate = GetTemplateContent();

            if (importedRelativeFilePath.HasValue())
            {
                var importedData = ImportData();
                return new CodeTemplateTest(codeTemplate, GenerateImportedCode(importedData));
            }

            var generatedData = GenerateTestData();
            var code = GenerateGeneratedCode(generatedData);
            if (exportedRelativeFilePath.HasValue())
            {
                ExportResult(generatedData);
            }

            return new CodeTemplateTest(codeTemplate, code);

            string GenerateImportedCode(Dictionary<string, object> data)
            {
                return this.textTemplatingEngine.Transform(
                    ApplicationMessages.AuthoringApplication_TestCodeTemplate_Description.Format(codeTemplate.Id),
                    textTemplate, data);
            }

            string GenerateGeneratedCode(LazyDraftItemDictionary data)
            {
                return this.textTemplatingEngine.Transform(
                    ApplicationMessages.AuthoringApplication_TestCodeTemplate_Description.Format(codeTemplate.Id),
                    textTemplate, data);
            }

            string GetTemplateContent()
            {
                var byteContents = this.store.DownloadCodeTemplate(pattern, codeTemplate);
                var contents = CodeTemplateFile.Encoding.GetString(byteContents.Content);
                return contents;
            }

            Dictionary<string, object> ImportData()
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

                return importedData;
            }

            LazyDraftItemDictionary GenerateTestData()
            {
                var draft = pattern.CreateTestDraft();
                var draftItem = draft.FindByCodeTemplate(codeTemplate.Id);
                if (draftItem.NotExists())
                {
                    throw new AutomateException(
                        ExceptionMessages.AuthoringApplication_CodeTemplateNotExistsTestDraft
                            .Format(codeTemplate.Id));
                }
                return draftItem.GetConfiguration(true);
            }

            void ExportResult(LazyDraftItemDictionary data)
            {
                var fullPath = this.fileResolver.CreatePath(rootPath, exportedRelativeFilePath);
                try
                {
                    this.fileResolver.CreateFileAtPath(fullPath,
                        SystemIoFileConstants.Encoding.GetBytes(data.ToJson()));
                }
                catch (Exception ex)
                {
                    throw new AutomateException(
                        ExceptionMessages.AuthoringApplication_TestDataExport_NotValidFile
                            .Format(fullPath, ex.Message));
                }
            }
        }

        public ToolkitPackage BuildAndExportToolkit(string versionInstruction, bool forceVersion)
        {
            var pattern = EnsureCurrentPatternExists();

            return this.packager.PackAndExport(pattern, new VersionInstruction(versionInstruction, forceVersion));
        }

        private IPatternElement ResolveTargetElement(PatternDefinition pattern, string parentExpression)
        {
            if (parentExpression.HasValue())
            {
                var target = this.patternResolver.Resolve(pattern, parentExpression);
                if (target.NotExists())
                {
                    throw new AutomateException(
                        ExceptionMessages.AuthoringApplication_PathExpressionNotFound.Format(parentExpression));
                }

                return target;
            }

            return pattern;
        }

        private PatternDefinition EnsureCurrentPatternExists()
        {
            var pattern = this.store.GetCurrent();

            if (pattern.NotExists())
            {
                throw new AutomateException(ExceptionMessages.AuthoringApplication_NoCurrentPattern);
            }

            return pattern;
        }
    }
}