using System;
using automate.Domain;
using automate.Extensions;
using FluentAssertions;
using Xunit;

namespace CLI.UnitTests.Domain
{
    [Trait("Category", "Unit")]
    public class AutomationCommandSpec
    {
        [Fact]
        public void WhenConstructedAndNameIsMissing_ThenThrows()
        {
            FluentActions.Invoking(() => new AutomationCommand(null, false, "~/afilepath"))
                .Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void WhenConstructedAndNameIsInvalid_ThenThrows()
        {
            FluentActions.Invoking(() => new AutomationCommand("^aninvalidname^", false, "~/afilepath"))
                .Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage(ValidationMessages.InvalidNameIdentifier.Format("^aninvalidname^") + "*");
        }

        [Fact]
        public void WhenConstructedAndFilePathIsMissing_ThenThrows()
        {
            FluentActions.Invoking(() => new AutomationCommand("aname", false, null))
                .Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void WhenConstructedAndFilePathIsInvalid_ThenThrows()
        {
            FluentActions.Invoking(() => new AutomationCommand("aname", false, "^aninvalidfilepath^"))
                .Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage(ValidationMessages.Automation_InvalidFilePath.Format("^aninvalidfilepath^") + "*");
        }
    }
}