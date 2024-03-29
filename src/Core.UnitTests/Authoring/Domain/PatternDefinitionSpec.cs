﻿using System;
using System.Collections.Generic;
using Automate.Authoring.Domain;
using Automate.Common.Domain;
using Automate.Common.Extensions;
using FluentAssertions;
using Moq;
using Xunit;
using Attribute = Automate.Authoring.Domain.Attribute;

namespace Core.UnitTests.Authoring.Domain
{
    [Trait("Category", "Unit")]
    public class PatternDefinitionSpec
    {
        private PatternDefinition pattern;

        public PatternDefinitionSpec()
        {
            this.pattern = new PatternDefinition("aname");
        }

        [Fact]
        public void WhenConstructedWithOnlyName_ThenAssigned()
        {
            this.pattern = new PatternDefinition("aname");

            this.pattern.Name.Should().Be("aname");
            this.pattern.DisplayName.Should().Be("aname");
            this.pattern.Description.Should().BeNull();
            this.pattern.Id.Should().NotBeEmpty();
            this.pattern.ToolkitVersion.Current.Should().Be("0.0.0");
        }

        [Fact]
        public void WhenConstructed_ThenAssigned()
        {
            this.pattern =
                new PatternDefinition("aname", "adisplayname", "adescription");

            this.pattern.Name.Should().Be("aname");
            this.pattern.DisplayName.Should().Be("adisplayname");
            this.pattern.Description.Should().Be("adescription");
            this.pattern.Id.Should().NotBeEmpty();
            this.pattern.ToolkitVersion.Current.Should().Be("0.0.0");
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
            element1.AddElement(element2);
            this.pattern.AddElement(element1);

            var result = this.pattern.GetAllCodeTemplates();

            result.Should().BeEmpty();
        }

        [Fact]
        public void WhenGetAllCodeTemplatesAndFoundOnPattern_ThenReturnsAutomation()
        {
            var template = new CodeTemplate("aname", "afullpath", "anextension");
            this.pattern.AddCodeTemplate(template);

            var result = this.pattern.GetAllCodeTemplates();

            result.Count.Should().Be(1);
            result.Should().ContainSingle(tem => tem.Template.Id == template.Id && tem.Parent == this.pattern);
        }

        [Fact]
        public void WhenGetAllCodeTemplatesAndFoundOnDescendantElement_ThenReturnsAutomation()
        {
            var template1 = new CodeTemplate("aname1", "afullpath", "anextension");
            var template2 = new CodeTemplate("aname2", "afullpath", "anextension");
            var element1 = new Element("anelementname1");
            var element2 = new Element("anelementname2");
            element2.AddCodeTemplate(template1);
            element1.AddElement(element2);
            this.pattern.AddElement(element1);
            this.pattern.AddCodeTemplate(template2);

            var result = this.pattern.GetAllCodeTemplates();

            result.Count.Should().Be(2);
            result.Should().Contain(tem => tem.Template.Id == template1.Id && tem.Parent == element2);
            result.Should().Contain(tem => tem.Template.Id == template2.Id && tem.Parent == this.pattern);
        }

        [Fact]
        public void WhenGetAllCodeTemplatesAndFoundOnPatternAndDescendantElements_ThenReturnsAutomation()
        {
            var template1 = new CodeTemplate("aname1", "afullpath", "anextension");
            var template2 = new CodeTemplate("aname1", "afullpath", "anextension");
            var template3 = new CodeTemplate("aname2", "afullpath", "anextension");
            var element1 = new Element("anelementname1");
            var element2 = new Element("anelementname2");
            element2.AddCodeTemplate(template3);
            element1.AddElement(element2);
            element1.AddCodeTemplate(template2);
            this.pattern.AddElement(element1);
            this.pattern.AddCodeTemplate(template1);

            var result = this.pattern.GetAllCodeTemplates();

            result.Count.Should().Be(3);
            result.Should().Contain(tem => tem.Template.Id == template1.Id && tem.Parent == this.pattern);
            result.Should().Contain(tem => tem.Template.Id == template2.Id && tem.Parent == element1);
            result.Should().Contain(tem => tem.Template.Id == template3.Id && tem.Parent == element2);
        }

