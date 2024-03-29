﻿using System;
using System.Collections.Generic;
using System.Threading;
using Automate.Authoring.Domain;
using Automate.Authoring.Infrastructure;
using Automate.CLI.Infrastructure;
using Automate.Common;
using Automate.Common.Domain;
using Automate.Common.Extensions;
using Automate.Common.Infrastructure;
using Automate.Runtime.Domain;
using FluentAssertions;
using Moq;
using Xunit;

namespace Core.UnitTests.Authoring.Domain
{
    [Trait("Category", "Unit")]
    public class ToolkitDefinitionSpec
    {
        [Fact]
        public void WhenConstructed_ThenAssigned()
        {
            var pattern = new PatternDefinition("apatternname");

            var result = new ToolkitDefinition(pattern);

            result.Pattern.Should().Be(pattern);
            result.PatternName.Should().Be("apatternname");
            result.Version.Should().Be("0.0.0");
            result.RuntimeVersion.Should().Be(MachineConstants.GetRuntimeVersion().ToString());
            result.CodeTemplateFiles.Should().BeEmpty();
        }

        [Fact]
        public void WhenMigratePatternAndNoTemplates_ThenReturnsEmptyResult()
        {
            var originalToolkit =
                MakeDetachedToolkit(new PatternDefinition("apatternname"));
            var draft = new DraftDefinition(originalToolkit);

            var result = new DraftUpgradeResult(draft, "0.0.0", "0.1.0");

            draft.Toolkit.MigratePattern(originalToolkit, result);

            draft.Toolkit.Version.Should().Be("0.1.0");
            result.IsSuccess.Should().BeTrue();
            result.Log.Should().BeEmpty();
        }

        [Fact]
        public void WhenMigratePatternAndExistingTemplateChanged_ThenReturnsResultForChanged()
        {
            var pattern = new PatternDefinition("apatternname");
            var codeTemplate1 = pattern.AddCodeTemplate("atemplatename1", "afullpath", "anextension");
            var codeTemplate2 = pattern.AddCodeTemplate("atemplatename2", "afullpath", "anextension");
            var originalToolkit = MakeDetachedToolkit(pattern, new Dictionary<string, byte[]>
            {
                { codeTemplate1.Id, new byte[] { 0x01, 0x02, 0x03 } },
                { codeTemplate2.Id, new byte[] { 0x04, 0x05, 0x06 } }
            });
            var draft = new DraftDefinition(originalToolkit);
            Thread.Sleep(TimeSpan.FromSeconds(1)); // Delay for LastModified comparisons
            var updatedToolkit = MakeDetachedToolkit(pattern, new Dictionary<string, byte[]>
            {
                { codeTemplate1.Id, new byte[] { 0x01, 0x02, 0x03 } },
                { codeTemplate2.Id, new byte[] { 0x09, 0x08, 0x07 } }
            });
            var result = new DraftUpgradeResult(draft, "0.0.0", "0.1.0");

            draft.Toolkit.MigratePattern(updatedToolkit, result);

            draft.Toolkit.Pattern.CodeTemplates.Should().Contain(x => x.Id == codeTemplate1.Id);
            draft.Toolkit.Pattern.CodeTemplates.Should().Contain(x => x.Id == codeTemplate2.Id);
            draft.Toolkit.CodeTemplateFiles[0].Contents.Should().ContainInOrder(0x01, 0x02, 0x03);
            draft.Toolkit.CodeTemplateFiles[1].Contents.Should().ContainInOrder(0x09, 0x08, 0x07);
            draft.Toolkit.Version.Should().Be("0.2.0");
            result.IsSuccess.Should().BeTrue();
            result.Log.Should().ContainSingle(x =>
                x.Type == MigrationChangeType.NonBreaking
                && x.MessageTemplate == MigrationMessages.ToolkitDefinition_CodeTemplateFile_ContentUpgraded
                && (string)x.Arguments[0] == codeTemplate2.Name
                && (string)x.Arguments[1] == codeTemplate2.Id);
        }

