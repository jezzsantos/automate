﻿using System;
using System.Collections.Generic;
using System.Linq;
using Automate.CLI;
using Automate.CLI.Domain;
using Automate.CLI.Extensions;
using FluentAssertions;
using Xunit;
using Attribute = Automate.CLI.Domain.Attribute;

namespace CLI.UnitTests.Domain
{
    [Trait("Category", "Unit")]
    public class SolutionItemSpec
    {
        [Fact]
        public void WhenConstructedWithPattern_ThenPatternAssigned()
        {
            var pattern = new PatternDefinition("apatternname")
            {
                Description = "adescription"
            };

            var result = new SolutionItem(pattern);

            result.Id.Should().NotBeNull();
            result.Should().NotBeNull();
            result.Value.Should().BeNull();
            result.IsMaterialised.Should().BeTrue();
            result.Parent.Should().BeNull();
        }

        [Fact]
        public void WhenConstructedWithValue_ThenValueAssigned()
        {
            var parent = new SolutionItem();
            var result = new SolutionItem("avalue", Attribute.DefaultType, parent);

            result.Id.Should().NotBeNull();
            result.IsMaterialised.Should().BeTrue();
            result.Value.Should().Be("avalue");
            result.Parent.Should().Be(parent);
        }

        [Fact]
        public void WhenConstructedWithAttributeWithoutDefaultValue_ThenAttributeAssigned()
        {
            var pattern = new PatternDefinition("apatternname");
            var attribute = new Attribute("aname");
            pattern.Attributes.Add(attribute);

            var result = new SolutionItem(pattern);

            result.Should().NotBeNull();
            result.Id.Should().NotBeNull();
            result.Properties["aname"].AttributeSchema.Should().Be(attribute);
            result.Properties["aname"].Value.Should().BeNull();
            result.Properties["aname"].IsMaterialised.Should().BeFalse();
        }

        [Fact]
        public void WhenConstructedWithAttributeWithDefaultValue_ThenAttributeAssigned()
        {
            var pattern = new PatternDefinition("apatternname");
            var attribute = new Attribute("aname", "string", false, "adefaultvalue");
            pattern.Attributes.Add(attribute);

            var result = new SolutionItem(pattern);

            result.Should().NotBeNull();
            result.Id.Should().NotBeNull();
            result.Properties["aname"].AttributeSchema.Should().Be(attribute);
            result.Properties["aname"].Value.Should().Be(attribute.DefaultValue);
            result.Properties["aname"].IsMaterialised.Should().BeTrue();
        }

        [Fact]
        public void WhenConstructedWithAttributeWithDateTimeDefaultValue_ThenAttributeAssignedUtcDateTime()
        {
            var date = DateTime.UtcNow;
            var pattern = new PatternDefinition("apatternname");
            var attribute = new Attribute("aname", "DateTime", false, date.ToIso8601());
            pattern.Attributes.Add(attribute);

            var result = new SolutionItem(pattern);

            result.Should().NotBeNull();
            result.Id.Should().NotBeNull();
            result.Properties["aname"].AttributeSchema.Should().Be(attribute);
            result.Properties["aname"].Value.Should().Be(date);
            result.Properties["aname"].IsMaterialised.Should().BeTrue();
        }

        [Fact]
        public void WhenConstructedWithElement_ThenElementAssigned()
        {
            var pattern = new PatternDefinition("apatternname");
            var element = new Element("anelementname", "adisplayname", "adescription");
            element.Attributes.Add(new Attribute("anattributename", "string", false, "adefaultvalue"));
            pattern.Elements.Add(element);

            var result = new SolutionItem(pattern);

            result.Should().NotBeNull();
            result.Id.Should().NotBeNull();
            result.Properties["anelementname"].ElementSchema.Should().Be(element);
            result.Properties["anelementname"].Value.Should().BeNull();
            result.Properties["anelementname"].IsMaterialised.Should().BeFalse();
            result.Properties["anelementname"].Items.Should().BeNull();
        }

        [Fact]
        public void WhenConstructedWithCollection_ThenCollectionAssigned()
        {
            var pattern = new PatternDefinition("apatternname");
            var element = new Element("acollectionname", "adisplayname", "adescription", true);
            element.Attributes.Add(new Attribute("anattributename", "string", false, "adefaultvalue"));
            pattern.Elements.Add(element);

            var result = new SolutionItem(pattern);

            result.Should().NotBeNull();
            result.Id.Should().NotBeNull();
            result.Properties["acollectionname"].ElementSchema.Should().Be(element);
            result.Properties["acollectionname"].Value.Should().BeNull();
            result.Properties["acollectionname"].IsMaterialised.Should().BeFalse();
            result.Properties["acollectionname"].Items.Should().BeNull();
        }

