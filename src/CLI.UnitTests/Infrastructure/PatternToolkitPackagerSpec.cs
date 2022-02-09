﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            this.toolkitStore.Setup(store => store.Export(It.IsAny<ToolkitDefinition>()))
                .Returns("alocation");
            this.resolver = new Mock<IFilePathResolver>();
            patternStore.Create("apatternname");
            this.packager = new PatternToolkitPackager(patternStore, this.toolkitStore.Object, this.resolver.Object);
        }

        [Fact]
        public void WhenPackAndVersionInstructionIsAutoAndFirstVersion_ThenPackagesVersion1OfToolkit()
        {
            var pattern = new PatternDefinition { Id = "apatternid", Name = "apatternname" };

            var result = this.packager.Pack(pattern, PatternToolkitPackager.AutoIncrementInstruction);

            result.Toolkit.Version.Should().Be("1.0.0");
        }

        [Fact]
        public void WhenPackAndVersionInstructionIsNullAndFirstVersion_ThenPackagesVersion1OfToolkit()
        {
            var pattern = new PatternDefinition { Id = "apatternid", Name = "apatternname" };

            var result = this.packager.Pack(pattern, null);

            result.Toolkit.Version.Should().Be("1.0.0");
        }

        [Fact]
        public void WhenPackAndVersionInstructionIsAutoAndHasVersion_ThenPackagesNextVersionOfToolkit()
        {
            var pattern = new PatternDefinition { Id = "apatternid", Name = "apatternname", ToolkitVersion = "1.0.0" };

            var result = this.packager.Pack(pattern, PatternToolkitPackager.AutoIncrementInstruction);

            result.Toolkit.Version.Should().Be("2.0.0");
        }

        [Fact]
        public void WhenPackAndVersionInstructionIsNullAndHasVersion_ThenPackagesNextVersionOfToolkit()
        {
            var pattern = new PatternDefinition { Id = "apatternid", Name = "apatternname", ToolkitVersion = "1.0.0" };

            var result = this.packager.Pack(pattern, null);

            result.Toolkit.Version.Should().Be("2.0.0");
        }

        [Fact]
        public void WhenPackAndVersionInstructionIsBeforeCurrentVersion_ThenThrows()
        {
            var pattern = new PatternDefinition { Id = "apatternid", Name = "apatternname", ToolkitVersion = "1.0.0" };

            this.packager
                .Invoking(x => x.Pack(pattern, "0.1"))
                .Should().Throw<AutomateException>()
                .WithMessage(ExceptionMessages.PatternToolkitPackager_VersionBeforeCurrent.Format("0.1", "1.0.0"));
        }

        [Fact]
        public void WhenPackAndVersionInstructionIsInvalid_ThenThrows()
        {
            var pattern = new PatternDefinition { Id = "apatternid", Name = "apatternname", ToolkitVersion = "1.0.0" };

            this.packager
                .Invoking(x => x.Pack(pattern, "notaversionnumber"))
                .Should().Throw<AutomateException>()
                .WithMessage(
                    ExceptionMessages.PatternToolkitPackager_InvalidVersionInstruction.Format("notaversionnumber"));
        }

        [Fact]
        public void WhenPackAndVersionInstructionIsAVersion_ThenPackagesVersionOfToolkit()
        {
            var pattern = new PatternDefinition { Id = "apatternid", Name = "apatternname", ToolkitVersion = "1.0.0" };

            var result = this.packager.Pack(pattern, "2.1");

            result.Toolkit.Version.Should().Be("2.1.0");
        }

        [Fact]
        public void WhenPackAndCodeTemplates_ThenReturnsPackage()
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

            var result = this.packager.Pack(pattern, null);

            result.Toolkit.CodeTemplateFiles.Should().ContainSingle(ctf =>
                ctf.Id == pattern.CodeTemplates.Single().Id && ctf.Contents == fileContents);
            this.toolkitStore.Verify(repo => repo.Export(It.Is<ToolkitDefinition>(toolkit =>
                toolkit.Version == "1.0.0"
                && toolkit.CodeTemplateFiles.Single().Id == pattern.CodeTemplates.Single().Id
                && toolkit.CodeTemplateFiles.Single().Contents == fileContents
            )));
        }

        [Fact]
        public void WhenPack_ThenReturnsPackage()
        {
            var pattern = new PatternDefinition { Id = "apatternid", Name = "apatternname", ToolkitVersion = "0.0.0" };

            var result = this.packager.Pack(pattern, null);

            result.Toolkit.Id.Should().Be("apatternid");
            result.Toolkit.Version.Should().Be("1.0.0");
            result.Toolkit.PatternName.Should().Be("apatternname");
            result.BuiltLocation.Should().Be("alocation");
            this.toolkitStore.Verify(repo => repo.Export(It.Is<ToolkitDefinition>(toolkit =>
                toolkit.Version == "1.0.0"
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
            var toolkit = new ToolkitDefinition
            {
                Id = "atoolkitid",
                Pattern = new PatternDefinition()
            };
            var installer = new Mock<IFile>();
            installer.Setup(f => f.FullPath)
                .Returns("afullpath");
            installer.Setup(f => f.GetContents())
                .Returns(Encoding.UTF8.GetBytes(toolkit.ToJson()));

            var result = this.packager.UnPack(installer.Object);

            result.Id.Should().Be("atoolkitid");
            this.toolkitStore.Verify(ts => ts.Import(It.Is<ToolkitDefinition>(t =>
                t.Id == "atoolkitid"
            )));
        }
    }
}