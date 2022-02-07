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
    internal class ExceptionMessages {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal ExceptionMessages() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("automate.ExceptionMessages", typeof(ExceptionMessages).Assembly);
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
        ///   Looks up a localized string similar to The pattern meta-model could not be found for {0}.
        /// </summary>
        internal static string JsonFilePatternRepository_NotFound {
            get {
                return ResourceManager.GetString("JsonFilePatternRepository_NotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The pattern meta-model could not be found for {0}.
        /// </summary>
        internal static string MemoryRepository_NotFound {
            get {
                return ResourceManager.GetString("MemoryRepository_NotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to An attribute with name {0} already exists. The name must be unique.
        /// </summary>
        internal static string PatternApplication_AttributeByNameExists {
            get {
                return ResourceManager.GetString("PatternApplication_AttributeByNameExists", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The given &apos;defaultvalue&apos; must be one of the choices given in the &apos;isoneof&apos;..
        /// </summary>
        internal static string PatternApplication_AttributeDefaultValueIsNotAChoice {
            get {
                return ResourceManager.GetString("PatternApplication_AttributeDefaultValueIsNotAChoice", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Automation with name {0} already exists. The name must be unique.
        /// </summary>
        internal static string PatternApplication_AutomationByNameExists {
            get {
                return ResourceManager.GetString("PatternApplication_AutomationByNameExists", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The code file {1} could not be found at location {0}.
        /// </summary>
        internal static string PatternApplication_CodeTemplate_NotFoundAtLocation {
            get {
                return ResourceManager.GetString("PatternApplication_CodeTemplate_NotFoundAtLocation", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to A code template with name {0} already exists. The name must be unique.
        /// </summary>
        internal static string PatternApplication_CodeTemplateByNameExists {
            get {
                return ResourceManager.GetString("PatternApplication_CodeTemplateByNameExists", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to A collection with name {0} already exists. The name must be unique.
        /// </summary>
        internal static string PatternApplication_CollectionByNameExists {
            get {
                return ResourceManager.GetString("PatternApplication_CollectionByNameExists", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to An element with name {0} already exists. The name must be unique.
        /// </summary>
        internal static string PatternApplication_ElementByNameExists {
            get {
                return ResourceManager.GetString("PatternApplication_ElementByNameExists", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Pattern does not yet exist.
        /// </summary>
        internal static string PatternApplication_NoCurrentPattern {
            get {
                return ResourceManager.GetString("PatternApplication_NoCurrentPattern", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The expression: &apos;{0}&apos; does not resolve to an element in this pattern.
        /// </summary>
        internal static string PatternApplication_NodeExpressionNotFound {
            get {
                return ResourceManager.GetString("PatternApplication_NodeExpressionNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The expression {0} is not a valid pattern expression.
        /// </summary>
        internal static string PatternPathResolver_InvalidExpression {
            get {
                return ResourceManager.GetString("PatternPathResolver_InvalidExpression", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to A pattern with the name {0} already exists.
        /// </summary>
        internal static string PatternStore_FoundNamed {
            get {
                return ResourceManager.GetString("PatternStore_FoundNamed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to A pattern with id {0} cannot be found in the pattern store at location {1}.
        /// </summary>
        internal static string PatternStore_NotFoundAtLocationWithId {
            get {
                return ResourceManager.GetString("PatternStore_NotFoundAtLocationWithId", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Requested version number: {0}, is not a valid 2-dot version number.
        /// </summary>
        internal static string PatternToolkitPackager_InvalidVersionInstruction {
            get {
                return ResourceManager.GetString("PatternToolkitPackager_InvalidVersionInstruction", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Requested version number: {0}, cannot be before the current toolkit version number: {1}.
        /// </summary>
        internal static string PatternToolkitPackager_VersionBeforeCurrent {
            get {
                return ResourceManager.GetString("PatternToolkitPackager_VersionBeforeCurrent", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The file does not exist at path: {0}.
        /// </summary>
        internal static string SystemIoFile_SourceFileNotExist {
            get {
                return ResourceManager.GetString("SystemIoFile_SourceFileNotExist", resourceCulture);
            }
        }
    }
}
