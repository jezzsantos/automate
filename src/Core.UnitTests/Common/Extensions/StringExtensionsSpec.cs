using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using Automate.Common.Extensions;
using FluentAssertions;
using Xunit;

namespace Core.UnitTests.Common.Extensions
{
    [Trait("Category", "Unit")]
    public class StringExtensionsSpec
    {
        [Fact]
        public void WhenSubstituteWithNullMessage_ThenReturnsNull()
        {
            var result = ((string)null).Substitute();

            result.Should().BeNull();
        }

        [Fact]
        public void WhenSubstituteWithNoArgs_ThenReturnsMessage()
        {
            var result = "amessage".Substitute();

            result.Should().Be("amessage");
        }

        [Fact]
        public void WhenSubstituteWithNoArgsAndPlaceholder_ThenReturnsMessage()
        {
            var result = "amessage{0}".Substitute();

            result.Should().Be("amessage{0}");
        }

        [Fact]
        public void WhenSubstituteWithAnArgAndNoPlaceholder_ThenReturnsMessage()
        {
            var result = "amessage".Substitute("anarg1");

            result.Should().Be("amessage");
        }

        [Fact]
        public void WhenSubstituteWithMoreArgsThanPlaceholders_ThenReturnsMessage()
        {
            var result = "amessage{0}".Substitute("anarg1", "anarg2");

            result.Should().Be("amessageanarg1");
        }

        [Fact]
        public void WhenSubstituteWithLessArgsThanPlaceholders_ThenThrows()
        {
            "amessage{0}and{1}"
                .Invoking(x => x.Substitute("anarg1"))
                .Should().Throw<FormatException>();
        }

        [Fact]
        public void WhenSubstituteTemplateWithNoReplacements_ThenReturnsMessage()
        {
            var message = "amessage".SubstituteTemplate();

            message.Should().Be("amessage");
        }

        [Fact]
        public void WhenSubstituteTemplateWithNoArguments_ThenReturnsMessage()
        {
            var message = "amessage{anargument}".SubstituteTemplate();

            message.Should().Be("amessage{anargument}");
        }

        [Fact]
        public void WhenSubstituteTemplateWithMoreArgumentsThanTokens_ThenReturnsMessage()
        {
            var message = "amessage".SubstituteTemplate("arg1", "anarg2");

            message.Should().Be("amessage");
        }

        [Fact]
        public void WhenSubstituteTemplateWithMoreTokensThanArguments_ThenReturnsMessage()
        {
            var message = "amessage{atoken1}{atoken2}{atoken3}".SubstituteTemplate("anarg1", "anarg2");

            message.Should().Be("amessageanarg1anarg2{atoken3}");
        }

        [Fact]
        public void WhenSubstituteTemplateWithJsonObject_ThenReturnsMessage()
        {
            var jsonObject = JsonNode.Parse("{\"aproperty\":\"avalue\"}");
            var message = "amessage{atoken1}{atoken2}".SubstituteTemplate("anarg1", jsonObject);

            message.Should().Be($"amessageanarg1{{{Environment.NewLine}" +
                                $"  \"aproperty\": \"avalue\"{Environment.NewLine}" +
                                "}");
        }

        [Fact]
        public void WhenSubstituteTemplateStructuredWithNoReplacements_ThenReturnsMessage()
        {
            var message = "amessage".SubstituteTemplateStructured();

            message.Should().BeEquivalentTo(new StructuredMessage
            {
                Message = "amessage",
                Values = new Dictionary<string, object>()
            });
        }

        [Fact]
        public void WhenSubstituteTemplateStructuredWithNoArguments_ThenReturnsMessage()
        {
            var message = "amessage{anargument}".SubstituteTemplateStructured();

            message.Should().BeEquivalentTo(new StructuredMessage
            {
                Message = "amessage{anargument}",
                Values = new Dictionary<string, object>()
            });
        }

        [Fact]
        public void WhenSubstituteTemplateStructuredWithMoreArgumentsThanTokens_ThenReturnsMessage()
        {
            var message = "amessage".SubstituteTemplateStructured("arg1", "anarg2");

            message.Should().BeEquivalentTo(new StructuredMessage
            {
                Message = "amessage",
                Values = new Dictionary<string, object>()
            });
        }

        [Fact]
        public void WhenSubstituteTemplateStructuredWithMoreTokensThanArguments_ThenReturnsMessage()
        {
            var message = "amessage{atoken1}{atoken2}{atoken3}".SubstituteTemplateStructured("anarg1", "anarg2");

            message.Should().BeEquivalentTo(new StructuredMessage
            {
                Message = "amessage{atoken1}{atoken2}{atoken3}",
                Values = new Dictionary<string, object>
                {
                    { "atoken1", "anarg1" },
                    { "atoken2", "anarg2" }
                }
            });
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
        public void WhenToCamelCaseAndLowercasedSentence_ThenReturnsCamelCased()
        {
            var result = "one  Two  three".ToCamelCase();

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
            var result = "One  Two  Three".ToPascalCase();

            result.Should().Be("OneTwoThree");
        }

        [Fact]
        public void WhenToPascalCaseAndLowercasedSentence_ThenReturnsPascalCased()
        {
            var result = "one  Two  three".ToPascalCase();

            result.Should().Be("OneTwoThree");
        }

        [Fact]
        public void WhenToCapitalizedWordsAndNull_ThenReturnsNull()
        {
            var result = ((string)null).ToCapitalizedWords();

            result.Should().BeNull();
        }

        [Fact]
        public void WhenToCapitalizedWordsAndAllLowercase_ThenReturnsWordsCased()
        {
            var result = "avalue".ToCapitalizedWords();

            result.Should().Be("Avalue");
        }

        [Fact]
        public void WhenToCapitalizedWordsAndCapitalizedWord_ThenReturnsWordsCased()
        {
            var result = "Avalue".ToCapitalizedWords();

            result.Should().Be("Avalue");
        }

        [Fact]
        public void WhenToCapitalizedWordsAndWordsCased_ThenReturnsWordsCased()
        {
            var result = "OneTwoThree".ToCapitalizedWords();

            result.Should().Be("One Two Three");
        }

        [Fact]
        public void WhenToCapitalizedWordsAndSnakeCased_ThenReturnsCamelCased()
        {
            var result = "one_Two_three".ToCapitalizedWords();

            result.Should().Be("One Two Three");
        }

        [Fact]
        public void WhenToCapitalizedWordsAndSentence_ThenReturnsWordsCased()
        {
            var result = "One Two Three".ToCapitalizedWords();

            result.Should().Be("One Two Three");
        }

        [Fact]
        public void WhenToCapitalizedWordsAndLowercaseSentence_ThenReturnsWordsCased()
        {
            var result = "one  Two  three".ToCapitalizedWords();

            result.Should().Be("One Two Three");
        }
    }
}