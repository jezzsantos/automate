using Automate.Common.Extensions;
using FluentAssertions;
using Xunit;

namespace Core.UnitTests.Common.Extensions
{
    [Trait("Category", "Unit")]
    public class SemVersionExtensionsSpec
    {
        [Fact]
        public void WhenNextMinorAndHasMajorAndPatchAndPreRelease_ThenReturnsNextMinor()
        {
            var result = "1.1.1-prerelease".ToSemVersion().NextMinor();

            result.ToString().Should().Be("1.2.0");
        }

        [Fact]
        public void WhenNextMajorAndHasMinorAndPatchAndPreRelease_ThenReturnsNextMajor()
        {
            var result = "1.1.1-prerelease".ToSemVersion().NextMajor();

            result.ToString().Should().Be("2.0.0");
        }
    }
}