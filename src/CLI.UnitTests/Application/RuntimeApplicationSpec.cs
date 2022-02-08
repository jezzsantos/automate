using automate.Application;
using automate.Infrastructure;
using FluentAssertions;
using Moq;
using Xunit;

namespace CLI.UnitTests.Application
{
    [Trait("Category", "Unit")]
    public class RuntimeApplicationSpec
    {
        private readonly RuntimeApplication application;

        public RuntimeApplicationSpec()
        {
            var store = new Mock<IToolkitStore>();
            this.application = new RuntimeApplication(store.Object);
        }

        [Fact]
        public void WhenConstructed_ThenPropertiesAssigned()
        {
            this.application.CurrentToolkitId.Should().BeNull();
        }
    }
}