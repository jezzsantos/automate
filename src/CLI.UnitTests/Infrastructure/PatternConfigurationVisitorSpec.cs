using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using Automate.Authoring.Domain;
using Automate.CLI.Infrastructure;
using FluentAssertions;
using Xunit;
#if TESTINGONLY
using Automate.Common.Domain;
#endif

namespace CLI.UnitTests.Infrastructure
{
    public class PatternConfigurationVisitorSpec
    {
        private static void ConfigurePattern(PatternDefinition pattern)
        {
            pattern.AddCodeTemplate("acodetemplate1", "afilepath1", "anextension1");
            var command1 =
                pattern.AddCodeTemplateCommand("acodetemplatecommand1", "acodetemplate1", true, "atargetpath1");
            var command2 = pattern.AddCliCommand("aclicommand1", "anapplication1", "arguments1");
            pattern.AddCommandLaunchPoint("acommandlaunchpoint1", new List<string> { command1.Id, command2.Id },
                pattern);
            pattern.AddAttribute("anattributename1", null, true, "adefaultvalue1");
            pattern.AddAttribute("anattributename2", null, false, "achoice2",
                new List<string> { "achoice1", "achoice2", "achoice3" });
            var element1 = pattern.AddElement("anelementname1", ElementCardinality.One, true, "adisplayname1",
                "adescription1");
            element1.AddCodeTemplate("acodetemplate2", "afilepath2", "anextension2");
            var command3 =
                element1.AddCodeTemplateCommand("acodetemplatecommand2", "acodetemplate2", true, "atargetpath2");
            element1.AddCommandLaunchPoint("acommandlaunchpoint2", new List<string> { command3.Id }, element1);
            var collection = pattern.AddElement("acollectionname1", ElementCardinality.ZeroOrMany, true,
                "adisplayname2", "adescription2");
            collection.AddCodeTemplate("acodetemplate3", "afilepath3", "anextension3");
            var element2 = element1.AddElement("anelementname2", ElementCardinality.One, true, "adisplayname2",
                "adescription2");
            element2.AddAttribute("anattributename3", null, true, "adefaultvalue3");
            var command4 =
                collection.AddCodeTemplateCommand("acodetemplatecommand3", "acodetemplate3", true, "atargetpath3");
            collection.AddCommandLaunchPoint("acommandlaunchpoint3", new List<string> { command4.Id }, collection);
        }

        [Trait("Category", "Unit")]
        public class GivenSimpleText
        {
            private readonly PatternConfigurationVisitor visitor;

            public GivenSimpleText()
            {
                this.visitor = new PatternConfigurationVisitor(OutputFormat.Text, VisitorConfigurationOptions.Simple);
                var pattern = new PatternDefinition("apatternname");
                ConfigurePattern(pattern);
                pattern.TraverseDescendants(this.visitor);
            }

            [Fact]
            public void WhenToString_ThenReturnsText()
            {
                var result = this.visitor.ToOutput();

                result.Should().Be(
                    $"- apatternname (root element) (attached with 1 code templates){Environment.NewLine}" +
                    $"\t- anattributename1 (attribute) (string, required, default: adefaultvalue1){Environment.NewLine}" +
                    $"\t- anattributename2 (attribute) (string, oneof: achoice1;achoice2;achoice3, default: achoice2){Environment.NewLine}" +
                    $"\t- anelementname1 (element) (attached with 1 code templates){Environment.NewLine}" +
                    $"\t\t- anelementname2 (element){Environment.NewLine}" +
                    $"\t\t\t- anattributename3 (attribute) (string, required, default: adefaultvalue3){Environment.NewLine}" +
                    $"\t- acollectionname1 (collection) (attached with 1 code templates){Environment.NewLine}"
                );
            }
        }

        [Trait("Category", "Unit")]
        public class GivenDetailedText
        {
            private readonly PatternDefinition pattern;
            private readonly PatternConfigurationVisitor visitor;

            public GivenDetailedText()
            {
                this.visitor = new PatternConfigurationVisitor(OutputFormat.Text, VisitorConfigurationOptions.Detailed);
                this.pattern = new PatternDefinition("apatternname");
                ConfigurePattern(this.pattern);
                this.pattern.TraverseDescendants(this.visitor);
            }

