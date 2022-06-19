using System.Collections.Generic;
using Automate.Authoring.Domain;
using Automate.Common.Domain;
using FluentAssertions;
using Xunit;

namespace Core.UnitTests.Authoring.Domain
{
    [Trait("Category", "Unit")]
    public class CollectionAutoNamerSpec
    {
        private readonly Element element;
        private readonly CollectionAutoNamer namer;

        public CollectionAutoNamerSpec()
        {
            this.namer = new CollectionAutoNamer();
            this.element = new Element("anelementname");
        }

        [Fact]
        public void WhenGetNextAutomationNameWithNameAndNameNotAssigned_ThenReturnsProposedName()
        {
            var result =
                this.namer.GetNextAutomationName(AutomationType.Unknown, "aname", this.element);

            result.Should().Be("aname");
        }

        [Fact]
        public void WhenGetNextAutomationNameWithNullNameAndNoAutomation_ThenReturnsNextAvailableName()
        {
            var result =
                this.namer.GetNextAutomationName(AutomationType.Unknown, null, this.element);

            result.Should().Be("Unknown1");
        }

        [Fact]
        public void WhenGetNextAutomationNameWithNullNameAndAutomation_ThenReturnsNextAvailableName()
        {
            this.element.AddAutomation(new Automation("anautomationname",
                AutomationType.Unknown,
                new Dictionary<string, object>()));

            var result =
                this.namer.GetNextAutomationName(AutomationType.Unknown, null, this.element);

            result.Should().Be("Unknown2");
        }

        [Fact]
        public void WhenGetNextAutomationNameWithNullNameAndNextNameAssigned_ThenReturnsNextAvailableName()
        {
            this.element.AddAutomation(new Automation("Unknown2",
                AutomationType.Unknown,
                new Dictionary<string, object>()));

            var result =
                this.namer.GetNextAutomationName(AutomationType.Unknown, null, this.element);

            result.Should().Be("Unknown3");
        }
    }
}