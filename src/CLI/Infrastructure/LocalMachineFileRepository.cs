using System.Collections.Generic;
using System.IO;
using System.Linq;
using Automate.Application;
using Automate.Domain;
using Automate.Extensions;
using Automate.Infrastructure;

namespace Automate.CLI.Infrastructure
{
    internal class LocalMachineFileRepository : IPatternRepository, IToolkitRepository, IDraftRepository,
        ILocalStateRepository
    {
        private const string PatternDefinitionFilename = "Pattern.json";
        private const string ToolkitDefinitionFilename = "Toolkit.json";
        private const string DraftDefinitionFilename = "Draft.json";
        private const string CodeTemplateDirectoryName = "CodeTemplates";
        private const string ToolkitInstallerFileExtension = ".toolkit";
        private static readonly string PatternDirectoryPath =
            Path.Combine(InfrastructureConstants.RootPersistencePath, "patterns");
        private static readonly string ToolkitDirectoryPath =
            Path.Combine(InfrastructureConstants.RootPersistencePath, "toolkits");
        private static readonly string DraftDirectoryPath =
            Path.Combine(InfrastructureConstants.RootPersistencePath, "drafts");
        private readonly string currentDirectory;
        private readonly IFileSystemReaderWriter fileSystem;
        private readonly ILocalStateRepository localStateRepository;
        private readonly IPersistableFactory persistableFactory;

        public LocalMachineFileRepository(string currentDirectory, IFileSystemReaderWriter fileSystem,
            IPersistableFactory persistableFactory) : this(
            currentDirectory,
            fileSystem,
            new LocalMachineFileLocalStateRepository(currentDirectory, fileSystem, persistableFactory),
            persistableFactory)
        {
        }

        private LocalMachineFileRepository(string currentDirectory, IFileSystemReaderWriter fileSystem,
            ILocalStateRepository localStateRepository,
            IPersistableFactory persistableFactory)
        {
            currentDirectory.GuardAgainstNullOrEmpty(nameof(currentDirectory));
            fileSystem.GuardAgainstNull(nameof(fileSystem));
            localStateRepository.GuardAgainstNull(nameof(localStateRepository));
            persistableFactory.GuardAgainstNull(nameof(persistableFactory));
            this.currentDirectory = currentDirectory;
            this.fileSystem = fileSystem;
            this.localStateRepository = localStateRepository;
            this.persistableFactory = persistableFactory;
        }

        public string DraftLocation => this.fileSystem.MakePath(this.currentDirectory, DraftDirectoryPath);

        public void NewDraft(DraftDefinition draft)
        {
            UpsertDraft(draft);
        }

        public void UpsertDraft(DraftDefinition draft)
        {
            var filename = CreateFilenameForDraftById(draft.Id);
            EnsurePathExists(filename);

            var contents = draft.ToJson(this.persistableFactory);
            this.fileSystem.Write(contents, filename);
        }

        public DraftDefinition GetDraft(string id)
        {
            var filename = CreateFilenameForDraftById(id);
            if (!this.fileSystem.FileExists(filename))
            {
                throw new AutomateException(ExceptionMessages.JsonFileRepository_DraftNotFound.Substitute(id));
            }

            return this.fileSystem.ReadAllText(filename)
                .FromJson<DraftDefinition>(this.persistableFactory);
        }

        public List<DraftDefinition> ListDrafts()
        {
            if (!this.fileSystem.DirectoryExists(DraftLocation))
            {
                return new List<DraftDefinition>();
            }

            return this.fileSystem.GetSubDirectories(DraftLocation)
                .Select(directory => directory.Name)
                .Select(GetDraft)
                .ToList();
        }

        public DraftDefinition FindDraftById(string id)
        {
            return ListDrafts()
                .FirstOrDefault(draft => draft.Id == id);
        }

        public void SaveLocalState(LocalState state)
        {
            this.localStateRepository.SaveLocalState(state);
        }

        public LocalState GetLocalState()
        {
            return this.localStateRepository.GetLocalState();
        }

        public string PatternLocation => this.fileSystem.MakePath(this.currentDirectory, PatternDirectoryPath);

        public void NewPattern(PatternDefinition pattern)
        {
            UpsertPattern(pattern);
        }

        public PatternDefinition GetPattern(string id)
        {
            var filename = CreateFilenameForPatternById(id);
            if (!this.fileSystem.FileExists(filename))
            {
                throw new AutomateException(ExceptionMessages.JsonFileRepository_PatternNotFound.Substitute(id));
            }

            return this.fileSystem.ReadAllText(filename)
                .FromJson<PatternDefinition>(this.persistableFactory);
        }

        public List<PatternDefinition> ListPatterns()
        {
            if (!this.fileSystem.DirectoryExists(PatternLocation))
            {
                return new List<PatternDefinition>();
            }

            return this.fileSystem.GetSubDirectories(PatternLocation)
                .Select(directory => directory.Name)
                .Select(GetPattern)
                .ToList();
        }

        public void UpsertPattern(PatternDefinition pattern)
        {
            var filename = CreateFilenameForPatternById(pattern.Id);
            EnsurePathExists(filename);

            var contents = pattern.ToJson(this.persistableFactory);
            this.fileSystem.Write(contents, filename);
        }

        public PatternDefinition FindPatternByName(string name)
        {
            return ListPatterns()
                .FirstOrDefault(pattern => pattern.Name == name);
        }

        public PatternDefinition FindPatternById(string id)
        {
            return ListPatterns()
                .FirstOrDefault(pattern => pattern.Id == id);
        }

        public string UploadPatternCodeTemplate(PatternDefinition pattern, string codeTemplateId, IFile file)
        {
            var extension = file.Extension;
            var uploadedFilePath = CreateFilenameForCodeTemplate(pattern.Id, codeTemplateId, extension);
            file.CopyTo(uploadedFilePath);

            return uploadedFilePath;
        }

        public string GetCodeTemplateLocation(PatternDefinition pattern, string codeTemplateId, string extension)
        {
            return CreateFilenameForCodeTemplate(pattern.Id, codeTemplateId, extension);
        }

        public void DeleteTemplate(PatternDefinition pattern, string codeTemplateId, string extension)
        {
            var path = CreateFilenameForCodeTemplate(pattern.Id, codeTemplateId, extension);
            this.fileSystem.Delete(path);
        }

        public CodeTemplateContent DownloadPatternCodeTemplate(PatternDefinition pattern, string codeTemplateId,
            string extension)
        {
            var path = CreateFilenameForCodeTemplate(pattern.Id, codeTemplateId, extension);

            var content = this.fileSystem.GetContent(path);
            return new CodeTemplateContent
            {
                Content = content.RawBytes,
                LastModifiedUtc = content.LastModifiedUtc
            };
        }

        public void DestroyAll()
        {
            if (this.fileSystem.DirectoryExists(PatternLocation))
            {
                this.fileSystem.GetSubDirectories(PatternLocation)
                    .ToList()
                    .ForEach(directory => this.fileSystem.DirectoryDelete(directory));
            }
            if (this.fileSystem.DirectoryExists(ToolkitLocation))
            {
                this.fileSystem.GetSubDirectories(ToolkitLocation)
                    .ToList()
                    .ForEach(directory => this.fileSystem.DirectoryDelete(directory));
            }
            if (this.fileSystem.DirectoryExists(DraftLocation))
            {
                this.fileSystem.GetSubDirectories(DraftLocation)
                    .ToList()
                    .ForEach(directory => this.fileSystem.DirectoryDelete(directory));
            }

            var exportedDirectory = GetExportedToolkitDirectory();
            this.fileSystem.DeleteAllDirectoryFiles(exportedDirectory, $"*{ToolkitInstallerFileExtension}");

            this.localStateRepository.DestroyAll();
        }

        public List<ToolkitDefinition> ListToolkits()
        {
            if (!this.fileSystem.DirectoryExists(ToolkitLocation))
            {
                return new List<ToolkitDefinition>();
            }

            return this.fileSystem.GetSubDirectories(ToolkitLocation)
                .Select(directory => directory.Name)
                .Select(GetToolkit)
                .ToList();
        }

        public string ExportToolkit(ToolkitDefinition toolkit)
        {
            var filename = CreateFilenameForExportedToolkit(toolkit.PatternName, toolkit.Version);
            EnsurePathExists(filename);

            var contents = toolkit.ToJson(this.persistableFactory);
            this.fileSystem.Write(contents, filename);

            return filename;
        }

        public void ImportToolkit(ToolkitDefinition toolkit)
        {
            var filename = CreateFilenameForImportedToolkitById(toolkit.Id);
            EnsurePathExists(filename);

            var contents = toolkit.ToJson(this.persistableFactory);
            this.fileSystem.Write(contents, filename);
        }

        public string ToolkitLocation => this.fileSystem.MakePath(this.currentDirectory, ToolkitDirectoryPath);

        public ToolkitDefinition FindToolkitById(string id)
        {
            return ListToolkits()
                .FirstOrDefault(toolkit => toolkit.Id == id);
        }

        public ToolkitDefinition FindToolkitByName(string name)
        {
            return ListToolkits()
                .FirstOrDefault(toolkit => toolkit.PatternName == name);
        }

        public ToolkitDefinition GetToolkit(string id)
        {
            var filename = CreateFilenameForImportedToolkitById(id);
            if (!this.fileSystem.FileExists(filename))
            {
                throw new AutomateException(ExceptionMessages.JsonFileRepository_ToolkitNotFound.Substitute(id));
            }

            return this.fileSystem.ReadAllText(filename)
                .FromJson<ToolkitDefinition>(this.persistableFactory);
        }

        private void EnsurePathExists(string filename)
        {
            this.fileSystem.EnsureFileDirectoryExists(filename);
        }

        private string CreateFilenameForPatternById(string id)
        {
            var location = CreatePathForPattern(id);
            return this.fileSystem.MakePath(location, PatternDefinitionFilename);
        }

        private string CreatePathForPattern(string id)
        {
            return this.fileSystem.MakePath(PatternLocation, id);
        }

        private string CreateFilenameForCodeTemplate(string id, string codeTemplateId, string fileExtension)
        {
            var patternLocation = CreatePathForPattern(id);
            var templateLocation = this.fileSystem.MakePath(patternLocation, CodeTemplateDirectoryName);
            var templateFilename = $"{codeTemplateId}{(fileExtension.StartsWith(".") ? "" : ".")}{fileExtension}";

            return this.fileSystem.MakePath(this.currentDirectory,
                this.fileSystem.MakePath(templateLocation, templateFilename));
        }

        private string CreateFilenameForExportedToolkit(string name, string version)
        {
            var filename = $"{name}_{version}{ToolkitInstallerFileExtension}";
            var directory = GetExportedToolkitDirectory();

            return this.fileSystem.MakePath(directory, filename);
        }

        private static string GetExportedToolkitDirectory()
        {
            return InfrastructureConstants.GetExportDirectory();
        }

        private string CreateFilenameForImportedToolkitById(string id)
        {
            var location = CreatePathForToolkit(id);
            return this.fileSystem.MakePath(location, ToolkitDefinitionFilename);
        }

        private string CreatePathForToolkit(string id)
        {
            return this.fileSystem.MakePath(ToolkitLocation, id);
        }

        private string CreateFilenameForDraftById(string id)
        {
            var location = CreatePathForDraft(id);
            return this.fileSystem.MakePath(location, DraftDefinitionFilename);
        }

        private string CreatePathForDraft(string id)
        {
            return this.fileSystem.MakePath(DraftLocation, id);
        }
    }
}