﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Automate.CLI {
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
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Automate.CLI.ExceptionMessages", typeof(ExceptionMessages).Assembly);
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
        ///   Looks up a localized string similar to Unknown type of automation: {0}.
        /// </summary>
        internal static string AutomationService_UnknownAutomationType {
            get {
                return ResourceManager.GetString("AutomationService_UnknownAutomationType", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Command is misconfigured. A CodeTemplate with ID: &apos;{0}&apos; could not be found on any element in this draft.
        /// </summary>
        internal static string CodeTemplateCommand_TemplateNotExists {
            get {
                return ResourceManager.GetString("CodeTemplateCommand_TemplateNotExists", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to LaunchPoint is misconfigured. A CodeTemplateCommand with ID: &apos;{0}&apos; could not be found on any element in this draft.
        /// </summary>
        internal static string CommandLaunchPoint_CommandIdNotFound {
            get {
                return ResourceManager.GetString("CommandLaunchPoint_CommandIdNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The property assignment &apos;{0}&apos; does not have a name.
        /// </summary>
        internal static string CommandLineApiExtensions_SplitPropertyAssignment_ValueWithoutName {
            get {
                return ResourceManager.GetString("CommandLineApiExtensions_SplitPropertyAssignment_ValueWithoutName", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The draft definition could not be found for: &apos;{0}&apos;.
        /// </summary>
        internal static string JsonFileRepository_DraftNotFound {
            get {
                return ResourceManager.GetString("JsonFileRepository_DraftNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The pattern definition could not be found for: &apos;{0}&apos;.
        /// </summary>
        internal static string JsonFileRepository_PatternNotFound {
            get {
                return ResourceManager.GetString("JsonFileRepository_PatternNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The toolkit could not be found for: &apos;{0}&apos;.
        /// </summary>
        internal static string JsonFileRepository_ToolkitNotFound {
            get {
                return ResourceManager.GetString("JsonFileRepository_ToolkitNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The file does not exist at path: &apos;{0}&apos;.
        /// </summary>
        internal static string SystemIoFile_SourceFileNotExist {
            get {
                return ResourceManager.GetString("SystemIoFile_SourceFileNotExist", resourceCulture);
            }
        }
    }
}
