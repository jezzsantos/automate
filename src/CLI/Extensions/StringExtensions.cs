using System;
using System.Linq;

namespace automate.Extensions
{
    internal static class StringExtensions
    {
        public static string WithoutTrailingSlash(this string path)
        {
            return path.Trim('/');
        }

        public static string Format(this string value, params object[] arguments)
        {
            return string.Format(value, arguments);
        }

        public static bool HasValue(this string value)
        {
            return !string.IsNullOrEmpty(value);
        }

        public static bool EqualsOrdinal(this string value, string other)
        {
            return string.Equals(value, other, StringComparison.Ordinal);
        }

        public static bool NotEqualsOrdinal(this string value, string other)
        {
            return !value.EqualsOrdinal(other);
        }

        public static bool EqualsIgnoreCase(this string value, string other)
        {
            return string.Equals(value, other, StringComparison.OrdinalIgnoreCase);
        }

        public static bool NotEqualsIgnoreCase(this string value, string other)
        {
            return !value.EqualsIgnoreCase(other);
        }

        public static bool IsOneOf(this string source, params string[] values)
        {
            source.GuardAgainstNull(nameof(source));

            if (values.HasNone())
            {
                return false;
            }

            return values.Contains(source);
        }

        public static bool ToBool(this string value)
        {
            if (!value.HasValue())
            {
                return false;
            }
            return Convert.ToBoolean(value);
        }
    }
}