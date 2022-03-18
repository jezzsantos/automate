using System;
using System.Collections.Generic;
using Automate.CLI;
using Automate.CLI.Domain;
using Automate.CLI.Extensions;
using FluentAssertions;
using Xunit;
using Attribute = Automate.CLI.Domain.Attribute;

namespace CLI.UnitTests.Domain
{
    [Trait("Category", "Unit")]
    public class PatternElementSpec
    {
        private readonly TestPatternElement element;
        private readonly PatternDefinition pattern;

        public PatternElementSpec()
        {
            this.pattern = new PatternDefinition("apatternname");
            this.element = new TestPatternElement("aname");
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
                .WithMessage(ValidationMessages.InvalidNameIdentifier.Format("an invalid name") + "*");
        }

        [Fact]
        public void WhenAddAttributeAndAlreadyExists_ThenThrows()
        {
            this.element.AddAttribute("anattributename", null, false, null, null);

            this.element
                .Invoking(x => x.AddAttribute("anattributename", null, false, null, null))
                .Should().Throw<AutomateException>()
                .WithMessage(ExceptionMessages.PatternElement_AttributeByNameExists.Format("anattributename"));
        }

        [Fact]
        public void WhenAddAttributeWithReservedName_ThenThrows()
        {
            this.element
                .Invoking(x => x.AddAttribute(Attribute.ReservedAttributeNames[0], null, false, null, null))
                .Should().Throw<AutomateException>()
                .WithMessage(
                    ExceptionMessages.PatternElement_AttributeNameReserved.Format(
                        Attribute.ReservedAttributeNames[0]));
        }

        [Fact]
        public void WhenAddAttributeWithExistingNameOfElementOrCollection_ThenThrows()
        {
            this.element.AddElement("anelementname", null, null, false, ElementCardinality.Single);

            this.element
                .Invoking(x => x.AddAttribute("anelementname", null, false, null, null))
                .Should().Throw<AutomateException>()
                .WithMessage(
                    ExceptionMessages.PatternElement_AttributeByNameExistsAsElement.Format(
                        "anelementname"));
        }

        [Fact]
        public void WhenAddAttribute_TheAddsAttributeToElement()
        {
            var result = this.element.AddAttribute("anattributename", "string", false, "adefaultvalue", null);

            result.Name.Should().Be("anattributename");
            result.DataType.Should().Be("string");
            result.DefaultValue.Should().Be("adefaultvalue");
            result.IsRequired.Should().BeFalse();
            result.Choices.Should().BeNull();
            this.element.Pattern.ToolkitVersion.LastChanges.Should().Be(VersionChange.NonBreaking);
        }

        [Fact]
        public void WhenAddAttributeAndNoType_TheAddsAttributeWithDefaultType()
        {
            var result = this.element.AddAttribute("anattributename", null, false, null, null);

            result.Name.Should().Be("anattributename");
            result.DataType.Should().Be("string");
            result.DefaultValue.Should().BeNull();
            result.IsRequired.Should().BeFalse();
            result.Choices.Should().BeNull();
            this.element.Pattern.ToolkitVersion.LastChanges.Should().Be(VersionChange.NonBreaking);
        }

        [Fact]
        public void WhenDeleteAttribute_TheDeletesAttributeFromElement()
        {
            var attribute = this.element.AddAttribute("anattributename", "string", false, "adefaultvalue", null);

            var result = this.element.DeleteAttribute("anattributename");

            result.Id.Should().Be(attribute.Id);
            this.element.Attributes.Should().BeEmpty();
            this.element.Pattern.ToolkitVersion.LastChanges.Should().Be(VersionChange.Breaking);
        }

        [Fact]
        public void WhenAddElementAndAlreadyExists_ThenThrows()
        {
            this.element.AddElement("anelementname", null, null, false, ElementCardinality.Single);

            this.element
                .Invoking(x => x.AddElement("anelementname", null, null, false, ElementCardinality.Single))
                .Should().Throw<AutomateException>()
                .WithMessage(ExceptionMessages.PatternElement_ElementByNameExists.Format("anelementname"));
        }