        [Fact]
        public void WhenMigratePatternAndExistingTemplateDeleted_ThenReturnsResultForRemoved()
        {
            var pattern = new PatternDefinition("apatternname");
            var codeTemplate = pattern.AddCodeTemplate("atemplatename", "afullpath", "anextension");
            var originalToolkit = MakeDetachedToolkit(pattern, new Dictionary<string, byte[]>
            {
                { codeTemplate.Id, new byte[] { 0x01, 0x02, 0x03 } }
            });
            var draft = new DraftDefinition(originalToolkit);
            pattern.DeleteCodeTemplate(codeTemplate.Name, true);
            var updatedToolkit = MakeDetachedToolkit(pattern);
            var result = new DraftUpgradeResult(draft, "0.0.0", "0.1.0");

            draft.Toolkit.MigratePattern(updatedToolkit, result);

            draft.Toolkit.Pattern.CodeTemplates.Should().BeEmpty();
            draft.Toolkit.CodeTemplateFiles.Should().BeEmpty();
            draft.Toolkit.Version.Should().Be("1.0.0");
            result.IsSuccess.Should().BeTrue();
            result.Log.Should().ContainSingle(x =>
                x.Type == MigrationChangeType.Breaking
                && x.MessageTemplate == MigrationMessages.ToolkitDefinition_CodeTemplateFile_Deleted
                && (string)x.Arguments[0] == codeTemplate.Name
                && (string)x.Arguments[1] == codeTemplate.Id);
        }

        [Fact]
        public void WhenMigratePatternAndNewTemplateAdded_ThenReturnsResultForAdded()
        {
            var pattern = new PatternDefinition("apatternname");
            var codeTemplate1 = pattern.AddCodeTemplate("atemplatename1", "afullpath", "anextension");
            var originalToolkit = MakeDetachedToolkit(pattern, new Dictionary<string, byte[]>
            {
                { codeTemplate1.Id, new byte[] { 0x01, 0x02, 0x03 } }
            });
            var draft = new DraftDefinition(originalToolkit);
            var codeTemplate2 = pattern.AddCodeTemplate("atemplatename2", "afullpath", "anextension");
            var updatedToolkit = MakeDetachedToolkit(pattern, new Dictionary<string, byte[]>
            {
                { codeTemplate1.Id, new byte[] { 0x01, 0x02, 0x03 } },
                { codeTemplate2.Id, new byte[] { 0x09, 0x08, 0x07 } }
            });
            var result = new DraftUpgradeResult(draft, "0.0.0", "0.1.0");

            draft.Toolkit.MigratePattern(updatedToolkit, result);

            draft.Toolkit.Pattern.CodeTemplates.Should().Contain(x => x.Id == codeTemplate1.Id);
            draft.Toolkit.Pattern.CodeTemplates.Should().Contain(x => x.Id == codeTemplate2.Id);
            draft.Toolkit.CodeTemplateFiles[0].Contents.Should().ContainInOrder(0x01, 0x02, 0x03);
            draft.Toolkit.CodeTemplateFiles[1].Contents.Should().ContainInOrder(0x09, 0x08, 0x07);
            draft.Toolkit.Version.Should().Be("0.2.0");
            result.IsSuccess.Should().BeTrue();
            result.Log.Should().ContainSingle(x =>
                x.Type == MigrationChangeType.NonBreaking
                && x.MessageTemplate == MigrationMessages.ToolkitDefinition_CodeTemplateFile_Added
                && (string)x.Arguments[0] == codeTemplate2.Name
                && (string)x.Arguments[1] == codeTemplate2.Id);
        }

