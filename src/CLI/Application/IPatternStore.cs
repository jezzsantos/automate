using System.Collections.Generic;
using Automate.CLI.Domain;

namespace Automate.CLI.Application
{
    internal interface IPatternStore
    {
        void Save(PatternDefinition pattern);

        PatternDefinition GetCurrent();

        PatternDefinition Create(PatternDefinition pattern);

        PatternDefinition Find(string name);

        PatternDefinition ChangeCurrent(string id);

        string UploadCodeTemplate(PatternDefinition pattern, string codeTemplateId, IFile file);

        string GetCodeTemplateLocation(PatternDefinition pattern, CodeTemplate codeTemplate);

        CodeTemplateContent DownloadCodeTemplate(PatternDefinition pattern, CodeTemplate codeTemplate);

        List<PatternDefinition> ListAll();
    }
}