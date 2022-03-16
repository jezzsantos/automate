using System;
using System.Linq;
using Automate.CLI;
using Automate.CLI.Application;
using Automate.CLI.Domain;
using Automate.CLI.Extensions;
using Automate.CLI.Infrastructure;
using FluentAssertions;
using Moq;
using Xunit;

namespace CLI.UnitTests.Infrastructure
{
    [Trait("Category", "Unit")]
    public class PatternToolkitPackagerSpec
    {
        private readonly PatternToolkitPackager packager;
        private readonly Mock<IPatternStore> patternStore;
        private readonly Mock<IToolkitStore> toolkitStore;

        public PatternToolkitPackagerSpec()
        {
            this.patternStore = new Mock<IPatternStore>();
            this.toolkitStore = new Mock<IToolkitStore>();
            this.toolkitStore.Setup(store => store.Export(It.IsAny<ToolkitDefinition>()))
                .Returns("alocation");
            this.patternStore.Setup(ps => ps.GetCurrent())
                .Returns(new PatternDefinition("apatternname"));
            this.packager = new PatternToolkitPackager(this.patternStore.Object, this.toolkitStore.Object);
        }

        [Fact]
        public void WhenPackAndVersionInstruction_ThenPackagesFirstVersionOfToolkit()
        {
            var pattern = new PatternDefinition("apatternname");

            var result = this.packager.Pack(pattern, ToolkitVersion.AutoIncrementInstruction);

            result.Toolkit.Version.Should().Be("0.1.0");
        }

        [Fact]
        public void WhenPackAndVersionInstructionIsAVersion_ThenPackagesVersionOfToolkit()
        {
            var pattern = new PatternDefinition("apatternname");
            pattern.UpdateToolkitVersion("1.0.0");

            var result = this.packager.Pack(pattern, "2.1.0");

            result.Toolkit.Version.Should().Be("2.1.0");
        }

        [Fact]
        public void WhenPackAndCodeTemplates_ThenReturnsPackage()
        {
            var pattern = new PatternDefinition("apatternname");
            pattern.AddCodeTemplate(new CodeTemplate("acodetemplatename1", "afullpath1", "anextension1"));
            var element = new Element("anelementname");
            element.AddCodeTemplate(new CodeTemplate("acodetemplatename2", "afullpath2", "anextension2"));
            pattern.AddElement(element);
            var fileContents = new byte[] { 0x01 };
            this.patternStore.Setup(ps => ps.GetCurrent())
                .Returns(pattern);
            this.patternStore.Setup(ps => ps.DownloadCodeTemplate(It.IsAny<PatternDefinition>(), It.IsAny<CodeTemplate>()))
                .Returns(fileContents);

            var result = this.packager.Pack(pattern, null);

            result.Toolkit.CodeTemplateFiles.Should().ContainSingle(ctf =>
                ctf.Id == pattern.CodeTemplates.Single().Id && ctf.Contents == fileContents);
            this.toolkitStore.Verify(repo => repo.Export(It.Is<ToolkitDefinition>(toolkit =>
                toolkit.Version == "0.1.0"
                && toolkit.CodeTemplateFiles.Count == 2
                && toolkit.CodeTemplateFiles[0].Id == pattern.CodeTemplates.Single().Id
                && toolkit.CodeTemplateFiles[0].Contents == fileContents
                && toolkit.CodeTemplateFiles[1].Id == pattern.Elements.Single().CodeTemplates.Single().Id
                && toolkit.CodeTemplateFiles[1].Contents == fileContents
            )));
            this.patternStore.Verify(ps => ps.DownloadCodeTemplate(pattern, pattern.CodeTemplates.Single()));
            this.patternStore.Verify(ps => ps.DownloadCodeTemplate(pattern, pattern.Elements.Single().CodeTemplates.Single()));
        }

        [Fact]
        public void WhenPack_ThenReturnsPackage()
        {
            var pattern = new PatternDefinition("apatternname");

            var result = this.packager.Pack(pattern, null);

            result.Toolkit.Id.Should().NotBeNull();
            result.Toolkit.Version.Should().Be("0.1.0");
            result.Toolkit.PatternName.Should().Be("apatternname");
            result.BuiltLocation.Should().Be("alocation");
            this.toolkitStore.Verify(repo => repo.Export(It.Is<ToolkitDefinition>(toolkit =>
                toolkit.Version == "0.1.0"
                && toolkit.Pattern == pattern
            )));
        }

        [Fact]
        public void WhenUnPackAndFileIsEmpty_ThenThrows()
        {
            var installer = new Mock<IFile>();
            installer.Setup(f => f.FullPath)
                .Returns("afullpath");
            installer.Setup(f => f.GetContents())
                .Returns(Array.Empty<byte>());

            this.packager
                .Invoking(x => x.UnPack(installer.Object))
                .Should().Throw<AutomateException>()
                .WithMessage(
                    ExceptionMessages.PatternToolkitPackager_InvalidInstallerFile
                        .Format("afullpath"));
        }

        [Fact]
        public void WhenUnPackAndFileContainsInvalidDefinition_ThenThrows()
        {
            var installer = new Mock<IFile>();
            installer.Setup(f => f.FullPath)
                .Returns("afullpath");
            installer.Setup(f => f.GetContents())
                .Returns(new byte[] { 0x01 });

            this.packager
                .Invoking(x => x.UnPack(installer.Object))
                .Should().Throw<AutomateException>()
                .WithMessage(
                    ExceptionMessages.PatternToolkitPackager_InvalidInstallerFile
                        .Format("afullpath"));
        }

        [Fact]
        public void WhenUnPack_ThenReturnsToolkit()
        {
            var toolkit = new ToolkitDefinition(new PatternDefinition("apatternname"));
            var installer = new Mock<IFile>();
            installer.Setup(f => f.FullPath)
                .Returns("afullpath");
            installer.Setup(f => f.GetContents())
                .Returns(CodeTemplateFile.Encoding.GetBytes(toolkit.ToJson(new AutomatePersistableFactory())));

            var result = this.packager.UnPack(installer.Object);

            result.Id.Should().Be(toolkit.Id);
            this.toolkitStore.Verify(ts => ts.Import(It.Is<ToolkitDefinition>(t =>
                t.Id == toolkit.Id
            )));
        }
    }
}