using System;
using System.Collections.Generic;
using System.Linq;
using Automate.CLI;
using Automate.CLI.Domain;
using Automate.CLI.Extensions;
using Automate.CLI.Infrastructure;
using FluentAssertions;
using Xunit;
using Attribute = Automate.CLI.Domain.Attribute;

namespace CLI.UnitTests.Domain
{
    [Trait("Category", "Unit")]
    public class SolutionItemSpec
    {
        private readonly PatternDefinition pattern;
        private readonly ToolkitDefinition toolkit;

        public SolutionItemSpec()
        {
            this.pattern = new PatternDefinition("apatternname");
            this.toolkit = new ToolkitDefinition(this.pattern);
        }

        [Fact]
        public void WhenConstructedWithPattern_ThenPatternAssigned()
        {
            var result = new SolutionItem(this.toolkit, this.pattern);

            result.Id.Should().NotBeNull();
            result.Should().NotBeNull();
            result.Value.Should().BeNull();
            result.IsMaterialised.Should().BeTrue();
            result.Parent.Should().BeNull();
        }

        [Fact]
        public void WhenConstructedWithAttributeWithoutDefaultValue_ThenAttributeAssigned()
        {
            var attribute = new Attribute("aname");
            this.pattern.AddAttribute(attribute);

            var result = new SolutionItem(this.toolkit, this.pattern);

            result.Should().NotBeNull();
            result.Id.Should().NotBeNull();
            result.Properties["aname"].AttributeSchema.Object.Should().Be(attribute);
            result.Properties["aname"].Value.Should().BeNull();
            result.Properties["aname"].IsMaterialised.Should().BeFalse();
        }

        [Fact]
        public void WhenConstructedWithAttributeWithDefaultValue_ThenAttributeAssigned()
        {
            var attribute = new Attribute("aname", "string", false, "adefaultvalue");
            this.pattern.AddAttribute(attribute);

            var result = new SolutionItem(this.toolkit, this.pattern);

            result.Should().NotBeNull();
            result.Id.Should().NotBeNull();
            result.Properties["aname"].AttributeSchema.Object.Should().Be(attribute);
            result.Properties["aname"].Value.Should().Be(attribute.DefaultValue);
            result.Properties["aname"].IsMaterialised.Should().BeTrue();
        }

        [Fact]
        public void WhenConstructedWithAttributeWithDateTimeDefaultValue_ThenAttributeAssignedUtcDateTime()
        {
            var date = DateTime.UtcNow;
            var attribute = new Attribute("aname", "DateTime", false, date.ToIso8601());
            this.pattern.AddAttribute(attribute);

            var result = new SolutionItem(this.toolkit, this.pattern);

            result.Should().NotBeNull();
            result.Id.Should().NotBeNull();
            result.Properties["aname"].AttributeSchema.Object.Should().Be(attribute);
            result.Properties["aname"].Value.Should().Be(date);
            result.Properties["aname"].IsMaterialised.Should().BeTrue();
        }

        [Fact]
        public void WhenConstructedWithElement_ThenElementAssigned()
        {
            var element = new Element("anelementname", autoCreate: false, displayName: "adisplayname",
                description: "adescription");
            element.AddAttribute(new Attribute("anattributename", "string", false, "adefaultvalue"));
            this.pattern.AddElement(element);

            var result = new SolutionItem(this.toolkit, this.pattern);

            result.Should().NotBeNull();
            result.Id.Should().NotBeNull();
            result.Properties["anelementname"].ElementSchema.Object.Should().Be(element);
            result.Properties["anelementname"].Value.Should().BeNull();
            result.Properties["anelementname"].IsMaterialised.Should().BeFalse();
            result.Properties["anelementname"].Items.Should().BeNull();
        }

        [Fact]
        public void WhenConstructedWithCollection_ThenCollectionAssigned()
        {
            var element = new Element("acollectionname", autoCreate: false, displayName: "adisplayname",
                description: "adescription");
            element.AddAttribute(new Attribute("anattributename", "string", false, "adefaultvalue"));
            this.pattern.AddElement(element);

            var result = new SolutionItem(this.toolkit, this.pattern);

            result.Should().NotBeNull();
            result.Id.Should().NotBeNull();
            result.Properties["acollectionname"].ElementSchema.Object.Should().Be(element);
            result.Properties["acollectionname"].Value.Should().BeNull();
            result.Properties["acollectionname"].IsMaterialised.Should().BeFalse();
            result.Properties["acollectionname"].Items.Should().BeNull();
        }

        [Fact]
        public void WhenConstructedWithDescendantSchema_ThenDescendantElementsAssigned()
        {
            var element3 = new Element("anelementname3", autoCreate: false, displayName: "adisplayname3",
                description: "adescription3");
            element3.AddAttribute(new Attribute("anattributename3", "string", false, "adefaultvalue3"));
            var element2 = new Element("anelementname2", autoCreate: false, displayName: "adisplayname2",
                description: "adescription2");
            element2.AddAttribute(new Attribute("anattributename2", "string", false, "adefaultvalue2"));
            var element1 = new Element("anelementname1", autoCreate: false, displayName: "adisplayname1",
                description: "adescription1");
            element1.AddAttribute(new Attribute("anattributename1", "string", false, "adefaultvalue1"));
            element2.AddElement(element3);
            element1.AddElement(element2);
            this.pattern.AddElement(element1);

            var result = new SolutionItem(this.toolkit, this.pattern);

            result.Should().NotBeNull();
            result.Id.Should().NotBeNull();
            var solutionElement1 = result.Properties["anelementname1"];
            solutionElement1.ElementSchema.Object.Should().Be(element1);
            solutionElement1.Value.Should().BeNull();
            solutionElement1.IsMaterialised.Should().BeFalse();
            solutionElement1.Items.Should().BeNull();
            solutionElement1.Properties.Should().BeNull();
        }

        [Fact]
        public void WhenMaterialiseAndPattern_ThenThrows()
        {
            new SolutionItem(this.toolkit, this.pattern)
                .Invoking(x => x.Materialise())
                .Should().Throw<AutomateException>()
                .WithMessage(ExceptionMessages.SolutionItem_PatternAlreadyMaterialised.Format("apatternname"));
        }

        [Fact]
        public void WhenMaterialiseAndElement_ThenMaterialises()
        {
            var element = new Element("anelementname", displayName: "adisplayname", description: "adescription");
            var attribute = new Attribute("anattributename", null, false, "adefaultvalue");
            element.AddAttribute(attribute);
            this.pattern.AddElement(element);

            var result = new SolutionItem(this.toolkit, element, null)
                .Materialise();

            result.Id.Should().NotBeNull();
            result.IsMaterialised.Should().BeTrue();
            result.Value.Should().BeNull();
            result.Properties["anattributename"].Value.Should().Be("adefaultvalue");
            result.Items.Should().BeNull();
        }

        [Fact]
        public void WhenMaterialiseAndElementHasAutoCreateElement_ThenMaterialisesChildElement()
        {
            var element1 = new Element("anelementname1", displayName: "adisplayname", description: "adescription");
            var element2 = new Element("anelementname2", autoCreate: true, displayName: "adisplayname",
                description: "adescription");
            var attribute = new Attribute("anattributename", null, false, "adefaultvalue");
            element1.AddAttribute(attribute);
            element1.AddElement(element2);
            this.pattern.AddElement(element1);

            var result = new SolutionItem(this.toolkit, element1, null)
                .Materialise();

            result.Id.Should().NotBeNull();
            result.IsMaterialised.Should().BeTrue();
            result.Value.Should().BeNull();
            result.Properties["anattributename"].Value.Should().Be("adefaultvalue");
            result.Properties["anelementname2"].IsMaterialised.Should().BeTrue();
            result.Items.Should().BeNull();
        }

        [Fact]
        public void WhenMaterialiseAndElementHasAutoCreateCollection_ThenMaterialisesChildCollection()
        {
            var element1 = new Element("anelementname1", displayName: "adisplayname", description: "adescription");
            var collection1 = new Element("acollectionname1", ElementCardinality.OneOrMany, true, "adisplayname",
                "adescription");
            var attribute = new Attribute("anattributename", null, false, "adefaultvalue");
            element1.AddAttribute(attribute);
            element1.AddElement(collection1);
            this.pattern.AddElement(element1);

            var result = new SolutionItem(this.toolkit, element1, null)
                .Materialise();

            result.Id.Should().NotBeNull();
            result.IsMaterialised.Should().BeTrue();
            result.Value.Should().BeNull();
            result.Properties["anattributename"].Value.Should().Be("adefaultvalue");
            result.Properties["acollectionname1"].IsMaterialised.Should().BeTrue();
            result.Items.Should().BeNull();
        }

