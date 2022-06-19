using Automate.Domain;

namespace Automate.Application
{
    public interface IAutomationExecutor
    {
        void Execute(CommandExecutionResult result);
    }
}