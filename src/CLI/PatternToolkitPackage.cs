using automate.Extensions;

namespace automate
{
    internal class PatternToolkitPackage
    {
        public PatternToolkitPackage(PatternToolkit toolkit, string buildLocation)
        {
            toolkit.GuardAgainstNull(nameof(toolkit));
            buildLocation.GuardAgainstNullOrEmpty(nameof(buildLocation));

            BuiltLocation = buildLocation;
            Toolkit = toolkit;
        }

        public PatternToolkit Toolkit { get; set; }

        public string BuiltLocation { get; }
    }
}