        [Fact]
        public void WhenAddElementWithExistingNameOfAttribute_ThenThrows()
        {
            this.element.AddAttribute("anattributename", null, false, null, null);

            this.element
                .Invoking(x => x.AddElement("anattributename", null, null, false, ElementCardinality.Single))
                .Should().Throw<AutomateException>()
                .WithMessage(ExceptionMessages.PatternElement_ElementByNameExistsAsAttribute.Format("anattributename"));
        }

        [Fact]
        public void WhenAddElement_TheAddsElementToElement()
        {
            var result = this.element.AddElement("anelementname", "adisplayname", "adescription", false,
                ElementCardinality.Single);

            result.Name.Should().Be("anelementname");
            result.DisplayName.Should().Be("adisplayname");
            result.Description.Should().Be("adescription");
            result.IsCollection.Should().BeFalse();
            this.element.Pattern.ToolkitVersion.LastChanges.Should().Be(VersionChange.NonBreaking);
        }

        [Fact]
        public void WhenDeleteElement_TheDeletesElementFromElement()
        {
            var ele = this.element.AddElement("anelementname", null, null, false, ElementCardinality.Single);

            var result = this.element.DeleteElement("anelementname");

            result.Id.Should().Be(ele.Id);
            this.element.Elements.Should().BeEmpty();
            this.element.Pattern.ToolkitVersion.LastChanges.Should().Be(VersionChange.Breaking);
        }

        [Fact]
        public void WhenAttachCodeTemplateAndTemplateWithSameName_ThenThrows()
        {
            this.element.AttachCodeTemplate("atemplatename", "afullpath", "anextension");

            this.element
                .Invoking(x => x.AttachCodeTemplate("atemplatename", "afullpath", "anextension"))
                .Should().Throw<AutomateException>()
                .WithMessage(ExceptionMessages.PatternElement_CodeTemplateByNameExists.Format("atemplatename"));
        }

        [Fact]
        public void WhenAttachCodeTemplate_ThenTemplateAdded()
        {
            var result = this.element.AttachCodeTemplate("atemplatename", "afullpath", "anextension");

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
                .WithMessage(ExceptionMessages.PatternElement_CodeTemplateNoFound.Format("atemplatename"));
        }

        [Fact]
        public void WhenAddCodeTemplateCommandAndAlreadyExists_ThenThrows()
        {
            this.element.AttachCodeTemplate("atemplatename", "afullpath", "anextension");
            this.element.AddCodeTemplateCommand("acommandname", "atemplatename", false, "~/apath");

            this.element
                .Invoking(x => x.AddCodeTemplateCommand("acommandname", "atemplatename", false, "~/apath"))
                .Should().Throw<AutomateException>()
                .WithMessage(ExceptionMessages.PatternElement_AutomationByNameExists.Format("acommandname"));
        }

        [Fact]
        public void WhenAddCodeTemplateCommand_TheAddsAutomationToElement()
        {
            this.element.AttachCodeTemplate("atemplatename", "afullpath", "anextension");

            var result =
                this.element.AddCodeTemplateCommand("acommandname", "atemplatename", false, "~/apath");

            result.Name.Should().Be("acommandname");
            result.Metadata[nameof(CodeTemplateCommand.IsTearOff)].Should().Be(false);
            result.Metadata[nameof(CodeTemplateCommand.FilePath)].Should().Be("~/apath");
            this.element.Pattern.ToolkitVersion.LastChanges.Should().Be(VersionChange.NonBreaking);
        }

