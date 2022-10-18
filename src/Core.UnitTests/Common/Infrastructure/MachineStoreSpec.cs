using Automate.Common.Domain;
using Automate.Common.Infrastructure;
using FluentAssertions;
using Xunit;

namespace Core.UnitTests.Common.Infrastructure
{
    [Trait("Category", "Unit")]
    public class MachineStoreSpec
    {
        private readonly MachineStore store;
        private readonly MemoryRepository repository;

        public MachineStoreSpec()
        {
            this.repository = new MemoryRepository();
            this.store = new MachineStore(this.repository);
            this.repository.DestroyAll();
        }

        [Fact]
        public void WhenConstructed_ThenAssigned()
        {
            this.store.GetInstallationId().Should().BeNull();
        }

        [Fact]
        public void WhenGetInstallationIdAndExists_ThenReturns()
        {
            var state = new MachineState();
            state.SetInstallationId("aninstallationid");
            this.repository.SaveMachineState(state);

            var result = this.store.GetInstallationId();

            result.Should().Be("aninstallationid");
        }

        [Fact]
        public void WhenGetInstallationIdAfterSet_ThenReturns()
        {
            this.store.SetInstallationId("aninstallationid");

            var result = this.store.GetInstallationId();

            result.Should().Be("aninstallationid");
        }
    }
}