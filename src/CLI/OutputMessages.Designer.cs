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
    internal class OutputMessages {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal OutputMessages() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("automate.OutputMessages", typeof(OutputMessages).Assembly);
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
        ///   Looks up a localized string similar to Added attribute &apos;{Name}&apos; to element {ParentId}.
        /// </summary>
        internal static string CommandLine_Output_AttributeAdded {
            get {
                return ResourceManager.GetString("CommandLine_Output_AttributeAdded", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Added code template command &apos;{Name}&apos; with ID: {CommandId}.
        /// </summary>
        internal static string CommandLine_Output_CodeTemplateCommandAdded {
            get {
                return ResourceManager.GetString("CommandLine_Output_CodeTemplateCommandAdded", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Added code template {Name} from: {FilePath}.
        /// </summary>
        internal static string CommandLine_Output_CodeTemplatedAdded {
            get {
                return ResourceManager.GetString("CommandLine_Output_CodeTemplatedAdded", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Added collection &apos;{Name}&apos; to element {ParentId}.
        /// </summary>
        internal static string CommandLine_Output_CollectionAdded {
            get {
                return ResourceManager.GetString("CommandLine_Output_CollectionAdded", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Added element &apos;{Name}&apos; to element {ParentId}.
        /// </summary>
        internal static string CommandLine_Output_ElementAdded {
            get {
                return ResourceManager.GetString("CommandLine_Output_ElementAdded", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Added launch point for commands with name: &apos;{Name}&apos;.
        /// </summary>
        internal static string CommandLine_Output_LaunchPointAdded {
            get {
                return ResourceManager.GetString("CommandLine_Output_LaunchPointAdded", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to There are no code templates.
        /// </summary>
        internal static string CommandLine_Output_NoCodeTemplates {
            get {
                return ResourceManager.GetString("CommandLine_Output_NoCodeTemplates", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to No Pattern is in use. You must run `automate create &lt;Name&gt;` first to create a pattern.
        /// </summary>
        internal static string CommandLine_Output_NoPatternSelected {
            get {
                return ResourceManager.GetString("CommandLine_Output_NoPatternSelected", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to No Toolkit is in use. You must run `automate install &lt;Path&gt;` first, to install a toolkit.
        /// </summary>
        internal static string CommandLine_Output_NoToolkitSelected {
            get {
                return ResourceManager.GetString("CommandLine_Output_NoToolkitSelected", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Pattern: {Name} was created with ID: {PatternId}.
        /// </summary>
        internal static string CommandLine_Output_PatternCreated {
            get {
                return ResourceManager.GetString("CommandLine_Output_PatternCreated", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Pattern: {0} ({1}) is in use.
        /// </summary>
        internal static string CommandLine_Output_PatternInUse {
            get {
                return ResourceManager.GetString("CommandLine_Output_PatternInUse", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Pattern: {Name} is the default with ID: {PatternId}.
        /// </summary>
        internal static string CommandLine_Output_PatternSwitched {
            get {
                return ResourceManager.GetString("CommandLine_Output_PatternSwitched", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Toolkit: {0} ({1}) is being used.
        /// </summary>
        internal static string CommandLine_Output_ToolkitInUse {
            get {
                return ResourceManager.GetString("CommandLine_Output_ToolkitInUse", resourceCulture);
            }
        }
    }
}
