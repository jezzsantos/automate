using automate.Domain;
using FluentAssertions;
using Xunit;

namespace CLI.UnitTests.Domain
{
    [Trait("Category", "Unit")]
    public class PatternMetaModelSpec
    {
        private readonly PatternDefinition pattern;

        public PatternMetaModelSpec()
        {
            this.pattern = new PatternDefinition("aname");
        }

        [Fact]
        public void WhenConstructed_ThenAssigned()
        {
            this.pattern.Name.Should().Be("aname");
            this.pattern.Id.Should().NotBeEmpty();
        }
    }
}