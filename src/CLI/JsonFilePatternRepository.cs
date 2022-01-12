using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using automate.Extensions;

namespace automate
{
    internal class JsonFilePatternRepository : IPatternRepository
    {
        private const string PatternMetaModelFilename = "MetaModel.json";
        private const string StateFilename = "PatternState.json";
        private static readonly string patternDirectoryPath = Path.Combine(Constants.RootPersistencePath, "patterns");
        private readonly string currentDirectory;

        public JsonFilePatternRepository(string currentDirectory)
        {
            currentDirectory.GuardAgainstNullOrEmpty(nameof(currentDirectory));
            this.currentDirectory = currentDirectory;
        }

        public string Location => Path.Combine(this.currentDirectory, patternDirectoryPath);

        public void New(PatternMetaModel pattern)
        {
            Upsert(pattern);
        }

        public PatternMetaModel Get(string id)
        {
            var filename = CreateFilenameForPatternById(id);
            if (!File.Exists(filename))
            {
                throw new PatternException(ExceptionMessages.JsonFilePatternRepository_NotFound.Format(id));
            }

            return File.ReadAllText(filename).FromJson<PatternMetaModel>();
        }

        public List<PatternMetaModel> List()
        {
            if (!Directory.Exists(Location))
            {
                return new List<PatternMetaModel>();
            }

            return Directory.GetDirectories(Location)
                .Select(path => new DirectoryInfo(path).Name)
                .Select(Get)
                .ToList();
        }

        public void Upsert(PatternMetaModel pattern)
        {
            var filename = CreateFilenameForPatternById(pattern.Id);
            EnsurePathExists(filename);

            using (var file = File.CreateText(filename))
            {
                file.Write(pattern.ToJson());
            }
        }

        public PatternState GetState()
        {
            var filename = CreateFilenameForState();
            if (!File.Exists(filename))
            {
                var state = new PatternState();
                WriteState(filename, state);
                return state;
            }

            return File.ReadAllText(filename).FromJson<PatternState>();
        }

        public void SaveState(PatternState state)
        {
            var filename = CreateFilenameForState();
            WriteState(filename, state);
        }

        public PatternMetaModel FindByName(string name)
        {
            return List()
                .FirstOrDefault(pattern => pattern.Name == name);
        }

        public PatternMetaModel FindById(string id)
        {
            return List()
                .FirstOrDefault(pattern => pattern.Id == id);
        }

        public void DestroyAll()
        {
            if (!Directory.Exists(Location))
            {
                return;
            }

            Directory.GetDirectories(Location)
                .ToList()
                .ForEach(directory => Directory.Delete(directory, true));

            var stateFilename = CreateFilenameForState();
            File.Delete(stateFilename);
        }

        private static void WriteState(string filename, PatternState state)
        {
            EnsurePathExists(filename);
            using (var file = File.CreateText(filename))
            {
                file.Write(state.ToJson());
            }
        }

        private static string CreateFilenameForState()
        {
            return Path.Combine(patternDirectoryPath, StateFilename);
        }

        private static void EnsurePathExists(string filename)
        {
            var directory = Directory.GetParent(filename)?.FullName ?? string.Empty;
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        private static string CreatePathForPattern(string id)
        {
            return Path.Combine(patternDirectoryPath, id);
        }

        private static string CreateFilenameForPatternById(string id)
        {
            var location = CreatePathForPattern(id);
            return Path.Combine(location, PatternMetaModelFilename);
        }
    }

    internal static class JsonConversions
    {
        public static string ToJson<T>(this T instance) where T : new()
        {
            return JsonSerializer.Serialize(instance, new JsonSerializerOptions
            {
                WriteIndented = true,
                IgnoreNullValues = true,
                IncludeFields = false,
                DefaultIgnoreCondition = JsonIgnoreCondition.Never
            });
        }

        public static T FromJson<T>(this string json) where T : new()
        {
            return JsonSerializer.Deserialize<T>(json);
        }
    }
}