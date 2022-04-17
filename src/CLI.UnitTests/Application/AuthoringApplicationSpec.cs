using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Automate.CLI;
using Automate.CLI.Application;
using Automate.CLI.Domain;
using Automate.CLI.Extensions;
using Automate.CLI.Infrastructure;
using FluentAssertions;
using Moq;
using Xunit;

namespace CLI.UnitTests.Application
{
    [Trait("Category", "Unit")]
    public class AuthoringApplicationSpec
    {
        private readonly AuthoringApplication application;
        private readonly Mock<IPatternToolkitPackager> builder;
        private readonly Mock<IFilePathResolver> filePathResolver;
        private readonly Mock<IPatternPathResolver> patternPathResolver;
        private readonly PatternStore store;
        private readonly Mock<ITextTemplatingEngine> textTemplatingEngine;

        public AuthoringApplicationSpec()
        {
            this.filePathResolver = new Mock<IFilePathResolver>();
            this.filePathResolver.Setup(pr => pr.CreatePath(It.IsAny<string>(), It.IsAny<string>()))
                .Returns("afullpath");
            this.filePathResolver.Setup(pr => pr.GetFileExtension(It.IsAny<string>()))
                .Returns("anextension");
            this.filePathResolver.Setup(pr => pr.ExistsAtPath(It.IsAny<string>()))
                .Returns(true);
            this.filePathResolver.Setup(pr => pr.GetFileAtPath(It.IsAny<string>()))
                .Returns(new Mock<IFile>().Object);
            this.patternPathResolver = new Mock<IPatternPathResolver>();
            this.patternPathResolver.Setup(ppr => ppr.Resolve(It.IsAny<PatternDefinition>(), It.IsAny<string>()))
                .Returns((PatternDefinition model, string _) => model);
            this.textTemplatingEngine = new Mock<ITextTemplatingEngine>();
            this.textTemplatingEngine.Setup(tte => tte.Transform(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SolutionItem>()))
                .Returns("anoutput");
            this.textTemplatingEngine.Setup(tte => tte.Transform(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IDictionary>()))
                .Returns("anoutput");
            var repo = new MemoryRepository();
            this.store = new PatternStore(repo, repo);
            this.builder = new Mock<IPatternToolkitPackager>();
            this.application =
                new AuthoringApplication(this.store, this.filePathResolver.Object, this.patternPathResolver.Object,
                    this.builder.Object, this.textTemplatingEngine.Object);
        }

        [Fact]
        public void WhenConstructed_ThenPropertiesAssigned()
        {
            this.application.CurrentPatternId.Should().BeNull();
        }

        [Fact]
        public void WhenCreateNewPatternAndAlreadyExists_ThenThrows()
        {
            this.application.CreateNewPattern("apatternname");

            this.application
                .Invoking(x => x.CreateNewPattern("apatternname"))
                .Should().Throw<AutomateException>()
                .WithMessage(ExceptionMessages.PatternStore_FoundNamed.Format("apatternname"));
        }

        [Fact]
        public void WhenCreateNewPatternAndNotExists_ThenExists()
        {
            this.application.CreateNewPattern("apatternname");

            this.store.GetCurrent().Should().NotBeNull();
            this.application.CurrentPatternId.Should().NotBeNull();
        }

        [Fact]
        public void WhenSwitchCurrentPatternAndNotExists_ThenThrows()
        {
            this.store.DestroyAll();

            this.application
                .Invoking(x => x.SwitchCurrentPattern("aname"))
                .Should().Throw<AutomateException>()
                .WithMessage(
                    ExceptionMessages.PatternStore_NotFoundAtLocationWithId.Format("aname",
                        MemoryRepository.InMemoryLocation));
        }

        [Fact]
        public void WhenSwitchCurrentPattern_ThenCurrentIsChanged()
        {
            this.application.CreateNewPattern("aname1");
            this.application.CreateNewPattern("aname2");

            this.application.SwitchCurrentPattern("aname1");

            this.store.GetCurrent().Should().NotBeNull();
            this.application.CurrentPatternName.Should().Be("aname1");
        }

        [Fact]
        public void WhenAttachCodeTemplateAndTemplateNotExists_ThenThrows()
        {
            this.store.Create(new PatternDefinition("aname"));
            this.filePathResolver.Setup(pr => pr.ExistsAtPath(It.IsAny<string>()))
                .Returns(false);

            this.application
                .Invoking(x => x.AttachCodeTemplate("arootpath", "arelativepath", "atemplatename", null))
                .Should().Throw<AutomateException>()
                .WithMessage(
                    ExceptionMessages.AuthoringApplication_CodeTemplate_NotFoundAtLocation.Format("arootpath",
                        "arelativepath"));
        }

        [Fact]
        public void WhenAttachCodeTemplateAndCurrentPatternNotExists_ThenThrows()
        {
            this.application
                .Invoking(x => x.AttachCodeTemplate("arootpath", "arelativepath", "atemplatename", null))
                .Should().Throw<AutomateException>()
                .WithMessage(ExceptionMessages.AuthoringApplication_NoCurrentPattern);
        }

