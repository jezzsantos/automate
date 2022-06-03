using System;
using Automate.CLI.Domain;
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
            private readonly Mock<IApplicationExecutor> applicationExecutor;
            private readonly Mock<IDraftPathResolver> draftPathResolver;

            public GivenACommand()
            {
                this.draftPathResolver = new Mock<IDraftPathResolver>();
                this.draftPathResolver
                    .Setup(spr =>
                        spr.ResolveExpression(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DraftItem>()))
                    .Returns((string _, string expr, DraftItem _) => expr);
                this.applicationExecutor = new Mock<IApplicationExecutor>();
            }

            [Fact]
            public void WhenExecuteAndApplicationFails_ThenReturnsFailure()
            {
                var toolkit = new ToolkitDefinition(new PatternDefinition("apatternname"));
                var target = new DraftItem(toolkit, new Element("anelementname"), null);
                var draft = new DraftDefinition(toolkit);
                this.applicationExecutor.Setup(ae =>
                        ae.RunApplicationProcess(It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>()))
                    .Returns(ApplicationExecutionProcessResult.Failure("amessage"));

                var command = new CliCommand("acommandname", "anapplicationname", "arguments",
                    this.draftPathResolver.Object, this.applicationExecutor.Object);
                var result = command.Execute(draft, target);

                result.IsSuccess.Should().BeFalse();
                result.Log.Should()
                    .Contain("amessage");
                this.applicationExecutor.Verify(ae => ae.RunApplicationProcess(true, "anapplicationname", "arguments"));
            }

            [Fact]
            public void WhenExecuteAndApplicationSucceeds_ThenReturnsSuccess()
            {
                var toolkit = new ToolkitDefinition(new PatternDefinition("apatternname"));
                var target = new DraftItem(toolkit, new Element("anelementname"), null);
                var draft = new DraftDefinition(toolkit);
                this.applicationExecutor.Setup(ae =>
                        ae.RunApplicationProcess(It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>()))
                    .Returns(ApplicationExecutionProcessResult.Success("amessage"));

                var command = new CliCommand("acommandname", "anapplicationname", "arguments",
                    this.draftPathResolver.Object, this.applicationExecutor.Object);
                var result = command.Execute(draft, target);

                result.IsSuccess.Should().BeTrue();
                result.Log.Should()
                    .Contain("amessage");
                this.applicationExecutor.Verify(ae => ae.RunApplicationProcess(true, "anapplicationname", "arguments"));
            }
        }
    }
}