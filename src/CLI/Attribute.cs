using System.Collections.Generic;
using automate.Extensions;

namespace automate
{
    internal class Attribute : INamedEntity
    {
        public const string DefaultType = "string";
        public static readonly string[] SupportedTypes =
        {
            DefaultType, "boolean", "integer", "datetime"
        };

        public Attribute(string name, string type, bool isRequired, string defaultValue)
        {
            name.GuardAgainstNullOrEmpty(nameof(name));
            name.GuardAgainstInvalid(Validations.IsIdentifier, nameof(name),
                ExceptionMessages.Validations_InvalidIdentifier);
            if (type.HasValue())
            {
                type.GuardAgainstInvalid(Validations.IsSupportedAttributeType, nameof(type),
                    ExceptionMessages.Validations_UnsupportedAttributeType, SupportedTypes.Join(", "));
            }
            var resolvedType = type.HasValue()
                ? type
                : DefaultType;
            if (defaultValue.HasValue())
            {
                defaultValue.GuardAgainstInvalid(s => Validations.IsDefaultValueForType(s, resolvedType),
                    nameof(defaultValue),
                    ExceptionMessages.Validations_InvalidDefaultValue, resolvedType);
            }
            Id = IdGenerator.Create();
            Name = name;
            Type = resolvedType;
            IsRequired = isRequired;
            DefaultValue = defaultValue;
            Choices = new List<string>();
        }

        /// <summary>
        ///     For serialization
        /// </summary>
        public Attribute()
        {
        }

        public string Type { get; set; }

        public bool IsRequired { get; set; }

        public string DefaultValue { get; set; }

        public List<string> Choices { get; set; }

        public string Id { get; set; }

        public string Name { get; set; }
    }
}