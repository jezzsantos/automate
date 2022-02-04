using automate;
using FluentAssertions;
using Xunit;

namespace CLI.UnitTests
{
    [Trait("Category", "Unit")]
    public class RuntimeApplicationSpec
    {
        private readonly RuntimeApplication application;

        public RuntimeApplicationSpec()
        {
            var store = new ToolkitStore(new MemoryRepository());
            this.application =
                new RuntimeApplication(store);
        }

        [Fact]
        public void WhenConstructed_ThenPropertiesAssigned()
        {
            this.application.CurrentToolkitId.Should().BeNull();
        }
    }
}