using Automate.Common;
using Automate.Common.Extensions;
using Microsoft.Extensions.Logging;

namespace Automate.CLI.Infrastructure.Recording
{
    public class LoggingSessionReporter : ISessionReporter
    {
        private readonly ILogger logger;

        public LoggingSessionReporter(ILogger logger)
        {
            logger.GuardAgainstNull(nameof(logger));
            this.logger = logger;
        }

        public void EnableReporting(string machineId, string correlationId)
        {
        }

        public void MeasureStartSession(string messageTemplate, params object[] args)
        {
            this.logger.Log(LogLevel.Information, null, messageTemplate, args);
        }

        public void MeasureEndSession(bool success, string messageTemplate, params object[] args)
        {
            this.logger.Log(LogLevel.Information, null, messageTemplate, args);
        }
    }
}