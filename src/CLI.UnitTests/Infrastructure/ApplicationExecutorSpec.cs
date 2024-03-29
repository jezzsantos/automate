﻿using System;
using System.IO;
using Automate.CLI.Infrastructure;
using Automate.Common.Extensions;
using FluentAssertions;
using Xunit;

namespace CLI.UnitTests.Infrastructure
{
    [Trait("Category", "Unit.NOCI")]
    public class ApplicationExecutorSpec
    {
        private readonly ApplicationExecutor executor;
        private readonly string testApplicationName;

        public ApplicationExecutorSpec()
        {
            this.executor = new ApplicationExecutor(new CliRuntimeMetadata());
            this.testApplicationName = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory,
                "../../../../../tools/TestApp/TestApp.exe"));
        }

        [Fact]
        public void WhenRunApplicationProcessAndNotFound_ThenReturnsFailure()
        {
            const string unknownApplicationName = "anapplicationname";

            var result = this.executor.RunApplicationProcess(true, unknownApplicationName, null);

            result.IsSuccess.Should().BeFalse();

            result.Error.Should()
                .Contain(InfrastructureMessages.ApplicationExecutor_ExecutionFailed.Substitute(unknownApplicationName,
                    null,
                    $"An error occurred trying to start process '{unknownApplicationName}' with working directory '{Environment.CurrentDirectory}'." +
                    " The system cannot find the file specified."));
        }

        [Fact]
        public void WhenRunApplicationProcessAndFails_ThenReturnsFailure()
        {
            var result = this.executor.RunApplicationProcess(true, this.testApplicationName, "--fails");

            result.IsSuccess.Should().BeFalse();

            result.Error.Should()
                .Contain(InfrastructureMessages.ApplicationExecutor_ExecutionFailed.Substitute(this.testApplicationName,
                    "--fails",
                    $"Failed{Environment.NewLine}"));
        }

        [Fact(Skip = "Takes 5 seconds to complete")]
        public void WhenRunApplicationProcessAndApplicationHangs_ThenReturnsFailure()
        {
            var result = this.executor.RunApplicationProcess(true, this.testApplicationName, "--hangs");

            result.IsSuccess.Should().BeFalse();
            result.Error.Should()
                .Contain(InfrastructureMessages.ApplicationExecutor_ExecutionFailed.Substitute(this.testApplicationName,
                    "--hangs",
                    InfrastructureMessages.ApplicationExecutor_Hung.Substitute(
                        ApplicationExecutor.HangTime.TotalSeconds)));
        }

        [Fact]
        public void WhenRunApplicationProcessAndSucceeds_ThenReturnsFailure()
        {
            var result = this.executor.RunApplicationProcess(true, this.testApplicationName, "--succeeds");

            result.IsSuccess.Should().BeTrue();

            result.Output.Should()
                .Contain(InfrastructureMessages.ApplicationExecutor_Succeeded.Substitute(this.testApplicationName,
                    "--succeeds",
                    $"Success{Environment.NewLine}"));
        }
    }
}