﻿using Automate.Authoring.Infrastructure;
using Automate.Common.Infrastructure;
using FluentAssertions;
using Xunit;

namespace Core.UnitTests.Authoring.Infrastructure
{
    [Trait("Category", "Unit")]
    public class ToolkitStoreSpec
    {
        private readonly ToolkitStore store;

        public ToolkitStoreSpec()
        {
            var repository = new MemoryRepository();
            this.store = new ToolkitStore(repository, repository);
            repository.DestroyAll();
        }

        [Fact]
        public void WhenConstructed_ThenAssigned()
        {
            this.store.GetCurrent().Should().BeNull();
        }
    }
}