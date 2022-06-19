using System.Collections.Generic;
using Automate.Authoring.Application;
using Automate.Authoring.Domain;
using Automate.CLI;
using Automate.CLI.Infrastructure;
using Automate.Common;
using Automate.Common.Application;
using Automate.Common.Extensions;
using Automate.Runtime.Domain;
using FluentAssertions;
using Moq;
using Xunit;

namespace CLI.UnitTests.Infrastructure
{
    public class CodeTemplateCommandExecutorSpec
    {
        [Trait("Category", "Unit")]
        public class GivenAnyCommand
        {
            [Fact]
            public void WhenExecuteAndCodeTemplateNotExist_ThenThrows()
            {
                var toolkit =
                    new ToolkitDefinition(new PatternDefinition("apatternname"));
                var target = new DraftItem(toolkit,
                    new Element("anelementname"), null);
                var draft = new DraftDefinition(toolkit);
                var command = new CodeTemplateCommand("aname", "acodetemplateid", false, "afilepath");
                var executionResult =
                    new CommandExecutionResult("acommandname", new CommandExecutableContext(command, draft, target));

                new CodeTemplateCommandExecutor(Mock.Of<IFilePathResolver>(), Mock.Of<IFileSystemReaderWriter>(),
                        Mock.Of<IDraftPathResolver>(),
                        Mock.Of<ITextTemplatingEngine>())
                    .Invoking(x => x.Execute(command, executionResult))
                    .Should().Throw<AutomateException>()
                    .WithMessage(ExceptionMessages.CodeTemplateCommand_TemplateNotExists.Substitute("acodetemplateid"));
            }
        }

        [Trait("Category", "Unit")]
        public class GivenAEverytimeCommand
        {
            private readonly Mock<IFileSystemReaderWriter> fileSystem;
            private readonly CodeTemplateCommandExecutor executor;
            private readonly Mock<ITextTemplatingEngine> textTemplateEngine;
            private readonly CodeTemplateCommand command;

            public GivenAEverytimeCommand()
            {
                var filePathResolver = new Mock<IFilePathResolver>();
                filePathResolver.Setup(fpr => fpr.CreatePath(It.IsAny<string>(), It.IsAny<string>()))
                    .Returns("c:\\anabsolutepath\\afilename.cs");
                filePathResolver.Setup(fpr => fpr.GetFilename(It.IsAny<string>()))
                    .Returns("afilename.cs");
                this.fileSystem = new Mock<IFileSystemReaderWriter>();
                var draftPathResolver = new Mock<IDraftPathResolver>();
                draftPathResolver
                    .Setup(spr => spr.ResolveExpression(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DraftItem>()))
                    .Returns((string _, string expr, DraftItem _) => expr);
                this.textTemplateEngine = new Mock<ITextTemplatingEngine>();

                this.command = new CodeTemplateCommand("aname", "acodetemplateid", false, "~/afilepath.cs");
                this.executor = new CodeTemplateCommandExecutor(filePathResolver.Object,
                    this.fileSystem.Object,
                    draftPathResolver.Object, this.textTemplateEngine.Object);
            }

