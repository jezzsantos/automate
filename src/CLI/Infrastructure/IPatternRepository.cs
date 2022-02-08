using System.Collections.Generic;
using automate.Domain;

namespace automate.Infrastructure
{
    internal interface IPatternRepository
    {
        string PatternLocation { get; }

        void NewPattern(PatternDefinition pattern);

        PatternDefinition GetPattern(string id);

        List<PatternDefinition> ListPatterns();

        void UpsertPattern(PatternDefinition pattern);

        void DestroyAll();

        PatternDefinition FindPatternByName(string name);

        PatternDefinition FindPatternById(string id);

        void UploadPatternCodeTemplate(PatternDefinition pattern, string codeTemplateId, IFile source);
    }
}