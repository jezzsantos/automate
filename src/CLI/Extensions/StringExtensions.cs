using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

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

        private static string FormatStructuredMessage(string messageTemplate, params object[] args)
        {
            messageTemplate.GuardAgainstNullOrEmpty(nameof(messageTemplate));

            return ServiceStack.StringExtensions.ToJson(new
            {
                message = messageTemplate,
                values = GetMessageTemplateTokens(messageTemplate, args)
            });
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
            var replacements = tokens.ToDictionary(token => token.Value.TrimStart('{').TrimEnd('}'), _ =>
            {
                paramIndex++;
                return args.Length >= paramIndex
                    ? args[paramIndex - 1].ToString()
                    : null;
            });

            return replacements;
        }
    }
}