        [Fact]
        public void WhenAddCodeTemplateCommandAndNoName_TheAddsAutomationWithDefaultName()
        {
            this.element.AttachCodeTemplate("atemplatename", "afullpath", "anextension");

            var result = this.element.AddCodeTemplateCommand(null, "atemplatename", false, "~/apath");

            result.Name.Should().Be("CodeTemplateCommand1");
            result.Metadata[nameof(CodeTemplateCommand.IsTearOff)].Should().Be(false);
            result.Metadata[nameof(CodeTemplateCommand.FilePath)].Should().Be("~/apath");
        }

        [Fact]
        public void WhenAddCommandLaunchPointAndAlreadyExists_ThenThrows()
        {
            this.element.AttachCodeTemplate("atemplatename", "afullpath", "anextension");
            var command = this.element.AddCodeTemplateCommand("acommandname", "atemplatename", false, "~/apath");
            this.element.AddCommandLaunchPoint("alaunchpointname", new List<string> { command.Id }, this.pattern);

            this.element
                .Invoking(x => x.AddCommandLaunchPoint("alaunchpointname", new List<string> { command.Id }, this.pattern))
                .Should().Throw<AutomateException>()
                .WithMessage(ExceptionMessages.PatternElement_AutomationByNameExists.Format("alaunchpointname"));
        }

        [Fact]
        public void WhenAddCommandLaunchPointAndNoCommands_ThenThrows()
        {
            this.element
                .Invoking(x => x.AddCommandLaunchPoint("alaunchpointname", new List<string>(), this.pattern))
                .Should().Throw<AutomateException>()
                .WithMessage(ExceptionMessages.PatternElement_NoCommandIds);
        }

        [Fact]
        public void WhenAddCommandLaunchPointAndCommandNotExists_ThenThrows()
        {
            this.element
                .Invoking(x =>
                    x.AddCommandLaunchPoint(null, new List<string> { "acmdid" }, new PatternDefinition("apatternname")))
                .Should().Throw<AutomateException>()
                .WithMessage(
                    ExceptionMessages.PatternElement_CommandIdNotFound.Format("acmdid"));
        }

        [Fact]
        public void WhenAddCommandLaunchPointWithIdsFromAnywhereInPattern_TheAddsAutomationToElement()
        {
            this.pattern.AttachCodeTemplate("atemplatename", "afullpath", "anextension");
            var command1 = this.pattern.AddCodeTemplateCommand("acommandname1", "atemplatename", false, "~/apath");
            var command2 = this.pattern.AddCodeTemplateCommand("acommandname2", "atemplatename", false, "~/apath");

            var result = this.element.AddCommandLaunchPoint("alaunchpointname", new List<string> { command1.Id, command2.Id }, this.pattern);

            result.Name.Should().Be("alaunchpointname");
            result.Metadata[nameof(CommandLaunchPoint.CommandIds)].Should().Be($"{command1.Id};{command2.Id}");
            this.element.Pattern.ToolkitVersion.LastChanges.Should().Be(VersionChange.NonBreaking);
        }

        [Fact]
        public void WhenAddCommandLaunchPointToPatternWithWildcard_TheAddsAllAutomationFromPatternToElement()
        {
            this.pattern.AttachCodeTemplate("atemplatename", "afullpath", "anextension");
            var command1 = this.pattern.AddCodeTemplateCommand("acommandname1", "atemplatename", false, "~/apath");
            var command2 = this.pattern.AddCodeTemplateCommand("acommandname2", "atemplatename", false, "~/apath");

            var result = this.pattern.AddCommandLaunchPoint("alaunchpointname", new List<string> { PatternElement.LaunchPointSelectionWildcard }, this.pattern);

            result.Name.Should().Be("alaunchpointname");
            result.Metadata[nameof(CommandLaunchPoint.CommandIds)].Should().Be($"{command1.Id};{command2.Id}");
            this.pattern.ToolkitVersion.LastChanges.Should().Be(VersionChange.NonBreaking);
        }

