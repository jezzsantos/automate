using System.Collections.Generic;
using System.Linq;
using automate;
using automate.Extensions;
using FluentAssertions;
using Xunit;

namespace CLI.UnitTests
{
    [Trait("Category", "Unit")]
    public class PatternStoreSpec
    {
        private readonly MemoryRepository repository;
        private readonly PatternStore store;

        public PatternStoreSpec()
        {
            this.repository = new MemoryRepository();
            this.store = new PatternStore(this.repository);
            this.repository.DestroyAll();
        }

        [Fact]
        public void WhenConstructed_ThenAssigned()
        {
            this.store.GetCurrent().Should().BeNull();
        }

        [Fact]
        public void WhenLoadAllAndNoPatterns_ThenReturnsEmpty()
        {
            var result = this.store.LoadAll();

            result.Should().BeEmpty();
        }

        [Fact]
        public void WhenLoadAllAndPatterns_ThenReturnsAll()
        {
            this.repository.New(new PatternMetaModel("aname1", "anid1"));
            this.repository.New(new PatternMetaModel("aname2", "anid2"));
            this.repository.New(new PatternMetaModel("aname3", "anid3"));

            var result = this.store.LoadAll();

            result.Should().Contain(x => x.Id == "anid1");
            result.Should().Contain(x => x.Id == "anid2");
            result.Should().Contain(x => x.Id == "anid3");
        }

        [Fact]
        public void WhenSaveAllAndPatterns_ThenSaves()
        {
            this.repository.New(new PatternMetaModel("aname1", "anid1"));
            this.repository.New(new PatternMetaModel("aname2", "anid2"));
            this.repository.New(new PatternMetaModel("aname3", "anid3"));

            this.store.SaveAll(new List<PatternMetaModel>
            {
                new PatternMetaModel("aname1", "anid1"),
                new PatternMetaModel("aname2", "anid2"),
                new PatternMetaModel("aname3", "anid3")
            });

            var result = this.repository.List();
            result.Should().Contain(x => x.Id == "anid1");
            result.Should().Contain(x => x.Id == "anid2");
            result.Should().Contain(x => x.Id == "anid3");
        }

        [Fact]
        public void WhenFindAndNotExists_ThenThrows()
        {
            this.store
                .Invoking(x => x.Find("aname"))
                .Should().Throw<PatternException>()
                .WithMessage(
                    ExceptionMessages.PatternStore_NotFoundAtLocationWithId.Format("aname", this.repository.Location));
        }

        [Fact]
        public void WhenFindAndExists_ThenReturnsPattern()
        {
            this.repository.New(new PatternMetaModel("aname1", "anid1"));
            this.repository.New(new PatternMetaModel("aname2", "anid2"));
            this.repository.New(new PatternMetaModel("aname3", "anid3"));

            var result = this.store.Find("aname1");

            result.Id.Should().Be("anid1");
        }

        [Fact]
        public void WhenCreateAndPatternExists_ThenThrows()
        {
            this.repository.New(new PatternMetaModel("aname", "anid"));

            this.store
                .Invoking(x => x.Create("aname"))
                .Should().Throw<PatternException>()
                .WithMessage(ExceptionMessages.PatternStore_FoundNamed.Format("aname"));
        }

        [Fact]
        public void WhenCreate_ThenReturnsPatternAsCurrent()
        {
            var result = this.store.Create("aname");

            this.repository.List().Single().Name.Should().Be(result.Name);
            this.repository.GetState().Current.Should().Be(result.Id);
        }

        [Fact]
        public void WhenChangeCurrentAndPatternNotExist_ThenThrows()
        {
            this.store
                .Invoking(x => x.ChangeCurrent("anid"))
                .Should().Throw<PatternException>()
                .WithMessage(
                    ExceptionMessages.PatternStore_NotFoundAtLocationWithId.Format("anid", this.repository.Location));
        }

        [Fact]
        public void WhenChangeCurrent_ThenChangesCurrent()
        {
            this.repository.New(new PatternMetaModel("aname1", "anid1"));
            this.repository.New(new PatternMetaModel("aname2", "anid2"));

            this.store.ChangeCurrent("anid2");

            this.repository.GetState().Current.Should().Be("anid2");
        }
    }
}