        [Fact]
        public void WhenMigratePattern_ThenReturnsResultForAddedAndDeleted()
        {
            var pattern = new PatternDefinition("apatternname");
            var codeTemplate1 = pattern.AddCodeTemplate("atemplatename1", "afullpath", "anextension");
            var codeTemplate2 = pattern.AddCodeTemplate("atemplatename2", "afullpath", "anextension");
            var originalToolkit = MakeDetachedToolkit(pattern, new Dictionary<string, byte[]>
            {
                { codeTemplate1.Id, new byte[] { 0x01, 0x02, 0x03 } },
                { codeTemplate2.Id, new byte[] { 0x05, 0x06, 0x07 } }
            });
            var draft = new DraftDefinition(originalToolkit);
            var codeTemplate3 = pattern.AddCodeTemplate("atemplatename3", "afullpath", "anextension");
            pattern.DeleteCodeTemplate(codeTemplate2.Name, true);
            var updatedToolkit = MakeDetachedToolkit(pattern, new Dictionary<string, byte[]>
            {
                { codeTemplate1.Id, new byte[] { 0x01, 0x02, 0x03 } },
                { codeTemplate3.Id, new byte[] { 0x09, 0x08, 0x07 } }
            });
            var result = new DraftUpgradeResult(draft, "0.0.0", "0.1.0");

            draft.Toolkit.MigratePattern(updatedToolkit, result);

            draft.Toolkit.Pattern.CodeTemplates.Should().HaveCount(2);
            draft.Toolkit.Pattern.CodeTemplates.Should().Contain(x => x.Id == codeTemplate1.Id);
            draft.Toolkit.Pattern.CodeTemplates.Should().Contain(x => x.Id == codeTemplate3.Id);
            draft.Toolkit.CodeTemplateFiles[0].Contents.Should().ContainInOrder(0x01, 0x02, 0x03);
            draft.Toolkit.CodeTemplateFiles[1].Contents.Should().ContainInOrder(0x09, 0x08, 0x07);
            draft.Toolkit.Version.Should().Be("1.0.0");
            result.IsSuccess.Should().BeTrue();
            result.Log.Should().Contain(x =>
                x.Type == MigrationChangeType.Breaking
                && x.MessageTemplate == MigrationMessages.ToolkitDefinition_CodeTemplateFile_Deleted
                && (string)x.Arguments[0] == codeTemplate2.Name
                && (string)x.Arguments[1] == codeTemplate2.Id);
            result.Log.Should().Contain(x =>
                x.Type == MigrationChangeType.NonBreaking
                && x.MessageTemplate == MigrationMessages.ToolkitDefinition_CodeTemplateFile_Added
                && (string)x.Arguments[0] == codeTemplate3.Name
                && (string)x.Arguments[1] == codeTemplate3.Id);
        }

#if TESTINGONLY
        [Fact]
        public void WhenVerifyRuntimeCompatibilityAndPreReleaseAndToolkitVersionIsNonExistent_ThenThrows()
        {
            var metadata = new Mock<IRuntimeMetadata>();
            metadata.Setup(md => md.ProductName).Returns("aproductname");
            metadata.Setup(md => md.RuntimeVersion).Returns("0.3.0-preview".ToSemVersion);
            var toolkit = new ToolkitDefinition(new PatternDefinition("apatternname"), "1.0.0");
            toolkit.ResetRuntimeVersion();

            FluentActions.Invoking(() => ToolkitDefinition.VerifyRuntimeCompatibility(toolkit, metadata.Object))
                .Should().Throw<AutomateException>()
                .WithMessage(
                    ExceptionMessages.ToolkitDefinition_Incompatible_NoToolkitVersion.Substitute(
                        MachineConstants.FirstVersionSupportingRuntimeVersion, "0.3.0-preview",
                        "aproductname"));
        }
#endif

        [Fact]
        public void WhenVerifyRuntimeCompatibilityAndPreReleaseAndRuntimeVersionIsBreaking_ThenThrows()
        {
            var metadata = new Mock<IRuntimeMetadata>();
            metadata.Setup(md => md.ProductName).Returns("aproductname");
            metadata.Setup(md => md.RuntimeVersion).Returns("0.2.0-preview".ToSemVersion);
            var toolkit = new ToolkitDefinition(new PatternDefinition("apatternname"), "0.1.0-preview");

            FluentActions.Invoking(() => ToolkitDefinition.VerifyRuntimeCompatibility(toolkit, metadata.Object))
                .Should().Throw<AutomateException>()
                .WithMessage(
                    ExceptionMessages.ToolkitDefinition_Incompatible_MachineAheadOfToolkit.Substitute("0.1.0-preview",
                        "0.2.0-preview",
                        "aproductname"));
        }

        [Fact]
        public void WhenVerifyRuntimeCompatibilityAndPreReleaseAndRuntimeVersionIsNonBreaking_ThenSucceeds()
        {
            var metadata = new Mock<IRuntimeMetadata>();
            metadata.Setup(md => md.ProductName).Returns("aproductname");
            metadata.Setup(md => md.RuntimeVersion).Returns("0.1.0-preview".ToSemVersion);
            var toolkit = new ToolkitDefinition(new PatternDefinition("apatternname"), "0.1.1-preview");

            ToolkitDefinition.VerifyRuntimeCompatibility(toolkit, metadata.Object);
        }

        [Fact]
        public void WhenVerifyRuntimeCompatibilityAndPreReleaseAndToolkitVersionIsNonBreaking_ThenSucceeds()
        {
            var metadata = new Mock<IRuntimeMetadata>();
            metadata.Setup(md => md.ProductName).Returns("aproductname");
            metadata.Setup(md => md.RuntimeVersion).Returns("0.1.1-preview".ToSemVersion);
            var toolkit = new ToolkitDefinition(new PatternDefinition("apatternname"), "0.1.0-preview");

            ToolkitDefinition.VerifyRuntimeCompatibility(toolkit, metadata.Object);
        }