        [Fact]
        public void WhenConstructedWithDescendantSchema_ThenDescendantElementsAssigned()
        {
            var pattern = new PatternDefinition("apatternname");
            var element3 = new Element("anelementname3", "adisplayname3", "adescription3", true);
            element3.Attributes.Add(new Attribute("anattributename3", "string", false, "adefaultvalue3"));
            var element2 = new Element("anelementname2", "adisplayname2", "adescription2", true);
            element2.Attributes.Add(new Attribute("anattributename2", "string", false, "adefaultvalue2"));
            var element1 = new Element("anelementname1", "adisplayname1", "adescription1", true);
            element1.Attributes.Add(new Attribute("anattributename1", "string", false, "adefaultvalue1"));
            element2.Elements.Add(element3);
            element1.Elements.Add(element2);
            pattern.Elements.Add(element1);

            var result = new SolutionItem(pattern);

            result.Should().NotBeNull();
            result.Id.Should().NotBeNull();
            var solutionElement1 = result.Properties["anelementname1"];
            solutionElement1.ElementSchema.Should().Be(element1);
            solutionElement1.Value.Should().BeNull();
            solutionElement1.IsMaterialised.Should().BeFalse();
            solutionElement1.Items.Should().BeNull();
            solutionElement1.Properties.Should().BeNull();
        }

        [Fact]
        public void WhenMaterialiseAndValue_ThenThrows()
        {
            new SolutionItem(25, "int", null)
                .Invoking(x => x.Materialise(99))
                .Should().Throw<AutomateException>()
                .WithMessage(ExceptionMessages.SolutionItem_ValueAlreadyMaterialised);
        }

        [Fact]
        public void WhenMaterialiseAndPattern_ThenThrows()
        {
            new SolutionItem(new PatternDefinition("apatternname"))
                .Invoking(x => x.Materialise())
                .Should().Throw<AutomateException>()
                .WithMessage(ExceptionMessages.SolutionItem_PatternAlreadyMaterialised.Format("apatternname"));
        }

        [Fact]
        public void WhenMaterialiseAndElement_ThenMaterialises()
        {
            var element = new Element("anelementname", "adisplayname", "adescription");
            var attribute = new Attribute("anattributename", null, false, "adefaultvalue");
            element.Attributes.Add(attribute);

            var result = new SolutionItem(element, null)
                .Materialise();

            result.Id.Should().NotBeNull();
            result.IsMaterialised.Should().BeTrue();
            result.Value.Should().BeNull();
            result.Properties["anattributename"].Value.Should().Be("adefaultvalue");
            result.Items.Should().BeNull();
        }

        [Fact]
        public void WhenMaterialiseAndCollection_ThenMaterialises()
        {
            var collection = new Element("acollectionname", isCollection: true);
            var attribute = new Attribute("anattributename", null, false, "adefaultvalue");
            var element = new Element("anelementname");
            collection.Attributes.Add(attribute);
            collection.Elements.Add(element);

            var result = new SolutionItem(collection, null)
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

            var result = new SolutionItem(attribute, null)
                .Materialise("avalue");

            result.Id.Should().NotBeNull();
            result.IsMaterialised.Should().BeTrue();
            result.Value.Should().Be("avalue");
        }

        [Fact]
        public void WhenMaterialiseCollectionItemAndNotACollection_ThenThrows()
        {
            var element = new Element("anelementname", "adisplayname", "adescription");

            new SolutionItem(element, null)
                .Invoking(x => x.MaterialiseCollectionItem())
                .Should().Throw<AutomateException>()
                .WithMessage(ExceptionMessages.SolutionItem_MaterialiseNotACollection);
        }

        [Fact]
        public void WhenMaterialiseCollectionItem_ThenMaterialisesNewElement()
        {
            var element = new Element("anelementname", "adisplayname", "adescription", true);
            var attribute = new Attribute("anattributename", defaultValue: "adefaultvalue");
            element.Attributes.Add(attribute);
            var solutionItem = new SolutionItem(element, null);

            var result = solutionItem.MaterialiseCollectionItem();

            result.Id.Should().NotBeNull();
            result.IsMaterialised.Should().BeTrue();
            result.IsCollection.Should().BeFalse();
            result.Value.Should().BeNull();
            result.Items.Should().BeNull();
            result.Properties.Should().ContainSingle(prop =>
                prop.Key == "anattributename" && (string)prop.Value.Value == "adefaultvalue");

            solutionItem.IsMaterialised.Should().BeTrue();
            solutionItem.Items.Should().Contain(result);
        }

