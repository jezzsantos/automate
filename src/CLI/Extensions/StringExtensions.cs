using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Automate.CLI.Extensions
{
    internal static class StringExtensions
    {
        private const int LowerCaseOffset = 'a' - 'A';

        public static int ToInt(this string text)
        {
            return text == null
                ? default
                : int.Parse(text);
        }

        public static int ToInt(this string text, int defaultValue)
        {
            return int.TryParse(text, out var ret)
                ? ret
                : defaultValue;
        }

        public static TEnum ToEnumOrDefault<TEnum>(this string value, TEnum defaultValue)
        {
            if (value.HasNoValue())
            {
                return defaultValue;
            }

            return (TEnum)Enum.Parse(typeof(TEnum), value, true);
        }

        public static string WithoutTrailingSlash(this string path)
        {
            return path.Trim('/');
        }

        public static string Format(this string value, params object[] arguments)
        {
            return string.Format(value, arguments);
        }

        public static string FormatTemplate(this string value, params object[] arguments)
        {
            return FormatMessageTemplate(value, arguments);
        }

        public static string FormatTemplateStructured(this string value, params object[] arguments)
        {
            return FormatStructuredMessage(value, arguments);
        }

        public static bool HasValue(this string value)
        {
            return !string.IsNullOrEmpty(value);
        }

        public static bool HasNoValue(this string value)
        {
            return !value.HasValue();
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

        public static string ToSnakeCase(this string value)
        {
            if (value.HasNoValue())
            {
                return value;
            }

            value = value.ToCamelCase();

            var builder = new StringBuilder();
            foreach (var character in value)
            {
                if (char.IsDigit(character)
                    || (char.IsLetter(character) && char.IsLower(character)) || character == '_')
                {
                    builder.Append(character);
                }
                else
                {
                    builder.Append('_');
                    builder.Append(char.ToLower(character));
                }
            }
            return builder.ToString();
        }

        public static string ToCamelCase(this string value)
        {
            if (value.HasNoValue())
            {
                return value;
            }

            if (value.IndexOf('_') >= 0)
            {
                var parts = value.Split('_');
                var sb = new StringBuilder();
                var firstPart = true;
                foreach (var part in parts)
                {
                    if (part.HasNoValue())
                    {
                        continue;
                    }
                    var camelCased = part.ToCamelCase();
                    if (firstPart)
                    {
                        sb.Append(camelCased);
                        firstPart = false;
                    }
                    else
                    {
                        sb.Append(char.ToUpper(camelCased[0]) + camelCased.SafeSubstring(1, camelCased.Length));
                    }
                }
                return sb.ToString();
            }
            else
            {
                var valueLength = value.Length;
                var newValue = new char[valueLength];
                var firstPart = true;
                for (var index = 0; index < valueLength; ++index)
                {
                    var currentCharacter = value[index];
                    var nextCharacter = index < valueLength - 1
                        ? value[index + 1]
                        : 'A';
                    var isCurrentUpper = currentCharacter is >= 'A' and <= 'Z';
                    var isNextUpper = nextCharacter is >= 'A' and <= 'Z';

                    if (firstPart
                        && isCurrentUpper
                        && (isNextUpper || index == 0))
                    {
                        currentCharacter = (char)(currentCharacter + LowerCaseOffset);
                    }
                    else
                    {
                        firstPart = false;
                    }

                    newValue[index] = currentCharacter;
                }

                return new string(newValue)
                    .Replace(" ", string.Empty);
            }
        }

        public static string ToPascalCase(this string value)
        {
            if (value.HasNoValue())
            {
                return value;
            }

            if (value.IndexOf('_') >= 0)
            {
                var parts = value.Split('_');
                var sb = new StringBuilder();
                foreach (var part in parts)
                {
                    if (part.HasNoValue())
                    {
                        continue;
                    }
                    var camelCased = part.ToCamelCase();
                    sb.Append(char.ToUpper(camelCased[0]) + camelCased.SafeSubstring(1, camelCased.Length));
                }
                return sb.ToString();
            }

            var camelCase = value.ToCamelCase();
            return char.ToUpper(camelCase[0]) + camelCase.SafeSubstring(1, camelCase.Length);
        }

        private static string SafeSubstring(this string value, int startIndex, int length)
        {
            if (value.HasNoValue() || length <= 0)
            {
                return string.Empty;
            }

            if (startIndex < 0)
            {
                startIndex = 0;
            }

            if (value.Length >= startIndex + length)
            {
                return value.Substring(startIndex, length);
            }

            return value.Length > startIndex
                ? value.Substring(startIndex)
                : string.Empty;
        }

        private static string FormatStructuredMessage(string messageTemplate, params object[] args)
        {
            messageTemplate.GuardAgainstNullOrEmpty(nameof(messageTemplate));

            var instance = new
            {
                message = messageTemplate,
                values = GetMessageTemplateTokens(messageTemplate, args)
            };

            return instance.ToJson(false);
        }

        private static string FormatMessageTemplate(string messageTemplate, params object[] args)
        {
            messageTemplate.GuardAgainstNullOrEmpty(nameof(messageTemplate));

            var replacements = GetMessageTemplateTokens(messageTemplate, args);
            if (replacements.Count == 0)
            {
                return messageTemplate;
            }

            var message = messageTemplate;
            foreach (var key in replacements.Keys)
            {
                var token = $"{{{key}}}";
                message = message.Replace(token, replacements[key].HasValue()
                    ? replacements[key]
                    : token);
            }

            return message;
        }

        private static Dictionary<string, string> GetMessageTemplateTokens(string messageTemplate, params object[] args)
        {
            var tokens = Regex.Matches(messageTemplate, @"\{(.+?)\}");
            if (tokens.Count == 0)
            {
                return new Dictionary<string, string>();
            }

            var paramIndex = 0;
            var replacements = tokens
                .DistinctBy(token => token.Value)
                .ToDictionary(token => token.Value.TrimStart('{').TrimEnd('}'), _ =>
                {
                    paramIndex++;
                    return args.Length >= paramIndex
                        ? args[paramIndex - 1]?.ToString()
                        : null;
                })
                .Where(pair => pair.Value.HasValue())
                .ToDictionary(pair => pair.Key, pair => pair.Value);

            return replacements;
        }
    }
}