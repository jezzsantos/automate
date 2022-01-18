using System;
using automate;
using automate.Extensions;
using FluentAssertions;
using Xunit;
using Attribute = automate.Attribute;

namespace CLI.UnitTests
{
    [Trait("Category", "Unit")]
    public class AttributeSpec
    {
        [Fact]
        public void WhenConstructedWithInvalidName_ThenThrows()
        {
            FluentActions.Invoking(() => new Attribute("^aninvalidname^", "string", true, null))
                .Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage(ValidationMessages.InvalidNameIdentifier.Format("^aninvalidname^") + "*");
        }

        [Fact]
        public void WhenConstructedWithInvalidType_ThenThrows()
        {
            FluentActions.Invoking(() => new Attribute("aname", "aninvalidtype", true, null))
                .Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage(ValidationMessages.Attribute_UnsupportedDataType.Format("aninvalidtype",
                    Attribute.SupportedDataTypes.Join(", ") + "*"));
        }

        [Fact]
        public void WhenConstructedAndDefaultValueIsInvalidForBoolean_ThenThrows()
        {
            FluentActions.Invoking(() => new Attribute("aname", "bool", true, "notaboolean"))
                .Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage(
                    ValidationMessages.Attribute_InvalidDefaultValue.Format("notaboolean", "bool") + "*");
        }

        [Fact]
        public void WhenConstructedAndDefaultValueIsInvalidForInteger_ThenThrows()
        {
            FluentActions.Invoking(() => new Attribute("aname", "int", true, "notaninteger"))
                .Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage(
                    ValidationMessages.Attribute_InvalidDefaultValue.Format("notaninteger", "int") +
                    "*");
        }

        [Fact]
        public void WhenConstructedAndDefaultValueIsInvalidForDateTime_ThenThrows()
        {
            FluentActions.Invoking(() => new Attribute("aname", "DateTime", true, "notadatetime"))
                .Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage(
                    ValidationMessages.Attribute_InvalidDefaultValue.Format("notadatetime", "DateTime") +
                    "*");
        }

        [Fact]
        public void WhenConstructedWithNullType_ThenTypeIsDefaultType()
        {
            var attribute = new Attribute("aname", null, true, null);

            attribute.DataType.Should().Be(Attribute.DefaultType);
        }
    }
}