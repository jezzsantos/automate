using System;
using System.Collections.Generic;
using Automate.Domain;
using Automate.Extensions;
using FluentAssertions;
using Xunit;

namespace Core.UnitTests.Domain
{
    [Trait("Category", "Unit")]
    public class AutomationSpec
    {
        private readonly Automation automation;
        private readonly PatternDefinition pattern;

        public AutomationSpec()
        {
            this.pattern = new PatternDefinition("apatternname");
            this.automation = new Automation("12345678", AutomationType.Unknown,
                new Dictionary<string, object>
                {
                    { "aname", "avalue" }
                });
            this.pattern.AddAutomation(this.automation);
        }

        [Fact]
        public void WhenConstructedAndNameIsMissing_ThenThrows()
        {
            FluentActions.Invoking(() => new Automation(null, AutomationType.Unknown,
                    new Dictionary<string, object>()))
                .Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void WhenConstructedAndNameIsInvalid_ThenThrows()
        {
            FluentActions.Invoking(() =>
                    new Automation("^aninvalidname^", AutomationType.Unknown,
                        new Dictionary<string, object>()))
                .Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage(ValidationMessages.InvalidNameIdentifier.Substitute("^aninvalidname^") + "*");
        }

        [Fact]
        public void WhenConstructedAndMetadataIsMissing_ThenThrows()
        {
            FluentActions.Invoking(() =>
                    new Automation("12345678", AutomationType.Unknown, null))
                .Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void WhenChangeName_ThenChangesName()
        {
            this.automation.Rename("aname");

            this.automation.Name.Should().Be("aname");
            this.pattern.ToolkitVersion.LastChanges.Should().Be(VersionChange.NonBreaking);
        }

        [Fact]
        public void WhenUpdateMetadataAndPropertyNotExists_ThenAddsProperty()
        {
            this.automation.UpdateMetadata("anunknownname", "anewvalue");

            this.automation.Metadata.Should().HaveCount(2);
            this.automation.Metadata["aname"].Should().Be("avalue");
            this.automation.Metadata["anunknownname"].Should().Be("anewvalue");
            this.pattern.ToolkitVersion.LastChanges.Should().Be(VersionChange.NonBreaking);
        }

        [Fact]
        public void WhenUpdateMetadataAndPropertyExists_ThenUpdatesProperty()
        {
            this.automation.UpdateMetadata("aname", "anewvalue");

            this.automation.Metadata.Should().HaveCount(1);
            this.automation.Metadata["aname"].Should().Be("anewvalue");
            this.pattern.ToolkitVersion.LastChanges.Should().Be(VersionChange.NonBreaking);
        }
    }
}