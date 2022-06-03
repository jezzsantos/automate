using System;
using System.Collections.Generic;
using System.Linq;
using Automate.CLI.Extensions;
using Automate.CLI.Infrastructure;

namespace Automate.CLI.Domain
{
    internal class CodeTemplateCommand : IAutomation
    {
        private const char CurrentDirectoryPrefix = '~';
        private readonly Automation automation;
        private readonly IDraftPathResolver draftPathResolver;
        private readonly IFilePathResolver filePathResolver;
        private readonly IFileSystemWriter fileSystemWriter;
        private readonly ITextTemplatingEngine textTemplatingEngine;

        public CodeTemplateCommand(string name, string codeTemplateId, bool isOneOff, string filePath) : this(
            new SystemIoFilePathResolver(), new SystemIoFileSystemWriter(), new DraftPathResolver(),
            new TextTemplatingEngine(), name, codeTemplateId, isOneOff, filePath)
        {
        }

        internal CodeTemplateCommand(IFilePathResolver filePathResolver,
            IFileSystemWriter fileSystemWriter,
            IDraftPathResolver draftPathResolver,
            ITextTemplatingEngine textTemplatingEngine,
            string name, string codeTemplateId, bool isOneOff, string filePath) : this(new Automation(name,
            AutomationType.CodeTemplateCommand, new Dictionary<string, object>
            {
                { nameof(CodeTemplateId), codeTemplateId },
                { nameof(IsOneOff), isOneOff },
                { nameof(FilePath), filePath }
            }), filePathResolver, fileSystemWriter, draftPathResolver, textTemplatingEngine)
        {
            codeTemplateId.GuardAgainstNullOrEmpty(nameof(codeTemplateId));
            filePath.GuardAgainstNullOrEmpty(nameof(filePath));
            filePath.GuardAgainstInvalid(Validations.IsRuntimeFilePath, nameof(filePath),
                ValidationMessages.Automation_InvalidFilePath);
        }

        private CodeTemplateCommand(Automation automation) : this(
            automation, new SystemIoFilePathResolver(), new SystemIoFileSystemWriter(), new DraftPathResolver(),
            new TextTemplatingEngine())
        {
        }

        private CodeTemplateCommand(Automation automation,
            IFilePathResolver filePathResolver,
            IFileSystemWriter fileSystemWriter,
            IDraftPathResolver draftPathResolver,
            ITextTemplatingEngine textTemplatingEngine)
        {
            automation.GuardAgainstNull(nameof(automation));
            filePathResolver.GuardAgainstNull(nameof(filePathResolver));
            fileSystemWriter.GuardAgainstNull(nameof(fileSystemWriter));
            draftPathResolver.GuardAgainstNull(nameof(draftPathResolver));
            textTemplatingEngine.GuardAgainstNull(nameof(textTemplatingEngine));

            this.automation = automation;
            this.filePathResolver = filePathResolver;
            this.fileSystemWriter = fileSystemWriter;
            this.draftPathResolver = draftPathResolver;
            this.textTemplatingEngine = textTemplatingEngine;
        }

        public string CodeTemplateId => this.automation.Metadata[nameof(CodeTemplateId)].ToString();

        public string FilePath => this.automation.Metadata[nameof(FilePath)].ToString();

        public bool IsOneOff => this.automation.Metadata[nameof(IsOneOff)].ToString().ToBool();

        public static CodeTemplateCommand FromAutomation(Automation automation)
        {
            return new CodeTemplateCommand(automation);
        }

        public Automation AsAutomation()
        {
            return this.automation;
        }

        public void ChangeName(string name)
        {
            this.automation.Rename(name);
        }

        public void ChangeOneOff(bool isOneOff)
        {
            this.automation.UpdateMetadata(nameof(IsOneOff), isOneOff);
        }

        public void ChangeFilePath(string filePath)
        {
            filePath.GuardAgainstNullOrEmpty(nameof(filePath));
            filePath.GuardAgainstInvalid(Validations.IsRuntimeFilePath, nameof(filePath),
                ValidationMessages.Automation_InvalidFilePath);

            this.automation.UpdateMetadata(nameof(FilePath), filePath);
        }

        public string Id => this.automation.Id;

        public string Name => this.automation.Name;

