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
    internal class DomainMessages {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal DomainMessages() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Automate.CLI.Domain.DomainMessages", typeof(DomainMessages).Assembly);
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
        ///   Looks up a localized string similar to CodeTemplate ({0}) template.
        /// </summary>
        internal static string AuthoringApplication_TestCodeTemplate_Description {
            get {
                return ResourceManager.GetString("AuthoringApplication_TestCodeTemplate_Description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to CodeTemplateCommand ({0}) path expression.
        /// </summary>
        internal static string CodeTemplateCommand_FilePathExpression_Description {
            get {
                return ResourceManager.GetString("CodeTemplateCommand_FilePathExpression_Description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Generated file: &apos;{0}&apos; to: &apos;{1}&apos;.
        /// </summary>
        internal static string CodeTemplateCommand_Log_GeneratedFile {
            get {
                return ResourceManager.GetString("CodeTemplateCommand_Log_GeneratedFile", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Updated link to file: &apos;{0}&apos; at: &apos;{1}&apos;.
        /// </summary>
        internal static string CodeTemplateCommand_Log_UpdatedLink {
            get {
                return ResourceManager.GetString("CodeTemplateCommand_Log_UpdatedLink", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to CodeTemplate ({0}) template.
        /// </summary>
        internal static string CodeTemplateCommand_TemplateContent_Description {
            get {
                return ResourceManager.GetString("CodeTemplateCommand_TemplateContent_Description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Command: &apos;{0}&apos; failed to execute. Errors were:
        ///{1}.
        /// </summary>
        internal static string CommandLaunchPoint_CommandIdFailedExecution {
            get {
                return ResourceManager.GetString("CommandLaunchPoint_CommandIdFailedExecution", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Attribute (ID: {0}) was added to element &apos;{1}&apos;.
        /// </summary>
        internal static string PatternElement_VersionChange_Attribute_Add {
            get {
                return ResourceManager.GetString("PatternElement_VersionChange_Attribute_Add", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Automation (ID: {0}) was added to element &apos;{1}&apos;.
        /// </summary>
        internal static string PatternElement_VersionChange_Automation_Add {
            get {
                return ResourceManager.GetString("PatternElement_VersionChange_Automation_Add", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to CodeTemplate (ID: {0}) was added to element &apos;{1}&apos;.
        /// </summary>
        internal static string PatternElement_VersionChange_CodeTemplate_Add {
            get {
                return ResourceManager.GetString("PatternElement_VersionChange_CodeTemplate_Add", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Element/Collection (ID: {0}) was added to element &apos;{1}&apos;.
        /// </summary>
        internal static string PatternElement_VersionChange_Element_Add {
            get {
                return ResourceManager.GetString("PatternElement_VersionChange_Element_Add", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Requested version number: &apos;{0}&apos; should represent a change due to non-breaking changes (that have been auto-detected in this toolkit):
        ///&apos;{1}&apos;.
        /// </summary>
        internal static string ToolkitVersion_Warning {
            get {
                return ResourceManager.GetString("ToolkitVersion_Warning", resourceCulture);
            }
        }
    }
}
