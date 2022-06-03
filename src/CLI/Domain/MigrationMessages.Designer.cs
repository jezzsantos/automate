﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Automate.CLI.Domain {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class MigrationMessages {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal MigrationMessages() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Automate.CLI.Domain.MigrationMessages", typeof(MigrationMessages).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Failed, the updated toolkit (Name: &apos;{Name}&apos; v{Version}) contains at least one breaking change, and therefore cannot be automatically upgraded.
        /// </summary>
        internal static string DraftDefinition_Upgrade_BreakingChangeForbidden {
            get {
                return ResourceManager.GetString("DraftDefinition_Upgrade_BreakingChangeForbidden", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Upgrade was forcefully applied despite the toolkit (Name: &apos;{Name}&apos; v{Version}) indicating a breaking change. The integrity of the upgraded draft cannot be guaranteed, and may need to be updated.
        /// </summary>
        internal static string DraftDefinition_Upgrade_BreakingChangeForced {
            get {
                return ResourceManager.GetString("DraftDefinition_Upgrade_BreakingChangeForced", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Skipped, a newer version of the toolkit (Name: &apos;{Name}&apos; v{Version}) has not been installed yet.
        /// </summary>
        internal static string DraftDefinition_Upgrade_SameToolkitVersion {
            get {
                return ResourceManager.GetString("DraftDefinition_Upgrade_SameToolkitVersion", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Added attribute &apos;{Path}&apos; to draft, with value of &apos;{NewValue}&apos;.
        /// </summary>
        internal static string DraftItem_AttributeAdded {
            get {
                return ResourceManager.GetString("DraftItem_AttributeAdded", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Added Choices to attribute &apos;{Path}&apos;, and changed current value to &apos;{Value}&apos;.
        /// </summary>
        internal static string DraftItem_AttributeChoicesAdded {
            get {
                return ResourceManager.GetString("DraftItem_AttributeChoicesAdded", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Changed Choices of attribute &apos;{Path}&apos;, and changed current value from &apos;{Value}&apos; to &apos;{NewChoice}&apos;.
        /// </summary>
        internal static string DraftItem_AttributeChoicesChanged {
            get {
                return ResourceManager.GetString("DraftItem_AttributeChoicesChanged", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Deleted Choices from attribute &apos;{Path}&apos;.
        /// </summary>
        internal static string DraftItem_AttributeChoicesDeleted {
            get {
                return ResourceManager.GetString("DraftItem_AttributeChoicesDeleted", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Changed DataTye of attribute &apos;{Path}&apos; from &apos;{OldDataType}&apos; to &apos;{NewDataType}&apos;.
        /// </summary>
        internal static string DraftItem_AttributeDataTypeChanged {
            get {
                return ResourceManager.GetString("DraftItem_AttributeDataTypeChanged", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Changed DefaultValue of attribute &apos;{Path}&apos;, and changed the current default value from &apos;{Value}&apos; to &apos;{NewDefault}&apos;.
        /// </summary>
        internal static string DraftItem_AttributeDefaultValueChanged {
            get {
                return ResourceManager.GetString("DraftItem_AttributeDefaultValueChanged", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Deleted attribute &apos;{Path}&apos; from draft.
        /// </summary>
        internal static string DraftItem_AttributeDeleted {
            get {
                return ResourceManager.GetString("DraftItem_AttributeDeleted", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Changed Name of attribute &apos;{Path}&apos; from &apos;{OldName}&apos; to &apos;{NewName}&apos;.
        /// </summary>
        internal static string DraftItem_AttributeNameChanged {
            get {
                return ResourceManager.GetString("DraftItem_AttributeNameChanged", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Added element &apos;{Path}&apos; to draft.
        /// </summary>
        internal static string DraftItem_ElementAdded {
            get {
                return ResourceManager.GetString("DraftItem_ElementAdded", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Deleted element &apos;{Path}&apos; from draft.
        /// </summary>
        internal static string DraftItem_ElementDeleted {
            get {
                return ResourceManager.GetString("DraftItem_ElementDeleted", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Added new Code Template: &apos;{Name}&apos; (ID: &apos;{Id}&apos;) to draft.
        /// </summary>
        internal static string ToolkitDefinition_CodeTemplateFile_Added {
            get {
                return ResourceManager.GetString("ToolkitDefinition_CodeTemplateFile_Added", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Changed contents of Code Template: &apos;{Name}&apos; (ID: &apos;{Id}&apos;) in draft.
        /// </summary>
        internal static string ToolkitDefinition_CodeTemplateFile_ContentUpgraded {
            get {
                return ResourceManager.GetString("ToolkitDefinition_CodeTemplateFile_ContentUpgraded", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Deleted Code Template: &apos;{Name}&apos; (ID: &apos;{Id}&apos;) from draft.
        /// </summary>
        internal static string ToolkitDefinition_CodeTemplateFile_Deleted {
            get {
                return ResourceManager.GetString("ToolkitDefinition_CodeTemplateFile_Deleted", resourceCulture);
            }
        }
    }
}
