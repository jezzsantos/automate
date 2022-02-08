using System.Collections.Generic;
using System.Linq;
using automate;
using automate.Domain;
using automate.Extensions;
using automate.Infrastructure;
using FluentAssertions;
using Moq;
using Xunit;

namespace CLI.UnitTests.Infrastructure
{
    [Trait("Category", "Unit")]
    public class PatternToolkitPackagerSpec
    {
        private readonly PatternToolkitPackager packager;
        private readonly Mock<IFilePathResolver> resolver;
        private readonly Mock<IToolkitStore> toolkitStore;

        public PatternToolkitPackagerSpec()
        {
            var repo = new MemoryRepository();
            var patternStore = new PatternStore(repo, repo);
            this.toolkitStore = new Mock<IToolkitStore>();
            this.toolkitStore.Setup(store => store.Save(It.IsAny<PatternToolkitDefinition>()))
                .Returns("alocation");
            this.resolver = new Mock<IFilePathResolver>();
            patternStore.Create("apatternname");
            this.packager = new PatternToolkitPackager(patternStore, this.toolkitStore.Object, this.resolver.Object);
        }

        [Fact]
        public void WhenPackageAndVersionInstructionIsAutoAndFirstVersion_ThenPackagesVersion1OfToolkit()
        {
            var pattern = new PatternDefinition { Id = "apatternid", Name = "apatternname" };

            var result = this.packager.Package(pattern, PatternToolkitPackager.AutoIncrementInstruction);

            result.Toolkit.Version.Should().Be("1.0.0");
        }

        [Fact]
        public void WhenPackageAndVersionInstructionIsNullAndFirstVersion_ThenPackagesVersion1OfToolkit()
        {
            var pattern = new PatternDefinition { Id = "apatternid", Name = "apatternname" };

            var result = this.packager.Package(pattern, null);

            result.Toolkit.Version.Should().Be("1.0.0");
        }

        [Fact]
        public void WhenPackageAndVersionInstructionIsAutoAndHasVersion_ThenPackagesNextVersionOfToolkit()
        {
            var pattern = new PatternDefinition { Id = "apatternid", Name = "apatternname", ToolkitVersion = "1.0.0" };

            var result = this.packager.Package(pattern, PatternToolkitPackager.AutoIncrementInstruction);

            result.Toolkit.Version.Should().Be("2.0.0");
        }

        [Fact]
        public void WhenPackageAndVersionInstructionIsNullAndHasVersion_ThenPackagesNextVersionOfToolkit()
        {
            var pattern = new PatternDefinition { Id = "apatternid", Name = "apatternname", ToolkitVersion = "1.0.0" };

            var result = this.packager.Package(pattern, null);

            result.Toolkit.Version.Should().Be("2.0.0");
        }

        [Fact]
        public void WhenPackageAndVersionInstructionIsBeforeCurrentVersion_ThenThrows()
        {
            var pattern = new PatternDefinition { Id = "apatternid", Name = "apatternname", ToolkitVersion = "1.0.0" };

            this.packager
                .Invoking(x => x.Package(pattern, "0.1"))
                .Should().Throw<PatternException>()
                .WithMessage(ExceptionMessages.PatternToolkitPackager_VersionBeforeCurrent.Format("0.1", "1.0.0"));
        }

        [Fact]
        public void WhenPackageAndVersionInstructionIsInvalid_ThenThrows()
        {
            var pattern = new PatternDefinition { Id = "apatternid", Name = "apatternname", ToolkitVersion = "1.0.0" };

            this.packager
                .Invoking(x => x.Package(pattern, "notaversionnumber"))
                .Should().Throw<PatternException>()
                .WithMessage(
                    ExceptionMessages.PatternToolkitPackager_InvalidVersionInstruction.Format("notaversionnumber"));
        }

        [Fact]
        public void WhenPackageAndVersionInstructionIsAVersion_ThenPackagesVersionOfToolkit()
        {
            var pattern = new PatternDefinition { Id = "apatternid", Name = "apatternname", ToolkitVersion = "1.0.0" };

            var result = this.packager.Package(pattern, "2.1");

            result.Toolkit.Version.Should().Be("2.1.0");
        }

        [Fact]
        public void WhenPackageAndCodeTemplates_ThenReturnsPackage()
        {
            var pattern = new PatternDefinition
            {
                Id = "apatternid", Name = "apatternname", ToolkitVersion = "0.0.0",
                CodeTemplates = new List<CodeTemplate>
                {
                    new CodeTemplate("acodetemplatename", "afullpath")
                }
            };
            var fileContents = new byte[] { 0x01 };
            var file = new Mock<IFile>();
            file.Setup(f => f.GetContents()).Returns(fileContents);
            this.resolver.Setup(res => res.GetFileAtPath(It.IsAny<string>()))
                .Returns(file.Object);

            var result = this.packager.Package(pattern, null);

            result.Toolkit.CodeTemplateFiles.Should().ContainSingle(ctf =>
                ctf.Id == pattern.CodeTemplates.Single().Id && ctf.Contents == fileContents);
            this.toolkitStore.Verify(repo => repo.Save(It.Is<PatternToolkitDefinition>(toolkit =>
                toolkit.Version == "1.0.0"
                && toolkit.CodeTemplateFiles.Single().Id == pattern.CodeTemplates.Single().Id
                && toolkit.CodeTemplateFiles.Single().Contents == fileContents
            )));
        }

        [Fact]
        public void WhenPackage_ThenReturnsPackage()
        {
            var pattern = new PatternDefinition { Id = "apatternid", Name = "apatternname", ToolkitVersion = "0.0.0" };

            var result = this.packager.Package(pattern, null);

            result.Toolkit.Id.Should().Be("apatternid");
            result.Toolkit.Version.Should().Be("1.0.0");
            result.Toolkit.PatternName.Should().Be("apatternname");
            result.BuiltLocation.Should().Be("alocation");
            this.toolkitStore.Verify(repo => repo.Save(It.Is<PatternToolkitDefinition>(toolkit =>
                toolkit.Version == "1.0.0"
                && toolkit.Pattern == pattern
            )));
        }
    }
}