        public CommandExecutionResult Execute(DraftDefinition draft, DraftItem target)
        {
            var log = new List<string>();

            var codeTemplate = draft.Toolkit.CodeTemplateFiles.Safe().FirstOrDefault(ctf => ctf.Id == CodeTemplateId);
            if (codeTemplate.NotExists())
            {
                throw new AutomateException(
                    ExceptionMessages.CodeTemplateCommand_TemplateNotExists.Format(CodeTemplateId));
            }

            var existingLink = target.ArtifactLinks.Safe()
                .FirstOrDefault(link => link.CommandId.EqualsIgnoreCase(Id));

            var destinationFilePath = this.draftPathResolver.ResolveExpression(
                DomainMessages.CodeTemplateCommand_FilePathExpression_Description.Format(Id), FilePath, target);
            var destinationFullPath = destinationFilePath.StartsWith(CurrentDirectoryPrefix)
                ? this.filePathResolver.CreatePath(Environment.CurrentDirectory,
                    destinationFilePath.TrimStart(CurrentDirectoryPrefix).TrimStart('\\', '/'))
                : destinationFilePath;
            var destinationFileExists = this.fileSystemWriter.Exists(destinationFullPath);
            var existingLinkChangedLocation = existingLink.Exists()
                ? existingLink.Path.NotEqualsIgnoreCase(destinationFullPath)
                : false;

            var shouldMoveOldFile = IsOneOff && !destinationFileExists && existingLinkChangedLocation;
            var shouldWriteFile = shouldMoveOldFile == false && (!IsOneOff || (IsOneOff && !destinationFileExists));

            if (shouldWriteFile)
            {
                GenerateAndOverwriteFile(target, codeTemplate, destinationFullPath, log);
            }

            if (shouldMoveOldFile)
            {
                MoveExistingFile(existingLink, destinationFullPath, log);
            }

            UpdateArtifactLink(target, existingLink, existingLinkChangedLocation, destinationFileExists,
                destinationFullPath, log);

            return new CommandExecutionResult(Name, log);
        }

        private void GenerateAndOverwriteFile(DraftItem target, CodeTemplateFile codeTemplate,
            string destinationFullPath, List<string> log)
        {
            var destinationFilename = this.filePathResolver.GetFilename(destinationFullPath);

            var contents = codeTemplate.Contents.Exists()
                ? CodeTemplateFile.Encoding.GetString(codeTemplate.Contents.ToArray())
                : string.Empty;
            var generatedCode = this.textTemplatingEngine.Transform(
                DomainMessages.CodeTemplateCommand_TemplateContent_Description.Format(codeTemplate.Id), contents,
                target);

            this.fileSystemWriter.Write(generatedCode, destinationFullPath);
            log.Add(DomainMessages.CodeTemplateCommand_Log_GeneratedFile.Format(destinationFilename,
                destinationFullPath));
        }

        private void MoveExistingFile(ArtifactLink existingLink, string destinationFullPath, List<string> log)
        {
            var oldFilePath = existingLink.Path;
            this.fileSystemWriter.Move(oldFilePath, destinationFullPath);
            log.Add(DomainMessages.CodeTemplateCommand_Log_Warning_Moved.Format(oldFilePath, destinationFullPath));
        }

        private void UpdateArtifactLink(DraftItem target, ArtifactLink existingLink, bool existingLinkChangedLocation,
            bool destinationFileExists, string destinationFullPath, List<string> log)
        {
            var destinationFilename = this.filePathResolver.GetFilename(destinationFullPath);

            if (existingLink.Exists())
            {
                var oldFilePath = existingLink.Path;
                if (existingLinkChangedLocation)
                {
                    if (IsOneOff)
                    {
                        if (destinationFileExists)
                        {
                            this.fileSystemWriter.Delete(oldFilePath);
                            log.Add(DomainMessages.CodeTemplateCommand_Log_Warning_Deleted.Format(oldFilePath));
                        }
                    }
                    else
                    {
                        this.fileSystemWriter.Delete(oldFilePath);
                        log.Add(DomainMessages.CodeTemplateCommand_Log_Warning_Deleted.Format(oldFilePath));
                    }
                }
                existingLink.UpdatePathAndTag(destinationFullPath, destinationFilename);
            }
            else
            {
                target.AddArtifactLink(Id, destinationFullPath, destinationFilename);
                if (IsOneOff)
                {
                    log.Add(DomainMessages.CodeTemplateCommand_Log_UpdatedLink.Format(destinationFilename,
                        destinationFullPath));
                }
            }
        }
    }
}