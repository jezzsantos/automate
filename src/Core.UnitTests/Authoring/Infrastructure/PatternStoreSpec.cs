using System.Collections.Generic;
using System.Linq;
using Automate.Authoring.Domain;
using Automate.Authoring.Infrastructure;
using Automate.Common;
using Automate.Common.Extensions;
using Automate.Common.Infrastructure;
using FluentAssertions;
using Xunit;

namespace Core.UnitTests.Authoring.Infrastructure
{
    [Trait("Category", "Unit")]
    public class PatternStoreSpec
    {
        private readonly MemoryRepository repository;
        private readonly PatternStore store;

        public PatternStoreSpec()
        {
            this.repository = new MemoryRepository();
            this.store = new PatternStore(this.repository, this.repository);
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
            var pattern1 = new PatternDefinition("aname1");
            var pattern2 = new PatternDefinition("aname2");
            var pattern3 = new PatternDefinition("aname3");
            this.repository.NewPattern(pattern1);
            this.repository.NewPattern(pattern2);
            this.repository.NewPattern(pattern3);

            var result = this.store.LoadAll();

            result.Should().Contain(x => x.Id == pattern1.Id);
            result.Should().Contain(x => x.Id == pattern2.Id);
            result.Should().Contain(x => x.Id == pattern3.Id);
        }

        [Fact]
        public void WhenSaveAllAndPatterns_ThenSaves()
        {
            var pattern1 = new PatternDefinition("aname1");
            var pattern2 = new PatternDefinition("aname2");
            var pattern3 = new PatternDefinition("aname3");

            this.store.SaveAll(new List<PatternDefinition>
            {
                pattern1,
                pattern2,
                pattern3
            });

            var result = this.repository.ListPatterns();
            result.Should().Contain(x => x.Id == pattern1.Id);
            result.Should().Contain(x => x.Id == pattern2.Id);
            result.Should().Contain(x => x.Id == pattern3.Id);
        }

        [Fact]
        public void WhenFindByIdAndNotExists_ThenThrows()
        {
            this.store
                .Invoking(x => x.FindById("anid"))
                .Should().Throw<AutomateException>()
                .WithMessage(
                    ExceptionMessages.PatternStore_NotFoundAtLocationWithId.Substitute("anid",
                        this.repository.PatternLocation));
        }

        [Fact]
        public void WhenFindByIdAndExists_ThenReturnsPattern()
        {
            var pattern1 = new PatternDefinition("aname1");
            var pattern2 = new PatternDefinition("aname2");
            var pattern3 = new PatternDefinition("aname3");
            this.repository.NewPattern(pattern1);
            this.repository.NewPattern(pattern2);
            this.repository.NewPattern(pattern3);

            var result = this.store.FindById(pattern2.Id);

            result.Id.Should().Be(pattern2.Id);
        }

        [Fact]
        public void WhenCreateAndPatternExists_ThenThrows()
        {
            this.repository.NewPattern(new PatternDefinition("aname"));

            this.store
                .Invoking(x => x.Create(new PatternDefinition("aname")))
                .Should().Throw<AutomateException>()
                .WithMessage(ExceptionMessages.PatternStore_FoundNamed.Substitute("aname"));
        }

        [Fact]
        public void WhenCreate_ThenReturnsPatternAsCurrent()
        {
            var result = this.store.Create(new PatternDefinition("aname"));

            this.repository.ListPatterns().Single().Name.Should().Be(result.Name);
            this.repository.GetLocalState().CurrentPattern.Should().Be(result.Id);
        }

        [Fact]
        public void WhenChangeCurrentAndPatternNotExist_ThenThrows()
        {
            this.store
                .Invoking(x => x.ChangeCurrent("anid"))
                .Should().Throw<AutomateException>()
                .WithMessage(
                    ExceptionMessages.PatternStore_NotFoundAtLocationWithId.Substitute("anid",
                        this.repository.PatternLocation));
        }

        [Fact]
        public void WhenChangeCurrent_ThenChangesCurrent()
        {
            var pattern1 = new PatternDefinition("aname1");
            var pattern2 = new PatternDefinition("aname2");
            this.repository.NewPattern(pattern1);
            this.repository.NewPattern(pattern2);

            this.store.ChangeCurrent(pattern1.Id);

            this.repository.GetLocalState().CurrentPattern.Should().Be(pattern1.Id);

            this.store.ChangeCurrent(pattern2.Id);

            this.repository.GetLocalState().CurrentPattern.Should().Be(pattern2.Id);
        }
    }
}