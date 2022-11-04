using Automate.Common.Application;
using Automate.Common.Domain;
using Automate.Common.Extensions;
using Automate.Common.Infrastructure;

namespace Automate.CLI.Infrastructure
{
    public class LocalMachineUserRepository : IMachineRepository
    {
        private const string StateFilename = "MachineState.json";
        private readonly IFileSystemReaderWriter fileSystem;
        private readonly string persistenceDirectory;
        private readonly IPersistableFactory persistableFactory;

        public LocalMachineUserRepository(string persistenceDirectory, IFileSystemReaderWriter fileSystem,
            IPersistableFactory persistableFactory)
        {
            persistenceDirectory.GuardAgainstNullOrEmpty(nameof(persistenceDirectory));
            fileSystem.GuardAgainstNull(nameof(fileSystem));
            persistableFactory.GuardAgainstNull(nameof(persistableFactory));
            this.persistenceDirectory = persistenceDirectory;
            this.fileSystem = fileSystem;
            this.persistableFactory = persistableFactory;
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public string Location => CreateDirectoryForState();

        public void DestroyAll()
        {
            if (this.fileSystem.DirectoryExists(Location))
            {
                var stateFilename = CreateFilenameForState();
                this.fileSystem.Delete(stateFilename);
            }
        }

        public MachineState GetMachineState()
        {
            var filename = CreateFilenameForState();
            if (!this.fileSystem.FileExists(filename))
            {
                var state = new MachineState();
                WriteState(filename, state);
                return state;
            }

            return this.fileSystem.ReadAllText(filename)
                .FromJson<MachineState>(this.persistableFactory);
        }

        public void SaveMachineState(MachineState state)
        {
            var filename = CreateFilenameForState();
            WriteState(filename, state);
        }

        private void WriteState(string filename, MachineState state)
        {
            EnsurePathExists(filename);

            var contents = state.ToJson(this.persistableFactory);
            this.fileSystem.Write(contents, filename);
        }

        private string CreateDirectoryForState()
        {
            return this.fileSystem.MakeAbsolutePath(this.persistenceDirectory);
        }

        private string CreateFilenameForState()
        {
            return this.fileSystem.MakeAbsolutePath(CreateDirectoryForState(), StateFilename);
        }

        private void EnsurePathExists(string filename)
        {
            this.fileSystem.EnsureFileDirectoryExists(filename);
        }
    }
}