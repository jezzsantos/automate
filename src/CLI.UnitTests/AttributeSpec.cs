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
                .WithMessage(ExceptionMessages.Validations_InvalidIdentifier.Format("^aninvalidname^") + "*");
        }

        [Fact]
        public void WhenConstructedWithInvalidType_ThenThrows()
        {
            FluentActions.Invoking(() => new Attribute("aname", "aninvalidtype", true, null))
                .Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage(ExceptionMessages.Validations_UnsupportedAttributeType.Format("aninvalidtype",
                    Attribute.SupportedTypes.Join(", ") + "*"));
        }

        [Fact]
        public void WhenConstructedAndDefaultValueIsInvalidForBoolean_ThenThrows()
        {
            FluentActions.Invoking(() => new Attribute("aname", "boolean", true, "notaboolean"))
                .Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage(ExceptionMessages.Validations_InvalidDefaultValue.Format("notaboolean", "boolean") + "*");
        }

        [Fact]
        public void WhenConstructedAndDefaultValueIsInvalidForInteger_ThenThrows()
        {
            FluentActions.Invoking(() => new Attribute("aname", "integer", true, "notaninteger"))
                .Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage(ExceptionMessages.Validations_InvalidDefaultValue.Format("notaninteger", "integer") + "*");
        }

        [Fact]
        public void WhenConstructedAndDefaultValueIsInvalidForDateTime_ThenThrows()
        {
            FluentActions.Invoking(() => new Attribute("aname", "datetime", true, "notadatetime"))
                .Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage(ExceptionMessages.Validations_InvalidDefaultValue.Format("notadatetime", "datetime") +
                             "*");
        }

        [Fact]
        public void WhenConstructedWithNullType_ThenTypeIsDefaultType()
        {
            var attribute = new Attribute("aname", null, true, null);

            attribute.Type.Should().Be(Attribute.DefaultType);
        }
    }
}