        [Fact]
        public void WhenAttachCodeTemplate_ThenTemplateAdded()
        {
            this.store.Create(new PatternDefinition("aname"));

            var result = this.application.AttachCodeTemplate("arootpath", "arelativepath", "atemplatename", null);

            result.Template.Name.Should().Be("atemplatename");
            result.Template.Metadata.OriginalFilePath.Should().Be("afullpath");
            result.Location.Should().Be(MemoryRepository.InMemoryLocation);
            this.store.GetCurrent().CodeTemplates.Single().Name.Should().Be("atemplatename");
            this.store.GetCurrent().CodeTemplates.Single().Metadata.OriginalFilePath.Should().Be("afullpath");
        }

        [Fact]
        public void WhenListCodeTemplatesAndCurrentPatternNotExists_ThenThrows()
        {
            this.application
                .Invoking(x => x.ListCodeTemplates(null))
                .Should().Throw<AutomateException>()
                .WithMessage(ExceptionMessages.AuthoringApplication_NoCurrentPattern);
        }

        [Fact]
        public void WhenListCodeTemplates_ThenListsTemplates()
        {
            this.store.Create(new PatternDefinition("aname"));
            this.application.AttachCodeTemplate("arootpath", "arelativepath", "atemplatename1", null);
            this.application.AttachCodeTemplate("arootpath", "arelativepath", "atemplatename2", null);
            this.application.AttachCodeTemplate("arootpath", "arelativepath", "atemplatename3", null);

            var result = this.application.ListCodeTemplates(null);

            result.Should().Contain(template => template.Name == "atemplatename1");
            result.Should().Contain(template => template.Name == "atemplatename2");
            result.Should().Contain(template => template.Name == "atemplatename3");
        }

        [Fact]
        public void WhenAddAttributeAndCurrentPatternNotExists_ThenThrows()
        {
            this.application
                .Invoking(x => x.AddAttribute("anattributename", null, null, false, null, null))
                .Should().Throw<AutomateException>()
                .WithMessage(ExceptionMessages.AuthoringApplication_NoCurrentPattern);
        }

        [Fact]
        public void WhenAddAttribute_TheAddsAttributeToPattern()
        {
            this.application.CreateNewPattern("apatternname");

            var result = this.application.AddAttribute("anattributename", "string", "adefaultvalue", false, null, null);

            var attribute = this.store.GetCurrent().Attributes.Single();
            attribute.Name.Should().Be("anattributename");
            attribute.DataType.Should().Be("string");
            attribute.DefaultValue.Should().Be("adefaultvalue");
            attribute.IsRequired.Should().BeFalse();
            attribute.Choices.Should().BeEmpty();
            result.Parent.Id.Should().Be(this.store.GetCurrent().Id);
        }

        [Fact]
        public void WhenAddAttributeAndParentNotExists_ThenThrows()
        {
            this.patternPathResolver.Setup(ppr => ppr.Resolve(It.IsAny<PatternDefinition>(), It.IsAny<string>()))
                .Returns((PatternDefinition)null);

            this.application.CreateNewPattern("apatternname");

            this.application
                .Invoking(x =>
                    x.AddAttribute("anattributename", null, null, false, null, "anunknownparent"))
                .Should().Throw<AutomateException>()
                .WithMessage(
                    ExceptionMessages.AuthoringApplication_PathExpressionNotFound.Format("anunknownparent"));
        }

        [Fact]
        public void WhenAddAttributeAndParentIsElement_ThenAddsAttributeToElement()
        {
            this.application.CreateNewPattern("apatternname");
            this.application.AddElement("anelementname", null, null, false, ElementCardinality.Single, null);
            var parentElement = new Element("anelementname");
            this.patternPathResolver
                .Setup(ppr => ppr.Resolve(It.IsAny<PatternDefinition>(), "{apatternname.anelementname}"))
                .Returns(parentElement);

            var result = this.application.AddAttribute("anattributename", null, null, false, null,
                "{apatternname.anelementname}");

            var attribute = result.Parent.Attributes.Single();
            attribute.Name.Should().Be("anattributename");
            result.Parent.Id.Should().Be(parentElement.Id);
        }

        [Fact]
        public void WhenDeleteAttributeAndCurrentPatternNotExists_ThenThrows()
        {
            this.application
                .Invoking(x => x.DeleteAttribute("anattributename", null))
                .Should().Throw<AutomateException>()
                .WithMessage(ExceptionMessages.AuthoringApplication_NoCurrentPattern);
        }

        [Fact]
        public void WhenDeleteAttribute_TheDeletesAttributeFromPattern()
        {
            this.application.CreateNewPattern("apatternname");
            this.application.AddAttribute("anattributename", "string", "adefaultvalue", false, null, null);

            var result = this.application.DeleteAttribute("anattributename", null);

            this.store.GetCurrent().Attributes.Should().BeEmpty();
            result.Parent.Id.Should().Be(this.store.GetCurrent().Id);
        }

        [Fact]
        public void WhenDeleteAttributeAndParentNotExists_ThenThrows()
        {
            this.patternPathResolver.Setup(ppr => ppr.Resolve(It.IsAny<PatternDefinition>(), It.IsAny<string>()))
                .Returns((PatternDefinition)null);

            this.application.CreateNewPattern("apatternname");

            this.application
                .Invoking(x =>
                    x.DeleteAttribute("anattributename", "anunknownparent"))
                .Should().Throw<AutomateException>()
                .WithMessage(
                    ExceptionMessages.AuthoringApplication_PathExpressionNotFound.Format("anunknownparent"));
        }

