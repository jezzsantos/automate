using System;
using Automate.CLI.Domain;
using Automate.CLI.Extensions;
using FluentAssertions;
using Xunit;

namespace CLI.UnitTests.Domain
{
    [Trait("Category", "Unit")]
    public class ElementSpec
    {
        private readonly Element element;

        public ElementSpec()
        {
            var pattern = new PatternDefinition("apatternname");
            this.element = pattern.AddElement("anelementname");
            this.element.SetParent(pattern);
        }

        [Fact]
        public void WhenRenameWithInvalidName_ThenThrows()
        {
            this.element
                .Invoking(x => x.Rename("^aninvalidname^"))
                .Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage(ValidationMessages.InvalidNameIdentifier.Format("^aninvalidname^") + "*");
        }

        [Fact]
        public void WhenRename_ThenRenames()
        {
            this.element.Rename("aname");

            this.element.Name.Should().Be("aname");
            this.element.Pattern.ToolkitVersion.LastChanges.Should().Be(VersionChange.Breaking);
        }

        [Fact]
        public void WhenSetCardinality_ThenSets()
        {
            this.element.SetCardinality(ElementCardinality.OneOrMany);

            this.element.Cardinality.Should().Be(ElementCardinality.OneOrMany);
            this.element.Pattern.ToolkitVersion.LastChanges.Should().Be(VersionChange.Breaking);
        }

        [Fact]
        public void WhenSetDisplayNameWithEmpty_ThenThrows()
        {
            this.element
                .Invoking(x => x.SetDisplayName(""))
                .Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void WhenSetDisplayName_ThenSetDisplayNames()
        {
            this.element.SetDisplayName("adisplayname");

            this.element.DisplayName.Should().Be("adisplayname");
            this.element.Pattern.ToolkitVersion.LastChanges.Should().Be(VersionChange.NonBreaking);
        }

        [Fact]
        public void WhenSetDescriptionWithEmpty_ThenThrows()
        {
            this.element
                .Invoking(x => x.SetDescription(""))
                .Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void WhenSetDescription_ThenSetDescriptions()
        {
            this.element.SetDescription("adescription");

            this.element.Description.Should().Be("adescription");
            this.element.Pattern.ToolkitVersion.LastChanges.Should().Be(VersionChange.NonBreaking);
        }
    }
}