﻿using System.Linq;
using Automate.Authoring.Domain;
using Automate.Common;
using Automate.Common.Extensions;
using Automate.Common.Infrastructure;
using Automate.Runtime.Domain;
using Automate.Runtime.Infrastructure;
using FluentAssertions;
using Xunit;

namespace Core.UnitTests.Runtime.Infrastructure
{
    [Trait("Category", "Unit")]
    public class DraftStoreSpec
    {
        private readonly MemoryRepository repository;
        private readonly DraftStore store;

        public DraftStoreSpec()
        {
            this.repository = new MemoryRepository();
            this.store = new DraftStore(this.repository, this.repository);
        }

        [Fact]
        public void WhenListAllAndDrafts_ThenReturnsAll()
        {
            var draft1 =
                new DraftDefinition(new ToolkitDefinition(new PatternDefinition("apatternname1")));
            var draft2 =
                new DraftDefinition(new ToolkitDefinition(new PatternDefinition("apatternname2")));
            var draft3 =
                new DraftDefinition(new ToolkitDefinition(new PatternDefinition("apatternname3")));
            this.repository.NewDraft(draft1);
            this.repository.NewDraft(draft2);
            this.repository.NewDraft(draft3);

            var result = this.store.ListAll();

            result.Should().Contain(x => x.Id == draft1.Id);
            result.Should().Contain(x => x.Id == draft2.Id);
            result.Should().Contain(x => x.Id == draft3.Id);
        }

        [Fact]
        public void WhenFindByIdAndNotExists_ThenReturnsNull()
        {
            var result = this.store.FindById("adraftid");

            result.Should().BeNull();
        }

        [Fact]
        public void WhenFindByIdAndExists_ThenReturnsDraft()
        {
            var draft1 =
                new DraftDefinition(new ToolkitDefinition(new PatternDefinition("apatternname1")));
            var draft2 =
                new DraftDefinition(new ToolkitDefinition(new PatternDefinition("apatternname2")));
            var draft3 =
                new DraftDefinition(new ToolkitDefinition(new PatternDefinition("apatternname3")));
            this.repository.NewDraft(draft1);
            this.repository.NewDraft(draft2);
            this.repository.NewDraft(draft3);

            var result = this.store.FindById(draft1.Id);

            result.Id.Should().Be(draft1.Id);
        }

        [Fact]
        public void WhenCreate_ThenReturnsDraftAsCurrent()
        {
            var result =
                this.store.Create(new DraftDefinition(new ToolkitDefinition(new PatternDefinition("apatternname1")),
                    "aname"));

            this.repository.ListDrafts().Single().Id.Should().Be(result.Id);
            this.repository.GetLocalState().CurrentDraft.Should().Be(result.Id);
            result.Name.Should().Be("aname");
        }

        [Fact]
        public void WhenChangeCurrentAndDraftNotExist_ThenThrows()
        {
            this.store
                .Invoking(x => x.ChangeCurrent("anid"))
                .Should().Throw<AutomateException>()
                .WithMessage(
                    ExceptionMessages.DraftStore_NotFoundAtLocationWithId.Substitute("anid",
                        this.repository.PatternsLocation));
        }

        [Fact]
        public void WhenChangeCurrent_ThenChangesCurrent()
        {
            var draft1 = new DraftDefinition(new ToolkitDefinition(new PatternDefinition("apatternname1")));
            var draft2 = new DraftDefinition(new ToolkitDefinition(new PatternDefinition("apatternname1")));
            this.repository.NewDraft(draft1);
            this.repository.NewDraft(draft2);

            this.store.ChangeCurrent(draft1.Id);

            this.repository.GetLocalState().CurrentDraft.Should().Be(draft1.Id);

            this.store.ChangeCurrent(draft2.Id);

            this.repository.GetLocalState().CurrentDraft.Should().Be(draft2.Id);
        }

        [Fact]
        public void WhenDelete_ThenDeletes()
        {
            var draft =
                this.store.Create(new DraftDefinition(new ToolkitDefinition(new PatternDefinition("apatternname1")),
                    "aname"));

            this.store.DeleteById(draft.Id);

            this.repository.ListDrafts().Count.Should().Be(0);
            this.repository.GetLocalState().CurrentDraft.Should().BeNull();
        }
    }
}