using System;
using System.Collections.Generic;
using Automate.Authoring.Domain;
using Automate.Common.Domain;
using Automate.Common.Extensions;
using Automate.Runtime.Domain;
using FluentAssertions;
using Xunit;
using Attribute = Automate.Authoring.Domain.Attribute;

namespace Core.UnitTests.Runtime.Domain
{
    [Trait("Category", "Unit")]
    public class DraftDefinitionSpec
    {
        [Fact]
        public void WhenConstructWithNullName_ThenAssignsRandomName()
        {
            var result = new DraftDefinition(new ToolkitDefinition(new PatternDefinition("apatternname")));

            result.Name.Should().StartWith("apatternname");
            result.Name.Should().MatchRegex("apatternname[\\d]{3}");
        }

        [Fact]
        public void WhenConstructWithInvalidName_ThenThrows()
        {
            FluentActions.Invoking(() =>
                    new DraftDefinition(new ToolkitDefinition(new PatternDefinition("apatternname")),
                        "an invalid name"))
                .Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage(ValidationMessages.InvalidNameIdentifier.Substitute("an invalid name") + "*");
        }

        [Fact]
        public void WhenConstructed_ThenInitialisesTopLevelElementsAndCollectionsOnly()
        {
            var pattern = new PatternDefinition("apatternname");
            var element3 = new Element("anelementname3", autoCreate: false);
            element3.AddAttribute(new Attribute("anattributename3", "string", false, "adefaultvalue3"));
            var collection3 = new Element("acollectionname3", autoCreate: false);
            var element2 = new Element("anelementname2", autoCreate: false);
            element2.AddAttribute(new Attribute("anattributename2", "string", false, "adefaultvalue2"));
            var collection2 = new Element("acollectionname2", autoCreate: false);
            var element1 = new Element("anelementname1", autoCreate: false);
            element1.AddAttribute(new Attribute("anattributename1", "string", false, "adefaultvalue1"));
            var collection1 = new Element("acollectionname1", autoCreate: false);
            element2.AddElement(element3);
            element2.AddElement(collection3);
            pattern.AddElement(element1);
            pattern.AddElement(collection1);
            pattern.AddElement(element2);
            pattern.AddElement(collection2);

            var draft = new DraftDefinition(new ToolkitDefinition(pattern));

            draft.Name.Should().Match("apatternname???");
            draft.Model.Should().NotBeNull();
            var draftElement1 = draft.Model.Properties["anelementname1"];
            draftElement1.ElementSchema.Element.Should().Be(element1);
            draftElement1.Value.Should().BeNull();
            draftElement1.IsMaterialised.Should().BeFalse();
            draftElement1.Items.Should().BeNull();
            draftElement1.Properties["anattributename1"].IsMaterialised.Should().BeFalse();

            var draftCollection1 = draft.Model.Properties["acollectionname1"];
            draftCollection1.ElementSchema.Element.Should().Be(collection1);
            draftCollection1.IsMaterialised.Should().BeFalse();
            draftCollection1.Items.Should().BeNull();
            draftCollection1.Properties.Should().BeEmpty();

            var draftElement2 = draft.Model.Properties["anelementname2"];
            draftElement2.ElementSchema.Element.Should().Be(element2);
            draftElement2.Value.Should().BeNull();
            draftElement2.IsMaterialised.Should().BeFalse();
            draftElement2.Items.Should().BeNull();
            draftElement2.Properties["anelementname3"].IsMaterialised.Should().BeFalse();
            draftElement2.Properties["acollectionname3"].IsMaterialised.Should().BeFalse();

            var draftCollection2 = draft.Model.Properties["acollectionname2"];
            draftCollection2.ElementSchema.Element.Should().Be(collection2);
            draftCollection2.IsMaterialised.Should().BeFalse();
            draftCollection2.Items.Should().BeNull();
            draftCollection2.Properties.Should().BeEmpty();
        }

        [Fact]
        public void WhenFindByAutomationAndNotExist_ThenReturnsEmpty()
        {
            var pattern = new PatternDefinition("apatternname");
            var draft = new DraftDefinition(new ToolkitDefinition(pattern));

            var pairs = draft.FindByAutomation("anautomationid");

            pairs.Should().BeEmpty();
        }