        [Fact]
        public void WhenDeleteAttributeAndParentIsElement_ThenDeletesAttributeFromElement()
        {
            this.application.CreateNewPattern("apatternname");
            var (_, parentElement) = this.application.AddElement("anelementname", null, null, false, ElementCardinality.Single, null);
            this.patternPathResolver
                .Setup(ppr => ppr.Resolve(It.IsAny<PatternDefinition>(), "{apatternname.anelementname}"))
                .Returns(parentElement);
            this.application.AddAttribute("anattributename", "string", "adefaultvalue", false, null, "{apatternname.anelementname}");

            var result = this.application.DeleteAttribute("anattributename", "{apatternname.anelementname}");

            result.Parent.Attributes.Should().BeEmpty();
            result.Parent.Id.Should().Be(parentElement.Id);
        }

        [Fact]
        public void WhenAddElementAndCurrentPatternNotExists_ThenThrows()
        {
            this.application
                .Invoking(x => x.AddElement("anelementname", null, null, false, ElementCardinality.Single, null))
                .Should().Throw<AutomateException>()
                .WithMessage(ExceptionMessages.AuthoringApplication_NoCurrentPattern);
        }

        [Fact]
        public void WhenAddElementAndParentNotExists_ThenThrows()
        {
            this.patternPathResolver.Setup(ppr => ppr.Resolve(It.IsAny<PatternDefinition>(), It.IsAny<string>()))
                .Returns((PatternDefinition)null);

            this.application.CreateNewPattern("apatternname");

            this.application
                .Invoking(x =>
                    x.AddElement("anelementname", null, null, false, ElementCardinality.Single, "anunknownparent"))
                .Should().Throw<AutomateException>()
                .WithMessage(
                    ExceptionMessages.AuthoringApplication_PathExpressionNotFound.Format("anunknownparent"));
        }

        [Fact]
        public void WhenAddElement_TheAddsElementToPattern()
        {
            this.application.CreateNewPattern("apatternname");

            var result = this.application.AddElement("anelementname", "adisplayname", "adescription", false,
                ElementCardinality.Single, null);

            var element = this.store.GetCurrent().Elements.Single();
            element.Name.Should().Be("anelementname");
            element.DisplayName.Should().Be("adisplayname");
            element.Description.Should().Be("adescription");
            element.IsCollection.Should().BeFalse();
            result.Parent.Id.Should().Be(this.store.GetCurrent().Id);
        }

        [Fact]
        public void WhenAddElementAndParentIsElement_ThenAddsElementToElement()
        {
            this.application.CreateNewPattern("apatternname");
            this.application.AddElement("aparentelementname", null, null, false, ElementCardinality.Single, null);
            var parentElement = new Element("aparentelementname");
            this.patternPathResolver.Setup(ppr =>
                    ppr.Resolve(It.IsAny<PatternDefinition>(), "{apatternname.aparentelementname}"))
                .Returns(parentElement);

            var result =
                this.application.AddElement("achildelementname", null, null, false, ElementCardinality.Single,
                    "{apatternname.aparentelementname}");

            var element = result.Parent.Elements.Single();
            element.Name.Should().Be("achildelementname");
            result.Parent.Id.Should().Be(parentElement.Id);
        }

        [Fact]
        public void WhenDeleteElementAndCurrentPatternNotExists_ThenThrows()
        {
            this.application
                .Invoking(x => x.DeleteElement("anelementname", null))
                .Should().Throw<AutomateException>()
                .WithMessage(ExceptionMessages.AuthoringApplication_NoCurrentPattern);
        }

        [Fact]
        public void WhenDeleteElement_TheDeletesElementFromPattern()
        {
            this.application.CreateNewPattern("apatternname");
            this.application.AddElement("anelementname", null, null, false, ElementCardinality.Single, null);

            var result = this.application.DeleteElement("anelementname", null);

            this.store.GetCurrent().Elements.Should().BeEmpty();
            result.Parent.Id.Should().Be(this.store.GetCurrent().Id);
        }

        [Fact]
        public void WhenDeleteElementAndParentNotExists_ThenThrows()
        {
            this.patternPathResolver.Setup(ppr => ppr.Resolve(It.IsAny<PatternDefinition>(), It.IsAny<string>()))
                .Returns((PatternDefinition)null);

            this.application.CreateNewPattern("apatternname");

            this.application
                .Invoking(x =>
                    x.DeleteElement("anelementname", "anunknownparent"))
                .Should().Throw<AutomateException>()
                .WithMessage(
                    ExceptionMessages.AuthoringApplication_PathExpressionNotFound.Format("anunknownparent"));
        }

        [Fact]
        public void WhenDeleteElementAndParentIsElement_ThenDeletesElementFromElement()
        {
            this.application.CreateNewPattern("apatternname");
            var (_, parentElement) = this.application.AddElement("anelementname", null, null, false, ElementCardinality.Single, null);
            this.patternPathResolver
                .Setup(ppr => ppr.Resolve(It.IsAny<PatternDefinition>(), "{apatternname.anelementname}"))
                .Returns(parentElement);
            this.application.AddElement("anelementname2", null, null, false, ElementCardinality.Single, "{apatternname.anelementname}");

            var result = this.application.DeleteElement("anelementname2", "{apatternname.anelementname}");

            result.Parent.Elements.Should().BeEmpty();
            result.Parent.Id.Should().Be(parentElement.Id);
        }

