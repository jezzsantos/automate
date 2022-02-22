using System;
using System.IO;
using System.Linq;
using Automate.CLI;
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
        public void WhenUseWithoutName_ThenDisplaysError()
        {
            this.setup.RunCommand($"{CommandLineApi.EditCommandName} use");

            this.setup.Should().DisplayErrorForMissingArgument("use");
        }

        [Fact]
        public void WhenUseWithNameAndNotExists_ThenDisplaysError()
        {
            this.setup.RunCommand($"{CommandLineApi.EditCommandName} use APattern");

            this.setup.Should()
                .DisplayError(ExceptionMessages.PatternStore_NotFoundAtLocationWithId, "APattern", this.setup.Location);
        }

        [Fact]
        public void WhenUseWithNameAndExists_ThenUsesPattern()
        {
            this.setup.RunCommand($"{CommandLineApi.CreateCommandName} pattern APattern");
            this.setup.RunCommand($"{CommandLineApi.EditCommandName} use APattern");

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
        public void WhenListCodeTemplatesAndNone_ThenDisplaysNone()
        {
            this.setup.RunCommand($"{CommandLineApi.CreateCommandName} pattern APattern");

            this.setup.RunCommand($"{CommandLineApi.EditCommandName} list-codetemplates");

            this.setup.Should().DisplayNoError();
            this.setup.Should().DisplayMessage(OutputMessages.CommandLine_Output_NoCodeTemplates);
        }

        [Fact]
        public void WhenListCodeTemplatesAndOne_ThenDisplaysOne()
        {
            this.setup.RunCommand($"{CommandLineApi.CreateCommandName} pattern APattern");
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-codetemplate \"Assets/CodeTemplates/code1.code\" --name ATemplateName");

            this.setup.RunCommand($"{CommandLineApi.EditCommandName} list-codetemplates");

            var template = this.setup.Patterns.Single().CodeTemplates.Single();

            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayMessage(
                    OutputMessages.CommandLine_Output_CodeTemplatesListed.FormatTemplate(
                        $"{{\"Name\": \"ATemplateName\", \"ID\": \"{template.Id}\"}}"));
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
        public void WhenAddAttributeAsChildOfDeepElement_ThenAddsAttribute()
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
        public void WhenAddElementAsChildOfDeepElement_ThenAddsElement()
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

            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayMessage(
                    OutputMessages.CommandLine_Output_LaunchPointAdded.FormatTemplate("ALaunchPoint"));
        }

        [Fact]
        public void WhenBuildAndNoCommands_ThenDisplaysError()
        {
            this.setup.RunCommand($"{CommandLineApi.BuildCommandName}");

            this.setup.Should().DisplayErrorForMissingCommand();
        }

        [Fact]
        public void WhenBuildToolkit_ThenBuildsToolkitOnDesktop()
        {
            this.setup.RunCommand($"{CommandLineApi.CreateCommandName} pattern APattern");
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-codetemplate \"Assets/CodeTemplates/code1.code\" --name ATemplateName");

            this.setup.RunCommand($"{CommandLineApi.BuildCommandName} toolkit");

            var desktopFolder = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            var location = Path.Combine(desktopFolder, "APattern_1.0.toolkit");
            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayMessage(
                    OutputMessages.CommandLine_Output_BuiltToolkit.FormatTemplate("APattern", "1.0.0", location));
        }

        [Fact]
        public void WhenListElementsAndNoCurrentPattern_ThenDisplaysError()
        {
            this.setup.RunCommand($"{CommandLineApi.EditCommandName} list-elements");

            this.setup.Should()
                .DisplayError(ExceptionMessages.AuthoringApplication_NoCurrentPattern);
        }

        [Fact]
        public void WhenListElements_ThenDisplaysTree()
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

            this.setup.RunCommand($"{CommandLineApi.EditCommandName} list-elements");

            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayMessage(
                    OutputMessages.CommandLine_Output_ElementsListed.FormatTemplate(
                        "- APattern (root element) (attached with 1 code templates)\n" +
                        "\t- AProperty (attribute) (string)\n" +
                        "\t- AnElement (element)\n" +
                        "\t\t- AProperty (attribute) (string)\n" +
                        "\t- ACollection (collection)\n" +
                        "\t\t- AProperty (attribute) (string)\n"
                        ,
                        this.setup.Patterns.Single().Id));
        }
    }
}