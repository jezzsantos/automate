using System;
using System.Collections.Generic;
using System.Linq;
using Automate.CLI.Extensions;
using Automate.CLI.Infrastructure;

namespace Automate.CLI.Domain
{
    internal class CodeTemplateCommand : Automation
    {
        private const char CurrentDirectoryPrefix = '~';
        private readonly IFilePathResolver filePathResolver;
        private readonly IFileSystemWriter fileSystemWriter;
        private readonly ISolutionPathResolver solutionPathResolver;
        private readonly ITextTemplatingEngine textTemplatingEngine;

        public CodeTemplateCommand(string name, string codeTemplateId, bool isTearOff, string filePath) : this(
            new SystemIoFilePathResolver(), new SystemIoFileSystemWriter(), new SolutionPathResolver(),
            new TextTemplatingEngine(), name, codeTemplateId, isTearOff, filePath)
        {
        }

        public CodeTemplateCommand(IFilePathResolver filePathResolver,
            IFileSystemWriter fileSystemWriter,
            ISolutionPathResolver solutionPathResolver,
            ITextTemplatingEngine textTemplatingEngine,
            string name, string codeTemplateId, bool isTearOff, string filePath) : base(name)
        {
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
            IsTearOff = isTearOff;
            FilePath = filePath;
            CodeTemplateId = codeTemplateId;
        }

        /// <summary>
        ///     For serialization
        /// </summary>
        public CodeTemplateCommand()
        {
            this.filePathResolver = new SystemIoFilePathResolver();
            this.fileSystemWriter = new SystemIoFileSystemWriter();
            this.solutionPathResolver = new SolutionPathResolver();
            this.textTemplatingEngine = new TextTemplatingEngine();
        }

        public string CodeTemplateId { get; set; }

        public string FilePath { get; set; }

        public bool IsTearOff { get; set; }

        public override CommandExecutionResult Execute(SolutionDefinition solution, SolutionItem target)
        {
            var log = new List<string>();

            var codeTemplate = solution.Toolkit.CodeTemplateFiles.Safe().FirstOrDefault(ctf => ctf.Id == CodeTemplateId);
            if (codeTemplate.NotExists())
            {
                throw new AutomateException(
                    ExceptionMessages.CodeTemplateCommand_TemplateNotExists.Format(CodeTemplateId));
            }

            var filePath = this.solutionPathResolver.ResolveExpression($"CodeTemplateCommand ({Id}) path expression", FilePath, target);
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
                var generatedCode = this.textTemplatingEngine.Transform($"CodeTemplate ({codeTemplate.Id}) template", contents, target);

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