using System;
using System.Collections.Generic;
using Automate.Common.Extensions;
using Microsoft.Extensions.Logging;

namespace Automate.Common
{
    public class Recorder : IRecorder, IDisposable
    {
        private readonly ILogger logger;
        private readonly ICrashReporter crasher;
        private readonly IMetricReporter measurer;

        public Recorder(ILogger logger, ICrashReporter crasher, IMetricReporter measurer)
        {
            logger.GuardAgainstNull(nameof(logger));
            crasher.GuardAgainstNull(nameof(crasher));
            measurer.GuardAgainstNull(nameof(measurer));
            this.logger = logger;
            this.crasher = crasher;
            this.measurer = measurer;
        }

        public void Measure(string eventName, Dictionary<string, string> context = null)
        {
            this.logger.Log(LogLevel.Information, "Measured event: {Event}", eventName);
            this.measurer.Measure(eventName, context);
        }

        public void Crash(CrashLevel level, Exception exception, string messageTemplate, params object[] args)
        {
            this.logger.Log(LogLevel.Error, exception, $"Crashed: {messageTemplate}", args);
            this.crasher.Crash(level, exception, messageTemplate, args);
        }

        public void Trace(LogLevel level, string messageTemplate, params object[] args)
        {
            this.logger.Log(level, messageTemplate, args);
        }

        public void Dispose()
        {
            (this.crasher as IDisposable)?.Dispose();
            (this.measurer as IDisposable)?.Dispose();
        }
    }
}