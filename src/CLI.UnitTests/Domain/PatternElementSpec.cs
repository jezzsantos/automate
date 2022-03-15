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

        public PatternElementSpec()
        {
            this.element = new TestPatternElement("aname");
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
                    ExceptionMessages.AuthoringApplication_AttributeNameReserved.Format(
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
                    ExceptionMessages.AuthoringApplication_AttributeByNameExistsAsElement.Format(
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
                .WithMessage(ExceptionMessages.AuthoringApplication_AutomationByNameExists.Format("acommandname"));
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
            this.element.AddCommandLaunchPoint("alaunchpointname", new List<string> { command.Id }, this.element);

            this.element
                .Invoking(x => x.AddCommandLaunchPoint("alaunchpointname", new List<string> { command.Id }, this.element))
                .Should().Throw<AutomateException>()
                .WithMessage(ExceptionMessages.AuthoringApplication_AutomationByNameExists.Format("alaunchpointname"));
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
        public void WhenAddCommandLaunchPoint_TheAddsAutomationToElement()
        {
            var pattern = new PatternDefinition("apatternname");
            pattern.AttachCodeTemplate("atemplatename", "afullpath", "anextension");
            var command1 = pattern.AddCodeTemplateCommand("acommandname1", "atemplatename", false, "~/apath");
            var command2 = pattern.AddCodeTemplateCommand("acommandname2", "atemplatename", false, "~/apath");

            var result = this.element.AddCommandLaunchPoint("alaunchpointname", new List<string> { command1.Id, command2.Id }, pattern);

            result.Name.Should().Be("alaunchpointname");
            result.Metadata[nameof(CommandLaunchPoint.CommandIds)].Should().Be($"{command1.Id};{command2.Id}");
        }

        [Fact]
        public void WhenAddCommandLaunchPointAndNoName_TheAddsAutomationWithDefaultName()
        {
            var pattern = new PatternDefinition("apatternname");
            pattern.AttachCodeTemplate("atemplatename", "afullpath", "anextension");
            var command = pattern.AddCodeTemplateCommand("acommandname", "atemplatename", false, "~/apath");

            var result = this.element.AddCommandLaunchPoint(null, new List<string> { command.Id }, pattern);

            result.Name.Should().Be("LaunchPoint1");
            result.Metadata[nameof(CommandLaunchPoint.CommandIds)].Should().Be($"{command.Id}");
        }
    }

    internal class TestPatternElement : PatternElement
    {
        public TestPatternElement(string name) : base(name)
        {
        }

        public TestPatternElement(PersistableProperties properties, IPersistableFactory factory) : base(properties, factory)
        {
        }
    }
}