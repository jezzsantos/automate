using System;
using System.Collections.Generic;
using Automate.CLI.Infrastructure;
using Automate.Common.Extensions;
using FluentAssertions;
using Xunit;

namespace CLI.IntegrationTests
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

            var metadata = new CliAssemblyMetadata();
            var info = new StructuredMessage
            {
                Message = OutputMessages.CommandLine_Output_Info,
                Values = new Dictionary<string, object>
                {
                    { "Command", metadata.ProductName },
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
            this.setup.Recordings.IsUsageCollectionEnabled.Should().BeFalse();
        }

        [Fact]
        public void WhenInfoAndCollectingUsage_ThenMeasuresWithUserId()
        {
            this.setup.RunCommand($"{CommandLineApi.InfoCommandName} --output-structured");

            var metadata = new CliAssemblyMetadata();
            var info = new StructuredMessage
            {
                Message = OutputMessages.CommandLine_Output_Info,
                Values = new Dictionary<string, object>
                {
                    { "Command", metadata.ProductName },
                    { "RuntimeVersion", metadata.RuntimeVersion.ToString() },
                    {
                        "CollectUsage", new Dictionary<string, object>
                        {
                            { "IsEnabled", true },
                            { "UserId", this.setup.Recordings.UserId }
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
            this.setup.Recordings.IsUsageCollectionEnabled.Should().BeTrue();
            this.setup.Recordings.Measurements.Should().ContainSingle(measurement =>
                measurement.EventName == "use" && measurement.UserId.HasValue());
        }

        public void Dispose()
        {
            this.setup.Reset();
        }
    }
}