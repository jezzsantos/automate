using System.Collections.Generic;
using automate.Domain;
using automate.Extensions;
using automate.Infrastructure;

namespace automate.Application
{
    internal class RuntimeApplication
    {
        private readonly IFilePathResolver fileResolver;
        private readonly IPatternToolkitPackager packager;
        private readonly ISolutionStore solutionStore;
        private readonly IToolkitStore toolkitStore;

        public RuntimeApplication(string currentDirectory) : this(currentDirectory, new PatternStore(currentDirectory),
            new ToolkitStore(currentDirectory), new SolutionStore(currentDirectory), new SystemIoFilePathResolver())
        {
        }

        private RuntimeApplication(string currentDirectory, IPatternStore patternStore, IToolkitStore toolkitStore,
            ISolutionStore solutionStore, IFilePathResolver fileResolver) :
            this(toolkitStore, solutionStore, fileResolver,
                new PatternToolkitPackager(patternStore, toolkitStore, fileResolver))
        {
            currentDirectory.GuardAgainstNullOrEmpty(nameof(currentDirectory));
        }

        internal RuntimeApplication(IToolkitStore toolkitStore, ISolutionStore solutionStore,
            IFilePathResolver fileResolver,
            IPatternToolkitPackager packager)
        {
            toolkitStore.GuardAgainstNull(nameof(toolkitStore));
            solutionStore.GuardAgainstNull(nameof(solutionStore));
            fileResolver.GuardAgainstNull(nameof(fileResolver));
            packager.GuardAgainstNull(nameof(packager));
            this.toolkitStore = toolkitStore;
            this.solutionStore = solutionStore;
            this.fileResolver = fileResolver;
            this.packager = packager;
        }

        public string CurrentToolkitId => this.toolkitStore.GetCurrent()?.Id;

        public string CurrentToolkitName => this.toolkitStore.GetCurrent().PatternName;

        public ToolkitDefinition InstallToolkit(string installerLocation)
        {
            if (!this.fileResolver.ExistsAtPath(installerLocation))
            {
                throw new AutomateException(
                    ExceptionMessages.RuntimeApplication_ToolkitInstallerNotFound.Format(installerLocation));
            }

            var installer = this.fileResolver.GetFileAtPath(installerLocation);
            var toolkit = this.packager.UnPack(installer);

            this.toolkitStore.ChangeCurrent(toolkit.Id);

            return toolkit;
        }

        public SolutionDefinition CreateSolution(string toolkitName)
        {
            var toolkit = this.toolkitStore.FindByName(toolkitName);
            if (toolkit.NotExists())
            {
                throw new AutomateException(ExceptionMessages.RuntimeApplication_ToolkitNotFound.Format(toolkitName));
            }

            var solution = new SolutionDefinition(toolkit.Id, toolkitName);
            this.solutionStore.Save(solution);

            return solution;
        }

        public List<ToolkitDefinition> ListInstalledToolkits()
        {
            return this.toolkitStore.ListAll();
        }

        public List<SolutionDefinition> ListCreatedSolutions()
        {
            return this.solutionStore.ListAll();
        }
    }
}