        [Fact]
        public void WhenFindAutomationAndNone_ThenReturnsNull()
        {
            var result = this.pattern.FindAutomation("acmdid", _ => true);

            result.Should().BeNull();
        }

        [Fact]
        public void WhenFindAutomationAndNotExistsOnDescendant_ThenReturnsNull()
        {
            var element1 = new Element("anelementname1");
            var element2 = new Element("anelementname2");
            element1.AddElement(element2);
            this.pattern.AddElement(element1);

            var result = this.pattern.FindAutomation("acmdid", _ => true);

            result.Should().BeNull();
        }

        [Fact]
        public void WhenCreateTestDraftAndOnlyPattern_ThenReturnsTestPattern()
        {
            var result = this.pattern.CreateTestDraft();

            result.PatternName.Should().Be("aname");
            result.Toolkit.Version.Should().Be("0.0.0");
            result.Toolkit.CodeTemplateFiles.Should().BeEmpty();
            result.Model.Name.Should().Be("aname");
            result.Model.Properties.Should().BeEmpty();
            result.Model.Items.Should().BeNull();
        }

        [Fact]
        public void WhenCreateTestDraftAndDescendantElements_ThenReturnsTestPattern()
        {
            var date = DateTime.UtcNow;
            var element1 = new Element("anelement1");
            var element2 = new Element("anelement2");
            var element3 = new Element("anelement3");
            var attribute1 = new Attribute("anattribute1", "string", false, "adefaultvalue");
            var attribute2 = new Attribute("anattribute2", "int", false, "9");
            var attribute3 = new Attribute("anattribute3", "bool", false, "true");
            var attribute4 = new Attribute("anattribute4", "float", false, "9.9");
            var attribute5 = new Attribute("anattribute5", "datetime", false, date.ToIso8601());
            var attribute6 = new Attribute("anattribute6", "string", false, "B", new List<string> { "A", "B" });
            element3.AddAttributes(attribute1, attribute2, attribute3, attribute4, attribute5, attribute6);
            element2.AddElement(element3);
            element1.AddElement(element2);
            this.pattern.AddElement(element1);

            var result = this.pattern.CreateTestDraft();

            result.Model.Properties.Should().ContainSingle(x => x.Value.Name == "anelement1");
            result.Model.Properties["anelement1"].Properties.Should().ContainSingle(x => x.Value.Name == "anelement2");
            result.Model.Properties["anelement1"].Properties["anelement2"].Properties.Should()
                .ContainSingle(x => x.Value.Name == "anelement3");
            result.Model.Properties["anelement1"].Properties["anelement2"].Properties["anelement3"]
                .Properties["anattribute1"].Value.Should().Be("adefaultvalue");
            result.Model.Properties["anelement1"].Properties["anelement2"].Properties["anelement3"]
                .Properties["anattribute2"].Value.Should().Be(9);
            result.Model.Properties["anelement1"].Properties["anelement2"].Properties["anelement3"]
                .Properties["anattribute3"].Value.Should().Be(true);
            result.Model.Properties["anelement1"].Properties["anelement2"].Properties["anelement3"]
                .Properties["anattribute4"].Value.Should().Be(9.9M);
            result.Model.Properties["anelement1"].Properties["anelement2"].Properties["anelement3"]
                .Properties["anattribute5"].Value.Should().Be(date);
            result.Model.Properties["anelement1"].Properties["anelement2"].Properties["anelement3"]
                .Properties["anattribute6"].Value.Should().Be("B");
        }

