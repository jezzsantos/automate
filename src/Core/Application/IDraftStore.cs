using System.Collections.Generic;
using Automate.Domain;

namespace Automate.Application
{
    public interface IDraftStore
    {
        DraftDefinition Create(DraftDefinition draft);

        void Save(DraftDefinition draft);

        List<DraftDefinition> ListAll();

        DraftDefinition FindById(string id);

        DraftDefinition ChangeCurrent(string id);

        DraftDefinition GetCurrent();

        void DestroyAll();
    }
}