using System;
using Automate.Domain;
using Automate.Extensions;
using FluentAssertions;
using Xunit;

namespace Core.UnitTests.Domain
{
    public class CodeTemplateCommandSpec
    {
        [Trait("Category", "Unit")]
        public class GivenAnyCommand
        {
            [Fact]
            public void WhenConstructedAndCodeTemplateIdIsMissing_ThenThrows()
            {
                FluentActions.Invoking(() =>
                        new CodeTemplateCommand("aname", null, false, "~/afilepath"))
                    .Should().Throw<ArgumentNullException>();
            }

            [Fact]
            public void WhenConstructedAndFilePathIsMissing_ThenThrows()
            {
                FluentActions.Invoking(() =>
                        new CodeTemplateCommand("aname", "acodetemplateid", false,
                            null))
                    .Should().Throw<ArgumentNullException>();
            }

            [Fact]
            public void WhenConstructedAndFilePathIsInvalid_ThenThrows()
            {
                FluentActions.Invoking(() =>
                        new CodeTemplateCommand("aname", "acodetemplateid", false,
                            "^aninvalidfilepath^"))
                    .Should().Throw<ArgumentOutOfRangeException>()
                    .WithMessage(ValidationMessages.Automation_InvalidFilePath.Substitute("^aninvalidfilepath^") + "*");
            }
        }
    }
}