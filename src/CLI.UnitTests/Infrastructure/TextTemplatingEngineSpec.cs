using Automate.CLI.Domain;
using Automate.CLI.Infrastructure;
using FluentAssertions;
using Xunit;

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
            var result = this.engine.Transform(string.Empty, new SolutionItem());

            result.Should().BeEmpty();
        }

        [Fact]
        public void WhenTransformAndTemplate_ThenReturnsTransformedTemplate()
        {
            var result = this.engine.Transform("atemplate", new SolutionItem());

            result.Should().Be("atemplate");
        }

        [Fact]
        public void WhenTransformAndTemplateContainsSubstitution_ThenReturnsTransformedTemplate()
        {
            var element = new Element("anelementname");
            var attribute = new Attribute("anattributename", defaultValue: "adefaultvalue");
            element.Attributes.Add(attribute);
            var solution = new SolutionItem(element, null);
            solution.Materialise();

            var result = this.engine.Transform("{{model.anattributename}}", solution);

            result.Should().Be("adefaultvalue");
        }
    }
}