        [Fact]
        public void WhenMaterialiseAndCollection_ThenMaterialises()
        {
            var collection = new Element("acollectionname", ElementCardinality.OneOrMany);
            var attribute = new Attribute("anattributename", null, false, "adefaultvalue");
            var element = new Element("anelementname");
            collection.AddAttribute(attribute);
            collection.AddElement(element);
            this.pattern.AddElement(collection);

            var result = new SolutionItem(this.toolkit, collection, null)
                .Materialise();

            result.Id.Should().NotBeNull();
            result.IsMaterialised.Should().BeTrue();
            result.Value.Should().BeNull();
            result.Properties.Should().BeEmpty();
            result.Items.Should().BeEmpty();
        }

        [Fact]
        public void WhenMaterialiseAndAttribute_ThenMaterialises()
        {
            var attribute = new Attribute("anattributename", null, false, "adefaultvalue");
            this.pattern.AddAttribute(attribute);

            var result = new SolutionItem(this.toolkit, attribute, null)
                .Materialise("avalue");

            result.Id.Should().NotBeNull();
            result.IsMaterialised.Should().BeTrue();
            result.Value.Should().Be("avalue");
        }

        [Fact]
        public void WhenMaterialiseCollectionItemAndNotACollection_ThenThrows()
        {
            var element = new Element("anelementname", displayName: "adisplayname", description: "adescription");

            new SolutionItem(this.toolkit, element, null)
                .Invoking(x => x.MaterialiseCollectionItem())
                .Should().Throw<AutomateException>()
                .WithMessage(ExceptionMessages.SolutionItem_MaterialiseNotACollection);
        }

        [Fact]
        public void WhenMaterialiseCollectionItem_ThenMaterialisesNewElement()
        {
            var collection = new Element("acollectionname", ElementCardinality.OneOrMany);
            var attribute = new Attribute("anattributename", defaultValue: "adefaultvalue");
            collection.AddAttribute(attribute);
            this.pattern.AddElement(collection);

            var solutionItem = new SolutionItem(this.toolkit, collection, null);

            var result = solutionItem.MaterialiseCollectionItem();

            result.Id.Should().NotBeNull();
            result.IsMaterialised.Should().BeTrue();
            result.IsEphemeralCollection.Should().BeFalse();
            result.ElementSchema.Name.Should().Be("acollectionname");
            result.ElementSchema.HasCardinalityOfAtLeastOne().Should().BeTrue();
            result.ElementSchema.HasCardinalityOfAtMostOne().Should().BeTrue();
            result.Value.Should().BeNull();
            result.Items.Should().BeNull();
            result.Properties.Should().ContainSingle(prop =>
                prop.Key == "anattributename" && (string)prop.Value.Value == "adefaultvalue");

            solutionItem.IsMaterialised.Should().BeTrue();
            solutionItem.Items.Should().Contain(result);
        }

        [Fact]
        public void WhenMaterialiseCollectionItemAndHasChildCollection_ThenMaterialisesNewElement()
        {
            var collection1 = new Element("acollectionname1", ElementCardinality.OneOrMany);
            var attribute = new Attribute("anattributename", defaultValue: "adefaultvalue");
            collection1.AddAttribute(attribute);
            var collection2 = new Element("acollectionname2", ElementCardinality.OneOrMany);
            collection1.AddElement(collection2);
            this.pattern.AddElement(collection1);

            var solutionItem = new SolutionItem(this.toolkit, collection1, null);

            var result1 = solutionItem.MaterialiseCollectionItem();

            result1.Id.Should().NotBeNull();
            result1.IsMaterialised.Should().BeTrue();
            result1.IsEphemeralCollection.Should().BeFalse();
            result1.ElementSchema.Name.Should().Be("acollectionname1");
            result1.ElementSchema.HasCardinalityOfAtLeastOne().Should().BeTrue();
            result1.ElementSchema.HasCardinalityOfAtMostOne().Should().BeTrue();
            result1.Value.Should().BeNull();
            result1.Items.Should().BeNull();
            result1.Properties.Should().Contain(prop =>
                prop.Key == "anattributename" && (string)prop.Value.Value == "adefaultvalue");
            result1.Properties.Should().Contain(prop =>
                prop.Key == "acollectionname2" && prop.Value.IsEphemeralCollection == true);

            var result2 = result1.Properties["acollectionname2"].MaterialiseCollectionItem();
            result2.IsMaterialised.Should().BeTrue();
            result2.IsEphemeralCollection.Should().BeFalse();
            result2.ElementSchema.Name.Should().Be("acollectionname2");
            result2.ElementSchema.HasCardinalityOfAtLeastOne().Should().BeTrue();
            result2.ElementSchema.HasCardinalityOfAtMostOne().Should().BeTrue();
            result2.Value.Should().BeNull();
            result2.Items.Should().BeNull();
            result2.Properties.Should().BeEmpty();

            solutionItem.IsMaterialised.Should().BeTrue();
            solutionItem.Items.Single().Should().Be(result1);
            solutionItem.Items.Single().Properties["acollectionname2"].Items.Single().Should().Be(result2);
        }

        [Fact]
        public void WhenHasAttributeAndPropertyInPatternSchema_ThenReturnsTrue()
        {
            var attribute = new Attribute("anattributename", null, false, "adefaultvalue");
            this.pattern.AddAttribute(attribute);

            var result = new SolutionItem(this.toolkit, this.pattern)
                .HasAttribute("anattributename");

            result.Should().BeTrue();
        }

        [Fact]
        public void WhenHasAttributeAndPropertyInElementSchema_ThenReturnsTrue()
        {
            var element = new Element("anelementname", displayName: "adisplayname", description: "adescription");
            var attribute = new Attribute("anattributename", null, false, "adefaultvalue");
            element.AddAttribute(attribute);
            this.pattern.AddElement(element);

            var result = new SolutionItem(this.toolkit, element, null)
                .HasAttribute("anattributename");

            result.Should().BeTrue();
        }

        [Fact]
        public void WhenHasAttributeAndPropertyNotInSchema_ThenReturnsFalse()
        {
            var element = new Element("anelementname", displayName: "adisplayname", description: "adescription");
            this.pattern.AddElement(element);

            var result = new SolutionItem(this.toolkit, element, null)
                .HasAttribute("anattributename");

            result.Should().BeFalse();
        }

        [Fact]
        public void WhenHasAttributeAndAttribute_ThenReturnsFalse()
        {
            var attribute = new Attribute("anattributename", null);
            this.pattern.AddAttribute(attribute);

            var result = new SolutionItem(this.toolkit, attribute, null)
                .HasAttribute("anattributename");

            result.Should().BeFalse();
        }

        [Fact]
        public void WhenGetPropertyAndNotAnAttribute_ThenThrows()
        {
            var element = new Element("anelementname");
            this.pattern.AddElement(element);

            new SolutionItem(this.toolkit, this.pattern)
                .Invoking(x => x.GetProperty("anelementname"))
                .Should().Throw<AutomateException>()
                .WithMessage(ExceptionMessages.SolutionItem_NotAnAttribute.Format("anelementname"));
        }

        [Fact]
        public void WhenGetPropertyAndNotMaterialised_ThenThrows()
        {
            var element = new Element("anelementname");
            var attribute = new Attribute("anattributename", null, false, "adefaultvalue");
            element.AddAttribute(attribute);

            new SolutionItem(this.toolkit, element, null)
                .Invoking(x => x.GetProperty("anattributename"))
                .Should().Throw<AutomateException>()
                .WithMessage(ExceptionMessages.SolutionItem_NotMaterialised);
        }

        [Fact]
        public void WhenGetPropertyAndMaterialised_ThenReturnsProperty()
        {
            var element = new Element("anelementname");
            var attribute = new Attribute("anattributename", null, false, "adefaultvalue");
            element.AddAttribute(attribute);
            this.pattern.AddElement(element);

            var result = new SolutionItem(this.toolkit, element, null)
                .Materialise()
                .GetProperty("anattributename");

            result.Name.Should().Be("anattributename");
        }

        [Fact]
        public void WhenValidateAndIsPatternWithMissingRequiredAttribute_ThenReturnsErrors()
        {
            this.pattern.AddAttribute(new Attribute("anattributename", isRequired: true));

            var result = new SolutionItem(this.toolkit, this.pattern)
                .Validate();

            result.Results.Single().Context.Path.Should().Be("{apatternname.anattributename}");
            result.Results.Single().Message.Should()
                .Be(ValidationMessages.Attribute_ValidationRule_RequiredAttributeValue.Format("anattributename"));
        }

