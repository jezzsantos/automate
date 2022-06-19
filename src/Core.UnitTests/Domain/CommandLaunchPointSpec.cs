using System;
using System.Collections.Generic;
using System.Linq;
using Automate.Domain;
using Automate.Extensions;
using FluentAssertions;
using Xunit;

namespace Core.UnitTests.Domain
{
    [Trait("Category", "Unit")]
    public class CommandLaunchPointSpec
    {
        private readonly CommandLaunchPoint launchPoint;

        public CommandLaunchPointSpec()
        {
            var pattern = new PatternDefinition("apatternname");
            this.launchPoint = new CommandLaunchPoint("alaunchpointname",
                new List<string> { IdGenerator.Create() });
            pattern.AddAutomation(this.launchPoint.AsAutomation());
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
            FluentActions.Invoking(() =>
                    new CommandLaunchPoint("aname", new List<string>()))
                .Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage(ValidationMessages.Automation_EmptyCommandIds + "*");
        }

        [Fact]
        public void WhenConstructedAndCommandIdsInvalid_ThenThrows()
        {
            var cmdIds = new List<string> { IdGenerator.Create(), "aninvalidcmdid", IdGenerator.Create() };
            FluentActions.Invoking(() => new CommandLaunchPoint("aname", cmdIds))
                .Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage(ValidationMessages.Automation_InvalidCommandIds.Substitute(cmdIds.Join(", ")) +
                             "*");
        }

        [Fact]
        public void WhenAppendCommandIdsAndHasNone_ThenAddsOne()
        {
            this.launchPoint.AppendCommandIds(new List<string> { "acmdid" });

            this.launchPoint.CommandIds.Should().ContainInOrder(this.launchPoint.CommandIds.First(), "acmdid");
        }

        [Fact]
        public void WhenAppendCommandIdsWithNew_ThenAddsNew()
        {
            this.launchPoint.AppendCommandIds(new List<string> { "acmdid1" });

            this.launchPoint.AppendCommandIds(new List<string> { "acmdid2" });

            this.launchPoint.CommandIds.Should()
                .ContainInOrder(this.launchPoint.CommandIds.First(), "acmdid1", "acmdid2");
        }

        [Fact]
        public void WhenAppendCommandIdsWithNewAndDuplicates_ThenAddsNew()
        {
            this.launchPoint.AppendCommandIds(new List<string> { "acmdid1" });
            this.launchPoint.AppendCommandIds(new List<string> { "acmdid2" });
            this.launchPoint.AppendCommandIds(new List<string> { "acmdid3" });

            this.launchPoint.AppendCommandIds(new List<string> { "acmdid2", "acmdid3", "acmdid4" });

            this.launchPoint.CommandIds.Should().ContainInOrder(this.launchPoint.CommandIds.First(), "acmdid1",
                "acmdid2", "acmdid3", "acmdid4");
        }
    }
}