﻿using System;
using System.IO;
using System.Linq;
using Automate.Authoring.Domain;
using Automate.CLI.Infrastructure;
using Automate.CLI.Infrastructure.Api;
using Automate.Common;
using Automate.Common.Extensions;
using Automate.Runtime.Domain;
using FluentAssertions;
using Xunit;

namespace CLI.IntegrationTests.Infrastructure.Api
{
    [Trait("Category", "Integration")] [Collection("CLI")]
    public class UpgradeSpec : IDisposable
    {
        private readonly CliTestSetup setup;

        public UpgradeSpec(CliTestSetup setup)
        {
            this.setup = setup;
            this.setup.ResetRepository();
            DeleteOutputFolders();
        }

        [Fact]
        public void WhenConfigureDraftAndToolkitUpgraded_ThenDisplaysError()
        {
            CreateDraftFromBuiltToolkit();

            this.setup.RunCommand(
                $"{CommandLineApi.ConfigureCommandName} on {{APattern}} --and-set \"AProperty1=avalue1\"");
            this.setup.RunCommand($"{CommandLineApi.EditCommandName} add-attribute AProperty5");
            RebuildReversionAndInstallToolkit();

            this.setup.RunCommand(
                $"{CommandLineApi.ConfigureCommandName} on {{APattern}} --and-set \"AProperty1=avalue2\"");

            var draft = this.setup.Draft;
            this.setup.Should()
                .DisplayError(
                    ExceptionMessages.RuntimeApplication_Incompatible_ToolkitAheadOfDraft.Substitute(draft.Name,
                        draft.Id,
                        "0.1.0", "0.2.0"));
        }

        [Fact]
        public void WhenUpgradeDraftAndToolkitUpgradedWithChanges_ThenDisplaysSuccess()
        {
            CreateDraftFromBuiltToolkit();
            this.setup.RunCommand(
                $"{CommandLineApi.ConfigureCommandName} on {{APattern}} --and-set \"AProperty1=avalue1\"");
            this.setup.RunCommand($"{CommandLineApi.EditCommandName} add-attribute AProperty5");
            RebuildReversionAndInstallToolkit();

            this.setup.RunCommand($"{CommandLineApi.UpgradeCommandName} draft");

            var draft = this.setup.Draft;
            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayOutput(OutputMessages.CommandLine_Output_DraftUpgradeSucceeded.SubstituteTemplate(draft.Name,
                    draft.Id, draft.PatternName, "0.1.0", "0.2.0",
                    $"* {MigrationChangeType.NonBreaking}: " +
                    MigrationMessages.DraftItem_AttributeAdded.SubstituteTemplate("APattern.AProperty5", null)));
        }

        [Fact]
        public void WhenUpgradeDraftAndToolkitNotAutoUpgradeable_ThenDisplaysError()
        {
            CreateDraftFromBuiltToolkit();
            this.setup.RunCommand(
                $"{CommandLineApi.ConfigureCommandName} on {{APattern}} --and-set \"AProperty1=avalue1\"");
            this.setup.RunCommand($"{CommandLineApi.EditCommandName} delete-attribute \"AProperty1\"");
            RebuildReversionAndInstallToolkit();

            this.setup.RunCommand($"{CommandLineApi.UpgradeCommandName} draft");

            var draft = this.setup.Draft;
            const string newVersion = "1.0.0";
            this.setup.Should()
                .DisplayError(ExceptionMessages.RuntimeApplication_UpgradeDraftFailed.SubstituteTemplate(draft.Name,
                    draft.Id, draft.PatternName, "0.1.0", newVersion,
                    $"* {MigrationChangeType.Abort}: " +
                    MigrationMessages.DraftDefinition_Upgrade_BreakingChangeForbidden.SubstituteTemplate(
                        draft.PatternName, newVersion) + $"{Environment.NewLine}"));
        }

        [Fact]
        public void WhenUpgradeDraftAndToolkitNotAutoUpgradeableAndForced_ThenDisplaysResults()
        {
            CreateDraftFromBuiltToolkit();
            this.setup.RunCommand(
                $"{CommandLineApi.ConfigureCommandName} on {{APattern}} --and-set \"AProperty1=avalue1\"");

            this.setup.RunCommand($"{CommandLineApi.EditCommandName} delete-attribute \"AProperty1\"");
            RebuildReversionAndInstallToolkit();

            this.setup.RunCommand($"{CommandLineApi.UpgradeCommandName} draft --force");

            var draft = this.setup.Draft;
            const string newVersion = "1.0.0";
            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayOutput(OutputMessages.CommandLine_Output_DraftUpgradeSucceeded.SubstituteTemplate(draft.Name,
                    draft.Id, draft.PatternName, "0.1.0", newVersion,
                    $"* {MigrationChangeType.Breaking}: " +
                    MigrationMessages.DraftDefinition_Upgrade_BreakingChangeForced.SubstituteTemplate(
                        draft.PatternName, newVersion) + $"{Environment.NewLine}"));
        }

