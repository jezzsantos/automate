using System;
using System.Collections.Generic;
using automate.Domain;
using automate.Extensions;
using FluentAssertions;
using Moq;
using Xunit;

namespace CLI.UnitTests.Domain
{
    [Trait("Category", "Unit")]
    public class CommandLaunchPointSpec
    {
        [Fact]
        public void WhenConstructedAndNameIsMissing_ThenThrows()
        {
            FluentActions.Invoking(() => new CommandLaunchPoint(null, new List<string> { "acmdid" }))
                .Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void WhenConstructedAndNameIsInvalid_ThenThrows()
        {
            FluentActions.Invoking(() => new CommandLaunchPoint("^aninvalidname^", new List<string> { "acmdid" }))
                .Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage(ValidationMessages.InvalidNameIdentifier.Format("^aninvalidname^") + "*");
        }

        [Fact]
        public void WhenConstructedAndCommandIdsMissing_ThenThrows()
        {
            FluentActions.Invoking(() => new CommandLaunchPoint("aname", null))
                .Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void WhenConstructedAndCommandIdsEmpty_ThenThrows()
        {
            FluentActions.Invoking(() => new CommandLaunchPoint("aname", new List<string>()))
                .Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage(ValidationMessages.Automation_EmptyCommandIds + "*");
        }

        [Fact]
        public void WhenConstructedAndCommandIdsInvalid_ThenThrows()
        {
            var cmdIds = new List<string> { IdGenerator.Create(), "aninvalidcmdid", IdGenerator.Create() };
            FluentActions.Invoking(() => new CommandLaunchPoint("aname", cmdIds))
                .Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage(ValidationMessages.Automation_InvalidCommandIds.Format(cmdIds.Join(", ")) +
                             "*");
        }

        [Fact]
        public void WhenExecute_ThenExecutesAutomation()
        {
            var commandId = IdGenerator.Create();
            var launchPoint =
                new CommandLaunchPoint("alaunchpointname", new List<string> { commandId });
            var ownerSolution = new SolutionItem();
            var automation = new Mock<IAutomation>();
            automation.Setup(aut => aut.Id)
                .Returns(commandId);
            automation.Setup(aut => aut.Execute(It.IsAny<ToolkitDefinition>(), It.IsAny<SolutionItem>()))
                .Returns(new CommandExecutionResult("anautomationname", new List<string> { "alogentry" }));
            var pattern = new PatternDefinition("apatternname")
            {
                Automation = new List<IAutomation>
                {
                    automation.Object
                }
            };
            var toolkit = new ToolkitDefinition(pattern, "1.0");

            var result = launchPoint.Execute(toolkit, ownerSolution);

            result.CommandName.Should().Be("alaunchpointname");
            result.Log.Should().ContainSingle("alogentry");
            automation.Verify(aut => aut.Execute(toolkit, ownerSolution));
        }
    }
}