        [Fact]
        public void WhenAddCodeTemplateCommandAndCurrentPatternNotExists_ThenThrows()
        {
            this.application
                .Invoking(x => x.AddCodeTemplateCommand("acommandname", "atemplatename", false, "~/apath", null))
                .Should().Throw<AutomateException>()
                .WithMessage(ExceptionMessages.AuthoringApplication_NoCurrentPattern);
        }

        [Fact]
        public void WhenAddCodeTemplateCommandAndCodeTemplateNotExistsOnPattern_ThenThrows()
        {
            this.application.CreateNewPattern("apatternname");

            this.application
                .Invoking(x => x.AddCodeTemplateCommand("atemplatename", "acommandname", false, "~/apath", null))
                .Should().Throw<AutomateException>()
                .WithMessage(ExceptionMessages.AuthoringApplication_CodeTemplateNotExistsRoot.Format("atemplatename"));
        }

        [Fact]
        public void WhenAddCodeTemplateCommandAndCodeTemplateNotExistsOnDescendant_ThenThrows()
        {
            this.application.CreateNewPattern("apatternname");

            this.application
                .Invoking(x => x.AddCodeTemplateCommand("atemplatename", "acommandname", false, "~/apath", "{apatternname.anelementname}"))
                .Should().Throw<AutomateException>()
                .WithMessage(ExceptionMessages.AuthoringApplication_CodeTemplateNotExistsElement.Format("atemplatename", "{apatternname.anelementname}"));
        }

        [Fact]
        public void WhenAddCodeTemplateCommandAndParentNotExists_ThenThrows()
        {
            this.patternPathResolver.Setup(ppr => ppr.Resolve(It.IsAny<PatternDefinition>(), It.IsAny<string>()))
                .Returns((PatternDefinition)null);

            this.application.CreateNewPattern("apatternname");

            this.application
                .Invoking(x =>
                    x.AddCodeTemplateCommand("atemplatename", "acommandname", false, "~/apath", "anunknownparent"))
                .Should().Throw<AutomateException>()
                .WithMessage(
                    ExceptionMessages.AuthoringApplication_PathExpressionNotFound.Format("anunknownparent"));
        }

        [Fact]
        public void WhenAddCodeTemplateCommand_TheAddsAutomationToPattern()
        {
            this.application.CreateNewPattern("apatternname");
            this.application.AttachCodeTemplate("arootpath", "arelativepath", "atemplatename", null);

            var result =
                this.application.AddCodeTemplateCommand("atemplatename", "acommandname", false, "~/apath", null);

            var automation = this.store.GetCurrent().Automation.Single();
            automation.Name.Should().Be("acommandname");
            automation.Metadata[nameof(CodeTemplateCommand.IsTearOff)].Should().Be(false);
            automation.Metadata[nameof(CodeTemplateCommand.FilePath)].Should().Be("~/apath");
            result.Id.Should().Be(automation.Id);
        }

        [Fact]
        public void WhenAddCodeTemplateCommandOnDescendantElement_ThenAddsAutomationToElement()
        {
            this.application.CreateNewPattern("apatternname");
            this.application.AddElement("anelementname", null, null, false, ElementCardinality.Single, null);
            this.patternPathResolver.Setup(ppr =>
                    ppr.Resolve(It.IsAny<PatternDefinition>(), "{apatternname.anelementname}"))
                .Returns(this.application.GetCurrentPattern().Elements.Single);
            this.application.AttachCodeTemplate("arootpath", "arelativepath", "atemplatename", "{apatternname.anelementname}");

            var result = this.application.AddCodeTemplateCommand("atemplatename", "acommandname", false, "~/apath",
                "{apatternname.anelementname}");

            result.Name.Should().Be("acommandname");
            this.application.GetCurrentPattern().Elements.Single().Automation.Single().Id.Should().Be(result.Id);
        }

        [Fact]
        public void WhenUpdateCodeTemplateCommandAndCurrentPatternNotExists_ThenThrows()
        {
            this.application
                .Invoking(x => x.UpdateCodeTemplateCommand("acommandname", null, null, null, null))
                .Should().Throw<AutomateException>()
                .WithMessage(ExceptionMessages.AuthoringApplication_NoCurrentPattern);
        }

        [Fact]
        public void WhenCodeTemplateCommandAndParentNotExists_ThenThrows()
        {
            this.patternPathResolver.Setup(ppr => ppr.Resolve(It.IsAny<PatternDefinition>(), It.IsAny<string>()))
                .Returns((PatternDefinition)null);

            this.application.CreateNewPattern("apatternname");

            this.application
                .Invoking(x =>
                    x.UpdateCodeTemplateCommand("acommandname", null, null, null, "anunknownparent"))
                .Should().Throw<AutomateException>()
                .WithMessage(
                    ExceptionMessages.AuthoringApplication_PathExpressionNotFound.Format("anunknownparent"));
        }

