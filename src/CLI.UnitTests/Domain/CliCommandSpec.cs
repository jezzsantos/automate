using System;
using System.IO;
using Automate.CLI.Domain;
using Automate.CLI.Extensions;
using FluentAssertions;
using Moq;
using Xunit;

namespace CLI.UnitTests.Domain
{
    public class CliCommandSpec
    {
        [Trait("Category", "Unit")]
        public class GivenAnyCommand
        {
            [Fact]
            public void WhenConstructedAndApplicationNameIsMissing_ThenThrows()
            {
                FluentActions.Invoking(() => new CliCommand("aname", null, "anargument"))
                    .Should().Throw<ArgumentNullException>();
            }
        }

        [Trait("Category", "Unit")]
        public class GivenACommand
        {
            private readonly Mock<ISolutionPathResolver> solutionPathResolver;
            private readonly string testApplicationName;

            public GivenACommand()
            {
                this.solutionPathResolver = new Mock<ISolutionPathResolver>();
                this.solutionPathResolver
                    .Setup(spr => spr.ResolveExpression(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SolutionItem>()))
                    .Returns((string _, string expr, SolutionItem _) => expr);
                this.testApplicationName = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "../../../../../tools/TestApp/TestApp.exe"));
            }

            [Fact]
            public void WhenExecuteAndApplicationNotFound_ThenReturnsFailure()
            {
                var toolkit = new ToolkitDefinition(new PatternDefinition("apatternname"));
                var target = new SolutionItem(toolkit, new Element("anelementname"), null);
                var solution = new SolutionDefinition(toolkit);

                var command = new CliCommand("acommandname", "anapplicationname", null, this.solutionPathResolver.Object);
                var result = command.Execute(solution, target);

                result.IsSuccess.Should().BeFalse();
                result.Log.Should()
                    .Contain(DomainMessages.CliCommand_Log_ExecutionFailed.Format("anapplicationname", null,
                        "The system cannot find the file specified."));
            }

            [Fact]
            public void WhenExecuteAndApplicationFailsWithError_ThenReturnsFailure()
            {
                var toolkit = new ToolkitDefinition(new PatternDefinition("apatternname"));
                var target = new SolutionItem(toolkit, new Element("anelementname"), null);
                var solution = new SolutionDefinition(toolkit);

                var command = new CliCommand("acommandname", this.testApplicationName, "--fails", this.solutionPathResolver.Object);
                var result = command.Execute(solution, target);

                result.IsSuccess.Should().BeFalse();
                result.Log.Should()
                    .Contain(DomainMessages.CliCommand_Log_ExecutionFailed.Format(this.testApplicationName, "--fails",
                        $"Failed{Environment.NewLine}"));
            }

            [Fact(Skip = "Takes 5 seconds to complete")]
            public void WhenExecuteAndApplicationHangs_ThenReturnsFailure()
            {
                var toolkit = new ToolkitDefinition(new PatternDefinition("apatternname"));
                var target = new SolutionItem(toolkit, new Element("anelementname"), null);
                var solution = new SolutionDefinition(toolkit);

                var command = new CliCommand("acommandname", this.testApplicationName, "--hangs", this.solutionPathResolver.Object);
                var result = command.Execute(solution, target);

                result.IsSuccess.Should().BeFalse();
                result.Log.Should()
                    .Contain(DomainMessages.CliCommand_Log_ExecutionFailed.Format(this.testApplicationName, "--hangs",
                        DomainMessages.CliCommand_Log_Hung.Format(CliCommand.HangTime.TotalSeconds)));
            }

            [Fact]
            public void WhenExecuteAndSucceeds_ThenReturnsSuccess()
            {
                var toolkit = new ToolkitDefinition(new PatternDefinition("apatternname"));
                var target = new SolutionItem(toolkit, new Element("anelementname"), null);
                var solution = new SolutionDefinition(toolkit);

                var command = new CliCommand("acommandname", this.testApplicationName, "--succeeds", this.solutionPathResolver.Object);
                var result = command.Execute(solution, target);

                result.IsSuccess.Should().BeTrue();
                result.Log.Should()
                    .Contain(DomainMessages.CliCommand_Log_ExecutionSucceeded.Format(this.testApplicationName, "--succeeds",
                        $"Success{Environment.NewLine}"));
            }
        }
    }
}