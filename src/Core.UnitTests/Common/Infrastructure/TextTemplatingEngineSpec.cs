using System;
using Automate.Authoring.Domain;
using Automate.Common;
using Automate.Common.Extensions;
using Automate.Common.Infrastructure;
using Automate.Runtime.Domain;
using FluentAssertions;
using Xunit;
using Attribute = Automate.Authoring.Domain.Attribute;

namespace Core.UnitTests.Common.Infrastructure
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
            var pattern = new PatternDefinition("apatternname");
            var toolkit = new ToolkitDefinition(pattern);
            var element = new Element("anelementname");
            pattern.AddElement(element);

            var result = this.engine.Transform("adescription", string.Empty,
                new DraftItem(toolkit, element, new DraftItem(toolkit, pattern)));

            result.Should().BeEmpty();
        }

        [Fact]
        public void WhenTransformAndTemplate_ThenReturnsTransformedTemplate()
        {
            var pattern = new PatternDefinition("apatternname");
            var toolkit = new ToolkitDefinition(pattern);
            var element = new Element("anelementname");
            pattern.AddElement(element);

            var result = this.engine.Transform("adescription", "atemplate",
                new DraftItem(toolkit, element, new DraftItem(toolkit, pattern)));

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
            var draft = new DraftItem(toolkit, element, new DraftItem(toolkit, pattern));
            draft.Materialise();

            var result = this.engine.Transform("adescription", "{{anattributename}}", draft);

            result.Should().Be("adefaultvalue");
        }

        [Fact]
        public void WhenTransformAndHasSyntaxErrors_ThenThrows()
        {
            var pattern = new PatternDefinition("apatternname");
            var toolkit = new ToolkitDefinition(pattern);
            var element = new Element("anelementname");
            pattern.AddElement(element);

            this.engine
                .Invoking(x => x.Transform("adescription", "{{anything.}}",
                    new DraftItem(toolkit, element, new DraftItem(toolkit, pattern))))
                .Should().Throw<AutomateException>()
                .WithMessage(ExceptionMessages.TextTemplatingExtensions_HasSyntaxErrors.Substitute("adescription",
                    "((11:0,11),(12:0,12)): Invalid token `CodeExit`. The dot operator is expected to be followed by a plain identifier" +
                    Environment.NewLine +
                    "((10:0,10),(10:0,10)): Invalid token found `.`. Expecting <EOL>/end of line."));
        }

        [Fact]
        public void WhenTransformAndHasTransformationErrors_ThenThrows()
        {
            var pattern = new PatternDefinition("apatternname");
            var toolkit = new ToolkitDefinition(pattern);
            var element = new Element("anelementname");
            pattern.AddElement(element);

            this.engine
                .Invoking(x => x.Transform("adescription", "{{parent.notexists}}",
                    new DraftItem(toolkit, element, new DraftItem(toolkit, pattern))))
                .Should().Throw<AutomateException>()
                .WithMessage(ExceptionMessages.TextTemplatingExtensions_TransformFailed.Substitute("adescription",
                    "<input>(1,10) : error : Cannot get the member parent.notexists for a null object." +
                    Environment.NewLine));
        }
    }
}