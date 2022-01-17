using automate;
using FluentAssertions;
using Xunit;

namespace CLI.UnitTests
{
    [Trait("Category", "Unit")]
    public class PatternMetaModelSpec
    {
        private readonly PatternMetaModel metaModel;

        public PatternMetaModelSpec()
        {
            this.metaModel = new PatternMetaModel("aname");
        }

        [Fact]
        public void WhenConstructed_ThenAssigned()
        {
            this.metaModel.Name.Should().Be("aname");
            this.metaModel.Id.Should().NotBeEmpty();
        }
        
    }
}