using System.Collections.Generic;
using Automate.CLI.Domain;

namespace Automate.CLI.Infrastructure
{
    internal interface IDraftRepository
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