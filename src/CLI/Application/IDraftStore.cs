using System.Collections.Generic;
using Automate.CLI.Domain;

namespace Automate.CLI.Application
{
    internal interface IDraftStore
    {
        DraftDefinition Create(DraftDefinition draft);

        void Save(DraftDefinition draft);

        List<DraftDefinition> ListAll();

        DraftDefinition FindById(string id);

        void ChangeCurrent(string id);

        DraftDefinition GetCurrent();

        void DestroyAll();
    }
}