        [Fact]
        public void WhenCreateTestDraftAndDescendantCollections_ThenReturnsTestPattern()
        {
            var collection1 =
                new Element("acollection1", ElementCardinality.OneOrMany);
            var collection2 =
                new Element("acollection2", ElementCardinality.OneOrMany);
            var collection3 =
                new Element("acollection3", ElementCardinality.OneOrMany);
            var attribute1 = new Attribute("anattribute1");
            var attribute2 = new Attribute("anattribute2", "int");
            var attribute3 = new Attribute("anattribute3", "bool");
            var attribute4 = new Attribute("anattribute4", "float");
            var attribute5 = new Attribute("anattribute5", "datetime");
            var attribute6 = new Attribute("anattribute6", "string", false, null, new List<string> { "A", "B" });
            collection3.AddAttributes(attribute1, attribute2, attribute3, attribute4, attribute5, attribute6);
            collection2.AddElement(collection3);
            collection1.AddElement(collection2);
            this.pattern.AddElement(collection1);

            var result = this.pattern.CreateTestDraft();

            result.Model.Properties.Should().ContainSingle(x => x.Value.Name == "acollection1");
            result.Model.Properties["acollection1"].Items.Count.Should().Be(3);
            result.Model.Properties["acollection1"].Items[0].Properties["acollection2"].Items.Count.Should().Be(3);
            result.Model.Properties["acollection1"].Items[1].Properties["acollection2"].Items.Count.Should().Be(3);
            result.Model.Properties["acollection1"].Items[2].Properties["acollection2"].Items.Count.Should().Be(3);
            var firstCollection3Items = result.Model.Properties["acollection1"].Items[0].Properties["acollection2"]
                .Items[0]
                .Properties["acollection3"].Items;
            firstCollection3Items.Count.Should().Be(3);
            firstCollection3Items[0].Properties["anattribute1"].Value.Should().Be("anattribute11");
            firstCollection3Items[0].Properties["anattribute2"].Value.Should().Be(1);
            firstCollection3Items[0].Properties["anattribute3"].Value.Should().Be(true);
            firstCollection3Items[0].Properties["anattribute4"].Value.Should().Be(1.1M);
            firstCollection3Items[0].Properties["anattribute5"].Value.As<DateTime>().Should()
                .BeCloseTo(DateTime.Today.AddHours(1).ToUniversalTime(), TimeSpan.FromSeconds(1));
            firstCollection3Items[0].Properties["anattribute6"].Value.Should().Be("A");
            firstCollection3Items[1].Properties["anattribute1"].Value.Should().Be("anattribute12");
            firstCollection3Items[1].Properties["anattribute2"].Value.Should().Be(2);
            firstCollection3Items[1].Properties["anattribute3"].Value.Should().Be(false);
            firstCollection3Items[1].Properties["anattribute4"].Value.Should().Be(2.2M);
            firstCollection3Items[1].Properties["anattribute5"].Value.As<DateTime>().Should()
                .BeCloseTo(DateTime.Today.AddHours(2).ToUniversalTime(), TimeSpan.FromSeconds(1));
            firstCollection3Items[1].Properties["anattribute6"].Value.Should().Be("B");
            firstCollection3Items[2].Properties["anattribute1"].Value.Should().Be("anattribute13");
            firstCollection3Items[2].Properties["anattribute2"].Value.Should().Be(3);
            firstCollection3Items[2].Properties["anattribute3"].Value.Should().Be(true);
            firstCollection3Items[2].Properties["anattribute4"].Value.Should().Be(3.3M);
            firstCollection3Items[2].Properties["anattribute5"].Value.As<DateTime>().Should()
                .BeCloseTo(DateTime.Today.AddHours(3).ToUniversalTime(), TimeSpan.FromSeconds(1));
            firstCollection3Items[2].Properties["anattribute6"].Value.Should().Be("A");

            result.Model.Properties["acollection1"].Items[0].Properties["acollection2"].Items[1]
                .Properties["acollection3"].Items.Count.Should().Be(3);
            result.Model.Properties["acollection1"].Items[0].Properties["acollection2"].Items[2]
                .Properties["acollection3"].Items.Count.Should().Be(3);
            result.Model.Properties["acollection1"].Items[1].Properties["acollection2"].Items[0]
                .Properties["acollection3"].Items.Count.Should().Be(3);
            result.Model.Properties["acollection1"].Items[1].Properties["acollection2"].Items[1]
                .Properties["acollection3"].Items.Count.Should().Be(3);
            result.Model.Properties["acollection1"].Items[1].Properties["acollection2"].Items[2]
                .Properties["acollection3"].Items.Count.Should().Be(3);
            result.Model.Properties["acollection1"].Items[2].Properties["acollection2"].Items[0]
                .Properties["acollection3"].Items.Count.Should().Be(3);
            result.Model.Properties["acollection1"].Items[2].Properties["acollection2"].Items[1]
                .Properties["acollection3"].Items.Count.Should().Be(3);
            result.Model.Properties["acollection1"].Items[2].Properties["acollection2"].Items[2]
                .Properties["acollection3"].Items.Count.Should().Be(3);
        }

