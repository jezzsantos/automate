using Automate.CLI.Extensions;
using FluentAssertions;
using Xunit;

namespace CLI.UnitTests.Extensions
{
    [Trait("Category", "Unit")]
    public class StringExtensionsSpec
    {
        [Fact]
        public void WhenFormatTemplateWithNoReplacements_ThenReturnsMessage()
        {
            var message = "amessage".FormatTemplate();

            message.Should().Be("amessage");
        }

        [Fact]
        public void WhenFormatTemplateWithNoArguments_ThenReturnsMessage()
        {
            var message = "amessage{anargument}".FormatTemplate();

            message.Should().Be("amessage{anargument}");
        }

        [Fact]
        public void WhenFormatTemplateWithMoreArgumentsThanTokens_ThenReturnsMessage()
        {
            var message = "amessage".FormatTemplate("arg1", "anarg2");

            message.Should().Be("amessage");
        }

        [Fact]
        public void WhenFormatTemplateWithMoreTokensThanArguments_ThenReturnsMessage()
        {
            var message = "amessage{atoken1}{atoken2}{atoken3}".FormatTemplate("anarg1", "anarg2");

            message.Should().Be("amessageanarg1anarg2{atoken3}");
        }

        [Fact]
        public void WhenFormatTemplateStructuredWithNoReplacements_ThenReturnsMessage()
        {
            var message = "amessage".FormatTemplateStructured();

            message.Should().Be("{\"message\":\"amessage\",\"values\":{}}");
        }

        [Fact]
        public void WhenFormatTemplateStructuredWithNoArguments_ThenReturnsMessage()
        {
            var message = "amessage{anargument}".FormatTemplateStructured();

            message.Should().Be("{\"message\":\"amessage{anargument}\",\"values\":{}}");
        }

        [Fact]
        public void WhenFormatTemplateStructuredWithMoreArgumentsThanTokens_ThenReturnsMessage()
        {
            var message = "amessage".FormatTemplateStructured("arg1", "anarg2");

            message.Should().Be("{\"message\":\"amessage\",\"values\":{}}");
        }

        [Fact]
        public void WhenFormatTemplateStructuredWithMoreTokensThanArguments_ThenReturnsMessage()
        {
            var message = "amessage{atoken1}{atoken2}{atoken3}".FormatTemplateStructured("anarg1", "anarg2");

            message.Should()
                .Be(
                    "{\"message\":\"amessage{atoken1}{atoken2}{atoken3}\",\"values\":{\"atoken1\":\"anarg1\",\"atoken2\":\"anarg2\"}}");
        }
    }
}