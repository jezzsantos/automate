using System;
using System.IO;
using System.Linq;
using automate;
using automate.Extensions;
using automate.Infrastructure;
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
            this.setup.RunCommand($"{Program.CreateCommandName}");

            this.setup.Should().DisplayErrorForMissingCommand();
        }

        [Fact]
        public void WhenCreateWithNameAndExists_ThenDisplaysError()
        {
            this.setup.RunCommand($"{Program.CreateCommandName} pattern apattern");
            this.setup.RunCommand($"{Program.CreateCommandName} pattern apattern");

            this.setup.Should().DisplayError(ExceptionMessages.PatternStore_FoundNamed, "apattern");
        }

        [Fact]
        public void WhenCreateWithName_ThenCreatesNewPattern()
        {
            this.setup.RunCommand($"{Program.CreateCommandName} pattern apattern");

            this.setup.Should().DisplayError(OutputMessages.CommandLine_Output_NoPatternSelected);
            this.setup.Patterns.Single().Name.Should().Be("apattern");
            this.setup.LocalState.CurrentPattern.Should().Be(this.setup.Patterns.Single().Id);
        }

        [Fact]
        public void WhenCreateMultipleTimes_ThenCreatesNewPatterns()
        {
            this.setup.RunCommand($"{Program.CreateCommandName} pattern apattern1");
            this.setup.RunCommand($"{Program.CreateCommandName} pattern apattern2");
            this.setup.RunCommand($"{Program.CreateCommandName} pattern apattern3");

            this.setup.Should().DisplayNoError();
            this.setup.Patterns.Should().Contain(x => x.Name == "apattern1");
            this.setup.Patterns.Should().Contain(x => x.Name == "apattern2");
            this.setup.Patterns.Should().Contain(x => x.Name == "apattern3");
            this.setup.LocalState.CurrentPattern.Should()
                .Be(this.setup.Patterns.Find(x => x.Name == "apattern3")!.Id);
        }

        [Fact]
        public void WhenEditAndNoCommands_ThenDisplaysError()
        {
            this.setup.RunCommand($"{Program.EditCommandName}");

            this.setup.Should().DisplayErrorForMissingCommand();
        }

        [Fact]
        public void WhenUseWithoutName_ThenDisplaysError()
        {
            this.setup.RunCommand($"{Program.EditCommandName} use");

            this.setup.Should().DisplayErrorForMissingArgument("use");
        }

        [Fact]
        public void WhenUseWithNameAndNotExists_ThenDisplaysError()
        {
            this.setup.RunCommand($"{Program.EditCommandName} use apattern");

            this.setup.Should()
                .DisplayError(ExceptionMessages.PatternStore_NotFoundAtLocationWithId, "apattern", this.setup.Location);
        }

        [Fact]
        public void WhenUseWithNameAndExists_ThenUsesPattern()
        {
            this.setup.RunCommand($"{Program.CreateCommandName} pattern apattern");
            this.setup.RunCommand($"{Program.EditCommandName} use apattern");

            this.setup.Should().DisplayNoError();
            this.setup.LocalState.CurrentPattern.Should().Be(this.setup.Patterns.Single().Id);
        }

        [Fact]
        public void WhenAddCodeTemplateAndNoCurrentPattern_ThenDisplaysError()
        {
            var template = Path.Combine(Environment.CurrentDirectory, "Assets/CodeTemplates/code1.code");

            this.setup.RunCommand($"{Program.EditCommandName} add-codetemplate {template}");

            this.setup.Should()
                .DisplayError(ExceptionMessages.AuthoringApplication_NoCurrentPattern);
        }

        [Fact]
        public void WhenAddCodeTemplateAndFileMissing_ThenDisplaysHelp()
        {
            this.setup.RunCommand($"{Program.CreateCommandName} pattern apattern");
            this.setup.RunCommand($"{Program.EditCommandName} add-codetemplate");

            this.setup.Should().DisplayErrorForMissingArgument("add-codetemplate");
        }

        [Fact]
        public void WhenAddCodeTemplateAndUnnamed_ThenAddsCodeTemplateWithDefaultName()
        {
            var template = Path.Combine(Environment.CurrentDirectory, "Assets/CodeTemplates/code1.code");

            this.setup.RunCommand($"{Program.CreateCommandName} pattern apattern");
            this.setup.RunCommand($"{Program.EditCommandName} add-codetemplate \"{template}\"");

            this.setup.Should().DisplayNoError();
            this.setup.Patterns.Single().CodeTemplates.First().Name.Should().Be("CodeTemplate1");
        }

        [Fact]
        public void WhenAddCodeTemplateAndNamed_ThenAddsCodeTemplate()
        {
            var template = Path.Combine(Environment.CurrentDirectory, "Assets/CodeTemplates/code1.code");
            this.setup.RunCommand($"{Program.CreateCommandName} pattern apattern");

            this.setup.RunCommand(
                $"{Program.EditCommandName} add-codetemplate \"{template}\" --name atemplatename");

            this.setup.Should().DisplayNoError();
            this.setup.Patterns.Single().CodeTemplates.First().Name.Should().Be("atemplatename");
        }

        [Fact]
        public void WhenListCodeTemplatesAndNone_ThenDisplaysNone()
        {
            this.setup.RunCommand($"{Program.CreateCommandName} pattern apattern");

            this.setup.RunCommand($"{Program.EditCommandName} list-codetemplates");

            this.setup.Should().DisplayNoError();
            this.setup.Should().DisplayMessage(OutputMessages.CommandLine_Output_NoCodeTemplates);
        }

        [Fact]
        public void WhenListCodeTemplatesAndOne_ThenDisplaysOne()
        {
            var location = Path.Combine(Environment.CurrentDirectory, "Assets/CodeTemplates/code1.code");
            this.setup.RunCommand($"{Program.CreateCommandName} pattern apattern");
            this.setup.RunCommand(
                $"{Program.EditCommandName} add-codetemplate \"{location}\" --name atemplatename");

            this.setup.RunCommand($"{Program.EditCommandName} list-codetemplates");

            var template = this.setup.Patterns.Single().CodeTemplates.Single();

            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayMessage(
                    OutputMessages.CommandLine_Output_CodeTemplatesListed.FormatTemplate(
                        $"{{\"Name\": \"atemplatename\", \"ID\": \"{template.Id}\"}}"));
        }

        [Fact]
        public void WhenAddAttributeAndNoCurrentPattern_ThenDisplaysError()
        {
            this.setup.RunCommand($"{Program.EditCommandName} add-attribute anattribute");

            this.setup.Should()
                .DisplayError(ExceptionMessages.AuthoringApplication_NoCurrentPattern);
        }

        [Fact]
        public void WhenAddAttribute_ThenAddsAttribute()
        {
            this.setup.RunCommand($"{Program.CreateCommandName} pattern apattern");
            this.setup.RunCommand($"{Program.EditCommandName} add-attribute anattribute");

            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayMessage(
                    OutputMessages.CommandLine_Output_AttributeAdded.FormatTemplate("anattribute",
                        this.setup.Patterns.Single().Id, this.setup.Patterns.Single().Attributes.Single().Id));
            this.setup.Patterns.Single().Attributes.Single().IsRequired.Should().BeFalse();
        }

        [Fact]
        public void WhenAddAttributeWithIsRequired_ThenAddsAttribute()
        {
            this.setup.RunCommand($"{Program.CreateCommandName} pattern apattern");
            this.setup.RunCommand($"{Program.EditCommandName} add-attribute anattribute --isrequired");

            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayMessage(
                    OutputMessages.CommandLine_Output_AttributeAdded.FormatTemplate("anattribute",
                        this.setup.Patterns.Single().Id, this.setup.Patterns.Single().Attributes.Single().Id));
            this.setup.Patterns.Single().Attributes.Single().IsRequired.Should().BeTrue();
        }

        [Fact]
        public void WhenAddAttributeWithIsRequiredFalse_ThenAddsAttribute()
        {
            this.setup.RunCommand($"{Program.CreateCommandName} pattern apattern");
            this.setup.RunCommand($"{Program.EditCommandName} add-attribute anattribute --isrequired false");

            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayMessage(
                    OutputMessages.CommandLine_Output_AttributeAdded.FormatTemplate("anattribute",
                        this.setup.Patterns.Single().Id, this.setup.Patterns.Single().Attributes.Single().Id));
            this.setup.Patterns.Single().Attributes.Single().IsRequired.Should().BeFalse();
        }

        [Fact]
        public void WhenAddAttributeAsChildOfDeepElement_ThenAddsAttribute()
        {
            this.setup.RunCommand($"{Program.CreateCommandName} pattern apattern");
            this.setup.RunCommand($"{Program.EditCommandName} add-element anelementname1");
            this.setup.RunCommand(
                $"{Program.EditCommandName} add-element anelementname2 --aschildof {{apattern.anelementname1}}");
            this.setup.RunCommand(
                $"{Program.EditCommandName} add-attribute anattribute --aschildof {{apattern.anelementname1.anelementname2}}");

            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayMessage(
                    OutputMessages.CommandLine_Output_AttributeAdded.FormatTemplate("anattribute",
                        this.setup.Patterns.Single().Elements.Single().Elements.Single().Id,
                        this.setup.Patterns.Single().Elements.Single().Elements.Single().Attributes.Single().Id));
        }

        [Fact]
        public void WhenAddElementAndNoCurrentPattern_ThenDisplaysError()
        {
            this.setup.RunCommand($"{Program.EditCommandName} add-element anelement");

            this.setup.Should()
                .DisplayError(ExceptionMessages.AuthoringApplication_NoCurrentPattern);
        }

        [Fact]
        public void WhenAddElement_ThenAddsElement()
        {
            this.setup.RunCommand($"{Program.CreateCommandName} pattern apattern");
            this.setup.RunCommand($"{Program.EditCommandName} add-element anelement");

            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayMessage(
                    OutputMessages.CommandLine_Output_ElementAdded.FormatTemplate("anelement",
                        this.setup.Patterns.Single().Id, this.setup.Patterns.Single().Elements.Single().Id));
        }

        [Fact]
        public void WhenAddElementAsChildOfDeepElement_ThenAddsElement()
        {
            this.setup.RunCommand($"{Program.CreateCommandName} pattern apattern");
            this.setup.RunCommand($"{Program.EditCommandName} add-element anelementname1");
            this.setup.RunCommand(
                $"{Program.EditCommandName} add-element anelementname2 --aschildof {{apattern.anelementname1}}");
            this.setup.RunCommand(
                $"{Program.EditCommandName} add-element anelementname3 --aschildof {{apattern.anelementname1.anelementname2}}");

            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayMessage(
                    OutputMessages.CommandLine_Output_ElementAdded.FormatTemplate("anelementname3",
                        this.setup.Patterns.Single().Elements.Single().Elements.Single().Id,
                        this.setup.Patterns.Single().Elements.Single().Elements.Single().Elements.Single().Id));
        }

        [Fact]
        public void WhenAddCollectionAndNoCurrentPattern_ThenDisplaysError()
        {
            this.setup.RunCommand($"{Program.EditCommandName} add-collection acollection");

            this.setup.Should()
                .DisplayError(ExceptionMessages.AuthoringApplication_NoCurrentPattern);
        }

        [Fact]
        public void WhenAddCollection_ThenAddsAttribute()
        {
            this.setup.RunCommand($"{Program.CreateCommandName} pattern apattern");
            this.setup.RunCommand(
                $"{Program.EditCommandName} add-collection acollection --displayedas adisplayname --describedas adescription");

            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayMessage(
                    OutputMessages.CommandLine_Output_CollectionAdded.FormatTemplate("acollection",
                        this.setup.Patterns.Single().Id, this.setup.Patterns.Single().Elements.Single().Id));
        }

        [Fact]
        public void WhenAddCodeTemplateCommand_ThenAddsCommand()
        {
            this.setup.RunCommand($"{Program.CreateCommandName} pattern apattern");
            this.setup.RunCommand(
                $"{Program.EditCommandName} add-codetemplate-command \"CodeTemplate1\" --withpath ~/afilepath");

            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayMessage(
                    OutputMessages.CommandLine_Output_CodeTemplateCommandAdded.FormatTemplate("CodeTemplate1",
                        this.setup.Patterns.Single().Automation.Single().Id));
        }

        [Fact]
        public void WhenAddCommandLaunchPoint_ThenAddsLaunchPoint()
        {
            this.setup.RunCommand($"{Program.CreateCommandName} pattern apattern");
            this.setup.RunCommand(
                $"{Program.EditCommandName} add-codetemplate-command \"CodeTemplate1\" --withpath ~/afilepath");
            var commandId = this.setup.Patterns.Single().Automation.Single().Id;

            this.setup.RunCommand(
                $"{Program.EditCommandName} add-command-launchpoint {commandId} --name alaunchpoint");

            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayMessage(
                    OutputMessages.CommandLine_Output_LaunchPointAdded.FormatTemplate("alaunchpoint"));
        }

        [Fact]
        public void WhenBuildAndNoCommands_ThenDisplaysError()
        {
            this.setup.RunCommand($"{Program.BuildCommandName}");

            this.setup.Should().DisplayErrorForMissingCommand();
        }

        [Fact]
        public void WhenBuildToolkit_ThenBuildsToolkitOnDesktop()
        {
            var template = Path.Combine(Environment.CurrentDirectory, "Assets/CodeTemplates/code1.code");

            this.setup.RunCommand($"{Program.CreateCommandName} pattern apattern");
            this.setup.RunCommand(
                $"{Program.EditCommandName} add-codetemplate \"{template}\" --name atemplatename");

            this.setup.RunCommand($"{Program.BuildCommandName} toolkit");

            var desktopFolder = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            var location = Path.Combine(desktopFolder, "apattern_1.0.toolkit");
            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayMessage(
                    OutputMessages.CommandLine_Output_BuiltToolkit.FormatTemplate("apattern", "1.0.0", location));
        }

        [Fact]
        public void WhenListElementsAndNoCurrentPattern_ThenDisplaysError()
        {
            this.setup.RunCommand($"{Program.EditCommandName} list-elements");

            this.setup.Should()
                .DisplayError(ExceptionMessages.AuthoringApplication_NoCurrentPattern);
        }

        [Fact]
        public void WhenListElements_ThenDisplaysTree()
        {
            var template = Path.Combine(Environment.CurrentDirectory, "Assets/CodeTemplates/code1.code");
            this.setup.RunCommand($"{Program.CreateCommandName} pattern apattern");
            this.setup.RunCommand(
                $"{Program.EditCommandName} add-codetemplate \"{template}\" --name atemplatename");
            this.setup.RunCommand($"{Program.EditCommandName} add-attribute anattribute");
            this.setup.RunCommand($"{Program.EditCommandName} add-element anelement");
            this.setup.RunCommand(
                $"{Program.EditCommandName} add-attribute anattribute --aschildof {{apattern.anelement}}");
            this.setup.RunCommand($"{Program.EditCommandName} add-collection acollection");
            this.setup.RunCommand(
                $"{Program.EditCommandName} add-attribute anattribute --aschildof {{apattern.acollection}}");

            this.setup.RunCommand($"{Program.EditCommandName} list-elements");

            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayMessage(
                    OutputMessages.CommandLine_Output_ElementsListed.FormatTemplate(
                        "- apattern (root element) (attached with 1 code templates)\n" +
                        "\t- anattribute (attribute) (string)\n" +
                        "\t- anelement (element)\n" +
                        "\t\t- anattribute (attribute) (string)\n" +
                        "\t- acollection (collection)\n" +
                        "\t\t- anattribute (attribute) (string)\n"
                        ,
                        this.setup.Patterns.Single().Id));
        }
    }
}