            [Fact]
            public void WhenToString_ThenReturnsText()
            {
                var result = this.visitor.ToOutput();

                var codeTemplate1 = this.pattern.CodeTemplates.First();
                var codeTemplate2 = this.pattern.Elements[0].CodeTemplates.First();
                var codeTemplate3 = this.pattern.Elements[1].CodeTemplates.First();
                var codeTemplateCommand1 = this.pattern.Automation[0];
                var cliCommand1 = this.pattern.Automation[1];
                var codeTemplateCommand2 = this.pattern.Elements[0].Automation[0];
                var codeTemplateCommand3 = this.pattern.Elements[1].Automation[0];
                var launchPoint1 = this.pattern.Automation[2];
                var launchPoint2 = this.pattern.Elements[0].Automation[1];
                var launchPoint3 = this.pattern.Elements[1].Automation[1];
                var element1 = this.pattern.Elements[0];
                var collection1 = this.pattern.Elements[1];
                var element2 = this.pattern.Elements[0].Elements.First();
                result.Should().Be($"- apatternname [{this.pattern.Id}] (root element){Environment.NewLine}" +
                                   $"\t- CodeTemplates:{Environment.NewLine}" +
                                   $"\t\t- acodetemplate1 [{codeTemplate1.Id}] (original: afilepath1){Environment.NewLine}" +
                                   $"\t- Automation:{Environment.NewLine}" +
                                   $"\t\t- acodetemplatecommand1 [{codeTemplateCommand1.Id}] (CodeTemplateCommand) (template: {codeTemplate1.Id}, onceonly, path: atargetpath1){Environment.NewLine}" +
                                   $"\t\t- aclicommand1 [{cliCommand1.Id}] (CliCommand) (app: anapplication1, args: arguments1){Environment.NewLine}" +
                                   $"\t\t- acommandlaunchpoint1 [{launchPoint1.Id}] (CommandLaunchPoint) (ids: {codeTemplateCommand1.Id};{cliCommand1.Id}){Environment.NewLine}" +
                                   $"\t- Attributes:{Environment.NewLine}" +
                                   $"\t\t- anattributename1 (string, required, default: adefaultvalue1){Environment.NewLine}" +
                                   $"\t\t- anattributename2 (string, oneof: achoice1;achoice2;achoice3, default: achoice2){Environment.NewLine}" +
                                   $"\t- Elements:{Environment.NewLine}" +
                                   $"\t\t- anelementname1 [{element1.Id}] (element){Environment.NewLine}" +
                                   $"\t\t\t- CodeTemplates:{Environment.NewLine}" +
                                   $"\t\t\t\t- acodetemplate2 [{codeTemplate2.Id}] (original: afilepath2){Environment.NewLine}" +
                                   $"\t\t\t- Automation:{Environment.NewLine}" +
                                   $"\t\t\t\t- acodetemplatecommand2 [{codeTemplateCommand2.Id}] (CodeTemplateCommand) (template: {codeTemplate2.Id}, onceonly, path: atargetpath2){Environment.NewLine}" +
                                   $"\t\t\t\t- acommandlaunchpoint2 [{launchPoint2.Id}] (CommandLaunchPoint) (ids: {codeTemplateCommand2.Id}){Environment.NewLine}" +
                                   $"\t\t\t- Elements:{Environment.NewLine}" +
                                   $"\t\t\t\t- anelementname2 [{element2.Id}] (element){Environment.NewLine}" +
                                   $"\t\t\t\t\t- Attributes:{Environment.NewLine}" +
                                   $"\t\t\t\t\t\t- anattributename3 (string, required, default: adefaultvalue3){Environment.NewLine}" +
                                   $"\t\t- acollectionname1 [{collection1.Id}] (collection){Environment.NewLine}" +
                                   $"\t\t\t- CodeTemplates:{Environment.NewLine}" +
                                   $"\t\t\t\t- acodetemplate3 [{codeTemplate3.Id}] (original: afilepath3){Environment.NewLine}" +
                                   $"\t\t\t- Automation:{Environment.NewLine}" +
                                   $"\t\t\t\t- acodetemplatecommand3 [{codeTemplateCommand3.Id}] (CodeTemplateCommand) (template: {codeTemplate3.Id}, onceonly, path: atargetpath3){Environment.NewLine}" +
                                   $"\t\t\t\t- acommandlaunchpoint3 [{launchPoint3.Id}] (CommandLaunchPoint) (ids: {codeTemplateCommand3.Id}){Environment.NewLine}"
                );
            }
        }

        [Trait("Category", "Unit")]
        public class GivenLaunchPointsText
        {
            private readonly PatternDefinition pattern;
            private readonly PatternConfigurationVisitor visitor;

