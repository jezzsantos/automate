using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Automate.Common.Extensions
{
    public static class RecorderExtensions
    {
        public static void TraceDebug(this IRecorder recorder, string messageTemplate, params object[] args)
        {
            recorder.Trace(LogLevel.Debug, messageTemplate, args);
        }

        public static void TraceInformation(this IRecorder recorder, string messageTemplate, params object[] args)
        {
            recorder.Trace(LogLevel.Information, messageTemplate, args);
        }

        public static void TraceWarning(this IRecorder recorder, string messageTemplate, params object[] args)
        {
            recorder.Trace(LogLevel.Warning, messageTemplate, args);
        }

        public static void TraceError(this IRecorder recorder, string messageTemplate, params object[] args)
        {
            recorder.Trace(LogLevel.Error, messageTemplate, args);
        }

        public static void CrashFatal(this IRecorder recorder, Exception exception)
        {
            recorder.Crash(CrashLevel.Fatal, exception, null);
        }

        public static void CrashFatal(this IRecorder recorder, Exception exception, string message)
        {
            recorder.Crash(CrashLevel.Fatal, exception, message, null);
        }

        public static void CrashFatal(this IRecorder recorder, Exception exception, string messageTemplate,
            params object[] args)
        {
            recorder.Crash(CrashLevel.Fatal, exception, messageTemplate, args);
        }

        public static void CrashRecoverable(this IRecorder recorder, Exception exception, string messageTemplate,
            params object[] args)
        {
            recorder.Crash(CrashLevel.Recoverable, exception, messageTemplate, args);
        }

        public static void Measure(this IRecorder recorder, string eventName)
        {
            recorder.Measure(eventName, new Dictionary<string, string>());
        }
    }
}