        [Fact]
        public void WhenMaterialiseOnDescendantCollectionItem_ThenPopulatesParent()
        {
            var pattern = new PatternDefinition("apatternname");
            var attribute1 = new Attribute("anattributename1", defaultValue: "adefaultvalue");
            pattern.AddAttribute(attribute1);
            var collection1 = new Element("acollectionname1",
                ElementCardinality.OneOrMany, false);
            var attribute2 = new Attribute("anattributename2", defaultValue: "adefaultvalue");
            collection1.AddAttribute(attribute2);
            var element2 = new Element("anelementname2", autoCreate: false);
            var attribute3 = new Attribute("anattributename3", defaultValue: "adefaultvalue");
            element2.AddAttribute(attribute3);
            element2.AddElement(collection1);
            var element1 = new Element("anelementname1", autoCreate: false);
            var attribute4 = new Attribute("anattributename4", defaultValue: "adefaultvalue");
            element1.AddAttribute(attribute4);
            element1.AddElement(element2);
            pattern.AddElement(element1);

            var draft = new DraftDefinition(new ToolkitDefinition(pattern));
            draft.Model.Properties["anelementname1"].Materialise();
            draft.Model.Properties["anelementname1"].Properties["anelementname2"].Materialise();
            draft.Model.Properties["anelementname1"].Properties["anelementname2"].Properties["acollectionname1"]
                .MaterialiseCollectionItem();

            var draftPattern = draft.Model;
            draftPattern.Parent.Should().BeNull();
            draftPattern.Properties["anattributename1"].Parent.Should().Be(draftPattern);
            var draftElement1 = draftPattern.Properties["anelementname1"];
            draftElement1.Parent.Should().Be(draftPattern);
            draftElement1.Properties["anattributename4"].Parent.Should().Be(draftElement1);
            var draftElement2 = draftElement1.Properties["anelementname2"];
            draftElement2.Parent.Should().Be(draftElement1);
            draftElement2.Properties["anattributename3"].Parent.Should().Be(draftElement2);
            var draftCollection = draftElement2.Properties["acollectionname1"];
            draftCollection.Parent.Should().Be(draftElement2);
            draftCollection.Items[0].Parent.Should().Be(draftElement2);
            draftCollection.Items[0].Properties["anattributename2"].Parent.Should().Be(draftCollection.Items[0]);
        }

        [Fact]
        public void WhenMaterialiseOnDescendantCollectionItemWithElement_ThenPopulatesParent()
        {
            var pattern = new PatternDefinition("apatternname");
            var attribute1 = new Attribute("anattributename1", defaultValue: "adefaultvalue");
            pattern.AddAttribute(attribute1);
            var collection1 = new Element("acollectionname1",
                ElementCardinality.OneOrMany);
            var attribute2 = new Attribute("anattributename2", defaultValue: "adefaultvalue");
            collection1.AddAttribute(attribute2);
            var element1 = new Element("anelementname1");
            var attribute3 = new Attribute("anattributename3", defaultValue: "adefaultvalue");
            element1.AddAttribute(attribute3);
            collection1.AddElement(element1);
            pattern.AddElement(collection1);

            var draft = new DraftDefinition(new ToolkitDefinition(pattern));
            draft.Model.Properties["acollectionname1"].MaterialiseCollectionItem();
            draft.Model.Properties["acollectionname1"].Items[0].Properties["anelementname1"].Materialise();

            var draftPattern = draft.Model;
            draftPattern.Parent.Should().BeNull();
            draftPattern.Properties["anattributename1"].Parent.Should().Be(draftPattern);
            var draftCollection = draftPattern.Properties["acollectionname1"];
            draftCollection.Parent.Should().Be(draftPattern);
            draftCollection.Items[0].Parent.Should().Be(draftPattern);
            draftCollection.Items[0].Properties["anattributename2"].Parent.Should().Be(draftCollection.Items[0]);
            var draftElement1 = draftCollection.Items[0].Properties["anelementname1"];
            draftElement1.Parent.Should().Be(draftCollection.Items[0]);
            draftElement1.Properties["anattributename3"].Parent.Should().Be(draftElement1);
        }

