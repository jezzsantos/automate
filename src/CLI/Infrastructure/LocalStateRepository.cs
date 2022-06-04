using System.IO;
using Automate.CLI.Domain;
using Automate.CLI.Extensions;

namespace Automate.CLI.Infrastructure
{
    internal class LocalStateRepository : ILocalStateRepository
    {
        private const string StateFilename = "LocalState.json";
        private readonly string currentDirectory;
        private readonly IPersistableFactory persistableFactory;

        internal LocalStateRepository(string currentDirectory) : this(currentDirectory,
            new AutomatePersistableFactory())
        {
        }

        private LocalStateRepository(string currentDirectory, IPersistableFactory persistableFactory)
        {
            currentDirectory.GuardAgainstNullOrEmpty(nameof(currentDirectory));
            persistableFactory.GuardAgainstNull(nameof(persistableFactory));
            this.currentDirectory = currentDirectory;
            this.persistableFactory = persistableFactory;
        }

        public string Location => Path.Combine(this.currentDirectory, InfrastructureConstants.RootPersistencePath);

        public LocalState GetLocalState()
        {
            var filename = CreateFilenameForState();
            if (!File.Exists(filename))
            {
                var state = new LocalState();
                WriteState(filename, state);
                return state;
            }

            return File.ReadAllText(filename).FromJson<LocalState>(this.persistableFactory);
        }

        public void SaveLocalState(LocalState state)
        {
            var filename = CreateFilenameForState();
            WriteState(filename, state);
        }

        public void DestroyAll()
        {
            if (Directory.Exists(Location))
            {
                var stateFilename = CreateFilenameForState();
                File.Delete(stateFilename);
            }
        }

        private void WriteState(string filename, LocalState state)
        {
            EnsurePathExists(filename);

            using (var file = File.CreateText(filename))
            {
                file.Write(state.ToJson(this.persistableFactory));
            }
        }

        private static string CreateFilenameForState()
        {
            return Path.Combine(InfrastructureConstants.RootPersistencePath, StateFilename);
        }

        private static void EnsurePathExists(string filename)
        {
            var fullPath = Directory.GetParent(filename)?.FullName ?? string.Empty;
            if (fullPath.HasValue())
            {
                if (!Directory.Exists(fullPath))
                {
                    Directory.CreateDirectory(fullPath);
                }
            }
        }
    }
}