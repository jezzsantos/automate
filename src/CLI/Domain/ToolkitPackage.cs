using Automate.CLI.Extensions;

namespace Automate.CLI.Domain
{
    internal class ToolkitPackage
    {
        public ToolkitPackage(ToolkitDefinition toolkit, string buildLocation, string message)
        {
            toolkit.GuardAgainstNull(nameof(toolkit));
            buildLocation.GuardAgainstNullOrEmpty(nameof(buildLocation));

            BuiltLocation = buildLocation;
            Toolkit = toolkit;
            Message = message;
        }

        public ToolkitDefinition Toolkit { get; }

        public string BuiltLocation { get; }

        public string Message { get; }
    }
}