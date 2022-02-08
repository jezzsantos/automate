using System;
using automate.Domain;
using automate.Extensions;
using automate.Infrastructure;

namespace automate.Application
{
    internal class RuntimeApplication
    {
        private readonly IToolkitStore store;

        public RuntimeApplication(string currentDirectory) : this(new ToolkitStore(currentDirectory))
        {
            currentDirectory.GuardAgainstNullOrEmpty(nameof(currentDirectory));
        }

        internal RuntimeApplication(IToolkitStore store)
        {
            store.GuardAgainstNull(nameof(store));
            this.store = store;
        }

        public string CurrentToolkitId => this.store.GetCurrent()?.Id;

        public string CurrentToolkitName => this.store.GetCurrent().Name;

        public PatternToolkitDefinition InstallToolkit(string installerLocation)
        {
            //TODO: file not exists -> error
            //TODO: file not contains a toolkit -> error
            //TODO: extract the pattern and copy to the repo, as if it were being authored.

            throw new NotImplementedException();
        }
    }
}