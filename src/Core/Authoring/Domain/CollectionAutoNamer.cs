using System;
using System.Linq;
using Automate.Common.Domain;
using Automate.Common.Extensions;

namespace Automate.Authoring.Domain
{
    internal class CollectionAutoNamer : ICollectionAutoNamer
    {
        public string GetNextAutomationName(AutomationType type, string name, IPatternElement element)
        {
            if (name.HasValue())
            {
                return name;
            }

            return FindNextName(type.ToString(), element.Automation.Count,
                nextName => element.Automation.Any(automation => automation.Name.EqualsIgnoreCase(nextName)));
        }

        public string GetNextCodeTemplateName(string name, IPatternElement element)
        {
            if (name.HasValue())
            {
                return name;
            }

            return FindNextName(nameof(CodeTemplate), element.CodeTemplates.Count,
                nextName => element.CodeTemplates.Any(template => template.Name.EqualsIgnoreCase(nextName)));
        }

        private static string FindNextName(string name, int collectionItems, Func<string, bool> predicate)
        {
            var numberOfAutomation = collectionItems;
            var autoName = GetNextElementName(name, numberOfAutomation);
            while (predicate(autoName))
            {
                autoName = GetNextElementName(name, ++numberOfAutomation);
            }

            return autoName;
        }

        private static string GetNextElementName(string elementTypeName, int numberOfElementTypes)
        {
            return $"{elementTypeName}{numberOfElementTypes + 1}";
        }
    }
}