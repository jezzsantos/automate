using Automate.Common.Domain;
using Automate.Common.Infrastructure;
using FluentAssertions;
using Xunit;

namespace Core.UnitTests.Common.Infrastructure
{
    [Trait("Category", "Unit")]
    public class MachineStoreSpec
    {
        private readonly MemoryRepository repository;
        private readonly MachineStore store;

        public MachineStoreSpec()
        {
            this.repository = new MemoryRepository();
            this.store = new MachineStore(this.repository, () => "anewid");
            this.repository.DestroyAll();
        }

        [Fact]
        public void WhenGetOrCreateInstallationIdAndExists_ThenReturns()
        {
            var state = new MachineState();
            state.SetInstallationId("aninstallationid");
            this.repository.SaveMachineState(state);

            var result = this.store.GetOrCreateInstallationId();

            result.Should().Be("aninstallationid");
        }

        [Fact]
        public void WhenGetOrCreateAndNotExists_ThenCreatesAndReturns()
        {
            var result = this.store.GetOrCreateInstallationId();

            result.Should().Be("anewid");
        }
    }
}