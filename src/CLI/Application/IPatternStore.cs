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

        void ChangeCurrent(string id);

        string UploadCodeTemplate(PatternDefinition pattern, string codeTemplateId, IFile file);

        CodeTemplateContent DownloadCodeTemplate(PatternDefinition pattern, CodeTemplate codeTemplate);

        List<PatternDefinition> ListAll();
    }
}