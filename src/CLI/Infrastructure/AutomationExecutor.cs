using System;
using System.Collections.Generic;
using System.Linq;
using Automate.Authoring.Application;
using Automate.Authoring.Domain;
using Automate.Common;
using Automate.Common.Application;
using Automate.Common.Domain;
using Automate.Common.Extensions;
using Automate.Runtime.Application;
using Automate.Runtime.Domain;

namespace Automate.CLI.Infrastructure
{
    public class AutomationExecutor : IAutomationExecutor
    {
        private readonly IApplicationExecutor applicationExecutor;
        private readonly IDraftPathResolver draftPathResolver;
        private readonly IFilePathResolver filePathResolver;
        private readonly IFileSystemReaderWriter fileSystem;
        private readonly ITextTemplatingEngine textTemplatingEngine;

        public AutomationExecutor(IFilePathResolver filePathResolver,
            IFileSystemReaderWriter fileSystem,
            IDraftPathResolver draftPathResolver,
            ITextTemplatingEngine textTemplatingEngine,
            IApplicationExecutor applicationExecutor)
        {
            filePathResolver.GuardAgainstNull(nameof(filePathResolver));
            fileSystem.GuardAgainstNull(nameof(fileSystem));
            draftPathResolver.GuardAgainstNull(nameof(draftPathResolver));
            textTemplatingEngine.GuardAgainstNull(nameof(textTemplatingEngine));
            applicationExecutor.GuardAgainstNull(nameof(applicationExecutor));
            this.filePathResolver = filePathResolver;
            this.fileSystem = fileSystem;
            this.draftPathResolver = draftPathResolver;
            this.textTemplatingEngine = textTemplatingEngine;
            this.applicationExecutor = applicationExecutor;
        }

        public void Execute(CommandExecutionResult result)
        {
            ExecuteAutomation(result, result.ExecutableContext.Executable, result.ExecutableContext.Executable.Type);
        }

        private void ExecuteAutomation(CommandExecutionResult result, IAutomation automation, AutomationType type)
        {
            switch (type)
            {
                case AutomationType.CodeTemplateCommand:
                    new CodeTemplateCommandExecutor(this.filePathResolver, this.fileSystem, this.draftPathResolver,
                            this.textTemplatingEngine)
                        .Execute(automation as CodeTemplateCommand, result);
                    break;

                case AutomationType.CliCommand:
                    new CliCommandExecutor(this.draftPathResolver, this.applicationExecutor)
                        .Execute(automation as CliCommand, result);
                    break;

                case AutomationType.CommandLaunchPoint:
                    new CommandLaunchPointExecutor()
                        .Execute(automation as CommandLaunchPoint, result, ExecuteAutomation);
                    break;

#if TESTINGONLY
                case AutomationType.TestingOnlyLaunchable:
                    result.Record("testingonly");
                    break;
#endif
                case AutomationType.Unknown:
                default:
                    throw new AutomateException(
                        ExceptionMessages.AutomationService_UnknownAutomationType.Substitute(type));
            }
        }
    }

    public class CodeTemplateCommandExecutor
    {
        private const char CurrentDirectoryPrefix = '~';
        private readonly IDraftPathResolver draftPathResolver;
        private readonly IFilePathResolver filePathResolver;
        private readonly IFileSystemReaderWriter fileSystem;
        private readonly ITextTemplatingEngine textTemplatingEngine;

        internal CodeTemplateCommandExecutor(
            IFilePathResolver filePathResolver,
            IFileSystemReaderWriter fileSystem,
            IDraftPathResolver draftPathResolver,
            ITextTemplatingEngine textTemplatingEngine)
        {
            filePathResolver.GuardAgainstNull(nameof(filePathResolver));
            fileSystem.GuardAgainstNull(nameof(fileSystem));
            draftPathResolver.GuardAgainstNull(nameof(draftPathResolver));
            textTemplatingEngine.GuardAgainstNull(nameof(textTemplatingEngine));

            this.filePathResolver = filePathResolver;
            this.fileSystem = fileSystem;
            this.draftPathResolver = draftPathResolver;
            this.textTemplatingEngine = textTemplatingEngine;
        }