        [Fact]
        public void WhenHasAttributeAndPropertyInPatternSchema_ThenReturnsTrue()
        {
            var pattern = new PatternDefinition("apattername");
            var attribute = new Attribute("anattributename", null, false, "adefaultvalue");
            pattern.Attributes.Add(attribute);

            var result = new SolutionItem(pattern)
                .HasAttribute("anattributename");

            result.Should().BeTrue();
        }

        [Fact]
        public void WhenHasAttributeAndPropertyInElementSchema_ThenReturnsTrue()
        {
            var element = new Element("anelementname", "adisplayname", "adescription");
            var attribute = new Attribute("anattributename", null, false, "adefaultvalue");
            element.Attributes.Add(attribute);

            var result = new SolutionItem(element, null)
                .HasAttribute("anattributename");

            result.Should().BeTrue();
        }

        [Fact]
        public void WhenHasAttributeAndPropertyNotInSchema_ThenReturnsFalse()
        {
            var element = new Element("anelementname", "adisplayname", "adescription");

            var result = new SolutionItem(element, null)
                .HasAttribute("anattributename");

            result.Should().BeFalse();
        }

        [Fact]
        public void WhenHasAttributeAndAttribute_ThenReturnsFalse()
        {
            var result = new SolutionItem(new Attribute("anattributename", null), null)
                .HasAttribute("anattributename");

            result.Should().BeFalse();
        }

        [Fact]
        public void WhenHasAttributeAndValue_ThenReturnsFalse()
        {
            var result = new SolutionItem("avalue", Attribute.DefaultType, null)
                .HasAttribute("anattributename");

            result.Should().BeFalse();
        }

        [Fact]
        public void WhenGetPropertyAndNotExists_ThenThrows()
        {
            new SolutionItem("avalue", Attribute.DefaultType, null)
                .Invoking(x => x.GetProperty("anunknownname"))
                .Should().Throw<AutomateException>()
                .WithMessage(ExceptionMessages.SolutionItem_NotAProperty.Format("anunknownname"));
        }

        [Fact]
        public void WhenGetPropertyAndNotAnAttribute_ThenThrows()
        {
            var pattern = new PatternDefinition("apatternname");
            var element = new Element("anelementname");
            pattern.Elements.Add(element);

            new SolutionItem(pattern)
                .Invoking(x => x.GetProperty("anelementname"))
                .Should().Throw<AutomateException>()
                .WithMessage(ExceptionMessages.SolutionItem_NotAnAttribute.Format("anelementname"));
        }

        [Fact]
        public void WhenGetPropertyAndNotMaterialised_ThenThrows()
        {
            var element = new Element("anelementname");
            var attribute = new Attribute("anattributename", null, false, "adefaultvalue");
            element.Attributes.Add(attribute);

            new SolutionItem(element, null)
                .Invoking(x => x.GetProperty("anattributename"))
                .Should().Throw<AutomateException>()
                .WithMessage(ExceptionMessages.SolutionItem_NotMaterialised);
        }

        [Fact]
        public void WhenGetPropertyAndMaterialised_ThenReturnsProperty()
        {
            var element = new Element("anelementname");
            var attribute = new Attribute("anattributename", null, false, "adefaultvalue");
            element.Attributes.Add(attribute);

            var result = new SolutionItem(element, null)
                .Materialise()
                .GetProperty("anattributename");

            result.Name.Should().Be("anattributename");
        }

        [Fact]
        public void WhenValidateAndIsPatternWithMissingRequiredAttribute_ThenReturnsErrors()
        {
            var pattern = new PatternDefinition("apatternname");
            pattern.Attributes.Add(new Attribute("anattributename", isRequired: true));

            var result = new SolutionItem(pattern)
                .Validate(new ValidationContext());

            result.Results.Single().Context.Path.Should().Be("{apatternname.anattributename}");
            result.Results.Single().Message.Should()
                .Be(ValidationMessages.Attribute_ValidationRule_RequiredAttributeValue.Format("anattributename"));
        }

