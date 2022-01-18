using automate.Extensions;

namespace automate
{
    internal class AutomationCommand : Automation
    {
        public AutomationCommand(string name, bool isTearOff, string filePath) : base(name)
        {
            filePath.GuardAgainstNullOrEmpty(nameof(filePath));
            filePath.GuardAgainstInvalid(Validations.IsRuntimeFilePath, nameof(filePath),
                ValidationMessages.Automation_InvalidFilePath);

            IsTearOff = isTearOff;
            FilePath = filePath;
        }

        /// <summary>
        ///     For serialization
        /// </summary>
        public AutomationCommand()
        {
        }

        public string FilePath { get; set; }

        public bool IsTearOff { get; set; }
    }
}