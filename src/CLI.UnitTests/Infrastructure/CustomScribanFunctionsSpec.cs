using Automate.CLI.Infrastructure;
using FluentAssertions;
using Xunit;

namespace CLI.UnitTests.Infrastructure
{
    [Trait("Category", "Unit")]
    public class CustomScribanFunctionsSpec
    {
        [Fact]
        public void WhenTransformWithCamelCase_ThenReturnsTransformedTemplate()
        {
            var model = new
            {
                aproperty = "OneTwoThree"
            };

            var result =
                model.Transform("adescription", "{{aproperty | camelcase}}\\n{{aproperty | string.camelcase}}");

            result.Should().Be("oneTwoThree\\noneTwoThree");
        }
    }
}