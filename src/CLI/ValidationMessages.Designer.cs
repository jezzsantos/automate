﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace automate {
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
    internal class ValidationMessages {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal ValidationMessages() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("automate.ValidationMessages", typeof(ValidationMessages).Assembly);
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
        ///   Looks up a localized string similar to The default value &apos;{0}&apos; is not a valid default for an attribute of dataType &apos;{1}&apos;.
        /// </summary>
        internal static string Attribute_InvalidDefaultValue {
            get {
                return ResourceManager.GetString("Attribute_InvalidDefaultValue", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unsupported attribute dataType: &apos;{0}&apos;. Must be one of these values: {1}.
        /// </summary>
        internal static string Attribute_UnsupportedDataType {
            get {
                return ResourceManager.GetString("Attribute_UnsupportedDataType", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The command identifiers must include at least one identifier.
        /// </summary>
        internal static string Automation_EmptyCommandIds {
            get {
                return ResourceManager.GetString("Automation_EmptyCommandIds", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The command identifiers &apos;{0}&apos; contain at least one invalid value.
        /// </summary>
        internal static string Automation_InvalidCommandIds {
            get {
                return ResourceManager.GetString("Automation_InvalidCommandIds", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The file path &apos;{0}&apos; is not a valid file path for automation.
        /// </summary>
        internal static string Automation_InvalidFilePath {
            get {
                return ResourceManager.GetString("Automation_InvalidFilePath", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The value &apos;{0}&apos; is not a valid identifier.
        /// </summary>
        internal static string InvalidNameIdentifier {
            get {
                return ResourceManager.GetString("InvalidNameIdentifier", resourceCulture);
            }
        }
    }
}