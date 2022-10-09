using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Semver;

namespace Automate.Common.Extensions
{
    public class StructuredMessage
    {
        public string Message { get; set; }

        public Dictionary<string, object> Values { get; set; }
    }

    public static class StringExtensions
    {
        public static SemVersion ToSemVersion(this string value)
        {
            if (value.HasNoValue())
            {
                return null;
            }

            return SemVersion.Parse(value, SemVersionStyles.Any);
        }

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

        public static string Substitute(this string value, params object[] arguments)
        {
            if (arguments.HasNone())
            {
                return value;
            }

            return string.Format(value, arguments);
        }

        public static string SubstituteTemplate(this string value, params object[] arguments)
        {
            return FormatMessageTemplate(value, arguments);
        }

        public static StructuredMessage SubstituteTemplateStructured(this string value, params object[] arguments)
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

            if (value.Contains('_'))
            {
                return FromSnakeCase(value);
            }

            if (value.Contains(' '))
            {
                return FromSentence(value);
            }

            return FromWholeWord(value);

            static string FromWholeWord(string value)
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
                    var isCurrentUpper = char.IsUpper(currentCharacter);
                    var isNextUpper = char.IsUpper(nextCharacter);

                    if (firstPart
                        && isCurrentUpper
                        && (isNextUpper || index == 0))
                    {
                        currentCharacter = char.ToLower(currentCharacter);
                    }
                    else
                    {
                        firstPart = false;
                    }

                    newValue[index] = currentCharacter;
                }

                return new string(newValue);
            }

            static string FromSentence(string value)
            {
                var parts = value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                return FromWords(parts);
            }

            static string FromSnakeCase(string value)
            {
                var parts = value.Split('_', StringSplitOptions.RemoveEmptyEntries);
                return FromWords(parts);
            }

            static string FromWords(string[] parts)
            {
                var sb = new StringBuilder();
                var firstPart = true;
                foreach (var part in parts)
                {
                    var camelCased = FromWholeWord(part);
                    if (firstPart)
                    {
                        sb.Append(camelCased);
                        firstPart = false;
                    }
                    else
                    {
                        sb.Append(camelCased.Capitalize());
                    }
                }
                return sb.ToString();
            }
        }

        public static string ToPascalCase(this string value)
        {
            if (value.HasNoValue())
            {
                return value;
            }

            var camelCase = value.ToCamelCase();
            return camelCase.Capitalize();
        }

        public static string ToCapitalizedWords(this string value)
        {
            if (value.HasNoValue())
            {
                return value;
            }

            var pascal = value.ToPascalCase();

            var words = Regex.Split(pascal, @"(?<!^)(?=\p{Lu}\p{Ll})");
            var sb = new StringBuilder();
            var index = 0;
            foreach (var word in words)
            {
                var space = ++index == 1
                    ? string.Empty
                    : " ";

                sb.Append($"{space}{word.Trim(' ').Capitalize()}");
            }

            return sb.ToString();
        }

        private static string Capitalize(this string value)
        {
            if (value.HasNoValue())
            {
                return value;
            }

            return char.ToUpper(value[0]) + value.SafeSubstring(1, value.Length);
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

        private static StructuredMessage FormatStructuredMessage(string messageTemplate, params object[] args)
        {
            messageTemplate.GuardAgainstNullOrEmpty(nameof(messageTemplate));

            var instance = new StructuredMessage
            {
                Message = messageTemplate,
                Values = GetMessageTemplateTokens(messageTemplate, args)
            };

            return instance;
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

                string replacementString;
                var replacement = replacements[key];
                if (replacement.NotExists())
                {
                    replacementString = token;
                }
                else
                {
                    if (replacement is JsonNode jsonNode)
                    {
                        replacementString = jsonNode.ToJsonString(new JsonSerializerOptions
                        {
                            WriteIndented = true
                        });
                    }
                    else
                    {
                        if (replacement is string value)
                        {
                            replacementString = value;
                        }
                        else
                        {
                            replacementString = replacement.ToString();
                        }

                        if (replacementString.HasNoValue())
                        {
                            replacementString = token;
                        }
                    }
                }

                message = message.Replace(token, replacementString);
            }

            return message;
        }

        private static Dictionary<string, object> GetMessageTemplateTokens(string messageTemplate, params object[] args)
        {
            var tokens = Regex.Matches(messageTemplate, @"\{(.+?)\}");
            if (tokens.Count == 0)
            {
                return new Dictionary<string, object>();
            }

            var paramIndex = 0;
            var replacements = tokens
                .DistinctBy(token => token.Value)
                .ToDictionary(token => token.Value.TrimStart('{').TrimEnd('}'), _ =>
                {
                    paramIndex++;
                    return args.Length >= paramIndex
                        ? args[paramIndex - 1]
                        : null;
                })
                .Where(pair => pair.Value.Exists())
                .ToDictionary(pair => pair.Key, pair => pair.Value);

            return replacements;
        }
    }
}