        [Fact]
        public void WhenValidateAndIsOptionalElementAndNotMaterialised_ThenSucceeds()
        {
            var element = new Element("anelementname", ElementCardinality.ZeroOrOne);
            this.pattern.AddElement(element);

            var result = new SolutionItem(this.toolkit, this.pattern)
                .Properties["anelementname"].Validate();

            result.Results.Should().BeEmpty();
        }

        [Fact]
        public void WhenValidateAndIsOneOnlyOneElementAndNotMaterialised_ThenReturnsErrors()
        {
            var element = new Element("anelementname", autoCreate: false);
            this.pattern.AddElement(element);

            var result = new SolutionItem(this.toolkit, this.pattern)
                .Properties["anelementname"].Validate();

            result.Results.Single().Context.Path.Should().Be("{apatternname.anelementname}");
            result.Results.Single().Message.Should()
                .Be(ValidationMessages.SolutionItem_ValidationRule_ElementRequiresAtLeastOneInstance.Format(
                    "anelementname"));
        }

        [Fact]
        public void WhenValidateAndIsCollectionWithZeroAndNotMaterialised_ThenSucceeds()
        {
            var element = new Element("acollectionname", ElementCardinality.ZeroOrMany);
            this.pattern.AddElement(element);

            var result = new SolutionItem(this.toolkit, element, new SolutionItem(this.toolkit, this.pattern))
                .Validate();

            result.Results.Should().BeEmpty();
        }

        [Fact]
        public void WhenValidateAndIsCollectionWithOneAndNotMaterialised_ThenReturnsErrors()
        {
            var element = new Element("acollectionname", ElementCardinality.OneOrMany);
            this.pattern.AddElement(element);

            var result = new SolutionItem(this.toolkit, element, new SolutionItem(this.toolkit, this.pattern))
                .Validate();

            result.Results.Single().Context.Path.Should().Be("{apatternname.acollectionname}");
            result.Results.Single().Message.Should()
                .Be(ValidationMessages.SolutionItem_ValidationRule_ElementRequiresAtLeastOneInstance.Format(
                    "acollectionname"));
        }

        [Fact]
        public void WhenValidateAndIsElementWithMissingRequiredAttribute_ThenReturnsErrors()
        {
            var element = new Element("anelementname");
            element.AddAttribute(new Attribute("anattributename", isRequired: true));
            this.pattern.AddElement(element);

            var result = new SolutionItem(this.toolkit, element, new SolutionItem(this.toolkit, this.pattern))
                .Materialise()
                .Validate();

            result.Results.Single().Context.Path.Should().Be("{apatternname.anelementname.anattributename}");
            result.Results.Single().Message.Should()
                .Be(ValidationMessages.Attribute_ValidationRule_RequiredAttributeValue.Format("anattributename"));
        }

        [Fact]
        public void WhenValidateAndIsElementWithRequiredItems_ThenReturnsErrors()
        {
            var element1 = new Element("anelementname1", autoCreate: false);
            var element2 = new Element("anelementname2", autoCreate: false);
            element1.AddElement(element2);
            this.pattern.AddElement(element1);

            var result = new SolutionItem(this.toolkit, element1, new SolutionItem(this.toolkit, this.pattern))
                .Materialise()
                .Validate();

            result.Results.Single().Context.Path.Should().Be("{apatternname.anelementname1.anelementname2}");
            result.Results.Single().Message.Should()
                .Be(ValidationMessages.SolutionItem_ValidationRule_ElementRequiresAtLeastOneInstance.Format(
                    "anelementname2"));
        }

        [Fact]
        public void WhenValidateAndIsCollectionWithRequiredItems_ThenReturnsErrors()
        {
            var collection = new Element("acollectionname",
                ElementCardinality.OneOrMany);
            this.pattern.AddElement(collection);

            var result = new SolutionItem(this.toolkit, collection, new SolutionItem(this.toolkit, this.pattern))
                .Materialise()
                .Validate();

            result.Results.Single().Context.Path.Should().Be("{apatternname.acollectionname}");
            result.Results.Single().Message.Should()
                .Be(ValidationMessages.SolutionItem_ValidationRule_ElementRequiresAtLeastOneInstance.Format(
                    "acollectionname"));
        }

        [Fact]
        public void WhenValidateAndIsDescendantCollectionWithMissingRequiredAttribute_ThenReturnsErrors()
        {
            var collection = new Element("acollectionname", ElementCardinality.OneOrMany, false);
            var element = new Element("anelementname", autoCreate: false);
            var attribute = new Attribute("anattributename", isRequired: true);
            element.AddAttribute(attribute);
            collection.AddElement(element);
            this.pattern.AddElement(collection);

            var solutionItem = new SolutionItem(this.toolkit, collection, new SolutionItem(this.toolkit, this.pattern));
            solutionItem.MaterialiseCollectionItem();

            var result = solutionItem.Validate();

            result.Results.Single().Context.Path.Should()
                .Be($"{{apatternname.acollectionname.{solutionItem.Items.Single().Id}.anelementname}}");
            result.Results.Single().Message.Should()
                .Be(ValidationMessages.SolutionItem_ValidationRule_ElementRequiresAtLeastOneInstance);
        }

        [Fact]
        public void WhenValidateAndIsAttributeWithMissingRequiredValue_ThenReturnsErrors()
        {
            var attribute = new Attribute("anattributename", isRequired: true);
            this.pattern.AddAttribute(attribute);

            var result = new SolutionItem(this.toolkit, attribute, new SolutionItem(this.toolkit, this.pattern))
                .Validate();

            result.Results.Single().Context.Path.Should().Be("{apatternname.anattributename}");
            result.Results.Single().Message.Should()
                .Be(ValidationMessages.Attribute_ValidationRule_RequiredAttributeValue.Format("anattributename"));
        }

        [Fact]
        public void WhenValidateAndIsAttributeWithWrongDataTypeValue_ThenReturnsErrors()
        {
            var attribute = new Attribute("anattributename");
            this.pattern.AddAttribute(attribute);
            var solutionItem = new SolutionItem(this.toolkit, attribute, new SolutionItem(this.toolkit, this.pattern))
            {
                Value = "awrongvalue"
            };
            attribute.ResetDataType("int");

            var result = solutionItem
                .Validate();

            result.Results.Single().Context.Path.Should().Be("{apatternname.anattributename}");
            result.Results.Single().Message.Should()
                .Be(ValidationMessages.Attribute_ValidationRule_WrongDataTypeValue.Format("awrongvalue", "int"));
        }

        [Fact]
        public void WhenGetConfiguration_ThenReturnsConfiguration()
        {
            var attribute1 = new Attribute("anattributename1", null, false, "adefaultvalue1");
            var attribute2 = new Attribute("anattributename2", null, false, "adefaultvalue2");
            var attribute3 = new Attribute("anattributename3", "int", false, "25");
            var elementLevel1 =
                new Element("anelementname1", displayName: "adisplayname1", description: "adescription1");
            var elementLevel2 =
                new Element("anelementname2", displayName: "adisplayname2", description: "adescription2");
            var collectionLevel1 = new Element("acollectionname2", ElementCardinality.OneOrMany, false, "adisplayname1",
                "adescription1");
            elementLevel2.AddAttribute(attribute2);
            collectionLevel1.AddAttribute(attribute3);
            elementLevel1.AddElement(elementLevel2);
            this.pattern.AddAttribute(attribute1);
            this.pattern.AddElement(elementLevel1);
            this.pattern.AddElement(collectionLevel1);

            var solutionItem = new SolutionItem(this.toolkit, this.pattern);
            solutionItem.Properties["anelementname1"].Materialise();
            solutionItem.Properties["anelementname1"].Properties["anelementname2"].Materialise();
            solutionItem.Properties["acollectionname2"].MaterialiseCollectionItem();

            var result = solutionItem.GetConfiguration(false).ToJson();

            result.Should().Be(new Dictionary<string, object>
            {
                { nameof(SolutionItem.Id), solutionItem.Id },
                { "anattributename1", "adefaultvalue1" },
                {
                    "anelementname1", new Dictionary<string, object>
                    {
                        { nameof(SolutionItem.Id), solutionItem.Properties["anelementname1"].Id },
                        {
                            "anelementname2", new Dictionary<string, object>
                            {
                                {
                                    nameof(SolutionItem.Id),
                                    solutionItem.Properties["anelementname1"].Properties["anelementname2"].Id
                                },
                                { "anattributename2", "adefaultvalue2" }
                            }
                        }
                    }
                },
                {
                    "acollectionname2", new Dictionary<string, object>
                    {
                        { nameof(SolutionItem.Id), solutionItem.Properties["acollectionname2"].Id },
                        {
                            nameof(SolutionItem.Items), new List<object>
                            {
                                new Dictionary<string, object>
                                {
                                    {
                                        nameof(SolutionItem.Id), solutionItem.Properties["acollectionname2"].Items[0].Id
                                    },
                                    { "anattributename3", 25 }
                                }
                            }
                        }
                    }
                }
            }.ToJson());
        }

