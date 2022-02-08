using System;
using System.Collections.Generic;
using automate.Domain;
using automate.Extensions;
using FluentAssertions;
using Xunit;

namespace CLI.UnitTests.Domain
{
    [Trait("Category", "Unit")]
    public class AutomationLaunchPointSpec
    {
        [Fact]
        public void WhenConstructedAndNameIsMissing_ThenThrows()
        {
            FluentActions.Invoking(() => new AutomationLaunchPoint(null, new List<string> { "acmdid" }))
                .Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void WhenConstructedAndNameIsInvalid_ThenThrows()
        {
            FluentActions.Invoking(() => new AutomationLaunchPoint("^aninvalidname^", new List<string> { "acmdid" }))
                .Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage(ValidationMessages.InvalidNameIdentifier.Format("^aninvalidname^") + "*");
        }

        [Fact]
        public void WhenConstructedAndCommandIdsMissing_ThenThrows()
        {
            FluentActions.Invoking(() => new AutomationLaunchPoint("aname", null))
                .Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void WhenConstructedAndCommandIdsEmpty_ThenThrows()
        {
            FluentActions.Invoking(() => new AutomationLaunchPoint("aname", new List<string>()))
                .Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage(ValidationMessages.Automation_EmptyCommandIds + "*");
        }

        [Fact]
        public void WhenConstructedAndCommandIdsInvalid_ThenThrows()
        {
            var cmdIds = new List<string> { IdGenerator.Create(), "aninvalidcmdid", IdGenerator.Create() };
            FluentActions.Invoking(() => new AutomationLaunchPoint("aname", cmdIds))
                .Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage(ValidationMessages.Automation_InvalidCommandIds.Format(cmdIds.Join(", ")) +
                             "*");
        }
    }
}