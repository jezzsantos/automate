using System;
using System.Collections.Generic;
using System.IO;
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
        private readonly IRuntimeMetadata metadata;
        private readonly IPatternToolkitPackager packager;
        private readonly IPatternPathResolver patternResolver;
        private readonly IRecorder recorder;
        private readonly IPatternStore store;
        private readonly ITextTemplatingEngine textTemplatingEngine;

        public AuthoringApplication(IRecorder recorder, IPatternStore store, IFilePathResolver fileResolver,
            IPatternPathResolver patternResolver, IPatternToolkitPackager packager,
            ITextTemplatingEngine textTemplatingEngine, IApplicationExecutor applicationExecutor,
            IRuntimeMetadata metadata)
        {
            recorder.GuardAgainstNull(nameof(recorder));
            store.GuardAgainstNull(nameof(store));
            fileResolver.GuardAgainstNull(nameof(fileResolver));
            patternResolver.GuardAgainstNull(nameof(patternResolver));
            packager.GuardAgainstNull(nameof(packager));
            textTemplatingEngine.GuardAgainstNull(nameof(textTemplatingEngine));
            applicationExecutor.GuardAgainstNull(nameof(applicationExecutor));
            metadata.GuardAgainstNull(nameof(metadata));
            this.recorder = recorder;
            this.store = store;
            this.fileResolver = fileResolver;
            this.patternResolver = patternResolver;
            this.packager = packager;
            this.textTemplatingEngine = textTemplatingEngine;
            this.applicationExecutor = applicationExecutor;
            this.metadata = metadata;
        }

        public string CurrentPatternId => this.store.GetCurrent()?.Id;

        public string CurrentPatternName => this.store.GetCurrent()?.Name;

        public string CurrentPatternVersion => this.store.GetCurrent()?.ToolkitVersion.Current;

        public PatternDefinition GetCurrentPattern()
        {
            var pattern = EnsureCurrentPatternExists();

            this.recorder.MeasurePatternViewed(this.store.GetCurrent());
            return pattern;
        }

        public PatternDefinition CreateNewPattern(string name, string displayName, string description)
        {
            name.GuardAgainstNullOrEmpty(nameof(name));

            var pattern = new PatternDefinition(name, displayName, description);
            var created = this.store.Create(pattern);

            this.recorder.MeasurePatternCreated(created);
            return created;
        }

        public PatternDefinition UpdatePattern(string name, string displayName, string description)
        {
            var pattern = EnsureCurrentPatternExists();

            pattern.RenameAndDescribe(name, displayName, description);

            this.store.Save(pattern);

            this.recorder.MeasurePatternUpdated(pattern);
            return pattern;
        }

        public PatternDefinition SwitchCurrentPattern(string id)
        {
            id.GuardAgainstNullOrEmpty(nameof(id));

            var current = this.store.FindById(id);
            var pattern = this.store.ChangeCurrent(current.Id);

            this.recorder.MeasurePatternSwitched(pattern);
            return pattern;
        }

        public List<PatternDefinition> ListPatterns()
        {
            this.recorder.MeasurePatternsListed();
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

            this.recorder.MeasureAttributeAdded(pattern, attribute);
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

            this.recorder.MeasureAttributeUpdated(pattern, attribute);
            return (target, attribute);
        }

        public (IPatternElement Parent, Attribute Attribute) DeleteAttribute(string name, string parentExpression)
        {
            name.GuardAgainstNullOrEmpty(nameof(name));

            var pattern = EnsureCurrentPatternExists();
            var target = ResolveTargetElement(pattern, parentExpression);

            var attribute = target.DeleteAttribute(name);
            this.store.Save(pattern);

            this.recorder.MeasureAttributeDeleted(pattern, attribute);
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

            this.recorder.MeasureElementAdded(pattern, element);
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

            this.recorder.MeasureElementUpdated(pattern, element);
            return (target, element);
        }

        public (IPatternElement Parent, Element Element) DeleteElement(string name, string parentExpression)
        {
            name.GuardAgainstNullOrEmpty(nameof(name));

            var pattern = EnsureCurrentPatternExists();
            var target = ResolveTargetElement(pattern, parentExpression);

            var element = target.DeleteElement(name);
            this.store.Save(pattern);

            this.recorder.MeasureElementDeleted(pattern, element);
            return (target, element);
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

            this.recorder.MeasureCodeTemplateAdded(pattern, codeTemplate);
            return (target, new UploadedCodeTemplate(codeTemplate, location));
        }

        public (IPatternElement Parent, UploadedCodeTemplate Template, Automation Command) AddCodeTemplateWithCommand(
            string rootPath, string relativeFilePath, string codeTemplateName, string commandName, bool isOneOff,
            string filePath,
            string parentExpression)
        {
            rootPath.GuardAgainstNullOrEmpty(nameof(rootPath));
            relativeFilePath.GuardAgainstNullOrEmpty(nameof(relativeFilePath));
            filePath.GuardAgainstNullOrEmpty(nameof(filePath));

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

            var codeTemplate = target.AddCodeTemplate(codeTemplateName, fullPath, extension);
            this.store.Save(pattern);

            var sourceFile = this.fileResolver.GetFileAtPath(fullPath);
            var location = this.store.UploadCodeTemplate(pattern, codeTemplate.Id, sourceFile);

            var command = target.AddCodeTemplateCommand(commandName, codeTemplate.Name, isOneOff, filePath);
            this.store.Save(pattern);

            this.recorder.MeasureCodeTemplateWithCommandAdded(pattern, command, codeTemplate);
            return (target, new UploadedCodeTemplate(codeTemplate, location), command);
        }

        public (IPatternElement Parent, CodeTemplate Template, string Location) EditCodeTemplateContent(
            string templateName,
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

            this.recorder.MeasureCodeTemplateContentEdited(pattern, codeTemplate);
            return (target, codeTemplate, location);
        }

        public (IPatternElement Parent, CodeTemplate Template, string Location, string content) ViewCodeTemplateContent(
            string templateName, string parentExpression)
        {
            templateName.GuardAgainstNullOrEmpty(nameof(templateName));

            var pattern = EnsureCurrentPatternExists();
            var target = ResolveTargetElement(pattern, parentExpression);

            var codeTemplate = target.FindCodeTemplateByName(templateName);
            var location = this.store.GetCodeTemplateLocation(pattern, codeTemplate);
            var content = File.ReadAllText(location);

            this.recorder.MeasureCodeTemplateContentViewed(pattern, codeTemplate);
            return (target, codeTemplate, location, content);
        }

        public (IPatternElement Parent, CodeTemplate Template) DeleteCodeTemplate(string templateName,
            string parentExpression)
        {
            templateName.GuardAgainstNullOrEmpty(nameof(templateName));

            var pattern = EnsureCurrentPatternExists();
            var target = ResolveTargetElement(pattern, parentExpression);

            var codeTemplate = target.DeleteCodeTemplate(templateName, true);
            this.store.Save(pattern);

            this.store.DeleteCodeTemplate(pattern, codeTemplate);

            this.recorder.MeasureCodeTemplateDeleted(pattern, codeTemplate);
            return (target, codeTemplate);
        }

        public List<CodeTemplate> ListCodeTemplates(string parentExpression)
        {
            var pattern = EnsureCurrentPatternExists();
            var target = ResolveTargetElement(pattern, parentExpression);

            this.recorder.MeasureCodeTemplatesListed(pattern);
            return target.CodeTemplates.ToListSafe();
        }

        public (IPatternElement Parent, Automation Command) AddCodeTemplateCommand(string templateName, string name,
            bool isOneOff, string filePath, string parentExpression)
        {
            templateName.GuardAgainstNullOrEmpty(nameof(templateName));
            filePath.GuardAgainstNullOrEmpty(nameof(filePath));

            var pattern = EnsureCurrentPatternExists();
            var target = ResolveTargetElement(pattern, parentExpression);

            try
            {
                var command = target.AddCodeTemplateCommand(name, templateName, isOneOff, filePath);
                this.store.Save(pattern);

                this.recorder.MeasureCodeTemplateCommandAdded(pattern, command);
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

            this.recorder.MeasureCodeTemplateCommandUpdated(pattern, command);
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

            this.recorder.MeasureCliCommandAdded(pattern, command);
            return (target, command);
        }

        public (IPatternElement Parent, Automation Command) DeleteCommand(string commandName, string parentExpression)
        {
            commandName.GuardAgainstNull(nameof(commandName));

            var pattern = EnsureCurrentPatternExists();
            var target = ResolveTargetElement(pattern, parentExpression);

            var command = target.DeleteAutomation(commandName);
            this.store.Save(pattern);

            this.recorder.MeasureCommandDeleted(pattern, command);
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

            this.recorder.MeasureLaunchPointAdded(pattern, launchPoint);
            return (target, launchPoint);
        }

        public (IPatternElement Parent, Automation LaunchPoint) UpdateCommandLaunchPoint(string launchPointName,
            string name, List<string> addCommandIds, List<string> removeCommandIds,
            string sourceExpression, string parentExpression)
        {
            launchPointName.GuardAgainstNullOrEmpty(nameof(launchPointName));
            addCommandIds.GuardAgainstNull(nameof(addCommandIds));
            removeCommandIds.GuardAgainstNull(nameof(removeCommandIds));

            var pattern = EnsureCurrentPatternExists();
            var target = ResolveTargetElement(pattern, parentExpression);
            var source = sourceExpression.HasValue()
                ? ResolveTargetElement(pattern, sourceExpression)
                : target;

            var launchPoint =
                target.UpdateCommandLaunchPoint(launchPointName, name, addCommandIds, removeCommandIds, source);
            this.store.Save(pattern);

            this.recorder.MeasureLaunchPointUpdated(pattern, launchPoint);
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

            this.recorder.MeasureLaunchPointDeleted(pattern, launchPoint);
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
                this.recorder.MeasureCodeTemplateTestedWithImport(pattern, codeTemplate);
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
                this.recorder.MeasureCodeTemplateTestedWithExport(pattern, codeTemplate);
            }

            this.recorder.MeasureCodeTemplateTested(pattern, codeTemplate);
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

            return this.packager.PackAndExport(this.metadata, pattern,
                new VersionInstruction(versionInstruction, forceVersion));
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