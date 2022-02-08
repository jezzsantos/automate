using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using automate.Domain;
using automate.Extensions;

namespace automate.Infrastructure
{
    internal class JsonFileRepository : IPatternRepository, IToolkitRepository, ILocalStateRepository
    {
        private const string PatternMetaModelFilename = "MetaModel.json";
        private const string CodeTemplateDirectoryName = "CodeTemplates";
        private const string ToolkitFileExtension = ".toolkit";
        private static readonly string PatternDirectoryPath =
            Path.Combine(InfrastructureConstants.RootPersistencePath, "patterns");
        private static readonly string ToolkitDirectoryPath =
            Path.Combine(InfrastructureConstants.RootPersistencePath, "toolkits");
        private readonly string currentDirectory;
        private readonly ILocalStateRepository localStateRepository;

        public JsonFileRepository(string currentDirectory) : this(currentDirectory,
            new LocalStateRepository(currentDirectory))
        {
        }

        private JsonFileRepository(string currentDirectory, ILocalStateRepository localStateRepository)
        {
            currentDirectory.GuardAgainstNullOrEmpty(nameof(currentDirectory));
            localStateRepository.GuardAgainstNull(nameof(localStateRepository));
            this.currentDirectory = currentDirectory;
            this.localStateRepository = localStateRepository;
        }

        public List<PatternToolkitDefinition> ListToolkits()
        {
            if (!Directory.Exists(PatternLocation))
            {
                return new List<PatternToolkitDefinition>();
            }

            return Directory.GetDirectories(ToolkitLocation)
                .Select(path => new DirectoryInfo(path).Name)
                .Select(GetToolkit)
                .ToList();
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
                throw new PatternException(ExceptionMessages.JsonFileRepository_PatternNotFound.Format(id));
            }

            return File.ReadAllText(filename).FromJson<PatternDefinition>();
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
                file.Write(pattern.ToJson());
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

        public void UploadPatternCodeTemplate(PatternDefinition pattern, string codeTemplateId, IFile file)
        {
            var uploadedFilePath = CreateFilenameForCodeTemplate(pattern.Id, codeTemplateId, file.FullPath);
            file.CopyTo(uploadedFilePath);
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

            this.localStateRepository.DestroyAll();
        }

        public string ExportToolkit(PatternToolkitDefinition toolkit)
        {
            var filename = CreateFilenameForExportedToolkit(toolkit.PatternName, toolkit.Version);
            EnsurePathExists(filename);

            using (var file = File.CreateText(filename))
            {
                file.Write(toolkit.ToJson());
            }

            return filename;
        }

        public void ImportToolkit(PatternToolkitDefinition toolkit)
        {
            var filename = CreateFilenameForImportedToolkitById(toolkit.Id);
            EnsurePathExists(filename);

            using (var file = File.CreateText(filename))
            {
                file.Write(toolkit.ToJson());
            }
        }

        public string ToolkitLocation => Path.Combine(this.currentDirectory, ToolkitDirectoryPath);

        public PatternToolkitDefinition FindToolkitById(string id)
        {
            return ListToolkits()
                .FirstOrDefault(toolkit => toolkit.Id == id);
        }

        public PatternToolkitDefinition GetToolkit(string id)
        {
            var filename = CreateFilenameForImportedToolkitById(id);
            if (!File.Exists(filename))
            {
                throw new PatternException(ExceptionMessages.JsonFileRepository_ToolkitNotFound.Format(id));
            }

            return File.ReadAllText(filename).FromJson<PatternToolkitDefinition>();
        }

        private static void EnsurePathExists(string filename)
        {
            var directory = Directory.GetParent(filename)?.FullName ?? string.Empty;
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        private string CreatePathForPattern(string id)
        {
            return Path.Combine(PatternLocation, id);
        }

        private string CreateFilenameForPatternById(string id)
        {
            var location = CreatePathForPattern(id);
            return Path.Combine(location, PatternMetaModelFilename);
        }

        private string CreateFilenameForCodeTemplate(string id, string codeTemplateId, string templateFullPath)
        {
            var patternLocation = CreatePathForPattern(id);
            var templateLocation = Path.Combine(patternLocation, CodeTemplateDirectoryName);
            var templateFilename = $"{codeTemplateId}{Path.GetExtension(templateFullPath)}";

            return Path.Combine(this.currentDirectory, Path.Combine(templateLocation, templateFilename));
        }

        private static string CreateFilenameForExportedToolkit(string name, string version)
        {
            var filename = Path.ChangeExtension($"{name}_{version}", ToolkitFileExtension);
            var directory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            return Path.Combine(directory, filename);
        }

        private string CreateFilenameForImportedToolkitById(string id)
        {
            var location = CreatePathForToolkit(id);
            return Path.Combine(location, PatternMetaModelFilename);
        }

        private string CreatePathForToolkit(string id)
        {
            return Path.Combine(ToolkitLocation, id);
        }
    }
}