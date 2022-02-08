using System.Collections.Generic;
using automate.Extensions;

namespace automate.Domain
{
    internal class Attribute : INamedEntity
    {
        public const string DefaultType = "string";
        public static readonly string[] SupportedDataTypes =
        {
            DefaultType, "bool", "int", "DateTime"
        };

        public Attribute(string name, string dataType, bool isRequired, string defaultValue)
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
                    s => Validations.IsDefaultValueForAttributeDataType(s, resolvedDataType),
                    nameof(defaultValue),
                    ValidationMessages.Attribute_InvalidDefaultValue, resolvedDataType);
            }
            Id = IdGenerator.Create();
            Name = name;
            DataType = resolvedDataType;
            IsRequired = isRequired;
            DefaultValue = defaultValue;
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

        public List<string> Choices { get; set; } = new List<string>();

        public string Id { get; set; }

        public string Name { get; set; }
    }
}