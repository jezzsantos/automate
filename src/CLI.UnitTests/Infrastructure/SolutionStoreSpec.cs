using System.Linq;
using Automate.CLI;
using Automate.CLI.Domain;
using Automate.CLI.Extensions;
using Automate.CLI.Infrastructure;
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
            var solution1 =
                new SolutionDefinition(new ToolkitDefinition(new PatternDefinition("apatternname1"), "1.0"));
            var solution2 =
                new SolutionDefinition(new ToolkitDefinition(new PatternDefinition("apatternname2"), "1.0"));
            var solution3 =
                new SolutionDefinition(new ToolkitDefinition(new PatternDefinition("apatternname3"), "1.0"));
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
            var solution1 =
                new SolutionDefinition(new ToolkitDefinition(new PatternDefinition("apatternname1"), "1.0"));
            var solution2 =
                new SolutionDefinition(new ToolkitDefinition(new PatternDefinition("apatternname2"), "1.0"));
            var solution3 =
                new SolutionDefinition(new ToolkitDefinition(new PatternDefinition("apatternname3"), "1.0"));
            this.repository.NewSolution(solution1);
            this.repository.NewSolution(solution2);
            this.repository.NewSolution(solution3);

            var result = this.store.FindById(solution1.Id);

            result.Id.Should().Be(solution1.Id);
        }
    
        [Fact]
        public void WhenCreate_ThenReturnsSolutionAsCurrent()
        {
            var result = this.store.Create(new ToolkitDefinition(new PatternDefinition("apatternname1"), "1.0"));

            this.repository.ListSolutions().Single().Id.Should().Be(result.Id);
            this.repository.GetLocalState().CurrentSolution.Should().Be(result.Id);
        }

        [Fact]
        public void WhenChangeCurrentAndSolutionNotExist_ThenThrows()
        {
            this.store
                .Invoking(x => x.ChangeCurrent("anid"))
                .Should().Throw<AutomateException>()
                .WithMessage(
                    ExceptionMessages.SolutionStore_NotFoundAtLocationWithId.Format("anid",
                        this.repository.PatternLocation));
        }

        [Fact]
        public void WhenChangeCurrent_ThenChangesCurrent()
        {
            var solution1 = new SolutionDefinition(new ToolkitDefinition(new PatternDefinition("apatternname1"), "1.0"));
            var solution2 = new SolutionDefinition(new ToolkitDefinition(new PatternDefinition("apatternname1"), "1.0"));
            this.repository.NewSolution(solution1);
            this.repository.NewSolution(solution2);

            this.store.ChangeCurrent(solution1.Id);

            this.repository.GetLocalState().CurrentSolution.Should().Be(solution1.Id);

            this.store.ChangeCurrent(solution2.Id);

            this.repository.GetLocalState().CurrentSolution.Should().Be(solution2.Id);
        }
    }
}