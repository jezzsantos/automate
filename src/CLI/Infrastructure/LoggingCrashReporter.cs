using System;
using Automate.Common;
using Automate.Common.Extensions;
using Microsoft.Extensions.Logging;

namespace Automate.CLI.Infrastructure
{
    public class LoggingCrashReporter : ICrashReporter
    {
        private readonly ILogger logger;

        public LoggingCrashReporter(ILogger logger)
        {
            logger.GuardAgainstNull(nameof(logger));
            this.logger = logger;
        }

        public void Crash(CrashLevel level, Exception exception, string messageTemplate, params object[] args)
        {
            this.logger.Log(LogLevel.Error, $"Crashed: with message {messageTemplate}, and exception: {exception}",
                args);
        }
    }
}