        [Fact]
        public void WhenUpdateCodeTemplateCommand_TheUpdatesAutomationOnPattern()
        {
            this.application.CreateNewPattern("apatternname");
            this.application.AttachCodeTemplate("arootpath", "arelativepath", "atemplatename", null);
            var command = this.application.AddCodeTemplateCommand("atemplatename", "acommandname", false, "~/apath", null);

            var result =
                this.application.UpdateCodeTemplateCommand(command.Name, "anewname", true, "anewpath", null);

            var automation = this.store.GetCurrent().Automation.Last();
            automation.Name.Should().Be("anewname");
            automation.Metadata[nameof(CodeTemplateCommand.IsTearOff)].Should().Be(true);
            automation.Metadata[nameof(CodeTemplateCommand.FilePath)].Should().Be("anewpath");
            result.Id.Should().Be(automation.Id);
        }

        [Fact]
        public void WhenUpdateCodeTemplateCommandOnDescendantElement_ThenUpdatesAutomationOnElement()
        {
            this.application.CreateNewPattern("apatternname");
            this.application.AddElement("anelementname", null, null, false, ElementCardinality.Single, null);
            this.patternPathResolver.Setup(ppr =>
                    ppr.Resolve(It.IsAny<PatternDefinition>(), "{apatternname.anelementname}"))
                .Returns(this.application.GetCurrentPattern().Elements.First);
            this.application.AttachCodeTemplate("arootpath", "arelativepath", "atemplatename", "{apatternname.anelementname}");
            var command = this.application.AddCodeTemplateCommand("atemplatename", "acommandname", false, "~/apath", "{apatternname.anelementname}");

            var result =
                this.application.UpdateCodeTemplateCommand(command.Name, "anewname", true, "anewpath", "{apatternname.anelementname}");

            var automation = this.store.GetCurrent().Elements.Single().Automation.Last();
            automation.Name.Should().Be("anewname");
            automation.Metadata[nameof(CodeTemplateCommand.IsTearOff)].Should().Be(true);
            automation.Metadata[nameof(CodeTemplateCommand.FilePath)].Should().Be("anewpath");
            result.Id.Should().Be(automation.Id);
        }

        [Fact]
        public void WhenAddCliCommandAndCurrentPatternNotExists_ThenThrows()
        {
            this.application
                .Invoking(x => x.AddCliCommand("anapplicationname", null, "acommandname", null))
                .Should().Throw<AutomateException>()
                .WithMessage(ExceptionMessages.AuthoringApplication_NoCurrentPattern);
        }

        [Fact]
        public void WhenAddCliCommandAndParentNotExists_ThenThrows()
        {
            this.patternPathResolver.Setup(ppr => ppr.Resolve(It.IsAny<PatternDefinition>(), It.IsAny<string>()))
                .Returns((PatternDefinition)null);

            this.application.CreateNewPattern("apatternname");

            this.application
                .Invoking(x =>
                    x.AddCliCommand("anapplicationname", null, "acommandname", "anunknownparent"))
                .Should().Throw<AutomateException>()
                .WithMessage(
                    ExceptionMessages.AuthoringApplication_PathExpressionNotFound.Format("anunknownparent"));
        }

        [Fact]
        public void WhenAddCliCommand_TheAddsAutomationToPattern()
        {
            this.application.CreateNewPattern("apatternname");
            this.application.AttachCodeTemplate("arootpath", "arelativepath", "atemplatename", null);

            var result =
                this.application.AddCliCommand("anapplicationname", "anargument", "acommandname", null);

            var automation = this.store.GetCurrent().Automation.Single();
            automation.Name.Should().Be("acommandname");
            automation.Metadata[nameof(CliCommand.ApplicationName)].Should().Be("anapplicationname");
            automation.Metadata[nameof(CliCommand.Arguments)].Should().Be("anargument");
            result.Id.Should().Be(automation.Id);
        }

        [Fact]
        public void WhenAddCliCommandOnDescendantElement_ThenAddsAutomationToElement()
        {
            this.application.CreateNewPattern("apatternname");
            this.application.AddElement("anelementname", null, null, false, ElementCardinality.Single, null);
            this.patternPathResolver.Setup(ppr =>
                    ppr.Resolve(It.IsAny<PatternDefinition>(), "{apatternname.anelementname}"))
                .Returns(this.application.GetCurrentPattern().Elements.Single);
            this.application.AttachCodeTemplate("arootpath", "arelativepath", "atemplatename", "{apatternname.anelementname}");

            var result = this.application.AddCliCommand("anapplicationname", null, "acommandname",
                "{apatternname.anelementname}");

            result.Name.Should().Be("acommandname");
            this.application.GetCurrentPattern().Elements.Single().Automation.Single().Id.Should().Be(result.Id);
        }

        [Fact]
        public void WhenAddCommandLaunchPointAndCurrentPatternNotExists_ThenThrows()
        {
            this.application
                .Invoking(x => x.AddCommandLaunchPoint(null, new List<string> { "acmdid" }, null))
                .Should().Throw<AutomateException>()
                .WithMessage(ExceptionMessages.AuthoringApplication_NoCurrentPattern);
        }

        [Fact]
        public void WhenAddCommandLaunchPointAndParentNotExists_ThenThrows()
        {
            this.patternPathResolver.Setup(ppr => ppr.Resolve(It.IsAny<PatternDefinition>(), It.IsAny<string>()))
                .Returns((PatternDefinition)null);

            this.application.CreateNewPattern("apatternname");

            this.application
                .Invoking(x =>
                    x.AddCommandLaunchPoint(null, new List<string> { "acmdid1" }, "anunknownparent"))
                .Should().Throw<AutomateException>()
                .WithMessage(
                    ExceptionMessages.AuthoringApplication_PathExpressionNotFound.Format("anunknownparent"));
        }

