using Automate.Common.Domain;

namespace Automate.Authoring.Domain
{
#if TESTINGONLY
    public class TestingOnlyAutomation : IAutomation
    {
        public TestingOnlyAutomation(string name, AutomationType type)
        {
            Name = name;
            Type = type;
        }
        public string Id => "testingonly";

        public string Name { get; }

        public AutomationType Type { get; }

        public bool IsLaunchable => Type == AutomationType.TestingOnlyLaunchable;
    }
#endif
}