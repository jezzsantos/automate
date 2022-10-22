using System;
using Automate.Common.Extensions;
using FluentAssertions;
using Xunit;

namespace Core.UnitTests.Common.Extensions
{
    [Trait("Category", "Unit")]
    public class RecorderExtensionsSpec
    {
        [Fact]
        public void WhenAnonymiseIdentifier_ThenReturnsHashed()
        {
            var result = "anid".AnonymiseIdentifier();

            result.Should().Be("VUzZubCSjTxfTMXN7MD/1A==");
        }

        [Fact]
        public void WhenAnonymiseIdentifierOnSameData_ThenReturnsSameHash()
        {
            var random = Guid.NewGuid().ToString("N");
            var result1 = random.AnonymiseIdentifier();
            var result2 = random.AnonymiseIdentifier();
            var result3 = random.AnonymiseIdentifier();

            result1.Should().Be(result2);
            result2.Should().Be(result3);
            result3.Should().Be(result1);
        }
    }
}