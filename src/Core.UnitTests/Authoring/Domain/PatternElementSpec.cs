﻿using System;
using System.Collections.Generic;
using System.Linq;
using Automate.Authoring.Domain;
using Automate.Common;
using Automate.Common.Domain;
using Automate.Common.Extensions;
using FluentAssertions;
using Xunit;
using Attribute = Automate.Authoring.Domain.Attribute;

namespace Core.UnitTests.Authoring.Domain
{
    [Trait("Category", "Unit")]
    public class PatternElementSpec
    {
        private readonly TestPatternElement element;
        private readonly PatternDefinition pattern;

        public PatternElementSpec()
        {
            this.pattern = new PatternDefinition("apatternname");
            this.element = new TestPatternElement("anelementname");
            this.pattern.AddElement(this.element);
        }

        [Fact]
        public void WhenConstructWithNullName_ThenThrows()
        {
            FluentActions.Invoking(() => new TestPatternElement(null))
                .Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void WhenConstructWithInvalidName_ThenThrows()
        {
            FluentActions.Invoking(() => new TestPatternElement("an invalid name"))
                .Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage(ValidationMessages.InvalidNameIdentifier.Substitute("an invalid name") + "*");
        }

        [Fact]
        public void WhenAddAttributeAndAlreadyExists_ThenThrows()
        {
            this.element.AddAttribute("anattributename", null);

            this.element
                .Invoking(x => x.AddAttribute("anattributename", null))
                .Should().Throw<AutomateException>()
                .WithMessage(ExceptionMessages.PatternElement_AttributeByNameExists.Substitute("anattributename"));
        }

        [Fact]
        public void WhenAddAttributeWithReservedName_ThenThrows()
        {
            this.element
                .Invoking(x => x.AddAttribute(Attribute.ReservedAttributeNames[0], null))
                .Should().Throw<ArgumentOutOfRangeException>()
                .WithMessageLike(
                    ValidationMessages.Attribute_ReservedName.Substitute(
                        Attribute.ReservedAttributeNames[0]));
        }

        [Fact]
        public void WhenAddAttributeWithExistingNameOfElementOrCollection_ThenThrows()
        {
            this.element.AddElement("anelementname");

            this.element
                .Invoking(x => x.AddAttribute("anelementname", null))
                .Should().Throw<AutomateException>()
                .WithMessage(
                    ExceptionMessages.PatternElement_AttributeByNameExistsAsElement.Substitute(
                        "anelementname"));
        }

        [Fact]
        public void WhenAddAttribute_TheAddsAttributeToElement()
        {
            var result = this.element.AddAttribute("anattributename", "string", false, "adefaultvalue");

            result.Name.Should().Be("anattributename");
            result.DataType.Should().Be("string");
            result.DefaultValue.Should().Be("adefaultvalue");
            result.IsRequired.Should().BeFalse();
            result.Choices.Should().BeEmpty();
            this.element.Pattern.ToolkitVersion.LastChanges.Should().Be(VersionChange.NonBreaking);
        }

        [Fact]
        public void WhenAddAttributeAndNoType_TheAddsAttributeWithDefaultType()
        {
            var result = this.element.AddAttribute("anattributename", null);

            result.Name.Should().Be("anattributename");
            result.DataType.Should().Be("string");
            result.DefaultValue.Should().BeNull();
            result.IsRequired.Should().BeFalse();
            result.Choices.Should().BeEmpty();
            this.element.Pattern.ToolkitVersion.LastChanges.Should().Be(VersionChange.NonBreaking);
        }

        [Fact]
        public void WhenUpdateAttributeAndNotExists_ThenThrows()
        {
            this.element
                .Invoking(x => x.UpdateAttribute("anattributename"))
                .Should().Throw<AutomateException>()
                .WithMessage(ExceptionMessages.PatternElement_AttributeByNameNotExists.Substitute("anattributename"));
        }

        [Fact]
        public void WhenUpdateAttributeAndNewNameIsReserved_ThenThrows()
        {
            this.element.AddAttribute("anattributename");

            this.element
                .Invoking(x => x.UpdateAttribute("anattributename", Attribute.ReservedAttributeNames.First()))
                .Should().Throw<ArgumentOutOfRangeException>()
                .WithMessageLike(
                    ValidationMessages.Attribute_ReservedName.Substitute(
                        Attribute.ReservedAttributeNames.First()));
        }

        [Fact]
        public void WhenUpdateAttributeAndNewNameAlreadyExists_ThenThrows()
        {
            this.element.AddAttribute("anattributename");
            this.element.AddAttribute("anotherattributename");

            this.element
                .Invoking(x => x.UpdateAttribute("anattributename", "anotherattributename"))
                .Should().Throw<AutomateException>()
                .WithMessage(ExceptionMessages.PatternElement_AttributeByNameExists.Substitute("anotherattributename"));
        }

        [Fact]
        public void WhenUpdateAttributeAndNewNameExistsAsElement_ThenThrows()
        {
            this.element.AddAttribute("anattributename");
            this.element.AddElement("anelementname");

            this.element
                .Invoking(x => x.UpdateAttribute("anattributename", "anelementname"))
                .Should().Throw<AutomateException>()
                .WithMessage(
                    ExceptionMessages.PatternElement_AttributeByNameExistsAsElement.Substitute("anelementname"));
        }

        [Fact]
        public void WhenUpdateAttribute_ThenUpdates()
        {
            this.element.AddAttribute("anattributename1", "int");

            var result = this.element.UpdateAttribute("anattributename1", "anattributename2", "string", true, "one",
                new List<string> { "one", "two" });

            result.Name.Should().Be("anattributename2");
            result.DataType.Should().Be("string");
            result.IsRequired.Should().BeTrue();
            result.DefaultValue.Should().Be("one");
            result.Choices.Should().Contain("one", "two");
            this.element.Pattern.ToolkitVersion.LastChanges.Should().Be(VersionChange.Breaking);
        }

        [Fact]
        public void WhenDeleteAttribute_TheDeletesAttributeFromElement()
        {
            var attribute = this.element.AddAttribute("anattributename", "string", false, "adefaultvalue");

            var result = this.element.DeleteAttribute("anattributename");

            result.Id.Should().Be(attribute.Id);
            this.element.Attributes.Should().BeEmpty();
            this.element.Pattern.ToolkitVersion.LastChanges.Should().Be(VersionChange.Breaking);
        }

        [Fact]
        public void WhenAddElementAndAlreadyExists_ThenThrows()
        {
            this.element.AddElement("anelementname");

            this.element
                .Invoking(x => x.AddElement("anelementname"))
                .Should().Throw<AutomateException>()
                .WithMessage(ExceptionMessages.PatternElement_ElementByNameExists.Substitute("anelementname"));
        }

        [Fact]
        public void WhenAddElementWithExistingNameOfAttribute_ThenThrows()
        {
            this.element.AddAttribute("anattributename", null);

            this.element
                .Invoking(x => x.AddElement("anattributename"))
                .Should().Throw<AutomateException>()
                .WithMessage(
                    ExceptionMessages.PatternElement_ElementByNameExistsAsAttribute.Substitute("anattributename"));
        }

        [Fact]
        public void WhenAddElementWithReservedName_ThenThrows()
        {
            this.element
                .Invoking(x => x.AddElement(PatternElement.ReservedElementNames[0]))
                .Should().Throw<ArgumentOutOfRangeException>()
                .WithMessageLike(
                    ValidationMessages.Element_ReservedName.Substitute(
                        PatternElement.ReservedElementNames[0]));
        }

        [Fact]
        public void WhenAddElement_TheAddsElementToElement()
        {
            var result =
                this.element.AddElement("anelementname", ElementCardinality.One, false, "adisplayname", "adescription");

            result.Name.Should().Be("anelementname");
            result.DisplayName.Should().Be("adisplayname");
            result.Description.Should().Be("adescription");
            result.IsCollection.Should().BeFalse();
            result.AutoCreate.Should().BeFalse();
            this.element.Pattern.ToolkitVersion.LastChanges.Should().Be(VersionChange.NonBreaking);
        }

        [Fact]
        public void WhenUpdateElementAndNotExists_ThenThrows()
        {
            this.element
                .Invoking(x => x.UpdateElement("anelementname"))
                .Should().Throw<AutomateException>()
                .WithMessage(ExceptionMessages.PatternElement_ElementByNameNotExists.Substitute("anelementname"));
        }

        [Fact]
        public void WhenUpdateElementAndNewNameIsReserved_ThenThrows()
        {
            this.element.AddElement("anelementname");

            this.element
                .Invoking(x => x.UpdateElement("anelementname", PatternElement.ReservedElementNames.First()))
                .Should().Throw<ArgumentOutOfRangeException>()
                .WithMessageLike(
                    ValidationMessages.Element_ReservedName.Substitute(
                        PatternElement.ReservedElementNames.First()));
        }

        [Fact]
        public void WhenUpdateElementAndNewNameAlreadyExists_ThenThrows()
        {
            this.element.AddElement("anelementname");
            this.element.AddElement("anotherelementname");

            this.element
                .Invoking(x => x.UpdateElement("anelementname", "anotherelementname"))
                .Should().Throw<AutomateException>()
                .WithMessage(ExceptionMessages.PatternElement_ElementByNameExists.Substitute("anotherelementname"));
        }

        [Fact]
        public void WhenUpdateElementAndNewNameExistsAsElement_ThenThrows()
        {
            this.element.AddElement("anelementname");
            this.element.AddAttribute("anattributename");

            this.element
                .Invoking(x => x.UpdateElement("anelementname", "anattributename"))
                .Should().Throw<AutomateException>()
                .WithMessage(
                    ExceptionMessages.PatternElement_ElementByNameExistsAsAttribute.Substitute("anattributename"));
        }

        [Fact]
        public void WhenUpdateElementForElementAndNotRequired_ThenUpdates()
        {
            this.element.AddElement("anelement1");

            var result = this.element.UpdateElement("anelement1", "anelement2", false, false,
                "adisplayname", "adescription");

            result.Name.Should().Be("anelement2");
            result.Cardinality.Should().Be(ElementCardinality.ZeroOrOne);
            this.element.Pattern.ToolkitVersion.LastChanges.Should().Be(VersionChange.Breaking);
        }

        [Fact]
        public void WhenUpdateElementForElementAndRequired_ThenUpdates()
        {
            this.element.AddElement("anelement1", ElementCardinality.ZeroOrOne);

            var result = this.element.UpdateElement("anelement1", "anelement2", true, false,
                "adisplayname", "adescription");

            result.Name.Should().Be("anelement2");
            result.Cardinality.Should().Be(ElementCardinality.One);
            this.element.Pattern.ToolkitVersion.LastChanges.Should().Be(VersionChange.Breaking);
        }

        [Fact]
        public void WhenUpdateElementForCollectionAndNotRequired_ThenUpdates()
        {
            this.element.AddElement("anelement1", ElementCardinality.OneOrMany);

            var result = this.element.UpdateElement("anelement1", "anelement2", false, false,
                "adisplayname", "adescription");

            result.Name.Should().Be("anelement2");
            result.Cardinality.Should().Be(ElementCardinality.ZeroOrMany);
            this.element.Pattern.ToolkitVersion.LastChanges.Should().Be(VersionChange.Breaking);
        }

        [Fact]
        public void WhenUpdateElementForCollectionAndRequired_ThenUpdates()
        {
            this.element.AddElement("anelement1", ElementCardinality.ZeroOrMany);

            var result = this.element.UpdateElement("anelement1", "anelement2", true, false,
                "adisplayname", "adescription");

            result.Name.Should().Be("anelement2");
            result.Cardinality.Should().Be(ElementCardinality.OneOrMany);
            this.element.Pattern.ToolkitVersion.LastChanges.Should().Be(VersionChange.Breaking);
        }

        [Fact]
        public void WhenUpdateElement_ThenUpdates()
        {
            this.element.AddElement("anelement1");

            var result = this.element.UpdateElement("anelement1", "anelement2", true, true,
                "adisplayname", "adescription");

            result.Name.Should().Be("anelement2");
            result.Cardinality.Should().Be(ElementCardinality.One);
            result.DisplayName.Should().Be("adisplayname");
            result.Description.Should().Be("adescription");
            result.AutoCreate.Should().BeTrue();
            this.element.Pattern.ToolkitVersion.LastChanges.Should().Be(VersionChange.Breaking);
        }

        [Fact]
        public void WhenDeleteElement_TheDeletesElementFromElement()
        {
            var ele = this.element.AddElement("anelementname");

            var result = this.element.DeleteElement("anelementname");

            result.Id.Should().Be(ele.Id);
            this.element.Elements.Should().BeEmpty();
            this.element.Pattern.ToolkitVersion.LastChanges.Should().Be(VersionChange.Breaking);
        }

        [Fact]
        public void WhenAddCodeTemplateAndTemplateWithSameName_ThenThrows()
        {
            this.element.AddCodeTemplate("atemplatename", "afullpath", "anextension");

            this.element
                .Invoking(x => x.AddCodeTemplate("atemplatename", "afullpath", "anextension"))
                .Should().Throw<AutomateException>()
                .WithMessage(ExceptionMessages.PatternElement_CodeTemplateByNameExists.Substitute("atemplatename"));
        }

        [Fact]
        public void WhenAddCodeTemplate_ThenTemplateAdded()
        {
            var result = this.element.AddCodeTemplate("atemplatename", "afullpath", "anextension");

            result.Name.Should().Be("atemplatename");
            result.Metadata.OriginalFilePath.Should().Be("afullpath");
            this.element.Pattern.ToolkitVersion.LastChanges.Should().Be(VersionChange.NonBreaking);
        }

        [Fact]
        public void WhenAddCodeTemplateCommandAndCodeTemplateNotExists_ThenThrows()
        {
            this.element
                .Invoking(x => x.AddCodeTemplateCommand("acommandname", "atemplatename", false, "~/apath"))
                .Should().Throw<AutomateException>()
                .WithMessage(ExceptionMessages.PatternElement_CodeTemplateNotFound.Substitute("atemplatename"));
        }

        [Fact]
        public void WhenAddCodeTemplateCommandAndAlreadyExists_ThenThrows()
        {
            this.element.AddCodeTemplate("atemplatename", "afullpath", "anextension");
            this.element.AddCodeTemplateCommand("acommandname", "atemplatename", false, "~/apath");

            this.element
                .Invoking(x => x.AddCodeTemplateCommand("acommandname", "atemplatename", false, "~/apath"))
                .Should().Throw<AutomateException>()
                .WithMessage(ExceptionMessages.PatternElement_AutomationByNameExists.Substitute("acommandname"));
        }

        [Fact]
        public void WhenAddCodeTemplateCommand_TheAddsAutomationToElement()
        {
            this.element.AddCodeTemplate("atemplatename", "afullpath", "anextension");

            var result =
                this.element.AddCodeTemplateCommand("acommandname", "atemplatename", false, "~/apath");

            result.Name.Should().Be("acommandname");
            result.Metadata[nameof(CodeTemplateCommand.IsOneOff)].Should().Be(false);
            result.Metadata[nameof(CodeTemplateCommand.FilePath)].Should().Be("~/apath");
            this.element.Pattern.ToolkitVersion.LastChanges.Should().Be(VersionChange.NonBreaking);
        }

        [Fact]
        public void WhenAddCodeTemplateCommandAndNoName_TheAddsAutomationWithDefaultName()
        {
            this.element.AddCodeTemplate("atemplatename", "afullpath", "anextension");

            var result = this.element.AddCodeTemplateCommand(null, "atemplatename", false, "~/apath");

            result.Name.Should().Be("CodeTemplateCommand1");
            result.Metadata[nameof(CodeTemplateCommand.IsOneOff)].Should().Be(false);
            result.Metadata[nameof(CodeTemplateCommand.FilePath)].Should().Be("~/apath");
            this.element.Pattern.ToolkitVersion.LastChanges.Should().Be(VersionChange.NonBreaking);
        }

        [Fact]
        public void WhenUpdateCodeTemplateCommandAndNotExists_ThenThrows()
        {
            this.element.AddCodeTemplate("atemplatename", "afullpath", "anextension");

            this.element
                .Invoking(x => x.UpdateCodeTemplateCommand("acommandname", null, null, null))
                .Should().Throw<AutomateException>()
                .WithMessage(
                    ExceptionMessages.PatternElement_AutomationNotExistsByName.Substitute(
                        AutomationType.CodeTemplateCommand, "acommandname"));
        }

        [Fact]
        public void WhenUpdateCodeTemplateCommand_ThenUpdates()
        {
            this.element.AddCodeTemplate("atemplatename", "afullpath", "anextension");
            this.element.AddCodeTemplateCommand("acommandname", "atemplatename", false, "~/apath");

            var result = this.element.UpdateCodeTemplateCommand("acommandname", "anewname", true, "anewpath");

            result.Should().Be(this.element.Automation[0]);
            result.Name.Should().Be("anewname");
            result.Metadata[nameof(CodeTemplateCommand.IsOneOff)].Should().Be(true);
            result.Metadata[nameof(CodeTemplateCommand.FilePath)].Should().Be("anewpath");
            this.element.Pattern.ToolkitVersion.LastChanges.Should().Be(VersionChange.NonBreaking);
        }

        [Fact]
        public void WhenDeleteCodeTemplateCommandAndNotExists_ThenThrows()
        {
            this.element
                .Invoking(x => x.DeleteCodeTemplateCommand("anid", false))
                .Should().Throw<AutomateException>()
                .WithMessage(
                    ExceptionMessages.PatternElement_AutomationNotExistsById.Substitute(
                        AutomationType.CodeTemplateCommand,
                        "anid"));
        }

        [Fact]
        public void WhenDeleteCodeTemplateCommandAndNotIncludeReferencingLaunchPoints_ThenDeletesCommandOnly()
        {
            var codeTemplate = new CodeTemplate("atemplatename", "afullpath", "afileextension");
            this.element.AddCodeTemplate(codeTemplate);
            var command = this.element.AddCodeTemplateCommand("acommandname", codeTemplate.Name, false, "afilepath");
            var launchPoint =
                this.element.AddCommandLaunchPoint("alaunchpointname", new List<string> { command.Id }, this.element);

            this.element.DeleteCodeTemplateCommand(command.Id, false);

            this.element.CodeTemplates.Should().ContainSingle(x => x.Id == codeTemplate.Id);
            this.element.Automation.Should().ContainSingle(x => x.Id == launchPoint.Id);
            this.element.Pattern.ToolkitVersion.LastChanges.Should().Be(VersionChange.Breaking);
        }

        [Fact]
        public void
            WhenDeleteCodeTemplateCommandAndIncludeAutomationAndHasReferencingLaunchPoint_ThenDeletesCommandAndLaunchPoint()
        {
            var codeTemplate = new CodeTemplate("atemplatename", "afullpath", "afileextension");
            this.element.AddCodeTemplate(codeTemplate);
            var command = this.element.AddCodeTemplateCommand("acommandname", codeTemplate.Name, false, "afilepath");
            this.element.AddCommandLaunchPoint("alaunchpointname", new List<string> { command.Id }, this.element);

            this.element.DeleteCodeTemplateCommand(command.Id, true);

            this.element.CodeTemplates.Should().ContainSingle(x => x.Id == codeTemplate.Id);
            this.element.Automation.Should().BeEmpty();
            this.element.Pattern.ToolkitVersion.LastChanges.Should().Be(VersionChange.Breaking);
        }

        [Fact]
        public void
            WhenDeleteCodeTemplateCommandAndIncludeAutomationAndHasReferencingLaunchPointToOtherCommands_ThenDeletesCommandOnly()
        {
            var codeTemplate = new CodeTemplate("aname1", "afullpath", "afileextension");
            this.element.AddCodeTemplate(codeTemplate);
            var command1 = this.element.AddCodeTemplateCommand("acommandname1", codeTemplate.Name, false, "afilepath");
            var command2 = this.element.AddCodeTemplateCommand("acommandname2", codeTemplate.Name, false, "afilepath");
            var launchPoint = this.element.AddCommandLaunchPoint("alaunchpointname",
                new List<string> { command1.Id, command2.Id }, this.element);

            this.element.DeleteCodeTemplateCommand(command1.Id, true);

            this.element.CodeTemplates.Should().ContainSingle(x => x.Id == codeTemplate.Id);
            this.element.Automation.Should().HaveCount(2);
            this.element.Automation[0].Id.Should().Be(command2.Id);
            this.element.Automation[1].Id.Should().Be(launchPoint.Id);
            this.element.Automation[1].Metadata.Should().Contain(x =>
                x.Key == nameof(CommandLaunchPoint.CommandIds)
                && (string)x.Value == new List<string> { command2.Id }.Join(""));
            this.element.Pattern.ToolkitVersion.LastChanges.Should().Be(VersionChange.Breaking);
        }

        [Fact]
        public void WhenAddCliCommandAndAlreadyExists_ThenThrows()
        {
            this.element.AddCliCommand("acommandname", "anapplicationname", "anargument");

            this.element
                .Invoking(x => x.AddCliCommand("acommandname", "anapplicationname", "anargument"))
                .Should().Throw<AutomateException>()
                .WithMessage(ExceptionMessages.PatternElement_AutomationByNameExists.Substitute("acommandname"));
        }

        [Fact]
        public void WhenAddCliCommand_TheAddsAutomationToElement()
        {
            var result =
                this.element.AddCliCommand("acommandname", "anapplicationname", "anargument");

            result.Name.Should().Be("acommandname");
            result.Metadata[nameof(CliCommand.ApplicationName)].Should().Be("anapplicationname");
            result.Metadata[nameof(CliCommand.Arguments)].Should().Be("anargument");
            this.element.Pattern.ToolkitVersion.LastChanges.Should().Be(VersionChange.NonBreaking);
        }

        [Fact]
        public void WhenAddCliCommandAndNoName_TheAddsAutomationWithDefaultName()
        {
            var result = this.element.AddCliCommand(null, "anapplicationname", "anargument");

            result.Name.Should().Be("CliCommand1");
            result.Metadata[nameof(CliCommand.ApplicationName)].Should().Be("anapplicationname");
            result.Metadata[nameof(CliCommand.Arguments)].Should().Be("anargument");
            this.element.Pattern.ToolkitVersion.LastChanges.Should().Be(VersionChange.NonBreaking);
        }

        [Fact]
        public void WhenUpdateCliCommandAndNotExists_ThenThrows()
        {
            this.element
                .Invoking(x => x.UpdateCliCommand("acommandname", null, "anapplicationname", "anargument"))
                .Should().Throw<AutomateException>()
                .WithMessage(
                    ExceptionMessages.PatternElement_AutomationNotExistsByName.Substitute(AutomationType.CliCommand,
                        "acommandname"));
        }

        [Fact]
        public void WhenUpdateCliCommand_ThenUpdates()
        {
            this.element.AddCliCommand("acommandname", "anapplicationname", "anargument");

            var result =
                this.element.UpdateCliCommand("acommandname", "anewname", "anewapplicationname", "anewargument");

            result.Should().Be(this.element.Automation[0]);
            result.Name.Should().Be("anewname");
            result.Metadata[nameof(CliCommand.ApplicationName)].Should().Be("anewapplicationname");
            result.Metadata[nameof(CliCommand.Arguments)].Should().Be("anewargument");
            this.element.Pattern.ToolkitVersion.LastChanges.Should().Be(VersionChange.NonBreaking);
        }

        [Fact]
        public void WhenDeleteCliCommandAndNotExists_ThenThrows()
        {
            this.element
                .Invoking(x => x.DeleteCliCommand("anid", true))
                .Should().Throw<AutomateException>()
                .WithMessage(
                    ExceptionMessages.PatternElement_AutomationNotExistsById.Substitute(AutomationType.CliCommand,
                        "anid"));
        }

        [Fact]
        public void WhenDeleteCliCommandAndNotIncludeReferencingLaunchPoints_ThenDeletesCommandOnly()
        {
            var command = this.element.AddCliCommand("acommandname", "anapplicationname", null);
            var launchPoint =
                this.element.AddCommandLaunchPoint("alaunchpointname", new List<string> { command.Id }, this.element);

            this.element.DeleteCliCommand(command.Id, false);

            this.element.Automation.Should().ContainSingle(x => x.Id == launchPoint.Id);
            this.element.Pattern.ToolkitVersion.LastChanges.Should().Be(VersionChange.Breaking);
        }

        [Fact]
        public void
            WhenDeleteCliCommandAndIncludeAutomationAndHasReferencingLaunchPoint_ThenDeletesCommandAndLaunchPoint()
        {
            var command = this.element.AddCliCommand("acommandname", "anapplicationname", null);
            this.element.AddCommandLaunchPoint("alaunchpointname", new List<string> { command.Id }, this.element);

            this.element.DeleteCliCommand(command.Id, true);

            this.element.Automation.Should().BeEmpty();
            this.element.Pattern.ToolkitVersion.LastChanges.Should().Be(VersionChange.Breaking);
        }

        [Fact]
        public void
            WhenDeleteCliCommandAndIncludeAutomationAndHasReferencingLaunchPointToOtherCommands_ThenDeletesCommandOnly()
        {
            var command1 = this.element.AddCliCommand("acommandname1", "anapplicationname", null);
            var command2 = this.element.AddCliCommand("acommandname2", "anapplicationname", null);
            var launchPoint = this.element.AddCommandLaunchPoint("alaunchpointname",
                new List<string> { command1.Id, command2.Id }, this.element);

            this.element.DeleteCliCommand(command1.Id, true);

            this.element.Automation.Should().HaveCount(2);
            this.element.Automation[0].Id.Should().Be(command2.Id);
            this.element.Automation[1].Id.Should().Be(launchPoint.Id);
            this.element.Automation[1].Metadata.Should().Contain(x =>
                x.Key == nameof(CommandLaunchPoint.CommandIds)
                && (string)x.Value == new List<string> { command2.Id }.Join(""));
            this.element.Pattern.ToolkitVersion.LastChanges.Should().Be(VersionChange.Breaking);
        }

        [Fact]
        public void WhenAddCommandLaunchPointAndAlreadyExists_ThenThrows()
        {
            this.element.AddCodeTemplate("atemplatename", "afullpath", "anextension");
            var command = this.element.AddCodeTemplateCommand("acommandname", "atemplatename", false, "~/apath");
            this.element.AddCommandLaunchPoint("alaunchpointname", new List<string> { command.Id }, this.element);

            this.element
                .Invoking(x =>
                    x.AddCommandLaunchPoint("alaunchpointname", new List<string> { command.Id }, this.element))
                .Should().Throw<AutomateException>()
                .WithMessage(ExceptionMessages.PatternElement_AutomationByNameExists.Substitute("alaunchpointname"));
        }

        [Fact]
        public void WhenAddCommandLaunchPointAndNoCommands_ThenThrows()
        {
            this.element
                .Invoking(x => x.AddCommandLaunchPoint("alaunchpointname", new List<string>(), this.element))
                .Should().Throw<AutomateException>()
                .WithMessage(ExceptionMessages.PatternElement_NoCommandIds);
        }

        [Fact]
        public void WhenAddCommandLaunchPointAndCommandNotExists_ThenThrows()
        {
            this.element
                .Invoking(x =>
                    x.AddCommandLaunchPoint(null, new List<string> { "acmdid" }, this.element))
                .Should().Throw<AutomateException>()
                .WithMessage(
                    ExceptionMessages.PatternElement_CommandIdNotFound.Substitute("acmdid"));
        }

        [Fact]
        public void WhenAddCommandLaunchPointWithIdsFromAnywhereInPattern_TheAddsAutomationToElement()
        {
            this.pattern.AddCodeTemplate("atemplatename", "afullpath", "anextension");
            var command1 = this.pattern.AddCodeTemplateCommand("acommandname1", "atemplatename", false, "~/apath");
            var command2 = this.pattern.AddCodeTemplateCommand("acommandname2", "atemplatename", false, "~/apath");

            var result =
                this.element.AddCommandLaunchPoint("alaunchpointname", new List<string> { command1.Id, command2.Id },
                    this.pattern);

            result.Name.Should().Be("alaunchpointname");
            result.Metadata[nameof(CommandLaunchPoint.CommandIds)].Should().Be($"{command1.Id};{command2.Id}");
            this.element.Pattern.ToolkitVersion.LastChanges.Should().Be(VersionChange.NonBreaking);
        }

        [Fact]
        public void WhenAddCommandLaunchPointToPatternWithWildcard_TheAddsAllAutomationFromPatternToElement()
        {
            this.pattern.AddCodeTemplate("atemplatename", "afullpath", "anextension");
            var command1 = this.pattern.AddCodeTemplateCommand("acommandname1", "atemplatename", false, "~/apath");
            var command2 = this.pattern.AddCodeTemplateCommand("acommandname2", "atemplatename", false, "~/apath");

            var result = this.pattern.AddCommandLaunchPoint("alaunchpointname",
                new List<string> { PatternElement.LaunchPointSelectionWildcard }, this.pattern);

            result.Name.Should().Be("alaunchpointname");
            result.Metadata[nameof(CommandLaunchPoint.CommandIds)].Should().Be($"{command1.Id};{command2.Id}");
            this.pattern.ToolkitVersion.LastChanges.Should().Be(VersionChange.NonBreaking);
        }

        [Fact]
        public void WhenAddCommandLaunchPointToPatternWithWildcardAndNoAutomation_ThenThrows()
        {
            this.pattern
                .Invoking(x => x.AddCommandLaunchPoint("alaunchpointname",
                    new List<string> { PatternElement.LaunchPointSelectionWildcard }, this.pattern))
                .Should().Throw<AutomateException>()
                .WithMessage(ExceptionMessages.PatternElement_NoCommandsToLaunch);
        }

        [Fact]
        public void WhenAddCommandLaunchPointToElementWithWildcard_TheAddsAllAutomationFromElementToElement()
        {
            this.element.AddCodeTemplate("atemplatename", "afullpath", "anextension");
            var command1 = this.element.AddCodeTemplateCommand("acommandname1", "atemplatename", false, "~/apath");
            var command2 = this.element.AddCodeTemplateCommand("acommandname2", "atemplatename", false, "~/apath");

            var result = this.element.AddCommandLaunchPoint("alaunchpointname",
                new List<string> { PatternElement.LaunchPointSelectionWildcard }, this.element);

            result.Name.Should().Be("alaunchpointname");
            result.Metadata[nameof(CommandLaunchPoint.CommandIds)].Should().Be($"{command1.Id};{command2.Id}");
            this.pattern.ToolkitVersion.LastChanges.Should().Be(VersionChange.NonBreaking);
        }

        [Fact]
        public void WhenAddCommandLaunchPointAndNoName_TheAddsAutomationWithDefaultName()
        {
            this.element.AddCodeTemplate("atemplatename", "afullpath", "anextension");
            var command = this.element.AddCodeTemplateCommand("acommandname", "atemplatename", false, "~/apath");

            var result = this.element.AddCommandLaunchPoint(null, new List<string> { command.Id }, this.element);

            result.Name.Should().Be("CommandLaunchPoint2");
            result.Should().Be(this.element.Automation[1]);
            result.Metadata[nameof(CommandLaunchPoint.CommandIds)].Should().Be($"{command.Id}");
            this.element.Pattern.ToolkitVersion.LastChanges.Should().Be(VersionChange.NonBreaking);
        }

        [Fact]
        public void WhenUpdateCommandLaunchPointAndCommandNotExists_ThenThrows()
        {
            this.element
                .Invoking(x =>
                    x.UpdateCommandLaunchPoint("alaunchpointname", null, new List<string> { "acmdid" },
                        new List<string>(), this.element))
                .Should().Throw<AutomateException>()
                .WithMessage(
                    ExceptionMessages.PatternElement_CommandIdNotFound.Substitute("acmdid"));
        }

        [Fact]
        public void WhenUpdateCommandLaunchPointAndNotExists_ThenThrows()
        {
            this.pattern.AddCodeTemplate("atemplatename", "afullpath", "anextension");
            var command1 = this.pattern.AddCodeTemplateCommand("acommandname1", "atemplatename", false, "~/apath");

            this.element
                .Invoking(x =>
                    x.UpdateCommandLaunchPoint("alaunchpointname", null, new List<string> { command1.Id },
                        new List<string>(),
                        this.element))
                .Should().Throw<AutomateException>()
                .WithMessage(
                    ExceptionMessages.PatternElement_AutomationNotExistsByName.Substitute(
                        AutomationType.CommandLaunchPoint,
                        "alaunchpointname"));
        }

        [Fact]
        public void
            WhenUpdateCommandLaunchPointWithAddWildcardForCommandsElsewhereInPattern_ThenUpdatesWithAllCommands()
        {
            this.pattern.AddCodeTemplate("atemplatename", "afullpath", "anextension");
            var command1 = this.pattern.AddCodeTemplateCommand("acommandname1", "atemplatename", false, "~/apath");
            var command2 = this.pattern.AddCodeTemplateCommand("acommandname2", "atemplatename", false, "~/apath");
            this.element.AddCommandLaunchPoint("alaunchpointname", new List<string> { command1.Id }, this.pattern);

            var result = this.element.UpdateCommandLaunchPoint("alaunchpointname", "anewname",
                new List<string> { PatternElement.LaunchPointSelectionWildcard }, new List<string>(), this.pattern);

            result.Should().Be(this.element.Automation[0]);
            result.Name.Should().Be("anewname");
            result.Metadata[nameof(CommandLaunchPoint.CommandIds)].Should()
                .Be(new[] { command1.Id, command2.Id }.Join(CommandLaunchPoint.CommandIdDelimiter));
            this.element.Pattern.ToolkitVersion.LastChanges.Should().Be(VersionChange.NonBreaking);
        }

        [Fact]
        public void WhenUpdateCommandLaunchPointWithAddForCommandsElsewhereInPattern_ThenAddsThem()
        {
            this.pattern.AddCodeTemplate("atemplatename", "afullpath", "anextension");
            var command1 = this.pattern.AddCodeTemplateCommand("acommandname1", "atemplatename", false, "~/apath");
            var command2 = this.pattern.AddCodeTemplateCommand("acommandname2", "atemplatename", false, "~/apath");
            this.element.AddCommandLaunchPoint("alaunchpointname", new List<string> { command1.Id }, this.pattern);

            var result = this.element.UpdateCommandLaunchPoint("alaunchpointname", "anewname",
                new List<string> { command2.Id }, new List<string>(), this.element);

            result.Should().Be(this.element.Automation[0]);
            result.Name.Should().Be("anewname");
            result.Metadata[nameof(CommandLaunchPoint.CommandIds)].Should()
                .Be(new[] { command1.Id, command2.Id }.Join(CommandLaunchPoint.CommandIdDelimiter));
            this.element.Pattern.ToolkitVersion.LastChanges.Should().Be(VersionChange.NonBreaking);
        }

        [Fact]
        public void
            WhenUpdateCommandLaunchPointWithRemoveWildcardForCommandsElsewhereInPattern_ThenUpdatesWithAllCommands()
        {
            this.pattern.AddCodeTemplate("atemplatename", "afullpath", "anextension");
            var command1 = this.pattern.AddCodeTemplateCommand("acommandname1", "atemplatename", false, "~/apath");
            var command2 = this.pattern.AddCodeTemplateCommand("acommandname2", "atemplatename", false, "~/apath");
            this.element.AddCommandLaunchPoint("alaunchpointname", new List<string> { command1.Id, command2.Id },
                this.pattern);

            var result = this.element.UpdateCommandLaunchPoint("alaunchpointname", "anewname",
                new List<string> { command2.Id }, new List<string> { PatternElement.LaunchPointSelectionWildcard },
                this.pattern);

            result.Should().Be(this.element.Automation[0]);
            result.Name.Should().Be("anewname");
            result.Metadata[nameof(CommandLaunchPoint.CommandIds)].Should()
                .Be(new[] { command2.Id }.Join(CommandLaunchPoint.CommandIdDelimiter));
            this.element.Pattern.ToolkitVersion.LastChanges.Should().Be(VersionChange.NonBreaking);
        }

        [Fact]
        public void WhenUpdateCommandLaunchPointWithRemoveForCommandsElsewhereInPattern_ThenAddsThem()
        {
            this.pattern.AddCodeTemplate("atemplatename", "afullpath", "anextension");
            var command1 = this.pattern.AddCodeTemplateCommand("acommandname1", "atemplatename", false, "~/apath");
            var command2 = this.pattern.AddCodeTemplateCommand("acommandname2", "atemplatename", false, "~/apath");
            this.element.AddCommandLaunchPoint("alaunchpointname", new List<string> { command1.Id, command2.Id },
                this.pattern);

            var result = this.element.UpdateCommandLaunchPoint("alaunchpointname", "anewname",
                new List<string> { command2.Id }, new List<string> { command1.Id }, this.element);

            result.Should().Be(this.element.Automation[0]);
            result.Name.Should().Be("anewname");
            result.Metadata[nameof(CommandLaunchPoint.CommandIds)].Should()
                .Be(new[] { command2.Id }.Join(CommandLaunchPoint.CommandIdDelimiter));
            this.element.Pattern.ToolkitVersion.LastChanges.Should().Be(VersionChange.NonBreaking);
        }

        [Fact]
        public void WhenDeleteCodeTemplateAndNotExists_ThenThrows()
        {
            this.element
                .Invoking(x => x.DeleteCodeTemplate("anid", false))
                .Should().Throw<AutomateException>()
                .WithMessage(ExceptionMessages.PatternElement_CodeTemplateNotExistsById.Substitute("anid"));
        }

        [Fact]
        public void WhenDeleteCodeTemplate_ThenDeletesTemplate()
        {
            var codeTemplate = new CodeTemplate("atemplatename", "afullpath", "afileextension");
            this.element.AddCodeTemplate(codeTemplate);

            this.element.DeleteCodeTemplate(codeTemplate.Name, false);

            this.element.CodeTemplates.Should().BeEmpty();
            this.element.Pattern.ToolkitVersion.LastChanges.Should().Be(VersionChange.Breaking);
        }

        [Fact]
        public void WhenDeleteCodeTemplateAndNotIncludeReferencingCodeTemplateCommand_ThenDeletesTemplateOnly()
        {
            var codeTemplate = new CodeTemplate("atemplatename", "afullpath", "afileextension");
            this.element.AddCodeTemplate(codeTemplate);
            var command = this.element.AddCodeTemplateCommand("acommandname", codeTemplate.Name, false, "afilepath");

            this.element.DeleteCodeTemplate(codeTemplate.Name, false);

            this.element.CodeTemplates.Should().BeEmpty();
            this.element.Automation.Should().ContainSingle(x => x.Id == command.Id);
            this.element.Pattern.ToolkitVersion.LastChanges.Should().Be(VersionChange.Breaking);
        }

        [Fact]
        public void
            WhenDeleteCodeTemplateAndIncludeAutomationAndHasReferencingCodeTemplateCommand_ThenDeletesTemplateAndCommand()
        {
            var codeTemplate = new CodeTemplate("atemplatename", "afullpath", "afileextension");
            this.element.AddCodeTemplate(codeTemplate);
            this.element.AddCodeTemplateCommand("acommandname", codeTemplate.Name, false, "afilepath");

            this.element.DeleteCodeTemplate(codeTemplate.Name, true);

            this.element.CodeTemplates.Should().BeEmpty();
            this.element.Automation.Should().BeEmpty();
            this.element.Pattern.ToolkitVersion.LastChanges.Should().Be(VersionChange.Breaking);
        }

        [Fact]
        public void
            WhenDeleteCodeTemplateAndIncludeAutomationAndHasDedicatedReferencingLaunchPoint_ThenDeletesTemplateAndCommandAndLaunchPoint()
        {
            var codeTemplate = new CodeTemplate("atemplatename", "afullpath", "afileextension");
            this.element.AddCodeTemplate(codeTemplate);
            var command = this.element.AddCodeTemplateCommand("acommandname", codeTemplate.Name, false, "afilepath");
            this.element.AddCommandLaunchPoint("alaunchpointname", new List<string> { command.Id }, this.element);

            this.element.DeleteCodeTemplate(codeTemplate.Name, true);

            this.element.CodeTemplates.Should().BeEmpty();
            this.element.Automation.Should().BeEmpty();
            this.element.Pattern.ToolkitVersion.LastChanges.Should().Be(VersionChange.Breaking);
        }

        [Fact]
        public void
            WhenDeleteCodeTemplateAndIncludeAutomationAndHasReferencingLaunchPointToOtherCommands_ThenDeletesTemplateAndCommand()
        {
            var codeTemplate1 = new CodeTemplate("aname1", "afullpath", "afileextension");
            var codeTemplate2 = new CodeTemplate("aname2", "afullpath", "afileextension");
            this.element.AddCodeTemplate(codeTemplate1);
            this.element.AddCodeTemplate(codeTemplate2);
            var command1 = this.element.AddCodeTemplateCommand("acommandname1", codeTemplate1.Name, false, "afilepath");
            var command2 = this.element.AddCodeTemplateCommand("acommandname2", codeTemplate1.Name, false, "afilepath");
            var command3 = this.element.AddCodeTemplateCommand("acommandname3", codeTemplate2.Name, false, "afilepath");
            var launchPoint = this.element.AddCommandLaunchPoint("alaunchpointname",
                new List<string> { command1.Id, command2.Id, command3.Id }, this.element);

            this.element.DeleteCodeTemplate(codeTemplate1.Name, true);

            this.element.CodeTemplates.Should().ContainSingle(x => x.Id == codeTemplate2.Id);
            this.element.Automation.Should().HaveCount(2);
            this.element.Automation[0].Id.Should().Be(command3.Id);
            this.element.Automation[1].Id.Should().Be(launchPoint.Id);
            this.element.Automation[1].Metadata.Should().Contain(x =>
                x.Key == nameof(CommandLaunchPoint.CommandIds)
                && (string)x.Value == new List<string> { command3.Id }.Join(""));
            this.element.Pattern.ToolkitVersion.LastChanges.Should().Be(VersionChange.Breaking);
        }

        [Fact]
        public void WhenDeleteCommandLaunchPointAndNotExists_ThenThrows()
        {
            this.element
                .Invoking(x => x.DeleteCommandLaunchPoint("anid"))
                .Should().Throw<AutomateException>()
                .WithMessage(
                    ExceptionMessages.PatternElement_AutomationNotExistsById.Substitute(
                        AutomationType.CommandLaunchPoint,
                        "anid"));
        }

        [Fact]
        public void WhenDeleteCommandLaunchPoint_ThenDeletesLaunchPoint()
        {
            var codeTemplate = new CodeTemplate("atemplatename", "afullpath", "afileextension");
            this.element.AddCodeTemplate(codeTemplate);
            var command = this.element.AddCodeTemplateCommand("acommandname", codeTemplate.Name, false, "afilepath");
            var launchPoint =
                this.element.AddCommandLaunchPoint("alaunchpointname", new List<string> { command.Id }, this.element);

            this.element.DeleteCommandLaunchPoint(launchPoint.Id);

            this.element.CodeTemplates.Should().ContainSingle(x => x.Id == codeTemplate.Id);
            this.element.Automation.Should().ContainSingle(x => x.Id == command.Id);
            this.element.Pattern.ToolkitVersion.LastChanges.Should().Be(VersionChange.Breaking);
        }

        [Fact]
        public void WhenDeleteAutomationAndNotExists_ThenThrows()
        {
            this.element
                .Invoking(x => x.DeleteAutomation("anautomationname"))
                .Should().Throw<AutomateException>()
                .WithMessage(
                    ExceptionMessages.PatternElement_AutomationNotExistsByName.Substitute(AutomationType.Unknown,
                        "anautomationname"));
        }

        [Fact]
        public void WhenDeleteAutomationForCodeTemplateCommand_ThenDeletesCommand()
        {
            var codeTemplate = new CodeTemplate("atemplatename", "afullpath", "afileextension");
            this.element.AddCodeTemplate(codeTemplate);
            var command = this.element.AddCodeTemplateCommand("acommandname", codeTemplate.Name, false, "afilepath");
            this.element.AddCommandLaunchPoint("alaunchpointname", new List<string> { command.Id }, this.element);

            this.element.DeleteAutomation(command.Name);

            this.element.CodeTemplates.Should().ContainSingle(x => x.Id == codeTemplate.Id);
            this.element.Automation.Should().BeEmpty();
            this.element.Pattern.ToolkitVersion.LastChanges.Should().Be(VersionChange.Breaking);
        }

        [Fact]
        public void WhenDeleteAutomationForCliCommand_ThenDeletesCommand()
        {
            var command = this.element.AddCliCommand("acommandname", "anapp", null);
            this.element.AddCommandLaunchPoint("alaunchpointname", new List<string> { command.Id }, this.element);

            this.element.DeleteAutomation(command.Name);

            this.element.Automation.Should().BeEmpty();
            this.element.Pattern.ToolkitVersion.LastChanges.Should().Be(VersionChange.Breaking);
        }

        [Fact]
        public void WhenDeleteAutomationForLaunchPoint_ThenDeletesLaunchPoint()
        {
            var command = this.element.AddCliCommand("acommandname", "anapp", null);
            var launchPoint =
                this.element.AddCommandLaunchPoint("alaunchpointname", new List<string> { command.Id }, this.element);

            this.element.DeleteAutomation(launchPoint.Name);

            this.element.Automation.Should().ContainSingle(x => x.Id == command.Id);
            this.element.Pattern.ToolkitVersion.LastChanges.Should().Be(VersionChange.Breaking);
        }

        [Fact]
        public void WhenRenameAndDescribeWithEmptyName_ThenThrows()
        {
            this.element
                .Invoking(x => x.RenameAndDescribe(string.Empty))
                .Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage(ValidationMessages.InvalidNameIdentifier.Substitute("") + "*");
        }

        [Fact]
        public void WhenRenameAndDescribeWithInvalidName_ThenThrows()
        {
            this.element
                .Invoking(x => x.RenameAndDescribe("^aninvalidname^"))
                .Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage(ValidationMessages.InvalidNameIdentifier.Substitute("^aninvalidname^") + "*");
        }

        [Fact]
        public void WhenRenameAndDescribeWithNoDisplayName_ThenRenames()
        {
            this.element.RenameAndDescribe("OneTwoThree");

            this.element.Name.Should().Be("OneTwoThree");
            this.element.DisplayName.Should().Be("One Two Three");
            this.element.Pattern.ToolkitVersion.LastChanges.Should().Be(VersionChange.Breaking);
        }

        [Fact]
        public void WhenRenameAndDescribeWithDisplayName_ThenRenames()
        {
            this.element.RenameAndDescribe("anelementname", "adisplayname");

            this.element.Name.Should().Be("anelementname");
            this.element.DisplayName.Should().Be("adisplayname");
            this.element.Pattern.ToolkitVersion.LastChanges.Should().Be(VersionChange.NonBreaking);
        }

        [Fact]
        public void WhenRenameAndDescribeWithEmptyDisplayName_ThenThrows()
        {
            this.element
                .Invoking(x => x.RenameAndDescribe(null, ""))
                .Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void WhenRenameAndDescribe_ThenSetsDisplayName()
        {
            this.element.RenameAndDescribe(null, "adisplayname");

            this.element.Name.Should().Be("anelementname");
            this.element.DisplayName.Should().Be("adisplayname");
            this.element.Pattern.ToolkitVersion.LastChanges.Should().Be(VersionChange.NonBreaking);
        }

        [Fact]
        public void WhenRenameAndDescribeWithEmptyDescription_ThenThrows()
        {
            this.element
                .Invoking(x => x.RenameAndDescribe(null, null, ""))
                .Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void WhenRenameAndDescribeWithDescription_ThenSetDescriptions()
        {
            this.element.RenameAndDescribe(null, null, "adescription");

            this.element.Description.Should().Be("adescription");
            this.element.Pattern.ToolkitVersion.LastChanges.Should().Be(VersionChange.NonBreaking);
        }

        [Fact]
        public void WhenGetEditPathAndOnlyPattern_ThenReturnsPath()
        {
            var result = new PatternDefinition("apatternname").EditPath;

            result.Should().Be($"{{{this.pattern.Name}}}");
        }

        [Fact]
        public void WhenGetEditPathHasSomeDescendants_ThenReturnsPath()
        {
            var element2 = new Element("anelementname2");
            var element3 = new Element("anelementname3");
            element2.AddElement(element3);
            this.element.AddElement(element2);

            var result = element3.EditPath;

            result.Should().Be($"{{{this.pattern.Name}.anelementname.anelementname2.anelementname3}}");
        }
    }

    internal class TestPatternElement : Element
    {
        public TestPatternElement(string name) : base(name)
        {
        }
    }
}