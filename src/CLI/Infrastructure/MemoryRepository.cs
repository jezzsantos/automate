using System;
using System.Collections.Generic;
using System.Linq;
using automate.Domain;
using automate.Extensions;

namespace automate.Infrastructure
{
    internal class MemoryRepository : IPatternRepository, IToolkitRepository, ILocalStateRepository
    {
        public const string InMemoryLocation = "in-memory";

        // ReSharper disable once CollectionNeverQueried.Local
        private readonly Dictionary<string, byte[]> inMemoryCodeTemplates = new Dictionary<string, byte[]>();
        private readonly Dictionary<string, PatternDefinition> inMemoryPatterns =
            new Dictionary<string, PatternDefinition>();

        // ReSharper disable once CollectionNeverUpdated.Local
        private readonly Dictionary<string, PatternToolkitDefinition> inMemoryToolkits =
            new Dictionary<string, PatternToolkitDefinition>();
        private LocalState inMemoryState = new LocalState();

        public void SaveLocalState(LocalState state)
        {
            this.inMemoryState = state;
        }

        public LocalState GetLocalState()
        {
            return this.inMemoryState;
        }

        public string PatternLocation => InMemoryLocation;

        public void NewPattern(PatternDefinition pattern)
        {
            this.inMemoryPatterns.Add(pattern.Id, pattern);
        }

        public PatternDefinition GetPattern(string id)
        {
            if (this.inMemoryPatterns.ContainsKey(id))
            {
                return this.inMemoryPatterns[id];
            }

            throw new PatternException(ExceptionMessages.MemoryRepository_NotFound.Format(id));
        }

        public List<PatternDefinition> ListPatterns()
        {
            return this.inMemoryPatterns
                .Select(pattern => pattern.Value)
                .ToList();
        }

        public void UpsertPattern(PatternDefinition pattern)
        {
            this.inMemoryPatterns[pattern.Id] = pattern;
        }

        public void DestroyAll()
        {
            this.inMemoryPatterns.Clear();
            this.inMemoryToolkits.Clear();
            this.inMemoryState = new LocalState();
        }

        public PatternDefinition FindPatternByName(string name)
        {
            return this.inMemoryPatterns
                .FirstOrDefault(p => p.Value.Name == name).Value;
        }

        public PatternDefinition FindPatternById(string id)
        {
            return this.inMemoryPatterns
                .FirstOrDefault(p => p.Key == id).Value;
        }

        public void UploadPatternCodeTemplate(PatternDefinition pattern, string codeTemplateId, IFile file)
        {
            this.inMemoryCodeTemplates.Add(codeTemplateId, file.GetContents());
        }

        public string ToolkitLocation => InMemoryLocation;

        public PatternToolkitDefinition GetToolkit(string id)
        {
            if (this.inMemoryToolkits.ContainsKey(id))
            {
                return this.inMemoryToolkits[id];
            }

            throw new PatternException(ExceptionMessages.MemoryRepository_NotFound.Format(id));
        }

        public string SaveToolkit(PatternToolkitDefinition toolkit)
        {
            throw new NotImplementedException();
        }
    }
}