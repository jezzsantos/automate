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
        private readonly IFilePathResolver filePathResolver;
        private readonly IFileSystemWriter fileSystemWriter;
        private readonly ISolutionPathResolver solutionPathResolver;
        private readonly ITextTemplatingEngine textTemplatingEngine;

        public CodeTemplateCommand(string id, string name, string codeTemplateId, bool isTearOff, string filePath) : this(
            new SystemIoFilePathResolver(), new SystemIoFileSystemWriter(), new SolutionPathResolver(),
            new TextTemplatingEngine(), id, name, codeTemplateId, isTearOff, filePath)
        {
        }

        public CodeTemplateCommand(string id, string name, Dictionary<string, object> metadata) : this(
            new SystemIoFilePathResolver(), new SystemIoFileSystemWriter(), new SolutionPathResolver(),
            new TextTemplatingEngine(), id, name, metadata[nameof(CodeTemplateId)].ToString(), metadata[nameof(IsTearOff)].ToString().ToBool(), metadata[nameof(FilePath)].ToString())
        {
        }

        internal CodeTemplateCommand(IFilePathResolver filePathResolver,
            IFileSystemWriter fileSystemWriter,
            ISolutionPathResolver solutionPathResolver,
            ITextTemplatingEngine textTemplatingEngine,
            string id, string name, string codeTemplateId, bool isTearOff, string filePath)
        {
            id.GuardAgainstNullOrEmpty(nameof(id));
            id.GuardAgainstInvalid(IdGenerator.IsValid, nameof(id),
                ValidationMessages.InvalidIdentifier);
            name.GuardAgainstNullOrEmpty(nameof(name));
            name.GuardAgainstInvalid(Validations.IsNameIdentifier, nameof(name),
                ValidationMessages.InvalidNameIdentifier);
            filePathResolver.GuardAgainstNull(nameof(filePathResolver));
            fileSystemWriter.GuardAgainstNull(nameof(fileSystemWriter));
            solutionPathResolver.GuardAgainstNull(nameof(solutionPathResolver));
            textTemplatingEngine.GuardAgainstNull(nameof(textTemplatingEngine));
            codeTemplateId.GuardAgainstNullOrEmpty(nameof(codeTemplateId));
            filePath.GuardAgainstNullOrEmpty(nameof(filePath));
            filePath.GuardAgainstInvalid(Validations.IsRuntimeFilePath, nameof(filePath),
                ValidationMessages.Automation_InvalidFilePath);

            this.filePathResolver = filePathResolver;
            this.fileSystemWriter = fileSystemWriter;
            this.solutionPathResolver = solutionPathResolver;
            this.textTemplatingEngine = textTemplatingEngine;
            Id = id;
            Name = name;
            IsTearOff = isTearOff;
            FilePath = filePath;
            CodeTemplateId = codeTemplateId;
        }

        public string CodeTemplateId { get; }

        public string FilePath { get; }

        public bool IsTearOff { get; }

        public string Id { get; }

        public string Name { get; }

        public CommandExecutionResult Execute(SolutionDefinition solution, SolutionItem target)
        {
            var log = new List<string>();

            var codeTemplate = solution.Toolkit.CodeTemplateFiles.Safe().FirstOrDefault(ctf => ctf.Id == CodeTemplateId);
            if (codeTemplate.NotExists())
            {
                throw new AutomateException(
                    ExceptionMessages.CodeTemplateCommand_TemplateNotExists.Format(CodeTemplateId));
            }

            var filePath = this.solutionPathResolver.ResolveExpression(DomainMessages.CodeTemplateCommand_FilePathExpression_Description.Format(Id), FilePath, target);
            var absoluteFilePath = filePath.StartsWith(CurrentDirectoryPrefix)
                ? this.filePathResolver.CreatePath(Environment.CurrentDirectory,
                    filePath.TrimStart(CurrentDirectoryPrefix).TrimStart('\\', '/'))
                : filePath;
            var filename = this.filePathResolver.GetFilename(absoluteFilePath);

            var fileExists = this.fileSystemWriter.Exists(absoluteFilePath);
            var willGenerateFile = !IsTearOff || IsTearOff && !fileExists;
            if (willGenerateFile)
            {
                var contents = codeTemplate.Contents.Exists()
                    ? CodeTemplateFile.Encoding.GetString(codeTemplate.Contents)
                    : string.Empty;
                var generatedCode = this.textTemplatingEngine.Transform(DomainMessages.CodeTemplateCommand_TemplateContent_Description.Format(codeTemplate.Id), contents, target);

                this.fileSystemWriter.Write(generatedCode, absoluteFilePath);
                log.Add(DomainMessages.CodeTemplateCommand_Log_GeneratedFile.Format(filename, absoluteFilePath));
            }

            var link = target.ArtifactLinks.Safe()
                .FirstOrDefault(link => link.CommandId.EqualsIgnoreCase(Id));
            if (link.NotExists())
            {
                link = new ArtifactLink(Id, absoluteFilePath, filename);
                if (target.ArtifactLinks.NotExists())
                {
                    target.ArtifactLinks = new List<ArtifactLink>();
                }
                target.ArtifactLinks.Add(link);
                if (IsTearOff)
                {
                    log.Add(DomainMessages.CodeTemplateCommand_Log_UpdatedLink.Format(filename, absoluteFilePath));
                }
            }
            else
            {
                link.UpdatePathAndTag(absoluteFilePath, filename);
            }

            return new CommandExecutionResult(Name, log);
        }
    }
}