        [Fact]
        public void WhenExecuteCommandAndAutomationNotExists_ThenThrows()
        {
            var solutionItem = new SolutionItem(this.toolkit, this.pattern);

            solutionItem
                .Invoking(x =>
                    x.ExecuteCommand(new SolutionDefinition(new ToolkitDefinition(this.pattern)), "acommandname"))
                .Should().Throw<AutomateException>()
                .WithMessage(ExceptionMessages.SolutionItem_UnknownAutomation.Format("acommandname"));
        }

        [Fact]
        public void WhenExecuteCommand_ThenReturnsResult()
        {
            var automation =
                new Automation("acommandname", AutomationType.TestingOnly, new Dictionary<string, object>());
            this.pattern.AddAutomation(automation);
            var solutionItem = new SolutionItem(this.toolkit, this.pattern);
            var solution = new SolutionDefinition(new ToolkitDefinition(this.pattern));

            var result = solutionItem.ExecuteCommand(solution, "acommandname");

            result.CommandName.Should().Be("acommandname");
            result.IsSuccess.Should().BeTrue();
            result.Log.Should().ContainSingle("testingonly");
            result.ValidationErrors.Should().BeEmpty();
        }

        [Fact]
        public void WhenSetPropertiesAndAnyPropertyLeftHandSideOfAssigmentInvalid_ThenThrows()
        {
            var solutionItem = new SolutionItem(this.toolkit, this.pattern);

            solutionItem
                .Invoking(x => x.SetProperties(new Dictionary<string, string>
                {
                    { "notavalidpropertyassignment", "^" }
                }))
                .Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage(
                    ExceptionMessages.SolutionItem_ConfigureSolution_PropertyAssignmentInvalid.Format(
                        "notavalidpropertyassignment=^", "notavalidpropertyassignment", solutionItem.Id) + "*");
        }

        [Fact]
        public void WhenSetPropertiesAndAnyPropertyRightHandSideOfAssigmentInvalid_ThenThrows()
        {
            var solutionItem = new SolutionItem(this.toolkit, this.pattern);

            solutionItem
                .Invoking(x => x.SetProperties(new Dictionary<string, string>
                {
                    { "", "notavalidpropertyassignment" }
                }))
                .Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage(
                    ExceptionMessages.SolutionItem_ConfigureSolution_PropertyAssignmentInvalid.Format(
                        "=notavalidpropertyassignment", "", solutionItem.Id) + "*");
        }

        [Fact]
        public void WhenSetPropertiesWithUnknownProperty_ThenThrows()
        {
            var solutionItem = new SolutionItem(this.toolkit, this.pattern);

            solutionItem
                .Invoking(x => x.SetProperties(new Dictionary<string, string>
                {
                    { "anunknownattribute", "avalue" }
                }))
                .Should().Throw<AutomateException>()
                .WithMessage(
                    ExceptionMessages.SolutionItem_ConfigureSolution_ElementPropertyNotExists.Format("apatternname",
                        "anunknownattribute"));
        }

        [Fact]
        public void WhenSetPropertiesWithWithPropertyOfWrongChoice_ThenThrows()
        {
            this.pattern.AddAttribute(new Attribute("anattributename", choices: new List<string> { "avalue" }));
            var solutionItem = new SolutionItem(this.toolkit, this.pattern);

            solutionItem
                .Invoking(x => x.SetProperties(new Dictionary<string, string>
                {
                    { "anattributename", "awrongvalue" }
                }))
                .Should().Throw<AutomateException>()
                .WithMessage(ExceptionMessages.SolutionItem_ConfigureSolution_ElementPropertyValueIsNotOneOf.Format(
                    "apatternname", "anattributename", new List<string> { "avalue" }.Join(";"), "awrongvalue"));
        }

        [Fact]
        public void WhenSetPropertiesWithPropertyOfWrongDataType_ThenThrows()
        {
            this.pattern.AddAttribute(new Attribute("anattributename", "int"));
            var solutionItem = new SolutionItem(this.toolkit, this.pattern);

            solutionItem
                .Invoking(x => x.SetProperties(new Dictionary<string, string>
                {
                    { "anattributename", "astring" }
                }))
                .Should().Throw<AutomateException>()
                .WithMessage(
                    ExceptionMessages.SolutionItem_ConfigureSolution_ElementPropertyValueNotCompatible.Format(
                        "apatternname", "anattributename", "int", "astring"));
        }

        [Fact]
        public void WhenSetPropertiesAndPropertyChoice_ThenUpdatesProperties()
        {
            this.pattern.AddAttribute(new Attribute("anattributename", choices: new List<string> { "avalue" }));
            var solutionItem = new SolutionItem(this.toolkit, this.pattern);

            solutionItem.SetProperties(new Dictionary<string, string>
            {
                { "anattributename", "avalue" }
            });

            solutionItem.Properties["anattributename"].Value.Should().Be("avalue");
        }

        [Fact]
        public void WhenSetProperties_ThenUpdatesProperties()
        {
            this.pattern.AddAttribute(new Attribute("anattributename1"));
            this.pattern.AddAttribute(new Attribute("anattributename2"));
            this.pattern.AddAttribute(new Attribute("anattributename3"));
            var solutionItem = new SolutionItem(this.toolkit, this.pattern);

            solutionItem.SetProperties(new Dictionary<string, string>
            {
                { "anattributename1", "avalue1" },
                { "anattributename2", "avalue2" },
                { "anattributename3", "avalue3" }
            });

            solutionItem.Properties["anattributename1"].Value.Should().Be("avalue1");
            solutionItem.Properties["anattributename2"].Value.Should().Be("avalue2");
            solutionItem.Properties["anattributename3"].Value.Should().Be("avalue3");
        }

        [Fact]
        public void WhenMigrateAndAddElementToPattern_ThenNothing()
        {
            var solutionItem = new SolutionItem(this.toolkit, this.pattern);

            var latestPattern = ClonePattern(this.pattern);
            latestPattern.AddElement("anelementname");
            var latestToolkit = new ToolkitDefinition(latestPattern);
            var result = new SolutionUpgradeResult(new SolutionDefinition(latestToolkit), "0", "1");

            solutionItem.Migrate(latestToolkit, result);

            solutionItem.Properties.ContainsKey("anelementname").Should().BeFalse();
            result.IsSuccess.Should().BeTrue();
            result.Log.Should().BeEmpty();
        }

        [Fact]
        public void WhenMigrateAndAddCollectionToPattern_ThenNothing()
        {
            var solutionItem = new SolutionItem(this.toolkit, this.pattern);

            var latestPattern = ClonePattern(this.pattern);
            latestPattern.AddElement("acollectionname", ElementCardinality.OneOrMany);
            var latestToolkit = new ToolkitDefinition(latestPattern);
            var result = new SolutionUpgradeResult(new SolutionDefinition(latestToolkit), "0", "1");

            solutionItem.Migrate(latestToolkit, result);

            solutionItem.Properties.ContainsKey("acollectionname").Should().BeFalse();
            result.IsSuccess.Should().BeTrue();
            result.Log.Should().BeEmpty();
        }

        [Fact]
        public void WhenMigrateAndAddAttributeToPattern_ThenAddsElement()
        {
            var solutionItem = new SolutionItem(this.toolkit, this.pattern);

            var latestPattern = ClonePattern(this.pattern);
            latestPattern.AddAttribute("anattributename", defaultValue: "adefaultvalue");
            var latestToolkit = new ToolkitDefinition(latestPattern);
            var result = new SolutionUpgradeResult(new SolutionDefinition(latestToolkit), "0", "1");

            solutionItem.Migrate(latestToolkit, result);

            solutionItem.Properties.ContainsKey("anattributename").Should().BeTrue();
            solutionItem.Properties["anattributename"].IsMaterialised.Should().BeTrue();
            result.IsSuccess.Should().BeTrue();
            result.Log.Should().Contain(x =>
                x.Type == MigrationChangeType.NonBreaking
                && x.MessageTemplate == MigrationMessages.SolutionItem_AttributeAdded
                && (string)x.Arguments[0] == "apatternname.anattributename"
                && (string)x.Arguments[1] == "adefaultvalue");
        }