        [Fact]
        public void WhenAddCommandLaunchPoint_TheAddsAutomationToPattern()
        {
            this.application.CreateNewPattern("apatternname");
            this.application.AttachCodeTemplate("arootpath", "arelativepath", "atemplatename", null);
            var command1 = this.application.AddCodeTemplateCommand("atemplatename", "acommandname1", false, "~/apath", null);
            var command2 = this.application.AddCodeTemplateCommand("atemplatename", "acommandname2", false, "~/apath", null);

            var result =
                this.application.AddCommandLaunchPoint("alaunchpointname", new List<string> { command1.Id, command2.Id }, null);

            var automation = this.store.GetCurrent().Automation.Last();
            automation.Name.Should().Be("alaunchpointname");
            automation.Metadata[nameof(CommandLaunchPoint.CommandIds)].Should().Be($"{command1.Id};{command2.Id}");
            result.Id.Should().Be(automation.Id);
        }

        [Fact]
        public void WhenAddCommandLaunchPointOnDescendantElement_ThenAddsAutomationToElement()
        {
            this.application.CreateNewPattern("apatternname");
            this.application.AddElement("anelementname", null, null, false, ElementCardinality.Single, null);
            this.patternPathResolver.Setup(ppr =>
                    ppr.Resolve(It.IsAny<PatternDefinition>(), "{apatternname.anelementname}"))
                .Returns(this.application.GetCurrentPattern().Elements.Single);
            this.application.AttachCodeTemplate("arootpath", "arelativepath", "atemplatename", null);
            var command = this.application.AddCodeTemplateCommand("atemplatename", "acommandname1", false, "~/apath", null);

            var result =
                this.application.AddCommandLaunchPoint("alaunchpointname",
                    new List<string> { command.Id }, "{apatternname.anelementname}");

            var automation = this.store.GetCurrent().Elements.Single().Automation.Last();
            result.Name.Should().Be("alaunchpointname");
            result.Id.Should().Be(automation.Id);
        }

        [Fact]
        public void WhenUpdateCommandLaunchPointAndCurrentPatternNotExists_ThenThrows()
        {
            this.application
                .Invoking(x => x.UpdateCommandLaunchPoint("alaunchpointname", null, new List<string> { "acmdid" }, null, null))
                .Should().Throw<AutomateException>()
                .WithMessage(ExceptionMessages.AuthoringApplication_NoCurrentPattern);
        }

        [Fact]
        public void WhenUpdateCommandLaunchPointAndParentNotExists_ThenThrows()
        {
            this.patternPathResolver.Setup(ppr => ppr.Resolve(It.IsAny<PatternDefinition>(), It.IsAny<string>()))
                .Returns((PatternDefinition)null);

            this.application.CreateNewPattern("apatternname");

            this.application
                .Invoking(x =>
                    x.UpdateCommandLaunchPoint("alaunchpointname", null, new List<string> { "acmdid1" }, null, "anunknownparent"))
                .Should().Throw<AutomateException>()
                .WithMessage(
                    ExceptionMessages.AuthoringApplication_PathExpressionNotFound.Format("anunknownparent"));
        }

        [Fact]
        public void WhenUpdateCommandLaunchPoint_TheUpdatesAutomationOnPattern()
        {
            this.application.CreateNewPattern("apatternname");
            this.application.AttachCodeTemplate("arootpath", "arelativepath", "atemplatename", null);
            var command1 = this.application.AddCodeTemplateCommand("atemplatename", "acommandname1", false, "~/apath", null);
            var command2 = this.application.AddCodeTemplateCommand("atemplatename", "acommandname2", false, "~/apath", null);
            var launchPoint = this.application.AddCommandLaunchPoint("alaunchpointname", new List<string> { command1.Id }, null);

            var result =
                this.application.UpdateCommandLaunchPoint(launchPoint.Name, "anewname", new List<string> { command1.Id, command2.Id }, null, null);

            var automation = this.store.GetCurrent().Automation.Last();
            automation.Name.Should().Be("anewname");
            automation.Metadata[nameof(CommandLaunchPoint.CommandIds)].Should().Be($"{command1.Id};{command2.Id}");
            result.Id.Should().Be(automation.Id);
        }

        [Fact]
        public void WhenUpdateCommandLaunchPointOnDescendantElement_ThenUpdatesAutomationOnElement()
        {
            this.application.CreateNewPattern("apatternname");
            this.application.AddElement("anelementname", null, null, false, ElementCardinality.Single, null);
            this.patternPathResolver.Setup(ppr =>
                    ppr.Resolve(It.IsAny<PatternDefinition>(), "{apatternname.anelementname}"))
                .Returns(this.application.GetCurrentPattern().Elements.First);
            this.application.AttachCodeTemplate("arootpath", "arelativepath", "atemplatename", "{apatternname.anelementname}");
            var command1 = this.application.AddCodeTemplateCommand("atemplatename", "acommandname1", false, "~/apath", "{apatternname.anelementname}");
            var command2 = this.application.AddCodeTemplateCommand("atemplatename", "acommandname2", false, "~/apath", "{apatternname.anelementname}");
            var launchPoint = this.application.AddCommandLaunchPoint("alaunchpointname", new List<string> { command1.Id }, "{apatternname.anelementname}");

            var result =
                this.application.UpdateCommandLaunchPoint(launchPoint.Name, "anewname",
                    new List<string> { command1.Id, command2.Id }, null, "{apatternname.anelementname}");

            var automation = this.store.GetCurrent().Elements.Single().Automation.Last();
            automation.Name.Should().Be("anewname");
            automation.Metadata[nameof(CommandLaunchPoint.CommandIds)].Should().Be($"{command1.Id};{command2.Id}");
            result.Id.Should().Be(automation.Id);
        }

