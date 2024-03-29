﻿using System;
using System.Collections.Generic;
using System.Linq;
using Automate.Authoring.Domain;
using Automate.Common.Domain;
using Automate.Common.Extensions;
using FluentAssertions;
using Xunit;
using Attribute = Automate.Authoring.Domain.Attribute;

namespace Core.UnitTests.Authoring.Domain
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
                .WithMessage(ValidationMessages.InvalidNameIdentifier.Substitute("^aninvalidname^") + "*");
        }

        [Fact]
        public void WhenConstructedWithInvalidType_ThenThrows()
        {
            FluentActions.Invoking(() => new Attribute("aname", "aninvalidtype", true))
                .Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage(ValidationMessages.Attribute_UnsupportedDataType.Substitute("aninvalidtype",
                    Attribute.SupportedDataTypes.Join(", ") + "*"));
        }

        [Fact]
        public void WhenConstructedAndDefaultValueIsInvalidForBoolean_ThenThrows()
        {
            FluentActions.Invoking(() => new Attribute("aname", "bool", true, "notaboolean"))
                .Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage(
                    ValidationMessages.Attribute_InvalidDefaultValue.Substitute("notaboolean", "bool") + "*");
        }

        [Fact]
        public void WhenConstructedAndDefaultValueIsInvalidForInteger_ThenThrows()
        {
            FluentActions.Invoking(() => new Attribute("aname", "int", true, "notaninteger"))
                .Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage(
                    ValidationMessages.Attribute_InvalidDefaultValue.Substitute("notaninteger", "int") +
                    "*");
        }

        [Fact]
        public void WhenConstructedAndDefaultValueIsInvalidDataType_ThenThrows()
        {
            FluentActions.Invoking(() => new Attribute("aname", "datetime", true, "notadatetime"))
                .Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage(
                    ValidationMessages.Attribute_InvalidDefaultValue.Substitute("notadatetime", "datetime") +
                    "*");
        }

        [Fact]
        public void WhenConstructedAndDefaultValueIsNotChoice_ThenThrows()
        {
            FluentActions.Invoking(() => new Attribute("aname", defaultValue: "notachoice",
                    choices: new List<string> { "achoice1", "achoice2" }))
                .Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage(
                    ValidationMessages.Attribute_DefaultValueIsNotAChoice.Substitute("notachoice",
                        new[] { "achoice1", "achoice2" }.Join("; ")) +
                    "*");
        }

        [Fact]
        public void WhenConstructedAndChoiceIsInvalidDataType_ThenThrows()
        {
            FluentActions.Invoking(() =>
                    new Attribute("aname", "datetime", choices: new List<string> { "achoice1", "achoice2" }))
                .Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage(
                    ValidationMessages.Attribute_WrongDataTypeChoice.Substitute("achoice1", "datetime") +
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
            var result = this.attribute.Validate(new ValidationContext("apath"), null);

            result.Results.Should().BeEmpty();
        }

        [Fact]
        public void WhenValidateAndIsRequiredAndNoValue_ThenReturnsError()
        {
            var attribute = new Attribute("aname", "string", true);

            var result = attribute.Validate(new ValidationContext("apath"), null);

            result.Results.Single().Message.Should()
                .Be(ValidationMessages.Attribute_ValidationRule_RequiredAttributeValue.Substitute("aname"));
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
                .WithMessage(ValidationMessages.InvalidNameIdentifier.Substitute("^aninvalidname^") + "*");
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
                .WithMessage(ValidationMessages.Attribute_UnsupportedDataType.Substitute("anunknowndatatype",
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
                .WithMessage(ValidationMessages.Attribute_InvalidDefaultValue.Substitute("aninvalidinteger", "int") +
                             "*");
        }

        [Fact]
        public void WhenSetDefaultValueAndNotSameChoice_ThenThrows()
        {
            this.attribute.SetChoices(new List<string> { "achoice" });
            this.attribute
                .Invoking(x => x.SetDefaultValue("notachoice"))
                .Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage(
                    ValidationMessages.Attribute_DefaultValueIsNotAChoice.Substitute("notachoice", "achoice") + "*");
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