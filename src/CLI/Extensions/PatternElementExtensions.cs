using Automate.Authoring.Domain;
using Automate.Common.Domain;

namespace Automate.CLI.Extensions
{
    public static class PatternElementExtensions
    {
        public static bool IsLaunching(this Automation automation)
        {
            return automation.Type == AutomationType.CommandLaunchPoint;
        }
    }
}