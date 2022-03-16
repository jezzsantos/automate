using System;
using System.Collections.Generic;
using System.Linq;
using Automate.CLI;
using Automate.CLI.Domain;
using Automate.CLI.Extensions;
using FluentAssertions;
using Xunit;
using Attribute = Automate.CLI.Domain.Attribute;
using StringExtensions = ServiceStack.StringExtensions;

namespace CLI.UnitTests.Domain
{
    [Trait("Category", "Unit")]
    public class SolutionItemSpec
    {
        [Fact]
        public void WhenConstructedWithPattern_ThenPatternAssigned()
        {
            var pattern = new PatternDefinition("apatternname");

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
            var parent = new SolutionItem(new Element("anelementname"), null);
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
            pattern.AddAttribute(attribute);

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
            pattern.AddAttribute(attribute);

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
            pattern.AddAttribute(attribute);

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
            element.AddAttribute(new Attribute("anattributename", "string", false, "adefaultvalue"));
            pattern.AddElement(element);

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
            element.AddAttribute(new Attribute("anattributename", "string", false, "adefaultvalue"));
            pattern.AddElement(element);

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
            element3.AddAttribute(new Attribute("anattributename3", "string", false, "adefaultvalue3"));
            var element2 = new Element("anelementname2", "adisplayname2", "adescription2", true);
            element2.AddAttribute(new Attribute("anattributename2", "string", false, "adefaultvalue2"));
            var element1 = new Element("anelementname1", "adisplayname1", "adescription1", true);
            element1.AddAttribute(new Attribute("anattributename1", "string", false, "adefaultvalue1"));
            element2.AddElement(element3);
            element1.AddElement(element2);
            pattern.AddElement(element1);

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
            element.AddAttribute(attribute);

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
            collection.AddAttribute(attribute);
            collection.AddElement(element);

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
            var collection = new Element("acollectionname", isCollection: true);
            var attribute = new Attribute("anattributename", defaultValue: "adefaultvalue");
            collection.AddAttribute(attribute);
            var solutionItem = new SolutionItem(collection, null);

            var result = solutionItem.MaterialiseCollectionItem();

            result.Id.Should().NotBeNull();
            result.IsMaterialised.Should().BeTrue();
            result.IsCollection.Should().BeFalse();
            result.ElementSchema.Name.Should().Be("acollectionname");
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
            var collection1 = new Element("acollectionname1", isCollection: true);
            var attribute = new Attribute("anattributename", defaultValue: "adefaultvalue");
            collection1.AddAttribute(attribute);
            var collection2 = new Element("acollectionname2", isCollection: true);
            collection1.AddElement(collection2);
            var solutionItem = new SolutionItem(collection1, null);

            var result1 = solutionItem.MaterialiseCollectionItem();

            result1.Id.Should().NotBeNull();
            result1.IsMaterialised.Should().BeTrue();
            result1.IsCollection.Should().BeFalse();
            result1.ElementSchema.Name.Should().Be("acollectionname1");
            result1.Value.Should().BeNull();
            result1.Items.Should().BeNull();
            result1.Properties.Should().Contain(prop =>
                prop.Key == "anattributename" && (string)prop.Value.Value == "adefaultvalue");
            result1.Properties.Should().Contain(prop =>
                prop.Key == "acollectionname2" && prop.Value.IsCollection == true);

            var result2 = result1.Properties["acollectionname2"].MaterialiseCollectionItem();
            result2.IsMaterialised.Should().BeTrue();
            result2.IsCollection.Should().BeFalse();
            result2.ElementSchema.Name.Should().Be("acollectionname2");
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
            var pattern = new PatternDefinition("apatternname");
            var attribute = new Attribute("anattributename", null, false, "adefaultvalue");
            pattern.AddAttribute(attribute);

            var result = new SolutionItem(pattern)
                .HasAttribute("anattributename");

            result.Should().BeTrue();
        }

        [Fact]
        public void WhenHasAttributeAndPropertyInElementSchema_ThenReturnsTrue()
        {
            var element = new Element("anelementname", "adisplayname", "adescription");
            var attribute = new Attribute("anattributename", null, false, "adefaultvalue");
            element.AddAttribute(attribute);

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
            pattern.AddElement(element);

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
            element.AddAttribute(attribute);

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
            element.AddAttribute(attribute);

            var result = new SolutionItem(element, null)
                .Materialise()
                .GetProperty("anattributename");

            result.Name.Should().Be("anattributename");
        }

        [Fact]
        public void WhenValidateAndIsPatternWithMissingRequiredAttribute_ThenReturnsErrors()
        {
            var pattern = new PatternDefinition("apatternname");
            pattern.AddAttribute(new Attribute("anattributename", isRequired: true));

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
            element.AddAttribute(new Attribute("anattributename", isRequired: true));

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
            element1.AddElement(element2);

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
            element.AddAttribute(attribute);
            collection.AddElement(element);

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
            attribute.SetDataType("int");

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
            elementLevel2.AddAttribute(attribute2);
            collectionLevel1.AddAttribute(attribute3);
            elementLevel1.AddElement(elementLevel2);
            var pattern = new PatternDefinition("apatternname");
            pattern.AddAttribute(attribute1);
            pattern.AddElement(elementLevel1);
            pattern.AddElement(collectionLevel1);

            var solutionItem = new SolutionItem(pattern);
            solutionItem.Properties["anelementname1"].Materialise();
            solutionItem.Properties["anelementname1"].Properties["anelementname2"].Materialise();
            solutionItem.Properties["acollectionname2"].MaterialiseCollectionItem();

            var result = StringExtensions.ToJson(solutionItem.GetConfiguration(false));

            result.Should().Be(StringExtensions.ToJson(new Dictionary<string, object>
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
            }));
        }

