using System.Collections.Generic;
using Automate.CLI.Extensions;
using Automate.CLI.Infrastructure;

namespace Automate.CLI.Domain
{
    internal class CliCommand : IAutomation
    {
        private readonly IApplicationExecutor applicationExecutor;
        private readonly Automation automation;
        private readonly ISolutionPathResolver solutionPathResolver;

        public CliCommand(string name, string applicationName, string arguments) : this(name, applicationName,
            arguments, new SolutionPathResolver(), new ApplicationExecutor())
        {
        }

        internal CliCommand(string name, string applicationName, string arguments,
            ISolutionPathResolver solutionPathResolver, IApplicationExecutor applicationExecutor) : this(
            new Automation(name, AutomationType.CliCommand, new Dictionary<string, object>
            {
                { nameof(ApplicationName), applicationName },
                { nameof(Arguments), arguments }
            }), solutionPathResolver, applicationExecutor)
        {
            applicationName.GuardAgainstNull(nameof(applicationName));
        }

        private CliCommand(Automation automation) : this(
            automation, new SolutionPathResolver(), new ApplicationExecutor())
        {
        }

        private CliCommand(Automation automation, ISolutionPathResolver solutionPathResolver,
            IApplicationExecutor applicationExecutor)
        {
            automation.GuardAgainstNull(nameof(automation));
            solutionPathResolver.GuardAgainstNull(nameof(solutionPathResolver));
            applicationExecutor.GuardAgainstNull(nameof(applicationExecutor));

            this.automation = automation;
            this.solutionPathResolver = solutionPathResolver;
            this.applicationExecutor = applicationExecutor;
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
            this.automation.Rename(name);
        }

        public string Id => this.automation.Id;

        public string Name => this.automation.Name;

        public CommandExecutionResult Execute(SolutionDefinition solution, SolutionItem target)
        {
            var outcome = new CommandExecutionResult(Name);

            var applicationName = ApplicationName.HasValue()
                ? this.solutionPathResolver.ResolveExpression(
                    DomainMessages.CliCommand_ApplicationName_Description.Format(Id), ApplicationName, target)
                : string.Empty;
            var arguments = Arguments.HasValue()
                ? this.solutionPathResolver.ResolveExpression(
                    DomainMessages.CliCommand_Arguments_Description.Format(Id), Arguments, target)
                : string.Empty;

            var result = this.applicationExecutor.RunApplicationProcess(true, applicationName, arguments);
            if (result.IsSuccess)
            {
                outcome.Add(result.Output);
            }
            else
            {
                outcome.Fail(result.Error);
            }

            return outcome;
        }
    }
}