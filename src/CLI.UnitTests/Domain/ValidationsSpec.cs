using System.IO;
using System.Linq;
using Automate.CLI.Domain;
using FluentAssertions;
using Xunit;

namespace CLI.UnitTests.Domain
{
    [Trait("Category", "Unit")]
    public class ValidationsSpec
    {
        [Fact]
        public void WhenIsRuntimeFilePathAndIsNull_ThenReturnsFalse()
        {
            var result = Validations.IsRuntimeFilePath(null);

            result.Should().BeFalse();
        }

        [Fact]
        public void WhenIsRuntimeFilePathWithAbsolutePath_ThenReturnsTrue()
        {
            var result = Validations.IsRuntimeFilePath(@"c:/adirectory/afilename.anextension");

            result.Should().BeTrue();
        }

        [Fact]
        public void WhenIsRuntimeFilePathWithAbsolutePathContainingExpressions_ThenReturnsTrue()
        {
            var result = Validations.IsRuntimeFilePath("C:/adirectory/{{anelementname.anattributename}}/{{anelementname.anattributename}}.anextension");

            result.Should().BeTrue();
        }

        [Fact]
        public void WhenIsRuntimeFilePathWithAbsolutePathAndInvalidPathChar_ThenReturnsFalse()
        {
            var invalidChar = Path.GetInvalidPathChars().First();
            var result = Validations.IsRuntimeFilePath($"C:/a{invalidChar}directory/afilename.anextension");

            result.Should().BeFalse();
        }

        [Fact]
        public void WhenIsRuntimeFilePathWithAbsolutePathAndInvalidFilenameChar_ThenReturnsFalse()
        {
            var invalidChar = Path.GetInvalidFileNameChars().First();
            var result = Validations.IsRuntimeFilePath($"C:/adirectory/a{invalidChar}filename.anextension");

            result.Should().BeFalse();
        }

        [Fact]
        public void WhenIsRuntimeFilePathWithNakedPath_ThenReturnsTrue()
        {
            var result = Validations.IsRuntimeFilePath("adirectory/afilename.anextension");

            result.Should().BeTrue();
        }

        [Fact]
        public void WhenIsRuntimeFilePathWithNakedPathContainingExpressions_ThenReturnsTrue()
        {
            var result = Validations.IsRuntimeFilePath("adirectory/{{anelementname.anattributename}}/{{anelementname.anattributename}}.anextension");

            result.Should().BeTrue();
        }

        [Fact]
        public void WhenIsRuntimeFilePathWithNakedPathAndInvalidPathChar_ThenReturnsFalse()
        {
            var invalidChar = Path.GetInvalidPathChars().First();
            var result = Validations.IsRuntimeFilePath($"a{invalidChar}directory/afilename.anextension");

            result.Should().BeFalse();
        }

        [Fact]
        public void WhenIsRuntimeFilePathWithNakedPathAndInvalidFilenameChar_ThenReturnsFalse()
        {
            var invalidChar = Path.GetInvalidFileNameChars().First();
            var result = Validations.IsRuntimeFilePath($"adirectory/a{invalidChar}filename.anextension");

            result.Should().BeFalse();
        }

        [Fact]
        public void WhenIsRuntimeFilePathWithRelativePath_ThenReturnsTrue()
        {
            var result = Validations.IsRuntimeFilePath("/adirectory/afilename.anextension");

            result.Should().BeTrue();
        }

        [Fact]
        public void WhenIsRuntimeFilePathWithRelativePathContainingExpressions_ThenReturnsTrue()
        {
            var result = Validations.IsRuntimeFilePath("/adirectory/{{anelementname.anattributename}}/{{anelementname.anattributename}}.anextension");

            result.Should().BeTrue();
        }

        [Fact]
        public void WhenIsRuntimeFilePathWithRelativePathAndInvalidPathChar_ThenReturnsFalse()
        {
            var invalidChar = Path.GetInvalidPathChars().First();
            var result = Validations.IsRuntimeFilePath($"/a{invalidChar}directory/afilename.anextension");

            result.Should().BeFalse();
        }

        [Fact]
        public void WhenIsRuntimeFilePathWithRelativePathAndInvalidFilenameChar_ThenReturnsFalse()
        {
            var invalidChar = Path.GetInvalidFileNameChars().First();
            var result = Validations.IsRuntimeFilePath($"/adirectory/a{invalidChar}filename.anextension");

            result.Should().BeFalse();
        }

        [Fact]
        public void WhenIsRuntimeFilePathWithSolutionPath_ThenReturnsTrue()
        {
            var result = Validations.IsRuntimeFilePath("~/adirectory/afilename.anextension");

            result.Should().BeTrue();
        }

        [Fact]
        public void WhenIsRuntimeFilePathWithSolutionPathContainingExpressions_ThenReturnsTrue()
        {
            var result = Validations.IsRuntimeFilePath("~/adirectory/{{anelementname.anattributename}}/{{anelementname.anattributename}}.anextension");

            result.Should().BeTrue();
        }

        [Fact]
        public void WhenIsRuntimeFilePathWithSolutionPathAndInvalidPathChar_ThenReturnsFalse()
        {
            var invalidChar = Path.GetInvalidPathChars().First();
            var result = Validations.IsRuntimeFilePath($"~/a{invalidChar}directory/afilename.anextension");

            result.Should().BeFalse();
        }

        [Fact]
        public void WhenIsRuntimeFilePathWithSolutionPathAndInvalidFilenameChar_ThenReturnsFalse()
        {
            var invalidChar = Path.GetInvalidFileNameChars().First();
            var result = Validations.IsRuntimeFilePath($"~/adirectory/a{invalidChar}filename.anextension");

            result.Should().BeFalse();
        }
    }
}