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
            this.application.AttachCodeTemplate("arootpath", "arelativepath", "atemplatename", null);

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
            attribute.Choices.Should().BeNull();
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
            var element = new Element("anelementname");
            this.patternPathResolver.Setup(ppr =>
                    ppr.Resolve(It.IsAny<PatternDefinition>(), "{apatternname.anelementname}"))
                .Returns(element);
            this.application.AttachCodeTemplate("arootpath", "arelativepath", "atemplatename", null);
            var command = this.application.AddCodeTemplateCommand("atemplatename", "acommandname1", false, "~/apath", null);

            var result =
                this.application.AddCommandLaunchPoint("alaunchpointname",
                    new List<string> { command.Id }, "{apatternname.anelementname}");

            result.Name.Should().Be("alaunchpointname");
            element.Automation.Single().Id.Should().Be(result.Id);
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

            result.Should().Be("anoutput");
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

            result.Should().Be("anoutput");
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

            result.Should().Be("anoutput");
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

            result.Should().Be("anoutput");
            this.textTemplatingEngine.Verify(tte => tte.Transform(It.IsAny<string>(), "atexttemplate", It.IsAny<IDictionary>()));
            this.filePathResolver.Verify(pr => pr.CreateFileAtPath("afullpath", It.IsAny<byte[]>()));
        }
    }
}