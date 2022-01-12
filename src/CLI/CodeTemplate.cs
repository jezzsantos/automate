using automate.Extensions;

namespace automate
{
    internal class CodeTemplate
    {
        public CodeTemplate(string name, string fullPath)
        {
            name.GuardAgainstNullOrEmpty(nameof(name));
            fullPath.GuardAgainstNullOrEmpty(nameof(fullPath));
            Name = name;
            FullPath = fullPath;
        }

        public CodeTemplate()
        {
        }

        public string Name { get; set; }

        public string FullPath { get; set; }
    }
}