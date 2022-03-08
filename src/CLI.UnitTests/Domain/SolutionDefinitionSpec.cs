using Automate.CLI.Domain;
using FluentAssertions;
using Xunit;

namespace CLI.UnitTests.Domain
{
    [Trait("Category", "Unit")]
    public class SolutionDefinitionSpec
    {
        [Fact]
        public void WhenConstructed_ThenInitialisesTopLevelElementsAndCollectionsOnly()
        {
            var pattern = new PatternDefinition("apatternname");
            var element3 = new Element("anelementname3");
            element3.Attributes.Add(new Attribute("anattributename3", "string", false, "adefaultvalue3"));
            var collection3 = new Element("acollectionname3", isCollection: true);
            var element2 = new Element("anelementname2");
            element2.Attributes.Add(new Attribute("anattributename2", "string", false, "adefaultvalue2"));
            var collection2 = new Element("acollectionname2", isCollection: true);
            var element1 = new Element("anelementname1");
            element1.Attributes.Add(new Attribute("anattributename1", "string", false, "adefaultvalue1"));
            var collection1 = new Element("acollectionname1", isCollection: true);
            element2.Elements.Add(element3);
            element2.Elements.Add(collection3);
            pattern.Elements.Add(element1);
            pattern.Elements.Add(collection1);
            pattern.Elements.Add(element2);
            pattern.Elements.Add(collection2);
            var solution = new SolutionDefinition(new ToolkitDefinition(pattern, "1.0"));

            solution.Name.Should().Match("apatternname???");
            solution.Model.Should().NotBeNull();
            var solutionElement1 = solution.Model.Properties["anelementname1"];
            solutionElement1.ElementSchema.Should().Be(element1);
            solutionElement1.Value.Should().BeNull();
            solutionElement1.IsMaterialised.Should().BeFalse();
            solutionElement1.Items.Should().BeNull();
            solutionElement1.Properties.Should().BeNull();

            var solutionCollection1 = solution.Model.Properties["acollectionname1"];
            solutionCollection1.ElementSchema.Should().Be(collection1);
            solutionCollection1.IsMaterialised.Should().BeFalse();
            solutionCollection1.Items.Should().BeNull();
            solutionCollection1.Properties.Should().BeNull();

            var solutionElement2 = solution.Model.Properties["anelementname2"];
            solutionElement2.ElementSchema.Should().Be(element2);
            solutionElement2.Value.Should().BeNull();
            solutionElement2.IsMaterialised.Should().BeFalse();
            solutionElement2.Items.Should().BeNull();
            solutionElement2.Properties.Should().BeNull();

            var solutionCollection2 = solution.Model.Properties["acollectionname2"];
            solutionCollection2.ElementSchema.Should().Be(collection2);
            solutionCollection2.IsMaterialised.Should().BeFalse();
            solutionCollection2.Items.Should().BeNull();
            solutionCollection2.Properties.Should().BeNull();
        }

        [Fact]
        public void WhenFindByAutomationAndNotExist_ThenReturnsNull()
        {
            var pattern = new PatternDefinition("apatternname");
            var solution = new SolutionDefinition(new ToolkitDefinition(pattern, "1.0"));

            var (automation, solutionItem) = solution.FindByAutomation("anautomationid");

            automation.Should().BeNull();
            solutionItem.Should().BeNull();
        }

        [Fact]
        public void WhenFindByAutomationOnPattern_ThenReturnsAutomation()
        {
            var pattern = new PatternDefinition("apatternname");
            var automation1 = new TestAutomation("anautomationid");
            pattern.Automation.Add(automation1);
            var solution = new SolutionDefinition(new ToolkitDefinition(pattern, "1.0"));

            var (automation, solutionItem) = solution.FindByAutomation("anautomationid");

            automation.Should().Be(automation1);
            solutionItem.Should().Be(solution.Model);
        }

        [Fact]
        public void WhenFindByAutomationOnDescendantElement_ThenReturnsAutomation()
        {
            var pattern = new PatternDefinition("apatternname");
            var automation1 = new TestAutomation("anautomationid");
            var element3 = new Element("anelementname3");
            element3.Automation.Add(automation1);
            var element2 = new Element("anelementname2");
            element2.Elements.Add(element3);
            var element1 = new Element("anelementname1");
            element1.Elements.Add(element2);
            pattern.Elements.Add(element1);
            var solution = new SolutionDefinition(new ToolkitDefinition(pattern, "1.0"));
            solution.Model.Properties["anelementname1"].Materialise();
            solution.Model.Properties["anelementname1"].Properties["anelementname2"].Materialise();
            solution.Model.Properties["anelementname1"].Properties["anelementname2"].Properties["anelementname3"].Materialise();

            var (automation, solutionItem) = solution.FindByAutomation("anautomationid");

            automation.Should().Be(automation1);
            solutionItem.Should().Be(solution.Model.Properties["anelementname1"].Properties["anelementname2"].Properties["anelementname3"]);
        }

