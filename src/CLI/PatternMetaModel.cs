using System.Collections.Generic;
using automate.Extensions;

namespace automate
{
    internal class PatternMetaModel : IPatternElement
    {
        public PatternMetaModel(string name)
        {
            name.GuardAgainstNullOrEmpty(nameof(name));
            name.GuardAgainstInvalid(Validations.IsNameIdentifier, nameof(name),
                ValidationMessages.InvalidNameIdentifier);

            Name = name;
            Id = IdGenerator.Create();
        }

        /// <summary>
        ///     For serialization
        /// </summary>
        public PatternMetaModel()
        {
        }

        public List<CodeTemplate> CodeTemplates { get; set; } = new List<CodeTemplate>();

        public List<IAutomation> Automation { get; set; } = new List<IAutomation>();

        public List<Attribute> Attributes { get; set; } = new List<Attribute>();

        public List<Element> Elements { get; set; } = new List<Element>();

        public string Name { get; set; }

        public string Id { get; set; }
    }
}