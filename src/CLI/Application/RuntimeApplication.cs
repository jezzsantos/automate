using automate.Domain;
using automate.Extensions;
using automate.Infrastructure;

namespace automate.Application
{
    internal class RuntimeApplication
    {
        private readonly IFilePathResolver fileResolver;
        private readonly IPatternToolkitPackager packager;
        private readonly IToolkitStore store;

        public RuntimeApplication(string currentDirectory) : this(currentDirectory, new PatternStore(currentDirectory),
            new ToolkitStore(currentDirectory),
            new SystemIoFilePathResolver())
        {
        }

        private RuntimeApplication(string currentDirectory, IPatternStore patternStore, IToolkitStore toolkitStore,
            IFilePathResolver fileResolver) :
            this(toolkitStore, fileResolver, new PatternToolkitPackager(patternStore, toolkitStore, fileResolver))
        {
            currentDirectory.GuardAgainstNullOrEmpty(nameof(currentDirectory));
        }

        internal RuntimeApplication(IToolkitStore store, IFilePathResolver fileResolver,
            IPatternToolkitPackager packager)
        {
            store.GuardAgainstNull(nameof(store));
            fileResolver.GuardAgainstNull(nameof(fileResolver));
            packager.GuardAgainstNull(nameof(packager));
            this.store = store;
            this.fileResolver = fileResolver;
            this.packager = packager;
        }

        public string CurrentToolkitId => this.store.GetCurrent()?.Id;

        public string CurrentToolkitName => this.store.GetCurrent().Name;

        public PatternToolkitDefinition InstallToolkit(string installerLocation)
        {
            if (!this.fileResolver.ExistsAtPath(installerLocation))
            {
                throw new PatternException(
                    ExceptionMessages.RuntimeApplication_ToolkitInstallerNotFound.Format(installerLocation));
            }

            var installer = this.fileResolver.GetFileAtPath(installerLocation);
            var toolkit = this.packager.UnPack(installer);

            this.store.ChangeCurrent(toolkit.Id);

            return toolkit;
        }
    }
}