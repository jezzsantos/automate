using System.Linq;
using automate;
using automate.Application;
using automate.Domain;
using automate.Extensions;
using automate.Infrastructure;
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

        public AuthoringApplicationSpec()
        {
            this.filePathResolver = new Mock<IFilePathResolver>();
            this.filePathResolver.Setup(pr => pr.CreatePath(It.IsAny<string>(), It.IsAny<string>()))
                .Returns("afullpath");
            this.filePathResolver.Setup(pr => pr.ExistsAtPath(It.IsAny<string>()))
                .Returns(true);
            var file = new Mock<IFile>();
            this.filePathResolver.Setup(pr => pr.GetFileAtPath(It.IsAny<string>()))
                .Returns(file.Object);
            this.patternPathResolver = new Mock<IPatternPathResolver>();
            this.patternPathResolver.Setup(ppr => ppr.Resolve(It.IsAny<PatternDefinition>(), It.IsAny<string>()))
                .Returns((PatternDefinition model, string _) => model);
            var repo = new MemoryRepository();
            this.store = new PatternStore(repo, repo);
            this.builder = new Mock<IPatternToolkitPackager>();
            this.application =
                new AuthoringApplication(this.store, this.filePathResolver.Object, this.patternPathResolver.Object,
                    this.builder.Object);
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
        public void WhenSwitchCurrentPatternAndExists_ThenCurrentIsChanged()
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
            this.store.Create("aname");
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
        public void WhenAttachCodeTemplateAndTemplateWithSameName_ThenThrows()
        {
            this.store.Create("aname");
            this.application.AttachCodeTemplate("arootpath", "arelativepath", "atemplatename", null);

            this.application
                .Invoking(x => x.AttachCodeTemplate("arootpath", "arelativepath", "atemplatename", null))
                .Should().Throw<AutomateException>()
                .WithMessage(ExceptionMessages.AuthoringApplication_CodeTemplateByNameExists.Format("atemplatename"));
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
            this.store.Create("aname");
            this.application.AttachCodeTemplate("arootpath", "arelativepath", "atemplatename", null);

            this.store.GetCurrent().CodeTemplates.Single().Name.Should().Be("atemplatename");
            this.store.GetCurrent().CodeTemplates.Single().Metadata.OriginalFilePath.Should().Be("afullpath");
        }

        [Fact]
        public void WhenListCodeTemplatesAndCurrentPatternNotExists_ThenThrows()
        {
            this.application
                .Invoking(x => x.ListCodeTemplates())
                .Should().Throw<AutomateException>()
                .WithMessage(ExceptionMessages.AuthoringApplication_NoCurrentPattern);
        }

        [Fact]
        public void WhenListCodeTemplates_ThenListsTemplates()
        {
            this.store.Create("aname");
            this.application.AttachCodeTemplate("arootpath", "arelativepath", "atemplatename1", null);
            this.application.AttachCodeTemplate("arootpath", "arelativepath", "atemplatename2", null);
            this.application.AttachCodeTemplate("arootpath", "arelativepath", "atemplatename3", null);

            var result = this.application.ListCodeTemplates();

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
        public void WhenAddAttributeAndAlreadyExists_ThenThrows()
        {
            this.application.CreateNewPattern("apatternname");
            this.application.AddAttribute("anattributename", null, null, false, null, null);

            this.application
                .Invoking(x => x.AddAttribute("anattributename", null, null, false, null, null))
                .Should().Throw<AutomateException>()
                .WithMessage(ExceptionMessages.AuthoringApplication_AttributeByNameExists.Format("anattributename"));
        }

        [Fact]
        public void WhenAddAttributeWithReservedName_ThenThrows()
        {
            this.application.CreateNewPattern("apatternname");

            this.application
                .Invoking(x => x.AddAttribute(Attribute.ReservedAttributeNames[0], null, null, false, null, null))
                .Should().Throw<AutomateException>()
                .WithMessage(
                    ExceptionMessages.AuthoringApplication_AttributeNameReserved.Format(
                        Attribute.ReservedAttributeNames[0]));
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
            result.parent.Id.Should().Be(this.store.GetCurrent().Id);
        }

        [Fact]
        public void WhenAddAttributeAndDefaultValueIsNotOneOfChoices_ThenThrows()
        {
            this.application.CreateNewPattern("apatternname");

            this.application
                .Invoking(x =>
                    x.AddAttribute("anattributename", null, "adefaultvalue", false, "avalue1;avalue2;avalue3", null))
                .Should().Throw<AutomateException>()
                .WithMessage(ExceptionMessages.AuthoringApplication_AttributeDefaultValueIsNotAChoice);
        }

        [Fact]
        public void WhenAddAttributeAndNoType_TheAddsAttributeWithDefaultType()
        {
            this.application.CreateNewPattern("apatternname");

            this.application.AddAttribute("anattributename", null, null, false, null, null);

            var attribute = this.store.GetCurrent().Attributes.Single();
            attribute.Name.Should().Be("anattributename");
            attribute.DataType.Should().Be("string");
            attribute.DefaultValue.Should().BeNull();
            attribute.IsRequired.Should().BeFalse();
            attribute.Choices.Should().BeEmpty();
        }

        [Fact]
        public void WhenAddAttributeAndDefaultValueIsOneOfChoices_ThenAddsAttribute()
        {
            this.application.CreateNewPattern("apatternname");

            this.application.AddAttribute("anattributename", null, "avalue2", false, "avalue1;avalue2;avalue3", null);

            var attribute = this.store.GetCurrent().Attributes.Single();
            attribute.DefaultValue.Should().Be("avalue2");
            attribute.IsRequired.Should().BeFalse();
            attribute.Choices.Should().ContainInOrder("avalue1", "avalue2", "avalue3");
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
                    ExceptionMessages.AuthoringApplication_NodeExpressionNotFound.Format("anunknownparent"));
        }

        [Fact]
        public void WhenAddAttributeAndParentIsElement_ThenAddsAttributeToElement()
        {
            this.application.CreateNewPattern("apatternname");
            this.application.AddElement("anelementname", null, null, false, null);
            var parentElement = new Element("anelementname", null, null, false);
            this.patternPathResolver
                .Setup(ppr => ppr.Resolve(It.IsAny<PatternDefinition>(), "{apatternname.anelementname}"))
                .Returns(parentElement);

            var result = this.application.AddAttribute("anattributename", null, null, false, null,
                "{apatternname.anelementname}");

            var attribute = result.parent.Attributes.Single();
            attribute.Name.Should().Be("anattributename");
            result.parent.Id.Should().Be(parentElement.Id);
        }

        [Fact]
        public void WhenAddElementAndCurrentPatternNotExists_ThenThrows()
        {
            this.application
                .Invoking(x => x.AddElement("anelementname", null, null, false, null))
                .Should().Throw<AutomateException>()
                .WithMessage(ExceptionMessages.AuthoringApplication_NoCurrentPattern);
        }

        [Fact]
        public void WhenAddElementAndAlreadyExists_ThenThrows()
        {
            this.application.CreateNewPattern("apatternname");
            this.application.AddElement("anelementname", null, null, false, null);

            this.application
                .Invoking(x => x.AddElement("anelementname", null, null, false, null))
                .Should().Throw<AutomateException>()
                .WithMessage(ExceptionMessages.AuthoringApplication_ElementByNameExists.Format("anelementname"));
        }

        [Fact]
        public void WhenAddElementAndParentNotExists_ThenThrows()
        {
            this.patternPathResolver.Setup(ppr => ppr.Resolve(It.IsAny<PatternDefinition>(), It.IsAny<string>()))
                .Returns((PatternDefinition)null);

            this.application.CreateNewPattern("apatternname");

            this.application
                .Invoking(x =>
                    x.AddElement("anelementname", null, null, false, "anunknownparent"))
                .Should().Throw<AutomateException>()
                .WithMessage(
                    ExceptionMessages.AuthoringApplication_NodeExpressionNotFound.Format("anunknownparent"));
        }

        [Fact]
        public void WhenAddElement_TheAddsElementToPattern()
        {
            this.application.CreateNewPattern("apatternname");

            var result = this.application.AddElement("anelementname", "adisplayname", "adescription", false, null);

            var element = this.store.GetCurrent().Elements.Single();
            element.Name.Should().Be("anelementname");
            element.DisplayName.Should().Be("adisplayname");
            element.Description.Should().Be("adescription");
            element.IsCollection.Should().BeFalse();
            result.parent.Id.Should().Be(this.store.GetCurrent().Id);
        }

        [Fact]
        public void WhenAddElementAndParentIsElement_ThenAddsElementToElement()
        {
            this.application.CreateNewPattern("apatternname");
            this.application.AddElement("aparentelementname", null, null, false, null);
            var parentElement = new Element("aparentelementname", null, null, false);
            this.patternPathResolver.Setup(ppr =>
                    ppr.Resolve(It.IsAny<PatternDefinition>(), "{apatternname.aparentelementname}"))
                .Returns(parentElement);

            var result =
                this.application.AddElement("achildelementname", null, null, false,
                    "{apatternname.aparentelementname}");

            var element = result.parent.Elements.Single();
            element.Name.Should().Be("achildelementname");
            result.parent.Id.Should().Be(parentElement.Id);
        }

        [Fact]
        public void WhenAddCodeTemplateCommandAndCurrentPatternNotExists_ThenThrows()
        {
            this.application
                .Invoking(x => x.AddCodeTemplateCommand("acommandname", false, "~/apath", null))
                .Should().Throw<AutomateException>()
                .WithMessage(ExceptionMessages.AuthoringApplication_NoCurrentPattern);
        }

        [Fact]
        public void WhenAddCodeTemplateCommandAndAlreadyExists_ThenThrows()
        {
            this.application.CreateNewPattern("apatternname");
            this.application.AddCodeTemplateCommand("acommandname", false, "~/apath", null);

            this.application
                .Invoking(x => x.AddCodeTemplateCommand("acommandname", false, "~/apath", null))
                .Should().Throw<AutomateException>()
                .WithMessage(ExceptionMessages.AuthoringApplication_AutomationByNameExists.Format("acommandname"));
        }

        [Fact]
        public void WhenAddCodeTemplateCommandAndParentNotExists_ThenThrows()
        {
            this.patternPathResolver.Setup(ppr => ppr.Resolve(It.IsAny<PatternDefinition>(), It.IsAny<string>()))
                .Returns((PatternDefinition)null);

            this.application.CreateNewPattern("apatternname");

            this.application
                .Invoking(x =>
                    x.AddCodeTemplateCommand("acommandname", false, "~/apath", "anunknownparent"))
                .Should().Throw<AutomateException>()
                .WithMessage(
                    ExceptionMessages.AuthoringApplication_NodeExpressionNotFound.Format("anunknownparent"));
        }

        [Fact]
        public void WhenAddCodeTemplateCommand_TheAddsAutomationToPattern()
        {
            this.application.CreateNewPattern("apatternname");

            var result = this.application.AddCodeTemplateCommand("acommandname", false, "~/apath", null);

            var automation = this.store.GetCurrent().Automation.Single().As<AutomationCommand>();
            automation.Name.Should().Be("acommandname");
            automation.IsTearOff.Should().BeFalse();
            automation.FilePath.Should().Be("~/apath");
            result.Id.Should().Be(automation.Id);
        }

        [Fact]
        public void WhenAddCodeTemplateCommandAndParentIsElement_ThenAddsAutomationToElement()
        {
            this.application.CreateNewPattern("apatternname");
            this.application.AddElement("aparentelementname", null, null, false, null);
            var parentElement = new Element("aparentelementname", null, null, false);
            this.patternPathResolver.Setup(ppr =>
                    ppr.Resolve(It.IsAny<PatternDefinition>(), "{apatternname.aparentelementname}"))
                .Returns(parentElement);

            var result = this.application.AddCodeTemplateCommand("acommandname", false, "~/apath",
                "{apatternname.aparentelementname}");

            result.Name.Should().Be("acommandname");
            parentElement.Automation.Single().Id.Should().Be(result.Id);
        }

        [Fact]
        public void WhenAddCommandLaunchPointAndCurrentPatternNotExists_ThenThrows()
        {
            this.application
                .Invoking(x => x.AddCommandLaunchPoint("acmdid1", null, null))
                .Should().Throw<AutomateException>()
                .WithMessage(ExceptionMessages.AuthoringApplication_NoCurrentPattern);
        }

        [Fact]
        public void WhenAddCommandLaunchPointAndAlreadyExists_ThenThrows()
        {
            this.application.CreateNewPattern("apatternname");
            this.application.AddCommandLaunchPoint(IdGenerator.Create(), "alaunchpointname", null);

            this.application
                .Invoking(x => x.AddCommandLaunchPoint(IdGenerator.Create(), "alaunchpointname", null))
                .Should().Throw<AutomateException>()
                .WithMessage(ExceptionMessages.AuthoringApplication_AutomationByNameExists.Format("alaunchpointname"));
        }

        [Fact]
        public void WhenAddCommandLaunchPointAndParentNotExists_ThenThrows()
        {
            this.patternPathResolver.Setup(ppr => ppr.Resolve(It.IsAny<PatternDefinition>(), It.IsAny<string>()))
                .Returns((PatternDefinition)null);

            this.application.CreateNewPattern("apatternname");

            this.application
                .Invoking(x =>
                    x.AddCommandLaunchPoint("acmdid1", null, "anunknownparent"))
                .Should().Throw<AutomateException>()
                .WithMessage(
                    ExceptionMessages.AuthoringApplication_NodeExpressionNotFound.Format("anunknownparent"));
        }

        [Fact]
        public void WhenAddCommandLaunchPoint_TheAddsAutomationToPattern()
        {
            this.application.CreateNewPattern("apatternname");

            var commandId1 = IdGenerator.Create();
            var commandId2 = IdGenerator.Create();
            var commandId3 = IdGenerator.Create();
            var result =
                this.application.AddCommandLaunchPoint(new[] { commandId1, commandId2, commandId3 }.SafeJoin(";"),
                    "alaunchpointname", null);

            var automation = this.store.GetCurrent().Automation.Single().As<AutomationLaunchPoint>();
            automation.Name.Should().Be("alaunchpointname");
            automation.CommandIds.Should().ContainInOrder(commandId1, commandId2, commandId3);
            result.Id.Should().Be(automation.Id);
        }

        [Fact]
        public void WhenAddCommandLaunchPointAndParentIsElement_ThenAddsAutomationToElement()
        {
            this.application.CreateNewPattern("apatternname");
            this.application.AddElement("aparentelementname", null, null, false, null);
            var parentElement = new Element("aparentelementname", null, null, false);
            this.patternPathResolver.Setup(ppr =>
                    ppr.Resolve(It.IsAny<PatternDefinition>(), "{apatternname.aparentelementname}"))
                .Returns(parentElement);

            var result =
                this.application.AddCommandLaunchPoint(IdGenerator.Create(), "alaunchpointname",
                    "{apatternname.aparentelementname}");

            result.Name.Should().Be("alaunchpointname");
            parentElement.Automation.Single().Id.Should().Be(result.Id);
        }

        [Fact]
        public void WhenAddCommandLaunchPointAndNoName_TheAddsAutomationWithDefaultName()
        {
            this.application.CreateNewPattern("apatternname");
            var commandId = IdGenerator.Create();

            var result = this.application.AddCommandLaunchPoint(commandId, null, null);

            var automation = this.store.GetCurrent().Automation.Single().As<AutomationLaunchPoint>();
            automation.Name.Should().Be("LaunchPoint1");
            automation.CommandIds.Should().ContainSingle(commandId);
            result.Id.Should().Be(automation.Id);
        }

        [Fact]
        public void WhenBuildToolkitAndCurrentPatternNotExists_ThenThrows()
        {
            this.application
                .Invoking(x => x.PackageToolkit(null))
                .Should().Throw<AutomateException>()
                .WithMessage(ExceptionMessages.AuthoringApplication_NoCurrentPattern);
        }

        [Fact]
        public void WhenPackageToolkit_ThenPackagesToolkit()
        {
            this.application.CreateNewPattern("apatternname");
            this.builder.Setup(bdr => bdr.Pack(It.IsAny<PatternDefinition>(), It.IsAny<string>()))
                .Returns((PatternDefinition pattern, string version) =>
                    new PatternToolkitPackage(new ToolkitDefinition(pattern, version), "abuildlocation"));

            var toolkit = this.application.PackageToolkit("2.0");

            this.builder.Verify(bdr => bdr.Pack(It.IsAny<PatternDefinition>(), "2.0"));
            toolkit.BuiltLocation.Should().Be("abuildlocation");
            toolkit.Toolkit.PatternName.Should().Be("apatternname");
            toolkit.Toolkit.Version.Should().Be("2.0");
        }
    }
}