using Automate.CLI.Extensions;

namespace Automate.CLI.Domain
{
    internal class ToolkitPackage
    {
        public ToolkitPackage(ToolkitDefinition toolkit, string buildLocation, string message)
        {
            toolkit.GuardAgainstNull(nameof(toolkit));
            buildLocation.GuardAgainstNullOrEmpty(nameof(buildLocation));

            ExportedLocation = buildLocation;
            Toolkit = toolkit;
            Message = message;
        }

        public ToolkitDefinition Toolkit { get; }

        public string ExportedLocation { get; }

        public string Message { get; }
    }

    internal class VersionInstruction
    {
        public VersionInstruction(string instruction = null, bool force = false)
        {
            instruction.GuardAgainstInvalid(_ => Validations.IsVersionInstruction(instruction), nameof(instruction),
                ExceptionMessages.VersionInstruction_InvalidVersionInstruction);

            Instruction = instruction;
            Force = force;
        }

        public string Instruction { get; }

        public bool Force { get; }
    }
}