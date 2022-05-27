using System;
using System.Collections.Generic;
using System.Linq;
using Automate.CLI.Domain;
using Automate.CLI.Extensions;
using FluentAssertions;
using Xunit;
using Attribute = Automate.CLI.Domain.Attribute;

namespace CLI.UnitTests.Domain
{
    [Trait("Category", "Unit")]
    public class AttributeSpec
    {
        private readonly Attribute attribute;
        private readonly PatternDefinition pattern;

        public AttributeSpec()
        {
            this.pattern = new PatternDefinition("apatternname");
            this.attribute = this.pattern.AddAttribute("anattributename");
        }
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
                        new[] { "achoice1", "achoice2" }.Join("; ")) +
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
            var result = attribute.Validate(new ValidationContext("apath"), null);

            result.Results.Should().BeEmpty();
        }

        [Fact]
        public void WhenValidateAndIsRequiredAndNoValue_ThenReturnsError()
        {
            var attribute = new Attribute("aname", "string", true);

            var result = attribute.Validate(new ValidationContext("apath"), null);

            result.Results.Single().Message.Should()
                .Be(ValidationMessages.Attribute_ValidationRule_RequiredAttributeValue.Format("aname"));
        }

        [Fact]
        public void WhenValidateAndIsRequiredAndValue_ThenReturnsNoErrors()
        {
            var attribute = new Attribute("aname", "string", true);

            var result = attribute.Validate(new ValidationContext("apath"), "avalue");

            result.Results.Should().BeEmpty();
        }

        [Fact]
        public void WhenRenameWithInvalidName_ThenThrows()
        {
            this.attribute
                .Invoking(x => x.Rename("^aninvalidname^"))
                .Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage(ValidationMessages.InvalidNameIdentifier.Format("^aninvalidname^") + "*");
        }

        [Fact]
        public void WhenRename_ThenRenames()
        {
            this.attribute.Rename("aname");

            this.attribute.Name.Should().Be("aname");
            this.pattern.ToolkitVersion.LastChanges.Should().Be(VersionChange.Breaking);
        }

        [Fact]
        public void WhenSetRequired_ThenSets()
        {
            this.attribute.SetRequired(true);

            this.attribute.IsRequired.Should().BeTrue();
            this.pattern.ToolkitVersion.LastChanges.Should().Be(VersionChange.NonBreaking);
        }

        [Fact]
        public void WhenSetDataTypeWithInvalidDataType_ThenThrows()
        {
            this.attribute
                .Invoking(x => x.ResetDataType("anunknowndatatype"))
                .Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage(ValidationMessages.Attribute_UnsupportedDataType.Format("anunknowndatatype",
                    Attribute.SupportedDataTypes.Join(", ")) + "*");
        }

        [Fact]
        public void WhenResetDataType_ThenResetsDataType()
        {
            this.attribute.ResetDataType(Attribute.SupportedDataTypes.Last());

            this.attribute.DataType.Should().Be(Attribute.SupportedDataTypes.Last());
            this.pattern.ToolkitVersion.LastChanges.Should().Be(VersionChange.Breaking);
        }

        [Fact]
        public void WhenSetDefaultValueAndNotSameDataType_ThenThrows()
        {
            this.attribute.ResetDataType("int");
            this.attribute
                .Invoking(x => x.SetDefaultValue("aninvalidinteger"))
                .Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage(ValidationMessages.Attribute_InvalidDefaultValue.Format("aninvalidinteger", "int") + "*");
        }

        [Fact]
        public void WhenSetDefaultValueAndNotSameChoice_ThenThrows()
        {
            this.attribute.SetChoices(new List<string> { "achoice" });
            this.attribute
                .Invoking(x => x.SetDefaultValue("notachoice"))
                .Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage(
                    ValidationMessages.Attribute_DefaultValueIsNotAChoice.Format("notachoice", "achoice") + "*");
        }

        [Fact]
        public void WhenSetDefaultValueForDataType_ThenSetDefaultValues()
        {
            this.attribute.ResetDataType("int");

            this.attribute.SetDefaultValue("25");

            this.attribute.DefaultValue.Should().Be("25");
            this.pattern.ToolkitVersion.LastChanges.Should().Be(VersionChange.Breaking);
        }

        [Fact]
        public void WhenSetDefaultValueForChoice_ThenSetDefaultValues()
        {
            this.attribute.SetChoices(new List<string> { "achoice" });

            this.attribute.SetDefaultValue("achoice");

            this.attribute.DefaultValue.Should().Be("achoice");
            this.pattern.ToolkitVersion.LastChanges.Should().Be(VersionChange.NonBreaking);
        }

        [Fact]
        public void WhenSetDefaultValueForString_ThenSetDefaultValues()
        {
            this.attribute.SetDefaultValue("astring");

            this.attribute.DefaultValue.Should().Be("astring");
            this.pattern.ToolkitVersion.LastChanges.Should().Be(VersionChange.NonBreaking);
        }
    }
}