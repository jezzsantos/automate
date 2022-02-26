using System.Collections.Generic;
using Automate.CLI.Domain;
using FluentAssertions;
using Xunit;

namespace CLI.UnitTests.Domain
{
    [Trait("Category", "Unit")]
    public class PatternDefinitionSpec
    {
        private readonly PatternDefinition pattern;

        public PatternDefinitionSpec()
        {
            this.pattern = new PatternDefinition("aname");
        }

        [Fact]
        public void WhenConstructed_ThenAssigned()
        {
            this.pattern.Name.Should().Be("aname");
            this.pattern.Id.Should().NotBeEmpty();
        }

        [Fact]
        public void WhenGetAllCodeTemplatesAndNone_ThenReturnsNull()
        {
            var result = this.pattern.GetAllCodeTemplates();

            result.Should().BeEmpty();
        }

        [Fact]
        public void WhenGetAllCodeTemplatesAndNotExistsOnDescendant_ThenReturnsNull()
        {
            var element1 = new Element("anelementname1");
            var element2 = new Element("anelementname2");
            element1.Elements.Add(element2);
            this.pattern.Elements.Add(element1);

            var result = this.pattern.GetAllCodeTemplates();

            result.Should().BeEmpty();
        }

        [Fact]
        public void WhenGetAllCodeTemplatesAndFoundOnPattern_ThenReturnsAutomation()
        {
            var template = new CodeTemplate("aname", "afullpath", "anextension");
            this.pattern.CodeTemplates.Add(template);

            var result = this.pattern.GetAllCodeTemplates();

            result.Should().ContainSingle(tem => tem.Id == template.Id);
        }

        [Fact]
        public void WhenGetAllCodeTemplatesAndFoundOnDescendantElement_ThenReturnsAutomation()
        {
            var template1 = new CodeTemplate("aname1", "afullpath", "anextension");
            var template2 = new CodeTemplate("aname2", "afullpath", "anextension");
            var element1 = new Element("anelementname1");
            var element2 = new Element("anelementname2");
            element2.CodeTemplates.Add(template1);
            element1.Elements.Add(element2);
            this.pattern.Elements.Add(element1);
            this.pattern.CodeTemplates.Add(template2);

            var result = this.pattern.GetAllCodeTemplates();

            result.Should().Contain(tem => tem.Id == template1.Id);
            result.Should().Contain(tem => tem.Id == template2.Id);
        }

        [Fact]
        public void WhenFindAutomationAndNone_ThenReturnsNull()
        {
            var result = this.pattern.FindAutomation("acmdid");

            result.Should().BeNull();
        }

        [Fact]
        public void WhenFindAutomationAndNotExistsOnDescendant_ThenReturnsNull()
        {
            var element1 = new Element("anelementname1");
            var element2 = new Element("anelementname2");
            element1.Elements.Add(element2);
            this.pattern.Elements.Add(element1);

            var result = this.pattern.FindAutomation("acmdid");

            result.Should().BeNull();
        }

        [Fact]
        public void WhenFindAutomationAndFoundOnPattern_ThenReturnsAutomation()
        {
            var automation = new TestAutomation("acmdid");
            this.pattern.Automation.Add(automation);

            var result = this.pattern.FindAutomation("acmdid");

            result.Should().Be(automation);
        }

        [Fact]
        public void WhenFindAutomationAndFoundOnDescendantElement_ThenReturnsAutomation()
        {
            var automation = new TestAutomation("acmdid");
            var element1 = new Element("anelementname1");
            var element2 = new Element("anelementname2");
            element2.Automation.Add(automation);
            element1.Elements.Add(element2);
            this.pattern.Elements.Add(element1);

            var result = this.pattern.FindAutomation("acmdid");

            result.Should().Be(automation);
        }
    }

    internal class TestAutomation : IAutomation
    {
        public TestAutomation(string id)
        {
            Id = id;
        }

        public string Id { get; set; }

        public string Name { get; set; } = "anautomationname";

        public CommandExecutionResult Execute(SolutionDefinition solution, SolutionItem target)
        {
            return new CommandExecutionResult("anautomationname", new List<string> { "alogentry" });
        }
    }
}