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
                .Invoking(x => x.Resolve(null, "anexpression"))
                .Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void WhenResolveAndExpressionIsNull_ThenReturnsNull()
        {
            this.resolver
                .Invoking(x => x.Resolve(new SolutionDefinition(), null))
                .Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void WhenResolveAndExpressionIsInvalidFormat_ThenThrows()
        {
            this.resolver
                .Invoking(x => x.Resolve(new SolutionDefinition(), "notavalidexpression"))
                .Should().Throw<AutomateException>()
                .WithMessage(ExceptionMessages.SolutionPathResolver_InvalidExpression.Format("notavalidexpression"));
        }

        [Fact]
        public void WhenResolveAndExpressionIsEmpty_ThenThrows()
        {
            this.resolver
                .Invoking(x => x.Resolve(new SolutionDefinition(), "{}"))
                .Should().Throw<AutomateException>()
                .WithMessage(ExceptionMessages.SolutionPathResolver_InvalidExpression.Format("{}"));
        }

        [Fact]
        public void WhenResolveAndExpressionIsJustNameOfPattern_ThenReturnsPattern()
        {
            var solution =
                new SolutionDefinition { Id = "asolutionid", Pattern = new PatternDefinition("apatternname") };

            var result = this.resolver.Resolve(solution, "{apatternname}");

            result.Should().Be(solution.Model);
        }

        [Fact]
        public void WhenResolveAndExpressionElementNotExist_ThenReturnsNull()
        {
            var solution =
                new SolutionDefinition("atoolkitid", new PatternDefinition("apatternname"));

            var result = this.resolver.Resolve(solution, "{anunknownelementname}");

            result.Should().BeNull();
        }

        [Fact]
        public void WhenResolveAndElementExpressionExists_ThenReturnsElement()
        {
            var pattern = new PatternDefinition("apatternname");
            var element = new Element("anelementname", null, null, false);
            pattern.Elements.Add(element);
            var solution = new SolutionDefinition("atoolkitid", pattern);

            var result = this.resolver.Resolve(solution, "{anelementname}");

            result.ElementSchema.Should().Be(element);
        }

        [Fact]
        public void WhenResolveAndFullyQualifiedElementExpressionExists_ThenReturnsElement()
        {
            var pattern = new PatternDefinition("apatternname");
            var element = new Element("anelementname", null, null, false);
            pattern.Elements.Add(element);
            var solution = new SolutionDefinition("atoolkitid", pattern);

            var result = this.resolver.Resolve(solution, "{apatternname.anelementname}");

            result.ElementSchema.Should().Be(element);
        }

        [Fact]
        public void WhenResolveAndDeepElementExpressionExists_ThenReturnsElement()
        {
            var pattern = new PatternDefinition("apatternname");
            var element3 = new Element("anelementname3", null, null, false);
            var element2 = new Element("anelementname2", null, null, false);
            var element1 = new Element("anelementname1", null, null, false);
            element2.Elements.Add(element3);
            element1.Elements.Add(element2);
            pattern.Elements.Add(element1);
            var solution = new SolutionDefinition("atoolkitid", pattern);
            solution.Model.Properties["anelementname1"].Materialise().Properties["anelementname2"].Materialise()
                .Properties["anelementname3"].Materialise();

            var result = this.resolver.Resolve(solution, "{anelementname1.anelementname2.anelementname3}");

            result.ElementSchema.Should().Be(element3);
        }
    }
}