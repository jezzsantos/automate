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
        public void WhenTransformWithToPlural_ThenReturnsTransformedTemplate()
        {
            var model = new
            {
                simple = "word",
                infinitive = "sheep"
            };

            var result =
                model.Transform("adescription", "{{simple | string.to_plural}}\n{{infinitive | string.to_plural}}");

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
                model.Transform("adescription", "{{simple | string.to_singular}}\n{{infinitive | string.to_singular}}");

            result.Should().Be("word\nsheep");
        }

        [Fact]
        public void WhenTransformWithPascalCasedPlural_ThenReturnsTransformedTemplate()
        {
            var model = new
            {
                aproperty = "word"
            };

            var result =
                model.Transform("adescription", "{{aproperty | string.to_plural | string.pascalcase}}");

            result.Should().Be("Words");
        }

        [Fact]
        public void WhenTransformWithCamelCasedPlural_ThenReturnsTransformedTemplate()
        {
            var model = new
            {
                aproperty = "Word"
            };

            var result =
                model.Transform("adescription", "{{aproperty | string.to_plural | string.camelcase}}");

            result.Should().Be("words");
        }
    }
}