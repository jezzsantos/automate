using System;
using System.IO;
using automate.Domain;
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
            var testDirectory = Path.Combine(Environment.CurrentDirectory, "CodeTemplateSpec");
            this.template = new CodeTemplate("aname", Path.Combine(testDirectory, "afilepath.txt"));
            if (Directory.Exists(testDirectory))
            {
                Directory.Delete(testDirectory, true);
            }
        }

        [Fact]
        public void WhenConstructed_ThenNamed()
        {
            this.template.Name.Should().Be("aname");
        }
    }
}