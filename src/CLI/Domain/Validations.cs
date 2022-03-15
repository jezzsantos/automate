using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Automate.CLI.Extensions;

namespace Automate.CLI.Domain
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
            return Regex.IsMatch(path, @"^[~]{0,1}[\/\\\._ A-Za-z0-9\(\)\{\}]+$");
        }

        public static bool IsPropertyAssignment(string name, string value)
        {
            const string propertyNameExpression = @"[\w]+";
            const string propertyValueExpression = @"[\w\d \/\.\(\)]+";
            return Regex.IsMatch(name, propertyNameExpression) && Regex.IsMatch(value, propertyValueExpression);
        }
    }
}