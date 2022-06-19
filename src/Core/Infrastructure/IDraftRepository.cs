using System.Collections.Generic;
using Automate.Domain;

namespace Automate.Infrastructure
{
    public interface IDraftRepository
    {
        string DraftLocation { get; }

        void NewDraft(DraftDefinition draft);

        void UpsertDraft(DraftDefinition draft);

        DraftDefinition GetDraft(string id);

        DraftDefinition FindDraftById(string id);

        void DestroyAll();

        List<DraftDefinition> ListDrafts();
    }
}