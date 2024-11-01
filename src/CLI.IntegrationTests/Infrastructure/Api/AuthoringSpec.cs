using System;
using System.IO;
using System.Linq;
using Automate.Authoring.Domain;
using Automate.CLI.Infrastructure;
using Automate.CLI.Infrastructure.Api;
using Automate.Common;
using Automate.Common.Domain;
using Automate.Common.Extensions;
using FluentAssertions;
using Xunit;

namespace CLI.IntegrationTests.Infrastructure.Api
{
    [Trait("Category", "Integration")] [Collection("CLI")]
    public class AuthoringSpec : IDisposable
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
        public void WhenCreate_ThenCreatesNewPattern()
        {
            this.setup.RunCommand(
                $"{CommandLineApi.CreateCommandName} pattern APattern --displayedas ADisplayName --describedas ADescription");

            this.setup.Should().DisplayWarning(OutputMessages.CommandLine_Output_Preamble_NoPatternSelected);
            var pattern = this.setup.Pattern;
            pattern.Name.Should().Be("APattern");
            pattern.DisplayName.Should().Be("ADisplayName");
            pattern.Description.Should().Be("ADescription");
            this.setup.LocalState.CurrentPattern.Should().Be(this.setup.Pattern.Id);
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
                .DisplayError(ExceptionMessages.PatternStore_NotFoundAtLocationWithId, "APattern",
                    this.setup.PatternStoreLocation);
        }

        [Fact]
        public void WhenSwitchWithNameAndExists_ThenUsesPattern()
        {
            this.setup.RunCommand($"{CommandLineApi.CreateCommandName} pattern APattern");
            this.setup.RunCommand($"{CommandLineApi.EditCommandName} switch {this.setup.Pattern.Id}");

            this.setup.Should().DisplayNoError();
            this.setup.LocalState.CurrentPattern.Should().Be(this.setup.Pattern.Id);
        }

        [Fact]
        public void WhenViewPatternAndNoCurrentPattern_ThenDisplaysError()
        {
            this.setup.RunCommand($"{CommandLineApi.ViewCommandName} pattern");

            this.setup.Should()
                .DisplayError(ExceptionMessages.AuthoringApplication_NoCurrentPattern);
        }

        [Fact]
        public void WhenViewPatternWithoutAutomation_ThenDisplaysSchema()
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

