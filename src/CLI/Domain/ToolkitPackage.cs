using Automate.CLI.Extensions;

namespace Automate.CLI.Domain
{
    internal class ToolkitPackage
    {
        public ToolkitPackage(ToolkitDefinition toolkit, string buildLocation)
        {
            toolkit.GuardAgainstNull(nameof(toolkit));
            buildLocation.GuardAgainstNullOrEmpty(nameof(buildLocation));

            BuiltLocation = buildLocation;
            Toolkit = toolkit;
        }

        public ToolkitDefinition Toolkit { get; }

        public string BuiltLocation { get; }
    }
}