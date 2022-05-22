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

        [Fact]
        public void WhenToCamelCaseAndNull_ThenReturnsNull()
        {
            var result = ((string)null).ToCamelCase();

            result.Should().BeNull();
        }

        [Fact]
        public void WhenToCamelCaseAndLowercase_ThenReturnsCamelCased()
        {
            var result = "avalue".ToCamelCase();

            result.Should().Be("avalue");
        }

        [Fact]
        public void WhenToCamelCaseAndCapitalized_ThenReturnsCamelCased()
        {
            var result = "Avalue".ToCamelCase();

            result.Should().Be("avalue");
        }

        [Fact]
        public void WhenToCamelCaseAndPascalCased_ThenReturnsCamelCased()
        {
            var result = "OneTwoThree".ToCamelCase();

            result.Should().Be("oneTwoThree");
        }

        [Fact]
        public void WhenToCamelCaseAndSnakeCased_ThenReturnsCamelCased()
        {
            var result = "one_two_three".ToCamelCase();

            result.Should().Be("oneTwoThree");
        }

        [Fact]
        public void WhenToCamelCaseAndSentence_ThenReturnsCamelCased()
        {
            var result = "One Two Three".ToCamelCase();

            result.Should().Be("oneTwoThree");
        }

        [Fact]
        public void WhenToPascalCaseAndNull_ThenReturnsNull()
        {
            var result = ((string)null).ToPascalCase();

            result.Should().BeNull();
        }

        [Fact]
        public void WhenToPascalCaseAndLowercase_ThenReturnsPascalCased()
        {
            var result = "avalue".ToPascalCase();

            result.Should().Be("Avalue");
        }

        [Fact]
        public void WhenToPascalCaseAndCapitalized_ThenReturnsPascalCased()
        {
            var result = "Avalue".ToPascalCase();

            result.Should().Be("Avalue");
        }

        [Fact]
        public void WhenToPascalCaseAndPascalCased_ThenReturnsPascalCased()
        {
            var result = "OneTwoThree".ToPascalCase();

            result.Should().Be("OneTwoThree");
        }

        [Fact]
        public void WhenToPascalCaseAndSnakeCased_ThenReturnsCamelCased()
        {
            var result = "one_two_three".ToPascalCase();

            result.Should().Be("OneTwoThree");
        }

        [Fact]
        public void WhenToPascalCaseAndSentence_ThenReturnsPascalCased()
        {
            var result = "One Two Three".ToPascalCase();

            result.Should().Be("OneTwoThree");
        }
    }
}