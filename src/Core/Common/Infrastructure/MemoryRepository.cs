using System;
using System.Collections.Generic;
using System.Linq;
using Automate.Authoring.Application;
using Automate.Authoring.Domain;
using Automate.Authoring.Infrastructure;
using Automate.Common.Domain;
using Automate.Common.Extensions;
using Automate.Runtime.Domain;
using Automate.Runtime.Infrastructure;

namespace Automate.Common.Infrastructure
{
    internal class MemoryRepository : IPatternRepository, IToolkitRepository, IDraftRepository, ILocalStateRepository,
        IMachineRepository
    {
        public const string InMemoryLocation = "in-memory";

        // ReSharper disable once CollectionNeverQueried.Local
        private readonly Dictionary<string, byte[]> inMemoryCodeTemplates = new();
        private readonly Dictionary<string, DraftDefinition> inMemoryDrafts = new();
        private readonly Dictionary<string, PatternDefinition> inMemoryPatterns = new();

        // ReSharper disable once CollectionNeverUpdated.Local
        private readonly Dictionary<string, ToolkitDefinition> inMemoryToolkits = new();
        private LocalState inMemoryLocalState = new();
        private MachineState inMemoryMachineState = new();

        public string DraftLocation => InMemoryLocation;

        public void NewDraft(DraftDefinition draft)
        {
            this.inMemoryDrafts.Add(draft.Id, draft);
        }

        public void UpsertDraft(DraftDefinition draft)
        {
            this.inMemoryDrafts[draft.Id] = draft;
        }

        public DraftDefinition GetDraft(string id)
        {
            if (this.inMemoryDrafts.ContainsKey(id))
            {
                return this.inMemoryDrafts[id];
            }

            throw new AutomateException(ExceptionMessages.MemoryRepository_NotFound.Substitute(id));
        }

        public DraftDefinition FindDraftById(string id)
        {
            return this.inMemoryDrafts
                .FirstOrDefault(p => p.Key == id).Value;
        }

        public void DeleteDraft(string id)
        {
            if (!this.inMemoryDrafts.ContainsKey(id))
            {
                throw new AutomateException(ExceptionMessages.MemoryRepository_NotFound.Substitute(id));
            }

            this.inMemoryDrafts.Remove(id);
        }

        public List<DraftDefinition> ListDrafts()
        {
            return this.inMemoryDrafts
                .Select(draft => draft.Value)
                .ToList();
        }

        public void SaveLocalState(LocalState state)
        {
            this.inMemoryLocalState = state;
        }

        public LocalState GetLocalState()
        {
            return this.inMemoryLocalState;
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

            throw new AutomateException(ExceptionMessages.MemoryRepository_NotFound.Substitute(id));
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

        public MachineState GetMachineState()
        {
            return this.inMemoryMachineState;
        }

        public void SaveMachineState(MachineState state)
        {
            this.inMemoryMachineState = state;
        }

        public void DestroyAll()
        {
            this.inMemoryPatterns.Clear();
            this.inMemoryToolkits.Clear();
            this.inMemoryDrafts.Clear();
            this.inMemoryLocalState = new LocalState();
            this.inMemoryMachineState = new MachineState();
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

        public string UploadPatternCodeTemplate(PatternDefinition pattern, string codeTemplateId, IFile file)
        {
            this.inMemoryCodeTemplates.Add(codeTemplateId, file.GetContents());
            return InMemoryLocation;
        }

        public string GetCodeTemplateLocation(PatternDefinition pattern, string codeTemplateId, string extension)
        {
            return InMemoryLocation;
        }

        public void DeleteTemplate(PatternDefinition pattern, string codeTemplateId, string extension)
        {
            this.inMemoryCodeTemplates.Remove(codeTemplateId);
        }

        public CodeTemplateContent DownloadPatternCodeTemplate(PatternDefinition pattern, string codeTemplateId,
            string extension)
        {
            return new CodeTemplateContent
            {
                Content = this.inMemoryCodeTemplates[codeTemplateId],
                LastModifiedUtc = DateTime.UtcNow
            };
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

            throw new AutomateException(ExceptionMessages.MemoryRepository_NotFound.Substitute(id));
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