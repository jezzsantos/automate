﻿using System.Linq;
using automate;
using automate.Domain;
using automate.Extensions;
using FluentAssertions;
using Xunit;

namespace CLI.UnitTests.Domain
{
    [Trait("Category", "Unit")]
    public class SolutionItemSpec
    {
        [Fact]
        public void WhenConstructedWithValue_ThenValueAssigned()
        {
            var result = new SolutionItem("avalue", Attribute.DefaultType);

            result.IsMaterialised.Should().BeTrue();
            result.Value.Should().Be("avalue");
        }

        [Fact]
        public void WhenConstructedWithPattern_ThenPatternAssigned()
        {
            var pattern = new PatternDefinition("apatternname")
            {
                Description = "adescription"
            };

            var result = new SolutionItem(pattern);

            result.Should().NotBeNull();
            result.Value.Should().BeNull();
            result.IsMaterialised.Should().BeTrue();
        }

        [Fact]
        public void WhenConstructedWithAttributeWithoutDefaultValue_ThenAttributeAssigned()
        {
            var pattern = new PatternDefinition("apatternname");
            var attribute = new Attribute("aname");
            pattern.Attributes.Add(attribute);

            var result = new SolutionItem(pattern);

            result.Should().NotBeNull();
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
            result.Properties["aname"].AttributeSchema.Should().Be(attribute);
            result.Properties["aname"].Value.Should().Be(attribute.DefaultValue);
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
            result.Properties["acollectionname"].ElementSchema.Should().Be(element);
            result.Properties["acollectionname"].Value.Should().BeNull();
            result.Properties["acollectionname"].IsMaterialised.Should().BeFalse();
            result.Properties["acollectionname"].Items.Should().BeNull();
        }

        [Fact]
        public void WhenConstructedWithDeepSchema_ThenDeepElementsAssigned()
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
            new SolutionItem(25, "int")
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

            var result = new SolutionItem(element)
                .Materialise();

            result.IsMaterialised.Should().BeTrue();
            result.Value.Should().BeNull();
            result.Properties["anattributename"].Value.Should().Be("adefaultvalue");
            result.Items.Should().BeNull();
        }

        [Fact]
        public void WhenMaterialiseAndCollection_ThenMaterialises()
        {
            var element = new Element("anelementname", "adisplayname", "adescription", true);
            var attribute = new Attribute("anattributename", null, false, "adefaultvalue");
            element.Attributes.Add(attribute);

            var result = new SolutionItem(element)
                .Materialise();

            result.IsMaterialised.Should().BeTrue();
            result.Value.Should().BeNull();
            result.Properties.Should().BeEmpty();
            result.Items.Should().BeEmpty();
        }

        [Fact]
        public void WhenMaterialiseAndAttribute_ThenMaterialises()
        {
            var attribute = new Attribute("anattributename", null, false, "adefaultvalue");

            var result = new SolutionItem(attribute)
                .Materialise("avalue");

            result.IsMaterialised.Should().BeTrue();
            result.Value.Should().Be("avalue");
        }

        [Fact]
        public void WhenMaterialiseCollectionItemAndNotACollection_ThenThrows()
        {
            var element = new Element("anelementname", "adisplayname", "adescription");

            new SolutionItem(element)
                .Invoking(x => x.MaterialiseCollectionItem())
                .Should().Throw<AutomateException>()
                .WithMessage(ExceptionMessages.SolutionItem_MaterialiseNotACollection);
        }

        [Fact]
        public void WhenMaterialiseCollectionItem_ThenMaterialisesNewElement()
        {
            var item = new SolutionItem(new Element("anelementname", "adisplayname", "adescription", true));

            var result = item.MaterialiseCollectionItem();

            result.IsMaterialised.Should().BeTrue();
            result.Value.Should().BeNull();
            result.Items.Should().BeNull();

            item.IsMaterialised.Should().BeTrue();
            item.Items.Should().Contain(result);
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

            var result = new SolutionItem(element)
                .HasAttribute("anattributename");

            result.Should().BeTrue();
        }

        [Fact]
        public void WhenHasAttributeAndPropertyNotInSchema_ThenReturnsFalse()
        {
            var element = new Element("anelementname", "adisplayname", "adescription");

            var result = new SolutionItem(element)
                .HasAttribute("anattributename");

            result.Should().BeFalse();
        }

        [Fact]
        public void WhenHasAttributeAndAttribute_ThenReturnsFalse()
        {
            var result = new SolutionItem(new Attribute("anattributename", null))
                .HasAttribute("anattributename");

            result.Should().BeFalse();
        }

        [Fact]
        public void WhenHasAttributeAndValue_ThenReturnsFalse()
        {
            var result = new SolutionItem("avalue", Attribute.DefaultType)
                .HasAttribute("anattributename");

            result.Should().BeFalse();
        }