        [Fact]
        public void WhenSyncRuntimeChangesAndNoRuntimeVersion_ThenDoesNothing()
        {
            var installedMachineVersion = MachineConstants.GetRuntimeVersion();
            var metadata = new Mock<IRuntimeMetadata>();
            metadata.Setup(m => m.RuntimeVersion)
                .Returns(installedMachineVersion.NextMajor());
#if TESTINGONLY
            this.pattern.ResetRuntimeVersion();
#endif

            this.pattern.SyncRuntimeChanges(metadata.Object);

            metadata.VerifyGet(m => m.RuntimeVersion, Times.Never);
            this.pattern.RuntimeVersion.Should().BeNull();
        }

        [Fact]
        public void WhenSyncRuntimeChangesAndRuntimeVersionSameAsMachine_ThenDoesNothing()
        {
            var installedMachineVersion = MachineConstants.GetRuntimeVersion();
            var metadata = new Mock<IRuntimeMetadata>();
            metadata.Setup(m => m.RuntimeVersion)
                .Returns(installedMachineVersion);

            this.pattern.SyncRuntimeChanges(metadata.Object);

            metadata.VerifyGet(m => m.RuntimeVersion);
            this.pattern.ToolkitVersion.ChangeLog.Should().BeEmpty();
            this.pattern.RuntimeVersion.Should().Be(installedMachineVersion.ToString());
        }

        [Fact]
        public void WhenSyncRuntimeChangesAndRuntimeVersionSameMajorAsMachine_ThenUpdates()
        {
            var installedMachineVersion = MachineConstants.GetRuntimeVersion();
            var nextMinor = installedMachineVersion.NextMinor();
            var metadata = new Mock<IRuntimeMetadata>();
            metadata.Setup(m => m.RuntimeVersion)
                .Returns(nextMinor);

            this.pattern.SyncRuntimeChanges(metadata.Object);

            metadata.VerifyGet(m => m.RuntimeVersion);
            this.pattern.ToolkitVersion.ChangeLog.Should().BeEmpty();
            this.pattern.RuntimeVersion.Should().Be(nextMinor.ToString());
        }

        [Fact]
        public void WhenSyncRuntimeChangesAndRuntimeVersionLessThanMachine_ThenRegistersChangeAndUpdates()
        {
            var installedMachineVersion = MachineConstants.GetRuntimeVersion();
            var nextMajor = installedMachineVersion.NextMajor();
            var metadata = new Mock<IRuntimeMetadata>();
            metadata.Setup(m => m.RuntimeVersion)
                .Returns(nextMajor);

            this.pattern.SyncRuntimeChanges(metadata.Object);

            metadata.VerifyGet(m => m.RuntimeVersion);
            this.pattern.ToolkitVersion.ChangeLog.Should().ContainSingle(x =>
                x.MessageTemplate == VersionChanges.Pattern_RuntimeVersion_Update &&
                x.Change == VersionChange.NonBreaking);
            this.pattern.RuntimeVersion.Should().Be(nextMajor.ToString());
        }
#if TESTINGONLY
        [Fact]
        public void WhenFindAutomationAndFoundOnPattern_ThenReturnsAutomation()
        {
            var automation =
                new Automation("acommandname", AutomationType.TestingOnlyLaunchable,
                    new Dictionary<string, object>());
            this.pattern.AddAutomation(automation);

            var result = this.pattern.FindAutomation(automation.Id, _ => true);

            result.Should().Be(automation);
        }

        [Fact]
        public void WhenFindAutomationAndFoundOnDescendantElement_ThenReturnsAutomation()
        {
            var automation =
                new Automation("acommandname", AutomationType.TestingOnlyLaunchable,
                    new Dictionary<string, object>());
            var element1 = new Element("anelementname1");
            var element2 = new Element("anelementname2");
            element2.AddAutomation(automation);
            element1.AddElement(element2);
            this.pattern.AddElement(element1);

            var result = this.pattern.FindAutomation(automation.Id, _ => true);

            result.Should().Be(automation);
        }
#endif
    }
}