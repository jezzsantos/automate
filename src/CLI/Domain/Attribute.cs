using System;
using System.Collections.Generic;
using automate.Extensions;
using StringExtensions = ServiceStack.StringExtensions;

namespace automate.Domain
{
    internal class Attribute : INamedEntity, IValidateable
    {
        public const string DefaultType = "string";
        public static readonly string[] SupportedDataTypes =
        {
            DefaultType, "bool", "int", "decimal", "DateTime"
        };
        public static readonly string[] ReservedAttributeNames = { nameof(Id) };

        public Attribute(string name, string dataType = DefaultType, bool isRequired = false,
            string defaultValue = null, List<string> choices = null)
        {
            name.GuardAgainstNullOrEmpty(nameof(name));
            name.GuardAgainstInvalid(Validations.IsNameIdentifier, nameof(name),
                ValidationMessages.InvalidNameIdentifier);
            if (dataType.HasValue())
            {
                dataType.GuardAgainstInvalid(Validations.IsSupportedAttributeDataType, nameof(dataType),
                    ValidationMessages.Attribute_UnsupportedDataType, SupportedDataTypes.Join(", "));
            }
            var resolvedDataType = dataType.HasValue()
                ? dataType
                : DefaultType;
            if (defaultValue.HasValue())
            {
                defaultValue.GuardAgainstInvalid(
                    dv => Validations.IsValueOfDataType(dv, resolvedDataType),
                    nameof(defaultValue),
                    ValidationMessages.Attribute_InvalidDefaultValue, resolvedDataType);
                if (choices.HasAny())
                {
                    defaultValue.GuardAgainstInvalid(choices.Contains, nameof(defaultValue),
                        ValidationMessages.Attribute_DefaultValueIsNotAChoice, StringExtensions.Join(choices, "; "));
                }
            }

            if (choices.HasAny())
            {
                choices.ForEach(choice =>
                    choice.GuardAgainstInvalid(
                        _ => Validations.IsValueOfDataType(choice, resolvedDataType), nameof(choices),
                        ValidationMessages.Attribute_WrongDataTypeChoice.Format(choice, dataType)));
            }

            Id = IdGenerator.Create();
            Name = name;
            DataType = resolvedDataType;
            IsRequired = isRequired;
            DefaultValue = defaultValue;
            Choices = choices ?? new List<string>();
        }

        /// <summary>
        ///     For serialization
        /// </summary>
        public Attribute()
        {
        }

        public string DataType { get; set; }

        public bool IsRequired { get; set; }

        public string DefaultValue { get; set; }

        public List<string> Choices { get; set; }

        public bool IsValidDataType(string value)
        {
            return IsValidDataType(DataType, value);
        }

        public static bool IsValidDataType(string dataType, string value)
        {
            switch (dataType)
            {
                case "string":
                    return true;

                case "bool":
                    return bool.TryParse(value, out var _);

                case "int":
                    return int.TryParse(value, out var _);

                case "decimal":
                    return decimal.TryParse(value, out var _);

                case "DateTime":
                    return DateTime.TryParse(value, out var _);

                default:
                    throw new ArgumentOutOfRangeException(
                        ValidationMessages.Attribute_UnsupportedDataType.Format(dataType,
                            SupportedDataTypes.Join(", ")));
            }
        }

        public static bool IsValidDataType(string dataType, object value)
        {
            switch (dataType)
            {
                case "string":
                    return value is string || value is null;

                case "bool":
                    return value is bool;

                case "int":
                    return value is int;

                case "decimal":
                    return value is decimal;

                case "DateTime":
                    return value is DateTime;

                default:
                    throw new ArgumentOutOfRangeException(
                        ValidationMessages.Attribute_UnsupportedDataType.Format(dataType,
                            SupportedDataTypes.Join(", ")));
            }
        }

        public static object SetValue(object value, string dataType)
        {
            switch (dataType)
            {
                case "string":
                    return value.IsNull()
                        ? null
                        : value.ToString();

                case "bool":
                    return value.IsNull()
                        ? false
                        : Convert.ToBoolean(value);

                case "int":
                    return value.IsNull()
                        ? null
                        : Convert.ToInt32(value);

                case "decimal":
                    return value.IsNull() ? null : value.IsNull() ? false : Convert.ToDecimal(value);

                case "DateTime":
                    return value.IsNull()
                        ? null
                        : Convert.ToDateTime(value);

                default:
                    throw new ArgumentOutOfRangeException(
                        ValidationMessages.Attribute_UnsupportedDataType.Format(dataType,
                            SupportedDataTypes.Join(", ")));
            }
        }

        public string Id { get; set; }

        public string Name { get; set; }

        public ValidationResults Validate(ValidationContext context, object value)
        {
            var results = ValidationResults.None;
            if (IsRequired)
            {
                if (value.IsNull())
                {
                    results.Add(new ValidationResult(context,
                        ValidationMessages.Attribute_ValidationRule_RequiredValue.Format(Name)));
                }
            }

            if (!IsValidDataType(DataType, value))
            {
                results.Add(
                    new ValidationResult(context,
                        ValidationMessages.Attribute_ValidationRule_WrongDataTypeValue.Format(value, DataType)));
            }

            return results;
        }
    }
}