            public GivenLaunchPointsText()
            {
                this.visitor =
                    new PatternConfigurationVisitor(OutputFormat.Text, VisitorConfigurationOptions.OnlyLaunchPoints);
                this.pattern = new PatternDefinition("apatternname");
                ConfigurePattern(this.pattern);
                this.pattern.TraverseDescendants(this.visitor);
            }

            [Fact]
            public void WhenToString_ThenReturnsText()
            {
                var result = this.visitor.ToOutput();

                var launchPoint1 = this.pattern.Automation[2];
                var launchPoint2 = this.pattern.Elements[0].Automation[1];
                var launchPoint3 = this.pattern.Elements[1].Automation[1];

                result.Should().Be($"- apatternname (root element){Environment.NewLine}" +
                                   $"\t- LaunchPoints:{Environment.NewLine}" +
                                   $"\t\t- acommandlaunchpoint1 [{launchPoint1.Id}] (CommandLaunchPoint){Environment.NewLine}" +
                                   $"\t- Elements:{Environment.NewLine}" +
                                   $"\t\t- anelementname1 (element){Environment.NewLine}" +
                                   $"\t\t\t- LaunchPoints:{Environment.NewLine}" +
                                   $"\t\t\t\t- acommandlaunchpoint2 [{launchPoint2.Id}] (CommandLaunchPoint){Environment.NewLine}" +
                                   $"\t\t- acollectionname1 (collection){Environment.NewLine}" +
                                   $"\t\t\t- LaunchPoints:{Environment.NewLine}" +
                                   $"\t\t\t\t- acommandlaunchpoint3 [{launchPoint3.Id}] (CommandLaunchPoint){Environment.NewLine}"
                );
            }
        }

        [Trait("Category", "Unit")]
        public class GivenSimpleOrDetailedJson
        {
            private readonly PatternDefinition pattern;
            private readonly PatternConfigurationVisitor visitor;

            public GivenSimpleOrDetailedJson()
            {
                this.visitor = new PatternConfigurationVisitor(OutputFormat.Json, VisitorConfigurationOptions.Simple);
                this.pattern = new PatternDefinition("apatternname");
                ConfigurePattern(this.pattern);
                this.pattern.TraverseDescendants(this.visitor);
            }

