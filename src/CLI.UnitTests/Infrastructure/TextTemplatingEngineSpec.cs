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
    public class TextTemplatingEngineSpec
    {
        private readonly TextTemplatingEngine engine;

        public TextTemplatingEngineSpec()
        {
            this.engine = new TextTemplatingEngine();
        }

        [Fact]
        public void WhenTransformAndEmptyTemplate_ThenReturnsEmptyString()
        {
            var toolkit = new ToolkitDefinition(new PatternDefinition("apatternname"));

            var result = this.engine.Transform("adescription", string.Empty, new SolutionItem(toolkit, new Element("anelementname"), null));

            result.Should().BeEmpty();
        }

        [Fact]
        public void WhenTransformAndTemplate_ThenReturnsTransformedTemplate()
        {
            var toolkit = new ToolkitDefinition(new PatternDefinition("apatternname"));

            var result = this.engine.Transform("adescription", "atemplate", new SolutionItem(toolkit, new Element("anelementname"), null));

            result.Should().Be("atemplate");
        }

        [Fact]
        public void WhenTransformAndTemplateContainsSubstitution_ThenReturnsTransformedTemplate()
        {
            var pattern = new PatternDefinition("apatternname");
            var toolkit = new ToolkitDefinition(pattern);
            var element = new Element("anelementname");
            var attribute = new Attribute("anattributename", defaultValue: "adefaultvalue");
            element.AddAttribute(attribute);
            pattern.AddElement(element);
            var solution = new SolutionItem(toolkit, element, null);
            solution.Materialise();

            var result = this.engine.Transform("adescription", "{{model.anattributename}}", solution);

            result.Should().Be("adefaultvalue");
        }

        [Fact]
        public void WhenTransformAndHasSyntaxErrors_ThenThrows()
        {
            var toolkit = new ToolkitDefinition(new PatternDefinition("apatternname"));

            this.engine
                .Invoking(x => x.Transform("adescription", "{{model.}}", new SolutionItem(toolkit, new Element("anelementname"), null)))
                .Should().Throw<AutomateException>()
                .WithMessage(ExceptionMessages.TextTemplatingExtensions_HasSyntaxErrors.Format("adescription",
                    "((8:0,8),(9:0,9)): Invalid token `CodeExit`. The dot operator is expected to be followed by a plain identifier" + Environment.NewLine +
                    "((7:0,7),(7:0,7)): Invalid token found `.`. Expecting <EOL>/end of line."));
        }

        [Fact]
        public void WhenTransformAndHasTransformationErrors_ThenThrows()
        {
            var toolkit = new ToolkitDefinition(new PatternDefinition("apatternname"));

            this.engine
                .Invoking(x => x.Transform("adescription", "{{model.parent.notexists}}", new SolutionItem(toolkit, new Element("anelementname"), null)))
                .Should().Throw<AutomateException>()
                .WithMessage(ExceptionMessages.TextTemplatingExtensions_TransformFailed.Format("adescription",
                    "<input>(1,16) : error : Cannot get the member model.parent.notexists for a null object." + Environment.NewLine));
        }
    }
}