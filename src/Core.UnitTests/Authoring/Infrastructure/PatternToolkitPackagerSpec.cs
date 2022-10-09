using System;
using System.Linq;
using Automate.Authoring.Application;
using Automate.Authoring.Domain;
using Automate.Authoring.Infrastructure;
using Automate.Common;
using Automate.Common.Domain;
using Automate.Common.Extensions;
using Automate.Common.Infrastructure;
using FluentAssertions;
using Moq;
using Xunit;

namespace Core.UnitTests.Authoring.Infrastructure
{
    [Trait("Category", "Unit")]
    public class PatternToolkitPackagerSpec
    {
        private readonly Mock<IAssemblyMetadata> metadata;
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
            this.packager = new PatternToolkitPackager(this.patternStore.Object,
                this.toolkitStore.Object);
            this.metadata = new Mock<IAssemblyMetadata>();
            this.metadata.Setup(rm => rm.ProductName).Returns("aproductname");
            this.metadata.Setup(rm => rm.RuntimeVersion).Returns(MachineConstants.GetRuntimeVersion);
        }

        [Fact]
        public void WhenPackAndExportAndNoVersion_ThenPackagesFirstVersionOfToolkit()
        {
            var pattern = new PatternDefinition("apatternname");

            var result =
                this.packager.PackAndExport(this.metadata.Object, pattern,
                    new VersionInstruction(PatternVersioningHistory.AutoIncrementInstruction));

            result.Toolkit.Version.Should().Be("0.1.0");
            this.toolkitStore.Verify(ts => ts.Export(It.Is<ToolkitDefinition>(tk => tk.Version == "0.1.0")));
        }

        [Fact]
        public void WhenPackAndExportAndNoVersionWithInstruction_ThenPackagesFirstVersionOfToolkit()
        {
            var pattern = new PatternDefinition("apatternname");

            var result =
                this.packager.PackAndExport(this.metadata.Object, pattern,
                    new VersionInstruction(PatternVersioningHistory.AutoIncrementInstruction));

            result.Toolkit.Version.Should().Be("0.1.0");
            this.patternStore.Verify(ps =>
                ps.Save(It.Is<PatternDefinition>(pat => pat.ToolkitVersion.Current == "0.1.0")));
            this.toolkitStore.Verify(ts => ts.Export(It.Is<ToolkitDefinition>(tk => tk.Version == "0.1.0")));
        }

        [Fact]
        public void WhenPackAndExportWithAnyVersionAndNoChanges_ThenPackagesLastVersionOfToolkit()
        {
            var pattern = new PatternDefinition("apatternname");
            var result1 =
                this.packager.PackAndExport(this.metadata.Object, pattern,
                    new VersionInstruction(PatternVersioningHistory.AutoIncrementInstruction));

            var result2 =
                this.packager.PackAndExport(this.metadata.Object, pattern,
                    new VersionInstruction(PatternVersioningHistory.AutoIncrementInstruction));

            result1.Toolkit.Version.Should().Be("0.1.0");
            result2.Toolkit.Version.Should().Be("0.1.0");
            this.patternStore.Verify(ps =>
                ps.Save(It.Is<PatternDefinition>(pat => pat.ToolkitVersion.Current == "0.1.0")));
            this.toolkitStore.Verify(ts => ts.Export(It.Is<ToolkitDefinition>(tk => tk.Version == "0.1.0")),
                Times.Exactly(2));
        }

