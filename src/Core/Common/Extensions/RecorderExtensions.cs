using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
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

        public static void Count(this IRecorder recorder, string eventName)
        {
            recorder.Count(eventName, new Dictionary<string, string>());
        }

        public static string AnonymiseMeasure(this string identifier)
        {
            return Encoding.UTF8.GetString(MD5.HashData(Encoding.UTF8.GetBytes(identifier)));
        }
    }
}