            [Fact]
            public void WhenToString_ThenReturnsJson()
            {
                var result = this.visitor.ToOutput();

                var codeTemplate1 = this.pattern.CodeTemplates.First();
                var codeTemplate2 = this.pattern.Elements[0].CodeTemplates.First();
                var codeTemplate3 = this.pattern.Elements[1].CodeTemplates.First();
                var codeTemplateCommand1 = this.pattern.Automation[0];
                var cliCommand1 = this.pattern.Automation[1];
                var codeTemplateCommand2 = this.pattern.Elements[0].Automation[0];
                var codeTemplateCommand3 = this.pattern.Elements[1].Automation[0];
                var launchPoint1 = this.pattern.Automation[2];
                var launchPoint2 = this.pattern.Elements[0].Automation[1];
                var launchPoint3 = this.pattern.Elements[1].Automation[1];
                var attribute1 = this.pattern.Attributes.First();
                var attribute2 = this.pattern.Attributes[1];
                var attribute3 = this.pattern.Elements[0].Elements[0].Attributes[0];
                var element1 = this.pattern.Elements[0];
                var collection1 = this.pattern.Elements[1];
                var element2 = element1.Elements.Single();
                result.As<JsonNode>().ToJsonString().Should()
                    .Be("{" +
                        $"\"Id\":\"{this.pattern.Id}\",\"EditPath\":\"{this.pattern.EditPath}\",\"Name\":\"{this.pattern.Name}\",\"DisplayName\":\"{this.pattern.DisplayName}\",\"Description\":\"{this.pattern.Description}\"," +
                        "\"CodeTemplates\":[" +
                        $"{{\"Id\":\"{codeTemplate1.Id}\",\"Name\":\"acodetemplate1\",\"OriginalFilePath\":\"afilepath1\",\"OriginalFileExtension\":\"anextension1\"}}" +
                        "]," +
                        "\"Automation\":[" +
                        $"{{\"Id\":\"{codeTemplateCommand1.Id}\",\"Name\":\"acodetemplatecommand1\",\"Type\":\"CodeTemplateCommand\",\"TemplateId\":\"{codeTemplate1.Id}\",\"IsOneOff\":true,\"TargetPath\":\"atargetpath1\"}}," +
                        $"{{\"Id\":\"{cliCommand1.Id}\",\"Name\":\"aclicommand1\",\"Type\":\"CliCommand\",\"ApplicationName\":\"anapplication1\",\"Arguments\":\"arguments1\"}}," +
                        $"{{\"Id\":\"{launchPoint1.Id}\",\"Name\":\"acommandlaunchpoint1\",\"Type\":\"CommandLaunchPoint\",\"CommandIds\":[\"{codeTemplateCommand1.Id}\",\"{cliCommand1.Id}\"]}}" +
                        "]," +
                        "\"Attributes\":[" +
                        $"{{\"Id\":\"{attribute1.Id}\",\"Name\":\"anattributename1\",\"DataType\":\"string\",\"IsRequired\":true,\"Choices\":[],\"DefaultValue\":\"adefaultvalue1\"}}," +
                        $"{{\"Id\":\"{attribute2.Id}\",\"Name\":\"anattributename2\",\"DataType\":\"string\",\"IsRequired\":false,\"Choices\":[\"achoice1\",\"achoice2\",\"achoice3\"],\"DefaultValue\":\"achoice2\"}}" +
                        "]," +
                        "\"Elements\":[" +
                        $"{{\"Id\":\"{element1.Id}\",\"EditPath\":\"{element1.EditPath}\",\"Name\":\"{element1.Name}\",\"DisplayName\":\"{element1.DisplayName}\",\"Description\":\"{element1.Description}\",\"AutoCreate\":true,\"IsCollection\":false,\"Cardinality\":\"One\"," +
                        "\"CodeTemplates\":[" +
                        $"{{\"Id\":\"{codeTemplate2.Id}\",\"Name\":\"acodetemplate2\",\"OriginalFilePath\":\"afilepath2\",\"OriginalFileExtension\":\"anextension2\"}}" +
                        "]," +
                        "\"Automation\":[" +
                        $"{{\"Id\":\"{codeTemplateCommand2.Id}\",\"Name\":\"acodetemplatecommand2\",\"Type\":\"CodeTemplateCommand\",\"TemplateId\":\"{codeTemplate2.Id}\",\"IsOneOff\":true,\"TargetPath\":\"atargetpath2\"}}," +
                        $"{{\"Id\":\"{launchPoint2.Id}\",\"Name\":\"acommandlaunchpoint2\",\"Type\":\"CommandLaunchPoint\",\"CommandIds\":[\"{codeTemplateCommand2.Id}\"]}}" +
                        "]," +
                        "\"Attributes\":[]," +
                        "\"Elements\":[" +
                        $"{{\"Id\":\"{element2.Id}\",\"EditPath\":\"{element2.EditPath}\",\"Name\":\"{element2.Name}\",\"DisplayName\":\"{element2.DisplayName}\",\"Description\":\"{element2.Description}\",\"AutoCreate\":true,\"IsCollection\":false,\"Cardinality\":\"One\"," +
                        "\"CodeTemplates\":[]," +
                        "\"Automation\":[]," +
                        "\"Attributes\":[" +
                        $"{{\"Id\":\"{attribute3.Id}\",\"Name\":\"anattributename3\",\"DataType\":\"string\",\"IsRequired\":true,\"Choices\":[],\"DefaultValue\":\"adefaultvalue3\"}}" +
                        "]," +
                        "\"Elements\":[]" +
                        "}" +
                        "]" +
                        "}," +
                        $"{{\"Id\":\"{collection1.Id}\",\"EditPath\":\"{collection1.EditPath}\",\"Name\":\"{collection1.Name}\",\"DisplayName\":\"{collection1.DisplayName}\",\"Description\":\"{collection1.Description}\",\"AutoCreate\":true,\"IsCollection\":true,\"Cardinality\":\"ZeroOrMany\"," +
                        "\"CodeTemplates\":[" +
                        $"{{\"Id\":\"{codeTemplate3.Id}\",\"Name\":\"acodetemplate3\",\"OriginalFilePath\":\"afilepath3\",\"OriginalFileExtension\":\"anextension3\"}}" +
                        "]," +
                        "\"Automation\":[" +
                        $"{{\"Id\":\"{codeTemplateCommand3.Id}\",\"Name\":\"acodetemplatecommand3\",\"Type\":\"CodeTemplateCommand\",\"TemplateId\":\"{codeTemplate3.Id}\",\"IsOneOff\":true,\"TargetPath\":\"atargetpath3\"}}," +
                        $"{{\"Id\":\"{launchPoint3.Id}\",\"Name\":\"acommandlaunchpoint3\",\"Type\":\"CommandLaunchPoint\",\"CommandIds\":[\"{codeTemplateCommand3.Id}\"]}}" +
                        "]," +
                        "\"Attributes\":[]," +
                        "\"Elements\":[]" +
                        "}" +
                        "]" +
                        "}");
            }
        }

