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
        public void WhenFindByAutomationAndNotExist_ThenReturnsEmpty()
        {
            var pattern = new PatternDefinition("apatternname");
            var solution = new SolutionDefinition(new ToolkitDefinition(pattern, "1.0"));

            var pairs = solution.FindByAutomation("anautomationid");

            pairs.Should().BeEmpty();
        }

        [Fact]
        public void WhenFindByAutomationOnPattern_ThenReturnsAutomation()
        {
            var pattern = new PatternDefinition("apatternname");
            var automation1 = new TestAutomation("anautomationid");
            pattern.Automation.Add(automation1);
            var solution = new SolutionDefinition(new ToolkitDefinition(pattern, "1.0"));

            var pairs = solution.FindByAutomation("anautomationid");

            pairs.Should().ContainSingle(pair =>
                pair.Automation == automation1
                && pair.SolutionItem == solution.Model);
        }

        [Fact]
        public void WhenFindByAutomationOnDescendantElement_ThenReturnsAutomation()
        {
            var pattern = new PatternDefinition("apatternname");
            var element3 = new Element("anelementname3");
            var automation1 = new TestAutomation("anautomationid");
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

            var pairs = solution.FindByAutomation("anautomationid");

            pairs.Should().ContainSingle(pair =>
                pair.Automation == automation1
                && pair.SolutionItem == solution.Model.Properties["anelementname1"].Properties["anelementname2"].Properties["anelementname3"]);
        }

        [Fact]
        public void WhenFindByAutomationOnDescendantCollectionItem_ThenReturnsAutomation()
        {
            var pattern = new PatternDefinition("apatternname");
            var collection1 = new Element("acollectionname1", isCollection: true);
            var automation1 = new TestAutomation("anautomationid");
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

            var pairs = solution.FindByAutomation("anautomationid");

            pairs.Should().ContainSingle(pair =>
                pair.Automation == automation1
                && pair.SolutionItem == solution.Model.Properties["anelementname1"].Properties["anelementname2"].Properties["acollectionname1"].Items[0]);
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

            var pairs = solution.FindByAutomation("anautomationid");

            pairs.Should().ContainSingle(pair =>
                pair.Automation == automation1
                && pair.SolutionItem == solution.Model.Properties["acollectionname1"].Items[0].Properties["anelementname1"]);
        }

        [Fact]
        public void WhenFindByAutomationOnDescendantCollectionItemElements_ThenReturnsAutomations()
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
            solution.Model.Properties["acollectionname1"].MaterialiseCollectionItem();
            solution.Model.Properties["acollectionname1"].MaterialiseCollectionItem();
            solution.Model.Properties["acollectionname1"].Items[0].Properties["anelementname1"].Materialise();
            solution.Model.Properties["acollectionname1"].Items[1].Properties["anelementname1"].Materialise();
            solution.Model.Properties["acollectionname1"].Items[2].Properties["anelementname1"].Materialise();

            var pairs = solution.FindByAutomation("anautomationid");

            pairs.Should().Contain(pair =>
                pair.Automation == automation1
                && pair.SolutionItem == solution.Model.Properties["acollectionname1"].Items[0].Properties["anelementname1"]);
            pairs.Should().Contain(pair =>
                pair.Automation == automation1
                && pair.SolutionItem == solution.Model.Properties["acollectionname1"].Items[1].Properties["anelementname1"]);
            pairs.Should().Contain(pair =>
                pair.Automation == automation1
                && pair.SolutionItem == solution.Model.Properties["acollectionname1"].Items[2].Properties["anelementname1"]);
        }

        [Fact]
        public void WhenFindByAutomationOnUnMaterialisedDescendantElement_ThenReturnsNull()
        {
            var pattern = new PatternDefinition("apatternname");
            var element3 = new Element("anelementname3");
            var automation1 = new TestAutomation("anautomationid");
            element3.Automation.Add(automation1);
            var element2 = new Element("anelementname2");
            element2.Elements.Add(element3);
            var element1 = new Element("anelementname1");
            element1.Elements.Add(element2);
            pattern.Elements.Add(element1);
            var solution = new SolutionDefinition(new ToolkitDefinition(pattern, "1.0"));
            solution.Model.Properties["anelementname1"].Materialise();

            var pairs = solution.FindByAutomation("anautomationid");

            pairs.Should().BeEmpty();
        }

        [Fact]
        public void WhenPopulateAncestryAfterDeserializationOnDescendantCollectionItem_ThenPopulates()
        {
            var pattern = new PatternDefinition("apatternname");
            var attribute1 = new Attribute("anattributename1", defaultValue: "adefaultvalue");
            pattern.Attributes.Add(attribute1);
            var collection1 = new Element("acollectionname1", isCollection: true);
            var attribute2 = new Attribute("anattributename2", defaultValue: "adefaultvalue");
            collection1.Attributes.Add(attribute2);
            var element2 = new Element("anelementname2");
            var attribute3 = new Attribute("anattributename3", defaultValue: "adefaultvalue");
            element2.Attributes.Add(attribute3);
            element2.Elements.Add(collection1);
            var element1 = new Element("anelementname1");
            var attribute4 = new Attribute("anattributename4", defaultValue: "adefaultvalue");
            element1.Attributes.Add(attribute4);
            element1.Elements.Add(element2);
            pattern.Elements.Add(element1);
            var solution = new SolutionDefinition(new ToolkitDefinition(pattern, "1.0"));
            solution.Model.Properties["anelementname1"].Materialise();
            solution.Model.Properties["anelementname1"].Properties["anelementname2"].Materialise();
            solution.Model.Properties["anelementname1"].Properties["anelementname2"].Properties["acollectionname1"].MaterialiseCollectionItem();

            solution.PopulateAncestry();

            var patternItem = solution.Model;
            patternItem.Parent.Should().BeNull();
            patternItem.Properties["anattributename1"].Parent.Should().Be(patternItem);
            var element1Item = patternItem.Properties["anelementname1"];
            element1Item.Parent.Should().Be(patternItem);
            element1Item.Properties["anattributename4"].Parent.Should().Be(element1Item);
            var element2Item = element1Item.Properties["anelementname2"];
            element2Item.Parent.Should().Be(element1Item);
            element2Item.Properties["anattributename3"].Parent.Should().Be(element2Item);
            var collection1Item = element2Item.Properties["acollectionname1"];
            collection1Item.Parent.Should().Be(element2Item);
            collection1Item.Items[0].Parent.Should().Be(element2Item);
            collection1Item.Items[0].Properties["anattributename2"].Parent.Should().Be(collection1Item.Items[0]);
        }

        [Fact]
        public void WhenPopulateAncestryAfterDeserializationOnDescendantCollectionItemWithElement_ThenPopulatesParent()
        {
            var pattern = new PatternDefinition("apatternname");
            var attribute1 = new Attribute("anattributename1", defaultValue: "adefaultvalue");
            pattern.Attributes.Add(attribute1);
            var collection1 = new Element("acollectionname1", isCollection: true);
            var attribute2 = new Attribute("anattributename2", defaultValue: "adefaultvalue");
            collection1.Attributes.Add(attribute2);
            var element1 = new Element("anelementname1");
            var attribute3 = new Attribute("anattributename3", defaultValue: "adefaultvalue");
            element1.Attributes.Add(attribute3);
            collection1.Elements.Add(element1);
            pattern.Elements.Add(collection1);

            var solution = new SolutionDefinition(new ToolkitDefinition(pattern, "1.0"));
            solution.Model.Properties["acollectionname1"].MaterialiseCollectionItem();
            solution.Model.Properties["acollectionname1"].Items[0].Properties["anelementname1"].Materialise();

            solution.PopulateAncestry();

            var patternItem = solution.Model;
            patternItem.Parent.Should().BeNull();
            patternItem.Properties["anattributename1"].Parent.Should().Be(patternItem);
            var collection1Item = patternItem.Properties["acollectionname1"];
            collection1Item.Parent.Should().Be(patternItem);
            collection1Item.Items[0].Parent.Should().Be(patternItem);
            collection1Item.Items[0].Properties["anattributename2"].Parent.Should().Be(collection1Item.Items[0]);
            var element1Item = collection1Item.Items[0].Properties["anelementname1"];
            element1Item.Parent.Should().Be(collection1Item.Items[0]);
            element1Item.Properties["anattributename3"].Parent.Should().Be(element1Item);
        }
    }
}