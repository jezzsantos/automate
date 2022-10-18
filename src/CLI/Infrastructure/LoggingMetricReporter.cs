using System.Collections.Generic;
using Automate.Common;
using Automate.Common.Extensions;
using Microsoft.Extensions.Logging;

namespace Automate.CLI.Infrastructure
{
    public class LoggingMetricReporter : IMetricReporter
    {
        private readonly ILogger logger;

        public LoggingMetricReporter(ILogger logger)
        {
            logger.GuardAgainstNull(nameof(logger));
            this.logger = logger;
        }

        public void Measure(string eventName, Dictionary<string, string> context = null)
        {
            this.logger.Log(LogLevel.Information, "Measured event: '{EventName}', with context: {Context}", eventName,
                context.ToJson());
        }
    }
}