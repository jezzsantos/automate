using System.Collections.Generic;
using Automate.Authoring.Domain;

namespace Automate.Authoring.Application
{
    public interface IPatternStore
    {
        void Save(PatternDefinition pattern);

        PatternDefinition GetCurrent();

        PatternDefinition Create(PatternDefinition pattern);

        PatternDefinition FindById(string id);

        PatternDefinition ChangeCurrent(string id);

        string UploadCodeTemplate(PatternDefinition pattern, string codeTemplateId, IFile file);

        string GetCodeTemplateLocation(PatternDefinition pattern, CodeTemplate codeTemplate);

        CodeTemplateContent DownloadCodeTemplate(PatternDefinition pattern, CodeTemplate codeTemplate);

        List<PatternDefinition> ListAll();

        void DeleteCodeTemplate(PatternDefinition pattern, CodeTemplate codeTemplate);
    }
}