using automate.Domain;

namespace automate.Infrastructure
{
    internal interface IPatternStore
    {
        void Save(PatternDefinition pattern);

        PatternDefinition GetCurrent();

        PatternDefinition Create(string name);

        PatternDefinition Find(string name);

        void ChangeCurrent(string id);

        void UploadCodeTemplate(PatternDefinition pattern, string codeTemplateId, IFile file);
    }
}