        [Fact]
        public void WhenMigrateElementOfPatternAndHasBeenDeleted_ThenRemovesElement()
        {
            this.pattern.AddElement("anelementname");
            var solutionItem = new SolutionItem(this.toolkit, this.pattern);
            solutionItem.Properties["anelementname"].Materialise();

            var latestPattern = ClonePattern(this.pattern);
            latestPattern.DeleteElement("anelementname");
            var latestToolkit = new ToolkitDefinition(latestPattern);
            var result = new SolutionUpgradeResult(new SolutionDefinition(latestToolkit), "0", "1");

            solutionItem.Migrate(latestToolkit, result);

            solutionItem.Properties.ContainsKey("anelementname").Should().BeFalse();
            result.IsSuccess.Should().BeTrue();
            result.Log.Should().Contain(x =>
                x.Type == MigrationChangeType.Breaking
                && x.MessageTemplate == MigrationMessages.SolutionItem_ElementDeleted
                && (string)x.Arguments[0] == "apatternname.anelementname");
        }

        [Fact]
        public void WhenMigrateCollectionOfPatternWithItemsAndHasBeenDeleted_ThenRemovesCollectionAndAllItems()
        {
            this.pattern.AddElement("acollectionname", ElementCardinality.OneOrMany);
            var solutionItem = new SolutionItem(this.toolkit, this.pattern);
            solutionItem.Properties["acollectionname"].MaterialiseCollectionItem();
            solutionItem.Properties["acollectionname"].MaterialiseCollectionItem();

            var latestPattern = ClonePattern(this.pattern);
            latestPattern.DeleteElement("acollectionname");
            var latestToolkit = new ToolkitDefinition(latestPattern);
            var result = new SolutionUpgradeResult(new SolutionDefinition(latestToolkit), "0", "1");

            solutionItem.Migrate(latestToolkit, result);

            solutionItem.Properties.ContainsKey("acollectionname").Should().BeFalse();
            result.IsSuccess.Should().BeTrue();
            result.Log.Should().Contain(x =>
                x.Type == MigrationChangeType.Breaking
                && x.MessageTemplate == MigrationMessages.SolutionItem_ElementDeleted
                && (string)x.Arguments[0] == "apatternname.acollectionname");
        }

        [Fact]
        public void WhenMigrateElementOfCollectionOfPatternAndHasBeenDeleted_ThenRemovesElementsOfCollectionItems()
        {
            var collection = this.pattern.AddElement("acollectionname", ElementCardinality.OneOrMany);
            collection.AddElement("anelementname");
            var solutionItem = new SolutionItem(this.toolkit, this.pattern);
            var item1 = solutionItem.Properties["acollectionname"].MaterialiseCollectionItem();
            item1.Properties["anelementname"].Materialise();
            var item2 = solutionItem.Properties["acollectionname"].MaterialiseCollectionItem();
            item2.Properties["anelementname"].Materialise();
            var item3 = solutionItem.Properties["acollectionname"].MaterialiseCollectionItem();
            item3.Properties["anelementname"].Materialise();

            var latestPattern = ClonePattern(this.pattern);
            latestPattern.Elements.Single().DeleteElement("anelementname");
            var latestToolkit = new ToolkitDefinition(latestPattern);
            var result = new SolutionUpgradeResult(new SolutionDefinition(latestToolkit), "0", "1");

            solutionItem.Migrate(latestToolkit, result);

            solutionItem.Properties.ContainsKey("acollectionname").Should().BeTrue();
            solutionItem.Properties["acollectionname"].Items.Count.Should().Be(3);
            solutionItem.Properties["acollectionname"].Items[0].Properties.ContainsKey("anelementname").Should()
                .BeFalse();
            solutionItem.Properties["acollectionname"].Items[1].Properties.ContainsKey("anelementname").Should()
                .BeFalse();
            solutionItem.Properties["acollectionname"].Items[2].Properties.ContainsKey("anelementname").Should()
                .BeFalse();
            result.IsSuccess.Should().BeTrue();
            result.Log.Should().Contain(x =>
                x.Type == MigrationChangeType.Breaking
                && x.MessageTemplate == MigrationMessages.SolutionItem_ElementDeleted
                && (string)x.Arguments[0] == $"apatternname.acollectionname.{item1.Id}.anelementname");
            result.Log.Should().Contain(x =>
                x.Type == MigrationChangeType.Breaking
                && x.MessageTemplate == MigrationMessages.SolutionItem_ElementDeleted
                && (string)x.Arguments[0] == $"apatternname.acollectionname.{item2.Id}.anelementname");
            result.Log.Should().Contain(x =>
                x.Type == MigrationChangeType.Breaking
                && x.MessageTemplate == MigrationMessages.SolutionItem_ElementDeleted
                && (string)x.Arguments[0] == $"apatternname.acollectionname.{item3.Id}.anelementname");
        }

        [Fact]
        public void WhenMigrateAttributeOfPatternAndHasBeenDeleted_ThenRemovesAttribute()
        {
            this.pattern.AddAttribute("anattributename");
            var solutionItem = new SolutionItem(this.toolkit, this.pattern);

            var latestPattern = ClonePattern(this.pattern);
            latestPattern.DeleteAttribute("anattributename");
            var latestToolkit = new ToolkitDefinition(latestPattern);
            var result = new SolutionUpgradeResult(new SolutionDefinition(latestToolkit), "0", "1");

            solutionItem.Migrate(latestToolkit, result);

            solutionItem.Properties.ContainsKey("anattributename").Should().BeFalse();
            result.IsSuccess.Should().BeTrue();
            result.Log.Should().Contain(x =>
                x.Type == MigrationChangeType.Breaking
                && x.MessageTemplate == MigrationMessages.SolutionItem_AttributeDeleted
                && (string)x.Arguments[0] == "apatternname.anattributename");
        }

        [Fact]
        public void WhenMigrateDescendantElementAndHasBeenDeleted_ThenRemovesElement()
        {
            var element = this.pattern.AddElement("anelementname1");
            element.AddElement("anelementname2");
            var solutionItem = new SolutionItem(this.toolkit, this.pattern);
            solutionItem.Properties["anelementname1"].Materialise();
            solutionItem.Properties["anelementname1"].Properties["anelementname2"].Materialise();

            var latestPattern = ClonePattern(this.pattern);
            latestPattern.Elements.Single().DeleteElement("anelementname2");
            var latestToolkit = new ToolkitDefinition(latestPattern);
            var result = new SolutionUpgradeResult(new SolutionDefinition(latestToolkit), "0", "1");

            solutionItem.Migrate(latestToolkit, result);

            solutionItem.Properties["anelementname1"].Properties.ContainsKey("anelementname2").Should().BeFalse();
            result.IsSuccess.Should().BeTrue();
            result.Log.Should().Contain(x =>
                x.Type == MigrationChangeType.Breaking
                && x.MessageTemplate == MigrationMessages.SolutionItem_ElementDeleted
                && (string)x.Arguments[0] == "apatternname.anelementname1.anelementname2");
        }

        [Fact]
        public void WhenMigrateDescendantCollectionAndHasBeenDeleted_ThenRemovesElement()
        {
            var element = this.pattern.AddElement("anelementname");
            element.AddElement("acollectionname", ElementCardinality.OneOrMany);
            var solutionItem = new SolutionItem(this.toolkit, this.pattern);
            solutionItem.Properties["anelementname"].Materialise();
            solutionItem.Properties["anelementname"].Properties["acollectionname"].Materialise();

            var latestPattern = ClonePattern(this.pattern);
            latestPattern.Elements.Single().DeleteElement("acollectionname");
            var latestToolkit = new ToolkitDefinition(latestPattern);
            var result = new SolutionUpgradeResult(new SolutionDefinition(latestToolkit), "0", "1");

            solutionItem.Migrate(latestToolkit, result);

            solutionItem.Properties["anelementname"].Properties.ContainsKey("acollectionname").Should().BeFalse();
            result.IsSuccess.Should().BeTrue();
            result.Log.Should().Contain(x =>
                x.Type == MigrationChangeType.Breaking
                && x.MessageTemplate == MigrationMessages.SolutionItem_ElementDeleted
                && (string)x.Arguments[0] == "apatternname.anelementname.acollectionname");
        }

