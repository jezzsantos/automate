﻿using System;
using Automate.CLI;
using Automate.CLI.Domain;
using Automate.CLI.Extensions;
using Automate.CLI.Infrastructure;
using FluentAssertions;
using Xunit;
using Attribute = Automate.CLI.Domain.Attribute;

namespace CLI.UnitTests.Infrastructure
{
    [Trait("Category", "Unit")]
    public class SolutionPathResolverSpec
    {
        private readonly SolutionPathResolver resolver;

        public SolutionPathResolverSpec()
        {
            this.resolver = new SolutionPathResolver();
        }

        [Fact]
        public void WhenResolveAndSolutionIsNull_ThenReturnsNull()
        {
            this.resolver
                .Invoking(x => x.ResolveItem(null, "anexpression"))
                .Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void WhenResolveAndExpressionIsNull_ThenReturnsNull()
        {
            this.resolver
                .Invoking(x => x.ResolveItem(new SolutionDefinition(new ToolkitDefinition(new PatternDefinition("apatternname"), "1.0")), null))
                .Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void WhenResolveAndExpressionIsInvalidFormat_ThenThrows()
        {
            this.resolver
                .Invoking(x => x.ResolveItem(new SolutionDefinition(new ToolkitDefinition(new PatternDefinition("apatternname"), "1.0")), "notavalidexpression"))
                .Should().Throw<AutomateException>()
                .WithMessage(ExceptionMessages.SolutionPathResolver_InvalidExpression.Format("notavalidexpression"));
        }

        [Fact]
        public void WhenResolveAndExpressionIsEmpty_ThenThrows()
        {
            this.resolver
                .Invoking(x => x.ResolveItem(new SolutionDefinition(new ToolkitDefinition(new PatternDefinition("apatternname"), "1.0")), "{}"))
                .Should().Throw<AutomateException>()
                .WithMessage(ExceptionMessages.SolutionPathResolver_InvalidExpression.Format("{}"));
        }

        [Fact]
        public void WhenResolveAndExpressionIsJustNameOfPattern_ThenReturnsPattern()
        {
            var solution = new SolutionDefinition(new ToolkitDefinition(new PatternDefinition("apatternname"), "1.0"));

            var result = this.resolver.ResolveItem(solution, "{apatternname}");

            result.Should().Be(solution.Model);
        }

        [Fact]
        public void WhenResolveAndExpressionElementNotExist_ThenReturnsNull()
        {
            var solution = new SolutionDefinition(new ToolkitDefinition(new PatternDefinition("apatternname"), "1.0"));

            var result = this.resolver.ResolveItem(solution, "{anunknownelementname}");

            result.Should().BeNull();
        }

        [Fact]
        public void WhenResolveAndElementExpressionExists_ThenReturnsElement()
        {
            var pattern = new PatternDefinition("apatternname");
            var element = new Element("anelementname");
            pattern.Elements.Add(element);
            var solution = new SolutionDefinition(new ToolkitDefinition(pattern, "1.0"));

            var result = this.resolver.ResolveItem(solution, "{anelementname}");

            result.ElementSchema.Should().Be(element);
        }

        [Fact]
        public void WhenResolveAndCollectionItemExpressionExists_ThenReturnsElement()
        {
            var pattern = new PatternDefinition("apatternname");
            var collection = new Element("acollectionname", isCollection: true);
            var element = new Element("anelementname");
            collection.Elements.Add(element);
            pattern.Elements.Add(collection);
            var solution = new SolutionDefinition(new ToolkitDefinition(pattern, "1.0"));
            var collectionInstance = solution.Model.Properties["acollectionname"].MaterialiseCollectionItem();
            var elementInstance = collectionInstance.Properties["anelementname"].Materialise();

            var result = this.resolver.ResolveItem(solution, $"{{apatternname.acollectionname.{collectionInstance.Id}.anelementname}}");

            result.Id.Should().Be(elementInstance.Id);
        }

        [Fact]
        public void WhenResolveAndFullyQualifiedElementExpressionExists_ThenReturnsElement()
        {
            var pattern = new PatternDefinition("apatternname");
            var element = new Element("anelementname");
            pattern.Elements.Add(element);
            var solution = new SolutionDefinition(new ToolkitDefinition(pattern, "1.0"));

            var result = this.resolver.ResolveItem(solution, "{apatternname.anelementname}");

            result.ElementSchema.Should().Be(element);
        }

        [Fact]
        public void WhenResolveAndPartiallyQualifiedElementExpressionExists_ThenReturnsElement()
        {
            var pattern = new PatternDefinition("apatternname");
            var element = new Element("anelementname");
            pattern.Elements.Add(element);
            var solution = new SolutionDefinition(new ToolkitDefinition(pattern, "1.0"));

            var result = this.resolver.ResolveItem(solution, "{anelementname}");

            result.ElementSchema.Should().Be(element);
        }

        [Fact]
        public void WhenResolveAndDescendantElementExpressionNotMaterialised_ThenReturnsNull()
        {
            var pattern = new PatternDefinition("apatternname");
            var element3 = new Element("anelementname3");
            var element2 = new Element("anelementname2");
            var element1 = new Element("anelementname1");
            element2.Elements.Add(element3);
            element1.Elements.Add(element2);
            pattern.Elements.Add(element1);
            var solution = new SolutionDefinition(new ToolkitDefinition(pattern, "1.0"));
            solution.Model.Properties["anelementname1"].Materialise();

            var result = this.resolver.ResolveItem(solution, "{anelementname1.anelementname2.anelementname3}");

            result.Should().BeNull();
        }

        [Fact]
        public void WhenResolveAndDescendantElementExpressionExists_ThenReturnsElement()
        {
            var pattern = new PatternDefinition("apatternname");
            var element3 = new Element("anelementname3");
            var element2 = new Element("anelementname2");
            var element1 = new Element("anelementname1");
            element2.Elements.Add(element3);
            element1.Elements.Add(element2);
            pattern.Elements.Add(element1);
            var solution = new SolutionDefinition(new ToolkitDefinition(pattern, "1.0"));
            solution.Model.Properties["anelementname1"].Materialise().Properties["anelementname2"].Materialise()
                .Properties["anelementname3"].Materialise();

            var result = this.resolver.ResolveItem(solution, "{anelementname1.anelementname2.anelementname3}");

            result.ElementSchema.Should().Be(element3);
        }

        [Fact]
        public void WhenResolveExpressionAndExpressionIsNull_ThenReturnsNull()
        {
            var result = this.resolver.ResolveExpression("adescription", null, new SolutionItem(new Element("anelementname"), null));

            result.Should().BeNull();
        }

        [Fact]
        public void WhenResolveExpressionAndExpressionContainsNoSyntax_ThenReturnsExpression()
        {
            var result = this.resolver.ResolveExpression("adescription", "anexpression", new SolutionItem(new Element("anelementname"), null));

            result.Should().Be("anexpression");
        }

        [Fact]
        public void WhenResolveExpressionAndExpressionContainsSyntax_ThenReturnsExpression()
        {
            var element = new Element("anelementname");
            var attribute = new Attribute("anattributename", defaultValue: "adefaultvalue");
            element.Attributes.Add(attribute);
            var solutionItem = new SolutionItem(element, null);
            solutionItem.Materialise();

            var result = this.resolver.ResolveExpression("adescription", "anexpression{{anattributename}}anexpression", solutionItem);

            result.Should().Be("anexpressionadefaultvalueanexpression");
        }

        [Fact]
        public void WhenResolveExpressionAndExpressionContainsParentSyntax_ThenReturnsExpression()
        {
            var element = new Element("anelementname");
            var attribute = new Attribute("anattributename", defaultValue: "adefaultvalue");
            element.Attributes.Add(attribute);
            var pattern = new PatternDefinition("apatternname");
            pattern.Elements.Add(element);
            var solutionItem = new SolutionItem(pattern);
            solutionItem.Properties["anelementname"].Materialise();

            var result = this.resolver.ResolveExpression("adescription", "anexpression{{anelementname.parent.anelementname.anattributename}}anexpression", solutionItem);

            result.Should().Be("anexpressionadefaultvalueanexpression");
        }
    }
}