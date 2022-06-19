using System.Collections.Generic;
using Automate.Application;
using Automate.Domain;
using Automate.Extensions;

namespace Automate.Infrastructure
{
    public class PatternStore : IPatternStore
    {
        private readonly ILocalStateRepository localStateRepository;
        private readonly IPatternRepository patternRepository;

        public PatternStore(IPatternRepository patternRepository, ILocalStateRepository localStateRepository)
        {
            patternRepository.GuardAgainstNull(nameof(patternRepository));
            localStateRepository.GuardAgainstNull(nameof(localStateRepository));
            this.patternRepository = patternRepository;
            this.localStateRepository = localStateRepository;
        }

        public List<PatternDefinition> LoadAll()
        {
            return this.patternRepository.ListPatterns();
        }

        public void SaveAll(List<PatternDefinition> patterns)
        {
            patterns.ForEach(pattern => this.patternRepository.UpsertPattern(pattern));
        }

        public void DestroyAll()
        {
            this.patternRepository.DestroyAll();
            this.localStateRepository.DestroyAll();
        }

        public PatternDefinition GetCurrent()
        {
            var state = this.localStateRepository.GetLocalState();
            return state.CurrentPattern.HasValue()
                ? this.patternRepository.GetPattern(state.CurrentPattern)
                : null;
        }

        public PatternDefinition Find(string name)
        {
            name.GuardAgainstNullOrEmpty(nameof(name));
            var pattern = this.patternRepository.FindPatternByName(name);
            if (pattern.NotExists())
            {
                throw new AutomateException(
                    ExceptionMessages.PatternStore_NotFoundAtLocationWithId.Substitute(name,
                        this.patternRepository.PatternLocation));
            }

            return pattern;
        }

        public PatternDefinition Create(PatternDefinition pattern)
        {
            pattern.GuardAgainstNull(nameof(pattern));
            if (ExistsByName(pattern.Name))
            {
                throw new AutomateException(ExceptionMessages.PatternStore_FoundNamed.Substitute(pattern.Name));
            }

            this.patternRepository.NewPattern(pattern);

            var state = this.localStateRepository.GetLocalState();
            state.SetCurrentPattern(pattern.Id);
            this.localStateRepository.SaveLocalState(state);

            return pattern;
        }

        public PatternDefinition ChangeCurrent(string id)
        {
            var pattern = this.patternRepository.FindPatternById(id);
            if (pattern.NotExists())
            {
                throw new AutomateException(
                    ExceptionMessages.PatternStore_NotFoundAtLocationWithId.Substitute(id,
                        this.patternRepository.PatternLocation));
            }

            var state = this.localStateRepository.GetLocalState();
            state.SetCurrentPattern(pattern.Id);
            this.localStateRepository.SaveLocalState(state);

            return pattern;
        }

        public void Save(PatternDefinition pattern)
        {
            this.patternRepository.UpsertPattern(pattern);
        }

        public string UploadCodeTemplate(PatternDefinition pattern, string codeTemplateId, IFile source)
        {
            return this.patternRepository.UploadPatternCodeTemplate(pattern, codeTemplateId, source);
        }

        public string GetCodeTemplateLocation(PatternDefinition pattern, CodeTemplate codeTemplate)
        {
            return this.patternRepository.GetCodeTemplateLocation(pattern, codeTemplate.Id,
                codeTemplate.Metadata.OriginalFileExtension);
        }

        public CodeTemplateContent DownloadCodeTemplate(PatternDefinition pattern, CodeTemplate codeTemplate)
        {
            return this.patternRepository.DownloadPatternCodeTemplate(pattern, codeTemplate.Id,
                codeTemplate.Metadata.OriginalFileExtension);
        }

        public List<PatternDefinition> ListAll()
        {
            return this.patternRepository.ListPatterns();
        }

        public void DeleteCodeTemplate(PatternDefinition pattern, CodeTemplate codeTemplate)
        {
            this.patternRepository.DeleteTemplate(pattern, codeTemplate.Id,
                codeTemplate.Metadata.OriginalFileExtension);
        }

        private bool ExistsByName(string name)
        {
            name.GuardAgainstNullOrEmpty(nameof(name));
            return this.patternRepository.FindPatternByName(name).Exists();
        }
    }
}