using System.IO;
using Automate.Authoring.Domain;
using Automate.CLI.Infrastructure;
using Automate.Common;
using Automate.Common.Domain;
using Automate.Common.Extensions;
using Automate.Runtime.Domain;
using FluentAssertions;
using Xunit;
using ExceptionMessages = Automate.CLI.ExceptionMessages;

namespace CLI.IntegrationTests.Infrastructure
{
    [Trait("Category", "Integration")] [Collection("CLI")]
    public class LocalMachineFileRepositorySpec
    {
        private readonly SystemIoFileSystemReaderWriter readerWriter;
        private readonly LocalMachineFileRepository repository;
        private readonly string localPath;

        public LocalMachineFileRepositorySpec()
        {
            this.readerWriter = new SystemIoFileSystemReaderWriter();
            this.localPath = new CliRuntimeMetadata().LocalStateDataPath;
            this.repository =
                new LocalMachineFileRepository(localPath, this.readerWriter, new AutomatePersistableFactory());
            this.repository.DestroyAll();
        }

        [Fact]
        public void WhenListPatternsAndHasNone_ThenReturnsNone()
        {
            var result = this.repository.ListPatterns();

            result.Count.Should().Be(0);
            var stateFile =
                Path.GetFullPath(Path.Combine(this.localPath, LocalMachineFileLocalStateRepository.StateFilename));
            this.readerWriter.FileExists(stateFile).Should().BeFalse();
        }

        [Fact]
        public void WhenListPatternsAndHasSome_ThenReturnsSome()
        {
            this.repository.UpsertPattern(new PatternDefinition("apattername1"));
            this.repository.UpsertPattern(new PatternDefinition("apattername2"));
            this.repository.UpsertPattern(new PatternDefinition("apattername3"));

            var result = this.repository.ListPatterns();

            result.Count.Should().Be(3);
            result[0].Name.Should().Be("apattername1");
            result[1].Name.Should().Be("apattername2");
            result[2].Name.Should().Be("apattername3");
        }

        [Fact]
        public void WhenListPatternsAndHasSomeAndHasDeletedJsonFile_ThenReturnsRemaining()
        {
            var pattern2 = new PatternDefinition("apattername2");
            this.repository.UpsertPattern(new PatternDefinition("apattername1"));
            this.repository.UpsertPattern(pattern2);
            this.repository.UpsertPattern(new PatternDefinition("apattername3"));

            var toolkitFilename = this.repository.CreateFilenameForPatternById(pattern2.Id);
            this.readerWriter.Delete(toolkitFilename);

            var result = this.repository.ListPatterns();

            result.Count.Should().Be(2);
            result[0].Name.Should().Be("apattername1");
            result[1].Name.Should().Be("apattername3");
        }

        [Fact]
        public void WhenGetPattern_ThenReturnsPattern()
        {
            var pattern = new PatternDefinition("apattername");

            this.repository.UpsertPattern(pattern);

            var result = this.repository.GetPattern(pattern.Id);

            result.Id.Should().Be(pattern.Id);
            result.Name.Should().Be(pattern.Name);
        }

        [Fact]
        public void WhenGetPatternAndHasDeletedJsonFile_ThenThrows()
        {
            var pattern2 = new PatternDefinition("apattername2");
            this.repository.UpsertPattern(new PatternDefinition("apattername1"));
            this.repository.UpsertPattern(pattern2);
            this.repository.UpsertPattern(new PatternDefinition("apattername3"));

            var toolkitFilename = this.repository.CreateFilenameForPatternById(pattern2.Id);
            this.readerWriter.Delete(toolkitFilename);

            this.repository
                .Invoking(x => x.GetPattern(pattern2.Id))
                .Should().Throw<AutomateException>()
                .WithMessage(ExceptionMessages.LocalMachineFileRepository_PatternNotFound.Substitute(pattern2.Id));
        }

        [Fact]
        public void WhenListToolkitsAndHasNone_ThenReturnsNone()
        {
            var result = this.repository.ListToolkits();

            result.Count.Should().Be(0);
            var stateFile =
                Path.GetFullPath(Path.Combine(this.localPath, LocalMachineFileLocalStateRepository.StateFilename));
            this.readerWriter.FileExists(stateFile).Should().BeFalse();
        }

        [Fact]
        public void WhenListToolkitsAndHasSome_ThenReturnsSome()
        {
            this.repository.ImportToolkit(new ToolkitDefinition(new PatternDefinition("apattername1")));
            this.repository.ImportToolkit(new ToolkitDefinition(new PatternDefinition("apattername2")));
            this.repository.ImportToolkit(new ToolkitDefinition(new PatternDefinition("apattername3")));

            var result = this.repository.ListToolkits();

            result.Count.Should().Be(3);
            result[0].PatternName.Should().Be("apattername1");
            result[1].PatternName.Should().Be("apattername2");
            result[2].PatternName.Should().Be("apattername3");
        }