            var pattern = this.setup.Pattern;
            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayOutput(
                    OutputMessages.CommandLine_Output_PatternSchema.SubstituteTemplate(pattern.Name, pattern.Id,
                        pattern.ToolkitVersion.Current,
                        $"- APattern (root element) (attached with 1 code templates){Environment.NewLine}" +
                        $"\t- AProperty (attribute) (string){Environment.NewLine}" +
                        $"\t- AnElement (element){Environment.NewLine}" +
                        $"\t\t- AProperty (attribute) (string){Environment.NewLine}" +
                        $"\t- ACollection (collection){Environment.NewLine}" +
                        $"\t\t- AProperty (attribute) (string){Environment.NewLine}"
                        ,
                        this.setup.Pattern.Id));
        }

        [Fact]
        public void WhenViewPatternWithAllDetails_ThenDisplaysSchema()
        {
            this.setup.RunCommand($"{CommandLineApi.CreateCommandName} pattern APattern");
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-codetemplate \"Assets/CodeTemplates/code1.code\" --name ATemplateName");
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-codetemplate-command \"ATemplateName\" --name ACodeTemplateCommand1 --targetpath ~/afilepath");
            var commandId = this.setup.Pattern.Automation.Single().Id;
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

            var pattern = this.setup.Pattern;
            var codeTemplatePath =
                Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "Assets/CodeTemplates/code1.code"));

            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayOutput(
                    OutputMessages.CommandLine_Output_PatternSchema.SubstituteTemplate(pattern.Name, pattern.Id,
                        pattern.ToolkitVersion.Current,
                        $"- APattern [{pattern.Id}] (root element){Environment.NewLine}" +
                        $"\t- CodeTemplates:{Environment.NewLine}" +
                        $"\t\t- ATemplateName [{pattern.CodeTemplates.Single().Id}] (original: {PatternConfigurationVisitor.TruncateCodeTemplatePath(codeTemplatePath)}){Environment.NewLine}" +
                        $"\t- Automation:{Environment.NewLine}" +
                        $"\t\t- ACodeTemplateCommand1 [{pattern.Automation.First().Id}] (CodeTemplateCommand) (template: {pattern.CodeTemplates.Single().Id}, always, path: ~/afilepath){Environment.NewLine}" +
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
                        this.setup.Pattern.Id));
        }

        [Fact]
        public void WhenListPatternsAndNone_ThenDisplaysNone()
        {
            this.setup.RunCommand($"{CommandLineApi.ListCommandName} patterns");

            this.setup.Should()
                .DisplayOutput(
                    OutputMessages.CommandLine_Output_NoEditablePatterns);
        }

        [Fact]
        public void WhenListPatternsAndSome_ThenDisplaysList()
        {
            this.setup.RunCommand($"{CommandLineApi.CreateCommandName} pattern APattern1");
            this.setup.RunCommand($"{CommandLineApi.CreateCommandName} pattern APattern2");

            this.setup.RunCommand($"{CommandLineApi.ListCommandName} patterns");

            var pattern1 = this.setup.Patterns.First();
            var pattern2 = this.setup.Patterns.Last();

            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayOutput(
                    OutputMessages.CommandLine_Output_EditablePatternsListed.SubstituteTemplate(
                        $"\"Name\": \"{pattern1.Name}\", \"Version\": \"{pattern1.ToolkitVersion.Current}\", \"ID\": \"{pattern1.Id}\", \"IsCurrent\": \"false\"{Environment.NewLine}" +
                        $"\"Name\": \"{pattern2.Name}\", \"Version\": \"{pattern2.ToolkitVersion.Current}\", \"ID\": \"{pattern2.Id}\", \"IsCurrent\": \"true\""));
        }

        [Fact]
        public void WhenUpdatePattern_ThenUpdatesName()
        {
            this.setup.RunCommand($"{CommandLineApi.CreateCommandName} pattern APattern");
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} update-pattern --name APatternName2 --displayedas ADisplayName2 --describedas ADescription2");

            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayOutput(
                    OutputMessages.CommandLine_Output_PatternUpdated.SubstituteTemplate("APatternName2"));
            var pattern = this.setup.Pattern;
            pattern.Name.Should().Be("APatternName2");
            pattern.DisplayName.Should().Be("ADisplayName2");
            pattern.Description.Should().Be("ADescription2");
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

            var attribute = this.setup.Pattern.Attributes.Single();
            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayOutput(
                    OutputMessages.CommandLine_Output_AttributeAdded.SubstituteTemplate("AProperty",
                        attribute.Id, this.setup.Pattern.Id));
            attribute.IsRequired.Should().BeFalse();
        }

        [Fact]
        public void WhenAddAttributeWithIsRequired_ThenAddsAttribute()
        {
            this.setup.RunCommand($"{CommandLineApi.CreateCommandName} pattern APattern");
            this.setup.RunCommand($"{CommandLineApi.EditCommandName} add-attribute AProperty --isrequired");

            var attribute = this.setup.Pattern.Attributes.Single();
            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayOutput(
                    OutputMessages.CommandLine_Output_AttributeAdded.SubstituteTemplate("AProperty",
                        attribute.Id, this.setup.Pattern.Id));
            attribute.IsRequired.Should().BeTrue();
        }

        [Fact]
        public void WhenAddAttributeWithIsRequiredFalse_ThenAddsAttribute()
        {
            this.setup.RunCommand($"{CommandLineApi.CreateCommandName} pattern APattern");
            this.setup.RunCommand($"{CommandLineApi.EditCommandName} add-attribute AProperty --isrequired false");

            var attribute = this.setup.Pattern.Attributes.Single();
            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayOutput(
                    OutputMessages.CommandLine_Output_AttributeAdded.SubstituteTemplate("AProperty",
                        attribute.Id, this.setup.Pattern.Id));
            attribute.IsRequired.Should().BeFalse();
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

            var element = this.setup.Pattern.Elements.Single();
            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayOutput(
                    OutputMessages.CommandLine_Output_AttributeAdded.SubstituteTemplate("AProperty",
                        element.Elements.Single().Attributes.Single().Id,
                        element.Elements.Single().Id));
        }

        [Fact]
        public void WhenUpdateAttribute_ThenUpdatesAttribute()
        {
            this.setup.RunCommand($"{CommandLineApi.CreateCommandName} pattern APattern");
            this.setup.RunCommand($"{CommandLineApi.EditCommandName} add-attribute AProperty1 --isrequired false");

            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} update-attribute AProperty1 --name AProperty2 --isrequired true --isoftype int --defaultvalueis 25");

            var attribute = this.setup.Pattern.Attributes.Single();
            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayOutput(
                    OutputMessages.CommandLine_Output_AttributeUpdated.SubstituteTemplate(attribute.Name,
                        attribute.Id, this.setup.Pattern.Id));
            attribute.Name.Should().Be("AProperty2");
            attribute.IsRequired.Should().BeTrue();
            attribute.DataType.Should().Be("int");
            attribute.DefaultValue.Should().Be("25");
        }

        [Fact]
        public void WhenDeleteAttribute_ThenDeletesAttribute()
        {
            this.setup.RunCommand($"{CommandLineApi.CreateCommandName} pattern APattern");
            this.setup.RunCommand($"{CommandLineApi.EditCommandName} add-attribute AProperty");
            var attribute = this.setup.Pattern.Attributes.Single();

            this.setup.RunCommand($"{CommandLineApi.EditCommandName} delete-attribute AProperty");

            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayOutput(
                    OutputMessages.CommandLine_Output_AttributeDeleted.SubstituteTemplate(attribute.Name,
                        attribute.Id, this.setup.Pattern.Id));
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
                .DisplayOutput(
                    OutputMessages.CommandLine_Output_ElementAdded.SubstituteTemplate("AnElement",
                        this.setup.Pattern.Elements.Single().Id, this.setup.Pattern.Id));
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

            var element = this.setup.Pattern.Elements.Single();
            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayOutput(
                    OutputMessages.CommandLine_Output_ElementAdded.SubstituteTemplate("AnElement3",
                        element.Elements.Single().Elements.Single().Id,
                        element.Elements.Single().Id));
        }

        [Fact]
        public void WhenUpdateElement_ThenUpdatesElement()
        {
            this.setup.RunCommand($"{CommandLineApi.CreateCommandName} pattern APattern");
            this.setup.RunCommand($"{CommandLineApi.EditCommandName} add-element AnElement1 --isrequired false");

            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} update-element AnElement1 --name AnElement2 --isrequired true --displayedas adisplayname --describedas adescription");

            var element = this.setup.Pattern.Elements.Single();
            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayOutput(
                    OutputMessages.CommandLine_Output_ElementUpdated.SubstituteTemplate(element.Name,
                        element.Id, this.setup.Pattern.Id));
            element.Name.Should().Be("AnElement2");
            element.DisplayName.Should().Be("adisplayname");
            element.Description.Should().Be("adescription");
            element.Cardinality.Should().Be(ElementCardinality.One);
            element.IsCollection.Should().BeFalse();
        }

        [Fact]
        public void WhenDeleteElement_ThenDeletesElement()
        {
            this.setup.RunCommand($"{CommandLineApi.CreateCommandName} pattern APattern");
            this.setup.RunCommand($"{CommandLineApi.EditCommandName} add-element AnElement");
            var element = this.setup.Pattern.Elements.Single();

            this.setup.RunCommand($"{CommandLineApi.EditCommandName} delete-element AnElement");

            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayOutput(
                    OutputMessages.CommandLine_Output_ElementDeleted.SubstituteTemplate(element.Name,
                        element.Id, this.setup.Pattern.Id));
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

            var element = this.setup.Pattern.Elements.Single();
            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayOutput(
                    OutputMessages.CommandLine_Output_CollectionAdded.SubstituteTemplate("ACollection",
                        element.Id, this.setup.Pattern.Id));
        }

        [Fact]
        public void WhenDeleteCollection_ThenDeletesCollection()
        {
            this.setup.RunCommand($"{CommandLineApi.CreateCommandName} pattern APattern");
            this.setup.RunCommand($"{CommandLineApi.EditCommandName} add-collection ACollection");
            var collection = this.setup.Pattern.Elements.Single();

            this.setup.RunCommand($"{CommandLineApi.EditCommandName} delete-collection ACollection");

            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayOutput(
                    OutputMessages.CommandLine_Output_CollectionDeleted.SubstituteTemplate(collection.Name,
                        collection.Id, this.setup.Pattern.Id));
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
            this.setup.Pattern.CodeTemplates.First().Name.Should().Be("CodeTemplate1");
        }

        [Fact]
        public void WhenAddCodeTemplateAndNamed_ThenAddsTemplate()
        {
            this.setup.RunCommand($"{CommandLineApi.CreateCommandName} pattern APattern");

            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-codetemplate \"Assets/CodeTemplates/code1.code\" --name ATemplateName");

            this.setup.Should().DisplayNoError();
            this.setup.Pattern.CodeTemplates.Should().ContainSingle(x => x.Name == "ATemplateName");
        }

        [Fact]
        public void WhenAddCodeTemplateWithCommand_ThenAddsTemplateAndCommand()
        {
            this.setup.RunCommand($"{CommandLineApi.CreateCommandName} pattern APattern");
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-codetemplate-with-command \"Assets/CodeTemplates/code1.code\" --targetpath ~/afilepath --name ATemplateName");

            var pattern = this.setup.Pattern;
            var codeTemplate = this.setup.Pattern.CodeTemplates.Single();
            var codeTemplateLocation =
                this.setup.PatternStore.GetCodeTemplateLocation(this.setup.Pattern, codeTemplate.Id, "code");
            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayOutput(
                    OutputMessages.CommandLine_Output_CodeTemplateAdded.SubstituteTemplate("ATemplateName",
                        codeTemplate.Id, pattern.Id, codeTemplate.Metadata.OriginalFilePath, codeTemplateLocation));
            this.setup.Should()
                .DisplayOutput(
                    OutputMessages.CommandLine_Output_CodeTemplateCommandAdded.SubstituteTemplate(
                        "CodeTemplateCommand1",
                        this.setup.Pattern.Automation.Single().Id, pattern.Id));
            this.setup.Pattern.CodeTemplates.Should().ContainSingle(x => x.Name == "ATemplateName");
        }

        [Fact]
        public void WhenEditCodeTemplateWithEditor_ThenEditsTemplate()
        {
            var testApplicationName = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory,
                "../../../../../tools/TestApp/TestApp.exe"));

            this.setup.RunCommand($"{CommandLineApi.CreateCommandName} pattern APattern");
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-codetemplate \"Assets/CodeTemplates/code1.code\" --name ATemplateName");
            var codeTemplate = this.setup.Pattern.CodeTemplates.First();

            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} codetemplate \"ATemplateName\" --with \"{testApplicationName}\" --args \"--opens\"");

            var codeTemplateLocation =
                this.setup.PatternStore.GetCodeTemplateLocation(this.setup.Pattern, codeTemplate.Id, "code");

            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayOutput(
                    OutputMessages.CommandLine_Output_CodeTemplateContentEdited.SubstituteTemplate(codeTemplate.Name,
                        codeTemplate.Id, this.setup.Pattern.Id, testApplicationName, codeTemplateLocation));
        }

        [Fact]
        public void WhenViewCodeTemplate_ThenViewsTemplate()
        {
            this.setup.RunCommand($"{CommandLineApi.CreateCommandName} pattern APattern");
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-codetemplate \"Assets/CodeTemplates/code1.code\" --name ATemplateName");
            var codeTemplate = this.setup.Pattern.CodeTemplates.First();

            this.setup.RunCommand(
                $"{CommandLineApi.ViewCommandName} codetemplate \"ATemplateName\"");

            var codeTemplateLocation =
                this.setup.PatternStore.GetCodeTemplateLocation(this.setup.Pattern, codeTemplate.Id, "code");
            var content =
                File.ReadAllText(Path.GetFullPath(Path.Combine(Environment.CurrentDirectory,
                    "Assets/CodeTemplates/code1.code")));

            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayOutput(
                    OutputMessages.CommandLine_Output_CodeTemplateContentViewed.SubstituteTemplate(codeTemplate.Name,
                        codeTemplate.Id, this.setup.Pattern.Id, codeTemplate.Metadata.OriginalFilePath,
                        codeTemplate.Metadata.OriginalFileExtension, codeTemplateLocation, content));
        }

        [Fact]
        public void WhenDeleteCodeTemplate_ThenDeletesCodeTemplateAndLocalFile()
        {
            this.setup.RunCommand($"{CommandLineApi.CreateCommandName} pattern APattern");
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-codetemplate \"Assets/CodeTemplates/code1.code\" --name ATemplateName");
            var codeTemplate = this.setup.Pattern.CodeTemplates.First();

            this.setup.RunCommand($"{CommandLineApi.EditCommandName} delete-codetemplate \"ATemplateName\"");

            var codeTemplateLocation =
                this.setup.PatternStore.GetCodeTemplateLocation(this.setup.Pattern, codeTemplate.Id, "code");
            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayOutput(
                    OutputMessages.CommandLine_Output_CodeTemplateDeleted.SubstituteTemplate(codeTemplate.Name,
                        codeTemplate.Id, this.setup.Pattern.Id));
            this.setup.Pattern.CodeTemplates.Should().BeEmpty();
            File.Exists(codeTemplateLocation).Should().BeFalse();
        }

        [Fact]
        public void WhenAddAndDeleteAndAddCodeTemplates_ThenAddsTemplates()
        {
            this.setup.RunCommand($"{CommandLineApi.CreateCommandName} pattern APattern");
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-codetemplate \"Assets/CodeTemplates/code1.code\"");
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-codetemplate \"Assets/CodeTemplates/code1.code\"");
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-codetemplate \"Assets/CodeTemplates/code1.code\"");
            this.setup.RunCommand($"{CommandLineApi.EditCommandName} delete-codetemplate \"CodeTemplate2\"");
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-codetemplate \"Assets/CodeTemplates/code1.code\"");

            this.setup.Should().DisplayNoError();
            var templates = this.setup.Pattern.CodeTemplates;
            templates.Count.Should().Be(3);
            templates[0].Name.Should().Be("CodeTemplate1");
            templates[1].Name.Should().Be("CodeTemplate3");
            templates[2].Name.Should().Be("CodeTemplate4");
        }

        [Fact]
        public void WhenAddCodeTemplateCommand_ThenAddsCommand()
        {
            this.setup.RunCommand($"{CommandLineApi.CreateCommandName} pattern APattern");
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-codetemplate \"Assets/CodeTemplates/code1.code\" --name ATemplateName");
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-codetemplate-command \"ATemplateName\" --targetpath ~/afilepath");

            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayOutput(
                    OutputMessages.CommandLine_Output_CodeTemplateCommandAdded.SubstituteTemplate(
                        "CodeTemplateCommand1",
                        this.setup.Pattern.Automation.Single().Id, this.setup.Pattern.Id));
        }

        [Fact]
        public void WhenUpdateCodeTemplateCommand_ThenUpdatesCommand()
        {
            this.setup.RunCommand($"{CommandLineApi.CreateCommandName} pattern APattern");
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-codetemplate \"Assets/CodeTemplates/code1.code\" --name ATemplateName");
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-codetemplate-command \"ATemplateName\" --name ACommandName --targetpath ~/afilepath");
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} update-codetemplate-command \"ACommandName\" --isoneoff true --targetpath anewpath");

            var command = this.setup.Pattern.Automation.First();
            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayOutput(
                    OutputMessages.CommandLine_Output_CodeTemplateCommandUpdated.SubstituteTemplate(command.Name,
                        command.Id, this.setup.Pattern.Id, "anewpath", true));
        }

        [Fact]
        public void WhenDeleteCodeTemplateCommand_ThenDeletesCommand()
        {
            this.setup.RunCommand($"{CommandLineApi.CreateCommandName} pattern APattern");
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-codetemplate \"Assets/CodeTemplates/code1.code\" --name ATemplateName");
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-codetemplate-command \"ATemplateName\" --name ACommandName --targetpath ~/afilepath");
            var command = this.setup.Pattern.Automation.First();
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-command-launchpoint \"*\" --name ALaunchPoint");

            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} delete-command \"ACommandName\"");

            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayOutput(
                    OutputMessages.CommandLine_Output_CommandDeleted.SubstituteTemplate(command.Name,
                        command.Id, this.setup.Pattern.Id));
            this.setup.Pattern.Automation.Should().BeEmpty();
        }

        [Fact]
        public void WhenAddCliCommand_ThenAddsCommand()
        {
            this.setup.RunCommand($"{CommandLineApi.CreateCommandName} pattern APattern");
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-cli-command \"AnApplication\" --name ACommandName");

            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayOutput(
                    OutputMessages.CommandLine_Output_CliCommandAdded.SubstituteTemplate("ACommandName",
                        this.setup.Pattern.Automation.Single().Id, this.setup.Pattern.Id));
        }

        [Fact]
        public void WhenUpdateCliCommand_ThenUpdatesCommand()
        {
            this.setup.RunCommand($"{CommandLineApi.CreateCommandName} pattern APattern");
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-cli-command \"AnApplication1\"  --arguments \"Arguments1\" --name ACommandName");
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} update-cli-command \"ACommandName\" --app \"AnApplication2\" --arguments \"Arguments2\"");

            var command = this.setup.Pattern.Automation.First();
            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayOutput(
                    OutputMessages.CommandLine_Output_CliCommandUpdated.SubstituteTemplate(command.Name,
                        command.Id, this.setup.Pattern.Id, "AnApplication2", "Arguments2"));
        }

        [Fact]
        public void WhenDeleteCliCommand_ThenDeletesCommand()
        {
            this.setup.RunCommand($"{CommandLineApi.CreateCommandName} pattern APattern");
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-cli-command \"AnApplication\" --name ACommandName");
            var command = this.setup.Pattern.Automation.First();
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-command-launchpoint \"*\" --name ALaunchPoint");

            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} delete-command \"ACommandName\"");

            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayOutput(
                    OutputMessages.CommandLine_Output_CommandDeleted.SubstituteTemplate(command.Name,
                        command.Id, this.setup.Pattern.Id));
            this.setup.Pattern.Automation.Should().BeEmpty();
        }

        [Fact]
        public void WhenAddCommandLaunchPointWithAllCommands_ThenAddsLaunchPoint()
        {
            this.setup.RunCommand($"{CommandLineApi.CreateCommandName} pattern APattern");
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-codetemplate \"Assets/CodeTemplates/code1.code\" --name ATemplateName");
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-codetemplate-command \"ATemplateName\" --targetpath ~/afilepath");
            var commandId = this.setup.Pattern.Automation.Single().Id;

            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-command-launchpoint \"*\" --name ALaunchPoint");

            var launchPoint = this.setup.Pattern.Automation.Last();
            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayOutput(
                    OutputMessages.CommandLine_Output_LaunchPointAdded.SubstituteTemplate(launchPoint.Name,
                        launchPoint.Id,
                        this.setup.Pattern.Id,
                        commandId));
        }

        [Fact]
        public void WhenAddCommandLaunchPoint_ThenAddsLaunchPoint()
        {
            this.setup.RunCommand($"{CommandLineApi.CreateCommandName} pattern APattern");
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-codetemplate \"Assets/CodeTemplates/code1.code\" --name ATemplateName");
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-codetemplate-command \"ATemplateName\" --targetpath ~/afilepath");
            var commandId = this.setup.Pattern.Automation.Single().Id;

            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-command-launchpoint {commandId} --name ALaunchPoint");

            var launchPoint = this.setup.Pattern.Automation.Last();
            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayOutput(
                    OutputMessages.CommandLine_Output_LaunchPointAdded.SubstituteTemplate(launchPoint.Name,
                        launchPoint.Id,
                        this.setup.Pattern.Id,
                        commandId));
        }

        [Fact]
        public void WhenUpdateCommandLaunchPointWithMoreCommands_ThenUpdatesLaunchPoint()
        {
            this.setup.RunCommand($"{CommandLineApi.CreateCommandName} pattern APattern");
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-codetemplate \"Assets/CodeTemplates/code1.code\" --name ATemplateName");
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-codetemplate-command \"ATemplateName\" --targetpath ~/afilepath");
            var commandId1 = this.setup.Pattern.Automation.First().Id;
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-command-launchpoint \"*\" --name ALaunchPoint");
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-codetemplate-command \"ATemplateName\" --targetpath ~/afilepath");
            var commandId2 = this.setup.Pattern.Automation.Last().Id;
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} update-command-launchpoint \"ALaunchPoint\" --add \"*\" --from \"{{APattern}}\"");

            var launchPoint = this.setup.Pattern.Automation[1];
            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayOutput(
                    OutputMessages.CommandLine_Output_LaunchPointUpdated.SubstituteTemplate(launchPoint.Name,
                        launchPoint.Id, this.setup.Pattern.Id,
                        new[] { commandId1, commandId2 }.Join(CommandLaunchPoint.CommandIdDelimiter)));
        }

        [Fact]
        public void WhenDeleteCommandLaunchPoint_ThenDeletesLaunchPoint()
        {
            this.setup.RunCommand($"{CommandLineApi.CreateCommandName} pattern APattern");
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-cli-command \"AnApplication\" --name ACommandName");
            var command = this.setup.Pattern.Automation.First();
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-command-launchpoint \"*\" --name ALaunchPoint");
            var launchPoint = this.setup.Pattern.Automation.Last();

            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} delete-command-launchpoint \"ALaunchPoint\"");

            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayOutput(
                    OutputMessages.CommandLine_Output_LaunchPointDeleted.SubstituteTemplate(launchPoint.Name,
                        launchPoint.Id, this.setup.Pattern.Id));
            this.setup.Pattern.Automation.Should().ContainSingle(x => x.Id == command.Id);
        }

        [Fact]
        public void WhenPublishAndNoCommands_ThenDisplaysError()
        {
            this.setup.RunCommand($"{CommandLineApi.PublishCommandName}");

            this.setup.Should().DisplayErrorForMissingCommand();
        }

        [Fact]
        public void WhenPublishToolkitFirstTimeWithNoChanges_ThenPublishesFirstVersionOnDesktop()
        {
            this.setup.RunCommand($"{CommandLineApi.CreateCommandName} pattern APattern");

            this.setup.RunCommand($"{CommandLineApi.PublishCommandName} toolkit");

            var exportedLocation = Path.Combine(InfrastructureConstants.GetExportDirectory(), "APattern_0.1.0.toolkit");
            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayOutput(
                    OutputMessages.CommandLine_Output_BuiltToolkit.SubstituteTemplate("APattern", "0.1.0",
                        exportedLocation));
            this.setup.Toolkit.Should().BeNull();
        }

        [Fact]
        public void WhenPublishToolkitFirstTimeWithAChange_ThenPublishesNextVersionOnDesktop()
        {
            this.setup.RunCommand($"{CommandLineApi.CreateCommandName} pattern APattern");
            this.setup.RunCommand($"{CommandLineApi.EditCommandName} add-attribute AProperty1");

            this.setup.RunCommand($"{CommandLineApi.PublishCommandName} toolkit");

            var exportedLocation = Path.Combine(InfrastructureConstants.GetExportDirectory(), "APattern_0.1.0.toolkit");
            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayOutput(
                    OutputMessages.CommandLine_Output_BuiltToolkit.SubstituteTemplate("APattern", "0.1.0",
                        exportedLocation));
            this.setup.Toolkit.Should().BeNull();
        }

        [Fact]
        public void WhenPublishToolkitNextTimeWithAPatternChange_ThenPublishesNextVersionOnDesktop()
        {
            this.setup.RunCommand($"{CommandLineApi.CreateCommandName} pattern APattern");
            this.setup.RunCommand($"{CommandLineApi.EditCommandName} add-attribute AProperty1");

            this.setup.RunCommand($"{CommandLineApi.PublishCommandName} toolkit");

            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-codetemplate \"Assets/CodeTemplates/code1.code\" --name ATemplateName");

            this.setup.RunCommand($"{CommandLineApi.PublishCommandName} toolkit");

            var exportedLocation = Path.Combine(InfrastructureConstants.GetExportDirectory(), "APattern_0.2.0.toolkit");
            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayOutput(
                    OutputMessages.CommandLine_Output_BuiltToolkit.SubstituteTemplate("APattern", "0.2.0",
                        exportedLocation));
            this.setup.Toolkit.Should().BeNull();
        }

        [Fact]
        public void WhenPublishToolkitNextTimeWithACodeTemplateChange_ThenPublishesNextVersionOnDesktop()
        {
            this.setup.RunCommand($"{CommandLineApi.CreateCommandName} pattern APattern");
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-codetemplate \"Assets/CodeTemplates/code1.code\" --name ATemplateName");
            this.setup.RunCommand($"{CommandLineApi.PublishCommandName} toolkit");
            var codeTemplate = this.setup.Pattern.CodeTemplates.Single();
            ModifyCodeTemplateContent(codeTemplate, "anewcontent");

            this.setup.RunCommand($"{CommandLineApi.PublishCommandName} toolkit");

            var exportedLocation = Path.Combine(InfrastructureConstants.GetExportDirectory(), "APattern_0.2.0.toolkit");
            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayOutput(
                    OutputMessages.CommandLine_Output_BuiltToolkit.SubstituteTemplate("APattern", "0.2.0",
                        exportedLocation));
            this.setup.Toolkit.Should().BeNull();
        }

        [Fact]
        public void WhenPublishToolkitSecondTimeWithNoChanges_ThenPublishesSameVersionOnDesktop()
        {
            this.setup.RunCommand($"{CommandLineApi.CreateCommandName} pattern APattern");
            this.setup.RunCommand($"{CommandLineApi.EditCommandName} add-attribute AProperty1");
            this.setup.RunCommand($"{CommandLineApi.PublishCommandName} toolkit");

            this.setup.RunCommand($"{CommandLineApi.PublishCommandName} toolkit");

            var exportedLocation = Path.Combine(InfrastructureConstants.GetExportDirectory(), "APattern_0.1.0.toolkit");
            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayOutput(
                    OutputMessages.CommandLine_Output_BuiltToolkit.SubstituteTemplate("APattern", "0.1.0",
                        exportedLocation));
            this.setup.Toolkit.Should().BeNull();
        }

        [Fact]
        public void
            WhenPublishToolkitWithNonBreakingChangesAndSpecifySameVersion_ThenPublishesSameVersionOnDesktopAndWarns()
        {
            this.setup.RunCommand($"{CommandLineApi.CreateCommandName} pattern APattern");
            this.setup.RunCommand($"{CommandLineApi.EditCommandName} add-attribute AProperty1");

            this.setup.RunCommand($"{CommandLineApi.PublishCommandName} toolkit --asversion 0.1.0");

            var pattern = this.setup.Pattern;
            var attribute = pattern.Attributes.Single();
            var exportedLocation = Path.Combine(InfrastructureConstants.GetExportDirectory(), "APattern_0.1.0.toolkit");
            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayOutput(
                    OutputMessages.CommandLine_Output_BuiltToolkit.SubstituteTemplate("APattern", "0.1.0",
                        exportedLocation));
            this.setup.Should()
                .DisplayOutput(
                    OutputMessages.CommandLine_Output_BuiltToolkit_Warning
                        .SubstituteTemplate(DomainMessages.ToolkitVersion_Warning
                            .Substitute("0.1.0", new[]
                            {
                                VersionChanges.PatternElement_Attribute_Add
                                    .SubstituteTemplate(attribute.Id, pattern.Id)
                            }.ToBulletList())));
            this.setup.Toolkit.Should().BeNull();
        }

        [Fact]
        public void
            WhenPublishToolkitWithMultipleBreakingAndNonBreakingChangesAndForceSameVersion_ThenPublishesSameVersionOnDesktopAndWarns()
        {
            this.setup.RunCommand($"{CommandLineApi.CreateCommandName} pattern APattern");
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-attribute \"AnAttribute\" --isrequired ");
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-codetemplate \"Assets/CodeTemplates/code1.code\" --name ATemplateName");
            var pattern = this.setup.Pattern;
            var codeTemplate = pattern.CodeTemplates.First();
            var attribute = pattern.Attributes.First();
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} delete-attribute \"AnAttribute\"");

            this.setup.RunCommand($"{CommandLineApi.PublishCommandName} toolkit --asversion 0.1.0 --force");

            var exportedLocation = Path.Combine(InfrastructureConstants.GetExportDirectory(), "APattern_0.1.0.toolkit");
            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayOutput(
                    OutputMessages.CommandLine_Output_BuiltToolkit.SubstituteTemplate("APattern", "0.1.0",
                        exportedLocation));
            this.setup.Should()
                .DisplayOutput(
                    OutputMessages.CommandLine_Output_BuiltToolkit_Warning
                        .SubstituteTemplate(DomainMessages.ToolkitVersion_Forced
                            .Substitute("0.1.0", new[]
                            {
                                VersionChanges.PatternElement_Attribute_Add.SubstituteTemplate(attribute.Id,
                                    pattern.Id),
                                VersionChanges.PatternElement_CodeTemplate_Add.SubstituteTemplate(codeTemplate.Id,
                                    pattern.Id),
                                VersionChanges.PatternElement_Attribute_Delete.SubstituteTemplate(attribute.Id,
                                    pattern.Id)
                            }.ToBulletList())));
            this.setup.Toolkit.Should().BeNull();
        }

        [Fact]
        public void WhenPublishToolkitAndInstall_ThenPublishesFirstVersionOnDesktopAndInstalls()
        {
            this.setup.RunCommand($"{CommandLineApi.CreateCommandName} pattern APattern");

            this.setup.RunCommand($"{CommandLineApi.PublishCommandName} toolkit --install");

            var exportedLocation = Path.Combine(InfrastructureConstants.GetExportDirectory(), "APattern_0.1.0.toolkit");
            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayOutput(
                    OutputMessages.CommandLine_Output_BuiltToolkit.SubstituteTemplate("APattern", "0.1.0",
                        exportedLocation));
            this.setup.Should()
                .DisplayOutput(
                    OutputMessages.CommandLine_Output_InstalledToolkit.SubstituteTemplate("APattern", "0.1.0"));
            this.setup.Toolkit.Should().NotBeNull();
        }

        [Fact]
        public void WhenTestCodeTemplate_ThenDisplaysRenderedTemplate()
        {
            this.setup.RunCommand($"{CommandLineApi.CreateCommandName} pattern APattern");
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-attribute AProperty1 --defaultvalueis adefaultvalue1");
            this.setup.RunCommand($"{CommandLineApi.EditCommandName} add-element AnElement1");
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-attribute AProperty2 --aschildof {{APattern.AnElement1}} --defaultvalueis adefaultvalue2");
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-attribute AProperty3 --aschildof {{APattern.AnElement1}} --defaultvalueis adefaultvalue3");
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-codetemplate \"Assets/CodeTemplates/code2.code\" --name ATemplateName --aschildof {{APattern.AnElement1}}");

            this.setup.RunCommand(
                $"{CommandLineApi.TestCommandName} codetemplate ATemplateName --aschildof {{APattern.AnElement1}}");

            var template = this.setup.Pattern.Elements.Single().CodeTemplates.First();
            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayOutput(
                    OutputMessages.CommandLine_Output_CodeTemplateTested.SubstituteTemplate("ATemplateName",
                        template.Id,
                        $"adefaultvalue3{Environment.NewLine}" +
                        $"adefaultvalue1{Environment.NewLine}" +
                        $"adefaultvalue2{Environment.NewLine}"));
        }

        [Fact]
        public void WhenTestCodeTemplateWithImportedData_ThenDisplaysRenderedTemplate()
        {
            this.setup.RunCommand($"{CommandLineApi.CreateCommandName} pattern APattern");
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-attribute AProperty1 --defaultvalueis adefaultvalue1");
            this.setup.RunCommand($"{CommandLineApi.EditCommandName} add-element AnElement1");
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-attribute AProperty2 --aschildof {{APattern.AnElement1}} --defaultvalueis adefaultvalue2");
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-attribute AProperty3 --aschildof {{APattern.AnElement1}} --defaultvalueis adefaultvalue3");
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-codetemplate \"Assets/CodeTemplates/code2.code\" --name ATemplateName --aschildof {{APattern.AnElement1}}");
            this.setup.RunCommand(
                $"{CommandLineApi.TestCommandName} codetemplate ATemplateName --aschildof {{APattern.AnElement1}} --import-data \"Assets/TestData/data1.json\"");

            var template = this.setup.Pattern.Elements.Single().CodeTemplates.First();
            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayOutput(
                    OutputMessages.CommandLine_Output_CodeTemplateTested.SubstituteTemplate("ATemplateName",
                        template.Id,
                        $"avalue3{Environment.NewLine}" +
                        $"avalue1{Environment.NewLine}" +
                        $"avalue2{Environment.NewLine}"));
        }

        [Fact]
        public void WhenTestCodeTemplateAndExportData_ThenExportsToFile()
        {
            this.setup.RunCommand($"{CommandLineApi.CreateCommandName} pattern APattern");
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-attribute AProperty1 --defaultvalueis adefaultvalue1");
            this.setup.RunCommand($"{CommandLineApi.EditCommandName} add-element AnElement1");
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-attribute AProperty2 --aschildof {{APattern.AnElement1}} --defaultvalueis adefaultvalue2");
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-attribute AProperty3 --aschildof {{APattern.AnElement1}} --defaultvalueis adefaultvalue3");
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-codetemplate \"Assets/CodeTemplates/code2.code\" --name ATemplateName --aschildof {{APattern.AnElement1}}");

            this.setup.RunCommand(
                $"{CommandLineApi.TestCommandName} codetemplate ATemplateName --aschildof {{APattern.AnElement1}} --export-data \"exported.json\"");

            var template = this.setup.Pattern.Elements.Single().CodeTemplates.First();
            var exportedFile = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "exported.json"));
            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayOutput(
                    OutputMessages.CommandLine_Output_CodeTemplateTestExported.SubstituteTemplate("ATemplateName",
                        template.Id,
                        exportedFile));
        }

        [Fact]
        public void WhenTestCodeTemplateCommand_ThenDisplaysRenderedTemplate()
        {
            this.setup.RunCommand($"{CommandLineApi.CreateCommandName} pattern APattern");
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-attribute AProperty1 --defaultvalueis adefaultvalue1");
            this.setup.RunCommand($"{CommandLineApi.EditCommandName} add-element AnElement1");
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-attribute AProperty2 --aschildof {{APattern.AnElement1}} --defaultvalueis adefaultvalue2");
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-attribute AProperty3 --aschildof {{APattern.AnElement1}} --defaultvalueis adefaultvalue3");
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-codetemplate \"Assets/CodeTemplates/code2.code\" --name ATemplateName --aschildof {{APattern.AnElement1}}");
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-codetemplate-command \"ATemplateName\" --name ACommandName --aschildof {{APattern.AnElement1}} --targetpath \"~/{{{{Parent.AProperty1}}}}/{{{{AProperty3}}}}/afilename.anextension\"");

            this.setup.RunCommand(
                $"{CommandLineApi.TestCommandName} codetemplate-command ACommandName --aschildof {{APattern.AnElement1}}");

            var command = this.setup.Pattern.Elements.Single().Automation.First();
            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayOutput(
                    OutputMessages.CommandLine_Output_CodeTemplateCommandTested.SubstituteTemplate("ACommandName",
                        command.Id,
                        "~/adefaultvalue1/adefaultvalue3/afilename.anextension"));
        }

        [Fact]
        public void WhenTestCodeTemplateCommandWithImportedData_ThenDisplaysRenderedTemplate()
        {
            this.setup.RunCommand($"{CommandLineApi.CreateCommandName} pattern APattern");
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-attribute AProperty1 --defaultvalueis adefaultvalue1");
            this.setup.RunCommand($"{CommandLineApi.EditCommandName} add-element AnElement1");
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-attribute AProperty2 --aschildof {{APattern.AnElement1}} --defaultvalueis adefaultvalue2");
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-attribute AProperty3 --aschildof {{APattern.AnElement1}} --defaultvalueis adefaultvalue3");
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-codetemplate \"Assets/CodeTemplates/code2.code\" --name ATemplateName --aschildof {{APattern.AnElement1}}");
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-codetemplate-command \"ATemplateName\" --name ACommandName --aschildof {{APattern.AnElement1}} --targetpath \"~/{{{{Parent.AProperty1}}}}/{{{{AProperty3}}}}/afilename.anextension\"");

            this.setup.RunCommand(
                $"{CommandLineApi.TestCommandName} codetemplate-command ACommandName --aschildof {{APattern.AnElement1}} --import-data \"Assets/TestData/data1.json\"");

            var command = this.setup.Pattern.Elements.Single().Automation.First();
            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayOutput(
                    OutputMessages.CommandLine_Output_CodeTemplateCommandTested.SubstituteTemplate("ACommandName",
                        command.Id,
                        "~/avalue1/avalue3/afilename.anextension"));
        }

        [Fact]
        public void WhenTestCodeTemplateCommandAndExportData_ThenExportsToFile()
        {
            this.setup.RunCommand($"{CommandLineApi.CreateCommandName} pattern APattern");
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-attribute AProperty1 --defaultvalueis adefaultvalue1");
            this.setup.RunCommand($"{CommandLineApi.EditCommandName} add-element AnElement1");
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-attribute AProperty2 --aschildof {{APattern.AnElement1}} --defaultvalueis adefaultvalue2");
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-attribute AProperty3 --aschildof {{APattern.AnElement1}} --defaultvalueis adefaultvalue3");
            this.setup.RunCommand(
                $"{CommandLineApi.EditCommandName} add-codetemplate \"Assets/CodeTemplates/code2.code\" --name ATemplateName --aschildof {{APattern.AnElement1}}");

            this.setup.RunCommand(
                $"{CommandLineApi.TestCommandName} codetemplate ATemplateName --aschildof {{APattern.AnElement1}} --export-data \"exported.json\"");

            var template = this.setup.Pattern.Elements.Single().CodeTemplates.First();
            var exportedFile = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "exported.json"));
            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayOutput(
                    OutputMessages.CommandLine_Output_CodeTemplateTestExported.SubstituteTemplate("ATemplateName",
                        template.Id,
                        exportedFile));
        }

        public void Dispose()
        {
            this.setup.Reset();
        }

        private void ModifyCodeTemplateContent(CodeTemplate codeTemplate, string content)
        {
            var codeTemplateLocation =
                this.setup.PatternStore.GetCodeTemplateLocation(this.setup.Pattern, codeTemplate.Id, "code");
            File.WriteAllText(codeTemplateLocation, content);
        }
    }
}