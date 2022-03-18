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
        private readonly IFilePathResolver filePathResolver;
        private readonly IFileSystemWriter fileSystemWriter;
        private readonly ISolutionPathResolver solutionPathResolver;
        private readonly ITextTemplatingEngine textTemplatingEngine;

        public CodeTemplateCommand(string name, string codeTemplateId, bool isTearOff, string filePath) : this(
            new SystemIoFilePathResolver(), new SystemIoFileSystemWriter(), new SolutionPathResolver(),
            new TextTemplatingEngine(), name, codeTemplateId, isTearOff, filePath)
        {
        }

        internal CodeTemplateCommand(IFilePathResolver filePathResolver,
            IFileSystemWriter fileSystemWriter,
            ISolutionPathResolver solutionPathResolver,
            ITextTemplatingEngine textTemplatingEngine,
            string name, string codeTemplateId, bool isTearOff, string filePath) : this(new Automation(name, AutomationType.CodeTemplateCommand, new Dictionary<string, object>
        {
            { nameof(CodeTemplateId), codeTemplateId },
            { nameof(IsTearOff), isTearOff },
            { nameof(FilePath), filePath }
        }), filePathResolver, fileSystemWriter, solutionPathResolver, textTemplatingEngine)
        {
            codeTemplateId.GuardAgainstNullOrEmpty(nameof(codeTemplateId));
            filePath.GuardAgainstNullOrEmpty(nameof(filePath));
            filePath.GuardAgainstInvalid(Validations.IsRuntimeFilePath, nameof(filePath),
                ValidationMessages.Automation_InvalidFilePath);
        }

        private CodeTemplateCommand(Automation automation) : this(
            automation, new SystemIoFilePathResolver(), new SystemIoFileSystemWriter(), new SolutionPathResolver(), new TextTemplatingEngine())
        {
        }

        private CodeTemplateCommand(Automation automation,
            IFilePathResolver filePathResolver,
            IFileSystemWriter fileSystemWriter,
            ISolutionPathResolver solutionPathResolver,
            ITextTemplatingEngine textTemplatingEngine)
        {
            automation.GuardAgainstNull(nameof(automation));
            filePathResolver.GuardAgainstNull(nameof(filePathResolver));
            fileSystemWriter.GuardAgainstNull(nameof(fileSystemWriter));
            solutionPathResolver.GuardAgainstNull(nameof(solutionPathResolver));
            textTemplatingEngine.GuardAgainstNull(nameof(textTemplatingEngine));

            this.automation = automation;
            this.filePathResolver = filePathResolver;
            this.fileSystemWriter = fileSystemWriter;
            this.solutionPathResolver = solutionPathResolver;
            this.textTemplatingEngine = textTemplatingEngine;
        }

        public string CodeTemplateId => this.automation.Metadata[nameof(CodeTemplateId)].ToString();

        public string FilePath => this.automation.Metadata[nameof(FilePath)].ToString();

        public bool IsTearOff => this.automation.Metadata[nameof(IsTearOff)].ToString().ToBool();

        public static CodeTemplateCommand FromAutomation(Automation automation)
        {
            return new CodeTemplateCommand(automation);
        }

        public Automation AsAutomation()
        {
            return this.automation;
        }

        public string Id => this.automation.Id;

        public string Name => this.automation.Name;

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
                    ? CodeTemplateFile.Encoding.GetString(codeTemplate.Contents.ToArray())
                    : string.Empty;
                var generatedCode = this.textTemplatingEngine.Transform(DomainMessages.CodeTemplateCommand_TemplateContent_Description.Format(codeTemplate.Id), contents, target);

                this.fileSystemWriter.Write(generatedCode, absoluteFilePath);
                log.Add(DomainMessages.CodeTemplateCommand_Log_GeneratedFile.Format(filename, absoluteFilePath));
            }

            var link = target.ArtifactLinks.Safe()
                .FirstOrDefault(link => link.CommandId.EqualsIgnoreCase(Id));
            if (link.NotExists())
            {
                target.AddArtifactLink(Id, absoluteFilePath, filename);
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