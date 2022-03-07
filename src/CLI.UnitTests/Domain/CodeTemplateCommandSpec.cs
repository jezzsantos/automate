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
            public void WhenConstructedAndNameIsMissing_ThenThrows()
            {
                FluentActions.Invoking(() => new CodeTemplateCommand(null, "acodetemplateid", false, "~/afilepath"))
                    .Should().Throw<ArgumentNullException>();
            }

            [Fact]
            public void WhenConstructedAndNameIsInvalid_ThenThrows()
            {
                FluentActions.Invoking(() =>
                        new CodeTemplateCommand("^aninvalidname^", "acodetemplateid", false, "~/afilepath"))
                    .Should().Throw<ArgumentOutOfRangeException>()
                    .WithMessage(ValidationMessages.InvalidNameIdentifier.Format("^aninvalidname^") + "*");
            }

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
                var target = new SolutionItem();
                var toolkit = new ToolkitDefinition(new PatternDefinition("apatternname"), "1.0")
                {
                    CodeTemplateFiles = new List<CodeTemplateFile>
                    {
                        new CodeTemplateFile
                        {
                            Id = "acodetemplateid",
                            Contents = CodeTemplateFile.Encoding.GetBytes("atemplate")
                        }
                    }
                };
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
                this.textTemplateEngine.Verify(tte => tte.Transform(It.IsAny<string>(), "atemplate", target));
                this.fileSystemWriter.Verify(fw => fw.Write(It.Is<string>(content =>
                    content == "acontent"), "c:\\anabsolutepath\\afilename.cs"));
                target.ArtifactLinks.Should().ContainSingle(link =>
                    link.CommandId == this.command.Id
                    && link.Tag == "afilename.cs"
                    && link.Path == "c:\\anabsolutepath\\afilename.cs");
            }

            [Fact]
            public void WhenExecuteAndNoArtifactLinkButFileExists_ThenGeneratesFileAndAddsArtifactLink()
            {
                var ownerSolution = new SolutionItem();
                var toolkit = new ToolkitDefinition(new PatternDefinition("apatternname"), "1.0")
                {
                    CodeTemplateFiles = new List<CodeTemplateFile>
                    {
                        new CodeTemplateFile
                        {
                            Id = "acodetemplateid",
                            Contents = CodeTemplateFile.Encoding.GetBytes("atemplate")
                        }
                    }
                };
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
                this.textTemplateEngine.Verify(tte => tte.Transform(It.IsAny<string>(), "atemplate", ownerSolution));
                this.fileSystemWriter.Verify(fw => fw.Write(It.Is<string>(content =>
                    content == "acontent"), "c:\\anabsolutepath\\afilename.cs"));
                ownerSolution.ArtifactLinks.Should().ContainSingle(link =>
                    link.CommandId == this.command.Id
                    && link.Tag == "afilename.cs"
                    && link.Path == "c:\\anabsolutepath\\afilename.cs");
            }

            [Fact]
            public void WhenExecuteAndArtifactLinkButFileNotExists_ThenGeneratesFileAndUpdatesArtifactLink()
            {
                var ownerSolution = new SolutionItem
                {
                    ArtifactLinks = new List<ArtifactLink>
                    {
                        new ArtifactLink(this.command.Id, "apath", "atag")
                    }
                };
                var toolkit = new ToolkitDefinition(new PatternDefinition("apatternname"), "1.0")
                {
                    CodeTemplateFiles = new List<CodeTemplateFile>
                    {
                        new CodeTemplateFile
                        {
                            Id = "acodetemplateid",
                            Contents = CodeTemplateFile.Encoding.GetBytes("atemplate")
                        }
                    }
                };
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
                this.textTemplateEngine.Verify(tte => tte.Transform(It.IsAny<string>(), "atemplate", ownerSolution));
                this.fileSystemWriter.Verify(fw => fw.Write(It.Is<string>(content =>
                    content == "acontent"), "c:\\anabsolutepath\\afilename.cs"));
                ownerSolution.ArtifactLinks.Should().ContainSingle(link =>
                    link.CommandId == this.command.Id
                    && link.Tag == "afilename.cs"
                    && link.Path == "c:\\anabsolutepath\\afilename.cs");
            }

            [Fact]
            public void WhenExecuteAndArtifactLinkAndFileExists_ThenGeneratesFileAndUpdatesArtifactLink()
            {
                var ownerSolution = new SolutionItem
                {
                    ArtifactLinks = new List<ArtifactLink>
                    {
                        new ArtifactLink(this.command.Id, "apath", "atag")
                    }
                };
                var toolkit = new ToolkitDefinition(new PatternDefinition("apatternname"), "1.0")
                {
                    CodeTemplateFiles = new List<CodeTemplateFile>
                    {
                        new CodeTemplateFile
                        {
                            Id = "acodetemplateid",
                            Contents = CodeTemplateFile.Encoding.GetBytes("atemplate")
                        }
                    }
                };
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
                this.textTemplateEngine.Verify(tte => tte.Transform(It.IsAny<string>(), "atemplate", ownerSolution));
                this.fileSystemWriter.Verify(fw => fw.Write(It.Is<string>(content =>
                    content == "acontent"), "c:\\anabsolutepath\\afilename.cs"));
                ownerSolution.ArtifactLinks.Should().ContainSingle(link =>
                    link.CommandId == this.command.Id
                    && link.Tag == "afilename.cs"
                    && link.Path == "c:\\anabsolutepath\\afilename.cs");
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
            public void WhenExecuteAndNoArtifactLinkAndFileExists_ThenAddsArtifactLink()
            {
                var ownerSolution = new SolutionItem();
                var toolkit = new ToolkitDefinition(new PatternDefinition("apatternname"), "1.0")
                {
                    CodeTemplateFiles = new List<CodeTemplateFile>
                    {
                        new CodeTemplateFile
                        {
                            Id = "acodetemplateid",
                            Contents = CodeTemplateFile.Encoding.GetBytes("atemplate")
                        }
                    }
                };
                this.fileSystemWriter.Setup(fsw => fsw.Exists(It.IsAny<string>()))
                    .Returns(true);
                var solution = new SolutionDefinition(toolkit);

                var result = this.command.Execute(solution, ownerSolution);

                result.CommandName.Should().Be("acommandname");
                result.Log.Should()
                    .ContainSingle(DomainMessages.CodeTemplateCommand_Log_UpdatedLink.Format("afilename.cs",
                        "c:\\anabsolutepath\\afilename.cs"));
                this.textTemplateEngine.Verify(tte => tte.Transform(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SolutionItem>()),
                    Times.Never);
                this.fileSystemWriter.Verify(fw => fw.Write(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
                ownerSolution.ArtifactLinks.Should().ContainSingle(link =>
                    link.CommandId == this.command.Id
                    && link.Tag == "afilename.cs"
                    && link.Path == "c:\\anabsolutepath\\afilename.cs");
            }

            [Fact]
            public void WhenExecuteAndArtifactLinkAndFileExists_ThenDoesNothing()
            {
                var ownerSolution = new SolutionItem
                {
                    ArtifactLinks = new List<ArtifactLink>
                    {
                        new ArtifactLink(this.command.Id, "apath", "atag")
                    }
                };
                var toolkit = new ToolkitDefinition(new PatternDefinition("apatternname"), "1.0")
                {
                    CodeTemplateFiles = new List<CodeTemplateFile>
                    {
                        new CodeTemplateFile
                        {
                            Id = "acodetemplateid",
                            Contents = CodeTemplateFile.Encoding.GetBytes("atemplate")
                        }
                    }
                };
                this.textTemplateEngine.Setup(tte => tte.Transform(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SolutionItem>()))
                    .Returns("acontent");
                this.fileSystemWriter.Setup(fsw => fsw.Exists(It.IsAny<string>()))
                    .Returns(true);
                var solution = new SolutionDefinition(toolkit);

                var result = this.command.Execute(solution, ownerSolution);

                result.CommandName.Should().Be("acommandname");
                result.Log.Should().BeEmpty();
                this.textTemplateEngine.Verify(tte => tte.Transform(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SolutionItem>()),
                    Times.Never);
                this.fileSystemWriter.Verify(fw => fw.Write(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
                ownerSolution.ArtifactLinks.Should().ContainSingle(link =>
                    link.CommandId == this.command.Id
                    && link.Tag == "afilename.cs"
                    && link.Path == "c:\\anabsolutepath\\afilename.cs");
            }
        }
    }
}