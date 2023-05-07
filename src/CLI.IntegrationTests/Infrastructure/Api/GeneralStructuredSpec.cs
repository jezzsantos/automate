using System;
using System.Collections.Generic;
using Automate.CLI.Infrastructure;
using Automate.CLI.Infrastructure.Api;
using Automate.Common.Extensions;
using FluentAssertions;
using Xunit;

namespace CLI.IntegrationTests.Infrastructure.Api
{
    [Trait("Category", "Integration")] [Collection("CLI")]
    public class GeneralStructuredSpec : IDisposable
    {
        private readonly CliTestSetup setup;

        public GeneralStructuredSpec(CliTestSetup setup)
        {
            this.setup = setup;
            this.setup.ResetRepository();
            RuntimeSpec.DeleteOutputFolders();
        }

        [Fact]
        public void WhenInfoAndNotCollectingUsage_ThenWontMeasure()
        {
            this.setup.RunCommand($"{CommandLineApi.InfoCommandName} --collect-usage:false --output-structured");

            var metadata = new CliRuntimeMetadata();
            var info = new StructuredMessage
            {
                Message = OutputMessages.CommandLine_Output_Info,
                Values = new Dictionary<string, object>
                {
                    { "Command", metadata.ProductName },
                    { "Location", metadata.InstallationPath },
                    { "RuntimeVersion", metadata.RuntimeVersion.ToString() },
                    {
                        "CollectUsage", new Dictionary<string, object>
                        {
                            { "IsEnabled", false }
                        }
                    }
                }
            };

            var structuredOutput = new StructuredOutput
            {
                Info = new List<string>(),
                Output = new List<StructuredMessage>
                {
                    info
                }
            }.ToJson();
            this.setup.Should().DisplayNoError();
            this.setup.Should().DisplayOutput(structuredOutput);
            this.setup.Recordings.IsReportingEnabled.Should().BeFalse();
        }

        [Fact]
        public void WhenInfoAndCollectingUsage_ThenMeasuresAndReturnsInfo()
        {
            this.setup.RunCommand($"{CommandLineApi.InfoCommandName} --output-structured");

            var metadata = new CliRuntimeMetadata();
            var info = new StructuredMessage
            {
                Message = OutputMessages.CommandLine_Output_Info,
                Values = new Dictionary<string, object>
                {
                    { "Command", metadata.ProductName },
                    { "Location", metadata.InstallationPath },
                    { "RuntimeVersion", metadata.RuntimeVersion.ToString() },
                    {
                        "CollectUsage", new Dictionary<string, object>
                        {
                            { "IsEnabled", true },
                            { "MachineId", this.setup.Recordings.MachineId },
                            { "CorrelationId", this.setup.Recordings.CorrelationId }
                        }
                    }
                }
            };

            var structuredOutput = new StructuredOutput
            {
                Info = new List<string>(),
                Output = new List<StructuredMessage>
                {
                    info
                }
            }.ToJson();
            this.setup.Should().DisplayNoError();
            this.setup.Should().DisplayOutput(structuredOutput);
            this.setup.Recordings.IsReportingEnabled.Should().BeTrue();
            this.setup.Recordings.Session.Should().BeNull();
            this.setup.Recordings.Measurements.Should().ContainSingle(measurement =>
                measurement.EventName == "info" && measurement.MachineId.HasValue() &&
                measurement.CorrelationId == "acorrelationid");
        }

        [Fact]
        public void WhenInfoAndCollectingUsageWithCorrelationId_ThenMeasuresReturnsInfo()
        {
            this.setup.RunCommand(
                $"{CommandLineApi.InfoCommandName} --usage-correlation acorrelationid --output-structured");

            var metadata = new CliRuntimeMetadata();
            var info = new StructuredMessage
            {
                Message = OutputMessages.CommandLine_Output_Info,
                Values = new Dictionary<string, object>
                {
                    { "Command", metadata.ProductName },
                    { "Location", metadata.InstallationPath },
                    { "RuntimeVersion", metadata.RuntimeVersion.ToString() },
                    {
                        "CollectUsage", new Dictionary<string, object>
                        {
                            { "IsEnabled", true },
                            { "MachineId", this.setup.Recordings.MachineId },
                            { "CorrelationId", "acorrelationid" }
                        }
                    }
                }
            };

            var structuredOutput = new StructuredOutput
            {
                Info = new List<string>(),
                Output = new List<StructuredMessage>
                {
                    info
                }
            }.ToJson();
            this.setup.Should().DisplayNoError();
            this.setup.Should().DisplayOutput(structuredOutput);
            this.setup.Recordings.IsReportingEnabled.Should().BeTrue();
            this.setup.Recordings.Session.Should().BeNull();
            this.setup.Recordings.Measurements.Should().ContainSingle(measurement =>
                measurement.EventName == "info" && measurement.MachineId.HasValue() &&
                measurement.CorrelationId == "acorrelationid");
        }

        public void Dispose()
        {
            this.setup.Reset();
        }
    }
}