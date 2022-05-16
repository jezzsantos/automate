using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Automate.CLI.Extensions
{
    internal static class StringExtensions
    {
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

        private static string ToCamelCase(this string value)
        {
            return JsonNamingPolicy.CamelCase.ConvertName(value);
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