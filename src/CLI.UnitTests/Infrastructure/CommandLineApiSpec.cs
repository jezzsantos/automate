using System;
using Automate.CLI;
using Automate.CLI.Extensions;
using Automate.CLI.Infrastructure;
using FluentAssertions;
using Xunit;

namespace CLI.UnitTests.Infrastructure
{
    [Trait("Category", "Unit")]
    public class CommandLineExtensionsSpec
    {
        [Fact]
        public void WhenSetPropertyAssignmentAndNull_ThenThrows()
        {
            FluentActions.Invoking(() => CommandLineExtensions.SplitPropertyAssignment(null))
                .Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void WhenSetPropertyAssignmentAndContainsNoNameOrValue_ThenReturns()
        {
            "="
                .Invoking(x => x.SplitPropertyAssignment())
                .Should().Throw<AutomateException>()
                .WithMessage(
                    ExceptionMessages.CommandLineApiExtensions_SplitPropertyAssignment_ValueWithoutName.Format(
                        "="));
        }

        [Fact]
        public void WhenSetPropertyAssignmentAndContainsNoValue_ThenReturns()
        {
            var result = "aname".SplitPropertyAssignment();

            result.Name.Should().Be("aname");
            result.Value.Should().BeNull();
        }

        [Fact]
        public void WhenSetPropertyAssignmentAndContainsMissingValue_ThenReturns()
        {
            var result = "aname=".SplitPropertyAssignment();

            result.Name.Should().Be("aname");
            result.Value.Should().BeNull();
        }

        [Fact]
        public void WhenSetPropertyAssignmentAndContainsNoName_ThenReturns()
        {
            "=avalue"
                .Invoking(x => x.SplitPropertyAssignment())
                .Should().Throw<AutomateException>()
                .WithMessage(
                    ExceptionMessages.CommandLineApiExtensions_SplitPropertyAssignment_ValueWithoutName.Format(
                        "=avalue"));
        }

        [Fact]
        public void WhenSetPropertyAssignmentAndValueContainsDelimiter_ThenReturns()
        {
            var result = "aname=avalue=avalue".SplitPropertyAssignment();

            result.Name.Should().Be("aname");
            result.Value.Should().Be("avalue=avalue");
        }

        [Fact]
        public void WhenSetPropertyAssignmentAndValueContainsMultipleDelimiter_ThenReturns()
        {
            var result = "aname==avalue1=avalue2".SplitPropertyAssignment();

            result.Name.Should().Be("aname");
            result.Value.Should().Be("=avalue1=avalue2");
        }

        [Fact]
        public void WhenSetPropertyAssignmentAndValue_ThenReturns()
        {
            var result = "aname=avalue".SplitPropertyAssignment();

            result.Name.Should().Be("aname");
            result.Value.Should().Be("avalue");
        }
    }
}