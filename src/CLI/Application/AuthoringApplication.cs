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

        public string CurrentPatternVersion => this.store.GetCurrent()?.ToolkitVersion.Current;

        public PatternDefinition GetCurrentPattern()
        {
            return EnsureCurrentPatternExists();
        }

        public void CreateNewPattern(string name)
        {
            name.GuardAgainstNullOrEmpty(nameof(name));

            var pattern = new PatternDefinition(name);
            this.store.Create(pattern);
        }

        public void SwitchCurrentPattern(string name)
        {
            name.GuardAgainstNullOrEmpty(nameof(name));

            var current = this.store.Find(name);
            this.store.ChangeCurrent(current.Id);
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

        public (IPatternElement Parent, Attribute Attribute) DeleteAttribute(string name, string parentExpression)
        {
            name.GuardAgainstNullOrEmpty(nameof(name));

            var pattern = EnsureCurrentPatternExists();
            var target = ResolveTargetElement(pattern, parentExpression);

            var attribute = target.DeleteAttribute(name);
            this.store.Save(pattern);

            return (target, attribute);
        }

        public (IPatternElement Parent, Element Element) AddElement(string name, string displayName,
            string description, bool isCollection, ElementCardinality cardinality,
            string parentExpression)
        {
            name.GuardAgainstNullOrEmpty(nameof(name));

            var pattern = EnsureCurrentPatternExists();
            var target = ResolveTargetElement(pattern, parentExpression);

            var element = target.AddElement(name, displayName, description, isCollection, cardinality);
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

        public UploadedCodeTemplate AttachCodeTemplate(string rootPath, string relativeFilePath, string name,
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

            var codeTemplate = target.AttachCodeTemplate(name, fullPath, extension);
            this.store.Save(pattern);

            var sourceFile = this.fileResolver.GetFileAtPath(fullPath);
            var location = this.store.UploadCodeTemplate(pattern, codeTemplate.Id, sourceFile);

            return new UploadedCodeTemplate(codeTemplate, location);
        }

        public List<CodeTemplate> ListCodeTemplates(string parentExpression)
        {
            var pattern = EnsureCurrentPatternExists();
            var target = ResolveTargetElement(pattern, parentExpression);

            return target.CodeTemplates.ToListSafe();
        }

        public Automation AddCodeTemplateCommand(string codeTemplateName, string name, bool isTearOff,
            string filePath, string parentExpression)
        {
            codeTemplateName.GuardAgainstNullOrEmpty(nameof(codeTemplateName));
            filePath.GuardAgainstNullOrEmpty(nameof(filePath));

            var pattern = EnsureCurrentPatternExists();
            var target = ResolveTargetElement(pattern, parentExpression);

            try
            {
                var command = target.AddCodeTemplateCommand(name, codeTemplateName, isTearOff, filePath);
                this.store.Save(pattern);

                return command;
            }
            catch (AutomateException ex)
            {
                if (ex.Message == ExceptionMessages.PatternElement_CodeTemplateNoFound.Format(codeTemplateName))
                {
                    throw new AutomateException(parentExpression.HasValue()
                        ? ExceptionMessages.AuthoringApplication_CodeTemplateNotExistsElement.Format(codeTemplateName, parentExpression)
                        : ExceptionMessages.AuthoringApplication_CodeTemplateNotExistsRoot.Format(codeTemplateName, parentExpression));
                }

                throw;
            }
        }

        public Automation AddCommandLaunchPoint(string name, List<string> commandIds, string parentExpression)
        {
            commandIds.GuardAgainstNull(nameof(commandIds));

            var pattern = EnsureCurrentPatternExists();
            var target = ResolveTargetElement(pattern, parentExpression);

            var launchPoint = target.AddCommandLaunchPoint(name, commandIds, pattern);
            this.store.Save(pattern);

            return launchPoint;
        }

        public Automation UpdateCommandLaunchPoint(string name, List<string> commandIds, string sourceExpression, string parentExpression)
        {
            name.GuardAgainstNullOrEmpty(nameof(name));
            commandIds.GuardAgainstNull(nameof(commandIds));

            var pattern = EnsureCurrentPatternExists();
            var target = ResolveTargetElement(pattern, parentExpression);
            var source = ResolveTargetElement(pattern, sourceExpression);

            var launchPoint = target.UpdateCommandLaunchPoint(name, commandIds, source, pattern);
            this.store.Save(pattern);

            return launchPoint;
        }

        public CodeTemplateTest TestCodeTemplate(string codeTemplateName, string parentExpression, string rootPath, string importedRelativeFilePath, string exportedRelativeFilePath)
        {
            codeTemplateName.GuardAgainstNullOrEmpty(nameof(codeTemplateName));

            var pattern = EnsureCurrentPatternExists();
            var target = ResolveTargetElement(pattern, parentExpression);

            var codeTemplate = target.CodeTemplates.Safe().FirstOrDefault(ele => ele.Name.EqualsIgnoreCase(codeTemplateName));
            if (codeTemplate.NotExists())
            {
                throw new AutomateException(parentExpression.HasValue()
                    ? ExceptionMessages.AuthoringApplication_CodeTemplateNotExistsElement.Format(codeTemplateName, parentExpression)
                    : ExceptionMessages.AuthoringApplication_CodeTemplateNotExistsRoot.Format(codeTemplateName, parentExpression));
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
                return this.textTemplatingEngine.Transform(DomainMessages.AuthoringApplication_TestCodeTemplate_Description.Format(codeTemplate.Id), textTemplate, data);
            }

            string GenerateGeneratedCode(LazySolutionItemDictionary data)
            {
                return this.textTemplatingEngine.Transform(DomainMessages.AuthoringApplication_TestCodeTemplate_Description.Format(codeTemplate.Id), textTemplate, data);
            }

            string GetTemplateContent()
            {
                var byteContents = this.store.DownloadCodeTemplate(pattern, codeTemplate);
                var contents = CodeTemplateFile.Encoding.GetString(byteContents);
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

            LazySolutionItemDictionary GenerateTestData()
            {
                var solution = pattern.CreateTestSolution();
                var solutionItem = solution.FindByCodeTemplate(codeTemplate.Id);
                if (solutionItem.NotExists())
                {
                    throw new AutomateException(ExceptionMessages.AuthoringApplication_CodeTemplateNotExistsTestSolution.Format(codeTemplate.Id));
                }
                return solutionItem.GetConfiguration(true);
            }

            void ExportResult(LazySolutionItemDictionary data)
            {
                var fullPath = this.fileResolver.CreatePath(rootPath, exportedRelativeFilePath);
                try
                {
                    this.fileResolver.CreateFileAtPath(fullPath, SystemIoFileConstants.Encoding.GetBytes(data.ToJson()));
                }
                catch (Exception ex)
                {
                    throw new AutomateException(ExceptionMessages.AuthoringApplication_TestDataExport_NotValidFile.Format(fullPath, ex.Message));
                }
            }
        }

        public ToolkitPackage PackageToolkit(string versionInstruction, bool forceVersion)
        {
            var pattern = EnsureCurrentPatternExists();

            return this.packager.Pack(pattern, new VersionInstruction(versionInstruction, forceVersion));
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