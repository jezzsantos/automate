using automate.Extensions;

namespace automate.Domain
{
    internal class PatternToolkitPackage
    {
        public PatternToolkitPackage(PatternToolkitDefinition toolkit, string buildLocation)
        {
            toolkit.GuardAgainstNull(nameof(toolkit));
            buildLocation.GuardAgainstNullOrEmpty(nameof(buildLocation));

            BuiltLocation = buildLocation;
            Toolkit = toolkit;
        }

        public PatternToolkitDefinition Toolkit { get; set; }

        public string BuiltLocation { get; }
    }
}