        [Fact]
        public void WhenFindByAutomationOnDescendantCollectionItem_ThenReturnsAutomation()
        {
            var pattern = new PatternDefinition("apatternname");
            var automation1 = new TestAutomation("anautomationid");
            var collection1 = new Element("acollectionname1", isCollection: true);
            collection1.Automation.Add(automation1);
            var element2 = new Element("anelementname2");
            element2.Elements.Add(collection1);
            var element1 = new Element("anelementname1");
            element1.Elements.Add(element2);
            pattern.Elements.Add(element1);
            var solution = new SolutionDefinition(new ToolkitDefinition(pattern, "1.0"));
            solution.Model.Properties["anelementname1"].Materialise();
            solution.Model.Properties["anelementname1"].Properties["anelementname2"].Materialise();
            solution.Model.Properties["anelementname1"].Properties["anelementname2"].Properties["acollectionname1"].MaterialiseCollectionItem();

            var (automation, solutionItem) = solution.FindByAutomation("anautomationid");

            automation.Should().Be(automation1);
            solutionItem.Should().Be(solution.Model.Properties["anelementname1"].Properties["anelementname2"].Properties["acollectionname1"]);
        }

        [Fact]
        public void WhenFindByAutomationOnDescendantCollectionItemElement_ThenReturnsAutomation()
        {
            var pattern = new PatternDefinition("apatternname");
            var collection1 = new Element("acollectionname1", isCollection: true);
            var automation1 = new TestAutomation("anautomationid");
            var element1 = new Element("anelementname1");
            element1.Automation.Add(automation1);
            collection1.Elements.Add(element1);
            pattern.Elements.Add(collection1);

            var solution = new SolutionDefinition(new ToolkitDefinition(pattern, "1.0"));
            solution.Model.Properties["acollectionname1"].MaterialiseCollectionItem();
            solution.Model.Properties["acollectionname1"].Items[0].Properties["anelementname1"].Materialise();

            var (automation, solutionItem) = solution.FindByAutomation("anautomationid");

            automation.Should().Be(automation1);
            solutionItem.Should().Be(solution.Model.Properties["acollectionname1"].Items[0].Properties["anelementname1"]);
        }

        [Fact]
        public void WhenFindByAutomationOnUnMaterialisedDescendantElement_ThenReturnsNull()
        {
            var pattern = new PatternDefinition("apatternname");
            var automation1 = new TestAutomation("anautomationid");
            var element3 = new Element("anelementname3");
            element3.Automation.Add(automation1);
            var element2 = new Element("anelementname2");
            element2.Elements.Add(element3);
            var element1 = new Element("anelementname1");
            element1.Elements.Add(element2);
            pattern.Elements.Add(element1);
            var solution = new SolutionDefinition(new ToolkitDefinition(pattern, "1.0"));
            solution.Model.Properties["anelementname1"].Materialise();

            var (automation, solutionItem) = solution.FindByAutomation("anautomationid");

            automation.Should().BeNull();
            solutionItem.Should().BeNull();
        }

        [Fact]
        public void WhenPopulateAncestryAfterDeserialization_ThenPopulatesParentOnDescendantCollectionItem()
        {
            var pattern = new PatternDefinition("apatternname");
            var automation1 = new TestAutomation("anautomationid");
            var collection1 = new Element("acollectionname1", isCollection: true);
            collection1.Automation.Add(automation1);
            var element2 = new Element("anelementname2");
            element2.Elements.Add(collection1);
            var element1 = new Element("anelementname1");
            element1.Elements.Add(element2);
            pattern.Elements.Add(element1);
            var solution = new SolutionDefinition(new ToolkitDefinition(pattern, "1.0"));
            solution.Model.Properties["anelementname1"].Materialise();
            solution.Model.Properties["anelementname1"].Properties["anelementname2"].Materialise();
            solution.Model.Properties["anelementname1"].Properties["anelementname2"].Properties["acollectionname1"].MaterialiseCollectionItem();

            solution.PopulateAncestryAfterDeserialization();

            solution.Model.Parent.Should().BeNull();
            solution.Model.Properties["anelementname1"].Parent.Should().Be(solution.Model);
            solution.Model.Properties["anelementname1"].Properties["anelementname2"].Parent.Should().Be(solution.Model.Properties["anelementname1"]);
            solution.Model.Properties["anelementname1"].Properties["anelementname2"].Properties["acollectionname1"].Parent.Should().Be(solution.Model.Properties["anelementname1"].Properties["anelementname2"]);
        }

        [Fact]
        public void WhenPopulateAncestryAfterDeserialization_ThenPopulatesParentOnDescendantCollectionItemElement()
        {
            var pattern = new PatternDefinition("apatternname");
            var collection1 = new Element("acollectionname1", isCollection: true);
            var automation1 = new TestAutomation("anautomationid");
            var element1 = new Element("anelementname1");
            element1.Automation.Add(automation1);
            collection1.Elements.Add(element1);
            pattern.Elements.Add(collection1);

            var solution = new SolutionDefinition(new ToolkitDefinition(pattern, "1.0"));
            solution.Model.Properties["acollectionname1"].MaterialiseCollectionItem();
            solution.Model.Properties["acollectionname1"].Items[0].Properties["anelementname1"].Materialise();

            solution.PopulateAncestryAfterDeserialization();

            solution.Model.Parent.Should().BeNull();
            solution.Model.Properties["acollectionname1"].Parent.Should().Be(solution.Model);
            solution.Model.Properties["acollectionname1"].Items[0].Parent.Should().Be(solution.Model.Properties["acollectionname1"]);
            solution.Model.Properties["acollectionname1"].Items[0].Properties["anelementname1"].Parent.Should().Be(solution.Model.Properties["acollectionname1"].Items[0]);
        }
    }
}