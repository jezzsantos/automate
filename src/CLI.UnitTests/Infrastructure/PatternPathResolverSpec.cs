using System;
using automate;
using automate.Domain;
using automate.Extensions;
using automate.Infrastructure;
using FluentAssertions;
using Xunit;

namespace CLI.UnitTests.Infrastructure
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
                .Invoking(x => x.Resolve(new PatternDefinition("apatternname"), "notavalidformat"))
                .Should().Throw<PatternException>()
                .WithMessage(ExceptionMessages.PatternPathResolver_InvalidExpression.Format("notavalidformat"));
        }

        [Fact]
        public void WhenResolveAndExpressionIsEmpty_ThenThrows()
        {
            this.resolver
                .Invoking(x => x.Resolve(new PatternDefinition("apatternname"), "{}"))
                .Should().Throw<PatternException>()
                .WithMessage(ExceptionMessages.PatternPathResolver_InvalidExpression.Format("{}"));
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
            var element = new Element("anelementname", null, null, false);
            pattern.Elements.Add(element);

            var result = this.resolver.Resolve(pattern, "{apatternname.anelementname}");

            result.Should().Be(element);
        }

        [Fact]
        public void WhenResolveAndDeepElementExpressionNotExist_ThenReturnsNull()
        {
            var pattern = new PatternDefinition("apatternname");
            var elementLevel1 = new Element("anelementname1", null, null, false);
            pattern.Elements.Add(elementLevel1);
            var elementLevel2 = new Element("anelementname2", null, null, false);
            elementLevel1.Elements.Add(elementLevel2);

            var result = this.resolver.Resolve(pattern, "{apatternname.anelementname1.anelementname2.anuknownelement}");

            result.Should().BeNull();
        }

        [Fact]
        public void WhenResolveAndDeepElementExpressionExists_ThenReturnsElement()
        {
            var pattern = new PatternDefinition("apatternname");
            var elementLevel1 = new Element("anelementname1", null, null, false);
            pattern.Elements.Add(elementLevel1);
            var elementLevel2 = new Element("anelementname2", null, null, false);
            elementLevel1.Elements.Add(elementLevel2);
            var elementLevel3 = new Element("anelementname3", null, null, false);
            elementLevel2.Elements.Add(elementLevel3);

            var result = this.resolver.Resolve(pattern, "{apatternname.anelementname1.anelementname2.anelementname3}");

            result.Should().Be(elementLevel3);
        }

        [Fact]
        public void WhenResolveAndCollectionExpressionExists_ThenReturnsElement()
        {
            var pattern = new PatternDefinition("apatternname");
            var collection = new Element("acollectionname", null, null, true);
            pattern.Elements.Add(collection);

            var result = this.resolver.Resolve(pattern, "{apatternname.acollectionname}");

            result.Should().Be(collection);
        }

        [Fact]
        public void WhenResolveAndDeepCollectionExpressionNotExist_ThenReturnsNull()
        {
            var pattern = new PatternDefinition("apatternname");
            var collectionLevel1 = new Element("acollectionname1", null, null, true);
            pattern.Elements.Add(collectionLevel1);
            var collectionLevel2 = new Element("acollectionname2", null, null, true);
            collectionLevel1.Elements.Add(collectionLevel2);

            var result = this.resolver.Resolve(pattern,
                "{apatternname.acollectionname1.acollectionname2.anuknowncollection}");

            result.Should().BeNull();
        }

        [Fact]
        public void WhenResolveAndDeepCollectionExpressionExists_ThenReturnsElement()
        {
            var pattern = new PatternDefinition("apatternname");
            var collectionLevel1 = new Element("acollectionname1", null, null, true);
            pattern.Elements.Add(collectionLevel1);
            var collectionLevel2 = new Element("acollectionname2", null, null, true);
            collectionLevel1.Elements.Add(collectionLevel2);
            var collectionLevel3 = new Element("acollectionname3", null, null, true);
            collectionLevel2.Elements.Add(collectionLevel3);

            var result = this.resolver.Resolve(pattern,
                "{apatternname.acollectionname1.acollectionname2.acollectionname3}");

            result.Should().Be(collectionLevel3);
        }
    }
}