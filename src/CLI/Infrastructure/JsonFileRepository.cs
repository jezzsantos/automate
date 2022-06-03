using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Automate.CLI.Domain;
using Automate.CLI.Extensions;

namespace Automate.CLI.Infrastructure
{
    internal class JsonFileRepository : IPatternRepository, IToolkitRepository, IDraftRepository,
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
        private readonly ILocalStateRepository localStateRepository;
        private readonly IPersistableFactory persistableFactory;

        public JsonFileRepository(string currentDirectory) : this(currentDirectory,
            new LocalStateRepository(currentDirectory), new AutomatePersistableFactory())
        {
        }

        private JsonFileRepository(string currentDirectory, ILocalStateRepository localStateRepository,
            IPersistableFactory persistableFactory)
        {
            currentDirectory.GuardAgainstNullOrEmpty(nameof(currentDirectory));
            localStateRepository.GuardAgainstNull(nameof(localStateRepository));
            persistableFactory.GuardAgainstNull(nameof(persistableFactory));
            this.currentDirectory = currentDirectory;
            this.localStateRepository = localStateRepository;
            this.persistableFactory = persistableFactory;
        }

        public static string GetCodeTemplateLocation(string patternId, string codeTemplateId, string extension)
        {
            patternId.GuardAgainstNullOrEmpty(nameof(patternId));
            codeTemplateId.GuardAgainstNullOrEmpty(nameof(codeTemplateId));
            extension.GuardAgainstNullOrEmpty(nameof(extension));

            return $"{PatternDirectoryPath}/{patternId}/{CodeTemplateDirectoryName}/{codeTemplateId}.{extension}";
        }

        public string DraftLocation => Path.Combine(this.currentDirectory, DraftDirectoryPath);

        public void NewDraft(DraftDefinition draft)
        {
            UpsertDraft(draft);
        }

        public void UpsertDraft(DraftDefinition draft)
        {
            var filename = CreateFilenameForDraftById(draft.Id);
            EnsurePathExists(filename);

            using (var file = File.CreateText(filename))
            {
                file.Write(draft.ToJson(this.persistableFactory));
            }
        }

        public DraftDefinition GetDraft(string id)
        {
            var filename = CreateFilenameForDraftById(id);
            if (!File.Exists(filename))
            {
                throw new AutomateException(ExceptionMessages.JsonFileRepository_DraftNotFound.Format(id));
            }

            return File.ReadAllText(filename).FromJson<DraftDefinition>(this.persistableFactory);
        }