        [Fact]
        public void WhenListToolkitsAndHasSomeAndHasDeletedJsonFile_ThenReturnsRemaining()
        {
            var toolkit2 = new ToolkitDefinition(new PatternDefinition("apattername2"));
            this.repository.ImportToolkit(new ToolkitDefinition(new PatternDefinition("apattername1")));
            this.repository.ImportToolkit(toolkit2);
            this.repository.ImportToolkit(new ToolkitDefinition(new PatternDefinition("apattername3")));

            var toolkitFilename = this.repository.CreateFilenameForImportedToolkitById(toolkit2.Id);
            this.readerWriter.Delete(toolkitFilename);

            var result = this.repository.ListToolkits();

            result.Count.Should().Be(2);
            result[0].PatternName.Should().Be("apattername1");
            result[1].PatternName.Should().Be("apattername3");
        }

        [Fact]
        public void WhenGetToolkit_ThenReturnsToolkit()
        {
            var toolkit = new ToolkitDefinition(new PatternDefinition("apattername"));

            this.repository.ImportToolkit(toolkit);

            var result = this.repository.GetToolkit(toolkit.Id);

            result.Id.Should().Be(toolkit.Id);
            result.PatternName.Should().Be(toolkit.PatternName);
        }

        [Fact]
        public void WhenGetToolkitAndHasDeletedJsonFile_ThenThrows()
        {
            var toolkit2 = new ToolkitDefinition(new PatternDefinition("apattername2"));
            this.repository.ImportToolkit(new ToolkitDefinition(new PatternDefinition("apattername1")));
            this.repository.ImportToolkit(toolkit2);
            this.repository.ImportToolkit(new ToolkitDefinition(new PatternDefinition("apattername3")));

            var toolkitFilename = this.repository.CreateFilenameForImportedToolkitById(toolkit2.Id);
            this.readerWriter.Delete(toolkitFilename);

            this.repository
                .Invoking(x => x.GetToolkit(toolkit2.Id))
                .Should().Throw<AutomateException>()
                .WithMessage(ExceptionMessages.LocalMachineFileRepository_ToolkitNotFound.Substitute(toolkit2.Id));
        }

        [Fact]
        public void WhenListDraftsAndHasNone_ThenReturnsNone()
        {
            var result = this.repository.ListDrafts();

            result.Count.Should().Be(0);
            var stateFile =
                Path.GetFullPath(Path.Combine(this.localPath, LocalMachineFileLocalStateRepository.StateFilename));
            this.readerWriter.FileExists(stateFile).Should().BeFalse();
        }

        [Fact]
        public void WhenListDraftsAndHasSome_ThenReturnsSome()
        {
            var toolkit = new ToolkitDefinition(new PatternDefinition("apattername"));

            this.repository.UpsertDraft(new DraftDefinition(toolkit, "adraftname1"));
            this.repository.UpsertDraft(new DraftDefinition(toolkit, "adraftname2"));
            this.repository.UpsertDraft(new DraftDefinition(toolkit, "adraftname3"));

            var result = this.repository.ListDrafts();

            result.Count.Should().Be(3);
            result[0].Name.Should().Be("adraftname1");
            result[1].Name.Should().Be("adraftname2");
            result[2].Name.Should().Be("adraftname3");
        }

        [Fact]
        public void WhenListDraftsAndHasSomeAndHasDeletedJsonFile_ThenReturnsRemaining()
        {
            var toolkit = new ToolkitDefinition(new PatternDefinition("apattername"));

            var draft2 = new DraftDefinition(toolkit, "adraftname2");
            this.repository.UpsertDraft(new DraftDefinition(toolkit, "adraftname1"));
            this.repository.UpsertDraft(draft2);
            this.repository.UpsertDraft(new DraftDefinition(toolkit, "adraftname3"));

            var draftFilename = this.repository.CreateFilenameForDraftById(draft2.Id);
            this.readerWriter.Delete(draftFilename);

            var result = this.repository.ListDrafts();

            result.Count.Should().Be(2);
            result[0].Name.Should().Be("adraftname1");
            result[1].Name.Should().Be("adraftname3");
        }

        [Fact]
        public void WhenGetDraft_ThenReturnsDraft()
        {
            var toolkit = new ToolkitDefinition(new PatternDefinition("apattername"));
            var draft = new DraftDefinition(toolkit, "adraftname1");

            this.repository.UpsertDraft(draft);

            var result = this.repository.GetDraft(draft.Id);

            result.Id.Should().Be(draft.Id);
            result.Name.Should().Be(draft.Name);
        }

        [Fact]
        public void WhenGetDraftAndHasDeletedJsonFile_ThenThrows()
        {
            var toolkit = new ToolkitDefinition(new PatternDefinition("apattername"));

            var draft2 = new DraftDefinition(toolkit, "adraftname2");
            this.repository.UpsertDraft(new DraftDefinition(toolkit, "adraftname1"));
            this.repository.UpsertDraft(draft2);
            this.repository.UpsertDraft(new DraftDefinition(toolkit, "adraftname3"));

            var draftFilename = this.repository.CreateFilenameForDraftById(draft2.Id);
            this.readerWriter.Delete(draftFilename);

            this.repository
                .Invoking(x => x.GetDraft(draft2.Id))
                .Should().Throw<AutomateException>()
                .WithMessage(ExceptionMessages.LocalMachineFileRepository_DraftNotFound.Substitute(draft2.Id));
        }
        
    }
}