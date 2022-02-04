using automate.Extensions;

namespace automate
{
    internal class RuntimeApplication
    {
        private readonly ToolkitStore store;

        public RuntimeApplication(string currentDirectory) : this(new ToolkitStore(currentDirectory))
        {
            currentDirectory.GuardAgainstNullOrEmpty(nameof(currentDirectory));
        }

        internal RuntimeApplication(ToolkitStore store)
        {
            store.GuardAgainstNull(nameof(store));
            this.store = store;
        }

        public string CurrentToolkitId => this.store.GetCurrent()?.Id;

        public string CurrentToolkitName => this.store.GetCurrent().Name;
    }
}