using System;
using System.Collections.Generic;
using System.Linq;
using Automate.Common.Domain;
using Automate.Common.Extensions;
using Automate.Runtime.Domain;

namespace Automate.Authoring.Domain
{
    public class Attribute : INamedEntity, IValidateable, IPersistable, IPatternVisitable
    {
        public const string DefaultType = "string";
        public static readonly string[] SupportedDataTypes =
        {
            DefaultType, "bool", "int", "float", "datetime"
        };
        public static readonly string[] ReservedAttributeNames =
        {
            nameof(INamedEntity.Id), nameof(Element.DisplayName), nameof(Element.Description),
            nameof(DraftItem.ConfigurePath), nameof(DraftItem.Schema), nameof(DraftItem.Items)
        };
        private List<string> choices;

        public Attribute(string name, string dataType = DefaultType, bool isRequired = false,
            string defaultValue = null, List<string> choices = null)
        {
            name.GuardAgainstNullOrEmpty(nameof(name));
            name.GuardAgainstInvalid(Validations.IsNameIdentifier, nameof(name),
                ValidationMessages.InvalidNameIdentifier);
            name.GuardAgainstInvalid(x => Validations.IsNotReservedName(x, ReservedAttributeNames), nameof(name),
                ValidationMessages.Attribute_ReservedName);
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
                        ValidationMessages.Attribute_WrongDataTypeChoice.Substitute(choice, dataType)));
            }

            Id = IdGenerator.Create();
            Parent = null;
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

        private PatternElement Parent { get; set; }

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

        public static Attribute Rehydrate(PersistableProperties properties,
            IPersistableFactory factory)
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

                case "float":
                    return double.TryParse(value, out var _);

                case "datetime":
                    return DateTime.TryParse(value, out var _);

                default:
                    throw new ArgumentOutOfRangeException(
                        ValidationMessages.Attribute_UnsupportedDataType.Substitute(dataType,
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

                case "float":
                    return Convert.ToDouble(value);

                case "datetime":
                    return Convert.ToDateTime(value).ToUniversalTime();

                default:
                    throw new ArgumentOutOfRangeException(
                        ValidationMessages.Attribute_UnsupportedDataType.Substitute(dataType,
                            SupportedDataTypes.Join(", ")));
            }
        }

        public void Rename(string name)
        {
            name.GuardAgainstNullOrEmpty(nameof(name));
            name.GuardAgainstInvalid(Validations.IsNameIdentifier, nameof(name),
                ValidationMessages.InvalidNameIdentifier);
            name.GuardAgainstInvalid(x => Validations.IsNotReservedName(x, ReservedAttributeNames), nameof(name),
                ValidationMessages.Attribute_ReservedName);

            if (name.NotEqualsOrdinal(Name))
            {
                Name = name;
                Parent.RecordChange(VersionChange.Breaking, VersionChanges.Attribute_Update_Name, Id,
                    Parent.Id);
            }
        }

        public void SetRequired(bool isRequired)
        {
            if (isRequired != IsRequired)
            {
                IsRequired = isRequired;
                Parent.RecordChange(VersionChange.NonBreaking, VersionChanges.Attribute_Update_Required,
                    Id,
                    Parent.Id);
            }
        }

        public void ResetDataType(string dataType)
        {
            dataType.GuardAgainstInvalid(Validations.IsSupportedAttributeDataType, nameof(dataType),
                ValidationMessages.Attribute_UnsupportedDataType, SupportedDataTypes.Join(", "));

            if (dataType.NotEqualsOrdinal(DataType))
            {
                DataType = dataType;
                Parent.RecordChange(VersionChange.Breaking, VersionChanges.Attribute_Update_DataType, Id,
                    Parent.Id);
            }
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

            if (defaultValue.NotEqualsOrdinal(DefaultValue))
            {
                DefaultValue = defaultValue;
                Parent.RecordChange(VersionChange.NonBreaking,
                    VersionChanges.Attribute_Update_DefaultValue,
                    Id, Parent.Id);
            }
        }

        // ReSharper disable once ParameterHidesMember
        public void SetChoices(List<string> choices)
        {
            choices.GuardAgainstNull(nameof(choices));
            choices.ForEach(choice =>
                choice.GuardAgainstInvalid(
                    _ => Validations.IsValueOfDataType(choice, DataType), nameof(choices),
                    ValidationMessages.Attribute_WrongDataTypeChoice.Substitute(choice, DataType)));

            if (!choices.SequenceEqual(Choices))
            {
                var change = Choices.HasNone()
                    ? VersionChange.NonBreaking
                    : VersionChange.Breaking;

                this.choices = choices;
                Parent.RecordChange(change, VersionChanges.Attribute_Update_Choices, Id, Parent.Id);
            }
        }

        public void SetParent(PatternElement parent)
        {
            Parent = parent;
        }

        public string Id { get; }

        public string Name { get; private set; }

        public bool Accept(IPatternVisitor visitor)
        {
            return visitor.VisitAttribute(this);
        }

        public ValidationResults Validate(ValidationContext context, object value)
        {
            var results = ValidationResults.None;
            if (IsRequired)
            {
                if (value.IsNull())
                {
                    results.Add(new ValidationResult(context,
                        ValidationMessages.Attribute_ValidationRule_RequiredAttributeValue.Substitute(Name)));
                }
            }

            if (!IsValidDataType(DataType, value))
            {
                results.Add(
                    new ValidationResult(context,
                        ValidationMessages.Attribute_ValidationRule_WrongDataTypeValue.Substitute(value, DataType)));
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
                    return value is int or long;

                case "float":
                    return value is double;

                case "datetime":
                    return value is DateTime;

                default:
                    throw new ArgumentOutOfRangeException(
                        ValidationMessages.Attribute_UnsupportedDataType.Substitute(dataType,
                            SupportedDataTypes.Join(", ")));
            }
        }
    }
}