        [Fact]
        public void WhenValidateAndIsElementAndNotMaterialised_ThenReturnsErrors()
        {
            var element = new Element("anelementname");

            var result = new SolutionItem(element, null)
                .Validate(new ValidationContext());

            result.Results.Single().Context.Path.Should().Be("{anelementname}");
            result.Results.Single().Message.Should()
                .Be(ValidationMessages.SolutionItem_ValidationRule_ElementRequiresAtLeastOneInstance.Format(
                    "anelementname"));
        }

        [Fact]
        public void WhenValidateAndIsCollectionAndNotMaterialised_ThenReturnsErrors()
        {
            var element = new Element("acollectionname", isCollection: true, cardinality: ElementCardinality.OneOrMany);

            var result = new SolutionItem(element, null)
                .Validate(new ValidationContext());

            result.Results.Single().Context.Path.Should().Be("{acollectionname}");
            result.Results.Single().Message.Should()
                .Be(ValidationMessages.SolutionItem_ValidationRule_ElementRequiresAtLeastOneInstance.Format(
                    "acollectionname"));
        }

        [Fact]
        public void WhenValidateAndIsElementWithMissingRequiredAttribute_ThenReturnsErrors()
        {
            var element = new Element("anelementname");
            element.Attributes.Add(new Attribute("anattributename", isRequired: true));

            var result = new SolutionItem(element, null)
                .Materialise()
                .Validate(new ValidationContext());

            result.Results.Single().Context.Path.Should().Be("{anelementname.anattributename}");
            result.Results.Single().Message.Should()
                .Be(ValidationMessages.Attribute_ValidationRule_RequiredAttributeValue.Format("anattributename"));
        }

        [Fact]
        public void WhenValidateAndIsElementWithRequiredItems_ThenReturnsErrors()
        {
            var element1 = new Element("anelementname1");
            var element2 = new Element("anelementname2");
            element1.Elements.Add(element2);

            var result = new SolutionItem(element1, null)
                .Materialise()
                .Validate(new ValidationContext());

            result.Results.Single().Context.Path.Should().Be("{anelementname1.anelementname2}");
            result.Results.Single().Message.Should()
                .Be(ValidationMessages.SolutionItem_ValidationRule_ElementRequiresAtLeastOneInstance.Format(
                    "anelementname2"));
        }

        [Fact]
        public void WhenValidateAndIsCollectionWithRequiredItems_ThenReturnsErrors()
        {
            var collection = new Element("acollectionname", isCollection: true,
                cardinality: ElementCardinality.OneOrMany);

            var result = new SolutionItem(collection, null)
                .Materialise()
                .Validate(new ValidationContext());

            result.Results.Single().Context.Path.Should().Be("{acollectionname}");
            result.Results.Single().Message.Should()
                .Be(ValidationMessages.SolutionItem_ValidationRule_ElementRequiresAtLeastOneInstance.Format(
                    "acollectionname"));
        }

        [Fact]
        public void WhenValidateAndIsCollectionWithTooManyItems_ThenReturnsErrors()
        {
            var collection = new Element("acollectionname", isCollection: true,
                cardinality: ElementCardinality.Single);

            var solutionItem = new SolutionItem(collection, null);
            solutionItem.MaterialiseCollectionItem();
            solutionItem.MaterialiseCollectionItem();

            var result = solutionItem.Validate(new ValidationContext());

            result.Results.Single().Context.Path.Should().Be("{acollectionname}");
            result.Results.Single().Message.Should()
                .Be(ValidationMessages.SolutionItem_ValidationRule_ElementHasMoreThanOneInstance.Format(
                    "acollectionname"));
        }

        [Fact]
        public void WhenValidateAndIsDescendantCollectionWithMissingRequiredAttribute_ThenReturnsErrors()
        {
            var collection = new Element("acollectionname", isCollection: true);
            var element = new Element("anelementname");
            var attribute = new Attribute("anattributename", isRequired: true);
            element.Attributes.Add(attribute);
            collection.Elements.Add(element);

            var solutionItem = new SolutionItem(collection, null);
            solutionItem.MaterialiseCollectionItem();

            var result = solutionItem.Validate(new ValidationContext());

            result.Results.Single().Context.Path.Should().Be($"{{acollectionname.{solutionItem.Items.Single().Id}.anelementname}}");
            result.Results.Single().Message.Should()
                .Be(ValidationMessages.SolutionItem_ValidationRule_ElementRequiresAtLeastOneInstance);
        }