        [Fact]
        public void WhenPackAndExportWithAnyVersionAndChangesToCodeTemplates_ThenPackagesNextVersionOfToolkit()
        {
            var pattern = new PatternDefinition("apatternname");
            pattern.AddCodeTemplate(new CodeTemplate("acodetemplatename1", "afullpath1", "anextension1"));
            this.patternStore.Setup(ps => ps.GetCurrent())
                .Returns(pattern);
            var fileContents = new byte[] { 0x01 };
            this.patternStore.Setup(ps =>
                    ps.DownloadCodeTemplate(It.IsAny<PatternDefinition>(), It.IsAny<CodeTemplate>()))
                .Returns(new CodeTemplateContent { Content = fileContents, LastModifiedUtc = DateTime.UtcNow });
            var result1 =
                this.packager.PackAndExport(this.metadata.Object, pattern,
                    new VersionInstruction(PatternVersioningHistory.AutoIncrementInstruction));

            var result2 =
                this.packager.PackAndExport(this.metadata.Object, pattern,
                    new VersionInstruction(PatternVersioningHistory.AutoIncrementInstruction));

            result1.Toolkit.Version.Should().Be("0.1.0");
            result2.Toolkit.Version.Should().Be("0.1.0");
            this.patternStore.Verify(ps =>
                ps.Save(It.Is<PatternDefinition>(pat => pat.ToolkitVersion.Current == "0.1.0")));
            this.toolkitStore.Verify(ts => ts.Export(It.Is<ToolkitDefinition>(tk => tk.Version == "0.1.0")),
                Times.Exactly(2));
        }

        [Fact]
        public void WhenPackAndExportAndVersionInstructionIsAVersion_ThenPackagesVersionOfToolkit()
        {
            var pattern = new PatternDefinition("apatternname");
            pattern.UpdateToolkitVersion(new VersionInstruction("1.0.0"));

            var result = this.packager.PackAndExport(this.metadata.Object, pattern, new VersionInstruction("2.1.0"));

            result.Toolkit.Version.Should().Be("2.1.0");
            this.patternStore.Verify(ps =>
                ps.Save(It.Is<PatternDefinition>(pat => pat.ToolkitVersion.Current == "2.1.0")));
            this.toolkitStore.Verify(ts => ts.Export(It.Is<ToolkitDefinition>(tk => tk.Version == "2.1.0")));
        }

        [Fact]
        public void WhenPackAndExportAndCodeTemplates_ThenReturnsPackage()
        {
            var pattern = new PatternDefinition("apatternname");
            pattern.AddCodeTemplate(new CodeTemplate("acodetemplatename1", "afullpath1", "anextension1"));
            var element = new Element("anelementname");
            element.AddCodeTemplate(new CodeTemplate("acodetemplatename2", "afullpath2", "anextension2"));
            pattern.AddElement(element);
            var fileContents = new byte[] { 0x01 };
            this.patternStore.Setup(ps => ps.GetCurrent())
                .Returns(pattern);
            this.patternStore.Setup(ps =>
                    ps.DownloadCodeTemplate(It.IsAny<PatternDefinition>(), It.IsAny<CodeTemplate>()))
                .Returns(new CodeTemplateContent { Content = fileContents });

            var result = this.packager.PackAndExport(this.metadata.Object, pattern, new VersionInstruction());

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
            this.patternStore.Verify(ps =>
                ps.DownloadCodeTemplate(pattern, pattern.Elements.Single().CodeTemplates.Single()));
            this.patternStore.Verify(ps =>
                ps.Save(It.Is<PatternDefinition>(pat => pat.ToolkitVersion.Current == "0.1.0")));
        }

        [Fact]
        public void WhenPackAndExport_ThenReturnsPackage()
        {
            var pattern = new PatternDefinition("apatternname");

            var result = this.packager.PackAndExport(this.metadata.Object, pattern, new VersionInstruction());

            result.Toolkit.Id.Should().NotBeNull();
            result.Toolkit.Version.Should().Be("0.1.0");
            result.Toolkit.PatternName.Should().Be("apatternname");
            result.ExportedLocation.Should().Be("alocation");
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
                .Invoking(x => x.UnPack(this.metadata.Object, installer.Object))
                .Should().Throw<AutomateException>()
                .WithMessage(
                    ExceptionMessages.PatternToolkitPackager_InvalidInstallerFile
                        .Substitute("afullpath"));
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
                .Invoking(x => x.UnPack(this.metadata.Object, installer.Object))
                .Should().Throw<AutomateException>()
                .WithMessage(
                    ExceptionMessages.PatternToolkitPackager_InvalidInstallerFile
                        .Substitute("afullpath"));
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

            var result = this.packager.UnPack(this.metadata.Object, installer.Object);

            result.Id.Should().Be(toolkit.Id);
            this.toolkitStore.Verify(ts => ts.Import(It.Is<ToolkitDefinition>(t =>
                t.Id == toolkit.Id
            )));
        }
    }
}