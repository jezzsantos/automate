using System;
using Automate.Extensions;
using FluentAssertions;
using Xunit;

namespace Core.UnitTests.Extensions
{
    [Trait("Category", "Unit")]
    public class ExceptionExtensionsSpec
    {
        [Fact]
        public void WhenExceptionExtensionsAndNoMessage_ThenReturnsEmptyMessage()
        {
            var result = new Exception("").ToMessages();

            result.Should().Be("");
        }

        [Fact]
        public void WhenExceptionExtensionsAndNoInnerException_ThenReturnsMessage()
        {
            var result = new Exception("amessage").ToMessages();

            result.Should().Be("amessage");
        }

        [Fact]
        public void WhenExceptionExtensionsWithDescendantInnerExceptions_ThenReturnsExceptionMessages()
        {
            var result =
                new Exception("amessage1", new Exception("amessage2", new Exception("amessage3"))).ToMessages();

            result.Should().Be($"amessage1{Environment.NewLine}amessage2{Environment.NewLine}amessage3");
        }

        [Fact]
        public void WhenExceptionExtensionsAndNoInnerExceptionWithIndenting_ThenReturnsMessage()
        {
            var result = new Exception("amessage").ToMessages(true);

            result.Should().Be("amessage");
        }

        [Fact]
        public void WhenExceptionExtensionsWithDescendantInnerExceptionsWithIndenting_ThenReturnsExceptionMessages()
        {
            var result =
                new Exception("amessage1", new Exception("amessage2", new Exception("amessage3"))).ToMessages(true);

            result.Should().Be($"amessage1{Environment.NewLine}\tamessage2{Environment.NewLine}\t\tamessage3");
        }
    }
}