        [Fact]
        public void WhenUpgradeAndToolkitSameVersion_ThenReturnsSuccessWithWarning()
        {
            var pattern = new PatternDefinition("apatternname");
            var toolkit = new ToolkitDefinition(pattern);
            var draft = new DraftDefinition(toolkit);

            var result = draft.Upgrade(toolkit, false);

            draft.Toolkit.Pattern.ToolkitVersion.Current.Should().Be("0.0.0");
            draft.Toolkit.Version.Should().Be("0.0.0");
            result.IsSuccess.Should().BeTrue();
            result.Log.Should().ContainSingle(x =>
                x.Type == MigrationChangeType.Abort
                && x.MessageTemplate == MigrationMessages.DraftDefinition_Upgrade_SameToolkitVersion);
        }

        [Fact]
        public void WhenUpgradeAndNewToolkitHasBreakingChange_ThenReturnsFailureWithWarning()
        {
            var pattern = new PatternDefinition("apatternname");
            var toolkit = new ToolkitDefinition(pattern);
            var draft = new DraftDefinition(toolkit);

            pattern = new PatternDefinition("apatternname");
            pattern.UpdateToolkitVersion(new VersionInstruction("1.0.0"));
            var updatedToolkit = new ToolkitDefinition(pattern);

            var result = draft.Upgrade(updatedToolkit, false);

            draft.Toolkit.Pattern.ToolkitVersion.Current.Should().Be("0.0.0");
            draft.Toolkit.Version.Should().Be("0.0.0");
            result.IsSuccess.Should().BeFalse();
            result.Log.Should().ContainSingle(x =>
                x.Type == MigrationChangeType.Abort
                && x.MessageTemplate == MigrationMessages.DraftDefinition_Upgrade_BreakingChangeForbidden);
        }

        [Fact]
        public void WhenUpgradeAndNewToolkitHasBreakingChangeAndForce_ThenReturnsSuccessWithWarning()
        {
            var pattern = new PatternDefinition("apatternname");
            var toolkit = new ToolkitDefinition(pattern);
            var draft = new DraftDefinition(toolkit);

            pattern = new PatternDefinition("apatternname");
            pattern.UpdateToolkitVersion(new VersionInstruction("1.0.0"));
            var updatedToolkit = new ToolkitDefinition(pattern);

            var result = draft.Upgrade(updatedToolkit, true);

            draft.Toolkit.Pattern.ToolkitVersion.Current.Should().Be("1.0.0");
            draft.Toolkit.Version.Should().Be("1.0.0");
            result.IsSuccess.Should().BeTrue();
            result.Log.Should().ContainSingle(x =>
                x.Type == MigrationChangeType.Breaking
                && x.MessageTemplate == MigrationMessages.DraftDefinition_Upgrade_BreakingChangeForced);
        }

