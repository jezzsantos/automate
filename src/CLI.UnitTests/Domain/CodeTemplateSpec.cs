using System;
using Automate.CLI.Domain;
using FluentAssertions;
using Xunit;

namespace CLI.UnitTests.Domain
{
    [Trait("Category", "Unit")]
    public class CodeTemplateSpec
    {
        private readonly CodeTemplate template;

        public CodeTemplateSpec()
        {
            this.template = new CodeTemplate("aname", "afilepath.txt", "txt");
        }

        [Fact]
        public void WhenConstructed_ThenNamed()
        {
            this.template.Name.Should().Be("aname");
            this.template.LastModifiedUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
            this.template.Metadata.OriginalFilePath.Should().Be("afilepath.txt");
            this.template.Metadata.OriginalFileExtension.Should().Be("txt");
        }
    }
}