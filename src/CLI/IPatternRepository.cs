using System.Collections.Generic;

namespace automate
{
    internal interface IPatternRepository
    {
        string Location { get; }

        void New(PatternMetaModel pattern);

        PatternMetaModel Get(string id);

        List<PatternMetaModel> List();

        void Upsert(PatternMetaModel pattern);

        void DestroyAll();

        PatternState GetState();

        void SaveState(PatternState state);

        PatternMetaModel FindByName(string name);

        PatternMetaModel FindById(string id);

        void UploadCodeTemplate(PatternMetaModel pattern, string codeTemplateId, IFile source);
    }
}