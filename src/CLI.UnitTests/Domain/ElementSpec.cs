using Automate.CLI.Domain;
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
        public void WhenSetCardinality_ThenSets()
        {
            this.element.SetCardinality(ElementCardinality.OneOrMany);

            this.element.Cardinality.Should().Be(ElementCardinality.OneOrMany);
            this.element.Pattern.ToolkitVersion.LastChanges.Should().Be(VersionChange.Breaking);
        }
    }
}