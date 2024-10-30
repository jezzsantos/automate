using System.Collections.Generic;
using System.Linq;
using Automate.Authoring.Application;
using Automate.Authoring.Domain;
using Automate.Authoring.Infrastructure;
using Automate.Common;
using Automate.Common.Application;
using Automate.Common.Domain;
using Automate.Common.Extensions;
using Automate.Common.Infrastructure;
using Automate.Runtime.Domain;
using Automate.Runtime.Infrastructure;

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
        private const string PatternDirectoryPath = "patterns";
        private const string ToolkitDirectoryPath = "toolkits";
        private const string DraftDirectoryPath = "drafts";
        private readonly IFileSystemReaderWriter fileSystem;
        private readonly string localStatePath;
        private readonly ILocalStateRepository localStateRepository;
        private readonly IPersistableFactory persistableFactory;

        public LocalMachineFileRepository(string localStatePath, IFileSystemReaderWriter fileSystem,
            IPersistableFactory persistableFactory) : this(localStatePath, fileSystem,
            new LocalMachineFileLocalStateRepository(localStatePath, fileSystem, persistableFactory),
            persistableFactory)
        {
        }

        private LocalMachineFileRepository(string localStatePath, IFileSystemReaderWriter fileSystem,
            ILocalStateRepository localStateRepository, IPersistableFactory persistableFactory)
        {
            localStatePath.GuardAgainstNullOrEmpty(nameof(localStatePath));
            fileSystem.GuardAgainstNull(nameof(fileSystem));
            localStateRepository.GuardAgainstNull(nameof(localStateRepository));
            persistableFactory.GuardAgainstNull(nameof(persistableFactory));
            this.localStatePath = localStatePath;
            this.fileSystem = fileSystem;
            this.localStateRepository = localStateRepository;
            this.persistableFactory = persistableFactory;
        }

        public string DraftsLocation => this.fileSystem.MakeAbsolutePath(this.localStatePath, DraftDirectoryPath);

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
                throw new AutomateException(ExceptionMessages.LocalMachineFileRepository_DraftNotFound.Substitute(id));
            }

            return this.fileSystem.ReadAllText(filename)
                .FromJson<DraftDefinition>(this.persistableFactory);
        }

        public List<DraftDefinition> ListDrafts()
        {
            if (!this.fileSystem.DirectoryExists(DraftsLocation))
            {
                return new List<DraftDefinition>();
            }

            return this.fileSystem.GetSubDirectories(DraftsLocation)
                .Select(directory => directory.Name)
                .Where(DraftExists)
                .Select(GetDraft)
                .OrderBy(p => p.Name)
                .ToList();
        }

        public DraftDefinition FindDraftById(string id)
        {
            return ListDrafts()
                .FirstOrDefault(draft => draft.Id == id);
        }

        public void DeleteDraft(string id)
        {
            var filename = CreateFilenameForDraftById(id);
            if (!this.fileSystem.FileExists(filename))
            {
                throw new AutomateException(ExceptionMessages.LocalMachineFileRepository_DraftNotFound.Substitute(id));
            }

            this.fileSystem.Delete(filename);

            var directoryName = CreatePathForDraft(id);
            this.fileSystem.DirectoryDelete(this.fileSystem.GetDirectory(directoryName));
        }

        public void SaveLocalState(LocalState state)
        {
            this.localStateRepository.SaveLocalState(state);
        }

        public LocalState GetLocalState()
        {
            return this.localStateRepository.GetLocalState();
        }

        public string PatternsLocation => this.fileSystem.MakeAbsolutePath(this.localStatePath, PatternDirectoryPath);

        public void NewPattern(PatternDefinition pattern)
        {
            UpsertPattern(pattern);
        }

        public PatternDefinition GetPattern(string id)
        {
            var filename = CreateFilenameForPatternById(id);
            if (!this.fileSystem.FileExists(filename))
            {
                throw new AutomateException(ExceptionMessages.LocalMachineFileRepository_PatternNotFound
                    .Substitute(id));
            }

            return this.fileSystem.ReadAllText(filename)
                .FromJson<PatternDefinition>(this.persistableFactory);
        }

        public List<PatternDefinition> ListPatterns()
        {
            if (!this.fileSystem.DirectoryExists(PatternsLocation))
            {
                return new List<PatternDefinition>();
            }

            return this.fileSystem.GetSubDirectories(PatternsLocation)
                .Select(directory => directory.Name)
                .Where(PatternExists)
                .Select(GetPattern)
                .OrderBy(p => p.Name)
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
            if (this.fileSystem.DirectoryExists(PatternsLocation))
            {
                this.fileSystem.GetSubDirectories(PatternsLocation)
                    .ToList()
                    .ForEach(directory => this.fileSystem.DirectoryDelete(directory));
                this.fileSystem.DirectoryDelete(this.fileSystem.GetDirectory(PatternsLocation));
            }
            if (this.fileSystem.DirectoryExists(ToolkitsLocation))
            {
                this.fileSystem.GetSubDirectories(ToolkitsLocation)
                    .ToList()
                    .ForEach(directory => this.fileSystem.DirectoryDelete(directory));
                this.fileSystem.DirectoryDelete(this.fileSystem.GetDirectory(ToolkitsLocation));
            }
            if (this.fileSystem.DirectoryExists(DraftsLocation))
            {
                this.fileSystem.GetSubDirectories(DraftsLocation)
                    .ToList()
                    .ForEach(directory => this.fileSystem.DirectoryDelete(directory));
                this.fileSystem.DirectoryDelete(this.fileSystem.GetDirectory(DraftsLocation));
            }

            var exportedDirectory = GetExportedToolkitDirectory();
            this.fileSystem.DeleteAllDirectoryFiles(exportedDirectory, $"*{ToolkitInstallerFileExtension}");

            this.localStateRepository.DestroyAll();
        }

        public string ToolkitsLocation => this.fileSystem.MakeAbsolutePath(this.localStatePath, ToolkitDirectoryPath);

        public List<ToolkitDefinition> ListToolkits()
        {
            if (!this.fileSystem.DirectoryExists(ToolkitsLocation))
            {
                return new List<ToolkitDefinition>();
            }

            return this.fileSystem.GetSubDirectories(ToolkitsLocation)
                .Select(directory => directory.Name)
                .Where(ToolkitExists)
                .Select(GetToolkit)
                .OrderBy(p => p.PatternName)
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
                throw new AutomateException(ExceptionMessages.LocalMachineFileRepository_ToolkitNotFound
                    .Substitute(id));
            }

            return this.fileSystem.ReadAllText(filename)
                .FromJson<ToolkitDefinition>(this.persistableFactory);
        }

        private bool PatternExists(string id)
        {
            var filename = CreateFilenameForPatternById(id);
            return this.fileSystem.FileExists(filename);
        }

        private bool ToolkitExists(string id)
        {
            var filename = CreateFilenameForImportedToolkitById(id);
            return this.fileSystem.FileExists(filename);
        }

        private bool DraftExists(string id)
        {
            var filename = CreateFilenameForDraftById(id);
            return this.fileSystem.FileExists(filename);
        }

        private void EnsurePathExists(string filename)
        {
            this.fileSystem.EnsureFileDirectoryExists(filename);
        }

        internal string CreateFilenameForPatternById(string id)
        {
            var location = CreatePathForPattern(id);
            return this.fileSystem.MakeAbsolutePath(location, PatternDefinitionFilename);
        }

        private string CreatePathForPattern(string id)
        {
            return this.fileSystem.MakeAbsolutePath(PatternsLocation, id);
        }

        private string CreateFilenameForCodeTemplate(string id, string codeTemplateId, string fileExtension)
        {
            var patternLocation = CreatePathForPattern(id);
            var templateLocation = this.fileSystem.MakeAbsolutePath(patternLocation, CodeTemplateDirectoryName);
            var templateFilename = $"{codeTemplateId}{(fileExtension.StartsWith(".") ? "" : ".")}{fileExtension}";

            return this.fileSystem.MakeAbsolutePath(templateLocation, templateFilename);
        }

        private string CreateFilenameForExportedToolkit(string name, string version)
        {
            var filename = $"{name}_{version}{ToolkitInstallerFileExtension}";
            var directory = GetExportedToolkitDirectory();

            return this.fileSystem.MakeAbsolutePath(directory, filename);
        }

        private static string GetExportedToolkitDirectory()
        {
            return InfrastructureConstants.GetExportDirectory();
        }

        internal string CreateFilenameForImportedToolkitById(string id)
        {
            var location = CreatePathForToolkit(id);
            return this.fileSystem.MakeAbsolutePath(location, ToolkitDefinitionFilename);
        }

        private string CreatePathForToolkit(string id)
        {
            return this.fileSystem.MakeAbsolutePath(ToolkitsLocation, id);
        }

        internal string CreateFilenameForDraftById(string id)
        {
            var location = CreatePathForDraft(id);
            return this.fileSystem.MakeAbsolutePath(location, DraftDefinitionFilename);
        }

        private string CreatePathForDraft(string id)
        {
            return this.fileSystem.MakeAbsolutePath(DraftsLocation, id);
        }
    }
}