        [Fact]
        public void WhenMigrateDescendantAttributeAndHasBeenDeleted_ThenRemovesAttribute()
        {
            var element = this.pattern.AddElement("anelementname");
            element.AddAttribute("anattributename");
            var solutionItem = new SolutionItem(this.toolkit, this.pattern);
            solutionItem.Properties["anelementname"].Materialise();

            var latestPattern = ClonePattern(this.pattern);
            latestPattern.Elements.Single().DeleteAttribute("anattributename");
            var latestToolkit = new ToolkitDefinition(latestPattern);
            var result = new SolutionUpgradeResult(new SolutionDefinition(latestToolkit), "0", "1");

            solutionItem.Migrate(latestToolkit, result);

            solutionItem.Properties["anelementname"].Properties.ContainsKey("anattributename").Should().BeFalse();
            result.IsSuccess.Should().BeTrue();
            result.Log.Should().Contain(x =>
                x.Type == MigrationChangeType.Breaking
                && x.MessageTemplate == MigrationMessages.SolutionItem_AttributeDeleted
                && (string)x.Arguments[0] == "apatternname.anelementname.anattributename");
        }

        [Fact]
        public void WhenMigrateAttributeAndNameChanged_ThenRenamesAttribute()
        {
            this.pattern.AddAttribute("anattributename1");
            var solutionItem = new SolutionItem(this.toolkit, this.pattern);
            solutionItem.Properties["anattributename1"].Value = "avalue";

            var latestPattern = ClonePattern(this.pattern);
            latestPattern.UpdateAttribute("anattributename1", "anattributename2");
            var latestToolkit = new ToolkitDefinition(latestPattern);
            var result = new SolutionUpgradeResult(new SolutionDefinition(latestToolkit), "0", "1");

            solutionItem.Migrate(latestToolkit, result);

            solutionItem.Properties.ContainsKey("anattributename1").Should().BeFalse();
            solutionItem.Properties["anattributename2"].Value.Should().Be("avalue");
            result.IsSuccess.Should().BeTrue();
            result.Log.Should().Contain(x =>
                x.Type == MigrationChangeType.Breaking
                && x.MessageTemplate == MigrationMessages.SolutionItem_AttributeNameChanged
                && (string)x.Arguments[0] == "apatternname.anattributename1"
                && (string)x.Arguments[1] == "anattributename1"
                && (string)x.Arguments[2] == "anattributename2");
        }

        [Fact]
        public void WhenMigrateAttributeAndDataTypeChangedAndHasNoValue_ThenLeavesValue()
        {
            this.pattern.AddAttribute("anattributename", "int");
            var solutionItem = new SolutionItem(this.toolkit, this.pattern);
            solutionItem.Properties["anattributename"].Value = null;

            var latestPattern = ClonePattern(this.pattern);
            latestPattern.UpdateAttribute("anattributename", type: "bool");
            var latestToolkit = new ToolkitDefinition(latestPattern);
            var result = new SolutionUpgradeResult(new SolutionDefinition(latestToolkit), "0", "1");

            solutionItem.Migrate(latestToolkit, result);

            solutionItem.Properties["anattributename"].Value.Should().BeNull();
            result.IsSuccess.Should().BeTrue();
            result.Log.Should().Contain(x =>
                x.Type == MigrationChangeType.Breaking
                && x.MessageTemplate == MigrationMessages.SolutionItem_AttributeDataTypeChanged
                && (string)x.Arguments[0] == "apatternname.anattributename"
                && (string)x.Arguments[1] == "int"
                && (string)x.Arguments[2] == "bool");
        }

        [Fact]
        public void WhenMigrateAttributeAndDataTypeChangedAndHasIncorrectValueAndNoDefaultValue_ThenResetsValue()
        {
            this.pattern.AddAttribute("anattributename", "int");
            var solutionItem = new SolutionItem(this.toolkit, this.pattern);
            solutionItem.Properties["anattributename"].Value = 25;

            var latestPattern = ClonePattern(this.pattern);
            latestPattern.UpdateAttribute("anattributename", type: "bool");
            var latestToolkit = new ToolkitDefinition(latestPattern);
            var result = new SolutionUpgradeResult(new SolutionDefinition(latestToolkit), "0", "1");

            solutionItem.Migrate(latestToolkit, result);

            solutionItem.Properties["anattributename"].Value.Should().BeNull();
            result.IsSuccess.Should().BeTrue();
            result.Log.Should().Contain(x =>
                x.Type == MigrationChangeType.Breaking
                && x.MessageTemplate == MigrationMessages.SolutionItem_AttributeDataTypeChanged
                && (string)x.Arguments[0] == "apatternname.anattributename"
                && (string)x.Arguments[1] == "int"
                && (string)x.Arguments[2] == "bool");
        }

        [Fact]
        public void
            WhenMigrateAttributeAndDataTypeChangedAndHasIncorrectValueAndHasDefaultValue_ThenSetsValueToDefault()
        {
            this.pattern.AddAttribute("anattributename", "int");
            var solutionItem = new SolutionItem(this.toolkit, this.pattern);
            solutionItem.Properties["anattributename"].Value = 25;

            var latestPattern = ClonePattern(this.pattern);
            latestPattern.UpdateAttribute("anattributename", type: "bool", defaultValue: "True");
            var latestToolkit = new ToolkitDefinition(latestPattern);
            var result = new SolutionUpgradeResult(new SolutionDefinition(latestToolkit), "0", "1");

            solutionItem.Migrate(latestToolkit, result);

            solutionItem.Properties["anattributename"].Value.Should().Be(true);
            result.IsSuccess.Should().BeTrue();
            result.Log.Should().Contain(x =>
                x.Type == MigrationChangeType.Breaking
                && x.MessageTemplate == MigrationMessages.SolutionItem_AttributeDataTypeChanged
                && (string)x.Arguments[0] == "apatternname.anattributename"
                && (string)x.Arguments[1] == "int"
                && (string)x.Arguments[2] == "bool");
        }

        [Fact]
        public void
            WhenMigrateAttributeAndDataTypeChangedAndHasIncorrectValueAndHasIncorrectDefaultValue_ThenResetsValue()
        {
            this.pattern.AddAttribute("anattributename", "int");
            var solutionItem = new SolutionItem(this.toolkit, this.pattern);
            solutionItem.Properties["anattributename"].Value = 25;

            var latestPattern = ClonePattern(this.pattern);
            latestPattern.UpdateAttribute("anattributename", type: "int", defaultValue: "25");
            latestPattern.UpdateAttribute("anattributename", type: "bool");
            var latestToolkit = new ToolkitDefinition(latestPattern);
            var result = new SolutionUpgradeResult(new SolutionDefinition(latestToolkit), "0", "1");

            solutionItem.Migrate(latestToolkit, result);

            solutionItem.Properties["anattributename"].Value.Should().BeNull();
            result.IsSuccess.Should().BeTrue();
            result.Log.Should().Contain(x =>
                x.Type == MigrationChangeType.Breaking
                && x.MessageTemplate == MigrationMessages.SolutionItem_AttributeDataTypeChanged
                && (string)x.Arguments[0] == "apatternname.anattributename"
                && (string)x.Arguments[1] == "int"
                && (string)x.Arguments[2] == "bool");
        }

        [Fact]
        public void WhenMigrateAttributeAndChoicesAddedAndOldValueIsAChoice_ThenLeavesValue()
        {
            this.pattern.AddAttribute("anattributename");
            var solutionItem = new SolutionItem(this.toolkit, this.pattern);
            solutionItem.Properties["anattributename"].Value = "achoice2";

            var latestPattern = ClonePattern(this.pattern);
            latestPattern.UpdateAttribute("anattributename",
                choices: new List<string> { "achoice1", "achoice2", "achoice3" });
            var latestToolkit = new ToolkitDefinition(latestPattern);
            var result = new SolutionUpgradeResult(new SolutionDefinition(latestToolkit), "0", "1");

            solutionItem.Migrate(latestToolkit, result);

            solutionItem.Properties["anattributename"].Value.Should().Be("achoice2");
            result.IsSuccess.Should().BeTrue();
            result.Log.Should().Contain(x =>
                x.Type == MigrationChangeType.NonBreaking
                && x.MessageTemplate == MigrationMessages.SolutionItem_AttributeChoicesAdded
                && (string)x.Arguments[0] == "apatternname.anattributename"
                && (string)x.Arguments[1] == "achoice2");
        }

        [Fact]
        public void WhenMigrateAttributeAndChoicesAddedAndOldValueIsNotAChoice_ThenResetsValue()
        {
            this.pattern.AddAttribute("anattributename");
            var solutionItem = new SolutionItem(this.toolkit, this.pattern);
            solutionItem.Properties["anattributename"].Value = "notachoice";

            var latestPattern = ClonePattern(this.pattern);
            latestPattern.UpdateAttribute("anattributename",
                choices: new List<string> { "achoice1", "achoice2", "achoice3" });
            var latestToolkit = new ToolkitDefinition(latestPattern);
            var result = new SolutionUpgradeResult(new SolutionDefinition(latestToolkit), "0", "1");

            solutionItem.Migrate(latestToolkit, result);

            solutionItem.Properties["anattributename"].Value.Should().BeNull();
            result.IsSuccess.Should().BeTrue();
            result.Log.Should().Contain(x =>
                x.Type == MigrationChangeType.NonBreaking
                && x.MessageTemplate == MigrationMessages.SolutionItem_AttributeChoicesAdded
                && (string)x.Arguments[0] == "apatternname.anattributename"
                && (string)x.Arguments[1] == null);
        }

