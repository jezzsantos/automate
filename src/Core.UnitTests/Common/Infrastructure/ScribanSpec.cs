using Automate.Common.Infrastructure;
using FluentAssertions;
using Xunit;

namespace Core.UnitTests.Common.Infrastructure
{
    [Trait("Category", "Unit")]
    public class CustomScribanFunctionsSpec
    {
        [Fact]
        public void WhenTransformWithBuiltIns_ThenReturnsTransformedTemplate()
        {
            var source = new
            {
                AProperty = "OneTwoThree"
            };

            var result =
                source.Transform("adescription", "{{AProperty | string.downcase}}\\n{{AProperty | string.upcase}}");

            result.Should().Be("onetwothree\\nONETWOTHREE");
        }

        [Fact]
        public void WhenTransformWithCamelCase_ThenReturnsTransformedTemplate()
        {
            var source = new
            {
                AProperty = "OneTwoThree"
            };

            var result =
                source.Transform("adescription", "{{AProperty | string.camelcase}}");

            result.Should().Be("oneTwoThree");
        }

        [Fact]
        public void WhenTransformWithPascalCase_ThenReturnsTransformedTemplate()
        {
            var source = new
            {
                AProperty = "oneTwoThree"
            };

            var result =
                source.Transform("adescription",
                    "{{AProperty | string.pascalcase}}");

            result.Should().Be("OneTwoThree");
        }

        [Fact]
        public void WhenTransformWithSnakeCase_ThenReturnsTransformedTemplate()
        {
            var source = new
            {
                AProperty = "oneTwoThree"
            };

            var result =
                source.Transform("adescription",
                    "{{AProperty | string.snakecase}}");

            result.Should().Be("one_two_three");
        }

        [Fact]
        public void WhenTransformWithToPlural_ThenReturnsTransformedTemplate()
        {
            var source = new
            {
                SimpleWord = "word",
                InfinitiveWord = "sheep"
            };

            var result =
                source.Transform("adescription",
                    "{{SimpleWord | string.pluralize}}\n{{InfinitiveWord | string.pluralize}}");

            result.Should().Be("words\nsheep");
        }

        [Fact]
        public void WhenTransformWithToSingular_ThenReturnsTransformedTemplate()
        {
            var source = new
            {
                SimpleWord = "words",
                InfinitiveWord = "sheep"
            };

            var result =
                source.Transform("adescription",
                    "{{SimpleWord | string.singularize}}\n{{InfinitiveWord | string.singularize}}");

            result.Should().Be("word\nsheep");
        }

        [Fact]
        public void WhenTransformWithPascalPlural_ThenReturnsTransformedTemplate()
        {
            var source = new
            {
                AProperty = "one word"
            };

            var result =
                source.Transform("adescription", "{{AProperty | string.pascalplural}}");

            result.Should().Be("OneWords");
        }

        [Fact]
        public void WhenTransformWithCamelPlural_ThenReturnsTransformedTemplate()
        {
            var source = new
            {
                AProperty = "One Word"
            };

            var result =
                source.Transform("adescription", "{{AProperty | string.camelplural}}");

            result.Should().Be("oneWords");
        }

        [Fact]
        public void WhenTransformWithSnakePlural_ThenReturnsTransformedTemplate()
        {
            var source = new
            {
                AProperty = "One Word"
            };

            var result =
                source.Transform("adescription", "{{AProperty | string.snakeplural}}");

            result.Should().Be("one_words");
        }

        [Fact]
        public void WhenTransformWithPascalSingular_ThenReturnsTransformedTemplate()
        {
            var source = new
            {
                AProperty = "one words"
            };

            var result =
                source.Transform("adescription", "{{AProperty | string.pascalsingular}}");

            result.Should().Be("OneWord");
        }

        [Fact]
        public void WhenTransformWithCamelSingular_ThenReturnsTransformedTemplate()
        {
            var source = new
            {
                AProperty = "One Words"
            };

            var result =
                source.Transform("adescription", "{{AProperty | string.camelsingular}}");

            result.Should().Be("oneWord");
        }

        [Fact]
        public void WhenTransformWithSnakeSingular_ThenReturnsTransformedTemplate()
        {
            var source = new
            {
                AProperty = "One Words"
            };

            var result =
                source.Transform("adescription", "{{AProperty | string.snakesingular}}");

            result.Should().Be("one_word");
        }
    }
}