using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Automate.Common;
using Automate.Common.Extensions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Core.UnitTests.Common
{
    [Trait("Category", "Unit")]
    public class RecorderSpec
    {
        private readonly Mock<ICrashReporter> crasher;
        private readonly Mock<ILogger> logger;
        private readonly Mock<IMetricReporter> measurer;
        private readonly Recorder recorder;

        public RecorderSpec()
        {
            this.logger = new Mock<ILogger>();
            this.crasher = new Mock<ICrashReporter>();
            this.measurer = new Mock<IMetricReporter>();
            this.recorder = new Recorder(this.logger.Object, this.crasher.Object, this.measurer.Object);
        }

        [Fact]
        public void WhenConstructed_ThenAssigned()
        {
            this.recorder.GetReportingIds().MachineId.Should().BeNull();
            this.recorder.GetReportingIds().SessionId.Should().BeNull();
        }

        [Fact]
        public void WhenConstructed_ThenDoesTrace()
        {
            this.logger.Setup(l => l.IsEnabled(It.IsAny<LogLevel>()))
                .Returns(true);

            this.recorder.Trace(LogLevel.Information, "amessagetemplate");

            VerifyLogWasCalled(this.logger, LogLevel.Information, null, "amessagetemplate");
        }

        [Fact]
        public void WhenConstructedAndCount_ThenDoesNotCountButTraces()
        {
            this.logger.Setup(l => l.IsEnabled(It.IsAny<LogLevel>()))
                .Returns(true);

            this.recorder.Count("aneventname");

            this.measurer.Verify(m => m.Count(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()), Times.Never);
            VerifyLogWasCalled(this.logger, LogLevel.Information, null, LoggingMessages.Recorder_Measure,
                "cli_aneventname");
        }

        [Fact]
        public void WhenConstructedAndCrash_ThenDoesNotCrashButTraces()
        {
            this.logger.Setup(l => l.IsEnabled(It.IsAny<LogLevel>()))
                .Returns(true);
            var exception = new Exception("amessage");

            this.recorder.Crash(CrashLevel.Fatal, exception, "amessagetemplate");

            this.crasher.Verify(c => c.Crash(It.IsAny<CrashLevel>(), It.IsAny<Exception>(), It.IsAny<string>()),
                Times.Never);
            VerifyLogWasCalled(this.logger, LogLevel.Error, exception,
                LoggingMessages.Recorder_Crash.Substitute("amessagetemplate"), null);
        }

        [Fact]
        public void WhenTraceAndLevelNotEnabled_ThenDontTrace()
        {
            this.logger.Setup(l => l.IsEnabled(It.IsAny<LogLevel>()))
                .Returns(false);

            this.recorder.Trace(LogLevel.Critical, "amessagetemplate");

            VerifyLogWasNeverCalled(this.logger);
        }

        [Fact]
        public void WhenTraceAndLevelEnabled_ThenTrace()
        {
            this.logger.Setup(l => l.IsEnabled(It.IsAny<LogLevel>()))
                .Returns(true);

            this.recorder.Trace(LogLevel.Critical, "amessagetemplate");

            VerifyLogWasCalled(this.logger, LogLevel.Critical, null, "amessagetemplate");
        }

        [Fact]
        public void WhenCountAndReportingEnabled_ThenReportsAndTraces()
        {
            this.logger.Setup(l => l.IsEnabled(It.IsAny<LogLevel>()))
                .Returns(true);
            this.recorder.EnableReporting("amachineid", "asessionid");

            this.recorder.Count("AN Event Name");

            this.measurer.Verify(m => m.Count("cli_aneventname", null));
            VerifyLogWasCalled(this.logger, LogLevel.Information, null, LoggingMessages.Recorder_Measure,
                "cli_aneventname");
        }

        [Fact]
        public void WhenCrashAndReportingEnabled_ThenReportsAndTraces()
        {
            this.logger.Setup(l => l.IsEnabled(It.IsAny<LogLevel>()))
                .Returns(true);
            this.recorder.EnableReporting("amachineid", "asessionid");
            var exception = new Exception("amessage");

            this.recorder.Crash(CrashLevel.Fatal, exception, "amessagetemplate");

            this.crasher.Verify(m => m.Crash(CrashLevel.Fatal, exception, "amessagetemplate", Array.Empty<object>()));
            VerifyLogWasCalled(this.logger, LogLevel.Error, exception,
                LoggingMessages.Recorder_Crash.Substitute("amessagetemplate"), null);
        }

        private static void VerifyLogWasCalled(Mock<ILogger> logger, LogLevel level, Exception exception,
            string expectedMessage, params object[] args)
        {
            const string valueKey = "{OriginalFormat}";
            Func<object, Type, bool> state = (v, _) => CompareMessageTemplateAndArgs(v);

            logger.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == level),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => state(v, t)),
                    It.Is<Exception>(ex => ex == exception),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)));

            //This method emulates what Microsoft.Extensions.Logging.FormattedLogValues does, since that type is not accessible, nor mackable
            bool CompareMessageTemplateAndArgs(object v)
            {
                var value = v.ToJson(false);
                string expectedValue;
                if (args.IsNull() || args.Length == 0)
                {
                    expectedValue = new Dictionary<string, string>
                        {
                            { valueKey, expectedMessage }
                        }
                        .ToList()
                        .ToJson(false);
                }
                else
                {
                    var tokens = Regex.Matches(expectedMessage, @"\{(.+?)\}");
                    var paramIndex = 0;
                    var replacements = tokens
                        .ToDictionary(token => token.Value.TrimStart('{').TrimEnd('}'), _ =>
                        {
                            paramIndex++;
                            return args.Length >= paramIndex
                                ? args[paramIndex - 1]
                                : null;
                        })
                        .Where(pair => pair.Value.Exists())
                        .ToDictionary(pair => pair.Key, pair => pair.Value);

                    replacements.Add(valueKey, expectedMessage);
                    expectedValue = replacements
                        .ToList()
                        .ToJson(false);
                }

                return value.EqualsIgnoreCase(expectedValue);
            }
        }

        private static void VerifyLogWasNeverCalled(Mock<ILogger> logger)
        {
            logger.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Never);
        }
    }
}