using System;
using System.Collections.Generic;
using Automate.CLI.Domain;
using Automate.CLI.Extensions;
using FluentAssertions;
using Moq;
using Xunit;

namespace CLI.UnitTests.Domain
{
    public class CodeTemplateCommandSpec
    {
        [Trait("Category", "Unit")]
        public class GivenNoCommand
        {
            [Fact]
            public void WhenConstructedAndCodeTemplateIdIsMissing_ThenThrows()
            {
                FluentActions.Invoking(() => new CodeTemplateCommand("aname", null, false, "~/afilepath"))
                    .Should().Throw<ArgumentNullException>();
            }

            [Fact]
            public void WhenConstructedAndFilePathIsMissing_ThenThrows()
            {
                FluentActions.Invoking(() => new CodeTemplateCommand("aname", "acodetemplateid", false, null))
                    .Should().Throw<ArgumentNullException>();
            }

            [Fact]
            public void WhenConstructedAndFilePathIsInvalid_ThenThrows()
            {
                FluentActions.Invoking(() =>
                        new CodeTemplateCommand("aname", "acodetemplateid", false, "^aninvalidfilepath^"))
                    .Should().Throw<ArgumentOutOfRangeException>()
                    .WithMessage(ValidationMessages.Automation_InvalidFilePath.Format("^aninvalidfilepath^") + "*");
            }
        }

        [Trait("Category", "Unit")]
        public class GivenANormalCommand
        {
            private readonly CodeTemplateCommand command;
            private readonly Mock<IFileSystemWriter> fileSystemWriter;
            private readonly Mock<ITextTemplatingEngine> textTemplateEngine;

            public GivenANormalCommand()
            {
                var filePathResolver = new Mock<IFilePathResolver>();
                filePathResolver.Setup(fpr => fpr.CreatePath(It.IsAny<string>(), It.IsAny<string>()))
                    .Returns("c:\\anabsolutepath\\afilename.cs");
                filePathResolver.Setup(fpr => fpr.GetFilename(It.IsAny<string>()))
                    .Returns("afilename.cs");
                this.fileSystemWriter = new Mock<IFileSystemWriter>();
                var solutionPathResolver = new Mock<ISolutionPathResolver>();
                solutionPathResolver
                    .Setup(spr => spr.ResolveExpression(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SolutionItem>()))
                    .Returns((string description, string expr, SolutionItem item) => expr);
                this.textTemplateEngine = new Mock<ITextTemplatingEngine>();

                this.command = new CodeTemplateCommand(filePathResolver.Object, this.fileSystemWriter.Object,
                    solutionPathResolver.Object, this.textTemplateEngine.Object, "acommandname", "acodetemplateid",
                    false, "~/afilepath.cs");
            }

