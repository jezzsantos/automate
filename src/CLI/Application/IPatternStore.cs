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

        void UploadCodeTemplate(PatternDefinition pattern, string codeTemplateId, IFile file);

        byte[] DownloadCodeTemplate(PatternDefinition pattern, CodeTemplate codeTemplate);
    }
}