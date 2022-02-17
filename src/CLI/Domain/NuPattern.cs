using System.Collections.Generic;

namespace Automate.CLI.Domain
{
    internal class ViewSchema : CustomizableElementSchema
    {
        public bool IsVisible { get; set; }

        public bool IsDefault { get; set; }

        public string Caption { get; set; }

        public PatternSchema Pattern { get; set; }

        public List<AbstractElementSchema> Elements { get; set; }

        public List<ExtensionPointSchema> ExtensionPoints { get; set; }
    }

    internal class PatternModelSchema
    {
        public string BaseId { get; set; }

        public string BaseVersion { get; set; }

        public PatternSchema Pattern { get; set; }
    }

    internal class PatternSchema : PatternElementSchema
    {
        /// <summary>
        ///     The ID of the toolkit
        /// </summary>
        public string ExtensionId { get; set; }

        public List<ViewSchema> Views { get; set; }

        public string PatternLink { get; set; }

        public PatternModelSchema PatternModel { get; set; }

        public List<ExtensionPointSchema> ProvidedExtensionPoints { get; set; }
    }

    internal class PatternElementSchema : CustomizableElementSchema
    {
        public string ValidationRules { get; set; }

        public string Icon { get; set; }

        public List<PropertySchema> Properties { get; set; }

        public List<AutomationSettingsSchema> AutomationSettings { get; set; }
    }

    internal class NamedElementSchema
    {
        public string Name { get; set; }

        public string BaseId { get; set; }

        public string DisplayName { get; set; }

        public bool IsDisplayNameTracking { get; set; }

        public string Description { get; set; }

        public bool IsInheritedFromBase { get; set; }

        public string SchemaPath { get; set; }

        public bool IsSystem { get; set; }

        public string CodeIdentifier { get; set; }

        public bool IsCodeIdentifierTracking { get; set; }
    }

    internal class PropertySchema : CustomizableElementSchema
    {
        public string RawDefaultValue { get; set; }

        public string Type { get; set; }

        public bool IsVisible { get; set; }

        public bool IsReadOnly { get; set; }

        public string Category { get; set; }

        public PropertyUsages PropertyUsage { get; set; }

        public string TypeConverterTypeName { get; set; }

        public string EditorTypeName { get; set; }

        public string RawValidationRules { get; set; }

        public string RawValueProvider { get; set; }

        public PatternElementSchema Owner { get; set; }

        public string Icon { get; set; }
    }

    internal class AbstractElementSchema : PatternElementSchema
    {
        public bool IsVisible { get; set; }

        public ViewSchema View { get; set; }

        public List<AbstractElementSchema> Elements { get; set; }

        public AbstractElementSchema Owner { get; set; }

        public List<ExtensionPointSchema> ExtensionPoints { get; set; }
    }

    internal class CollectionSchema : AbstractElementSchema
    {
    }

    internal class ElementSchema : AbstractElementSchema
    {
    }

    internal class CustomizableElementSchema : NamedElementSchema
    {
        public CustomizationState IsCustomizable { get; set; }

        public bool IsCustomizationEnabled { get; set; }

        public bool IsCustomizationPolicyModifiable { get; set; }

        public CustomizationEnabledState IsCustomizationEnabledState { get; set; }

        public CustomizationPolicySchema Policy { get; set; }
    }

    internal class CustomizationPolicySchema
    {
        public bool IsModified { get; set; }

        public CustomizedLevel CustomizationLevel { get; set; }

        public string Name { get; set; }

        public List<CustomizableSettingSchema> Settings { get; set; }

        public CustomizableElementSchema Owner { get; set; }
    }

    internal class CustomizableSettingSchema
    {
        public bool IsEnabled { get; set; }

        public string Caption { get; set; }

        public string CaptionFormatter { get; set; }

        public bool IsModified { get; set; }

        public bool DefaultValue { get; set; }

        public bool Value { get; set; }

        public string PropertyId { get; set; }

        public string DescriptionFormatter { get; set; }

        public string Description { get; set; }

        public CustomizableDomainElementSettingType DomainElementSettingType { get; set; }

        public CustomizationPolicySchema Policy { get; set; }
    }

    internal class AutomationSettingsSchema : CustomizableElementSchema
    {
        public string AutomationType { get; set; }

        public string Settings { get; set; }

        public AutomationSettingsClassification Classification { get; set; }

        public PatternElementSchema Owner { get; set; }
    }

    internal class ExtensionPointSchema : PatternElementSchema
    {
        public string RequiredExtensionPointId { get; set; }

        public string Conditions { get; set; }

        public string RepresentedExtensionPointId { get; set; }

        public AbstractElementSchema Owner { get; set; }

        public ViewSchema View { get; set; }
    }

    internal enum AutomationSettingsClassification
    {
        /// <summary>
        ///     General classification.
        /// </summary>
        General = 0,

        /// <summary>
        ///     Applies to launch point.
        /// </summary>
        LaunchPoint = 1
    }

    internal enum CustomizableDomainElementSettingType
    {
        /// <summary>
        ///     The domain property is customizable as a whole.
        /// </summary>
        DomainProperty,
        /// <summary>
        ///     The domain role, and all its child elements are customizable as a whole.
        /// </summary>
        DomainRole
    }

    internal enum CustomizedLevel
    {
        /// <summary>
        ///     Nothing is customized.
        /// </summary>
        None,

        /// <summary>
        ///     Everything is customized.
        /// </summary>
        All,

        /// <summary>
        ///     Partially customized.
        /// </summary>
        Partially
    }

    internal enum CustomizationEnabledState
    {
        FalseEnabled,
        FalseDisabled,
        InheritedEnabled,
        TrueDisabled,
        TrueEnabled,
        InheritedDisabled
    }

    internal enum CustomizationState
    {
        /// <summary>
        ///     Customization is determined by the value of the item which this item is related to.
        /// </summary>
        Inherited,

        /// <summary>
        ///     Cannot be customized.
        /// </summary>
        False,

        /// <summary>
        ///     Can be customized.
        /// </summary>
        True
    }

    internal enum PropertyUsages
    {
        /// <summary>
        ///     The property is a regular element property.
        /// </summary>
        General = 0,

        /// <summary>
        ///     The property is imported from an extension point contract.
        /// </summary>
        ExtensionContract = 1
    }

    internal enum Cardinality
    {
        /// <summary>
        ///     OneToOne instance of this item.
        /// </summary>
        OneToOne = 0,

        /// <summary>
        ///     ZeroToOne instance of this item.
        /// </summary>
        ZeroToOne = 1,

        /// <summary>
        ///     OneToMany instance of this item.
        /// </summary>
        OneToMany = 2,

        /// <summary>
        ///     ZeroToMany instances of this item.
        /// </summary>
        ZeroToMany = 3
    }
}