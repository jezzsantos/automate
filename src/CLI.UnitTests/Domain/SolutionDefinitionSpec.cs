using System.Collections.Generic;
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
            element3.AddAttribute(new Attribute("anattributename3", "string", false, "adefaultvalue3"));
            var collection3 = new Element("acollectionname3", isCollection: true);
            var element2 = new Element("anelementname2");
            element2.AddAttribute(new Attribute("anattributename2", "string", false, "adefaultvalue2"));
            var collection2 = new Element("acollectionname2", isCollection: true);
            var element1 = new Element("anelementname1");
            element1.AddAttribute(new Attribute("anattributename1", "string", false, "adefaultvalue1"));
            var collection1 = new Element("acollectionname1", isCollection: true);
            element2.AddElement(element3);
            element2.AddElement(collection3);
            pattern.AddElement(element1);
            pattern.AddElement(collection1);
            pattern.AddElement(element2);
            pattern.AddElement(collection2);
            var solution = new SolutionDefinition(new ToolkitDefinition(pattern));

            solution.Name.Should().Match("apatternname???");
            solution.Model.Should().NotBeNull();
            var solutionElement1 = solution.Model.Properties["anelementname1"];
            solutionElement1.ElementSchema.Object.Should().Be(element1);
            solutionElement1.Value.Should().BeNull();
            solutionElement1.IsMaterialised.Should().BeFalse();
            solutionElement1.Items.Should().BeNull();
            solutionElement1.Properties.Should().BeNull();

            var solutionCollection1 = solution.Model.Properties["acollectionname1"];
            solutionCollection1.ElementSchema.Object.Should().Be(collection1);
            solutionCollection1.IsMaterialised.Should().BeFalse();
            solutionCollection1.Items.Should().BeNull();
            solutionCollection1.Properties.Should().BeNull();

            var solutionElement2 = solution.Model.Properties["anelementname2"];
            solutionElement2.ElementSchema.Object.Should().Be(element2);
            solutionElement2.Value.Should().BeNull();
            solutionElement2.IsMaterialised.Should().BeFalse();
            solutionElement2.Items.Should().BeNull();
            solutionElement2.Properties.Should().BeNull();

            var solutionCollection2 = solution.Model.Properties["acollectionname2"];
            solutionCollection2.ElementSchema.Object.Should().Be(collection2);
            solutionCollection2.IsMaterialised.Should().BeFalse();
            solutionCollection2.Items.Should().BeNull();
            solutionCollection2.Properties.Should().BeNull();
        }

        [Fact]
        public void WhenFindByAutomationAndNotExist_ThenReturnsEmpty()
        {
            var pattern = new PatternDefinition("apatternname");
            var solution = new SolutionDefinition(new ToolkitDefinition(pattern));

            var pairs = solution.FindByAutomation("anautomationid");

            pairs.Should().BeEmpty();
        }

        [Fact]
        public void WhenFindByAutomationOnPattern_ThenReturnsAutomation()
        {
            var pattern = new PatternDefinition("apatternname");
            var automation = new Automation("acommandname", AutomationType.TestingOnly, new Dictionary<string, object>());
            pattern.AddAutomation(automation);
            var solution = new SolutionDefinition(new ToolkitDefinition(pattern));

            var pairs = solution.FindByAutomation(automation.Id);

            pairs.Should().ContainSingle(pair =>
                pair.Automation.Object == automation
                && pair.SolutionItem == solution.Model);
        }

        [Fact]
        public void WhenFindByAutomationOnDescendantElement_ThenReturnsAutomation()
        {
            var pattern = new PatternDefinition("apatternname");
            var element3 = new Element("anelementname3");
            var automation = new Automation("acommandname", AutomationType.TestingOnly, new Dictionary<string, object>());
            element3.AddAutomation(automation);
            var element2 = new Element("anelementname2");
            element2.AddElement(element3);
            var element1 = new Element("anelementname1");
            element1.AddElement(element2);
            pattern.AddElement(element1);
            var solution = new SolutionDefinition(new ToolkitDefinition(pattern));
            solution.Model.Properties["anelementname1"].Materialise();
            solution.Model.Properties["anelementname1"].Properties["anelementname2"].Materialise();
            solution.Model.Properties["anelementname1"].Properties["anelementname2"].Properties["anelementname3"].Materialise();

            var pairs = solution.FindByAutomation(automation.Id);

            pairs.Should().ContainSingle(pair =>
                pair.Automation.Object == automation
                && pair.SolutionItem == solution.Model.Properties["anelementname1"].Properties["anelementname2"].Properties["anelementname3"]);
        }

        [Fact]
        public void WhenFindByAutomationOnDescendantCollectionItem_ThenReturnsAutomation()
        {
            var pattern = new PatternDefinition("apatternname");
            var collection1 = new Element("acollectionname1", isCollection: true);
            var automation = new Automation("acommandname", AutomationType.TestingOnly, new Dictionary<string, object>());
            collection1.AddAutomation(automation);
            var element2 = new Element("anelementname2");
            element2.AddElement(collection1);
            var element1 = new Element("anelementname1");
            element1.AddElement(element2);
            pattern.AddElement(element1);
            var solution = new SolutionDefinition(new ToolkitDefinition(pattern));
            solution.Model.Properties["anelementname1"].Materialise();
            solution.Model.Properties["anelementname1"].Properties["anelementname2"].Materialise();
            solution.Model.Properties["anelementname1"].Properties["anelementname2"].Properties["acollectionname1"].MaterialiseCollectionItem();

            var pairs = solution.FindByAutomation(automation.Id);

            pairs.Should().ContainSingle(pair =>
                pair.Automation.Object == automation
                && pair.SolutionItem == solution.Model.Properties["anelementname1"].Properties["anelementname2"].Properties["acollectionname1"].Items[0]);
        }

        [Fact]
        public void WhenFindByAutomationOnDescendantCollectionItemElement_ThenReturnsAutomation()
        {
            var pattern = new PatternDefinition("apatternname");
            var collection1 = new Element("acollectionname1", isCollection: true);
            var automation = new Automation("acommandname", AutomationType.TestingOnly, new Dictionary<string, object>());
            var element1 = new Element("anelementname1");
            element1.AddAutomation(automation);
            collection1.AddElement(element1);
            pattern.AddElement(collection1);

            var solution = new SolutionDefinition(new ToolkitDefinition(pattern));
            solution.Model.Properties["acollectionname1"].MaterialiseCollectionItem();
            solution.Model.Properties["acollectionname1"].Items[0].Properties["anelementname1"].Materialise();

            var pairs = solution.FindByAutomation(automation.Id);

            pairs.Should().ContainSingle(pair =>
                pair.Automation.Object == automation
                && pair.SolutionItem == solution.Model.Properties["acollectionname1"].Items[0].Properties["anelementname1"]);
        }

        [Fact]
        public void WhenFindByAutomationOnDescendantCollectionItemElements_ThenReturnsAutomations()
        {
            var pattern = new PatternDefinition("apatternname");
            var collection1 = new Element("acollectionname1", isCollection: true);
            var automation = new Automation("acommandname", AutomationType.TestingOnly, new Dictionary<string, object>());
            var element1 = new Element("anelementname1");
            element1.AddAutomation(automation);
            collection1.AddElement(element1);
            pattern.AddElement(collection1);

            var solution = new SolutionDefinition(new ToolkitDefinition(pattern));
            solution.Model.Properties["acollectionname1"].MaterialiseCollectionItem();
            solution.Model.Properties["acollectionname1"].MaterialiseCollectionItem();
            solution.Model.Properties["acollectionname1"].MaterialiseCollectionItem();
            solution.Model.Properties["acollectionname1"].Items[0].Properties["anelementname1"].Materialise();
            solution.Model.Properties["acollectionname1"].Items[1].Properties["anelementname1"].Materialise();
            solution.Model.Properties["acollectionname1"].Items[2].Properties["anelementname1"].Materialise();

            var pairs = solution.FindByAutomation(automation.Id);

            pairs.Should().Contain(pair =>
                pair.Automation.Object == automation
                && pair.SolutionItem == solution.Model.Properties["acollectionname1"].Items[0].Properties["anelementname1"]);
            pairs.Should().Contain(pair =>
                pair.Automation.Object == automation
                && pair.SolutionItem == solution.Model.Properties["acollectionname1"].Items[1].Properties["anelementname1"]);
            pairs.Should().Contain(pair =>
                pair.Automation.Object == automation
                && pair.SolutionItem == solution.Model.Properties["acollectionname1"].Items[2].Properties["anelementname1"]);
        }

        [Fact]
        public void WhenFindByAutomationOnUnMaterialisedDescendantElement_ThenReturnsNull()
        {
            var pattern = new PatternDefinition("apatternname");
            var element3 = new Element("anelementname3");
            var automation = new Automation("acommandname", AutomationType.TestingOnly, new Dictionary<string, object>());
            element3.AddAutomation(automation);
            var element2 = new Element("anelementname2");
            element2.AddElement(element3);
            var element1 = new Element("anelementname1");
            element1.AddElement(element2);
            pattern.AddElement(element1);
            var solution = new SolutionDefinition(new ToolkitDefinition(pattern));
            solution.Model.Properties["anelementname1"].Materialise();

            var pairs = solution.FindByAutomation(automation.Id);

            pairs.Should().BeEmpty();
        }

        [Fact]
        public void WhenMaterialiseOnDescendantCollectionItem_ThenPopulatesParent()
        {
            var pattern = new PatternDefinition("apatternname");
            var attribute1 = new Attribute("anattributename1", defaultValue: "adefaultvalue");
            pattern.AddAttribute(attribute1);
            var collection1 = new Element("acollectionname1", isCollection: true);
            var attribute2 = new Attribute("anattributename2", defaultValue: "adefaultvalue");
            collection1.AddAttribute(attribute2);
            var element2 = new Element("anelementname2");
            var attribute3 = new Attribute("anattributename3", defaultValue: "adefaultvalue");
            element2.AddAttribute(attribute3);
            element2.AddElement(collection1);
            var element1 = new Element("anelementname1");
            var attribute4 = new Attribute("anattributename4", defaultValue: "adefaultvalue");
            element1.AddAttribute(attribute4);
            element1.AddElement(element2);
            pattern.AddElement(element1);
            var solution = new SolutionDefinition(new ToolkitDefinition(pattern));
            solution.Model.Properties["anelementname1"].Materialise();
            solution.Model.Properties["anelementname1"].Properties["anelementname2"].Materialise();
            solution.Model.Properties["anelementname1"].Properties["anelementname2"].Properties["acollectionname1"].MaterialiseCollectionItem();

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
        public void WhenMaterialiseOnDescendantCollectionItemWithElement_ThenPopulatesParent()
        {
            var pattern = new PatternDefinition("apatternname");
            var attribute1 = new Attribute("anattributename1", defaultValue: "adefaultvalue");
            pattern.AddAttribute(attribute1);
            var collection1 = new Element("acollectionname1", isCollection: true);
            var attribute2 = new Attribute("anattributename2", defaultValue: "adefaultvalue");
            collection1.AddAttribute(attribute2);
            var element1 = new Element("anelementname1");
            var attribute3 = new Attribute("anattributename3", defaultValue: "adefaultvalue");
            element1.AddAttribute(attribute3);
            collection1.AddElement(element1);
            pattern.AddElement(collection1);

            var solution = new SolutionDefinition(new ToolkitDefinition(pattern));
            solution.Model.Properties["acollectionname1"].MaterialiseCollectionItem();
            solution.Model.Properties["acollectionname1"].Items[0].Properties["anelementname1"].Materialise();

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

        [Fact]
        public void WhenUpgradeAndToolkitSameVersion_ThenReturnsSuccessWithWarning()
        {
            var pattern = new PatternDefinition("apatternname");
            var toolkit = new ToolkitDefinition(pattern);
            var solution = new SolutionDefinition(toolkit);

            var result = solution.Upgrade(toolkit, false);

            solution.Toolkit.Pattern.ToolkitVersion.Current.Should().Be("0.0.0");
            solution.Toolkit.Version.Should().Be("0.0.0");
            result.IsSuccess.Should().BeTrue();
            result.Log.Should().ContainSingle(x =>
                x.Type == MigrationChangeType.Abort
                && x.MessageTemplate == MigrationMessages.SolutionDefinition_Upgrade_SameToolkitVersion);
        }

        [Fact]
        public void WhenUpgradeAndNewToolkitHasBreakingChange_ThenReturnsFailureWithWarning()
        {
            var pattern = new PatternDefinition("apatternname");
            var toolkit = new ToolkitDefinition(pattern);
            var solution = new SolutionDefinition(toolkit);

            pattern = new PatternDefinition("apatternname");
            pattern.UpdateToolkitVersion(new VersionInstruction("1.0.0"));
            var updatedToolkit = new ToolkitDefinition(pattern);

            var result = solution.Upgrade(updatedToolkit, false);

            solution.Toolkit.Pattern.ToolkitVersion.Current.Should().Be("0.0.0");
            solution.Toolkit.Version.Should().Be("0.0.0");
            result.IsSuccess.Should().BeFalse();
            result.Log.Should().ContainSingle(x =>
                x.Type == MigrationChangeType.Abort
                && x.MessageTemplate == MigrationMessages.SolutionDefinition_Upgrade_BreakingChangeForbidden);
        }

        [Fact]
        public void WhenUpgradeAndNewToolkitHasBreakingChangeAndForce_ThenReturnsSuccessWithWarning()
        {
            var pattern = new PatternDefinition("apatternname");
            var toolkit = new ToolkitDefinition(pattern);
            var solution = new SolutionDefinition(toolkit);

            pattern = new PatternDefinition("apatternname");
            pattern.UpdateToolkitVersion(new VersionInstruction("1.0.0"));
            var updatedToolkit = new ToolkitDefinition(pattern);

            var result = solution.Upgrade(updatedToolkit, true);

            solution.Toolkit.Pattern.ToolkitVersion.Current.Should().Be("1.0.0");
            solution.Toolkit.Version.Should().Be("1.0.0");
            result.IsSuccess.Should().BeTrue();
            result.Log.Should().ContainSingle(x =>
                x.Type == MigrationChangeType.Breaking
                && x.MessageTemplate == MigrationMessages.SolutionDefinition_Upgrade_BreakingChangeForced);
        }

        [Fact]
        public void WhenUpgrade_ThenUpgrades()
        {
            var pattern = new PatternDefinition("apatternname");
            var toolkit = new ToolkitDefinition(pattern);
            var solution = new SolutionDefinition(toolkit);

            pattern = new PatternDefinition("apatternname");
            pattern.UpdateToolkitVersion(new VersionInstruction());
            var updatedToolkit = new ToolkitDefinition(pattern);

            var result = solution.Upgrade(updatedToolkit, true);

            solution.Toolkit.Pattern.ToolkitVersion.Current.Should().Be("0.1.0");
            solution.Toolkit.Version.Should().Be("0.1.0");
            result.IsSuccess.Should().BeTrue();
            result.Log.Should().BeEmpty();
        }

        [Fact]
        public void WhenPopulateAncestry_ThenSetsAncestry()
        {
            var pattern = new PatternDefinition("apatternname");
            var element1 = new Element("anelementname1");
            element1.AddAttribute("anattributename1");
            pattern.AddElement(element1);

            var element2 = new Element("anelementname2");
            var collection1 = new Element("acollectionname1", isCollection: true);
            collection1.AddAttribute("anattributename1");
            element2.AddElement(collection1);
            pattern.AddElement(element2);

            var collection2 = new Element("acollectionname2", isCollection: true);
            var element3 = new Element("anelementname3");
            element3.AddAttribute("anattributename3");
            collection2.AddElement(element3);
            pattern.AddElement(collection2);

            var collection3 = new Element("acollectionname3", isCollection: true);
            var collection4 = new Element("acollectionname4", isCollection: true);
            collection3.AddElement(collection4);
            var collection5 = new Element("acollectionname5", isCollection: true);
            collection5.AddAttribute("anattributename4");
            collection4.AddElement(collection5);
            pattern.AddElement(collection3);

            var toolkit = new ToolkitDefinition(pattern);
            var solution = new SolutionDefinition(toolkit);

            solution.Model.Properties["anelementname1"].Materialise();
            solution.Model.Properties["anelementname2"].Materialise();
            solution.Model.Properties["anelementname2"].Properties["acollectionname1"].MaterialiseCollectionItem();
            solution.Model.Properties["acollectionname2"].MaterialiseCollectionItem();
            solution.Model.Properties["acollectionname2"].Items[0].Properties["anelementname3"].Materialise();
            solution.Model.Properties["acollectionname3"].MaterialiseCollectionItem();
            solution.Model.Properties["acollectionname3"].Items[0].Properties["acollectionname4"].MaterialiseCollectionItem();
            solution.Model.Properties["acollectionname3"].Items[0].Properties["acollectionname4"].Items[0].Properties["acollectionname5"].MaterialiseCollectionItem();

            solution.PopulateAncestry();

            solution.Model.Parent.Should().BeNull();
            solution.Model.Properties["anelementname1"].Parent.Should().Be(solution.Model);

            solution.Model.Properties["anelementname2"].Parent.Should().Be(solution.Model);
            solution.Model.Properties["anelementname2"].Properties["acollectionname1"].Parent.Should().Be(solution.Model.Properties["anelementname2"]);
            solution.Model.Properties["anelementname2"].Properties["acollectionname1"].Items[0].Parent.Should().Be(solution.Model.Properties["anelementname2"]);

            solution.Model.Properties["acollectionname2"].Parent.Should().Be(solution.Model);
            solution.Model.Properties["acollectionname2"].Items[0].Parent.Should().Be(solution.Model);
            solution.Model.Properties["acollectionname2"].Items[0].Properties["anelementname3"].Parent.Should().Be(solution.Model.Properties["acollectionname2"].Items[0]);

            solution.Model.Properties["acollectionname3"].Parent.Should().Be(solution.Model);
            solution.Model.Properties["acollectionname3"].Items[0].Parent.Should().Be(solution.Model);
            solution.Model.Properties["acollectionname3"].Items[0].Properties["acollectionname4"].Parent.Should().Be(solution.Model.Properties["acollectionname3"].Items[0]);
            solution.Model.Properties["acollectionname3"].Items[0].Properties["acollectionname4"].Items[0].Parent.Should().Be(solution.Model.Properties["acollectionname3"].Items[0]);
            solution.Model.Properties["acollectionname3"].Items[0].Properties["acollectionname4"].Items[0].Properties["acollectionname5"].Parent.Should()
                .Be(solution.Model.Properties["acollectionname3"].Items[0].Properties["acollectionname4"].Items[0]);
            solution.Model.Properties["acollectionname3"].Items[0].Properties["acollectionname4"].Items[0].Properties["acollectionname5"].Items[0].Parent.Should()
                .Be(solution.Model.Properties["acollectionname3"].Items[0].Properties["acollectionname4"].Items[0]);
        }
    }
}