﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Automate.Common.Domain;
using Automate.Common.Extensions;
using Automate.Runtime.Domain;

namespace Automate.Authoring.Domain
{
    internal static class Validations
    {
        private const string NameIdentifierExpression = @"[a-zA-Z0-9_\.\-]+";
        private const string DescriptiveNameExpression = @"[a-zA-Z0-9_\.\-]+";
        private static readonly string[] ReservedNames = { nameof(DraftItem.Parent) };

        public static bool IsNameIdentifier(string value)
        {
            var characterMatch = Regex.IsMatch(value, $@"^{NameIdentifierExpression}$");
            var wordMatch = IsNotReservedName(value, ReservedNames);

            return characterMatch && wordMatch;
        }

        public static bool IsNotReservedName(string value, string[] reservedNames)
        {
            return !reservedNames.Contains(value, StringComparer.OrdinalIgnoreCase);
        }

        public static bool IsDescriptiveName(string value)
        {
            return Regex.IsMatch(value, $@"^{DescriptiveNameExpression}$");
        }

        public static bool IsIdentifiers(List<string> values)
        {
            return values.TrueForAll(IdGenerator.IsValid);
        }

        public static bool IsSupportedAttributeDataType(string dataType)
        {
            return Attribute.SupportedDataTypes.Contains(dataType);
        }

        public static bool IsValueOfDataType(string value, string dataType)
        {
            if (!value.HasValue())
            {
                return true;
            }

            return Attribute.IsValidDataType(dataType, value);
        }

        public static bool IsRuntimeFilePath(string path)
        {
            if (path.NotExists())
            {
                return false;
            }

            var remainingPath = RemoveSolutionRoot();
            remainingPath = ReplaceTemplatingExpressions();
            if (remainingPath.NotExists())
            {
                return true;
            }

            return Uri.IsWellFormedUriString($"{Uri.UriSchemeFile}{remainingPath}", UriKind.RelativeOrAbsolute);

            string RemoveSolutionRoot()
            {
                return path.StartsWith("~")
                    ? path.Substring(1)
                    : path;
            }

            string ReplaceTemplatingExpressions()
            {
                return Regex.Replace(remainingPath, "{{[\x00-\x7F]+}}", "anexpression");
            }
        }

        public static bool IsPropertyAssignment(string name, string value)
        {
            const string propertyNameExpression = $@"{NameIdentifierExpression}";
            const string propertyValueExpression = @"[\w\d \/\.\(\)]+";

            var isValidName = Regex.IsMatch(name, propertyNameExpression);

            // ReSharper disable once SimplifyConditionalTernaryExpression
            var isValidValue = value.HasValue()
                ? Regex.IsMatch(value, propertyValueExpression)
                : true;

            return isValidName && isValidValue;
        }

        public static bool IsVersionInstruction(string instruction)
        {
            if (instruction.HasNoValue())
            {
                return true;
            }

            if (instruction.EqualsIgnoreCase(PatternVersioningHistory.AutoIncrementInstruction))
            {
                return true;
            }

            if (!Version.TryParse(instruction, out var version))
            {
                return false;
            }

            if (version.Revision != -1)
            {
                return false;
            }

            return true;
        }
    }
}