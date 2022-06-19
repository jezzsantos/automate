using System;
using Automate.Authoring.Domain;
using Automate.Common;
using Automate.Common.Extensions;
using Automate.Runtime.Domain;
using Automate.Runtime.Infrastructure;
using FluentAssertions;
using Xunit;
using Attribute = Automate.Authoring.Domain.Attribute;

namespace Core.UnitTests.Runtime.Infrastructure
{
    [Trait("Category", "Unit")]
    public class DraftPathResolverSpec
    {
        private readonly DraftPathResolver resolver;

        public DraftPathResolverSpec()
        {
            this.resolver = new DraftPathResolver();
        }

        [Fact]
        public void WhenResolveAndDraftIsNull_ThenReturnsNull()
        {
            this.resolver
                .Invoking(x => x.ResolveItem(null, "anexpression"))
                .Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void WhenResolveAndExpressionIsNull_ThenReturnsNull()
        {
            this.resolver
                .Invoking(x =>
                    x.ResolveItem(
                        new DraftDefinition(new ToolkitDefinition(
                            new PatternDefinition("apatternname"))),
                        null))
                .Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void WhenResolveAndExpressionIsInvalidFormat_ThenThrows()
        {
            this.resolver
                .Invoking(x =>
                    x.ResolveItem(
                        new DraftDefinition(new ToolkitDefinition(
                            new PatternDefinition("apatternname"))),
                        "notavalidexpression"))
                .Should().Throw<AutomateException>()
                .WithMessage(ExceptionMessages.DraftPathResolver_InvalidExpression.Substitute("notavalidexpression"));
        }

        [Fact]
        public void WhenResolveAndExpressionIsEmpty_ThenThrows()
        {
            this.resolver
                .Invoking(x =>
                    x.ResolveItem(
                        new DraftDefinition(new ToolkitDefinition(
                            new PatternDefinition("apatternname"))),
                        "{}"))
                .Should().Throw<AutomateException>()
                .WithMessage(ExceptionMessages.DraftPathResolver_InvalidExpression.Substitute("{}"));
        }

        [Fact]
        public void WhenResolveAndExpressionIsJustNameOfPattern_ThenReturnsPattern()
        {
            var draft = new DraftDefinition(new ToolkitDefinition(new PatternDefinition("apatternname")));

            var result = this.resolver.ResolveItem(draft, "{apatternname}");

            result.Should().Be(draft.Model);
        }

        [Fact]
        public void WhenResolveAndExpressionElementNotExist_ThenReturnsNull()
        {
            var draft = new DraftDefinition(new ToolkitDefinition(new PatternDefinition("apatternname")));

            var result = this.resolver.ResolveItem(draft, "{anunknownelementname}");

            result.Should().BeNull();
        }

        [Fact]
        public void WhenResolveAndElementExpressionExists_ThenReturnsElement()
        {
            var pattern = new PatternDefinition("apatternname");
            var element = new Element("anelementname");
            pattern.AddElement(element);
            var draft = new DraftDefinition(new ToolkitDefinition(pattern));

            var result = this.resolver.ResolveItem(draft, "{anelementname}");

            result.ElementSchema.Element.Should().Be(element);
        }

        [Fact]
        public void WhenResolveAndCollectionItemExpressionExists_ThenReturnsElement()
        {
            var pattern = new PatternDefinition("apatternname");
            var collection = new Element("acollectionname",
                ElementCardinality.OneOrMany);
            var element = new Element("anelementname");
            collection.AddElement(element);
            pattern.AddElement(collection);
            var draft = new DraftDefinition(new ToolkitDefinition(pattern));
            var collectionInstance = draft.Model.Properties["acollectionname"].MaterialiseCollectionItem();
            var elementInstance = collectionInstance.Properties["anelementname"].Materialise();

            var result = this.resolver.ResolveItem(draft,
                $"{{apatternname.acollectionname.{collectionInstance.Id}.anelementname}}");

            result.Id.Should().Be(elementInstance.Id);
        }

        [Fact]
        public void WhenResolveAndFullyQualifiedElementExpressionExists_ThenReturnsElement()
        {
            var pattern = new PatternDefinition("apatternname");
            var element = new Element("anelementname");
            pattern.AddElement(element);
            var draft = new DraftDefinition(new ToolkitDefinition(pattern));

            var result = this.resolver.ResolveItem(draft, "{apatternname.anelementname}");

            result.ElementSchema.Element.Should().Be(element);
        }

        [Fact]
        public void WhenResolveAndPartiallyQualifiedElementExpressionExists_ThenReturnsElement()
        {
            var pattern = new PatternDefinition("apatternname");
            var element = new Element("anelementname");
            pattern.AddElement(element);
            var draft = new DraftDefinition(new ToolkitDefinition(pattern));

            var result = this.resolver.ResolveItem(draft, "{anelementname}");

            result.ElementSchema.Element.Should().Be(element);
        }

        [Fact]
        public void WhenResolveAndDescendantElementExpressionNotMaterialised_ThenReturnsNull()
        {
            var pattern = new PatternDefinition("apatternname");
            var element3 = new Element("anelementname3", autoCreate: false);
            var element2 = new Element("anelementname2", autoCreate: false);
            var element1 = new Element("anelementname1");
            element2.AddElement(element3);
            element1.AddElement(element2);
            pattern.AddElement(element1);
            var draft = new DraftDefinition(new ToolkitDefinition(pattern));
            draft.Model.Properties["anelementname1"].Materialise();

            var result = this.resolver.ResolveItem(draft, "{anelementname1.anelementname2.anelementname3}");

            result.Should().BeNull();
        }

        [Fact]
        public void WhenResolveAndDescendantElementExpressionExists_ThenReturnsElement()
        {
            var pattern = new PatternDefinition("apatternname");
            var element3 = new Element("anelementname3");
            var element2 = new Element("anelementname2");
            var element1 = new Element("anelementname1");
            element2.AddElement(element3);
            element1.AddElement(element2);
            pattern.AddElement(element1);
            var draft = new DraftDefinition(new ToolkitDefinition(pattern));
            draft.Model.Properties["anelementname1"].Materialise().Properties["anelementname2"].Materialise()
                .Properties["anelementname3"].Materialise();

            var result = this.resolver.ResolveItem(draft, "{anelementname1.anelementname2.anelementname3}");

            result.ElementSchema.Element.Should().Be(element3);
        }

        [Fact]
        public void WhenResolveExpressionAndExpressionIsNull_ThenReturnsNull()
        {
            var toolkit = new ToolkitDefinition(new PatternDefinition("apatternname"));

            var result = this.resolver.ResolveExpression("adescription", null,
                new DraftItem(toolkit,
                    new Element("anelementname"), null));

            result.Should().BeNull();
        }

        [Fact]
        public void WhenResolveExpressionAndExpressionContainsNoSyntax_ThenReturnsExpression()
        {
            var toolkit = new ToolkitDefinition(new PatternDefinition("apatternname"));

            var result = this.resolver.ResolveExpression("adescription", "anexpression",
                new DraftItem(toolkit,
                    new Element("anelementname"), null));

            result.Should().Be("anexpression");
        }

        [Fact]
        public void WhenResolveExpressionAndExpressionContainsSyntax_ThenReturnsExpression()
        {
            var pattern = new PatternDefinition("apatternname");
            var element = new Element("anelementname");
            var attribute = new Attribute("anattributename", defaultValue: "adefaultvalue");
            element.AddAttribute(attribute);
            pattern.AddElement(element);
            var draftItem = new DraftItem(new ToolkitDefinition(pattern), element,
                null);
            draftItem.Materialise();

            var result = this.resolver.ResolveExpression("adescription", "anexpression{{anattributename}}anexpression",
                draftItem);

            result.Should().Be("anexpressionadefaultvalueanexpression");
        }

        [Fact]
        public void WhenResolveExpressionAndExpressionContainsParentSyntax_ThenReturnsExpression()
        {
            var pattern = new PatternDefinition("apatternname");
            var element = new Element("anelementname");
            var attribute = new Attribute("anattributename", defaultValue: "adefaultvalue");
            element.AddAttribute(attribute);
            pattern.AddElement(element);
            var draftItem = new DraftItem(new ToolkitDefinition(pattern), pattern);
            draftItem.Properties["anelementname"].Materialise();

            var result = this.resolver.ResolveExpression("adescription",
                "anexpression{{anelementname.Parent.anelementname.anattributename}}anexpression", draftItem);

            result.Should().Be("anexpressionadefaultvalueanexpression");
        }
    }
}