        [Fact]
        public void WhenAddCommandLaunchPointToElementWithWildcard_TheAddsAllAutomationFromElementToElement()
        {
            this.element.AttachCodeTemplate("atemplatename", "afullpath", "anextension");
            var command1 = this.element.AddCodeTemplateCommand("acommandname1", "atemplatename", false, "~/apath");
            var command2 = this.element.AddCodeTemplateCommand("acommandname2", "atemplatename", false, "~/apath");

            var result = this.element.AddCommandLaunchPoint("alaunchpointname", new List<string> { PatternElement.LaunchPointSelectionWildcard }, this.pattern);

            result.Name.Should().Be("alaunchpointname");
            result.Metadata[nameof(CommandLaunchPoint.CommandIds)].Should().Be($"{command1.Id};{command2.Id}");
            this.pattern.ToolkitVersion.LastChanges.Should().Be(VersionChange.NonBreaking);
        }

        [Fact]
        public void WhenAddCommandLaunchPointAndNoName_TheAddsAutomationWithDefaultName()
        {
            this.element.AttachCodeTemplate("atemplatename", "afullpath", "anextension");
            var command = this.element.AddCodeTemplateCommand("acommandname", "atemplatename", false, "~/apath");

            var result = this.element.AddCommandLaunchPoint(null, new List<string> { command.Id }, this.pattern);

            result.Name.Should().Be("LaunchPoint2");
            result.Should().Be(this.element.Automation[1]);
            result.Metadata[nameof(CommandLaunchPoint.CommandIds)].Should().Be($"{command.Id}");
        }

        [Fact]
        public void WhenUpdateCommandLaunchPointAndNoCommands_ThenThrows()
        {
            this.element
                .Invoking(x => x.UpdateCommandLaunchPoint("alaunchpointname", new List<string>(), this.element, this.pattern))
                .Should().Throw<AutomateException>()
                .WithMessage(ExceptionMessages.PatternElement_NoCommandIds);
        }

        [Fact]
        public void WhenUpdateCommandLaunchPointAndCommandNotExists_ThenThrows()
        {
            this.element
                .Invoking(x => x.UpdateCommandLaunchPoint("alaunchpointname", new List<string> { "acmdid" }, this.element, this.pattern))
                .Should().Throw<AutomateException>()
                .WithMessage(
                    ExceptionMessages.PatternElement_CommandIdNotFound.Format("acmdid"));
        }

        [Fact]
        public void WhenUpdateCommandLaunchPointAndNotExists_ThenThrows()
        {
            this.pattern.AttachCodeTemplate("atemplatename", "afullpath", "anextension");
            var command1 = this.pattern.AddCodeTemplateCommand("acommandname1", "atemplatename", false, "~/apath");

            this.element
                .Invoking(x => x.UpdateCommandLaunchPoint("alaunchpointname", new List<string> { command1.Id }, this.element, this.pattern))
                .Should().Throw<AutomateException>()
                .WithMessage(ExceptionMessages.PatternElement_AutomationNotExistsByName.Format("alaunchpointname"));
        }

        [Fact]
        public void WhenUpdateCommandLaunchPoint_ThenAddsCommands()
        {
            this.pattern.AttachCodeTemplate("atemplatename", "afullpath", "anextension");
            var command1 = this.pattern.AddCodeTemplateCommand("acommandname1", "atemplatename", false, "~/apath");
            var command2 = this.pattern.AddCodeTemplateCommand("acommandname2", "atemplatename", false, "~/apath");
            this.element.AddCommandLaunchPoint("alaunchpointname", new List<string> { command1.Id }, this.pattern);

            var result = this.element.UpdateCommandLaunchPoint("alaunchpointname", new List<string> { command2.Id }, this.element, this.pattern);

            result.Should().Be(this.element.Automation[0]);
            result.Metadata[nameof(CommandLaunchPoint.CommandIds)].Should().Be(new[] { command1.Id, command2.Id }.Join(CommandLaunchPoint.CommandIdDelimiter));
        }
    }

    internal class TestPatternElement : Element
    {
        public TestPatternElement(string name) : base(name)
        {
        }
    }
}