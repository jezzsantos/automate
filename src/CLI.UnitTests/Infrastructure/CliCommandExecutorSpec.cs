﻿using Automate.Authoring.Domain;
using Automate.CLI.Infrastructure;
using Automate.Runtime.Application;
using Automate.Runtime.Domain;
using FluentAssertions;
using Moq;
using Xunit;

namespace CLI.UnitTests.Infrastructure
{
    public class CliCommandExecutorSpec
    {
        [Trait("Category", "Unit")]
        public class GivenACommand
        {
            private readonly Mock<IApplicationExecutor> applicationExecutor;
            private readonly CliCommand command;
            private readonly CliCommandExecutor executor;

            public GivenACommand()
            {
                var draftPathResolver = new Mock<IDraftPathResolver>();
                draftPathResolver
                    .Setup(spr =>
                        spr.ResolveExpression(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DraftItem>()))
                    .Returns((string _, string expr, DraftItem _) => expr);
                this.applicationExecutor = new Mock<IApplicationExecutor>();

                this.command = new CliCommand("acommandname", "anapplicationname", "arguments");
                this.executor = new CliCommandExecutor(draftPathResolver.Object, this.applicationExecutor.Object);
            }

            [Fact]
            public void WhenExecuteAndApplicationFails_ThenReturnsFailure()
            {
                var pattern = new PatternDefinition("apatternname");
                var toolkit = new ToolkitDefinition(pattern);
                var element = new Element("anelementname");
                pattern.AddElement(element);
                var target = new DraftItem(toolkit, element);
                var draft = new DraftDefinition(toolkit);
                this.applicationExecutor.Setup(ae =>
                        ae.RunApplicationProcess(It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>()))
                    .Returns(ApplicationExecutionProcessResult.Failure("amessage"));
                var executionResult = new CommandExecutionResult("acommandname",
                    new CommandExecutableContext(this.command, draft, target));

                this.executor.Execute(this.command, executionResult);

                executionResult.IsSuccess.Should().BeFalse();
                executionResult.Log.Should()
                    .Contain(x => x.Type == CommandExecutionLogItemType.Failed && x.Message == "amessage");
                this.applicationExecutor.Verify(ae => ae.RunApplicationProcess(true, "anapplicationname", "arguments"));
            }

            [Fact]
            public void WhenExecuteAndApplicationSucceeds_ThenReturnsSuccess()
            {
                var pattern = new PatternDefinition("apatternname");
                var toolkit = new ToolkitDefinition(pattern);
                var element = new Element("anelementname");
                pattern.AddElement(element);
                var target = new DraftItem(toolkit, element);
                var draft = new DraftDefinition(toolkit);
                this.applicationExecutor.Setup(ae =>
                        ae.RunApplicationProcess(It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>()))
                    .Returns(ApplicationExecutionProcessResult.Success("amessage"));
                var executionResult = new CommandExecutionResult("acommandname",
                    new CommandExecutableContext(this.command, draft, target));

                this.executor.Execute(this.command, executionResult);

                executionResult.IsSuccess.Should().BeTrue();
                executionResult.Log.Should()
                    .Contain(x => x.Type == CommandExecutionLogItemType.Succeeded && x.Message == "amessage");
                this.applicationExecutor.Verify(ae => ae.RunApplicationProcess(true, "anapplicationname", "arguments"));
            }
        }
    }
}