using System;
using Automate.Common.Extensions;
using Microsoft.Extensions.Logging;

namespace Automate.Common
{
    public class DotNetCoreLogger : ILogger
    {
        private readonly ILogger logger;

        public DotNetCoreLogger(ILoggerFactory loggerFactory, string categoryName)
        {
            loggerFactory.GuardAgainstNull(nameof(loggerFactory));
            categoryName.GuardAgainstNullOrEmpty(nameof(categoryName));
            this.logger = loggerFactory.CreateLogger(categoryName);
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception,
            Func<TState, Exception, string> formatter)
        {
            this.logger.Log(logLevel, eventId, state, exception, formatter);
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return this.logger.IsEnabled(logLevel);
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return this.logger.BeginScope(state);
        }
    }
}