        [Fact]
        public void WhenBuildToolkitAndCurrentPatternNotExists_ThenThrows()
        {
            this.application
                .Invoking(x => x.PackageToolkit(null, false))
                .Should().Throw<AutomateException>()
                .WithMessage(ExceptionMessages.AuthoringApplication_NoCurrentPattern);
        }

        [Fact]
        public void WhenPackageToolkit_ThenPackagesToolkit()
        {
            this.application.CreateNewPattern("apatternname");
            this.builder.Setup(bdr => bdr.Pack(It.IsAny<PatternDefinition>(), It.IsAny<VersionInstruction>()))
                .Returns((PatternDefinition pattern, VersionInstruction version) =>
                    new ToolkitPackage(new ToolkitDefinition(pattern, new Version(version.Instruction)), "abuildlocation", null));

            var toolkit = this.application.PackageToolkit("2.0.0", false);

            this.builder.Verify(bdr => bdr.Pack(It.IsAny<PatternDefinition>(), It.Is<VersionInstruction>(vi =>
                vi.Instruction == "2.0.0")));
            toolkit.BuiltLocation.Should().Be("abuildlocation");
            toolkit.Toolkit.PatternName.Should().Be("apatternname");
            toolkit.Toolkit.Version.Should().Be("2.0.0");
        }

        [Fact]
        public void WhenTestCodeTemplateCommandAndCurrentPatternNotExists_ThenThrows()
        {
            this.application
                .Invoking(x => x.TestCodeTemplate("atemplatename", null, null, null, null))
                .Should().Throw<AutomateException>()
                .WithMessage(ExceptionMessages.AuthoringApplication_NoCurrentPattern);
        }

        [Fact]
        public void WhenTestCodeTemplateCommandAndCodeTemplateNotExists_ThenThrows()
        {
            this.application.CreateNewPattern("apatternname");

            this.application
                .Invoking(x => x.TestCodeTemplate("atemplatename", null, null, null, null))
                .Should().Throw<AutomateException>()
                .WithMessage(ExceptionMessages.AuthoringApplication_CodeTemplateNotExistsRoot.Format("atemplatename"));
        }

        [Fact]
        public void WhenTestCodeTemplateOnPattern_ThenReturnsResult()
        {
            this.filePathResolver.Setup(pr => pr.GetFileAtPath(It.IsAny<string>()))
                .Returns(Mock.Of<IFile>(file => file.GetContents() == CodeTemplateFile.Encoding.GetBytes("atexttemplate")));
            this.application.CreateNewPattern("apatternname");
            this.application.AttachCodeTemplate("arootpath", "arelativepath", "atemplatename", null);

            var result = this.application.TestCodeTemplate("atemplatename", null, null, null, null);

            result.Output.Should().Be("anoutput");
            this.textTemplatingEngine.Verify(tte => tte.Transform(It.IsAny<string>(), "atexttemplate", It.IsAny<IDictionary>()));
        }

        [Fact]
        public void WhenTestCodeTemplateOnDescendantElement_ThenReturnsResult()
        {
            this.filePathResolver.Setup(pr => pr.GetFileAtPath(It.IsAny<string>()))
                .Returns(Mock.Of<IFile>(file => file.GetContents() == CodeTemplateFile.Encoding.GetBytes("atexttemplate")));
            this.application.CreateNewPattern("apatternname");
            this.application.AddElement("anelementname", null, null, false, ElementCardinality.Single, null);
            this.patternPathResolver.Setup(ppr =>
                    ppr.Resolve(It.IsAny<PatternDefinition>(), "{apatternname.anelementname}"))
                .Returns(this.application.GetCurrentPattern().Elements.Single);
            this.application.AttachCodeTemplate("arootpath", "arelativepath", "atemplatename", "{apatternname.anelementname}");

            var result = this.application.TestCodeTemplate("atemplatename", "{apatternname.anelementname}", null, null, null);

            result.Output.Should().Be("anoutput");
            this.textTemplatingEngine.Verify(tte => tte.Transform(It.IsAny<string>(), "atexttemplate", It.IsAny<IDictionary>()));
        }

        [Fact]
        public void WhenTestCodeTemplateOnDescendantCollection_ThenReturnsResult()
        {
            this.filePathResolver.Setup(pr => pr.GetFileAtPath(It.IsAny<string>()))
                .Returns(Mock.Of<IFile>(file => file.GetContents() == CodeTemplateFile.Encoding.GetBytes("atexttemplate")));
            this.application.CreateNewPattern("apatternname");
            this.application.AddElement("acollectionname", null, null, true, ElementCardinality.Single, null);
            this.patternPathResolver.Setup(ppr =>
                    ppr.Resolve(It.IsAny<PatternDefinition>(), "{apatternname.acollectionname}"))
                .Returns(this.application.GetCurrentPattern().Elements.Single);
            this.application.AttachCodeTemplate("arootpath", "arelativepath", "atemplatename", "{apatternname.acollectionname}");

            var result = this.application.TestCodeTemplate("atemplatename", "{apatternname.acollectionname}", null, null, null);

            result.Output.Should().Be("anoutput");
            this.textTemplatingEngine.Verify(tte => tte.Transform(It.IsAny<string>(), "atexttemplate", It.IsAny<IDictionary>()));
        }

