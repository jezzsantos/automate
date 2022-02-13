using automate.Domain;
using automate.Infrastructure;
using FluentAssertions;
using Xunit;

namespace CLI.UnitTests.Infrastructure
{
    [Trait("Category", "Unit")]
    public class SolutionStoreSpec
    {
        private readonly MemoryRepository repository;
        private readonly SolutionStore store;

        public SolutionStoreSpec()
        {
            this.repository = new MemoryRepository();
            this.store = new SolutionStore(this.repository, this.repository);
        }

        [Fact]
        public void WhenListAllAndSolutions_ThenReturnsAll()
        {
            var solution1 = new SolutionDefinition("atoolkitid1", new PatternDefinition());
            var solution2 = new SolutionDefinition("atoolkitid2", new PatternDefinition());
            var solution3 = new SolutionDefinition("atoolkitid3", new PatternDefinition());
            this.repository.NewSolution(solution1);
            this.repository.NewSolution(solution2);
            this.repository.NewSolution(solution3);

            var result = this.store.ListAll();

            result.Should().Contain(x => x.Id == solution1.Id);
            result.Should().Contain(x => x.Id == solution2.Id);
            result.Should().Contain(x => x.Id == solution3.Id);
        }

        [Fact]
        public void WhenFindByIdAndNotExists_ThenReturnsNull()
        {
            var result = this.store.FindById("asolutionid");

            result.Should().BeNull();
        }

        [Fact]
        public void WhenFindByIdAndExists_ThenReturnsSolution()
        {
            var solution1 = new SolutionDefinition("atoolkitid1", new PatternDefinition());
            var solution2 = new SolutionDefinition("atoolkitid2", new PatternDefinition());
            var solution3 = new SolutionDefinition("atoolkitid3", new PatternDefinition());
            this.repository.NewSolution(solution1);
            this.repository.NewSolution(solution2);
            this.repository.NewSolution(solution3);

            var result = this.store.FindById(solution1.Id);

            result.Id.Should().Be(solution1.Id);
        }
    }
}