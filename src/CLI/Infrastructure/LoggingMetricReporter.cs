using System.Collections.Generic;
using Automate.Common;
using Automate.Common.Extensions;
using Microsoft.Extensions.Logging;

namespace Automate.CLI.Infrastructure
{
    public class LoggingMetricReporter : IMetricReporter
    {
        private readonly ILogger logger;
        private bool usageCollectionEnabled;
        private string userId;

        public LoggingMetricReporter(ILogger logger)
        {
            logger.GuardAgainstNull(nameof(logger));
            this.logger = logger;
            this.usageCollectionEnabled = true;
        }

        public void Count(string eventName, Dictionary<string, string> context = null)
        {
            if (this.usageCollectionEnabled)
            {
                this.logger.Log(LogLevel.Information,
                    "Measured event: '{EventName}' for '{TrackingId}', with context: {Context}",
                    eventName, this.userId,
                    context.ToJson());
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