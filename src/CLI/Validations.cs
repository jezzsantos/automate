using System;
using System.Linq;
using System.Text.RegularExpressions;
using automate.Extensions;

namespace automate
{
    internal static class Validations
    {
        public static bool IsIdentifier(string value)
        {
            return Regex.IsMatch(value, @"^[a-zA-Z0-9\.]+$");
        }

        public static bool IsSupportedAttributeType(string value)
        {
            return Attribute.SupportedTypes.Contains(value);
        }

        public static bool IsDefaultValueForType(string defaultValue, string type)
        {
            if (!defaultValue.HasValue())
            {
                return true;
            }

            switch (type)
            {
                case Attribute.DefaultType:
                    return true;

                case "boolean":
                    return bool.TryParse(defaultValue, out var _);

                case "integer":
                    return int.TryParse(defaultValue, out var _);

                case "datetime":
                    return DateTime.TryParse(defaultValue, out var _);

                default:
                    throw new ArgumentOutOfRangeException(
                        ExceptionMessages.Validations_UnsupportedAttributeType.Format(type,
                            Attribute.SupportedTypes.Join(", ")));
            }
        }
    }
}