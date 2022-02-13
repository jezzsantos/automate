using System;
using System.Collections.Generic;
using System.Linq;
using automate.Domain;
using automate.Extensions;

namespace automate.Infrastructure
{
    internal class MemoryRepository : IPatternRepository, IToolkitRepository, ISolutionRepository, ILocalStateRepository
    {
        public const string InMemoryLocation = "in-memory";

        // ReSharper disable once CollectionNeverQueried.Local
        private readonly Dictionary<string, byte[]> inMemoryCodeTemplates = new Dictionary<string, byte[]>();
        private readonly Dictionary<string, PatternDefinition> inMemoryPatterns =
            new Dictionary<string, PatternDefinition>();
        private readonly Dictionary<string, SolutionDefinition> inMemorySolutions =
            new Dictionary<string, SolutionDefinition>();

        // ReSharper disable once CollectionNeverUpdated.Local
        private readonly Dictionary<string, ToolkitDefinition> inMemoryToolkits =
            new Dictionary<string, ToolkitDefinition>();
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

            throw new AutomateException(ExceptionMessages.MemoryRepository_NotFound.Format(id));
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
            this.inMemorySolutions.Clear();
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

        public string SolutionLocation => InMemoryLocation;

        public void NewSolution(SolutionDefinition solution)
        {
            this.inMemorySolutions.Add(solution.Id, solution);
        }

        public void UpsertSolution(SolutionDefinition solution)
        {
            this.inMemorySolutions[solution.Id] = solution;
        }

        public SolutionDefinition GetSolution(string id)
        {
            if (this.inMemorySolutions.ContainsKey(id))
            {
                return this.inMemorySolutions[id];
            }

            throw new AutomateException(ExceptionMessages.MemoryRepository_NotFound.Format(id));
        }

        public SolutionDefinition FindSolutionById(string id)
        {
            return this.inMemorySolutions
                .FirstOrDefault(p => p.Key == id).Value;
        }

        public List<SolutionDefinition> ListSolutions()
        {
            return this.inMemorySolutions
                .Select(solution => solution.Value)
                .ToList();
        }

        public List<ToolkitDefinition> ListToolkits()
        {
            return this.inMemoryToolkits
                .Select(pair => pair.Value)
                .ToList();
        }

        public ToolkitDefinition FindToolkitById(string id)
        {
            return this.inMemoryToolkits
                .FirstOrDefault(t => t.Key == id).Value;
        }

        public ToolkitDefinition FindToolkitByName(string name)
        {
            return this.inMemoryToolkits
                .FirstOrDefault(p => p.Value.PatternName == name).Value;
        }

        public string ToolkitLocation => InMemoryLocation;

        public ToolkitDefinition GetToolkit(string id)
        {
            if (this.inMemoryToolkits.ContainsKey(id))
            {
                return this.inMemoryToolkits[id];
            }

            throw new AutomateException(ExceptionMessages.MemoryRepository_NotFound.Format(id));
        }

        public string ExportToolkit(ToolkitDefinition toolkit)
        {
            throw new NotImplementedException();
        }

        public void ImportToolkit(ToolkitDefinition toolkit)
        {
            this.inMemoryToolkits[toolkit.Id] = toolkit;
        }
    }
}