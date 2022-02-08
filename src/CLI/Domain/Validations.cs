using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using automate.Extensions;

namespace automate.Domain
{
    internal static class Validations
    {
        public static bool IsNameIdentifier(string value)
        {
            return Regex.IsMatch(value, @"^[a-zA-Z0-9\.]+$");
        }

        public static bool IsIdentifiers(List<string> values)
        {
            return values.TrueForAll(IdGenerator.IsValid);
        }

        public static bool IsSupportedAttributeDataType(string value)
        {
            return Attribute.SupportedDataTypes.Contains(value);
        }

        public static bool IsDefaultValueForAttributeDataType(string defaultValue, string dataType)
        {
            if (!defaultValue.HasValue())
            {
                return true;
            }

            switch (dataType)
            {
                case Attribute.DefaultType:
                    return true;

                case "bool":
                    return bool.TryParse(defaultValue, out var _);

                case "int":
                    return int.TryParse(defaultValue, out var _);

                case "DateTime":
                    return DateTime.TryParse(defaultValue, out var _);

                default:
                    throw new ArgumentOutOfRangeException(
                        ValidationMessages.Attribute_UnsupportedDataType.Format(dataType,
                            Attribute.SupportedDataTypes.Join(", ")));
            }
        }

        public static bool IsRuntimeFilePath(string path)
        {
            return Regex.IsMatch(path, @"^[~]{0,1}[\/\\\.A-Za-z0-9\(\)\{\}]+$");
        }
    }
}