        [Fact]
        public void WhenTestCodeTemplateAndImportDataNotExist_ThenThrows()
        {
            this.application.CreateNewPattern("apatternname");
            this.application.AttachCodeTemplate("arootpath", "arelativepath", "atemplatename", null);
            this.filePathResolver.Setup(pr => pr.ExistsAtPath(It.IsAny<string>()))
                .Returns(false);

            this.application
                .Invoking(x => x.TestCodeTemplate("atemplatename", null, "arootpath", "animportpath", null))
                .Should().Throw<AutomateException>()
                .WithMessage(ExceptionMessages.AuthoringApplication_TestDataImport_NotFoundAtLocation.Format("afullpath"));
        }

        [Fact]
        public void WhenTestCodeTemplateAndImportDataNotJson_ThenThrows()
        {
            this.application.CreateNewPattern("apatternname");
            this.application.AttachCodeTemplate("arootpath", "arelativepath", "atemplatename", null);
            this.filePathResolver.Setup(pr => pr.ExistsAtPath(It.IsAny<string>()))
                .Returns(true);
            this.filePathResolver.Setup(pr => pr.GetFileAtPath(It.IsAny<string>()))
                .Returns(Mock.Of<IFile>(file => file.GetContents() == SystemIoFileConstants.Encoding.GetBytes("\t")));

            this.application
                .Invoking(x => x.TestCodeTemplate("atemplatename", null, "arootpath", "animportpath", null))
                .Should().Throw<AutomateException>()
                .WithMessage(ExceptionMessages.AuthoringApplication_TestDataImport_NotValidJson);
        }

        [Fact]
        public void WhenTestCodeTemplateAndImportData_ThenReturnsResult()
        {
            this.filePathResolver.Setup(pr => pr.GetFileAtPath(It.IsAny<string>()))
                .Returns(Mock.Of<IFile>(file => file.GetContents() == CodeTemplateFile.Encoding.GetBytes("atexttemplate")));
            this.application.CreateNewPattern("apatternname");
            this.application.AttachCodeTemplate("arootpath", "arelativepath", "atemplatename", null);
            this.filePathResolver.Setup(pr => pr.ExistsAtPath(It.IsAny<string>()))
                .Returns(true);
            this.filePathResolver.Setup(pr => pr.GetFileAtPath(It.IsAny<string>()))
                .Returns(Mock.Of<IFile>(file => file.GetContents() == SystemIoFileConstants.Encoding.GetBytes("{\"aname\":\"avalue\"}")));

            var result = this.application.TestCodeTemplate("atemplatename", null, "arootpath", "animportpath", null);

            result.Output.Should().Be("anoutput");
            this.textTemplatingEngine.Verify(tte => tte.Transform(It.IsAny<string>(), "atexttemplate", It.Is<Dictionary<string, object>>(dic =>
                (string)dic["aname"] == "avalue")));
        }

        [Fact]
        public void WhenTestCodeTemplateAndExportDataAndNotFile_ThenThrows()
        {
            this.filePathResolver.Setup(pr => pr.GetFileAtPath(It.IsAny<string>()))
                .Returns(Mock.Of<IFile>(file => file.GetContents() == CodeTemplateFile.Encoding.GetBytes("atexttemplate")));
            this.application.CreateNewPattern("apatternname");
            this.application.AttachCodeTemplate("arootpath", "arelativepath", "atemplatename", null);
            this.filePathResolver.Setup(pr => pr.CreateFileAtPath(It.IsAny<string>(), It.IsAny<byte[]>()))
                .Throws(new FileNotFoundException("anerrormessage"));

            this.application
                .Invoking(x => x.TestCodeTemplate("atemplatename", null, "arootpath", null, "anexportpath"))
                .Should().Throw<AutomateException>()
                .WithMessage(ExceptionMessages.AuthoringApplication_TestDataExport_NotValidFile.Format("afullpath", "anerrormessage"));
        }

        [Fact]
        public void WhenTestCodeTemplateAndExportData_ThenWritesData()
        {
            this.filePathResolver.Setup(pr => pr.GetFileAtPath(It.IsAny<string>()))
                .Returns(Mock.Of<IFile>(file => file.GetContents() == CodeTemplateFile.Encoding.GetBytes("atexttemplate")));
            this.application.CreateNewPattern("apatternname");
            this.application.AttachCodeTemplate("arootpath", "arelativepath", "atemplatename", null);

            var result = this.application.TestCodeTemplate("atemplatename", null, "arootpath", null, "anexportpath");

            result.Output.Should().Be("anoutput");
            this.textTemplatingEngine.Verify(tte => tte.Transform(It.IsAny<string>(), "atexttemplate", It.IsAny<IDictionary>()));
            this.filePathResolver.Verify(pr => pr.CreateFileAtPath("afullpath", It.IsAny<byte[]>()));
        }
    }
}