        [Fact]
        public void
            WhenVerifyRuntimeCompatibilityAndPreReleaseAndToolkitVersionIsBreaking_ThenThrows()
        {
            var metadata = new Mock<IRuntimeMetadata>();
            metadata.Setup(md => md.ProductName).Returns("aproductname");
            metadata.Setup(md => md.RuntimeVersion).Returns("0.1.0-preview".ToSemVersion);
            var toolkit = new ToolkitDefinition(new PatternDefinition("apatternname"), "0.2.0-preview");

            FluentActions.Invoking(() => ToolkitDefinition.VerifyRuntimeCompatibility(toolkit, metadata.Object))
                .Should().Throw<AutomateException>()
                .WithMessage(
                    ExceptionMessages.ToolkitDefinition_Incompatible_ToolkitAheadOfMachine.Substitute("0.2.0-preview",
                        "0.1.0-preview", "aproductname"));
        }

        [Fact]
        public void WhenVerifyRuntimeCompatibilityAndToolkitPreReleaseAndToolkitVersionIsBackInPreview_ThenThrows()
        {
            var metadata = new Mock<IRuntimeMetadata>();
            metadata.Setup(md => md.ProductName).Returns("aproductname");
            metadata.Setup(md => md.RuntimeVersion).Returns("1.1.0".ToSemVersion);
            var toolkit = new ToolkitDefinition(new PatternDefinition("apatternname"), "1.0.0-preview");

            FluentActions.Invoking(() => ToolkitDefinition.VerifyRuntimeCompatibility(toolkit, metadata.Object))
                .Should().Throw<AutomateException>()
                .WithMessage(
                    ExceptionMessages.ToolkitDefinition_Incompatible_MachineAheadOfToolkit.Substitute("1.0.0-preview",
                        "1.1.0", "aproductname"));
        }

        [Fact]
        public void WhenVerifyRuntimeCompatibilityAndToolkitPreReleaseAndToolkitVersionIsAheadInPreview_ThenThrows()
        {
            var metadata = new Mock<IRuntimeMetadata>();
            metadata.Setup(md => md.ProductName).Returns("aproductname");
            metadata.Setup(md => md.RuntimeVersion).Returns("1.0.0".ToSemVersion);
            var toolkit = new ToolkitDefinition(new PatternDefinition("apatternname"), "1.1.0-preview");

            FluentActions.Invoking(() => ToolkitDefinition.VerifyRuntimeCompatibility(toolkit, metadata.Object))
                .Should().Throw<AutomateException>()
                .WithMessage(
                    ExceptionMessages.ToolkitDefinition_Incompatible_ToolkitAheadOfMachine.Substitute("1.1.0-preview",
                        "1.0.0", "aproductname"));
        }

        [Fact]
        public void WhenVerifyRuntimeCompatibilityAndRuntimePreReleaseAndRuntimeVersionIsAheadInPreview_ThenThrows()
        {
            var metadata = new Mock<IRuntimeMetadata>();
            metadata.Setup(md => md.ProductName).Returns("aproductname");
            metadata.Setup(md => md.RuntimeVersion).Returns("1.1.0-preview".ToSemVersion);
            var toolkit = new ToolkitDefinition(new PatternDefinition("apatternname"), "1.0.0");

            FluentActions.Invoking(() => ToolkitDefinition.VerifyRuntimeCompatibility(toolkit, metadata.Object))
                .Should().Throw<AutomateException>()
                .WithMessage(
                    ExceptionMessages.ToolkitDefinition_Incompatible_MachineAheadOfToolkit.Substitute("1.0.0",
                        "1.1.0-preview", "aproductname"));
        }

        [Fact]
        public void WhenVerifyRuntimeCompatibilityAndRuntimePreReleaseAndInstalledRuntimeIsBackInPreview_ThenThrows()
        {
            var metadata = new Mock<IRuntimeMetadata>();
            metadata.Setup(md => md.ProductName).Returns("aproductname");
            metadata.Setup(md => md.RuntimeVersion).Returns("1.0.0-preview".ToSemVersion);
            var toolkit = new ToolkitDefinition(new PatternDefinition("apatternname"), "1.1.0");

            FluentActions.Invoking(() => ToolkitDefinition.VerifyRuntimeCompatibility(toolkit, metadata.Object))
                .Should().Throw<AutomateException>()
                .WithMessage(
                    ExceptionMessages.ToolkitDefinition_Incompatible_ToolkitAheadOfMachine.Substitute("1.1.0",
                        "1.0.0-preview", "aproductname"));
        }

