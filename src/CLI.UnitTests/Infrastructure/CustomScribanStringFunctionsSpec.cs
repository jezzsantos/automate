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
            var model = new
            {
                aproperty = "OneTwoThree"
            };

            var result =
                model.Transform("adescription", "{{aproperty | string.downcase}}\\n{{aproperty | string.upcase}}");

            result.Should().Be("onetwothree\\nONETWOTHREE");
        }

        [Fact]
        public void WhenTransformWithCamelCase_ThenReturnsTransformedTemplate()
        {
            var model = new
            {
                aproperty = "OneTwoThree"
            };

            var result =
                model.Transform("adescription", "{{aproperty | string.camelcase}}\n{{aproperty | automate.camelcase}}");

            result.Should().Be("oneTwoThree\noneTwoThree");
        }

        [Fact]
        public void WhenTransformWithPascalCase_ThenReturnsTransformedTemplate()
        {
            var model = new
            {
                aproperty = "oneTwoThree"
            };

            var result =
                model.Transform("adescription",
                    "{{aproperty | string.pascalcase}}\n{{aproperty | automate.pascalcase}}");

            result.Should().Be("OneTwoThree\nOneTwoThree");
        }

        [Fact]
        public void WhenTransformWithSnakeCase_ThenReturnsTransformedTemplate()
        {
            var model = new
            {
                aproperty = "oneTwoThree"
            };

            var result =
                model.Transform("adescription",
                    "{{aproperty | string.snakecase}}");

            result.Should().Be("one_two_three");
        }

        [Fact]
        public void WhenTransformWithToPlural_ThenReturnsTransformedTemplate()
        {
            var model = new
            {
                simple = "word",
                infinitive = "sheep"
            };

            var result =
                model.Transform("adescription", "{{simple | string.pluralize}}\n{{infinitive | string.pluralize}}");

            result.Should().Be("words\nsheep");
        }

        [Fact]
        public void WhenTransformWithToSingular_ThenReturnsTransformedTemplate()
        {
            var model = new
            {
                simple = "words",
                infinitive = "sheep"
            };

            var result =
                model.Transform("adescription", "{{simple | string.singularize}}\n{{infinitive | string.singularize}}");

            result.Should().Be("word\nsheep");
        }

        [Fact]
        public void WhenTransformWithPascalPlural_ThenReturnsTransformedTemplate()
        {
            var model = new
            {
                aproperty = "one word"
            };

            var result =
                model.Transform("adescription", "{{aproperty | string.pascalplural}}");

            result.Should().Be("OneWords");
        }

        [Fact]
        public void WhenTransformWithCamelPlural_ThenReturnsTransformedTemplate()
        {
            var model = new
            {
                aproperty = "One Word"
            };

            var result =
                model.Transform("adescription", "{{aproperty | string.camelplural}}");

            result.Should().Be("oneWords");
        }

        [Fact]
        public void WhenTransformWithSnakePlural_ThenReturnsTransformedTemplate()
        {
            var model = new
            {
                aproperty = "One Word"
            };

            var result =
                model.Transform("adescription", "{{aproperty | string.snakeplural}}");

            result.Should().Be("one_words");
        }

        [Fact]
        public void WhenTransformWithPascalSingular_ThenReturnsTransformedTemplate()
        {
            var model = new
            {
                aproperty = "one words"
            };

            var result =
                model.Transform("adescription", "{{aproperty | string.pascalsingular}}");

            result.Should().Be("OneWord");
        }

        [Fact]
        public void WhenTransformWithCamelSingular_ThenReturnsTransformedTemplate()
        {
            var model = new
            {
                aproperty = "One Words"
            };

            var result =
                model.Transform("adescription", "{{aproperty | string.camelsingular}}");

            result.Should().Be("oneWord");
        }

        [Fact]
        public void WhenTransformWithSnakeSingular_ThenReturnsTransformedTemplate()
        {
            var model = new
            {
                aproperty = "One Words"
            };

            var result =
                model.Transform("adescription", "{{aproperty | string.snakesingular}}");

            result.Should().Be("one_word");
        }
    }
}