            [Fact]
            public void WhenExecuteAndNoArtifactLinkAndFileNotExist_ThenGeneratesFileAndAddsArtifactLink()
            {
                var target = new SolutionItem(new Element("anelementname"), null);
                var toolkit = new ToolkitDefinition(new PatternDefinition("apatternname"));
                toolkit.AddCodeTemplateFiles(new List<CodeTemplateFile> { new CodeTemplateFile(CodeTemplateFile.Encoding.GetBytes("atemplate"), "acodetemplateid") });
                var solution = new SolutionDefinition(toolkit);
                this.textTemplateEngine.Setup(tte => tte.Transform(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SolutionItem>()))
                    .Returns("acontent");
                this.fileSystemWriter.Setup(fsw => fsw.Exists(It.IsAny<string>()))
                    .Returns(false);

                var result = this.command.Execute(solution, target);

                result.CommandName.Should().Be("acommandname");
                result.Log.Should()
                    .ContainSingle(DomainMessages.CodeTemplateCommand_Log_GeneratedFile.Format("afilename.cs",
                        "c:\\anabsolutepath\\afilename.cs"));
                target.ArtifactLinks.Should().ContainSingle(link =>
                    link.CommandId == this.command.Id
                    && link.Tag == "afilename.cs"
                    && link.Path == "c:\\anabsolutepath\\afilename.cs");
                this.textTemplateEngine.Verify(tte => tte.Transform(It.IsAny<string>(), "atemplate", target));
                this.fileSystemWriter.Verify(fw => fw.Write(It.Is<string>(content =>
                    content == "acontent"), "c:\\anabsolutepath\\afilename.cs"));
                this.fileSystemWriter.Verify(fsw => fsw.Delete(It.IsAny<string>()), Times.Never);
                this.fileSystemWriter.Verify(fsw => fsw.Move(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            }

            [Fact]
            public void WhenExecuteAndNoArtifactLinkButFileExists_ThenOverwritesFileAndAddsArtifactLink()
            {
                var ownerSolution = new SolutionItem(new Element("anelementname"), null);
                var toolkit = new ToolkitDefinition(new PatternDefinition("apatternname"));
                toolkit.AddCodeTemplateFiles(new List<CodeTemplateFile> { new CodeTemplateFile(CodeTemplateFile.Encoding.GetBytes("atemplate"), "acodetemplateid") });
                this.textTemplateEngine.Setup(tte => tte.Transform(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SolutionItem>()))
                    .Returns("acontent");
                this.fileSystemWriter.Setup(fsw => fsw.Exists(It.IsAny<string>()))
                    .Returns(true);
                var solution = new SolutionDefinition(toolkit);

                var result = this.command.Execute(solution, ownerSolution);

                result.CommandName.Should().Be("acommandname");
                result.Log.Should()
                    .ContainSingle(DomainMessages.CodeTemplateCommand_Log_GeneratedFile.Format("afilename.cs",
                        "c:\\anabsolutepath\\afilename.cs"));
                ownerSolution.ArtifactLinks.Should().ContainSingle(link =>
                    link.CommandId == this.command.Id
                    && link.Tag == "afilename.cs"
                    && link.Path == "c:\\anabsolutepath\\afilename.cs");
                this.textTemplateEngine.Verify(tte => tte.Transform(It.IsAny<string>(), "atemplate", ownerSolution));
                this.fileSystemWriter.Verify(fw => fw.Write(It.Is<string>(content =>
                    content == "acontent"), "c:\\anabsolutepath\\afilename.cs"));
                this.fileSystemWriter.Verify(fsw => fsw.Delete(It.IsAny<string>()), Times.Never);
                this.fileSystemWriter.Verify(fsw => fsw.Move(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            }

            [Fact]
            public void WhenExecuteAndDifferentArtifactLinkExistsButFileNotExists_ThenDeletesLinkedFileAndGeneratesNewFileAndUpdatesArtifactLink()
            {
                var ownerSolution = new SolutionItem(new Element("anelementname"), null);
                ownerSolution.AddArtifactLink(this.command.Id, "anoriginalpath", "atag");
                var toolkit = new ToolkitDefinition(new PatternDefinition("apatternname"));
                toolkit.AddCodeTemplateFiles(new List<CodeTemplateFile> { new CodeTemplateFile(CodeTemplateFile.Encoding.GetBytes("atemplate"), "acodetemplateid") });
                this.textTemplateEngine.Setup(tte => tte.Transform(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SolutionItem>()))
                    .Returns("acontent");
                this.fileSystemWriter.Setup(fsw => fsw.Exists(It.IsAny<string>()))
                    .Returns(false);
                var solution = new SolutionDefinition(toolkit);

                var result = this.command.Execute(solution, ownerSolution);

                result.CommandName.Should().Be("acommandname");
                result.Log.Should()
                    .Contain(DomainMessages.CodeTemplateCommand_Log_GeneratedFile.Format("afilename.cs",
                        "c:\\anabsolutepath\\afilename.cs"));
                result.Log.Should()
                    .Contain(DomainMessages.CodeTemplateCommand_Log_Warning_Deleted.Format("anoriginalpath"));
                ownerSolution.ArtifactLinks.Should().ContainSingle(link =>
                    link.CommandId == this.command.Id
                    && link.Tag == "afilename.cs"
                    && link.Path == "c:\\anabsolutepath\\afilename.cs");
                this.textTemplateEngine.Verify(tte => tte.Transform(It.IsAny<string>(), "atemplate", ownerSolution));
                this.fileSystemWriter.Verify(fw => fw.Write(It.Is<string>(content =>
                    content == "acontent"), "c:\\anabsolutepath\\afilename.cs"));
                this.fileSystemWriter.Verify(fsw => fsw.Delete("anoriginalpath"));
                this.fileSystemWriter.Verify(fsw => fsw.Move(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            }

            [Fact]
            public void WhenExecuteAndDifferentArtifactLinkExistsAndFileExists_ThenDeletesLinkedFileAndOverwritesFileAndUpdatesArtifactLink()
            {
                var ownerSolution = new SolutionItem(new Element("anelementname"), null);
                ownerSolution.AddArtifactLink(this.command.Id, "anoriginalpath", "atag");
                var toolkit = new ToolkitDefinition(new PatternDefinition("apatternname"));
                toolkit.AddCodeTemplateFiles(new List<CodeTemplateFile> { new CodeTemplateFile(CodeTemplateFile.Encoding.GetBytes("atemplate"), "acodetemplateid") });
                this.textTemplateEngine.Setup(tte => tte.Transform(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SolutionItem>()))
                    .Returns("acontent");
                this.fileSystemWriter.Setup(fsw => fsw.Exists(It.IsAny<string>()))
                    .Returns(true);
                var solution = new SolutionDefinition(toolkit);

                var result = this.command.Execute(solution, ownerSolution);

                result.CommandName.Should().Be("acommandname");
                result.Log.Should()
                    .Contain(DomainMessages.CodeTemplateCommand_Log_GeneratedFile.Format("afilename.cs",
                        "c:\\anabsolutepath\\afilename.cs"));
                result.Log.Should()
                    .Contain(DomainMessages.CodeTemplateCommand_Log_Warning_Deleted.Format("anoriginalpath"));
                ownerSolution.ArtifactLinks.Should().ContainSingle(link =>
                    link.CommandId == this.command.Id
                    && link.Tag == "afilename.cs"
                    && link.Path == "c:\\anabsolutepath\\afilename.cs");
                this.textTemplateEngine.Verify(tte => tte.Transform(It.IsAny<string>(), "atemplate", ownerSolution));
                this.fileSystemWriter.Verify(fw => fw.Write(It.Is<string>(content =>
                    content == "acontent"), "c:\\anabsolutepath\\afilename.cs"));
                this.fileSystemWriter.Verify(fsw => fsw.Delete("anoriginalpath"));
                this.fileSystemWriter.Verify(fsw => fsw.Move(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            }

            [Fact]
            public void WhenExecuteAndSameArtifactLinkExistsButFileNotExists_ThenGeneratesNewFileAndUpdatesArtifactLink()
            {
                var ownerSolution = new SolutionItem(new Element("anelementname"), null);
                ownerSolution.AddArtifactLink(this.command.Id, "c:\\anabsolutepath\\afilename.cs", "atag");
                var toolkit = new ToolkitDefinition(new PatternDefinition("apatternname"));
                toolkit.AddCodeTemplateFiles(new List<CodeTemplateFile> { new CodeTemplateFile(CodeTemplateFile.Encoding.GetBytes("atemplate"), "acodetemplateid") });
                this.textTemplateEngine.Setup(tte => tte.Transform(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SolutionItem>()))
                    .Returns("acontent");
                this.fileSystemWriter.Setup(fsw => fsw.Exists(It.IsAny<string>()))
                    .Returns(false);
                var solution = new SolutionDefinition(toolkit);

                var result = this.command.Execute(solution, ownerSolution);

                result.CommandName.Should().Be("acommandname");
                result.Log.Should()
                    .ContainSingle(DomainMessages.CodeTemplateCommand_Log_GeneratedFile.Format("afilename.cs",
                        "c:\\anabsolutepath\\afilename.cs"));
                ownerSolution.ArtifactLinks.Should().ContainSingle(link =>
                    link.CommandId == this.command.Id
                    && link.Tag == "afilename.cs"
                    && link.Path == "c:\\anabsolutepath\\afilename.cs");
                this.textTemplateEngine.Verify(tte => tte.Transform(It.IsAny<string>(), "atemplate", ownerSolution));
                this.fileSystemWriter.Verify(fw => fw.Write(It.Is<string>(content =>
                    content == "acontent"), "c:\\anabsolutepath\\afilename.cs"));
                this.fileSystemWriter.Verify(fsw => fsw.Delete(It.IsAny<string>()), Times.Never);
                this.fileSystemWriter.Verify(fsw => fsw.Move(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            }

            [Fact]
            public void WhenExecuteAndSameArtifactLinkExistsAndFileExists_ThenOverwritesFileAndUpdatesArtifactLink()
            {
                var ownerSolution = new SolutionItem(new Element("anelementname"), null);
                ownerSolution.AddArtifactLink(this.command.Id, "c:\\anabsolutepath\\afilename.cs", "atag");
                var toolkit = new ToolkitDefinition(new PatternDefinition("apatternname"));
                toolkit.AddCodeTemplateFiles(new List<CodeTemplateFile> { new CodeTemplateFile(CodeTemplateFile.Encoding.GetBytes("atemplate"), "acodetemplateid") });
                this.textTemplateEngine.Setup(tte => tte.Transform(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SolutionItem>()))
                    .Returns("acontent");
                this.fileSystemWriter.Setup(fsw => fsw.Exists(It.IsAny<string>()))
                    .Returns(true);
                var solution = new SolutionDefinition(toolkit);

                var result = this.command.Execute(solution, ownerSolution);

                result.CommandName.Should().Be("acommandname");
                result.Log.Should()
                    .ContainSingle(DomainMessages.CodeTemplateCommand_Log_GeneratedFile.Format("afilename.cs",
                        "c:\\anabsolutepath\\afilename.cs"));
                ownerSolution.ArtifactLinks.Should().ContainSingle(link =>
                    link.CommandId == this.command.Id
                    && link.Tag == "afilename.cs"
                    && link.Path == "c:\\anabsolutepath\\afilename.cs");
                this.textTemplateEngine.Verify(tte => tte.Transform(It.IsAny<string>(), "atemplate", ownerSolution));
                this.fileSystemWriter.Verify(fw => fw.Write(It.Is<string>(content =>
                    content == "acontent"), "c:\\anabsolutepath\\afilename.cs"));
                this.fileSystemWriter.Verify(fsw => fsw.Delete(It.IsAny<string>()), Times.Never);
                this.fileSystemWriter.Verify(fsw => fsw.Move(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            }
            
        }

        [Trait("Category", "Unit")]
        public class GivenATearOffCommand
        {
            private readonly CodeTemplateCommand command;
            private readonly Mock<IFileSystemWriter> fileSystemWriter;
            private readonly Mock<ITextTemplatingEngine> textTemplateEngine;

            public GivenATearOffCommand()
            {
                var filePathResolver = new Mock<IFilePathResolver>();
                filePathResolver.Setup(fpr => fpr.CreatePath(It.IsAny<string>(), It.IsAny<string>()))
                    .Returns("c:\\anabsolutepath\\afilename.cs");
                filePathResolver.Setup(fpr => fpr.GetFilename(It.IsAny<string>()))
                    .Returns("afilename.cs");
                this.fileSystemWriter = new Mock<IFileSystemWriter>();
                var solutionPathResolver = new Mock<ISolutionPathResolver>();
                solutionPathResolver
                    .Setup(spr => spr.ResolveExpression(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SolutionItem>()))
                    .Returns((string description, string expr, SolutionItem item) => expr);
                this.textTemplateEngine = new Mock<ITextTemplatingEngine>();

                this.command = new CodeTemplateCommand(filePathResolver.Object, this.fileSystemWriter.Object,
                    solutionPathResolver.Object, this.textTemplateEngine.Object, "acommandname", "acodetemplateid",
                    true, "~/afilepath.cs");
            }

            [Fact]
            public void WhenExecuteAndNoArtifactLinkAndFileExists_ThenOnlyAddsArtifactLink()
            {
                var ownerSolution = new SolutionItem(new Element("anelementname"), null);
                var toolkit = new ToolkitDefinition(new PatternDefinition("apatternname"));
                toolkit.AddCodeTemplateFiles(new List<CodeTemplateFile> { new CodeTemplateFile(CodeTemplateFile.Encoding.GetBytes("atemplate"), "acodetemplateid") });
                this.fileSystemWriter.Setup(fsw => fsw.Exists(It.IsAny<string>()))
                    .Returns(true);
                var solution = new SolutionDefinition(toolkit);

                var result = this.command.Execute(solution, ownerSolution);

                result.CommandName.Should().Be("acommandname");
                result.Log.Should()
                    .ContainSingle(DomainMessages.CodeTemplateCommand_Log_UpdatedLink.Format("afilename.cs",
                        "c:\\anabsolutepath\\afilename.cs"));
                ownerSolution.ArtifactLinks.Should().ContainSingle(link =>
                    link.CommandId == this.command.Id
                    && link.Tag == "afilename.cs"
                    && link.Path == "c:\\anabsolutepath\\afilename.cs");
                this.textTemplateEngine.Verify(tte => tte.Transform(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SolutionItem>()),
                    Times.Never);
                this.fileSystemWriter.Verify(fw => fw.Write(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
                this.fileSystemWriter.Verify(fsw => fsw.Delete(It.IsAny<string>()), Times.Never);
                this.fileSystemWriter.Verify(fsw => fsw.Move(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            }

            [Fact]
            public void WhenExecuteAndDifferentArtifactLinkExistsAndFileExists_ThenDeletesLinkedFileAndUpdatesArtifactLink()
            {
                var ownerSolution = new SolutionItem(new Element("anelementname"), null);
                ownerSolution.AddArtifactLink(this.command.Id, "anoriginalpath", "atag");
                var toolkit = new ToolkitDefinition(new PatternDefinition("apatternname"));
                toolkit.AddCodeTemplateFiles(new List<CodeTemplateFile> { new CodeTemplateFile(CodeTemplateFile.Encoding.GetBytes("atemplate"), "acodetemplateid") });
                this.textTemplateEngine.Setup(tte => tte.Transform(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SolutionItem>()))
                    .Returns("acontent");
                this.fileSystemWriter.Setup(fsw => fsw.Exists(It.IsAny<string>()))
                    .Returns(true);
                var solution = new SolutionDefinition(toolkit);

                var result = this.command.Execute(solution, ownerSolution);

                result.CommandName.Should().Be("acommandname");
                result.Log.Should()
                    .Contain(DomainMessages.CodeTemplateCommand_Log_Warning_Deleted.Format("anoriginalpath"));
                ownerSolution.ArtifactLinks.Should().ContainSingle(link =>
                    link.CommandId == this.command.Id
                    && link.Tag == "afilename.cs"
                    && link.Path == "c:\\anabsolutepath\\afilename.cs");
                this.textTemplateEngine.Verify(tte => tte.Transform(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SolutionItem>()),
                    Times.Never);
                this.fileSystemWriter.Verify(fw => fw.Write(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
                this.fileSystemWriter.Verify(fsw => fsw.Delete("anoriginalpath"));
                this.fileSystemWriter.Verify(fsw => fsw.Move(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            }

            [Fact]
            public void WhenExecuteAndDifferentArtifactLinkExistsButFileNotExists_ThenMovesLinkedFileAndUpdatesArtifactLink()
            {
                var ownerSolution = new SolutionItem(new Element("anelementname"), null);
                ownerSolution.AddArtifactLink(this.command.Id, "apath", "atag");
                var toolkit = new ToolkitDefinition(new PatternDefinition("apatternname"));
                toolkit.AddCodeTemplateFiles(new List<CodeTemplateFile> { new CodeTemplateFile(CodeTemplateFile.Encoding.GetBytes("atemplate"), "acodetemplateid") });
                this.textTemplateEngine.Setup(tte => tte.Transform(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SolutionItem>()))
                    .Returns("acontent");
                this.fileSystemWriter.Setup(fsw => fsw.Exists(It.IsAny<string>()))
                    .Returns(false);
                var solution = new SolutionDefinition(toolkit);

                var result = this.command.Execute(solution, ownerSolution);

                result.CommandName.Should().Be("acommandname");
                result.Log.Should()
                    .ContainSingle(DomainMessages.CodeTemplateCommand_Log_Warning_Moved.Format("apath",
                        "c:\\anabsolutepath\\afilename.cs"));
                ownerSolution.ArtifactLinks.Should().ContainSingle(link =>
                    link.CommandId == this.command.Id
                    && link.Tag == "afilename.cs"
                    && link.Path == "c:\\anabsolutepath\\afilename.cs");
                this.textTemplateEngine.Verify(tte => tte.Transform(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SolutionItem>()), Times.Never);
                this.fileSystemWriter.Verify(fw => fw.Write(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
                this.fileSystemWriter.Verify(fsw => fsw.Delete(It.IsAny<string>()), Times.Never);
                this.fileSystemWriter.Verify(fsw => fsw.Move("apath", "c:\\anabsolutepath\\afilename.cs"));
            }

            [Fact]
            public void WhenExecuteAndSameArtifactLinkExistsAndFileExists_ThenDoesNothing()
            {
                var ownerSolution = new SolutionItem(new Element("anelementname"), null);
                ownerSolution.AddArtifactLink(this.command.Id, "c:\\anabsolutepath\\afilename.cs", "atag");
                var toolkit = new ToolkitDefinition(new PatternDefinition("apatternname"));
                toolkit.AddCodeTemplateFiles(new List<CodeTemplateFile> { new CodeTemplateFile(CodeTemplateFile.Encoding.GetBytes("atemplate"), "acodetemplateid") });
                this.textTemplateEngine.Setup(tte => tte.Transform(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SolutionItem>()))
                    .Returns("acontent");
                this.fileSystemWriter.Setup(fsw => fsw.Exists(It.IsAny<string>()))
                    .Returns(true);
                var solution = new SolutionDefinition(toolkit);

                var result = this.command.Execute(solution, ownerSolution);

                result.CommandName.Should().Be("acommandname");
                result.Log.Should().BeEmpty();
                ownerSolution.ArtifactLinks.Should().ContainSingle(link =>
                    link.CommandId == this.command.Id
                    && link.Tag == "afilename.cs"
                    && link.Path == "c:\\anabsolutepath\\afilename.cs");
                this.textTemplateEngine.Verify(tte => tte.Transform(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SolutionItem>()),
                    Times.Never);
                this.fileSystemWriter.Verify(fw => fw.Write(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
                this.fileSystemWriter.Verify(fsw => fsw.Delete(It.IsAny<string>()), Times.Never);
                this.fileSystemWriter.Verify(fsw => fsw.Move(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            }

            [Fact]
            public void WhenExecuteAndSameArtifactLinkExistsButFileNotExists_ThenGeneratesNewFileAndUpdatesArtifactLink()
            {
                var ownerSolution = new SolutionItem(new Element("anelementname"), null);
                ownerSolution.AddArtifactLink(this.command.Id, "c:\\anabsolutepath\\afilename.cs", "atag");
                var toolkit = new ToolkitDefinition(new PatternDefinition("apatternname"));
                toolkit.AddCodeTemplateFiles(new List<CodeTemplateFile> { new CodeTemplateFile(CodeTemplateFile.Encoding.GetBytes("atemplate"), "acodetemplateid") });
                this.textTemplateEngine.Setup(tte => tte.Transform(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SolutionItem>()))
                    .Returns("acontent");
                this.fileSystemWriter.Setup(fsw => fsw.Exists(It.IsAny<string>()))
                    .Returns(false);
                var solution = new SolutionDefinition(toolkit);

                var result = this.command.Execute(solution, ownerSolution);

                result.CommandName.Should().Be("acommandname");
                result.Log.Should()
                    .Contain(DomainMessages.CodeTemplateCommand_Log_GeneratedFile.Format("afilename.cs",
                        "c:\\anabsolutepath\\afilename.cs"));
                ownerSolution.ArtifactLinks.Should().ContainSingle(link =>
                    link.CommandId == this.command.Id
                    && link.Tag == "afilename.cs"
                    && link.Path == "c:\\anabsolutepath\\afilename.cs");
                this.textTemplateEngine.Verify(tte => tte.Transform(It.IsAny<string>(), "atemplate", ownerSolution));
                this.fileSystemWriter.Verify(fw => fw.Write(It.Is<string>(content =>
                    content == "acontent"), "c:\\anabsolutepath\\afilename.cs"));
                this.fileSystemWriter.Verify(fsw => fsw.Delete(It.IsAny<string>()), Times.Never);
                this.fileSystemWriter.Verify(fsw => fsw.Move(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            }
        }
    }
}