            [Fact]
            public void WhenExecuteAndNoArtifactLinkAndFileNotExist_ThenGeneratesFileAndAddsArtifactLink()
            {
                var toolkit =
                    new ToolkitDefinition(new PatternDefinition("apatternname"));
                var ownerItem = new DraftItem(toolkit,
                    new Element("anelementname"), null);
                toolkit.AddCodeTemplateFiles(new List<CodeTemplateFile>
                    { new(CodeTemplateFile.Encoding.GetBytes("atemplate"), "acodetemplateid") });
                var draft = new DraftDefinition(toolkit);
                this.textTemplateEngine.Setup(tte =>
                        tte.Transform(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DraftItem>()))
                    .Returns("acontent");
                this.fileSystem.Setup(fsw => fsw.FileExists(It.IsAny<string>()))
                    .Returns(false);
                var executionResult = new CommandExecutionResult("acommandname",
                    new CommandExecutableContext(this.command, draft, ownerItem));

                this.executor.Execute(this.command, executionResult);

                executionResult.CommandName.Should().Be("acommandname");
                executionResult.Log.Should()
                    .ContainSingle(InfrastructureMessages.CodeTemplateCommand_Log_GeneratedFile.Substitute(
                        "afilename.cs",
                        "acodetemplateid",
                        "c:\\anabsolutepath\\afilename.cs"));
                ownerItem.ArtifactLinks.Should().ContainSingle(link =>
                    link.CommandId == this.command.Id
                    && link.Tag == "afilename.cs"
                    && link.Path == "c:\\anabsolutepath\\afilename.cs");
                this.textTemplateEngine.Verify(tte => tte.Transform(It.IsAny<string>(), "atemplate", ownerItem));
                this.fileSystem.Verify(fw => fw.Write(It.Is<string>(content =>
                    content == "acontent"), "c:\\anabsolutepath\\afilename.cs"));
                this.fileSystem.Verify(fsw => fsw.Delete(It.IsAny<string>()), Times.Never);
                this.fileSystem.Verify(fsw => fsw.Move(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            }

            [Fact]
            public void WhenExecuteAndNoArtifactLinkButFileExists_ThenOverwritesFileAndAddsArtifactLink()
            {
                var toolkit =
                    new ToolkitDefinition(new PatternDefinition("apatternname"));
                var ownerItem = new DraftItem(toolkit,
                    new Element("anelementname"), null);
                toolkit.AddCodeTemplateFiles(new List<CodeTemplateFile>
                    { new(CodeTemplateFile.Encoding.GetBytes("atemplate"), "acodetemplateid") });
                this.textTemplateEngine.Setup(tte =>
                        tte.Transform(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DraftItem>()))
                    .Returns("acontent");
                this.fileSystem.Setup(fsw => fsw.FileExists(It.IsAny<string>()))
                    .Returns(true);
                var draft = new DraftDefinition(toolkit);
                var executionResult = new CommandExecutionResult("acommandname",
                    new CommandExecutableContext(this.command, draft, ownerItem));

                this.executor.Execute(this.command, executionResult);

                executionResult.CommandName.Should().Be("acommandname");
                executionResult.Log.Should()
                    .ContainSingle(InfrastructureMessages.CodeTemplateCommand_Log_GeneratedFile.Substitute(
                        "afilename.cs",
                        "acodetemplateid",
                        "c:\\anabsolutepath\\afilename.cs"));
                ownerItem.ArtifactLinks.Should().ContainSingle(link =>
                    link.CommandId == this.command.Id
                    && link.Tag == "afilename.cs"
                    && link.Path == "c:\\anabsolutepath\\afilename.cs");
                this.textTemplateEngine.Verify(tte => tte.Transform(It.IsAny<string>(), "atemplate", ownerItem));
                this.fileSystem.Verify(fw => fw.Write(It.Is<string>(content =>
                    content == "acontent"), "c:\\anabsolutepath\\afilename.cs"));
                this.fileSystem.Verify(fsw => fsw.Delete(It.IsAny<string>()), Times.Never);
                this.fileSystem.Verify(fsw => fsw.Move(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            }

            [Fact]
            public void
                WhenExecuteAndDifferentArtifactLinkExistsButFileNotExists_ThenDeletesLinkedFileAndGeneratesNewFileAndUpdatesArtifactLink()
            {
                var toolkit =
                    new ToolkitDefinition(new PatternDefinition("apatternname"));
                var ownerItem = new DraftItem(toolkit,
                    new Element("anelementname"), null);
                ownerItem.AddArtifactLink(this.command.Id, "anoriginalpath", "atag");
                toolkit.AddCodeTemplateFiles(new List<CodeTemplateFile>
                    { new(CodeTemplateFile.Encoding.GetBytes("atemplate"), "acodetemplateid") });
                this.textTemplateEngine.Setup(tte =>
                        tte.Transform(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DraftItem>()))
                    .Returns("acontent");
                this.fileSystem.Setup(fsw => fsw.FileExists(It.IsAny<string>()))
                    .Returns(false);
                var draft = new DraftDefinition(toolkit);
                var executionResult = new CommandExecutionResult("acommandname",
                    new CommandExecutableContext(this.command, draft, ownerItem));

                this.executor.Execute(this.command, executionResult);

                executionResult.CommandName.Should().Be("acommandname");
                executionResult.Log.Should()
                    .Contain(InfrastructureMessages.CodeTemplateCommand_Log_GeneratedFile.Substitute("afilename.cs",
                        "acodetemplateid",
                        "c:\\anabsolutepath\\afilename.cs"));
                executionResult.Log.Should()
                    .Contain(
                        InfrastructureMessages.CodeTemplateCommand_Log_Warning_Deleted.Substitute("anoriginalpath"));
                ownerItem.ArtifactLinks.Should().ContainSingle(link =>
                    link.CommandId == this.command.Id
                    && link.Tag == "afilename.cs"
                    && link.Path == "c:\\anabsolutepath\\afilename.cs");
                this.textTemplateEngine.Verify(tte => tte.Transform(It.IsAny<string>(), "atemplate", ownerItem));
                this.fileSystem.Verify(fw => fw.Write(It.Is<string>(content =>
                    content == "acontent"), "c:\\anabsolutepath\\afilename.cs"));
                this.fileSystem.Verify(fsw => fsw.Delete("anoriginalpath"));
                this.fileSystem.Verify(fsw => fsw.Move(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            }

            [Fact]
            public void
                WhenExecuteAndDifferentArtifactLinkExistsAndFileExists_ThenDeletesLinkedFileAndOverwritesFileAndUpdatesArtifactLink()
            {
                var toolkit =
                    new ToolkitDefinition(new PatternDefinition("apatternname"));
                var ownerItem = new DraftItem(toolkit,
                    new Element("anelementname"), null);
                ownerItem.AddArtifactLink(this.command.Id, "anoriginalpath", "atag");
                toolkit.AddCodeTemplateFiles(new List<CodeTemplateFile>
                    { new(CodeTemplateFile.Encoding.GetBytes("atemplate"), "acodetemplateid") });
                this.textTemplateEngine.Setup(tte =>
                        tte.Transform(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DraftItem>()))
                    .Returns("acontent");
                this.fileSystem.Setup(fsw => fsw.FileExists(It.IsAny<string>()))
                    .Returns(true);
                var draft = new DraftDefinition(toolkit);
                var executionResult = new CommandExecutionResult("acommandname",
                    new CommandExecutableContext(this.command, draft, ownerItem));

                this.executor.Execute(this.command, executionResult);

                executionResult.CommandName.Should().Be("acommandname");
                executionResult.Log.Should()
                    .Contain(InfrastructureMessages.CodeTemplateCommand_Log_GeneratedFile.Substitute("afilename.cs",
                        "acodetemplateid",
                        "c:\\anabsolutepath\\afilename.cs"));
                executionResult.Log.Should()
                    .Contain(
                        InfrastructureMessages.CodeTemplateCommand_Log_Warning_Deleted.Substitute("anoriginalpath"));
                ownerItem.ArtifactLinks.Should().ContainSingle(link =>
                    link.CommandId == this.command.Id
                    && link.Tag == "afilename.cs"
                    && link.Path == "c:\\anabsolutepath\\afilename.cs");
                this.textTemplateEngine.Verify(tte => tte.Transform(It.IsAny<string>(), "atemplate", ownerItem));
                this.fileSystem.Verify(fw => fw.Write(It.Is<string>(content =>
                    content == "acontent"), "c:\\anabsolutepath\\afilename.cs"));
                this.fileSystem.Verify(fsw => fsw.Delete("anoriginalpath"));
                this.fileSystem.Verify(fsw => fsw.Move(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            }

            [Fact]
            public void
                WhenExecuteAndSameArtifactLinkExistsButFileNotExists_ThenGeneratesNewFileAndUpdatesArtifactLink()
            {
                var toolkit =
                    new ToolkitDefinition(new PatternDefinition("apatternname"));
                var ownerItem = new DraftItem(toolkit,
                    new Element("anelementname"), null);
                ownerItem.AddArtifactLink(this.command.Id, "c:\\anabsolutepath\\afilename.cs", "atag");
                toolkit.AddCodeTemplateFiles(new List<CodeTemplateFile>
                    { new(CodeTemplateFile.Encoding.GetBytes("atemplate"), "acodetemplateid") });
                this.textTemplateEngine.Setup(tte =>
                        tte.Transform(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DraftItem>()))
                    .Returns("acontent");
                this.fileSystem.Setup(fsw => fsw.FileExists(It.IsAny<string>()))
                    .Returns(false);
                var draft = new DraftDefinition(toolkit);
                var executionResult = new CommandExecutionResult("acommandname",
                    new CommandExecutableContext(this.command, draft, ownerItem));

                this.executor.Execute(this.command, executionResult);

                executionResult.CommandName.Should().Be("acommandname");
                executionResult.Log.Should()
                    .ContainSingle(InfrastructureMessages.CodeTemplateCommand_Log_GeneratedFile.Substitute(
                        "afilename.cs",
                        "acodetemplateid",
                        "c:\\anabsolutepath\\afilename.cs"));
                ownerItem.ArtifactLinks.Should().ContainSingle(link =>
                    link.CommandId == this.command.Id
                    && link.Tag == "afilename.cs"
                    && link.Path == "c:\\anabsolutepath\\afilename.cs");
                this.textTemplateEngine.Verify(tte => tte.Transform(It.IsAny<string>(), "atemplate", ownerItem));
                this.fileSystem.Verify(fw => fw.Write(It.Is<string>(content =>
                    content == "acontent"), "c:\\anabsolutepath\\afilename.cs"));
                this.fileSystem.Verify(fsw => fsw.Delete(It.IsAny<string>()), Times.Never);
                this.fileSystem.Verify(fsw => fsw.Move(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            }

            [Fact]
            public void WhenExecuteAndSameArtifactLinkExistsAndFileExists_ThenOverwritesFileAndUpdatesArtifactLink()
            {
                var toolkit =
                    new ToolkitDefinition(new PatternDefinition("apatternname"));
                var ownerItem = new DraftItem(toolkit,
                    new Element("anelementname"), null);
                ownerItem.AddArtifactLink(this.command.Id, "c:\\anabsolutepath\\afilename.cs", "atag");
                toolkit.AddCodeTemplateFiles(new List<CodeTemplateFile>
                    { new(CodeTemplateFile.Encoding.GetBytes("atemplate"), "acodetemplateid") });
                this.textTemplateEngine.Setup(tte =>
                        tte.Transform(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DraftItem>()))
                    .Returns("acontent");
                this.fileSystem.Setup(fsw => fsw.FileExists(It.IsAny<string>()))
                    .Returns(true);
                var draft = new DraftDefinition(toolkit);
                var executionResult = new CommandExecutionResult("acommandname",
                    new CommandExecutableContext(this.command, draft, ownerItem));

                this.executor.Execute(this.command, executionResult);

                executionResult.CommandName.Should().Be("acommandname");
                executionResult.Log.Should()
                    .ContainSingle(InfrastructureMessages.CodeTemplateCommand_Log_GeneratedFile.Substitute(
                        "afilename.cs",
                        "acodetemplateid",
                        "c:\\anabsolutepath\\afilename.cs"));
                ownerItem.ArtifactLinks.Should().ContainSingle(link =>
                    link.CommandId == this.command.Id
                    && link.Tag == "afilename.cs"
                    && link.Path == "c:\\anabsolutepath\\afilename.cs");
                this.textTemplateEngine.Verify(tte => tte.Transform(It.IsAny<string>(), "atemplate", ownerItem));
                this.fileSystem.Verify(fw => fw.Write(It.Is<string>(content =>
                    content == "acontent"), "c:\\anabsolutepath\\afilename.cs"));
                this.fileSystem.Verify(fsw => fsw.Delete(It.IsAny<string>()), Times.Never);
                this.fileSystem.Verify(fsw => fsw.Move(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            }
        }

        [Trait("Category", "Unit")]
        public class GivenAOneOffCommand
        {
            private readonly Mock<IFileSystemReaderWriter> fileSystem;
            private readonly CodeTemplateCommandExecutor executor;
            private readonly Mock<ITextTemplatingEngine> textTemplateEngine;
            private readonly CodeTemplateCommand command;

            public GivenAOneOffCommand()
            {
                var filePathResolver = new Mock<IFilePathResolver>();
                filePathResolver.Setup(fpr => fpr.CreatePath(It.IsAny<string>(), It.IsAny<string>()))
                    .Returns("c:\\anabsolutepath\\afilename.cs");
                filePathResolver.Setup(fpr => fpr.GetFilename(It.IsAny<string>()))
                    .Returns("afilename.cs");
                this.fileSystem = new Mock<IFileSystemReaderWriter>();
                var draftPathResolver = new Mock<IDraftPathResolver>();
                draftPathResolver
                    .Setup(spr => spr.ResolveExpression(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DraftItem>()))
                    .Returns((string _, string expr, DraftItem _) => expr);
                this.textTemplateEngine = new Mock<ITextTemplatingEngine>();

                this.command = new CodeTemplateCommand("acommandname", "acodetemplateid", true, "~/afilepath.cs");
                this.executor = new CodeTemplateCommandExecutor(filePathResolver.Object,
                    this.fileSystem.Object,
                    draftPathResolver.Object,
                    this.textTemplateEngine.Object);
            }

            [Fact]
            public void WhenExecuteAndNoArtifactLinkAndFileExists_ThenOnlyAddsArtifactLink()
            {
                var toolkit =
                    new ToolkitDefinition(new PatternDefinition("apatternname"));
                var ownerItem = new DraftItem(toolkit,
                    new Element("anelementname"), null);
                toolkit.AddCodeTemplateFiles(new List<CodeTemplateFile>
                    { new(CodeTemplateFile.Encoding.GetBytes("atemplate"), "acodetemplateid") });
                this.fileSystem.Setup(fsw => fsw.FileExists(It.IsAny<string>()))
                    .Returns(true);
                var draft = new DraftDefinition(toolkit);
                var executionResult = new CommandExecutionResult("acommandname",
                    new CommandExecutableContext(this.command, draft, ownerItem));

                this.executor.Execute(this.command, executionResult);

                executionResult.CommandName.Should().Be("acommandname");
                executionResult.Log.Should()
                    .ContainSingle(InfrastructureMessages.CodeTemplateCommand_Log_UpdatedLink.Substitute("afilename.cs",
                        "c:\\anabsolutepath\\afilename.cs"));
                ownerItem.ArtifactLinks.Should().ContainSingle(link =>
                    link.CommandId == this.command.Id
                    && link.Tag == "afilename.cs"
                    && link.Path == "c:\\anabsolutepath\\afilename.cs");
                this.textTemplateEngine.Verify(
                    tte => tte.Transform(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DraftItem>()),
                    Times.Never);
                this.fileSystem.Verify(fw => fw.Write(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
                this.fileSystem.Verify(fsw => fsw.Delete(It.IsAny<string>()), Times.Never);
                this.fileSystem.Verify(fsw => fsw.Move(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            }

            [Fact]
            public void
                WhenExecuteAndDifferentArtifactLinkExistsAndFileExists_ThenDeletesLinkedFileAndUpdatesArtifactLink()
            {
                var toolkit =
                    new ToolkitDefinition(new PatternDefinition("apatternname"));
                var ownerItem = new DraftItem(toolkit,
                    new Element("anelementname"), null);
                ownerItem.AddArtifactLink(this.command.Id, "anoriginalpath", "atag");
                toolkit.AddCodeTemplateFiles(new List<CodeTemplateFile>
                    { new(CodeTemplateFile.Encoding.GetBytes("atemplate"), "acodetemplateid") });
                this.textTemplateEngine.Setup(tte =>
                        tte.Transform(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DraftItem>()))
                    .Returns("acontent");
                this.fileSystem.Setup(fsw => fsw.FileExists(It.IsAny<string>()))
                    .Returns(true);
                var draft = new DraftDefinition(toolkit);
                var executionResult = new CommandExecutionResult("acommandname",
                    new CommandExecutableContext(this.command, draft, ownerItem));

                this.executor.Execute(this.command, executionResult);

                executionResult.CommandName.Should().Be("acommandname");
                executionResult.Log.Should()
                    .Contain(
                        InfrastructureMessages.CodeTemplateCommand_Log_Warning_Deleted.Substitute("anoriginalpath"));
                ownerItem.ArtifactLinks.Should().ContainSingle(link =>
                    link.CommandId == this.command.Id
                    && link.Tag == "afilename.cs"
                    && link.Path == "c:\\anabsolutepath\\afilename.cs");
                this.textTemplateEngine.Verify(
                    tte => tte.Transform(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DraftItem>()),
                    Times.Never);
                this.fileSystem.Verify(fw => fw.Write(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
                this.fileSystem.Verify(fsw => fsw.Delete("anoriginalpath"));
                this.fileSystem.Verify(fsw => fsw.Move(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            }

            [Fact]
            public void
                WhenExecuteAndDifferentArtifactLinkExistsButFileNotExists_ThenMovesLinkedFileAndUpdatesArtifactLink()
            {
                var toolkit =
                    new ToolkitDefinition(new PatternDefinition("apatternname"));
                var ownerItem = new DraftItem(toolkit,
                    new Element("anelementname"), null);
                ownerItem.AddArtifactLink(this.command.Id, "apath", "atag");
                toolkit.AddCodeTemplateFiles(new List<CodeTemplateFile>
                    { new(CodeTemplateFile.Encoding.GetBytes("atemplate"), "acodetemplateid") });
                this.textTemplateEngine.Setup(tte =>
                        tte.Transform(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DraftItem>()))
                    .Returns("acontent");
                this.fileSystem.Setup(fsw => fsw.FileExists(It.IsAny<string>()))
                    .Returns(false);
                var draft = new DraftDefinition(toolkit);
                var executionResult = new CommandExecutionResult("acommandname",
                    new CommandExecutableContext(this.command, draft, ownerItem));

                this.executor.Execute(this.command, executionResult);

                executionResult.CommandName.Should().Be("acommandname");
                executionResult.Log.Should()
                    .Contain(InfrastructureMessages.CodeTemplateCommand_Log_Warning_Moved.Substitute("apath",
                        "c:\\anabsolutepath\\afilename.cs"));
                executionResult.Log.Should()
                    .Contain(InfrastructureMessages.CodeTemplateCommand_Log_UpdatedLink.Substitute("afilename.cs",
                        "c:\\anabsolutepath\\afilename.cs"));
                ownerItem.ArtifactLinks.Should().ContainSingle(link =>
                    link.CommandId == this.command.Id
                    && link.Tag == "afilename.cs"
                    && link.Path == "c:\\anabsolutepath\\afilename.cs");
                this.textTemplateEngine.Verify(
                    tte => tte.Transform(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DraftItem>()), Times.Never);
                this.fileSystem.Verify(fw => fw.Write(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
                this.fileSystem.Verify(fsw => fsw.Delete(It.IsAny<string>()), Times.Never);
                this.fileSystem.Verify(fsw => fsw.Move("apath", "c:\\anabsolutepath\\afilename.cs"));
            }

            [Fact]
            public void
                WhenExecuteAndSameArtifactLinkExistsButFileNotExists_ThenGeneratesNewFileAndUpdatesArtifactLink()
            {
                var toolkit =
                    new ToolkitDefinition(new PatternDefinition("apatternname"));
                var ownerItem = new DraftItem(toolkit,
                    new Element("anelementname"), null);
                ownerItem.AddArtifactLink(this.command.Id, "c:\\anabsolutepath\\afilename.cs", "atag");
                toolkit.AddCodeTemplateFiles(new List<CodeTemplateFile>
                    { new(CodeTemplateFile.Encoding.GetBytes("atemplate"), "acodetemplateid") });
                this.textTemplateEngine.Setup(tte =>
                        tte.Transform(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DraftItem>()))
                    .Returns("acontent");
                this.fileSystem.Setup(fsw => fsw.FileExists(It.IsAny<string>()))
                    .Returns(false);
                var draft = new DraftDefinition(toolkit);
                var executionResult = new CommandExecutionResult("acommandname",
                    new CommandExecutableContext(this.command, draft, ownerItem));

                this.executor.Execute(this.command, executionResult);

                executionResult.CommandName.Should().Be("acommandname");
                executionResult.Log.Should()
                    .Contain(InfrastructureMessages.CodeTemplateCommand_Log_GeneratedFile.Substitute("afilename.cs",
                        "acodetemplateid",
                        "c:\\anabsolutepath\\afilename.cs"));
                ownerItem.ArtifactLinks.Should().ContainSingle(link =>
                    link.CommandId == this.command.Id
                    && link.Tag == "afilename.cs"
                    && link.Path == "c:\\anabsolutepath\\afilename.cs");
                this.textTemplateEngine.Verify(tte => tte.Transform(It.IsAny<string>(), "atemplate", ownerItem));
                this.fileSystem.Verify(fw => fw.Write(It.Is<string>(content =>
                    content == "acontent"), "c:\\anabsolutepath\\afilename.cs"));
                this.fileSystem.Verify(fsw => fsw.Delete(It.IsAny<string>()), Times.Never);
                this.fileSystem.Verify(fsw => fsw.Move(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            }

            [Fact]
            public void WhenExecuteAndSameArtifactLinkExistsAndFileExists_ThenDoesNothing()
            {
                var toolkit =
                    new ToolkitDefinition(new PatternDefinition("apatternname"));
                var ownerItem = new DraftItem(toolkit,
                    new Element("anelementname"), null);
                ownerItem.AddArtifactLink(this.command.Id, "c:\\anabsolutepath\\afilename.cs", "atag");
                toolkit.AddCodeTemplateFiles(new List<CodeTemplateFile>
                    { new(CodeTemplateFile.Encoding.GetBytes("atemplate"), "acodetemplateid") });
                this.textTemplateEngine.Setup(tte =>
                        tte.Transform(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DraftItem>()))
                    .Returns("acontent");
                this.fileSystem.Setup(fsw => fsw.FileExists(It.IsAny<string>()))
                    .Returns(true);
                var draft = new DraftDefinition(toolkit);
                var executionResult = new CommandExecutionResult("acommandname",
                    new CommandExecutableContext(this.command, draft, ownerItem));

                this.executor.Execute(this.command, executionResult);

                executionResult.CommandName.Should().Be("acommandname");
                executionResult.Log.Should().BeEmpty();
                ownerItem.ArtifactLinks.Should().ContainSingle(link =>
                    link.CommandId == this.command.Id
                    && link.Tag == "afilename.cs"
                    && link.Path == "c:\\anabsolutepath\\afilename.cs");
                this.textTemplateEngine.Verify(
                    tte => tte.Transform(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DraftItem>()),
                    Times.Never);
                this.fileSystem.Verify(fw => fw.Write(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
                this.fileSystem.Verify(fsw => fsw.Delete(It.IsAny<string>()), Times.Never);
                this.fileSystem.Verify(fsw => fsw.Move(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            }
        }
    }
}