        [Fact]
        public void WhenUpgrade_ThenUpgrades()
        {
            var pattern = new PatternDefinition("apatternname");
            var toolkit = new ToolkitDefinition(pattern);
            var draft = new DraftDefinition(toolkit);

            pattern = new PatternDefinition("apatternname");
            pattern.UpdateToolkitVersion(new VersionInstruction());
            var updatedToolkit = new ToolkitDefinition(pattern);

            var result = draft.Upgrade(updatedToolkit, true);

            draft.Toolkit.Pattern.ToolkitVersion.Current.Should().Be("0.1.0");
            draft.Toolkit.Version.Should().Be("0.1.0");
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
            var collection1 = new Element("acollectionname1", ElementCardinality.OneOrMany);
            collection1.AddAttribute("anattributename1");
            element2.AddElement(collection1);
            pattern.AddElement(element2);

            var collection2 = new Element("acollectionname2", ElementCardinality.OneOrMany);
            var element3 = new Element("anelementname3");
            element3.AddAttribute("anattributename3");
            collection2.AddElement(element3);
            pattern.AddElement(collection2);

            var collection3 = new Element("acollectionname3", ElementCardinality.OneOrMany);
            var collection4 = new Element("acollectionname4", ElementCardinality.OneOrMany);
            collection3.AddElement(collection4);
            var collection5 = new Element("acollectionname5", ElementCardinality.OneOrMany);
            collection5.AddAttribute("anattributename4");
            collection4.AddElement(collection5);
            pattern.AddElement(collection3);

            var toolkit = new ToolkitDefinition(pattern);
            var draft = new DraftDefinition(toolkit);

            draft.Model.Properties["anelementname1"].Materialise();
            draft.Model.Properties["anelementname2"].Materialise();
            draft.Model.Properties["anelementname2"].Properties["acollectionname1"].MaterialiseCollectionItem();
            draft.Model.Properties["acollectionname2"].MaterialiseCollectionItem();
            draft.Model.Properties["acollectionname2"].Items[0].Properties["anelementname3"].Materialise();
            draft.Model.Properties["acollectionname3"].MaterialiseCollectionItem();
            draft.Model.Properties["acollectionname3"].Items[0].Properties["acollectionname4"]
                .MaterialiseCollectionItem();
            draft.Model.Properties["acollectionname3"].Items[0].Properties["acollectionname4"].Items[0]
                .Properties["acollectionname5"].MaterialiseCollectionItem();

            draft.PopulateAncestry();

            draft.Model.Parent.Should().BeNull();
            var draftElement1 = draft.Model.Properties["anelementname1"];
            draftElement1.Parent.Should().Be(draft.Model);

            var draftElement2 = draft.Model.Properties["anelementname2"];
            draftElement2.Parent.Should().Be(draft.Model);
            draftElement2.Properties["acollectionname1"].Parent.Should()
                .Be(draft.Model.Properties["anelementname2"]);
            draftElement2.Properties["acollectionname1"].Items[0].Parent.Should()
                .Be(draftElement2);

            var draftCollection2 = draft.Model.Properties["acollectionname2"];
            draftCollection2.Parent.Should().Be(draft.Model);
            draftCollection2.Items[0].Parent.Should().Be(draft.Model);
            draftCollection2.Items[0].Properties["anelementname3"].Parent.Should()
                .Be(draftCollection2.Items[0]);

            var draftCollection3 = draft.Model.Properties["acollectionname3"];
            draftCollection3.Parent.Should().Be(draft.Model);
            draftCollection3.Items[0].Parent.Should().Be(draft.Model);
            draftCollection3.Items[0].Properties["acollectionname4"].Parent.Should()
                .Be(draftCollection3.Items[0]);
            draftCollection3.Items[0].Properties["acollectionname4"].Items[0].Parent.Should()
                .Be(draftCollection3.Items[0]);
            draftCollection3.Items[0].Properties["acollectionname4"].Items[0]
                .Properties["acollectionname5"].Parent.Should()
                .Be(draftCollection3.Items[0].Properties["acollectionname4"].Items[0]);
            draftCollection3.Items[0].Properties["acollectionname4"].Items[0]
                .Properties["acollectionname5"].Items[0].Parent.Should()
                .Be(draftCollection3.Items[0].Properties["acollectionname4"].Items[0]);
        }
#if TESTINGONLY

        [Fact]
        public void WhenFindByAutomationOnPattern_ThenReturnsAutomation()
        {
            var pattern = new PatternDefinition("apatternname");
            var automation =
                new Automation("acommandname", AutomationType.TestingOnlyLaunchable,
                    new Dictionary<string, object>());
            pattern.AddAutomation(automation);
            var draft = new DraftDefinition(new ToolkitDefinition(pattern));

            var result = draft.FindByAutomation(automation.Id);

            result.Should().ContainSingle(pair =>
                pair.Automation.Automation == automation
                && pair.DraftItem == draft.Model);
        }

        [Fact]
        public void WhenFindByAutomationOnDescendantElement_ThenReturnsAutomation()
        {
            var pattern = new PatternDefinition("apatternname");
            var element3 = new Element("anelementname3");
            var automation =
                new Automation("acommandname", AutomationType.TestingOnlyLaunchable,
                    new Dictionary<string, object>());
            element3.AddAutomation(automation);
            var element2 = new Element("anelementname2");
            element2.AddElement(element3);
            var element1 = new Element("anelementname1");
            element1.AddElement(element2);
            pattern.AddElement(element1);
            var draft = new DraftDefinition(new ToolkitDefinition(pattern));
            draft.Model.Properties["anelementname1"].Materialise();
            draft.Model.Properties["anelementname1"].Properties["anelementname2"].Materialise();
            draft.Model.Properties["anelementname1"].Properties["anelementname2"].Properties["anelementname3"]
                .Materialise();

            var result = draft.FindByAutomation(automation.Id);

            result.Should().ContainSingle(pair =>
                pair.Automation.Automation == automation
                && pair.DraftItem == draft.Model.Properties["anelementname1"].Properties["anelementname2"]
                    .Properties["anelementname3"]);
        }

