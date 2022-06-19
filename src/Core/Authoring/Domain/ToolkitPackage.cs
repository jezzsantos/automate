using Automate.Common;
using Automate.Common.Extensions;

namespace Automate.Authoring.Domain
{
    public class ToolkitPackage
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

    public class VersionInstruction
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