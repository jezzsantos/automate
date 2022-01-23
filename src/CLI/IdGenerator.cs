using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using automate.Extensions;

namespace automate
{
    internal static class IdGenerator
    {
        internal const int IdCharacterLength = 8;
        internal const string IdCharacters =
            @"abcdefghjkmnpqrstuvwxyzABCDEFGHJKMNPQRSTUVWXYZ1234567890"; // omitted letters 'I', 'L' and 'O' in both cases

        public static string Create()
        {
            var result = new StringBuilder();

            for (var counter = 0; counter < IdCharacterLength; counter++)
            {
                var randomPositiveNumber = GetRandomPositiveNumber();
                var randomCharacterIndex = SelectCharacterIndex(randomPositiveNumber);

                var randomCharacter = IdCharacters[randomCharacterIndex];
                result.Append(randomCharacter);
            }

            return result.ToString();
        }

        public static bool IsValid(string id)
        {
            if (!id.HasValue())
            {
                return false;
            }

            if (id.Length != IdCharacterLength)
            {
                return false;
            }

            return id.ToCharArray().ToList().TrueForAll(@char => IdCharacters.Contains(@char));
        }

        private static int SelectCharacterIndex(uint randomPositiveNumber)
        {
            var remainder = randomPositiveNumber % (uint)IdCharacters.Length;

            // must round negative remainders to positive numbers
            var index = (int)remainder;

            return index;
        }

        private static uint GetRandomPositiveNumber()
        {
            var randomByte = new byte[sizeof(uint)];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(randomByte);
                return BitConverter.ToUInt32(randomByte, 0);
            }
        }
    }
}