        [Fact]
        public void WhenGetPropertyAndNotExists_ThenThrows()
        {
            new SolutionItem("avalue", Attribute.DefaultType)
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

            new SolutionItem(element)
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

            var result = new SolutionItem(element)
                .Materialise()
                .GetProperty("anattributename");

            result.Name.Should().Be("anattributename");
        }

        [Fact]
        public void WhenValidateAndIsPatternWithInvalidProperties_ThenReturnsErrors()
        {
            var pattern = new PatternDefinition("apatternname");
            pattern.Attributes.Add(new Attribute("anattributename", isRequired: true));

            var result = new SolutionItem(pattern)
                .Validate(new ValidationContext());

            result.Results.Single().Message.Should()
                .Be(ValidationMessages.Attribute_ValidationRule_RequiredValue.Format("anattributename"));
        }

        [Fact]
        public void WhenValidateAndIsElementAndNotMaterialised_ThenReturnsErrors()
        {
            var element = new Element("anelementname");

            var result = new SolutionItem(element)
                .Validate(new ValidationContext());

            result.Results.First().Message.Should()
                .Be(ValidationMessages.SolutionItem_ValidationRule_ElementRequiresAtLeastOneInstance.Format(
                    "anelementname"));
        }

        [Fact]
        public void WhenValidateAndIsCollectionAndNotMaterialised_ThenReturnsErrors()
        {
            var element = new Element("acollectionname", isCollection: true, cardinality: ElementCardinality.OneOrMany);

            var result = new SolutionItem(element)
                .Validate(new ValidationContext());

            result.Results.First().Message.Should()
                .Be(ValidationMessages.SolutionItem_ValidationRule_ElementRequiresAtLeastOneInstance.Format(
                    "acollectionname"));
        }

        [Fact]
        public void WhenValidateAndIsElementWithInvalidProperties_ThenReturnsErrors()
        {
            var element = new Element("anelementname");
            element.Attributes.Add(new Attribute("anattributename", isRequired: true));

            var result = new SolutionItem(element)
                .Materialise()
                .Validate(new ValidationContext());

            result.Results.Single().Message.Should()
                .Be(ValidationMessages.Attribute_ValidationRule_RequiredValue.Format("anattributename"));
        }

        [Fact]
        public void WhenValidateAndIsElementWithMissingItems_ThenReturnsErrors()
        {
            var element1 = new Element("anelementname1");
            var element2 = new Element("anelementname2");
            element1.Elements.Add(element2);

            var result = new SolutionItem(element1)
                .Materialise()
                .Validate(new ValidationContext());

            result.Results.Single().Message.Should()
                .Be(ValidationMessages.SolutionItem_ValidationRule_ElementRequiresAtLeastOneInstance.Format(
                    "anelementname2"));
        }

        [Fact]
        public void WhenValidateAndIsCollectionWithMissingItems_ThenReturnsErrors()
        {
            var collection = new Element("acollectionname", isCollection: true,
                cardinality: ElementCardinality.OneOrMany);

            var result = new SolutionItem(collection)
                .Materialise()
                .Validate(new ValidationContext());

            result.Results.Single().Message.Should()
                .Be(ValidationMessages.SolutionItem_ValidationRule_ElementRequiresAtLeastOneInstance.Format(
                    "acollectionname"));
        }

        [Fact]
        public void WhenValidateAndIsCollectionWithTooManyItems_ThenReturnsErrors()
        {
            var collection = new Element("acollectionname", isCollection: true,
                cardinality: ElementCardinality.Single);

            var solutionItem = new SolutionItem(collection);
            solutionItem.MaterialiseCollectionItem();
            solutionItem.MaterialiseCollectionItem();

            var result = solutionItem.Validate(new ValidationContext());

            result.Results.Single().Message.Should()
                .Be(ValidationMessages.SolutionItem_ValidationRule_ElementHasMoreThanOneInstance.Format(
                    "acollectionname"));
        }

        [Fact]
        public void WhenValidateAndIsAttributeWithInvalidValue_ThenReturnsErrors()
        {
            var attribute = new Attribute("anattributename", isRequired: true);

            var result = new SolutionItem(attribute)
                .Validate(new ValidationContext());

            result.Results.Single().Message.Should()
                .Be(ValidationMessages.Attribute_ValidationRule_RequiredValue.Format("anattributename"));
        }

        [Fact]
        public void WhenValidateAndIsAttributeWithWrongDataTypeValue_ThenReturnsErrors()
        {
            var attribute = new Attribute("anattributename", "int");
            var solutionItem = new SolutionItem(attribute)
            {
                Value = "awrongvalue"
            };

            var result = solutionItem
                .Validate(new ValidationContext());

            result.Results.Single().Message.Should()
                .Be(ValidationMessages.Attribute_ValidationRule_WrongDataTypeValue.Format("awrongvalue", "int"));
        }
    }
}