using System.IO;
using System.Linq;
using Automate.Authoring.Domain;
using FluentAssertions;
using Xunit;

namespace Core.UnitTests.Authoring.Domain
{
    [Trait("Category", "Unit")]
    public class ValidationsSpec
    {
        [Fact]
        public void WhenIsNameIdentifierWithInvalidCharacters_ThenReturnsFalse()
        {
            var result = Validations.IsNameIdentifier("aninvalidname^");

            result.Should().BeFalse();
        }

        [Fact]
        public void WhenIsNameIdentifierAndReservedName_ThenReturnsFalse()
        {
            var result = Validations.IsNameIdentifier("Parent");

            result.Should().BeFalse();
        }

        [Fact]
        public void WhenIsNameIdentifier_ThenReturnsTrue()
        {
            var result = Validations.IsNameIdentifier("aname");

            result.Should().BeTrue();
        }

        [Fact]
        public void WhenIsNotReservedNameAndReservedName_ThenReturnsFalse()
        {
            var result = Validations.IsNotReservedName("areservedname", new[] { "areservedname" });

            result.Should().BeFalse();
        }

        [Fact]
        public void WhenIsNotReservedNameAndNotReservedName_ThenReturnsTrue()
        {
            var result = Validations.IsNotReservedName("aname", new[] { "areservedname" });

            result.Should().BeTrue();
        }

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
            var result = Validations.IsRuntimeFilePath(
                "C:/adirectory/{{anelementname.anattributename}}/{{anelementname.anattributename}}.anextension");

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
            var result =
                Validations.IsRuntimeFilePath(
                    "adirectory/{{anelementname.anattributename}}/{{anelementname.anattributename}}.anextension");

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
            var result = Validations.IsRuntimeFilePath(
                "/adirectory/{{anelementname.anattributename}}/{{anelementname.anattributename}}.anextension");

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
        public void WhenIsRuntimeFilePathWithDraftPath_ThenReturnsTrue()
        {
            var result = Validations.IsRuntimeFilePath("~/adirectory/afilename.anextension");

            result.Should().BeTrue();
        }

        [Fact]
        public void WhenIsRuntimeFilePathWithDraftPathContainingExpressions_ThenReturnsTrue()
        {
            var result = Validations.IsRuntimeFilePath(
                "~/adirectory/{{anelementname.anattributename}}/{{anelementname.anattributename}}.anextension");

            result.Should().BeTrue();
        }

        [Fact]
        public void WhenIsRuntimeFilePathWithDraftPathAndInvalidPathChar_ThenReturnsFalse()
        {
            var invalidChar = Path.GetInvalidPathChars().First();
            var result = Validations.IsRuntimeFilePath($"~/a{invalidChar}directory/afilename.anextension");

            result.Should().BeFalse();
        }

        [Fact]
        public void WhenIsRuntimeFilePathWithDraftPathAndInvalidFilenameChar_ThenReturnsFalse()
        {
            var invalidChar = Path.GetInvalidFileNameChars().First();
            var result = Validations.IsRuntimeFilePath($"~/adirectory/a{invalidChar}filename.anextension");

            result.Should().BeFalse();
        }
    }
}