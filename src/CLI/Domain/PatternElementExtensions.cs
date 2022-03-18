using Automate.CLI.Extensions;

namespace Automate.CLI.Domain
{
    internal static class PatternElementExtensions
    {
        public static void AddAttributes(this PatternElement element, params Attribute[] attributes)
        {
            attributes.ToListSafe().ForEach(element.AddAttribute);
        }

        public static bool IsLaunchable(this Automation automation)
        {
            return automation.Type != AutomationType.Unknown && automation.Type != AutomationType.TestingOnly && automation.Type != AutomationType.CommandLaunchPoint;
        }
    }
}