        [Fact]
        public void WhenUpgradeDraftWithNonBreakingChanges_ThenDisplaysResults()
        {
            CreateDraftFromBuiltToolkit();
            this.setup.RunCommand(
                $"{CommandLineApi.ConfigureCommandName} on {{APattern}} --and-set \"AProperty1=avalue1\"");
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-codetemplate \"Assets/CodeTemplates/code1.code\" --name ACodeTemplate3");
            RebuildReversionAndInstallToolkit();

            this.setup.RunCommand($"{CommandLineApi.UpgradeCommandName} draft");

            var draft = this.setup.Draft;
            var codeTemplate = this.setup.Pattern.CodeTemplates.Last();
            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayOutput(OutputMessages.CommandLine_Output_DraftUpgradeSucceeded.SubstituteTemplate(draft.Name,
                    draft.Id, draft.PatternName, "0.1.0", "0.2.0",
                    $"* {MigrationChangeType.NonBreaking}: " +
                    MigrationMessages.ToolkitDefinition_CodeTemplateFile_Added.SubstituteTemplate(codeTemplate.Name,
                        codeTemplate.Id) + $"{Environment.NewLine}"));
        }

        [Fact]
        public void
            WhenExecuteCommandAfterToolkitUpgradedWithChangedCodeTemplateTargetPath_ThenDeletesOldFileAndWritesNewFile()
        {
            CreateDraftFromBuiltToolkit();
            this.setup.RunCommand($"{CommandLineApi.ExecuteCommandName} command \"ALaunchPoint1\"");
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} update-codetemplate-command \"CodeTemplateCommand1\" --isoneoff true --targetpath \"~/code/updated/updatedroundtrip.cs\"");
            RebuildReversionAndInstallToolkit();
            this.setup.RunCommand($"{CommandLineApi.UpgradeCommandName} draft");

            this.setup.RunCommand($"{CommandLineApi.ExecuteCommandName} command \"ALaunchPoint1\"");

            var artifactLink = this.setup.Draft.Model.ArtifactLinks.First().Path;
            var oldPath = GetFilePathInOutput(@"code/roundtrip.cs");
            var newPath = GetFilePathInOutput(@"code/updated/updatedroundtrip.cs");
            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayOutput(OutputMessages.CommandLine_Output_CommandExecutionSucceeded.SubstituteTemplate(
                    "ALaunchPoint1",
                    "* Warning: " +
                    InfrastructureMessages.CodeTemplateCommand_Log_Warning_Moved.Substitute(oldPath, newPath) +
                    $"{Environment.NewLine}" +
                    "* Succeeded: " + InfrastructureMessages.CodeTemplateCommand_Log_UpdatedLink.Substitute(
                        "updatedroundtrip.cs",
                        newPath) +
                    $"{Environment.NewLine}"
                ));
            artifactLink.Should().Be(newPath);
            var contents = File.ReadAllText(newPath);
            contents.Should().Be("some code");
        }

        public void Dispose()
        {
            this.setup.Reset();
        }

        private static void DeleteOutputFolders()
        {
            var directory = new DirectoryInfo(GetFilePathInOutput("code"));
            if (directory.Exists)
            {
                directory.Delete(true);
            }
        }

        private static string GetFilePathInOutput(string filename)
        {
            return Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, filename));
        }

        private static string GetFilePathOfExportedToolkit(string filename)
        {
            return Path.GetFullPath(Path.Combine(InfrastructureConstants.GetExportDirectory(), filename));
        }

        private void CreateDraftFromBuiltToolkit()
        {
            ConfigurePublishAndInstallToolkit();
            this.setup.RunCommand($"{CommandLineApi.RunCommandName} toolkit APattern");

            this.setup.Should().DisplayNoError();
        }

        private void ConfigurePublishAndInstallToolkit()
        {
            this.setup.RunCommand($"{CommandLineApi.CreateCommandName} pattern APattern");
            this.setup.RunCommand($"{CommandLineApi.EditCommandName} add-attribute AProperty1");
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-codetemplate \"Assets/CodeTemplates/code1.code\" --name ACodeTemplate1");
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-codetemplate-command \"ACodeTemplate1\" --targetpath \"~/code/roundtrip.cs\"");
            var commandId1 = this.setup.Pattern.Automation[0].Id;
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-command-launchpoint {commandId1} --name ALaunchPoint1");

            this.setup.Should().DisplayNoError();

            RebuildReversionAndInstallToolkit();
        }

        private void RebuildReversionAndInstallToolkit(
            string versionInstruction = PatternVersioningHistory.AutoIncrementInstruction)
        {
            this.setup.RunCommand($"{CommandLineApi.PublishCommandName} toolkit --asversion {versionInstruction}");
            var latestVersion = this.setup.Pattern.ToolkitVersion.Current;

            var location = GetFilePathOfExportedToolkit($"APattern_{latestVersion}.toolkit");
            this.setup.RunCommand($"{CommandLineApi.InstallCommandName} toolkit {location}");

            this.setup.Should().DisplayNoError();
        }
    }
}