namespace Automate.CLI.Domain
{
    internal interface IApplicationExecutor
    {
        ApplicationExecutionProcessResult
            RunApplicationProcess(bool awaitForExit, string applicationName, string arguments);
    }

    internal class ApplicationExecutionProcessResult
    {
        public ApplicationExecutionProcessResult()
        {
            IsSuccess = false;
            Output = null;
            Error = null;
        }

        public string Output { get; private set; }

        public string Error { get; private set; }

        public bool IsSuccess { get; private set; }

        public void Fails(string message)
        {
            IsSuccess = false;
            Output = null;
            Error = message;
        }

        public void Succeeds(string message)
        {
            IsSuccess = true;
            Output = message;
            Error = null;
        }

        public static ApplicationExecutionProcessResult Failure(string message)
        {
            var result = new ApplicationExecutionProcessResult();
            result.Fails(message);
            return result;
        }

        public static ApplicationExecutionProcessResult Success(string message)
        {
            var result = new ApplicationExecutionProcessResult();
            result.Succeeds(message);
            return result;
        }
    }
}