using System;
using System.Collections.Generic;
using automate.Extensions;

namespace automate
{
    internal class PatternStore
    {
        private readonly IPatternRepository repository;

        public PatternStore(string currentDirectory) : this(new JsonFilePatternRepository(currentDirectory))
        {
        }

        internal PatternStore(IPatternRepository repository)
        {
            repository.GuardAgainstNull(nameof(repository));
            this.repository = repository;
        }

        public PatternMetaModel GetCurrent()
        {
            var state = this.repository.GetState();
            return state.Current.HasValue()
                ? this.repository.Get(state.Current)
                : null;
        }

        public List<PatternMetaModel> LoadAll()
        {
            return this.repository.List();
        }

        public void SaveAll(List<PatternMetaModel> patterns)
        {
            patterns.ForEach(pattern => this.repository.Upsert(pattern));
        }

        public PatternMetaModel Find(string name)
        {
            name.GuardAgainstNullOrEmpty(nameof(name));
            var pattern = this.repository.FindByName(name);
            if (pattern.NotExists())
            {
                throw new PatternException(
                    ExceptionMessages.PatternStore_NotFoundAtLocationWithId.Format(name, this.repository.Location));
            }

            return pattern;
        }

        public PatternMetaModel Create(string name)
        {
            name.GuardAgainstNullOrEmpty(nameof(name));
            if (ExistsByName(name))
            {
                throw new PatternException(ExceptionMessages.PatternStore_FoundNamed.Format(name));
            }

            var id = Guid.NewGuid().ToString("N");
            var pattern = new PatternMetaModel(name, id);
            this.repository.New(pattern);

            var state = this.repository.GetState();
            state.Current = pattern.Id;
            this.repository.SaveState(state);

            return pattern;
        }

        public void ChangeCurrent(string id)
        {
            var pattern = this.repository.FindById(id);
            if (pattern.NotExists())
            {
                throw new PatternException(
                    ExceptionMessages.PatternStore_NotFoundAtLocationWithId.Format(id, this.repository.Location));
            }

            var state = this.repository.GetState();
            state.Current = pattern.Id;
            this.repository.SaveState(state);
        }

        public void DestroyAll()
        {
            this.repository.DestroyAll();
        }

        public void Save(PatternMetaModel pattern)
        {
            this.repository.Upsert(pattern);
        }

        private bool ExistsByName(string name)
        {
            name.GuardAgainstNullOrEmpty(nameof(name));
            return this.repository.FindByName(name).Exists();
        }
    }
}