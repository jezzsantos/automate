using automate;
using FluentAssertions;
using Xunit;

namespace CLI.UnitTests
{
    [Trait("Category", "Unit")]
    public class IdGeneratorSpec
    {
        [Fact]
        public void WhenCreate_ThenReturnsRandomIdentifier()
        {
            var result = IdGenerator.Create();

            result.Length.Should().Be(IdGenerator.IdCharacterLength);
            result.ToCharArray().Should().Contain(c => IdGenerator.IdCharacters.Contains(c));
        }

        [Fact]
        public void CharactersDoNotContainAmbiguousReadableCharacters()
        {
            IdGenerator.IdCharacters.ToCharArray().Should().NotContain(new[] { 'i', 'l', 'o', 'I', 'L', 'O' });
        }

        [Fact]
        public void WhenIsvalidAndEmpty_ThenReturnsFalse()
        {
            var result = IdGenerator.IsValid(string.Empty);

            result.Should().BeFalse();
        }

        [Fact]
        public void WhenIsValidAndTooShort_ThenReturnsFalse()
        {
            var identifier = IdGenerator.Create()
                .Substring(0, IdGenerator.IdCharacterLength - 1);

            var result = IdGenerator.IsValid(identifier);

            result.Should().BeFalse();
        }

        [Fact]
        public void WhenIsValidAndTooLong_ThenReturnsFalse()
        {
            var identifier = IdGenerator.Create() + IdGenerator.Create();

            var result = IdGenerator.IsValid(identifier);

            result.Should().BeFalse();
        }

        [Fact]
        public void WhenIsValidAndContainsInvalidIdentifier_ThenReturnsFalse()
        {
            var identifier = IdGenerator.Create();
            identifier = identifier.Replace(identifier[0], 'i');

            var result = IdGenerator.IsValid(identifier);

            result.Should().BeFalse();
        }

        [Fact]
        public void WhenIsValidAndContainsValidIdentifier_ThenReturnsTrue()
        {
            var identifier = IdGenerator.Create();

            var result = IdGenerator.IsValid(identifier);

            result.Should().BeTrue();
        }
    }
}