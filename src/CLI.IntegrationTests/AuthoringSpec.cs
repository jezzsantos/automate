using System;
using System.IO;
using System.Linq;
using Automate.CLI;
using Automate.CLI.Domain;
using Automate.CLI.Extensions;
using Automate.CLI.Infrastructure;
using FluentAssertions;
using Xunit;

namespace CLI.IntegrationTests
{
    [Trait("Category", "Integration")] [Collection("CLI")]
    public class AuthoringSpec
    {
        private readonly CliTestSetup setup;

        public AuthoringSpec(CliTestSetup setup)
        {
            this.setup = setup;
            this.setup.ResetRepository();
        }

        [Fact]
        public void WhenNoCommands_ThenDisplaysError()
        {
            this.setup.RunCommand("");

            this.setup.Should().DisplayErrorForMissingCommand();
        }

        [Fact]
        public void WhenCreateAndNoCommands_ThenDisplaysError()
        {
            this.setup.RunCommand($"{CommandLineApi.CreateCommandName}");

            this.setup.Should().DisplayErrorForMissingCommand();
        }

        [Fact]
        public void WhenCreateWithNameAndExists_ThenDisplaysError()
        {
            this.setup.RunCommand($"{CommandLineApi.CreateCommandName} pattern APattern");
            this.setup.RunCommand($"{CommandLineApi.CreateCommandName} pattern APattern");

            this.setup.Should().DisplayError(ExceptionMessages.PatternStore_FoundNamed, "APattern");
        }

        [Fact]
        public void WhenCreateWithName_ThenCreatesNewPattern()
        {
            this.setup.RunCommand($"{CommandLineApi.CreateCommandName} pattern APattern");

            this.setup.Should().DisplayError(OutputMessages.CommandLine_Output_NoPatternSelected);
            this.setup.Patterns.Single().Name.Should().Be("APattern");
            this.setup.LocalState.CurrentPattern.Should().Be(this.setup.Patterns.Single().Id);
        }

        [Fact]
        public void WhenCreateMultipleTimes_ThenCreatesNewPatterns()
        {
            this.setup.RunCommand($"{CommandLineApi.CreateCommandName} pattern APattern1");
            this.setup.RunCommand($"{CommandLineApi.CreateCommandName} pattern APattern2");
            this.setup.RunCommand($"{CommandLineApi.CreateCommandName} pattern APattern3");

            this.setup.Should().DisplayNoError();
            this.setup.Patterns.Should().Contain(x => x.Name == "APattern1");
            this.setup.Patterns.Should().Contain(x => x.Name == "APattern2");
            this.setup.Patterns.Should().Contain(x => x.Name == "APattern3");
            this.setup.LocalState.CurrentPattern.Should()
                .Be(this.setup.Patterns.Find(x => x.Name == "APattern3")!.Id);
        }

        [Fact]
        public void WhenEditAndNoCommands_ThenDisplaysError()
        {
            this.setup.RunCommand($"{CommandLineApi.EditCommandName}");

            this.setup.Should().DisplayErrorForMissingCommand();
        }

        [Fact]
        public void WhenSwitchWithoutName_ThenDisplaysError()
        {
            this.setup.RunCommand($"{CommandLineApi.EditCommandName} switch");

            this.setup.Should().DisplayErrorForMissingArgument("switch");
        }

        [Fact]
        public void WhenSwitchWithNameAndNotExists_ThenDisplaysError()
        {
            this.setup.RunCommand($"{CommandLineApi.EditCommandName} switch APattern");

            this.setup.Should()
                .DisplayError(ExceptionMessages.PatternStore_NotFoundAtLocationWithId, "APattern", this.setup.Location);
        }

        [Fact]
        public void WhenSwitchWithNameAndExists_ThenUsesPattern()
        {
            this.setup.RunCommand($"{CommandLineApi.CreateCommandName} pattern APattern");
            this.setup.RunCommand($"{CommandLineApi.EditCommandName} switch APattern");

            this.setup.Should().DisplayNoError();
            this.setup.LocalState.CurrentPattern.Should().Be(this.setup.Patterns.Single().Id);
        }

        [Fact]
        public void WhenAddCodeTemplateAndNoCurrentPattern_ThenDisplaysError()
        {
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-codetemplate \"Assets/CodeTemplates/code1.code\"");