        public void Execute(CodeTemplateCommand command, CommandExecutionResult result)
        {
            var log = new List<string>();

            var codeTemplate = result.ExecutableContext.Draft.Toolkit.CodeTemplateFiles.Safe()
                .FirstOrDefault(ctf => ctf.Id == command.CodeTemplateId);
            if (codeTemplate.NotExists())
            {
                throw new AutomateException(
                    ExceptionMessages.CodeTemplateCommand_TemplateNotExists.Substitute(command.CodeTemplateId));
            }

            var existingLink = result.ExecutableContext.Item.ArtifactLinks.Safe()
                .FirstOrDefault(link => link.CommandId.EqualsIgnoreCase(command.Id));

            var destinationFilePath = this.draftPathResolver.ResolveExpression(
                InfrastructureMessages.CodeTemplateCommand_FilePathExpression_Description.Substitute(command.Id),
                command.FilePath, result.ExecutableContext.Item);
            var destinationFullPath = destinationFilePath.StartsWith(CurrentDirectoryPrefix)
                ? this.filePathResolver.CreatePath(Environment.CurrentDirectory,
                    destinationFilePath.TrimStart(CurrentDirectoryPrefix).TrimStart('\\', '/'))
                : destinationFilePath;
            var destinationFileExists = this.fileSystem.FileExists(destinationFullPath);
            var existingLinkChangedLocation = existingLink.Exists()
                ? existingLink.Path.NotEqualsIgnoreCase(destinationFullPath)
                : false;

            var shouldMoveOldFile = command.IsOneOff && !destinationFileExists && existingLinkChangedLocation;
            var shouldWriteFile = shouldMoveOldFile == false &&
                                  (!command.IsOneOff || (command.IsOneOff && !destinationFileExists));

            if (shouldWriteFile)
            {
                GenerateAndOverwriteFile(command, result.ExecutableContext.Item, codeTemplate, destinationFullPath,
                    log);
            }

            if (shouldMoveOldFile)
            {
                MoveExistingFile(existingLink, destinationFullPath, log);
            }

            UpdateArtifactLink(command, result.ExecutableContext.Item, existingLink, existingLinkChangedLocation,
                destinationFileExists,
                destinationFullPath, log);

            result.Record(log);
        }

        private void GenerateAndOverwriteFile(CodeTemplateCommand command, DraftItem target,
            CodeTemplateFile codeTemplate,
            string destinationFullPath, List<string> log)
        {
            var destinationFilename = this.filePathResolver.GetFilename(destinationFullPath);

            var contents = codeTemplate.Contents.Exists()
                ? CodeTemplateFile.Encoding.GetString(codeTemplate.Contents.ToArray())
                : string.Empty;
            var generatedCode = this.textTemplatingEngine.Transform(
                InfrastructureMessages.CodeTemplateCommand_TemplateContent_Description.Substitute(codeTemplate.Id),
                contents,
                target);

            this.fileSystem.Write(generatedCode, destinationFullPath);
            log.Add(InfrastructureMessages.CodeTemplateCommand_Log_GeneratedFile.Substitute(destinationFilename,
                command.CodeTemplateId,
                destinationFullPath));
        }

        private void MoveExistingFile(ArtifactLink existingLink, string destinationFullPath, List<string> log)
        {
            var oldFilePath = existingLink.Path;
            this.fileSystem.Move(oldFilePath, destinationFullPath);
            log.Add(InfrastructureMessages.CodeTemplateCommand_Log_Warning_Moved.Substitute(oldFilePath,
                destinationFullPath));
        }