        [Fact]
        public void WhenFindByAutomationOnDescendantCollectionItem_ThenReturnsAutomation()
        {
            var pattern = new PatternDefinition("apatternname");
            var collection1 = new Element("acollectionname1",
                ElementCardinality.OneOrMany);
            var automation =
                new Automation("acommandname", AutomationType.TestingOnlyLaunchable,
                    new Dictionary<string, object>());
            collection1.AddAutomation(automation);
            var element2 = new Element("anelementname2");
            element2.AddElement(collection1);
            var element1 = new Element("anelementname1");
            element1.AddElement(element2);
            pattern.AddElement(element1);
            var draft = new DraftDefinition(new ToolkitDefinition(pattern));
            draft.Model.Properties["anelementname1"].Materialise();
            draft.Model.Properties["anelementname1"].Properties["anelementname2"].Materialise();
            draft.Model.Properties["anelementname1"].Properties["anelementname2"].Properties["acollectionname1"]
                .MaterialiseCollectionItem();

            var result = draft.FindByAutomation(automation.Id);

            result.Should().ContainSingle(pair =>
                pair.Automation.Automation == automation
                && pair.DraftItem == draft.Model.Properties["anelementname1"].Properties["anelementname2"]
                    .Properties["acollectionname1"].Items[0]);
        }

        [Fact]
        public void WhenFindByAutomationOnDescendantCollectionItemElement_ThenReturnsAutomation()
        {
            var pattern = new PatternDefinition("apatternname");
            var collection1 = new Element("acollectionname1",
                ElementCardinality.OneOrMany);
            var automation =
                new Automation("acommandname", AutomationType.TestingOnlyLaunchable,
                    new Dictionary<string, object>());
            var element1 = new Element("anelementname1");
            element1.AddAutomation(automation);
            collection1.AddElement(element1);
            pattern.AddElement(collection1);

            var draft = new DraftDefinition(new ToolkitDefinition(pattern));
            draft.Model.Properties["acollectionname1"].MaterialiseCollectionItem();
            draft.Model.Properties["acollectionname1"].Items[0].Properties["anelementname1"].Materialise();

            var result = draft.FindByAutomation(automation.Id);

            result.Should().ContainSingle(pair =>
                pair.Automation.Automation == automation
                && pair.DraftItem == draft.Model.Properties["acollectionname1"].Items[0].Properties["anelementname1"]);
        }

        [Fact]
        public void WhenFindByAutomationOnDescendantCollectionItemElements_ThenReturnsAutomations()
        {
            var pattern = new PatternDefinition("apatternname");
            var collection1 = new Element("acollectionname1",
                ElementCardinality.OneOrMany);
            var automation =
                new Automation("acommandname", AutomationType.TestingOnlyLaunchable,
                    new Dictionary<string, object>());
            var element1 = new Element("anelementname1");
            element1.AddAutomation(automation);
            collection1.AddElement(element1);
            pattern.AddElement(collection1);

            var draft = new DraftDefinition(new ToolkitDefinition(pattern));
            draft.Model.Properties["acollectionname1"].MaterialiseCollectionItem();
            draft.Model.Properties["acollectionname1"].MaterialiseCollectionItem();
            draft.Model.Properties["acollectionname1"].MaterialiseCollectionItem();
            draft.Model.Properties["acollectionname1"].Items[0].Properties["anelementname1"].Materialise();
            draft.Model.Properties["acollectionname1"].Items[1].Properties["anelementname1"].Materialise();
            draft.Model.Properties["acollectionname1"].Items[2].Properties["anelementname1"].Materialise();

            var result = draft.FindByAutomation(automation.Id);

            result.Should().Contain(pair =>
                pair.Automation.Automation == automation
                && pair.DraftItem == draft.Model.Properties["acollectionname1"].Items[0].Properties["anelementname1"]);
            result.Should().Contain(pair =>
                pair.Automation.Automation == automation
                && pair.DraftItem == draft.Model.Properties["acollectionname1"].Items[1].Properties["anelementname1"]);
            result.Should().Contain(pair =>
                pair.Automation.Automation == automation
                && pair.DraftItem == draft.Model.Properties["acollectionname1"].Items[2].Properties["anelementname1"]);
        }

