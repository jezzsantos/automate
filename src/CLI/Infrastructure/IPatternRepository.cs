using System.Collections.Generic;
using Automate.CLI.Domain;

namespace Automate.CLI.Infrastructure
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

        string UploadPatternCodeTemplate(PatternDefinition pattern, string codeTemplateId, IFile source);

        string GetCodeTemplateLocation(PatternDefinition pattern, string codeTemplateId, string extension);

        void DeleteTemplate(PatternDefinition pattern, string codeTemplateId, string extension);

        CodeTemplateContent DownloadPatternCodeTemplate(PatternDefinition pattern, string codeTemplateId,
            string extension);
    }
}