        [Fact]
        public void WhenMigrateAttributeAndChoicesChangedToScalar_ThenLeavesValue()
        {
            this.pattern.AddAttribute("anattributename",
                choices: new List<string> { "achoice1", "achoice2", "achoice3" });
            var solutionItem = new SolutionItem(this.toolkit, this.pattern);
            solutionItem.Properties["anattributename"].Value = "achoice2";

            var latestPattern = ClonePattern(this.pattern);
            latestPattern.UpdateAttribute("anattributename", choices: new List<string>());
            var latestToolkit = new ToolkitDefinition(latestPattern);
            var result = new SolutionUpgradeResult(new SolutionDefinition(latestToolkit), "0", "1");

            solutionItem.Migrate(latestToolkit, result);

            solutionItem.Properties["anattributename"].Value.Should().Be("achoice2");
            result.IsSuccess.Should().BeTrue();
            result.Log.Should().Contain(x =>
                x.Type == MigrationChangeType.Breaking
                && x.MessageTemplate == MigrationMessages.SolutionItem_AttributeChoicesDeleted
                && (string)x.Arguments[0] == "apatternname.anattributename");
        }

        [Fact]
        public void WhenMigrateAttributeAndChoicesChangedAndNoValue_ThenLeavesValue()
        {
            this.pattern.AddAttribute("anattributename", "string", false, null,
                new List<string> { "achoice1", "achoice1", "achoice1" });
            var solutionItem = new SolutionItem(this.toolkit, this.pattern);
            solutionItem.Properties["anattributename"].Value = null;

            var latestPattern = ClonePattern(this.pattern);
            latestPattern.UpdateAttribute("anattributename",
                choices: new List<string> { "achoice9", "achoice8", "achoice7" });
            var latestToolkit = new ToolkitDefinition(latestPattern);
            var result = new SolutionUpgradeResult(new SolutionDefinition(latestToolkit), "0", "1");

            solutionItem.Migrate(latestToolkit, result);

            solutionItem.Properties["anattributename"].Value.Should().BeNull();
            result.IsSuccess.Should().BeTrue();
            result.Log.Should().BeEmpty();
        }

        [Fact]
        public void WhenMigrateAttributeAndChoicesChangedAndHasIncorrectValueAndNoDefaultValue_ThenResetsValue()
        {
            this.pattern.AddAttribute("anattributename", "string", false, null,
                new List<string> { "achoice1", "achoice2", "achoice3" });
            var solutionItem = new SolutionItem(this.toolkit, this.pattern);
            solutionItem.Properties["anattributename"].Value = "achoice1";

            var latestPattern = ClonePattern(this.pattern);
            latestPattern.UpdateAttribute("anattributename",
                choices: new List<string> { "achoice9", "achoice8", "achoice7" });
            var latestToolkit = new ToolkitDefinition(latestPattern);
            var result = new SolutionUpgradeResult(new SolutionDefinition(latestToolkit), "0", "1");

            solutionItem.Migrate(latestToolkit, result);

            solutionItem.Properties["anattributename"].Value.Should().BeNull();
            result.IsSuccess.Should().BeTrue();
            result.Log.Should().Contain(x =>
                x.Type == MigrationChangeType.Breaking
                && x.MessageTemplate == MigrationMessages.SolutionItem_AttributeChoicesChanged
                && (string)x.Arguments[0] == "apatternname.anattributename");
        }

        [Fact]
        public void WhenMigrateAttributeAndChoicesChangedAndHasIncorrectValueAndHasDefaultValue_ThenSetsValueToDefault()
        {
            this.pattern.AddAttribute("anattributename", "string", false, null,
                new List<string> { "achoice1", "achoice2", "achoice3" });
            var solutionItem = new SolutionItem(this.toolkit, this.pattern);
            solutionItem.Properties["anattributename"].Value = "achoice1";

            var latestPattern = ClonePattern(this.pattern);
            latestPattern.UpdateAttribute("anattributename", defaultValue: "achoice9",
                choices: new List<string> { "achoice9", "achoice8", "achoice7" });
            var latestToolkit = new ToolkitDefinition(latestPattern);
            var result = new SolutionUpgradeResult(new SolutionDefinition(latestToolkit), "0", "1");

            solutionItem.Migrate(latestToolkit, result);

            solutionItem.Properties["anattributename"].Value.Should().Be("achoice9");
            result.IsSuccess.Should().BeTrue();
            result.Log.Should().Contain(x =>
                x.Type == MigrationChangeType.Breaking
                && x.MessageTemplate == MigrationMessages.SolutionItem_AttributeChoicesChanged
                && (string)x.Arguments[0] == "apatternname.anattributename");
        }

        [Fact]
        public void WhenMigrateAttributeAndChoicesChangedAndHasCorrectValue_ThenLeavesValue()
        {
            this.pattern.AddAttribute("anattributename", "string", false, null,
                new List<string> { "achoice1", "achoice2", "achoice3" });
            var solutionItem = new SolutionItem(this.toolkit, this.pattern);
            solutionItem.Properties["anattributename"].Value = "achoice3";

            var latestPattern = ClonePattern(this.pattern);
            latestPattern.UpdateAttribute("anattributename",
                choices: new List<string> { "achoice3", "achoice4", "achoice5" });
            var latestToolkit = new ToolkitDefinition(latestPattern);
            var result = new SolutionUpgradeResult(new SolutionDefinition(latestToolkit), "0", "1");

            solutionItem.Migrate(latestToolkit, result);

            solutionItem.Properties["anattributename"].Value.Should().Be("achoice3");
            result.IsSuccess.Should().BeTrue();
            result.Log.Should().BeEmpty();
        }

        [Fact]
        public void WhenMigrateAttributeAndDefaultValueChangedAndHasNoValue_ThenSetsNewDefaultValue()
        {
            this.pattern.AddAttribute("anattributename");
            var solutionItem = new SolutionItem(this.toolkit, this.pattern);
            solutionItem.Properties["anattributename"].Value = null;

            var latestPattern = ClonePattern(this.pattern);
            latestPattern.UpdateAttribute("anattributename", defaultValue: "adefaultvalue");
            var latestToolkit = new ToolkitDefinition(latestPattern);
            var result = new SolutionUpgradeResult(new SolutionDefinition(latestToolkit), "0", "1");

            solutionItem.Migrate(latestToolkit, result);

            solutionItem.Properties["anattributename"].Value.Should().Be("adefaultvalue");
            result.IsSuccess.Should().BeTrue();
            result.Log.Should().Contain(x =>
                x.Type == MigrationChangeType.NonBreaking
                && x.MessageTemplate == MigrationMessages.SolutionItem_AttributeDefaultValueChanged
                && (string)x.Arguments[0] == "apatternname.anattributename"
                && (string)x.Arguments[1] == null
                && (string)x.Arguments[2] == "adefaultvalue");
        }

        [Fact]
        public void WhenMigrateAttributeAndDefaultValueChangedAndHasNonDefaultValue_ThenLeavesValue()
        {
            this.pattern.AddAttribute("anattributename");
            var solutionItem = new SolutionItem(this.toolkit, this.pattern);
            solutionItem.Properties["anattributename"].Value = "avalue";

            var latestPattern = ClonePattern(this.pattern);
            latestPattern.UpdateAttribute("anattributename", defaultValue: "adefaultvalue");
            var latestToolkit = new ToolkitDefinition(latestPattern);
            var result = new SolutionUpgradeResult(new SolutionDefinition(latestToolkit), "0", "1");

            solutionItem.Migrate(latestToolkit, result);

            solutionItem.Properties["anattributename"].Value.Should().Be("avalue");
            result.IsSuccess.Should().BeTrue();
            result.Log.Should().BeEmpty();
        }

