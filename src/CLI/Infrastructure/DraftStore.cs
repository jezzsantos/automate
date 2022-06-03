using System.Collections.Generic;
using System.Linq;
using Automate.CLI.Application;
using Automate.CLI.Domain;
using Automate.CLI.Extensions;

namespace Automate.CLI.Infrastructure
{
    internal class DraftStore : IDraftStore
    {
        private readonly ILocalStateRepository localStateRepository;
        private readonly IDraftRepository draftRepository;

        public DraftStore(string currentDirectory) : this(new JsonFileRepository(currentDirectory))
        {
        }

        private DraftStore(JsonFileRepository repository) : this(repository, repository)
        {
        }

        internal DraftStore(IDraftRepository draftRepository, ILocalStateRepository localStateRepository)
        {
            draftRepository.GuardAgainstNull(nameof(draftRepository));
            localStateRepository.GuardAgainstNull(nameof(localStateRepository));
            this.draftRepository = draftRepository;
            this.localStateRepository = localStateRepository;
        }

        public DraftDefinition GetCurrent()
        {
            var state = this.localStateRepository.GetLocalState();
            return state.CurrentDraft.HasValue()
                ? this.draftRepository.GetDraft(state.CurrentDraft)
                : null;
        }

        public void DestroyAll()
        {
            this.draftRepository.DestroyAll();
            this.localStateRepository.DestroyAll();
        }

        public DraftDefinition Create(DraftDefinition draft)
        {
            draft.GuardAgainstNull(nameof(draft));

            this.draftRepository.NewDraft(draft);

            var state = this.localStateRepository.GetLocalState();
            state.SetCurrentDraft(draft.Id);
            this.localStateRepository.SaveLocalState(state);

            return draft;
        }

        public void Save(DraftDefinition draft)
        {
            this.draftRepository.UpsertDraft(draft);
        }

        public List<DraftDefinition> ListAll()
        {
            return this.draftRepository.ListDrafts();
        }

        public DraftDefinition FindById(string id)
        {
            return ListAll()
                .FirstOrDefault(sol => sol.Id == id);
        }

        public void ChangeCurrent(string id)
        {
            var draft = this.draftRepository.FindDraftById(id);
            if (draft.NotExists())
            {
                throw new AutomateException(
                    ExceptionMessages.DraftStore_NotFoundAtLocationWithId.Format(id,
                        this.draftRepository.DraftLocation));
            }

            var state = this.localStateRepository.GetLocalState();
            state.SetCurrentDraft(draft.Id);
            this.localStateRepository.SaveLocalState(state);
        }
    }
}