using System.Collections.Generic;
using automate.Extensions;

namespace automate
{
    internal class PatternMetaModel
    {
        public PatternMetaModel(string name, string id)
        {
            name.GuardAgainstNullOrEmpty(nameof(name));
            id.GuardAgainstNullOrEmpty(nameof(id));
            Name = name;
            Id = id;
            CodeTemplates = new List<CodeTemplate>();
        }

        public PatternMetaModel()
        {
        }

        public string Name { get; set; }

        public string Id { get; set; }

        public List<CodeTemplate> CodeTemplates { get; set; }
    }
}