        [Fact]
        public void WhenValidateAndIsAttributeWithMissingRequiredValue_ThenReturnsErrors()
        {
            var attribute = new Attribute("anattributename", isRequired: true);

            var result = new SolutionItem(attribute, null)
                .Validate(new ValidationContext());

            result.Results.Single().Context.Path.Should().Be("{anattributename}");
            result.Results.Single().Message.Should()
                .Be(ValidationMessages.Attribute_ValidationRule_RequiredAttributeValue.Format("anattributename"));
        }

        [Fact]
        public void WhenValidateAndIsAttributeWithWrongDataTypeValue_ThenReturnsErrors()
        {
            var attribute = new Attribute("anattributename");
            var solutionItem = new SolutionItem(attribute, null)
            {
                Value = "awrongvalue"
            };
            attribute.DataType = "int"; //HACK, this should not be possible

            var result = solutionItem
                .Validate(new ValidationContext());

            result.Results.Single().Context.Path.Should().Be("{anattributename}");
            result.Results.Single().Message.Should()
                .Be(ValidationMessages.Attribute_ValidationRule_WrongDataTypeValue.Format("awrongvalue", "int"));
        }

        [Fact]
        public void WhenGetConfiguration_ThenReturnsConfiguration()
        {
            var attribute1 = new Attribute("anattributename1", null, false, "adefaultvalue1");
            var attribute2 = new Attribute("anattributename2", null, false, "adefaultvalue2");
            var attribute3 = new Attribute("anattributename3", "int", false, "25");
            var elementLevel1 = new Element("anelementname1", "adisplayname1", "adescription1");
            var elementLevel2 = new Element("anelementname2", "adisplayname2", "adescription2");
            var collectionLevel1 = new Element("acollectionname2", "adisplayname1", "adescription1", true);
            elementLevel2.Attributes.Add(attribute2);
            collectionLevel1.Attributes.Add(attribute3);
            elementLevel1.Elements.Add(elementLevel2);
            var pattern = new PatternDefinition("apatternname");
            pattern.Attributes.Add(attribute1);
            pattern.Elements.Add(elementLevel1);
            pattern.Elements.Add(collectionLevel1);

            var solutionItem = new SolutionItem(pattern);
            solutionItem.Properties["anelementname1"].Materialise();
            solutionItem.Properties["anelementname1"].Properties["anelementname2"].Materialise();
            solutionItem.Properties["acollectionname2"].MaterialiseCollectionItem();

            var result = solutionItem.GetConfiguration(false);

            result.Should().BeEquivalentTo(new Dictionary<string, object>
            {
                { "id", solutionItem.Id },
                { "anattributename1", "adefaultvalue1" },
                {
                    "anelementname1", new Dictionary<string, object>
                    {
                        { "id", solutionItem.Properties["anelementname1"].Id },
                        {
                            "anelementname2", new Dictionary<string, object>
                            {
                                { "id", solutionItem.Properties["anelementname1"].Properties["anelementname2"].Id },
                                { "anattributename2", "adefaultvalue2" }
                            }
                        }
                    }
                },
                {
                    "acollectionname2", new Dictionary<string, object>
                    {
                        { "id", solutionItem.Properties["acollectionname2"].Id },
                        {
                            "items", new List<object>
                            {
                                new Dictionary<string, object>
                                {
                                    { "id", solutionItem.Properties["acollectionname2"].Items[0].Id },
                                    { "anattributename3", 25 }
                                }
                            }
                        }
                    }
                }
            });
        }

        [Fact]
        public void WhenExecuteCommandAndAutomationNotExists_ThenThrows()
        {
            var pattern = new PatternDefinition("apatternname");
            var solutionItem = new SolutionItem(pattern);

            solutionItem
                .Invoking(x => x.ExecuteCommand(new SolutionDefinition(new ToolkitDefinition(pattern, "1.0")), "acommandname"))
                .Should().Throw<AutomateException>()
                .WithMessage(ExceptionMessages.SolutionItem_UnknownAutomation.Format("acommandname"));
        }

        [Fact]
        public void WhenExecuteCommand_ThenReturnsResult()
        {
            var automation = new TestAutomation("anautomationid");
            var pattern = new PatternDefinition("apatternname");
            pattern.Automation.Add(automation);
            var solutionItem = new SolutionItem(pattern);
            var solution = new SolutionDefinition(new ToolkitDefinition(pattern, "1.0"));

            var result = solutionItem.ExecuteCommand(solution, "anautomationname");

            result.CommandName.Should().Be("anautomationname");
            result.IsSuccess.Should().BeTrue();
            result.Log.Should().ContainSingle("alogentry");
            result.ValidationErrors.Should().BeEmpty();
        }
    }
}