        [Fact]
        public void WhenFindByAutomationOnUnMaterialisedDescendantElement_ThenReturnsNull()
        {
            var pattern = new PatternDefinition("apatternname");
            var element3 = new Element("anelementname3", autoCreate: false);
            var automation =
                new Automation("acommandname", AutomationType.TestingOnlyLaunchable,
                    new Dictionary<string, object>());
            element3.AddAutomation(automation);
            var element2 = new Element("anelementname2", autoCreate: false);
            element2.AddElement(element3);
            var element1 = new Element("anelementname1", autoCreate: false);
            element1.AddElement(element2);
            pattern.AddElement(element1);
            var draft = new DraftDefinition(new ToolkitDefinition(pattern));
            draft.Model.Properties["anelementname1"].Properties["anelementname2"].Materialise();

            var result = draft.FindByAutomation(automation.Id);

            result.Should().BeEmpty();
        }

        [Fact]
        public void WhenFindByAutomationOnMaterialisedDescendantElement_ThenReturnsNull()
        {
            var pattern = new PatternDefinition("apatternname");
            var element3 = new Element("anelementname3", autoCreate: false);
            var automation =
                new Automation("acommandname", AutomationType.TestingOnlyLaunchable,
                    new Dictionary<string, object>());
            element3.AddAutomation(automation);
            var element2 = new Element("anelementname2", autoCreate: false);
            element2.AddElement(element3);
            var element1 = new Element("anelementname1", autoCreate: false);
            element1.AddElement(element2);
            pattern.AddElement(element1);
            var draft = new DraftDefinition(new ToolkitDefinition(pattern));
            draft.Model.Properties["anelementname1"].Properties["anelementname2"].Materialise();
            var draftItem3 = draft.Model.Properties["anelementname1"].Properties["anelementname2"]
                .Properties["anelementname3"].Materialise();

            var result = draft.FindByAutomation(automation.Id);

            result.Should().ContainSingle(pair => pair.DraftItem == draftItem3);
        }

        [Fact]
        public void WhenFindByCodeTemplateOnUnMaterialisedDescendantElement_ThenReturnsNull()
        {
            var pattern = new PatternDefinition("apatternname");
            var element3 = new Element("anelementname3", autoCreate: false);
            var codeTemplate =
                new CodeTemplate("atemplatename", "afullpath", "afileextension");
            element3.AddCodeTemplate(codeTemplate);
            var element2 = new Element("anelementname2", autoCreate: false);
            element2.AddElement(element3);
            var element1 = new Element("anelementname1", autoCreate: false);
            element1.AddElement(element2);
            pattern.AddElement(element1);
            var draft = new DraftDefinition(new ToolkitDefinition(pattern));
            draft.Model.Properties["anelementname1"].Properties["anelementname2"].Materialise();

            var result = draft.FindByCodeTemplate(codeTemplate.Id);

            result.Should().BeNull();
        }

        [Fact]
        public void WhenFindByCodeTemplateOnMaterialisedDescendantElement_ThenReturnsNull()
        {
            var pattern = new PatternDefinition("apatternname");
            var element3 = new Element("anelementname3", autoCreate: false);
            var codeTemplate =
                new CodeTemplate("atemplatename", "afullpath", "afileextension");
            element3.AddCodeTemplate(codeTemplate);
            element3.AddCodeTemplate(codeTemplate);
            var element2 = new Element("anelementname2", autoCreate: false);
            element2.AddElement(element3);
            var element1 = new Element("anelementname1", autoCreate: false);
            element1.AddElement(element2);
            pattern.AddElement(element1);
            var draft = new DraftDefinition(new ToolkitDefinition(pattern));
            draft.Model.Properties["anelementname1"].Properties["anelementname2"].Materialise();
            var draftItem3 = draft.Model.Properties["anelementname1"].Properties["anelementname2"]
                .Properties["anelementname3"].Materialise();

            var result = draft.FindByCodeTemplate(codeTemplate.Id);

            result.Should().Be(draftItem3);
        }
#endif
    }
}