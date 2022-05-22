using System;
using System.Collections.Generic;
using System.Threading;
using Automate.CLI.Domain;
using Automate.CLI.Extensions;
using Automate.CLI.Infrastructure;
using FluentAssertions;
using Xunit;

namespace CLI.UnitTests.Domain
{
    [Trait("Category", "Unit")]
    public class ToolkitDefinitionSpec
    {
        [Fact]
        public void WhenMigratePatternAndNoTemplates_ThenReturnsEmptyResult()
        {
            var originalToolkit = MakeDetachedToolkit(new PatternDefinition("apatternname"));
            var solution = new SolutionDefinition(originalToolkit);

            var result = new SolutionUpgradeResult(solution, "0.0.0", "0.1.0");

            solution.Toolkit.MigratePattern(originalToolkit, result);

            solution.Toolkit.Version.Should().Be("0.1.0");
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
            var solution = new SolutionDefinition(originalToolkit);
            Thread.Sleep(TimeSpan.FromSeconds(1)); // Delay for LastModified comparisons
            var updatedToolkit = MakeDetachedToolkit(pattern, new Dictionary<string, byte[]>
            {
                { codeTemplate1.Id, new byte[] { 0x01, 0x02, 0x03 } },
                { codeTemplate2.Id, new byte[] { 0x09, 0x08, 0x07 } }
            });
            var result = new SolutionUpgradeResult(solution, "0.0.0", "0.1.0");

            solution.Toolkit.MigratePattern(updatedToolkit, result);

            solution.Toolkit.Pattern.CodeTemplates.Should().Contain(x => x.Id == codeTemplate1.Id);
            solution.Toolkit.Pattern.CodeTemplates.Should().Contain(x => x.Id == codeTemplate2.Id);
            solution.Toolkit.CodeTemplateFiles[0].Contents.Should().ContainInOrder(0x01, 0x02, 0x03);
            solution.Toolkit.CodeTemplateFiles[1].Contents.Should().ContainInOrder(0x09, 0x08, 0x07);
            solution.Toolkit.Version.Should().Be("0.2.0");
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
            var solution = new SolutionDefinition(originalToolkit);
            pattern.DeleteCodeTemplate(codeTemplate.Name, true);
            var updatedToolkit = MakeDetachedToolkit(pattern);
            var result = new SolutionUpgradeResult(solution, "0.0.0", "0.1.0");

            solution.Toolkit.MigratePattern(updatedToolkit, result);

            solution.Toolkit.Pattern.CodeTemplates.Should().BeEmpty();
            solution.Toolkit.CodeTemplateFiles.Should().BeEmpty();
            solution.Toolkit.Version.Should().Be("1.0.0");
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
            var solution = new SolutionDefinition(originalToolkit);
            var codeTemplate2 = pattern.AddCodeTemplate("atemplatename2", "afullpath", "anextension");
            var updatedToolkit = MakeDetachedToolkit(pattern, new Dictionary<string, byte[]>
            {
                { codeTemplate1.Id, new byte[] { 0x01, 0x02, 0x03 } },
                { codeTemplate2.Id, new byte[] { 0x09, 0x08, 0x07 } }
            });
            var result = new SolutionUpgradeResult(solution, "0.0.0", "0.1.0");

            solution.Toolkit.MigratePattern(updatedToolkit, result);

            solution.Toolkit.Pattern.CodeTemplates.Should().Contain(x => x.Id == codeTemplate1.Id);
            solution.Toolkit.Pattern.CodeTemplates.Should().Contain(x => x.Id == codeTemplate2.Id);
            solution.Toolkit.CodeTemplateFiles[0].Contents.Should().ContainInOrder(0x01, 0x02, 0x03);
            solution.Toolkit.CodeTemplateFiles[1].Contents.Should().ContainInOrder(0x09, 0x08, 0x07);
            solution.Toolkit.Version.Should().Be("0.2.0");
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
            var solution = new SolutionDefinition(originalToolkit);
            var codeTemplate3 = pattern.AddCodeTemplate("atemplatename3", "afullpath", "anextension");
            pattern.DeleteCodeTemplate(codeTemplate2.Name, true);
            var updatedToolkit = MakeDetachedToolkit(pattern, new Dictionary<string, byte[]>
            {
                { codeTemplate1.Id, new byte[] { 0x01, 0x02, 0x03 } },
                { codeTemplate3.Id, new byte[] { 0x09, 0x08, 0x07 } }
            });
            var result = new SolutionUpgradeResult(solution, "0.0.0", "0.1.0");

            solution.Toolkit.MigratePattern(updatedToolkit, result);

            solution.Toolkit.Pattern.CodeTemplates.Should().HaveCount(2);
            solution.Toolkit.Pattern.CodeTemplates.Should().Contain(x => x.Id == codeTemplate1.Id);
            solution.Toolkit.Pattern.CodeTemplates.Should().Contain(x => x.Id == codeTemplate3.Id);
            solution.Toolkit.CodeTemplateFiles[0].Contents.Should().ContainInOrder(0x01, 0x02, 0x03);
            solution.Toolkit.CodeTemplateFiles[1].Contents.Should().ContainInOrder(0x09, 0x08, 0x07);
            solution.Toolkit.Version.Should().Be("1.0.0");
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

        private static ToolkitDefinition MakeDetachedToolkit(PatternDefinition oldPattern, Dictionary<string, byte[]> codeTemplateFiles = null)
        {
            var (_, toolkit) = PatternToolkitPackager.Pack(oldPattern, new VersionInstruction(), GetTemplateContent());

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