using System;
using Automate;
using Automate.Domain;
using Automate.Extensions;
using Automate.Infrastructure;
using FluentAssertions;
using Xunit;

namespace Core.UnitTests.Infrastructure
{
    [Trait("Category", "Unit")]
    public class PatternPathResolverSpec
    {
        private readonly PatternPathResolver resolver;

        public PatternPathResolverSpec()
        {
            this.resolver = new PatternPathResolver();
        }

        [Fact]
        public void WhenResolveAndPatternIsNull_ThenThrows()
        {
            this.resolver
                .Invoking(x => x.Resolve(null, "anexpression"))
                .Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void WhenResolveAndExpressionIsNull_ThenThrows()
        {
            this.resolver
                .Invoking(x => x.Resolve(new PatternDefinition("apatternname"), null))
                .Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void WhenResolveAndExpressionIsInvalidFormat_ThenThrows()
        {
            this.resolver
                .Invoking(x => x.Resolve(new PatternDefinition("apatternname"),
                    "notavalidformat"))
                .Should().Throw<AutomateException>()
                .WithMessage(ExceptionMessages.PatternPathResolver_InvalidExpression.Substitute("notavalidformat"));
        }

        [Fact]
        public void WhenResolveAndExpressionIsEmpty_ThenThrows()
        {
            this.resolver
                .Invoking(x => x.Resolve(new PatternDefinition("apatternname"), "{}"))
                .Should().Throw<AutomateException>()
                .WithMessage(ExceptionMessages.PatternPathResolver_InvalidExpression.Substitute("{}"));
        }

        [Fact]
        public void WhenResolveAndExpressionIsNotStartWithNameOfPattern_ThenReturnsNull()
        {
            var pattern = new PatternDefinition("apatternname");

            var result = this.resolver.Resolve(pattern, "{anunknownpattern}");

            result.Should().BeNull();
        }

        [Fact]
        public void WhenResolveAndExpressionIsNameOfPattern_ThenReturnsPattern()
        {
            var pattern = new PatternDefinition("apatternname");

            var result = this.resolver.Resolve(pattern, "{apatternname}");

            result.Should().Be(pattern);
        }

        [Fact]
        public void WhenResolveAndExpressionNotExist_ThenReturnsNull()
        {
            var pattern = new PatternDefinition("apatternname");

            var result = this.resolver.Resolve(pattern, "{apatternname.anuknownelement}");

            result.Should().BeNull();
        }

        [Fact]
        public void WhenResolveAndElementExpressionExists_ThenReturnsElement()
        {
            var pattern = new PatternDefinition("apatternname");
            var element = new Element("anelementname");
            pattern.AddElement(element);

            var result = this.resolver.Resolve(pattern, "{apatternname.anelementname}");

            result.Should().Be(element);
        }

        [Fact]
        public void WhenResolveAndDescendantElementExpressionNotExist_ThenReturnsNull()
        {
            var pattern = new PatternDefinition("apatternname");
            var elementLevel1 = new Element("anelementname1");
            pattern.AddElement(elementLevel1);
            var elementLevel2 = new Element("anelementname2");
            elementLevel1.AddElement(elementLevel2);

            var result = this.resolver.Resolve(pattern, "{apatternname.anelementname1.anelementname2.anuknownelement}");

            result.Should().BeNull();
        }

        [Fact]
        public void WhenResolveAndDescendantElementExpressionExists_ThenReturnsElement()
        {
            var pattern = new PatternDefinition("apatternname");
            var elementLevel1 = new Element("anelementname1");
            pattern.AddElement(elementLevel1);
            var elementLevel2 = new Element("anelementname2");
            elementLevel1.AddElement(elementLevel2);
            var elementLevel3 = new Element("anelementname3");
            elementLevel2.AddElement(elementLevel3);

            var result = this.resolver.Resolve(pattern, "{apatternname.anelementname1.anelementname2.anelementname3}");

            result.Should().Be(elementLevel3);
        }

        [Fact]
        public void WhenResolveAndCollectionExpressionExists_ThenReturnsElement()
        {
            var pattern = new PatternDefinition("apatternname");
            var collection = new Element("acollectionname", displayName: null,
                description: null);
            pattern.AddElement(collection);

            var result = this.resolver.Resolve(pattern, "{apatternname.acollectionname}");

            result.Should().Be(collection);
        }

        [Fact]
        public void WhenResolveAndDescendantCollectionExpressionNotExist_ThenReturnsNull()
        {
            var pattern = new PatternDefinition("apatternname");
            var collectionLevel1 = new Element("acollectionname1", displayName: null,
                description: null);
            pattern.AddElement(collectionLevel1);
            var collectionLevel2 = new Element("acollectionname2", displayName: null,
                description: null);
            collectionLevel1.AddElement(collectionLevel2);

            var result = this.resolver.Resolve(pattern,
                "{apatternname.acollectionname1.acollectionname2.anuknowncollection}");

            result.Should().BeNull();
        }

        [Fact]
        public void WhenResolveAndDescendantCollectionExpressionExists_ThenReturnsElement()
        {
            var pattern = new PatternDefinition("apatternname");
            var collectionLevel1 = new Element("acollectionname1", displayName: null,
                description: null);
            pattern.AddElement(collectionLevel1);
            var collectionLevel2 = new Element("acollectionname2", displayName: null,
                description: null);
            collectionLevel1.AddElement(collectionLevel2);
            var collectionLevel3 = new Element("acollectionname3", displayName: null,
                description: null);
            collectionLevel2.AddElement(collectionLevel3);

            var result = this.resolver.Resolve(pattern,
                "{apatternname.acollectionname1.acollectionname2.acollectionname3}");

            result.Should().Be(collectionLevel3);
        }
    }
}