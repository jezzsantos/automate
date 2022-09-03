using System;
using System.Collections.Generic;
using System.Linq;
using Automate.Authoring.Domain;
using Automate.Authoring.Infrastructure;
using Automate.Common;
using Automate.Common.Application;
using Automate.Common.Domain;
using Automate.Common.Extensions;
using Automate.Runtime.Application;
using Automate.Runtime.Domain;
using Attribute = Automate.Authoring.Domain.Attribute;

namespace Automate.Authoring.Application
{
    public class AuthoringApplication
    {
        private readonly IApplicationExecutor applicationExecutor;
        private readonly IFilePathResolver fileResolver;
        private readonly IPatternToolkitPackager packager;
        private readonly IPatternPathResolver patternResolver;
        private readonly IPatternStore store;
        private readonly ITextTemplatingEngine textTemplatingEngine;

        public AuthoringApplication(IPatternStore store, IFilePathResolver fileResolver,
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
            this.applicationExecutor = applicationExecutor;
        }

        public string CurrentPatternId => this.store.GetCurrent()?.Id;

        public string CurrentPatternName => this.store.GetCurrent()?.Name;

        public string CurrentPatternVersion => this.store.GetCurrent()?.ToolkitVersion.Current;

        public PatternDefinition GetCurrentPattern()
        {
            return EnsureCurrentPatternExists();
        }

        public PatternDefinition CreateNewPattern(string name, string displayName, string description)
        {
            name.GuardAgainstNullOrEmpty(nameof(name));

            var pattern = new PatternDefinition(name, displayName, description);
            this.store.Create(pattern);

            return pattern;
        }

        public PatternDefinition UpdatePattern(string name, string displayName, string description)
        {
            name.GuardAgainstNullOrEmpty(nameof(name));

            var pattern = EnsureCurrentPatternExists();

            pattern.RenameAndDescribe(name, displayName, description);

            this.store.Save(pattern);

            return pattern;
        }