        [Fact]
        public void WhenMigrateAttributeAndDefaultValueChangedAndHasOldDefaultValue_ThenSetsNewDefaultValue()
        {
            this.pattern.AddAttribute("anattributename", null, false, "adefaultvalue");
            var solutionItem = new SolutionItem(this.toolkit, this.pattern);
            solutionItem.Properties["anattributename"].Value = "adefaultvalue";

            var latestPattern = ClonePattern(this.pattern);
            latestPattern.UpdateAttribute("anattributename", defaultValue: "anewdefaultvalue");
            var latestToolkit = new ToolkitDefinition(latestPattern);
            var result = new SolutionUpgradeResult(new SolutionDefinition(latestToolkit), "0", "1");

            solutionItem.Migrate(latestToolkit, result);

            solutionItem.Properties["anattributename"].Value.Should().Be("anewdefaultvalue");
            result.IsSuccess.Should().BeTrue();
            result.Log.Should().Contain(x =>
                x.Type == MigrationChangeType.NonBreaking
                && x.MessageTemplate == MigrationMessages.SolutionItem_AttributeDefaultValueChanged
                && (string)x.Arguments[0] == "apatternname.anattributename"
                && (string)x.Arguments[1] == "adefaultvalue"
                && (string)x.Arguments[2] == "anewdefaultvalue");
        }

        [Fact]
        public void WhenResetAllPropertiesAndIsCollection_ThenThrows()
        {
            var collection = new Element("acollectionname", ElementCardinality.OneOrMany);
            this.pattern.AddElement(collection);
            var collectionItem = new SolutionItem(this.toolkit, this.pattern)
                .Properties["acollectionname"].Materialise();

            collectionItem
                .Invoking(x => x.ResetAllProperties())
                .Should().Throw<AutomateException>()
                .WithMessage(ExceptionMessages.SolutionItem_ResetPropertiesForNonElement);
        }

        [Fact]
        public void WhenResetAllPropertiesAndIsAttribute_ThenThrows()
        {
            var attribute = new Attribute("anattributename");
            this.pattern.AddAttribute(attribute);
            var attributeItem = new SolutionItem(this.toolkit, this.pattern)
                .Properties["anattributename"];

            attributeItem
                .Invoking(x => x.ResetAllProperties())
                .Should().Throw<AutomateException>()
                .WithMessage(ExceptionMessages.SolutionItem_ResetPropertiesForNonElement);
        }

        [Fact]
        public void WhenResetAllPropertiesOnPattern_ThenResetsProperties()
        {
            var attribute1 = new Attribute("anattributename1");
            var attribute2 = new Attribute("anattributename2", defaultValue: "adefaultvalue2");
            var attribute3 = new Attribute("anattributename3", "int");
            this.pattern.AddAttribute(attribute1);
            this.pattern.AddAttribute(attribute2);
            this.pattern.AddAttribute(attribute3);

            var patternItem = new SolutionItem(this.toolkit, this.pattern);
            patternItem.Properties["anattributename1"].Value = "avalue1";
            patternItem.Properties["anattributename2"].Value = "avalue2";
            patternItem.Properties["anattributename3"].Value = 25;

            patternItem.ResetAllProperties();

            patternItem.Properties["anattributename1"].Value.Should().BeNull();
            patternItem.Properties["anattributename2"].Value.Should().Be("adefaultvalue2");
            patternItem.Properties["anattributename3"].Value.Should().BeNull();
        }

        [Fact]
        public void WhenResetAllPropertiesOnElement_ThenResetsProperties()
        {
            var attribute1 = new Attribute("anattributename1");
            var attribute2 = new Attribute("anattributename2", defaultValue: "adefaultvalue2");
            var attribute3 = new Attribute("anattributename3", "int");
            var element = new Element("anelementname");
            element.AddAttribute(attribute1);
            element.AddAttribute(attribute2);
            element.AddAttribute(attribute3);
            this.pattern.AddElement(element);

            var elementName = new SolutionItem(this.toolkit, this.pattern)
                .Properties["anelementname"].Materialise();
            elementName.Properties["anattributename1"].Value = "avalue1";
            elementName.Properties["anattributename2"].Value = "avalue2";
            elementName.Properties["anattributename3"].Value = 25;

            elementName.ResetAllProperties();

            elementName.Properties["anattributename1"].Value.Should().BeNull();
            elementName.Properties["anattributename2"].Value.Should().Be("adefaultvalue2");
            elementName.Properties["anattributename3"].Value.Should().BeNull();
        }

        [Fact]
        public void WhenClearCollectionItemsAndNotIsCollection_ThenThrows()
        {
            var element = new Element("anelementname");
            this.pattern.AddElement(element);
            var elementItem = new SolutionItem(this.toolkit, this.pattern)
                .Properties["anelementname"].Materialise();

            elementItem
                .Invoking(x => x.ClearCollectionItems())
                .Should().Throw<AutomateException>()
                .WithMessage(ExceptionMessages.SolutionItem_ClearCollectionForNonCollection);
        }

        [Fact]
        public void WhenClearCollectionItems_ThenClearsCollectionItems()
        {
            var collection = new Element("acollectionname", ElementCardinality.OneOrMany);
            this.pattern.AddElement(collection);
            var collectionItem = new SolutionItem(this.toolkit, this.pattern)
                .Properties["acollectionname"].Materialise();
            collectionItem.MaterialiseCollectionItem();
            collectionItem.MaterialiseCollectionItem();
            collectionItem.MaterialiseCollectionItem();

            collectionItem.ClearCollectionItems();

            collectionItem.Items.Count.Should().Be(0);
        }

        [Fact]
        public void WhenDeleteAndDeletePattern_ThenThrows()
        {
            var element = new Element("anelementname");
            this.pattern.AddElement(element);
            var patternItem = new SolutionItem(this.toolkit, this.pattern);

            patternItem
                .Invoking(x => x.Delete(patternItem))
                .Should().Throw<AutomateException>()
                .WithMessage(ExceptionMessages.SolutionItem_DeleteForNonElementChild);
        }

        [Fact]
        public void WhenDeleteAndDeleteChildAttribute_ThenThrows()
        {
            var attribute = new Attribute("anattributename");
            this.pattern.AddAttribute(attribute);
            var patternItem = new SolutionItem(this.toolkit, this.pattern);
            var attributeItem = patternItem.Properties["anattributename"];

            attributeItem
                .Invoking(x => x.Delete(attributeItem))
                .Should().Throw<AutomateException>()
                .WithMessage(ExceptionMessages.SolutionItem_DeleteForNonElement);
        }

        [Fact]
        public void WhenDeleteAndDeleteChildElement_ThenDeletes()
        {
            var element = new Element("anelementname");
            this.pattern.AddElement(element);
            var patternItem = new SolutionItem(this.toolkit, this.pattern);
            var elementItem = patternItem.Properties["anelementname"];

            patternItem.Delete(elementItem);

            patternItem.Properties.Should().NotContainKey("anelementname");
        }

        [Fact]
        public void WhenDeleteAndDeleteChildCollection_ThenDeletes()
        {
            var collection = new Element("acollectionname", ElementCardinality.ZeroOrMany);
            this.pattern.AddElement(collection);
            var patternItem = new SolutionItem(this.toolkit, this.pattern);
            var collectionItem = patternItem.Properties["acollectionname"];

            patternItem.Delete(collectionItem);

            patternItem.Properties.Should().NotContainKey("acollectionname");
        }

        [Fact]
        public void WhenDeleteAndNotAChild_ThenThrows()
        {
            var element1 = new Element("anelementname1");
            var element2 = new Element("anelementname2");
            this.pattern.AddElement(element1);
            this.pattern.AddElement(element2);
            var patternItem = new SolutionItem(this.toolkit, this.pattern);
            var elementItem1 = patternItem.Properties["anelementname1"];
            var elementItem2 = patternItem.Properties["anelementname2"];

            elementItem1
                .Invoking(x => x.Delete(elementItem2))
                .Should().Throw<AutomateException>()
                .WithMessage(ExceptionMessages.SolutionItem_DeleteWithUnknownChild);
        }

        [Fact]
        public void WhenDeleteAndDeleteCollectionItem_ThenDeletes()
        {
            var collection = new Element("acollectionname", ElementCardinality.ZeroOrMany);
            this.pattern.AddElement(collection);
            var patternItem = new SolutionItem(this.toolkit, this.pattern);
            var collectionItem = patternItem.Properties["acollectionname"];
            var itemItem1 = collectionItem.MaterialiseCollectionItem();
            var itemItem2 = collectionItem.MaterialiseCollectionItem();
            var itemItem3 = collectionItem.MaterialiseCollectionItem();

            collectionItem.Delete(itemItem2);

            collectionItem.Items.Count.Should().Be(2);
            collectionItem.Items[0].Should().Be(itemItem1);
            collectionItem.Items[1].Should().Be(itemItem3);
        }

        private static PatternDefinition ClonePattern(PatternDefinition originalPattern)
        {
            var factory = new AutomatePersistableFactory();
            return originalPattern.ToJson(factory).FromJson<PatternDefinition>(factory);
        }
    }
}