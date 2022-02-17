using Automate.CLI.Domain;
using FluentAssertions;
using Xunit;

namespace CLI.UnitTests.Domain
{
    [Trait("Category", "Unit")]
    public class PatternDefinitionSpec
    {
        private readonly PatternDefinition pattern;

        public PatternDefinitionSpec()
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