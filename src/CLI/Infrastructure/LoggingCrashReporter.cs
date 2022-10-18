using System;
using Automate.Common;
using Automate.Common.Extensions;
using Microsoft.Extensions.Logging;

namespace Automate.CLI.Infrastructure
{
    public class LoggingCrashReporter : ICrashReporter
    {
        private readonly ILogger logger;
        private bool usageCollectionEnabled;
        private string userId;

        public LoggingCrashReporter(ILogger logger)
        {
            logger.GuardAgainstNull(nameof(logger));
            this.logger = logger;
            this.usageCollectionEnabled = true;
        }

        public void Crash(CrashLevel level, Exception exception, string messageTemplate, params object[] args)
        {
            if (this.usageCollectionEnabled)
            {
                this.logger.Log(LogLevel.Error,
                    $"Crashed: for '{this.userId}' with message {messageTemplate}, and exception: {exception}",
                    args);
            }
        }

        public void DisableUsageCollection()
        {
            this.usageCollectionEnabled = false;
        }

        public void SetUserId(string id)
        {
            this.userId = id;
        }
    }
}