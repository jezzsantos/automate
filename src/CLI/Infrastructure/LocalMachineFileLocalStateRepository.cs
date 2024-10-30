using Automate.Common.Application;
using Automate.Common.Domain;
using Automate.Common.Extensions;
using Automate.Common.Infrastructure;

namespace Automate.CLI.Infrastructure
{
    internal class LocalMachineFileLocalStateRepository : ILocalStateRepository
    {
        internal const string StateFilename = "LocalState.json";
        private readonly IFileSystemReaderWriter fileSystem;
        private readonly string localStatePath;
        private readonly IPersistableFactory persistableFactory;

        public LocalMachineFileLocalStateRepository(string localStatePath, IFileSystemReaderWriter fileSystem,
            IPersistableFactory persistableFactory)
        {
            localStatePath.GuardAgainstNullOrEmpty(nameof(localStatePath));
            fileSystem.GuardAgainstNull(nameof(fileSystem));
            persistableFactory.GuardAgainstNull(nameof(persistableFactory));
            this.localStatePath = localStatePath;
            this.fileSystem = fileSystem;
            this.persistableFactory = persistableFactory;
        }

        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once ConvertToAutoProperty
        internal string LocalStateLocation => this.localStatePath;

        public LocalState GetLocalState()
        {
            var filename = CreateFilenameForState();
            if (!this.fileSystem.FileExists(filename))
            {
                return new LocalState();
            }

            return this.fileSystem.ReadAllText(filename)
                .FromJson<LocalState>(this.persistableFactory);
        }

        public void SaveLocalState(LocalState state)
        {
            var filename = CreateFilenameForState();
            WriteState(filename, state);
        }

        public void DestroyAll()
        {
            if (this.fileSystem.DirectoryExists(LocalStateLocation))
            {
                var stateFilename = CreateFilenameForState();
                this.fileSystem.Delete(stateFilename);
            }
        }

        private void WriteState(string filename, LocalState state)
        {
            EnsurePathExists(filename);

            var contents = state.ToJson(this.persistableFactory);
            this.fileSystem.Write(contents, filename);
        }

        private string CreateFilenameForState()
        {
            return this.fileSystem.MakeAbsolutePath(LocalStateLocation, StateFilename);
        }

        private void EnsurePathExists(string filename)
        {
            this.fileSystem.EnsureFileDirectoryExists(filename);
        }
    }
}