            this.setup.Should()
                .DisplayError(ExceptionMessages.AuthoringApplication_NoCurrentPattern);
        }

        [Fact]
        public void WhenAddCodeTemplateAndFileMissing_ThenDisplaysHelp()
        {
            this.setup.RunCommand($"{CommandLineApi.CreateCommandName} pattern APattern");
            this.setup.RunCommand($"{CommandLineApi.EditCommandName} add-codetemplate");

            this.setup.Should().DisplayErrorForMissingArgument("add-codetemplate");
        }

        [Fact]
        public void WhenAddCodeTemplateAndUnnamed_ThenAddsCodeTemplateWithDefaultName()
        {
            this.setup.RunCommand($"{CommandLineApi.CreateCommandName} pattern APattern");
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-codetemplate \"Assets/CodeTemplates/code1.code\"");

            this.setup.Should().DisplayNoError();
            this.setup.Patterns.Single().CodeTemplates.First().Name.Should().Be("CodeTemplate1");
        }

        [Fact]
        public void WhenAddCodeTemplateAndNamed_ThenAddsCodeTemplate()
        {
            this.setup.RunCommand($"{CommandLineApi.CreateCommandName} pattern APattern");

            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-codetemplate \"Assets/CodeTemplates/code1.code\" --name ATemplateName");

            this.setup.Should().DisplayNoError();
            this.setup.Patterns.Single().CodeTemplates.First().Name.Should().Be("ATemplateName");
        }

        [Fact]
        public void WhenAddAttributeAndNoCurrentPattern_ThenDisplaysError()
        {
            this.setup.RunCommand($"{CommandLineApi.EditCommandName} add-attribute AProperty");

            this.setup.Should()
                .DisplayError(ExceptionMessages.AuthoringApplication_NoCurrentPattern);
        }

        [Fact]
        public void WhenAddAttribute_ThenAddsAttribute()
        {
            this.setup.RunCommand($"{CommandLineApi.CreateCommandName} pattern APattern");
            this.setup.RunCommand($"{CommandLineApi.EditCommandName} add-attribute AProperty");

            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayMessage(
                    OutputMessages.CommandLine_Output_AttributeAdded.FormatTemplate("AProperty",
                        this.setup.Patterns.Single().Id, this.setup.Patterns.Single().Attributes.Single().Id));
            this.setup.Patterns.Single().Attributes.Single().IsRequired.Should().BeFalse();
        }

        [Fact]
        public void WhenAddAttributeWithIsRequired_ThenAddsAttribute()
        {
            this.setup.RunCommand($"{CommandLineApi.CreateCommandName} pattern APattern");
            this.setup.RunCommand($"{CommandLineApi.EditCommandName} add-attribute AProperty --isrequired");

            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayMessage(
                    OutputMessages.CommandLine_Output_AttributeAdded.FormatTemplate("AProperty",
                        this.setup.Patterns.Single().Id, this.setup.Patterns.Single().Attributes.Single().Id));
            this.setup.Patterns.Single().Attributes.Single().IsRequired.Should().BeTrue();
        }

        [Fact]
        public void WhenAddAttributeWithIsRequiredFalse_ThenAddsAttribute()
        {
            this.setup.RunCommand($"{CommandLineApi.CreateCommandName} pattern APattern");
            this.setup.RunCommand($"{CommandLineApi.EditCommandName} add-attribute AProperty --isrequired false");

            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayMessage(
                    OutputMessages.CommandLine_Output_AttributeAdded.FormatTemplate("AProperty",
                        this.setup.Patterns.Single().Id, this.setup.Patterns.Single().Attributes.Single().Id));
            this.setup.Patterns.Single().Attributes.Single().IsRequired.Should().BeFalse();
        }

        [Fact]
        public void WhenAddAttributeAsChildOfDescendantElement_ThenAddsAttribute()
        {
            this.setup.RunCommand($"{CommandLineApi.CreateCommandName} pattern APattern");
            this.setup.RunCommand($"{CommandLineApi.EditCommandName} add-element AnElement1");
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-element AnElement2 --aschildof {{APattern.AnElement1}}");
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-attribute AProperty --aschildof {{APattern.AnElement1.AnElement2}}");

            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayMessage(
                    OutputMessages.CommandLine_Output_AttributeAdded.FormatTemplate("AProperty",
                        this.setup.Patterns.Single().Elements.Single().Elements.Single().Id,
                        this.setup.Patterns.Single().Elements.Single().Elements.Single().Attributes.Single().Id));
        }

        [Fact]
        public void WhenAddElementAndNoCurrentPattern_ThenDisplaysError()
        {
            this.setup.RunCommand($"{CommandLineApi.EditCommandName} add-element AnElement");

            this.setup.Should()
                .DisplayError(ExceptionMessages.AuthoringApplication_NoCurrentPattern);
        }

        [Fact]
        public void WhenAddElement_ThenAddsElement()
        {
            this.setup.RunCommand($"{CommandLineApi.CreateCommandName} pattern APattern");
            this.setup.RunCommand($"{CommandLineApi.EditCommandName} add-element AnElement");

            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayMessage(
                    OutputMessages.CommandLine_Output_ElementAdded.FormatTemplate("AnElement",
                        this.setup.Patterns.Single().Id, this.setup.Patterns.Single().Elements.Single().Id));
        }

        [Fact]
        public void WhenAddElementAsChildOfDescendantElement_ThenAddsElement()
        {
            this.setup.RunCommand($"{CommandLineApi.CreateCommandName} pattern APattern");
            this.setup.RunCommand($"{CommandLineApi.EditCommandName} add-element AnElement1");
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-element AnElement2 --aschildof {{APattern.AnElement1}}");
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-element AnElement3 --aschildof {{APattern.AnElement1.AnElement2}}");

            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayMessage(
                    OutputMessages.CommandLine_Output_ElementAdded.FormatTemplate("AnElement3",
                        this.setup.Patterns.Single().Elements.Single().Elements.Single().Id,
                        this.setup.Patterns.Single().Elements.Single().Elements.Single().Elements.Single().Id));
        }

        [Fact]
        public void WhenAddCollectionAndNoCurrentPattern_ThenDisplaysError()
        {
            this.setup.RunCommand($"{CommandLineApi.EditCommandName} add-collection ACollection");

            this.setup.Should()
                .DisplayError(ExceptionMessages.AuthoringApplication_NoCurrentPattern);
        }

        [Fact]
        public void WhenAddCollection_ThenAddsAttribute()
        {
            this.setup.RunCommand($"{CommandLineApi.CreateCommandName} pattern APattern");
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-collection ACollection --displayedas ADisplayName --describedas ADescription");

            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayMessage(
                    OutputMessages.CommandLine_Output_CollectionAdded.FormatTemplate("ACollection",
                        this.setup.Patterns.Single().Id, this.setup.Patterns.Single().Elements.Single().Id));
        }

        [Fact]
        public void WhenAddCodeTemplateCommand_ThenAddsCommand()
        {
            this.setup.RunCommand($"{CommandLineApi.CreateCommandName} pattern APattern");
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-codetemplate \"Assets/CodeTemplates/code1.code\" --name ATemplateName");
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-codetemplate-command \"ATemplateName\" --withpath ~/afilepath");

            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayMessage(
                    OutputMessages.CommandLine_Output_CodeTemplateCommandAdded.FormatTemplate("CodeTemplateCommand1",
                        this.setup.Patterns.Single().Automation.Single().Id));
        }

        [Fact]
        public void WhenAddCommandLaunchPointWithAllCommands_ThenAddsLaunchPoint()
        {
            this.setup.RunCommand($"{CommandLineApi.CreateCommandName} pattern APattern");
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-codetemplate \"Assets/CodeTemplates/code1.code\" --name ATemplateName");
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-codetemplate-command \"ATemplateName\" --withpath ~/afilepath");
            var commandId = this.setup.Patterns.Single().Automation.Single().Id;

            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-command-launchpoint \"*\" --name ALaunchPoint");

            var launchPoint = this.setup.Patterns.Single().Automation.Last();
            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayMessage(
                    OutputMessages.CommandLine_Output_LaunchPointAdded.FormatTemplate(launchPoint.Name, launchPoint.Id, commandId));
        }

        [Fact]
        public void WhenAddCommandLaunchPoint_ThenAddsLaunchPoint()
        {
            this.setup.RunCommand($"{CommandLineApi.CreateCommandName} pattern APattern");
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-codetemplate \"Assets/CodeTemplates/code1.code\" --name ATemplateName");
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-codetemplate-command \"ATemplateName\" --withpath ~/afilepath");
            var commandId = this.setup.Patterns.Single().Automation.Single().Id;

            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-command-launchpoint {commandId} --name ALaunchPoint");

            var launchPoint = this.setup.Patterns.Single().Automation.Last();
            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayMessage(
                    OutputMessages.CommandLine_Output_LaunchPointAdded.FormatTemplate(launchPoint.Name, launchPoint.Id, commandId));
        }

        [Fact]
        public void WhenUpdateCommandLaunchPointWithMoreCommands_ThenUpdatesLaunchPoint()
        {
            this.setup.RunCommand($"{CommandLineApi.CreateCommandName} pattern APattern");
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-codetemplate \"Assets/CodeTemplates/code1.code\" --name ATemplateName");
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-codetemplate-command \"ATemplateName\" --withpath ~/afilepath");
            var commandId1 = this.setup.Patterns.Single().Automation.First().Id;
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-command-launchpoint \"*\" --name ALaunchPoint");
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-codetemplate-command \"ATemplateName\" --withpath ~/afilepath");
            var commandId2 = this.setup.Patterns.Single().Automation.Last().Id;
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} update-command-launchpoint \"ALaunchPoint\" --add \"*\" --from \"{{APattern}}\"");

            var launchPoint = this.setup.Patterns.Single().Automation[1];
            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayMessage(
                    OutputMessages.CommandLine_Output_LaunchPointUpdated.FormatTemplate(launchPoint.Name, launchPoint.Id, new[] { commandId1, commandId2 }.Join(CommandLaunchPoint.CommandIdDelimiter)));
        }

        [Fact]
        public void WhenBuildAndNoCommands_ThenDisplaysError()
        {
            this.setup.RunCommand($"{CommandLineApi.BuildCommandName}");

            this.setup.Should().DisplayErrorForMissingCommand();
        }

        [Fact]
        public void WhenBuildToolkitWithCurrentVersion_ThenBuildsToolkitOnDesktopAndWarns()
        {
            this.setup.RunCommand($"{CommandLineApi.CreateCommandName} pattern APattern");
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-codetemplate \"Assets/CodeTemplates/code1.code\" --name ATemplateName");

            this.setup.RunCommand($"{CommandLineApi.BuildCommandName} toolkit --asversion 0.1.0");

            var pattern = this.setup.Patterns.Single();
            var codeTemplate = pattern.CodeTemplates.First();
            var desktopFolder = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            var location = Path.Combine(desktopFolder, "APattern_0.1.0.toolkit");
            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayMessage(
                    OutputMessages.CommandLine_Output_BuiltToolkit.FormatTemplate("APattern", "0.1.0", location));
            this.setup.Should()
                .DisplayMessage(
                    OutputMessages.CommandLine_Output_BuiltToolkit_Warning
                        .FormatTemplate(DomainMessages.ToolkitVersion_Warning
                            .Format("0.1.0", new[]
                            {
                                VersionChanges.PatternElement_CodeTemplate_Add
                                    .FormatTemplate(codeTemplate.Id, pattern.Id)
                            }.ToBulletList())));
        }

        [Fact]
        public void WhenBuildToolkitWithCurrentVersionAndBreakingChangesAndForceVersion_ThenBuildsToolkitOnDesktopAndWarns()
        {
            this.setup.RunCommand($"{CommandLineApi.CreateCommandName} pattern APattern");
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-attribute \"AnAttribute\" --isrequired ");
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-codetemplate \"Assets/CodeTemplates/code1.code\" --name ATemplateName");
            var pattern = this.setup.Patterns.Single();
            var codeTemplate = pattern.CodeTemplates.First();
            var attribute = pattern.Attributes.First();
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} delete-attribute \"AnAttribute\"");

            this.setup.RunCommand($"{CommandLineApi.BuildCommandName} toolkit --asversion 0.1.0 --force");

            var desktopFolder = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            var location = Path.Combine(desktopFolder, "APattern_0.1.0.toolkit");
            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayMessage(
                    OutputMessages.CommandLine_Output_BuiltToolkit.FormatTemplate("APattern", "0.1.0", location));
            this.setup.Should()
                .DisplayMessage(
                    OutputMessages.CommandLine_Output_BuiltToolkit_Warning
                        .FormatTemplate(DomainMessages.ToolkitVersion_Forced
                            .Format("0.1.0", new[]
                            {
                                VersionChanges.PatternElement_Attribute_Add.FormatTemplate(attribute.Id, pattern.Id),
                                VersionChanges.PatternElement_CodeTemplate_Add.FormatTemplate(codeTemplate.Id, pattern.Id),
                                VersionChanges.PatternElement_Attribute_Delete.FormatTemplate(attribute.Id, pattern.Id)
                            }.ToBulletList())));
        }

        [Fact]
        public void WhenBuildToolkit_ThenBuildsToolkitOnDesktop()
        {
            this.setup.RunCommand($"{CommandLineApi.CreateCommandName} pattern APattern");
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-codetemplate \"Assets/CodeTemplates/code1.code\" --name ATemplateName");

            this.setup.RunCommand($"{CommandLineApi.BuildCommandName} toolkit");

            var desktopFolder = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            var location = Path.Combine(desktopFolder, "APattern_0.1.0.toolkit");
            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayMessage(
                    OutputMessages.CommandLine_Output_BuiltToolkit.FormatTemplate("APattern", "0.1.0", location));
        }

        [Fact]
        public void WhenViewPatternAndNoCurrentPattern_ThenDisplaysError()
        {
            this.setup.RunCommand($"{CommandLineApi.ViewCommandName} pattern");

            this.setup.Should()
                .DisplayError(ExceptionMessages.AuthoringApplication_NoCurrentPattern);
        }

        [Fact]
        public void WhenViewPatternWithoutAutomation_ThenDisplaysTree()
        {
            this.setup.RunCommand($"{CommandLineApi.CreateCommandName} pattern APattern");
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-codetemplate \"Assets/CodeTemplates/code1.code\" --name ATemplateName");
            this.setup.RunCommand($"{CommandLineApi.EditCommandName} add-attribute AProperty");
            this.setup.RunCommand($"{CommandLineApi.EditCommandName} add-element AnElement");
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-attribute AProperty --aschildof {{APattern.AnElement}}");
            this.setup.RunCommand($"{CommandLineApi.EditCommandName} add-collection ACollection");
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-attribute AProperty --aschildof {{APattern.ACollection}}");

            this.setup.RunCommand($"{CommandLineApi.ViewCommandName} pattern");

            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayMessage(
                    OutputMessages.CommandLine_Output_PatternTree.FormatTemplate(
                        $"- APattern (root element) (attached with 1 code templates){Environment.NewLine}" +
                        $"\t- AProperty (attribute) (string){Environment.NewLine}" +
                        $"\t- AnElement (element){Environment.NewLine}" +
                        $"\t\t- AProperty (attribute) (string){Environment.NewLine}" +
                        $"\t- ACollection (collection){Environment.NewLine}" +
                        $"\t\t- AProperty (attribute) (string){Environment.NewLine}"
                        ,
                        this.setup.Patterns.Single().Id));
        }

        [Fact]
        public void WhenViewPatternWithAllDetails_ThenDisplaysTree()
        {
            this.setup.RunCommand($"{CommandLineApi.CreateCommandName} pattern APattern");
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-codetemplate \"Assets/CodeTemplates/code1.code\" --name ATemplateName");
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-codetemplate-command \"ATemplateName\" --name ACodeTemplateCommand1 --withpath ~/afilepath");
            var commandId = this.setup.Patterns.Single().Automation.Single().Id;
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-command-launchpoint {commandId} --name ALaunchPoint");
            this.setup.RunCommand($"{CommandLineApi.EditCommandName} add-attribute AProperty");
            this.setup.RunCommand($"{CommandLineApi.EditCommandName} add-element AnElement");
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-attribute AProperty --aschildof {{APattern.AnElement}}");
            this.setup.RunCommand($"{CommandLineApi.EditCommandName} add-collection ACollection");
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-attribute AProperty --aschildof {{APattern.ACollection}}");

            this.setup.RunCommand($"{CommandLineApi.ViewCommandName} pattern --all");

            var pattern = this.setup.Patterns.Single();
            var codeTemplatePath = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "Assets/CodeTemplates/code1.code"));

            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayMessage(
                    OutputMessages.CommandLine_Output_PatternTree.FormatTemplate(
                        $"- APattern [{pattern.Id}] (root element){Environment.NewLine}" +
                        $"\t- CodeTemplates:{Environment.NewLine}" +
                        $"\t\t- ATemplateName [{pattern.CodeTemplates.Single().Id}] (file: {codeTemplatePath}, ext: .code){Environment.NewLine}" +
                        $"\t- Automation:{Environment.NewLine}" +
                        $"\t\t- ACodeTemplateCommand1 [{pattern.Automation.First().Id}] (CodeTemplateCommand) (template: {pattern.CodeTemplates.Single().Id}, tearOff: false, path: ~/afilepath){Environment.NewLine}" +
                        $"\t\t- ALaunchPoint [{pattern.Automation.Last().Id}] (CommandLaunchPoint) (ids: {commandId}){Environment.NewLine}" +
                        $"\t- Attributes:{Environment.NewLine}" +
                        $"\t\t- AProperty (string){Environment.NewLine}" +
                        $"\t- Elements:{Environment.NewLine}" +
                        $"\t\t- AnElement [{pattern.Elements.First().Id}] (element){Environment.NewLine}" +
                        $"\t\t\t- Attributes:{Environment.NewLine}" +
                        $"\t\t\t\t- AProperty (string){Environment.NewLine}" +
                        $"\t\t- ACollection [{pattern.Elements.Last().Id}] (collection){Environment.NewLine}" +
                        $"\t\t\t- Attributes:{Environment.NewLine}" +
                        $"\t\t\t\t- AProperty (string){Environment.NewLine}"
                        ,
                        this.setup.Patterns.Single().Id));
        }

        [Fact]
        public void WhenTestTemplate_ThenDisplaysRenderedTemplate()
        {
            this.setup.RunCommand($"{CommandLineApi.CreateCommandName} pattern APattern");
            this.setup.RunCommand($"{CommandLineApi.EditCommandName} add-attribute AProperty1 --isrequired");
            this.setup.RunCommand($"{CommandLineApi.EditCommandName} add-attribute AProperty2 --typeis int");
            this.setup.RunCommand($"{CommandLineApi.EditCommandName} add-element AnElement1");
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-attribute AProperty3 --aschildof {{APattern.AnElement1}} --isoneof \"A;B;C\"");
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-codetemplate \"Assets/CodeTemplates/code2.code\" --name ATemplateName --aschildof {{APattern.AnElement1}}");

            this.setup.RunCommand(
                $"{CommandLineApi.TestCommandName} codetemplate ATemplateName --aschildof {{APattern.AnElement1}}");

            var template = this.setup.Patterns.Single().Elements.Single().CodeTemplates.First();
            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayMessage(
                    OutputMessages.CommandLine_Output_CodeTemplateTested.FormatTemplate("ATemplateName", template.Id,
                        $"A{Environment.NewLine}" +
                        $"aproperty11{Environment.NewLine}" +
                        $"A{Environment.NewLine}"));
        }

        [Fact]
        public void WhenTestTemplateWithImportedData_ThenDisplaysRenderedTemplate()
        {
            this.setup.RunCommand($"{CommandLineApi.CreateCommandName} pattern APattern");
            this.setup.RunCommand($"{CommandLineApi.EditCommandName} add-attribute AProperty1 --isrequired");
            this.setup.RunCommand($"{CommandLineApi.EditCommandName} add-attribute AProperty2 --typeis int");
            this.setup.RunCommand($"{CommandLineApi.EditCommandName} add-element AnElement1");
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-attribute AProperty3 --aschildof {{APattern.AnElement1}} --isoneof \"A;B;C\"");
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-codetemplate \"Assets/CodeTemplates/code2.code\" --name ATemplateName --aschildof {{APattern.AnElement1}}");
            this.setup.RunCommand(
                $"{CommandLineApi.TestCommandName} codetemplate ATemplateName --aschildof {{APattern.AnElement1}} --import-data \"Assets/TestData/data1.json\"");

            var template = this.setup.Patterns.Single().Elements.Single().CodeTemplates.First();
            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayMessage(
                    OutputMessages.CommandLine_Output_CodeTemplateTested.FormatTemplate("ATemplateName", template.Id,
                        $"A{Environment.NewLine}" +
                        $"aproperty11{Environment.NewLine}" +
                        $"A{Environment.NewLine}"));
        }
    }
}