        [Trait("Category", "Unit")]
        public class GivenLaunchPointJson
        {
            private readonly PatternDefinition pattern;
            private readonly PatternConfigurationVisitor visitor;

            public GivenLaunchPointJson()
            {
                this.visitor =
                    new PatternConfigurationVisitor(OutputFormat.Json, VisitorConfigurationOptions.OnlyLaunchPoints);
                this.pattern = new PatternDefinition("apatternname");
                ConfigurePattern(this.pattern);
                this.pattern.TraverseDescendants(this.visitor);
            }

            [Fact]
            public void WhenToString_ThenReturnsJson()
            {
                var result = this.visitor.ToOutput();

                var launchPoint1 = this.pattern.Automation[2];
                var launchPoint2 = this.pattern.Elements[0].Automation[1];
                var launchPoint3 = this.pattern.Elements[1].Automation[1];
                var element1 = this.pattern.Elements[0];
                var element2 = this.pattern.Elements[1];
                result.As<JsonNode>().ToJsonString().Should()
                    .Be("{" +
                        $"\"Id\":\"{this.pattern.Id}\",\"EditPath\":\"{this.pattern.EditPath}\",\"Name\":\"{this.pattern.Name}\",\"DisplayName\":\"{this.pattern.DisplayName}\",\"Description\":\"{this.pattern.Description}\"," +
                        "\"LaunchPoints\":[" +
                        $"{{\"Id\":\"{launchPoint1.Id}\",\"Name\":\"acommandlaunchpoint1\",\"Type\":\"CommandLaunchPoint\"}}" +
                        "]," +
                        "\"Elements\":[" +
                        $"{{\"Id\":\"{element1.Id}\",\"EditPath\":\"{element1.EditPath}\",\"Name\":\"{element1.Name}\",\"DisplayName\":\"{element1.DisplayName}\",\"Description\":\"{element1.Description}\"," +
                        "\"LaunchPoints\":[" +
                        $"{{\"Id\":\"{launchPoint2.Id}\",\"Name\":\"acommandlaunchpoint2\",\"Type\":\"CommandLaunchPoint\"}}" +
                        "]" +
                        "}," +
                        $"{{\"Id\":\"{element2.Id}\",\"EditPath\":\"{element2.EditPath}\",\"Name\":\"{element2.Name}\",\"DisplayName\":\"{element2.DisplayName}\",\"Description\":\"{element2.Description}\"," +
                        "\"LaunchPoints\":[" +
                        $"{{\"Id\":\"{launchPoint3.Id}\",\"Name\":\"acommandlaunchpoint3\",\"Type\":\"CommandLaunchPoint\"}}" +
                        "]" +
                        "}" +
                        "]" +
                        "}");
            }
        }

        [Trait("Category", "Unit")]
        public class VisitorExtensionsSpec
        {
            [Fact]
            public void WhenHasAnyDescendantLaunchPointsAndNone_ThenReturnsFalse()
            {
                var result = new List<Element>()
                    .HasAnyDescendantLaunchPoints();

                result.Should().BeFalse();
            }
#if TESTINGONLY
            [Fact]
            public void WhenHasAnyDescendantLaunchPointsAndTopLevel_ThenReturnsTrue()
            {
                var element = new Element("anelementname1");
                element.AddAutomation(new Automation("alaunchpoint1", AutomationType.TestingOnlyLaunching,
                    new Dictionary<string, object>()));

                var result = new List<Element> { element }
                    .HasAnyDescendantLaunchPoints();

                result.Should().BeTrue();
            }

            [Fact]
            public void WhenHasAnyDescendantLaunchPointsAndNested_ThenReturnsTrue()
            {
                var element1 = new Element("anelementname1");
                var element2 = new Element("anelementname2");
                var element3 = new Element("anelementname3");
                element3.AddAutomation(new Automation("alaunchpoint1", AutomationType.TestingOnlyLaunching,
                    new Dictionary<string, object>()));
                element1.AddElement(element2);
                element2.AddElement(element3);

                var result = new List<Element> { element1 }
                    .HasAnyDescendantLaunchPoints();

                result.Should().BeTrue();
            }
#endif
        }
    }
}