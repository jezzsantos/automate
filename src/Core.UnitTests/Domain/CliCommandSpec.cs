using System;
using Automate.Domain;
using FluentAssertions;
using Xunit;

namespace Core.UnitTests.Domain
{
    public class CliCommandSpec
    {
        [Trait("Category", "Unit")]
        public class GivenAnyCommand
        {
            [Fact]
            public void WhenConstructedAndApplicationNameIsMissing_ThenThrows()
            {
                FluentActions.Invoking(() =>
                        new CliCommand("aname", null, "anargument"))
                    .Should().Throw<ArgumentNullException>();
            }
        }
    }
}