using Automate.Common.Application;
using Automate.Common.Domain;
using Automate.Common.Extensions;
using Automate.Common.Infrastructure;

namespace Automate.CLI.Infrastructure
{
    internal class LocalMachineFileLocalStateRepository : ILocalStateRepository
    {
        private const string StateFilename = "LocalState.json";
        private readonly string currentDirectory;
        private readonly IFileSystemReaderWriter fileSystem;
        private readonly IPersistableFactory persistableFactory;

        public LocalMachineFileLocalStateRepository(string currentDirectory, IFileSystemReaderWriter fileSystem,
            IPersistableFactory persistableFactory)
        {
            currentDirectory.GuardAgainstNullOrEmpty(nameof(currentDirectory));
            fileSystem.GuardAgainstNull(nameof(fileSystem));
            persistableFactory.GuardAgainstNull(nameof(persistableFactory));
            this.currentDirectory = currentDirectory;
            this.fileSystem = fileSystem;
            this.persistableFactory = persistableFactory;
        }

        public string Location =>
            this.fileSystem.MakePath(this.currentDirectory, InfrastructureConstants.RootPersistencePath);

        public LocalState GetLocalState()
        {
            var filename = CreateFilenameForState();
            if (!this.fileSystem.FileExists(filename))
            {
                var state = new LocalState();
                WriteState(filename, state);
                return state;
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
            if (this.fileSystem.DirectoryExists(Location))
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
            return this.fileSystem.MakePath(InfrastructureConstants.RootPersistencePath, StateFilename);
        }

        private void EnsurePathExists(string filename)
        {
            this.fileSystem.EnsureFileDirectoryExists(filename);
        }
    }
}