        public void SwitchCurrentPattern(string id)
        {
            id.GuardAgainstNullOrEmpty(nameof(id));

            var current = this.store.FindById(id);
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

        public (IPatternElement Parent, UploadedCodeTemplate Template) AddCodeTemplate(string rootPath,
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
                    ExceptionMessages.AuthoringApplication_CodeTemplate_NotFoundAtLocation.Substitute(rootPath,
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

        public (IPatternElement Parent, UploadedCodeTemplate Template, Automation Command) AddCodeTemplateWithCommand(
            string rootPath, string relativeFilePath, string name, bool isOneOff, string targetPath,
            string parentExpression)
        {
            rootPath.GuardAgainstNullOrEmpty(nameof(rootPath));
            relativeFilePath.GuardAgainstNullOrEmpty(nameof(relativeFilePath));
            targetPath.GuardAgainstNullOrEmpty(nameof(targetPath));

            var pattern = EnsureCurrentPatternExists();

            var fullPath = this.fileResolver.CreatePath(rootPath, relativeFilePath);
            if (!this.fileResolver.ExistsAtPath(fullPath))
            {
                throw new AutomateException(
                    ExceptionMessages.AuthoringApplication_CodeTemplate_NotFoundAtLocation.Substitute(rootPath,
                        relativeFilePath));
            }
            var extension = this.fileResolver.GetFileExtension(fullPath);

            var target = ResolveTargetElement(pattern, parentExpression);

            var codeTemplate = target.AddCodeTemplate(name, fullPath, extension);
            this.store.Save(pattern);

            var sourceFile = this.fileResolver.GetFileAtPath(fullPath);
            var location = this.store.UploadCodeTemplate(pattern, codeTemplate.Id, sourceFile);

            var count = target.Automation.Count + 1;
            var commandName = $"{name}Command{count}";
            var command = target.AddCodeTemplateCommand(commandName, codeTemplate.Name, isOneOff, targetPath);
            this.store.Save(pattern);

            return (target, new UploadedCodeTemplate(codeTemplate, location), command);
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

            var result = this.applicationExecutor.RunApplicationProcess(false, editorPath, args);
            if (!result.IsSuccess)
            {
                throw new AutomateException(
                    ExceptionMessages.AuthoringApplication_EditingCodeTemplateError
                        .Substitute(editorPath, result.Error));
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

            this.store.DeleteCodeTemplate(pattern, template);

            return (target, template);
        }

        public List<CodeTemplate> ListCodeTemplates(string parentExpression)
        {
            var pattern = EnsureCurrentPatternExists();
            var target = ResolveTargetElement(pattern, parentExpression);

            return target.CodeTemplates.ToListSafe();
        }

        public (IPatternElement Parent, Automation Command) AddCodeTemplateCommand(string templateName, string name,
            bool isOneOff, string targetPath, string parentExpression)
        {
            templateName.GuardAgainstNullOrEmpty(nameof(templateName));
            targetPath.GuardAgainstNullOrEmpty(nameof(targetPath));

            var pattern = EnsureCurrentPatternExists();
            var target = ResolveTargetElement(pattern, parentExpression);

            try
            {
                var command = target.AddCodeTemplateCommand(name, templateName, isOneOff, targetPath);
                this.store.Save(pattern);

                return (target, command);
            }
            catch (AutomateException ex)
            {
                if (ex.Message == ExceptionMessages.PatternElement_CodeTemplateNotFound.Substitute(templateName))
                {
                    throw new AutomateException(parentExpression.HasValue()
                        ? ExceptionMessages.AuthoringApplication_CodeTemplateNotExistsElement.Substitute(templateName,
                            parentExpression)
                        : ExceptionMessages.AuthoringApplication_CodeTemplateNotExistsRoot.Substitute(templateName,
                            parentExpression));
                }

                throw;
            }
        }

        public (IPatternElement Parent, Automation Command) UpdateCodeTemplateCommand(string commandName, string name,
            bool? isOneOff, string filePath,
            string parentExpression)
        {
            commandName.GuardAgainstNullOrEmpty(nameof(commandName));

            var pattern = EnsureCurrentPatternExists();
            var target = ResolveTargetElement(pattern, parentExpression);

            var command = target.UpdateCodeTemplateCommand(commandName, name, isOneOff, filePath);
            this.store.Save(pattern);

            return (target, command);
        }

        public (IPatternElement Parent, Automation Command) AddCliCommand(string applicationName, string arguments,
            string name, string parentExpression)
        {
            applicationName.GuardAgainstNull(nameof(applicationName));

            var pattern = EnsureCurrentPatternExists();
            var target = ResolveTargetElement(pattern, parentExpression);

            var command = target.AddCliCommand(name, applicationName, arguments);
            this.store.Save(pattern);

            return (target, command);
        }

        public (IPatternElement Parent, Automation Command) UpdateCliCommand(string commandName, string name,
            string applicationName, string arguments,
            string parentExpression)
        {
            commandName.GuardAgainstNull(nameof(commandName));

            var pattern = EnsureCurrentPatternExists();
            var target = ResolveTargetElement(pattern, parentExpression);

            var command = target.UpdateCliCommand(commandName, name, applicationName, arguments);
            this.store.Save(pattern);

            return (target, command);
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

        public (IPatternElement Parent, Automation LaunchPoint) AddCommandLaunchPoint(string name,
            List<string> commandIds, string sourceExpression, string parentExpression)
        {
            commandIds.GuardAgainstNull(nameof(commandIds));

            var pattern = EnsureCurrentPatternExists();
            var target = ResolveTargetElement(pattern, parentExpression);
            var source = sourceExpression.HasValue()
                ? ResolveTargetElement(pattern, sourceExpression)
                : target;

            var launchPoint = target.AddCommandLaunchPoint(name, commandIds, source);
            this.store.Save(pattern);

            return (target, launchPoint);
        }

        public (IPatternElement Parent, Automation LaunchPoint) UpdateCommandLaunchPoint(string launchPointName,
            string name, List<string> commandIds,
            string sourceExpression, string parentExpression)
        {
            launchPointName.GuardAgainstNullOrEmpty(nameof(launchPointName));
            commandIds.GuardAgainstNull(nameof(commandIds));

            var pattern = EnsureCurrentPatternExists();
            var target = ResolveTargetElement(pattern, parentExpression);
            var source = sourceExpression.HasValue()
                ? ResolveTargetElement(pattern, sourceExpression)
                : target;

            var launchPoint = target.UpdateCommandLaunchPoint(launchPointName, name, commandIds, source);
            this.store.Save(pattern);

            return (target, launchPoint);
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

            var codeTemplate = Try.Safely(() => target.FindCodeTemplateByName(codeTemplateName));
            if (codeTemplate.NotExists())
            {
                throw new AutomateException(parentExpression.HasValue()
                    ? ExceptionMessages.AuthoringApplication_CodeTemplateNotExistsElement.Substitute(codeTemplateName,
                        parentExpression)
                    : ExceptionMessages.AuthoringApplication_CodeTemplateNotExistsRoot.Substitute(codeTemplateName,
                        parentExpression));
            }

            var textTemplate = GetTemplateContent();

            if (importedRelativeFilePath.HasValue())
            {
                var importedData = ImportData(this.fileResolver, rootPath, importedRelativeFilePath);
                var importedOutput = GenerateImportedCode(importedData);
                return new CodeTemplateTest(codeTemplate, importedOutput);
            }

            var generatedData = GenerateTestData(true);
            var output = GenerateGeneratedCode(generatedData);

            var exportedFilePath = string.Empty;
            if (exportedRelativeFilePath.HasValue())
            {
                var generatedDataForExport = GenerateTestData(false);
                exportedFilePath =
                    ExportResult(this.fileResolver, generatedDataForExport, rootPath, exportedRelativeFilePath);
            }

            return new CodeTemplateTest(codeTemplate, output, exportedFilePath);

            string GenerateImportedCode(Dictionary<string, object> data)
            {
                return this.textTemplatingEngine.Transform(
                    ApplicationMessages.AuthoringApplication_TestCodeTemplate_Description.Substitute(codeTemplate.Id),
                    textTemplate, data);
            }

            string GenerateGeneratedCode(LazyDraftItemDictionary data)
            {
                return this.textTemplatingEngine.Transform(
                    ApplicationMessages.AuthoringApplication_TestCodeTemplate_Description.Substitute(codeTemplate.Id),
                    textTemplate, data);
            }

            string GetTemplateContent()
            {
                var byteContents = this.store.DownloadCodeTemplate(pattern, codeTemplate);
                var contents = CodeTemplateFile.Encoding.GetString(byteContents.Content);
                return contents;
            }

            LazyDraftItemDictionary GenerateTestData(bool includeAncestry)
            {
                var draft = pattern.CreateTestDraft();
                var draftItem = draft.FindByCodeTemplate(codeTemplate.Id);
                if (draftItem.NotExists())
                {
                    throw new AutomateException(
                        ExceptionMessages.AuthoringApplication_CodeTemplateNotExistsTestDraft
                            .Substitute(codeTemplate.Id));
                }
                return draftItem.GetConfiguration(includeAncestry, false);
            }
        }

        public CodeTemplateCommandTest TestCodeTemplateCommand(string commandName, string parentExpression,
            string rootPath,
            string importedRelativeFilePath, string exportedRelativeFilePath)
        {
            commandName.GuardAgainstNullOrEmpty(nameof(commandName));

            var pattern = EnsureCurrentPatternExists();
            var target = ResolveTargetElement(pattern, parentExpression);

            var command = Try.Safely(() => target.FindCodeTemplateCommandByName(commandName));
            if (command.NotExists())
            {
                throw new AutomateException(parentExpression.HasValue()
                    ? ExceptionMessages.AuthoringApplication_CodeTemplateCommandNotExistsElement.Substitute(commandName,
                        parentExpression)
                    : ExceptionMessages.AuthoringApplication_CodeTemplateCommandNotExistsRoot.Substitute(commandName,
                        parentExpression));
            }

            var textTemplate = GetFilePathTemplate();

            if (importedRelativeFilePath.HasValue())
            {
                var importedData = ImportData(this.fileResolver, rootPath, importedRelativeFilePath);
                var importedOutput = GenerateImportedCode(importedData);
                return new CodeTemplateCommandTest(command, importedOutput);
            }

            var generatedData = GenerateTestData(true);
            var output = GenerateGeneratedCode(generatedData);

            var exportedFilePath = string.Empty;
            if (exportedRelativeFilePath.HasValue())
            {
                var generatedDataForExport = GenerateTestData(false);
                exportedFilePath =
                    ExportResult(this.fileResolver, generatedDataForExport, rootPath, exportedRelativeFilePath);
            }

            return new CodeTemplateCommandTest(command, output, exportedFilePath);

            string GetFilePathTemplate()
            {
                return command.FilePath;
            }

            string GenerateImportedCode(Dictionary<string, object> data)
            {
                return this.textTemplatingEngine.Transform(
                    ApplicationMessages.AuthoringApplication_TestCodeTemplate_Description.Substitute(command.Id),
                    textTemplate, data);
            }

            string GenerateGeneratedCode(LazyDraftItemDictionary data)
            {
                return this.textTemplatingEngine.Transform(
                    ApplicationMessages.AuthoringApplication_TestCodeTemplate_Description.Substitute(command.Id),
                    textTemplate, data);
            }

            LazyDraftItemDictionary GenerateTestData(bool includeAncestry)
            {
                var draft = pattern.CreateTestDraft();
                var draftItems = draft.FindByAutomation(command.Id);
                if (draftItems.HasNone())
                {
                    throw new AutomateException(
                        ExceptionMessages.AuthoringApplication_CodeTemplateNotExistsTestDraft
                            .Substitute(command.Id));
                }
                return draftItems.First().DraftItem.GetConfiguration(includeAncestry, false);
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
                        ExceptionMessages.AuthoringApplication_PathExpressionNotFound.Substitute(parentExpression));
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

        private static Dictionary<string, object> ImportData(IFilePathResolver fileResolver, string rootPath,
            string importedRelativeFilePath)
        {
            var fullPath = fileResolver.CreatePath(rootPath, importedRelativeFilePath);
            if (!fileResolver.ExistsAtPath(fullPath))
            {
                throw new AutomateException(
                    ExceptionMessages.AuthoringApplication_TestDataImport_NotFoundAtLocation.Substitute(fullPath));
            }

            var importedJson = fileResolver.GetFileAtPath(fullPath);
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

        private static string ExportResult(IFilePathResolver fileResolver, LazyDraftItemDictionary data,
            string rootPath,
            string exportedRelativeFilePath)
        {
            var fullPath = fileResolver.CreatePath(rootPath, exportedRelativeFilePath);
            try
            {
                fileResolver.CreateFileAtPath(fullPath,
                    SystemIoFileConstants.Encoding.GetBytes(data.ToJson()));
                return fullPath;
            }
            catch (Exception ex)
            {
                throw new AutomateException(
                    ExceptionMessages.AuthoringApplication_TestDataExport_NotValidFile
                        .Substitute(fullPath, ex.Message));
            }
        }
    }
}