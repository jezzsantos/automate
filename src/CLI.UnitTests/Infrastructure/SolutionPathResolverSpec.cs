using System;
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
                .Invoking(x => x.ResolveItem(new SolutionDefinition(new ToolkitDefinition(new PatternDefinition("apatternname"))), null))
                .Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void WhenResolveAndExpressionIsInvalidFormat_ThenThrows()
        {
            this.resolver
                .Invoking(x => x.ResolveItem(new SolutionDefinition(new ToolkitDefinition(new PatternDefinition("apatternname"))), "notavalidexpression"))
                .Should().Throw<AutomateException>()
                .WithMessage(ExceptionMessages.SolutionPathResolver_InvalidExpression.Format("notavalidexpression"));
        }

        [Fact]
        public void WhenResolveAndExpressionIsEmpty_ThenThrows()
        {
            this.resolver
                .Invoking(x => x.ResolveItem(new SolutionDefinition(new ToolkitDefinition(new PatternDefinition("apatternname"))), "{}"))
                .Should().Throw<AutomateException>()
                .WithMessage(ExceptionMessages.SolutionPathResolver_InvalidExpression.Format("{}"));
        }

        [Fact]
        public void WhenResolveAndExpressionIsJustNameOfPattern_ThenReturnsPattern()
        {
            var solution = new SolutionDefinition(new ToolkitDefinition(new PatternDefinition("apatternname")));

            var result = this.resolver.ResolveItem(solution, "{apatternname}");

            result.Should().Be(solution.Model);
        }

        [Fact]
        public void WhenResolveAndExpressionElementNotExist_ThenReturnsNull()
        {
            var solution = new SolutionDefinition(new ToolkitDefinition(new PatternDefinition("apatternname")));

            var result = this.resolver.ResolveItem(solution, "{anunknownelementname}");

            result.Should().BeNull();
        }

        [Fact]
        public void WhenResolveAndElementExpressionExists_ThenReturnsElement()
        {
            var pattern = new PatternDefinition("apatternname");
            var element = new Element("anelementname");
            pattern.AddElement(element);
            var solution = new SolutionDefinition(new ToolkitDefinition(pattern));

            var result = this.resolver.ResolveItem(solution, "{anelementname}");

            result.ElementSchema.Object.Should().Be(element);
        }

        [Fact]
        public void WhenResolveAndCollectionItemExpressionExists_ThenReturnsElement()
        {
            var pattern = new PatternDefinition("apatternname");
            var collection = new Element("acollectionname", ElementCardinality.OneOrMany);
            var element = new Element("anelementname");
            collection.AddElement(element);
            pattern.AddElement(collection);
            var solution = new SolutionDefinition(new ToolkitDefinition(pattern));
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
            pattern.AddElement(element);
            var solution = new SolutionDefinition(new ToolkitDefinition(pattern));

            var result = this.resolver.ResolveItem(solution, "{apatternname.anelementname}");

            result.ElementSchema.Object.Should().Be(element);
        }

        [Fact]
        public void WhenResolveAndPartiallyQualifiedElementExpressionExists_ThenReturnsElement()
        {
            var pattern = new PatternDefinition("apatternname");
            var element = new Element("anelementname");
            pattern.AddElement(element);
            var solution = new SolutionDefinition(new ToolkitDefinition(pattern));

            var result = this.resolver.ResolveItem(solution, "{anelementname}");

            result.ElementSchema.Object.Should().Be(element);
        }

        [Fact]
        public void WhenResolveAndDescendantElementExpressionNotMaterialised_ThenReturnsNull()
        {
            var pattern = new PatternDefinition("apatternname");
            var element3 = new Element("anelementname3");
            var element2 = new Element("anelementname2");
            var element1 = new Element("anelementname1");
            element2.AddElement(element3);
            element1.AddElement(element2);
            pattern.AddElement(element1);
            var solution = new SolutionDefinition(new ToolkitDefinition(pattern));
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
            element2.AddElement(element3);
            element1.AddElement(element2);
            pattern.AddElement(element1);
            var solution = new SolutionDefinition(new ToolkitDefinition(pattern));
            solution.Model.Properties["anelementname1"].Materialise().Properties["anelementname2"].Materialise()
                .Properties["anelementname3"].Materialise();

            var result = this.resolver.ResolveItem(solution, "{anelementname1.anelementname2.anelementname3}");

            result.ElementSchema.Object.Should().Be(element3);
        }

        [Fact]
        public void WhenResolveExpressionAndExpressionIsNull_ThenReturnsNull()
        {
            var toolkit = new ToolkitDefinition(new PatternDefinition("apatternname"));

            var result = this.resolver.ResolveExpression("adescription", null, new SolutionItem(toolkit, new Element("anelementname"), null));

            result.Should().BeNull();
        }

        [Fact]
        public void WhenResolveExpressionAndExpressionContainsNoSyntax_ThenReturnsExpression()
        {
            var toolkit = new ToolkitDefinition(new PatternDefinition("apatternname"));

            var result = this.resolver.ResolveExpression("adescription", "anexpression", new SolutionItem(toolkit, new Element("anelementname"), null));

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
            var solutionItem = new SolutionItem(new ToolkitDefinition(pattern), element, null);
            solutionItem.Materialise();

            var result = this.resolver.ResolveExpression("adescription", "anexpression{{anattributename}}anexpression", solutionItem);

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
            var solutionItem = new SolutionItem(new ToolkitDefinition(pattern), pattern);
            solutionItem.Properties["anelementname"].Materialise();

            var result = this.resolver.ResolveExpression("adescription", "anexpression{{anelementname.parent.anelementname.anattributename}}anexpression", solutionItem);

            result.Should().Be("anexpressionadefaultvalueanexpression");
        }
    }
}