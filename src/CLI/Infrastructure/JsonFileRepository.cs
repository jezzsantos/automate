using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Automate.CLI.Domain;
using Automate.CLI.Extensions;

namespace Automate.CLI.Infrastructure
{
    internal class JsonFileRepository : IPatternRepository, IToolkitRepository, ISolutionRepository,
        ILocalStateRepository
    {
        private const string PatternDefinitionFilename = "MetaModel.json";
        private const string SolutionDefinitionFilename = "Solution.json";
        private const string CodeTemplateDirectoryName = "CodeTemplates";
        private const string ToolkitInstallerFileExtension = ".toolkit";
        private static readonly string PatternDirectoryPath =
            Path.Combine(InfrastructureConstants.RootPersistencePath, "patterns");
        private static readonly string ToolkitDirectoryPath =
            Path.Combine(InfrastructureConstants.RootPersistencePath, "toolkits");
        private static readonly string SolutionDirectoryPath =
            Path.Combine(InfrastructureConstants.RootPersistencePath, "solutions");
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
            if (Directory.Exists(SolutionLocation))
            {
                Directory.GetDirectories(SolutionLocation)
                    .ToList()
                    .ForEach(directory => Directory.Delete(directory, true));
            }

            this.localStateRepository.DestroyAll();
        }

        public string SolutionLocation => Path.Combine(this.currentDirectory, SolutionDirectoryPath);

        public void NewSolution(SolutionDefinition solution)
        {
            UpsertSolution(solution);
        }

        public void UpsertSolution(SolutionDefinition solution)
        {
            var filename = CreateFilenameForSolutionById(solution.Id);
            EnsurePathExists(filename);

            using (var file = File.CreateText(filename))
            {
                file.Write(solution.ToJson());
            }
        }

        public SolutionDefinition GetSolution(string id)
        {
            var filename = CreateFilenameForSolutionById(id);
            if (!File.Exists(filename))
            {
                throw new AutomateException(ExceptionMessages.JsonFileRepository_SolutionNotFound.Format(id));
            }

            return File.ReadAllText(filename).FromJson<SolutionDefinition>();
        }

        public List<SolutionDefinition> ListSolutions()
        {
            if (!Directory.Exists(SolutionLocation))
            {
                return new List<SolutionDefinition>();
            }

            return Directory.GetDirectories(SolutionLocation)
                .Select(path => new DirectoryInfo(path).Name)
                .Select(GetSolution)
                .ToList();
        }

        public SolutionDefinition FindSolutionById(string id)
        {
            return ListSolutions()
                .FirstOrDefault(solution => solution.Id == id);
        }

        public List<ToolkitDefinition> ListToolkits()
        {
            if (!Directory.Exists(PatternLocation))
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
                file.Write(toolkit.ToJson());
            }

            return filename;
        }

        public void ImportToolkit(ToolkitDefinition toolkit)
        {
            var filename = CreateFilenameForImportedToolkitById(toolkit.Id);
            EnsurePathExists(filename);

            using (var file = File.CreateText(filename))
            {
                file.Write(toolkit.ToJson());
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

            return File.ReadAllText(filename).FromJson<ToolkitDefinition>();
        }

        private static void EnsurePathExists(string filename)
        {
            var directory = Directory.GetParent(filename)?.FullName ?? string.Empty;
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
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

        private string CreateFilenameForCodeTemplate(string id, string codeTemplateId, string templateFullPath)
        {
            var patternLocation = CreatePathForPattern(id);
            var templateLocation = Path.Combine(patternLocation, CodeTemplateDirectoryName);
            var templateFilename = $"{codeTemplateId}{Path.GetExtension(templateFullPath)}";

            return Path.Combine(this.currentDirectory, Path.Combine(templateLocation, templateFilename));
        }

        private static string CreateFilenameForExportedToolkit(string name, string version)
        {
            var filename = Path.ChangeExtension($"{name}_{version}", ToolkitInstallerFileExtension);
            var directory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            return Path.Combine(directory, filename);
        }

        private string CreateFilenameForImportedToolkitById(string id)
        {
            var location = CreatePathForToolkit(id);
            return Path.Combine(location, PatternDefinitionFilename);
        }

        private string CreatePathForToolkit(string id)
        {
            return Path.Combine(ToolkitLocation, id);
        }

        private string CreateFilenameForSolutionById(string id)
        {
            var location = CreatePathForSolution(id);
            return Path.Combine(location, SolutionDefinitionFilename);
        }

        private string CreatePathForSolution(string id)
        {
            return Path.Combine(SolutionLocation, id);
        }
    }
}