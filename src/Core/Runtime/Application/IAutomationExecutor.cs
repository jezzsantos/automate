using Automate.Authoring.Domain;

namespace Automate.Runtime.Application
{
    public interface IAutomationExecutor
    {
        void Execute(CommandExecutionResult result);
    }
}