        private void UpdateArtifactLink(CodeTemplateCommand command, DraftItem target, ArtifactLink existingLink,
            bool existingLinkChangedLocation,
            bool destinationFileExists, string destinationFullPath, List<string> log)
        {
            var destinationFilename = this.filePathResolver.GetFilename(destinationFullPath);

            if (existingLink.Exists())
            {
                var oldFilePath = existingLink.Path;
                if (existingLinkChangedLocation)
                {
                    if (command.IsOneOff)
                    {
                        if (destinationFileExists)
                        {
                            this.fileSystem.Delete(oldFilePath);
                            log.Add(InfrastructureMessages.CodeTemplateCommand_Log_Warning_Deleted.Substitute(
                                oldFilePath));
                        }
                        else
                        {
                            log.Add(InfrastructureMessages.CodeTemplateCommand_Log_UpdatedLink.Substitute(
                                destinationFilename,
                                destinationFullPath));
                        }
                    }
                    else
                    {
                        this.fileSystem.Delete(oldFilePath);
                        log.Add(InfrastructureMessages.CodeTemplateCommand_Log_Warning_Deleted.Substitute(oldFilePath));
                    }
                }
                existingLink.UpdatePathAndTag(destinationFullPath, destinationFilename);
            }
            else
            {
                target.AddArtifactLink(command.Id, destinationFullPath, destinationFilename);
                if (command.IsOneOff)
                {
                    log.Add(InfrastructureMessages.CodeTemplateCommand_Log_UpdatedLink.Substitute(destinationFilename,
                        destinationFullPath));
                }
            }
        }
    }

    public class CliCommandExecutor
    {
        private readonly IApplicationExecutor applicationExecutor;
        private readonly IDraftPathResolver draftPathResolver;

        internal CliCommandExecutor(IDraftPathResolver draftPathResolver,
            IApplicationExecutor applicationExecutor)
        {
            draftPathResolver.GuardAgainstNull(nameof(draftPathResolver));
            applicationExecutor.GuardAgainstNull(nameof(applicationExecutor));

            this.draftPathResolver = draftPathResolver;
            this.applicationExecutor = applicationExecutor;
        }

        public void Execute(CliCommand command, CommandExecutionResult result)
        {
            var applicationName = command.ApplicationName.HasValue()
                ? this.draftPathResolver.ResolveExpression(
                    InfrastructureMessages.CliCommand_ApplicationName_Description.Substitute(command.Id),
                    command.ApplicationName, result.ExecutableContext.Item)
                : string.Empty;
            var arguments = command.Arguments.HasValue()
                ? this.draftPathResolver.ResolveExpression(
                    InfrastructureMessages.CliCommand_Arguments_Description.Substitute(command.Id),
                    command.Arguments,
                    result.ExecutableContext.Item)
                : string.Empty;

            var execution = this.applicationExecutor.RunApplicationProcess(true, applicationName, arguments);
            if (execution.IsSuccess)
            {
                result.Record(execution.Output);
            }
            else
            {
                result.Fail(execution.Error);
            }
        }
    }

    public class CommandLaunchPointExecutor
    {
        public void Execute(CommandLaunchPoint launchPoint, CommandExecutionResult result,
            Action<CommandExecutionResult, IAutomation, AutomationType> automationExecutor)
        {
            launchPoint.CommandIds.ToListSafe().ForEach(cmdId =>
            {
                var commands = result.ExecutableContext.Draft.FindByAutomation(cmdId);
                if (commands.HasNone())
                {
                    throw new AutomateException(
                        ExceptionMessages.CommandLaunchPoint_CommandIdNotFound.Substitute(cmdId));
                }

                commands.ForEach(pair => ExecuteCommandSafely(pair.Automation, pair.DraftItem, cmdId));
            });

            void ExecuteCommandSafely(IAutomationSchema command, DraftItem draftItem, string cmdId)
            {
                try
                {
                    var executable = command.GetExecutable(result.ExecutableContext.Draft, draftItem);
                    if (executable.IsLaunchable)
                    {
                        automationExecutor(result, executable, executable.Type);
                    }
                }
                catch (Exception ex)
                {
                    result.Fail(
                        InfrastructureMessages.CommandLaunchPoint_CommandIdFailedExecution.Substitute(cmdId,
                            ex.ToMessages(true)));
                }
            }
        }
    }
}