using System.IO;
using Automate.CLI.Infrastructure;
using Automate.Common.Domain;
using FluentAssertions;
using Xunit;

namespace CLI.IntegrationTests.Infrastructure
{
    [Trait("Category", "Integration")] [Collection("CLI")]
    public class LocalMachineFileLocalStateRepositorySpec
    {
        private readonly SystemIoFileSystemReaderWriter readerWriter;
        private readonly LocalMachineFileLocalStateRepository repository;
        private readonly string localPath;

        public LocalMachineFileLocalStateRepositorySpec()
        {
            this.readerWriter = new SystemIoFileSystemReaderWriter();
            this.localPath = new CliRuntimeMetadata().LocalStateDataPath;
            this.repository =
                new LocalMachineFileLocalStateRepository(this.localPath, this.readerWriter,
                    new AutomatePersistableFactory());
            this.repository.DestroyAll();
        }

        [Fact]
        public void WhenGetLocalStateAndNone_ThenReturnsEmpty()
        {
            var result = this.repository.GetLocalState();

            result.CurrentDraft.Should().BeNull();
            result.CurrentPattern.Should().BeNull();
            result.CurrentToolkit.Should().BeNull();

            var stateFile =
                Path.GetFullPath(Path.Combine(this.localPath, LocalMachineFileLocalStateRepository.StateFilename));
            this.readerWriter.FileExists(stateFile).Should().BeFalse();
        }
    }
}