using Automate.Common.Extensions;
using FluentAssertions;
using Xunit;

namespace Core.UnitTests.Common.Extensions
{
    [Trait("Category", "Unit")]
    public class BooleanExtensionsSpec
    {
        [Fact]
        public void WhenToBoolAndNullAndDefault_ThenReturnsDefault()
        {
            var result = ((object)null).ToBool(true);

            result.Should().BeTrue();
        }

        [Fact]
        public void WhenToBoolAndNull_ThenReturnsFalse()
        {
            var result = ((object)null).ToBool();

            result.Should().BeFalse();
        }

        [Fact]
        public void WhenToBoolAndNotConvertableAndDefaultIsTrue_ThenReturnsDefault()
        {
            var result = "notaboolean".ToBool(true);

            result.Should().BeTrue();
        }

        [Fact]
        public void WhenToBoolAndNotConvertableAndDefaultIsFalse_ThenReturnsDefault()
        {
            var result = "notaboolean".ToBool(false);

            result.Should().BeFalse();
        }

        [Fact]
        public void WhenToBoolAndNotConvertable_ThenReturnsFalse()
        {
            var result = "notaboolean".ToBool();

            result.Should().BeFalse();
        }

        [Fact]
        public void WhenToBoolAndBoolean_ThenReturnsTrue()
        {
            var result = true.ToBool();

            result.Should().BeTrue();
        }

        [Fact]
        public void WhenToBoolAndBoolean_ThenReturnsFalse()
        {
            var result = false.ToBool();

            result.Should().BeFalse();
        }

        [Fact]
        public void WhenToBoolAndConvertable_ThenReturnsTrue()
        {
            var result = "true".ToBool();

            result.Should().BeTrue();
        }

        [Fact]
        public void WhenToBoolAndConvertable_ThenReturnsFalse()
        {
            var result = "false".ToBool();

            result.Should().BeFalse();
        }
    }
}