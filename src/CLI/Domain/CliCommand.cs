using System;
using System.Collections.Generic;
using System.Diagnostics;
using Automate.CLI.Extensions;
using Automate.CLI.Infrastructure;

namespace Automate.CLI.Domain
{
    internal class CliCommand : IAutomation
    {
        internal static readonly TimeSpan HangTime = TimeSpan.FromSeconds(5);
        private readonly Automation automation;
        private readonly ISolutionPathResolver solutionPathResolver;

        public CliCommand(string name, string applicationName, string arguments) : this(name, applicationName, arguments, new SolutionPathResolver())
        {
        }

        internal CliCommand(string name, string applicationName, string arguments, ISolutionPathResolver solutionPathResolver) : this(
            new Automation(name, AutomationType.CliCommand, new Dictionary<string, object>
            {
                { nameof(ApplicationName), applicationName },
                { nameof(Arguments), arguments }
            }), solutionPathResolver)
        {
            applicationName.GuardAgainstNull(nameof(applicationName));
        }

        private CliCommand(Automation automation) : this(
            automation, new SolutionPathResolver())
        {
        }

        private CliCommand(Automation automation, ISolutionPathResolver solutionPathResolver)
        {
            automation.GuardAgainstNull(nameof(automation));
            solutionPathResolver.GuardAgainstNull(nameof(solutionPathResolver));

            this.automation = automation;
            this.solutionPathResolver = solutionPathResolver;
        }

        public string ApplicationName => this.automation.Metadata[nameof(ApplicationName)].ToString();

        public string Arguments => this.automation.Metadata[nameof(Arguments)]?.ToString();

        public static CliCommand FromAutomation(Automation automation)
        {
            return new CliCommand(automation);
        }

        public Automation AsAutomation()
        {
            return this.automation;
        }

        public void ChangeArguments(string arguments)
        {
            this.automation.UpdateMetadata(nameof(Arguments), arguments);
        }

        public void ChangeApplicationName(string applicationName)
        {
            this.automation.UpdateMetadata(nameof(ApplicationName), applicationName);
        }

        public void ChangeName(string name)
        {
            this.automation.ChangeName(name);
        }

        public string Id => this.automation.Id;

        public string Name => this.automation.Name;

        public CommandExecutionResult Execute(SolutionDefinition solution, SolutionItem target)
        {
            var outcome = new CommandExecutionResult(Name);

            var applicationName = ApplicationName.HasValue()
                ? this.solutionPathResolver.ResolveExpression(DomainMessages.CliCommand_ApplicationName_Description.Format(Id), ApplicationName, target)
                : string.Empty;
            var arguments = Arguments.HasValue()
                ? this.solutionPathResolver.ResolveExpression(DomainMessages.CliCommand_Arguments_Description.Format(Id), Arguments, target)
                : string.Empty;

            try
            {
                var process = Process.Start(new ProcessStartInfo
                {
                    FileName = applicationName,
                    Arguments = arguments,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    WorkingDirectory = Environment.CurrentDirectory
                });
                if (process.NotExists())
                {
                    throw new InvalidOperationException();
                }
                if (process.HasExited)
                {
                    var error = process.StandardError.ReadToEnd();
                    if (error.HasValue())
                    {
                        throw new Exception(error);
                    }

                    throw new Exception(DomainMessages.CliCommand_Log_ProcessExited.Format(process.ExitCode));
                }
                var success = process.WaitForExit((int)HangTime.TotalMilliseconds);
                if (success)
                {
                    var error = process.StandardError.ReadToEnd();
                    if (error.HasValue())
                    {
                        throw new Exception(error);
                    }

                    var output = process.StandardOutput.ReadToEnd();
                    outcome.Add(DomainMessages.CliCommand_Log_ExecutionSucceeded.Format(applicationName, arguments, output));
                }
                else
                {
                    process.Kill();
                    outcome.Fail(DomainMessages.CliCommand_Log_ExecutionFailed.Format(applicationName, arguments, DomainMessages.CliCommand_Log_Hung.Format(HangTime.TotalSeconds)));
                }
            }
            catch (Exception ex)
            {
                outcome.Fail(DomainMessages.CliCommand_Log_ExecutionFailed.Format(applicationName, arguments, ex.Message));
            }

            return outcome;
        }
    }
}