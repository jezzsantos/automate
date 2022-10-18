using System;
using System.Collections.Generic;
using Automate.Common.Extensions;
using Microsoft.Extensions.Logging;

namespace Automate.Common
{
    public class Recorder : IRecorder, IDisposable
    {
        private readonly ICrashReporter crasher;
        private readonly ILogger logger;
        private readonly IMetricReporter measurer;
        private string userId;

        public Recorder(ILogger logger, ICrashReporter crasher, IMetricReporter measurer)
        {
            logger.GuardAgainstNull(nameof(logger));
            crasher.GuardAgainstNull(nameof(crasher));
            measurer.GuardAgainstNull(nameof(measurer));
            this.logger = logger;
            this.crasher = crasher;
            this.measurer = measurer;
        }

        public void Dispose()
        {
            (this.crasher as IDisposable)?.Dispose();
            (this.measurer as IDisposable)?.Dispose();
        }

        public void Count(string eventName, Dictionary<string, string> context = null)
        {
            Trace(LogLevel.Information, "Measured event: {Event}", eventName);
            this.measurer.Count(eventName, context);
        }

        public void DisableUsageCollection()
        {
            this.measurer.DisableUsageCollection();
            this.crasher.DisableUsageCollection();
        }

        public void SetUserId(string id)
        {
            this.userId = id;
            this.measurer.SetUserId(id);
            this.crasher.SetUserId(id);
        }

        public string GetUserId()
        {
            return this.userId;
        }

        public void Crash(CrashLevel level, Exception exception, string messageTemplate, params object[] args)
        {
            if (this.logger.IsEnabled(LogLevel.Error))
            {
                this.logger.Log(LogLevel.Error, exception, $"Crashed: {messageTemplate}", args);
            }
            this.crasher.Crash(level, exception, messageTemplate, args);
        }

        public void Trace(LogLevel level, string messageTemplate, params object[] args)
        {
            if (this.logger.IsEnabled(level))
            {
                this.logger.Log(level, messageTemplate, args);
            }
        }
    }
}