        public List<DraftDefinition> ListDrafts()
        {
            if (!Directory.Exists(DraftLocation))
            {
                return new List<DraftDefinition>();
            }

            return Directory.GetDirectories(DraftLocation)
                .Select(path => new DirectoryInfo(path).Name)
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

        public string PatternLocation => Path.Combine(this.currentDirectory, PatternDirectoryPath);

        public void NewPattern(PatternDefinition pattern)
        {
            UpsertPattern(pattern);
        }

        public PatternDefinition GetPattern(string id)
        {
            var filename = CreateFilenameForPatternById(id);
            if (!File.Exists(filename))
            {
                throw new AutomateException(ExceptionMessages.JsonFileRepository_PatternNotFound.Format(id));
            }

            return File.ReadAllText(filename).FromJson<PatternDefinition>(this.persistableFactory);
        }

        public List<PatternDefinition> ListPatterns()
        {
            if (!Directory.Exists(PatternLocation))
            {
                return new List<PatternDefinition>();
            }

            return Directory.GetDirectories(PatternLocation)
                .Select(path => new DirectoryInfo(path).Name)
                .Select(GetPattern)
                .ToList();
        }

        public void UpsertPattern(PatternDefinition pattern)
        {
            var filename = CreateFilenameForPatternById(pattern.Id);
            EnsurePathExists(filename);

            using (var file = File.CreateText(filename))
            {
                file.Write(pattern.ToJson(this.persistableFactory));
            }
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

        public CodeTemplateContent DownloadPatternCodeTemplate(PatternDefinition pattern, string codeTemplateId,
            string extension)
        {
            var path = CreateFilenameForCodeTemplate(pattern.Id, codeTemplateId, extension);
            var file = new SystemIoFile(path);
            return new CodeTemplateContent
            {
                Content = file.GetContents(),
                LastModifiedUtc = file.LastModifiedUtc
            };
        }

        public void DestroyAll()
        {
            if (Directory.Exists(PatternLocation))
            {
                Directory.GetDirectories(PatternLocation)
                    .ToList()
                    .ForEach(directory => Directory.Delete(directory, true));
            }
            if (Directory.Exists(ToolkitLocation))
            {
                Directory.GetDirectories(ToolkitLocation)
                    .ToList()
                    .ForEach(directory => Directory.Delete(directory, true));
            }
            if (Directory.Exists(DraftLocation))
            {
                Directory.GetDirectories(DraftLocation)
                    .ToList()
                    .ForEach(directory => Directory.Delete(directory, true));
            }

            var directory = new DirectoryInfo(GetExportedToolkitDirectory());
            if (directory.Exists)
            {
                var toolkits = directory.GetFiles($"*{ToolkitInstallerFileExtension}");
                foreach (var toolkit in toolkits)
                {
                    toolkit.Delete();
                }
            }

            this.localStateRepository.DestroyAll();
        }

        public List<ToolkitDefinition> ListToolkits()
        {
            if (!Directory.Exists(ToolkitLocation))
            {
                return new List<ToolkitDefinition>();
            }

            return Directory.GetDirectories(ToolkitLocation)
                .Select(path => new DirectoryInfo(path).Name)
                .Select(GetToolkit)
                .ToList();
        }

        public string ExportToolkit(ToolkitDefinition toolkit)
        {
            var filename = CreateFilenameForExportedToolkit(toolkit.PatternName, toolkit.Version);
            EnsurePathExists(filename);

            using (var file = File.CreateText(filename))
            {
                file.Write(toolkit.ToJson(this.persistableFactory));
            }

            return filename;
        }

        public void ImportToolkit(ToolkitDefinition toolkit)
        {
            var filename = CreateFilenameForImportedToolkitById(toolkit.Id);
            EnsurePathExists(filename);

            using (var file = File.CreateText(filename))
            {
                file.Write(toolkit.ToJson(this.persistableFactory));
            }
        }

        public string ToolkitLocation => Path.Combine(this.currentDirectory, ToolkitDirectoryPath);

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
            if (!File.Exists(filename))
            {
                throw new AutomateException(ExceptionMessages.JsonFileRepository_ToolkitNotFound.Format(id));
            }

            return File.ReadAllText(filename).FromJson<ToolkitDefinition>(this.persistableFactory);
        }

        private static void EnsurePathExists(string filename)
        {
            var fullPath = Directory.GetParent(filename)?.FullName ?? string.Empty;
            if (fullPath.HasValue())
            {
                var directory = new DirectoryInfo(fullPath);
                if (!directory.Exists)
                {
                    directory.Create();
                }
            }
        }

        private string CreateFilenameForPatternById(string id)
        {
            var location = CreatePathForPattern(id);
            return Path.Combine(location, PatternDefinitionFilename);
        }

        private string CreatePathForPattern(string id)
        {
            return Path.Combine(PatternLocation, id);
        }

        private string CreateFilenameForCodeTemplate(string id, string codeTemplateId, string fileExtension)
        {
            var patternLocation = CreatePathForPattern(id);
            var templateLocation = Path.Combine(patternLocation, CodeTemplateDirectoryName);
            var templateFilename = $"{codeTemplateId}{(fileExtension.StartsWith(".") ? "" : ".")}{fileExtension}";

            return Path.Combine(this.currentDirectory, Path.Combine(templateLocation, templateFilename));
        }

        private static string CreateFilenameForExportedToolkit(string name, string version)
        {
            var filename = $"{name}_{version}{ToolkitInstallerFileExtension}";
            var directory = GetExportedToolkitDirectory();

            return Path.Combine(directory, filename);
        }

        private static string GetExportedToolkitDirectory()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        }

        private string CreateFilenameForImportedToolkitById(string id)
        {
            var location = CreatePathForToolkit(id);
            return Path.Combine(location, ToolkitDefinitionFilename);
        }

        private string CreatePathForToolkit(string id)
        {
            return Path.Combine(ToolkitLocation, id);
        }

        private string CreateFilenameForDraftById(string id)
        {
            var location = CreatePathForDraft(id);
            return Path.Combine(location, DraftDefinitionFilename);
        }

        private string CreatePathForDraft(string id)
        {
            return Path.Combine(DraftLocation, id);
        }
    }
}