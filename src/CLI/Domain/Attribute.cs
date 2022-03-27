using System;
using System.Collections.Generic;
using Automate.CLI.Extensions;

namespace Automate.CLI.Domain
{
    internal class Attribute : INamedEntity, IValidateable, IPersistable
    {
        public const string DefaultType = "string";
        public static readonly string[] SupportedDataTypes =
        {
            DefaultType, "bool", "int", "decimal", "DateTime"
        };
        public static readonly string[] ReservedAttributeNames = { nameof(Id) };
        private List<string> choices;

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
                        ValidationMessages.Attribute_DefaultValueIsNotAChoice, choices.SafeJoin("; "));
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
            this.choices = choices.HasAny()
                ? choices
                : new List<string>();
        }

        private Attribute(PersistableProperties properties, IPersistableFactory factory)
        {
            Id = properties.Rehydrate<string>(factory, nameof(Id));
            Name = properties.Rehydrate<string>(factory, nameof(Name));
            DataType = properties.Rehydrate<string>(factory, nameof(DataType));
            IsRequired = properties.Rehydrate<bool>(factory, nameof(IsRequired));
            DefaultValue = properties.Rehydrate<string>(factory, nameof(DefaultValue));
            this.choices = properties.Rehydrate<List<string>>(factory, nameof(Choices));
        }

        public string DataType { get; private set; }

        public bool IsRequired { get; private set; }

        public string DefaultValue { get; private set; }

        public IReadOnlyList<string> Choices => this.choices;

        public PersistableProperties Dehydrate()
        {
            var properties = new PersistableProperties();
            properties.Dehydrate(nameof(Id), Id);
            properties.Dehydrate(nameof(Name), Name);
            properties.Dehydrate(nameof(DataType), DataType);
            properties.Dehydrate(nameof(IsRequired), IsRequired);
            properties.Dehydrate(nameof(DefaultValue), DefaultValue);
            properties.Dehydrate(nameof(Choices), Choices);

            return properties;
        }

        public static Attribute Rehydrate(PersistableProperties properties, IPersistableFactory factory)
        {
            return new Attribute(properties, factory);
        }

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

        public static object SetValue(string dataType, object value)
        {
            if (value.IsNull())
            {
                return null;
            }

            switch (dataType)
            {
                case "string":
                    return value.ToString();

                case "bool":
                    return Convert.ToBoolean(value);

                case "int":
                    return Convert.ToInt32(value);

                case "decimal":
                    return Convert.ToDecimal(value);

                case "DateTime":
                    return Convert.ToDateTime(value).ToUniversalTime();

                default:
                    throw new ArgumentOutOfRangeException(
                        ValidationMessages.Attribute_UnsupportedDataType.Format(dataType,
                            SupportedDataTypes.Join(", ")));
            }
        }

        public void SetDataType(string dataType)
        {
            dataType.GuardAgainstInvalid(Validations.IsSupportedAttributeDataType, nameof(dataType),
                ValidationMessages.Attribute_UnsupportedDataType, SupportedDataTypes.Join(", "));

            DataType = dataType;
        }

        public void SetName(string name)
        {
            name.GuardAgainstNullOrEmpty(nameof(name));
            name.GuardAgainstInvalid(Validations.IsNameIdentifier, nameof(name),
                ValidationMessages.InvalidNameIdentifier);

            Name = name;
        }

        public void SetRequired(bool isRequired)
        {
            IsRequired = isRequired;
        }

        public void SetDefaultValue(string defaultValue)
        {
            defaultValue.GuardAgainstInvalid(
                dv => Validations.IsValueOfDataType(dv, DataType),
                nameof(defaultValue),
                ValidationMessages.Attribute_InvalidDefaultValue, DataType);
            if (Choices.HasAny())
            {
                defaultValue.GuardAgainstInvalid(this.choices.Contains, nameof(defaultValue),
                    ValidationMessages.Attribute_DefaultValueIsNotAChoice, Choices.SafeJoin("; "));
            }

            DefaultValue = defaultValue;
        }

        // ReSharper disable once ParameterHidesMember
        public void SetChoices(List<string> choices)
        {
            choices.GuardAgainstNull(nameof(choices));
            choices.ForEach(choice =>
                choice.GuardAgainstInvalid(
                    _ => Validations.IsValueOfDataType(choice, DataType), nameof(choices),
                    ValidationMessages.Attribute_WrongDataTypeChoice.Format(choice, DataType)));

            this.choices = choices;
        }

        public string Id { get; }

        public string Name { get; private set; }

        public ValidationResults Validate(ValidationContext context, object value)
        {
            var results = ValidationResults.None;
            if (IsRequired)
            {
                if (value.IsNull())
                {
                    results.Add(new ValidationResult(context,
                        ValidationMessages.Attribute_ValidationRule_RequiredAttributeValue.Format(Name)));
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

        private static bool IsValidDataType(string dataType, object value)
        {
            if (value.IsNull())
            {
                return true;
            }

            switch (dataType)
            {
                case "string":
                    return value is string;

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
    }
}