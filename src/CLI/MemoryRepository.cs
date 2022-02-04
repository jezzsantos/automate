using System.Collections.Generic;
using System.Linq;
using automate.Extensions;

namespace automate
{
    internal class MemoryRepository : IPatternRepository
    {
        public const string InMemoryLocation = "in-memory";

        // ReSharper disable once CollectionNeverQueried.Local
        private readonly Dictionary<string, byte[]> inMemoryCodeTemplates = new Dictionary<string, byte[]>();
        private readonly Dictionary<string, PatternMetaModel> inMemoryPatterns =
            new Dictionary<string, PatternMetaModel>();
        private PatternState inMemoryState = new PatternState();

        public string Location => InMemoryLocation;

        public void New(PatternMetaModel pattern)
        {
            this.inMemoryPatterns.Add(pattern.Id, pattern);
        }

        public PatternMetaModel Get(string id)
        {
            if (this.inMemoryPatterns.ContainsKey(id))
            {
                return this.inMemoryPatterns[id];
            }

            throw new PatternException(ExceptionMessages.MemoryRepository_NotFound.Format(id));
        }

        public List<PatternMetaModel> List()
        {
            return this.inMemoryPatterns
                .Select(pattern => pattern.Value)
                .ToList();
        }

        public void Upsert(PatternMetaModel pattern)
        {
            this.inMemoryPatterns[pattern.Id] = pattern;
        }

        public void DestroyAll()
        {
            this.inMemoryPatterns.Clear();
        }

        public PatternState GetState()
        {
            return this.inMemoryState;
        }

        public void SaveState(PatternState state)
        {
            this.inMemoryState = state;
        }

        public PatternMetaModel FindByName(string name)
        {
            return this.inMemoryPatterns
                .FirstOrDefault(p => p.Value.Name == name).Value;
        }

        public PatternMetaModel FindById(string id)
        {
            return this.inMemoryPatterns
                .FirstOrDefault(p => p.Key == id).Value;
        }

        public void UploadCodeTemplate(PatternMetaModel pattern, string codeTemplateId, IFile file)
        {
            this.inMemoryCodeTemplates.Add(codeTemplateId, file.GetContents());
        }
    }
}