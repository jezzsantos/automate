using Automate.CLI.Infrastructure;
using FluentAssertions;
using Xunit;

namespace CLI.UnitTests.Infrastructure
{
    [Trait("Category", "Unit")]
    public class CustomScribanFunctionsSpec
    {
        [Fact]
        public void WhenTransformWithBuiltIns_ThenReturnsTransformedTemplate()
        {
            var source = new
            {
                aproperty = "OneTwoThree"
            };

            var result =
                source.Transform("adescription", "{{aproperty | string.downcase}}\\n{{aproperty | string.upcase}}");

            result.Should().Be("onetwothree\\nONETWOTHREE");
        }

        [Fact]
        public void WhenTransformWithCamelCase_ThenReturnsTransformedTemplate()
        {
            var source = new
            {
                aproperty = "OneTwoThree"
            };

            var result =
                source.Transform("adescription", "{{aproperty | string.camelcase}}");

            result.Should().Be("oneTwoThree");
        }

        [Fact]
        public void WhenTransformWithPascalCase_ThenReturnsTransformedTemplate()
        {
            var source = new
            {
                aproperty = "oneTwoThree"
            };

            var result =
                source.Transform("adescription",
                    "{{aproperty | string.pascalcase}}");

            result.Should().Be("OneTwoThree");
        }

        [Fact]
        public void WhenTransformWithSnakeCase_ThenReturnsTransformedTemplate()
        {
            var source = new
            {
                aproperty = "oneTwoThree"
            };

            var result =
                source.Transform("adescription",
                    "{{aproperty | string.snakecase}}");

            result.Should().Be("one_two_three");
        }

        [Fact]
        public void WhenTransformWithToPlural_ThenReturnsTransformedTemplate()
        {
            var source = new
            {
                simple = "word",
                infinitive = "sheep"
            };

            var result =
                source.Transform("adescription", "{{simple | string.pluralize}}\n{{infinitive | string.pluralize}}");

            result.Should().Be("words\nsheep");
        }

        [Fact]
        public void WhenTransformWithToSingular_ThenReturnsTransformedTemplate()
        {
            var source = new
            {
                simple = "words",
                infinitive = "sheep"
            };

            var result =
                source.Transform("adescription",
                    "{{simple | string.singularize}}\n{{infinitive | string.singularize}}");

            result.Should().Be("word\nsheep");
        }

        [Fact]
        public void WhenTransformWithPascalPlural_ThenReturnsTransformedTemplate()
        {
            var source = new
            {
                aproperty = "one word"
            };

            var result =
                source.Transform("adescription", "{{aproperty | string.pascalplural}}");

            result.Should().Be("OneWords");
        }

        [Fact]
        public void WhenTransformWithCamelPlural_ThenReturnsTransformedTemplate()
        {
            var source = new
            {
                aproperty = "One Word"
            };

            var result =
                source.Transform("adescription", "{{aproperty | string.camelplural}}");

            result.Should().Be("oneWords");
        }

        [Fact]
        public void WhenTransformWithSnakePlural_ThenReturnsTransformedTemplate()
        {
            var source = new
            {
                aproperty = "One Word"
            };

            var result =
                source.Transform("adescription", "{{aproperty | string.snakeplural}}");

            result.Should().Be("one_words");
        }

        [Fact]
        public void WhenTransformWithPascalSingular_ThenReturnsTransformedTemplate()
        {
            var source = new
            {
                aproperty = "one words"
            };

            var result =
                source.Transform("adescription", "{{aproperty | string.pascalsingular}}");

            result.Should().Be("OneWord");
        }

        [Fact]
        public void WhenTransformWithCamelSingular_ThenReturnsTransformedTemplate()
        {
            var source = new
            {
                aproperty = "One Words"
            };

            var result =
                source.Transform("adescription", "{{aproperty | string.camelsingular}}");

            result.Should().Be("oneWord");
        }

        [Fact]
        public void WhenTransformWithSnakeSingular_ThenReturnsTransformedTemplate()
        {
            var source = new
            {
                aproperty = "One Words"
            };

            var result =
                source.Transform("adescription", "{{aproperty | string.snakesingular}}");

            result.Should().Be("one_word");
        }
    }
}