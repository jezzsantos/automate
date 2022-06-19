using Automate.Common.Domain;

namespace Automate.Authoring.Domain
{
    internal interface ICollectionAutoNamer
    {
        string GetNextAutomationName(AutomationType type, string name, IPatternElement element);

        string GetNextCodeTemplateName(string name, IPatternElement element);
    }
}