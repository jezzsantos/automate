using System;
using System.Collections.Generic;
using System.Linq;
using Automate.CLI.Domain;
using Automate.CLI.Extensions;
using FluentAssertions;
using ServiceStack;
using Xunit;
using Attribute = Automate.CLI.Domain.Attribute;
using CollectionExtensions = Automate.CLI.Extensions.CollectionExtensions;

namespace CLI.UnitTests.Domain
{
    [Trait("Category", "Unit")]
    public class AttributeSpec
    {
        [Fact]
        public void WhenConstructedWithInvalidName_ThenThrows()
        {
            FluentActions.Invoking(() => new Attribute("^aninvalidname^", "string", true))
                .Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage(ValidationMessages.InvalidNameIdentifier.Format("^aninvalidname^") + "*");
        }

        [Fact]
        public void WhenConstructedWithInvalidType_ThenThrows()
        {
            FluentActions.Invoking(() => new Attribute("aname", "aninvalidtype", true))
                .Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage(ValidationMessages.Attribute_UnsupportedDataType.Format("aninvalidtype",
                    CollectionExtensions.Join(Attribute.SupportedDataTypes, ", ") + "*"));
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
        public void WhenConstructedAndDefaultValueIsInvalidDataType_ThenThrows()
        {
            FluentActions.Invoking(() => new Attribute("aname", "DateTime", true, "notadatetime"))
                .Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage(
                    ValidationMessages.Attribute_InvalidDefaultValue.Format("notadatetime", "DateTime") +
                    "*");
        }

        [Fact]
        public void WhenConstructedAndDefaultValueIsNotChoice_ThenThrows()
        {
            FluentActions.Invoking(() => new Attribute("aname", defaultValue: "notachoice",
                    choices: new List<string> { "achoice1", "achoice2" }))
                .Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage(
                    ValidationMessages.Attribute_DefaultValueIsNotAChoice.Format("notachoice",
                        ListExtensions.Join(new[] { "achoice1", "achoice2" }, "; ")) +
                    "*");
        }

        [Fact]
        public void WhenConstructedAndChoiceIsInvalidDataType_ThenThrows()
        {
            FluentActions.Invoking(() =>
                    new Attribute("aname", "DateTime", choices: new List<string> { "achoice1", "achoice2" }))
                .Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage(
                    ValidationMessages.Attribute_WrongDataTypeChoice.Format("achoice1", "DateTime") +
                    "*");
        }

        [Fact]
        public void WhenConstructedWithNullType_ThenTypeIsDefaultType()
        {
            var attribute = new Attribute("aname", null, true);

            attribute.DataType.Should().Be(Attribute.DefaultType);
        }

        [Fact]
        public void WhenConstructedWithChoices_ThenSetsChoices()
        {
            var result = new Attribute("aname", choices: new List<string> { "achoice1", "achoice2" });

            result.Choices.Should().Contain("achoice1");
            result.Choices.Should().Contain("achoice2");
        }

        [Fact]
        public void WhenValidateAndNotRequired_ThenReturnsNoErrors()
        {
            var attribute = new Attribute("aname");

            var result = attribute.Validate(new ValidationContext("apath"), null);

            result.Results.Should().BeEmpty();
        }

        [Fact]
        public void WhenValidateAndIsRequiredAndNoValue_ThenReturnsError()
        {
            var attribute = new Attribute("aname", "string", true);

            var result = attribute.Validate(new ValidationContext("apath"), null);

            result.Results.Single().Message.Should()
                .Be(ValidationMessages.Attribute_ValidationRule_RequiredValue.Format("aname"));
        }

        [Fact]
        public void WhenValidateAndIsRequiredAndValue_ThenReturnsNoErrors()
        {
            var attribute = new Attribute("aname", "string", true);

            var result = attribute.Validate(new ValidationContext("apath"), "avalue");

            result.Results.Should().BeEmpty();
        }
    }
}