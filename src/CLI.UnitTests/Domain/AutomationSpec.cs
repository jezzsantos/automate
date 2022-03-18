using System;
using System.Collections.Generic;
using Automate.CLI.Domain;
using Automate.CLI.Extensions;
using FluentAssertions;
using Xunit;

namespace CLI.UnitTests.Domain
{
    [Trait("Category", "Unit")]
    public class AutomationSpec
    {
        private readonly Automation automation;

        public AutomationSpec()
        {
            this.automation = new Automation("12345678", AutomationType.Unknown, new Dictionary<string, object>
            {
                { "aname", "avalue" }
            });
        }

        [Fact]
        public void WhenConstructedAndNameIsMissing_ThenThrows()
        {
            FluentActions.Invoking(() => new Automation(null, AutomationType.Unknown, new Dictionary<string, object>()))
                .Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void WhenConstructedAndNameIsInvalid_ThenThrows()
        {
            FluentActions.Invoking(() =>
                    new Automation("^aninvalidname^", AutomationType.Unknown, new Dictionary<string, object>()))
                .Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage(ValidationMessages.InvalidNameIdentifier.Format("^aninvalidname^") + "*");
        }

        [Fact]
        public void WhenConstructedAndMetadataIsMissing_ThenThrows()
        {
            FluentActions.Invoking(() => new Automation("12345678", AutomationType.Unknown, null))
                .Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void WhenUpdateMetadataAndPropertyNotExists_ThenAddsProperty()
        {
            this.automation.UpdateMetadata("anunknownname", "anewvalue");

            this.automation.Metadata.Should().HaveCount(2);
            this.automation.Metadata["aname"].Should().Be("avalue");
            this.automation.Metadata["anunknownname"].Should().Be("anewvalue");
        }

        [Fact]
        public void WhenUpdateMetadataAndPropertyExists_ThenUpdatesProperty()
        {
            this.automation.UpdateMetadata("aname", "anewvalue");

            this.automation.Metadata.Should().HaveCount(1);
            this.automation.Metadata["aname"].Should().Be("anewvalue");
        }
    }
}