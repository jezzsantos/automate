using System;
using Automate;
using Automate.Domain;
using Automate.Extensions;
using Automate.Infrastructure;
using FluentAssertions;
using Xunit;
using Attribute = Automate.Domain.Attribute;

namespace Core.UnitTests.Infrastructure
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

            var result = this.engine.Transform("adescription", string.Empty,
                new DraftItem(toolkit,
                    new Element("anelementname"), null));

            result.Should().BeEmpty();
        }

        [Fact]
        public void WhenTransformAndTemplate_ThenReturnsTransformedTemplate()
        {
            var toolkit = new ToolkitDefinition(new PatternDefinition("apatternname"));

            var result = this.engine.Transform("adescription", "atemplate",
                new DraftItem(toolkit,
                    new Element("anelementname"), null));

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
            var draft = new DraftItem(toolkit, element, null);
            draft.Materialise();

            var result = this.engine.Transform("adescription", "{{anattributename}}", draft);

            result.Should().Be("adefaultvalue");
        }

        [Fact]
        public void WhenTransformAndHasSyntaxErrors_ThenThrows()
        {
            var toolkit = new ToolkitDefinition(new PatternDefinition("apatternname"));

            this.engine
                .Invoking(x => x.Transform("adescription", "{{anything.}}",
                    new DraftItem(toolkit,
                        new Element("anelementname"), null)))
                .Should().Throw<AutomateException>()
                .WithMessage(ExceptionMessages.TextTemplatingExtensions_HasSyntaxErrors.Substitute("adescription",
                    "((11:0,11),(12:0,12)): Invalid token `CodeExit`. The dot operator is expected to be followed by a plain identifier" +
                    Environment.NewLine +
                    "((10:0,10),(10:0,10)): Invalid token found `.`. Expecting <EOL>/end of line."));
        }

        [Fact]
        public void WhenTransformAndHasTransformationErrors_ThenThrows()
        {
            var toolkit = new ToolkitDefinition(new PatternDefinition("apatternname"));

            this.engine
                .Invoking(x => x.Transform("adescription", "{{parent.notexists}}",
                    new DraftItem(toolkit,
                        new Element("anelementname"), null)))
                .Should().Throw<AutomateException>()
                .WithMessage(ExceptionMessages.TextTemplatingExtensions_TransformFailed.Substitute("adescription",
                    "<input>(1,10) : error : Cannot get the member parent.notexists for a null object." +
                    Environment.NewLine));
        }
    }
}