        [Fact]
        public void WhenExecuteCommandAndAutomationNotExists_ThenThrows()
        {
            var pattern = new PatternDefinition("apatternname");
            var solutionItem = new SolutionItem(pattern);

            solutionItem
                .Invoking(x => x.ExecuteCommand(new SolutionDefinition(new ToolkitDefinition(pattern)), "acommandname"))
                .Should().Throw<AutomateException>()
                .WithMessage(ExceptionMessages.SolutionItem_UnknownAutomation.Format("acommandname"));
        }

        [Fact]
        public void WhenExecuteCommand_ThenReturnsResult()
        {
            var automation = new Automation("acommandname", AutomationType.TestingOnly, new Dictionary<string, object>());
            var pattern = new PatternDefinition("apatternname");
            pattern.AddAutomation(automation);
            var solutionItem = new SolutionItem(pattern);
            var solution = new SolutionDefinition(new ToolkitDefinition(pattern));

            var result = solutionItem.ExecuteCommand(solution, "acommandname");

            result.CommandName.Should().Be("acommandname");
            result.IsSuccess.Should().BeTrue();
            result.Log.Should().ContainSingle("testingonly");
            result.ValidationErrors.Should().BeEmpty();
        }

        [Fact]
        public void WhenSetPropertiesAndAnyPropertyLeftHandSideOfAssigmentInvalid_ThenThrows()
        {
            var pattern = new PatternDefinition("apatternname");
            var solutionItem = new SolutionItem(pattern);

            solutionItem
                .Invoking(x => x.SetProperties(new Dictionary<string, string>
                {
                    { "notavalidpropertyassignment", "" }
                }))
                .Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage(
                    ExceptionMessages.SolutionItem_ConfigureSolution_PropertyAssignmentInvalid.Format(
                        "notavalidpropertyassignment=", "notavalidpropertyassignment", solutionItem.Id) + "*");
        }

        [Fact]
        public void WhenSetPropertiesAndAnyPropertyRightHandSideOfAssigmentInvalid_ThenThrows()
        {
            var pattern = new PatternDefinition("apatternname");
            var solutionItem = new SolutionItem(pattern);

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
            var pattern = new PatternDefinition("apatternname");
            var solutionItem = new SolutionItem(pattern);

            solutionItem
                .Invoking(x => x.SetProperties(new Dictionary<string, string>
                {
                    { "anunknownattribute", "avalue" }
                }))
                .Should().Throw<AutomateException>()
                .WithMessage(ExceptionMessages.SolutionItem_ConfigureSolution_ElementPropertyNotExists.Format("apatternname", "anunknownattribute"));
        }

        [Fact]
        public void WhenSetPropertiesWithWithPropertyOfWrongChoice_ThenThrows()
        {
            var pattern = new PatternDefinition("apatternname");
            pattern.AddAttribute(new Attribute("anattributename", choices: new List<string> { "avalue" }));
            var solutionItem = new SolutionItem(pattern);

            solutionItem
                .Invoking(x => x.SetProperties(new Dictionary<string, string>
                {
                    { "anattributename", "awrongvalue" }
                }))
                .Should().Throw<AutomateException>()
                .WithMessage(ExceptionMessages.SolutionItem_ConfigureSolution_ElementPropertyValueIsNotOneOf.Format("apatternname", "anattributename", new List<string> { "avalue" }.Join(";"), "awrongvalue"));
        }

        [Fact]
        public void WhenSetPropertiesWithPropertyOfWrongDataType_ThenThrows()
        {
            var pattern = new PatternDefinition("apatternname");
            pattern.AddAttribute(new Attribute("anattributename", "int"));
            var solutionItem = new SolutionItem(pattern);

            solutionItem
                .Invoking(x => x.SetProperties(new Dictionary<string, string>
                {
                    { "anattributename", "astring" }
                }))
                .Should().Throw<AutomateException>()
                .WithMessage(ExceptionMessages.SolutionItem_ConfigureSolution_ElementPropertyValueNotCompatible.Format("apatternname", "anattributename", "int", "astring"));
        }

        [Fact]
        public void WhenSetPropertiesAndPropertyChoice_ThenUpdatesProperties()
        {
            var pattern = new PatternDefinition("apatternname");
            pattern.AddAttribute(new Attribute("anattributename", choices: new List<string> { "avalue" }));
            var solutionItem = new SolutionItem(pattern);

            solutionItem.SetProperties(new Dictionary<string, string>
            {
                { "anattributename", "avalue" }
            });

            solutionItem.Properties["anattributename"].Value.Should().Be("avalue");
        }

        [Fact]
        public void WhenSetProperties_ThenUpdatesProperties()
        {
            var pattern = new PatternDefinition("apatternname");
            pattern.AddAttribute(new Attribute("anattributename1"));
            pattern.AddAttribute(new Attribute("anattributename2"));
            pattern.AddAttribute(new Attribute("anattributename3"));
            var solutionItem = new SolutionItem(pattern);

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
    }
}