        [Fact]
        public void WhenVerifyRuntimeCompatibilityAndRuntimeVersionIsBreaking_ThenThrows()
        {
            var metadata = new Mock<IRuntimeMetadata>();
            metadata.Setup(md => md.ProductName).Returns("aproductname");
            metadata.Setup(md => md.RuntimeVersion).Returns("1.0.0".ToSemVersion);
            var toolkit = new ToolkitDefinition(new PatternDefinition("apatternname"), "0.1.0");

            FluentActions.Invoking(() => ToolkitDefinition.VerifyRuntimeCompatibility(toolkit, metadata.Object))
                .Should().Throw<AutomateException>()
                .WithMessage(
                    ExceptionMessages.ToolkitDefinition_Incompatible_MachineAheadOfToolkit.Substitute("0.1.0", "1.0.0",
                        "aproductname"));
        }

        [Fact]
        public void WhenVerifyRuntimeCompatibilityAndToolkitVersionIsBreaking_ThenThrows()
        {
            var metadata = new Mock<IRuntimeMetadata>();
            metadata.Setup(md => md.ProductName).Returns("aproductname");
            metadata.Setup(md => md.RuntimeVersion).Returns("1.0.0".ToSemVersion);
            var toolkit = new ToolkitDefinition(new PatternDefinition("apatternname"), "2.0.0");

            FluentActions.Invoking(() => ToolkitDefinition.VerifyRuntimeCompatibility(toolkit, metadata.Object))
                .Should().Throw<AutomateException>()
                .WithMessage(
                    ExceptionMessages.ToolkitDefinition_Incompatible_ToolkitAheadOfMachine.Substitute("2.0.0", "1.0.0",
                        "aproductname"));
        }

        [Fact]
        public void WhenVerifyRuntimeCompatibilityAndRuntimeVersionIsNonBreaking_ThenSucceeds()
        {
            var metadata = new Mock<IRuntimeMetadata>();
            metadata.Setup(md => md.ProductName).Returns("aproductname");
            metadata.Setup(md => md.RuntimeVersion).Returns("0.2.0".ToSemVersion);
            var toolkit = new ToolkitDefinition(new PatternDefinition("apatternname"), "0.1.0");

            ToolkitDefinition.VerifyRuntimeCompatibility(toolkit, metadata.Object);
        }

        [Fact]
        public void
            WhenVerifyRuntimeCompatibilityAndToolkitVersionIsNonBreaking_ThenThrows()
        {
            var metadata = new Mock<IRuntimeMetadata>();
            metadata.Setup(md => md.ProductName).Returns("aproductname");
            metadata.Setup(md => md.RuntimeVersion).Returns("1.0.0".ToSemVersion);
            var toolkit = new ToolkitDefinition(new PatternDefinition("apatternname"), "1.1.0");

            ToolkitDefinition.VerifyRuntimeCompatibility(toolkit, metadata.Object);
        }

        private static ToolkitDefinition MakeDetachedToolkit(PatternDefinition oldPattern,
            Dictionary<string, byte[]> codeTemplateFiles = null)
        {
            var metadata = new CliRuntimeMetadata();
            var (_, toolkit) =
                PatternToolkitPackager.Pack(metadata, oldPattern, new VersionInstruction(), GetTemplateContent());

            Func<PatternDefinition, CodeTemplate, CodeTemplateContent> GetTemplateContent()
            {
                return (_, codeTemplate) =>
                {
                    if (codeTemplateFiles.NotExists())
                    {
                        return new CodeTemplateContent
                        {
                            Content = Array.Empty<byte>(),
                            LastModifiedUtc = codeTemplate.LastModifiedUtc
                        };
                    }

                    if (!codeTemplateFiles.ContainsKey(codeTemplate.Id))
                    {
                        throw new Exception($"No content for codetemplate: {codeTemplate.Id}");
                    }

                    return new CodeTemplateContent
                    {
                        Content = codeTemplateFiles[codeTemplate.Id],
                        LastModifiedUtc = DateTime.UtcNow
                    };
                };
            }

            var factory = new AutomatePersistableFactory();
            return toolkit.ToJson(factory).FromJson<ToolkitDefinition>(factory);
        }
    }
}