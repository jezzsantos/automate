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
        ///   Looks up a localized string similar to &apos;The code file: &apos;{1}&apos; could not be found at location: &apos;{0}&apos;.
        /// </summary>
        internal static string AuthoringApplication_CodeTemplate_NotFoundAtLocation {
            get {
                return ResourceManager.GetString("AuthoringApplication_CodeTemplate_NotFoundAtLocation", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to A CodeTemplate with name: &apos;{0}&apos; does not exist on element referenced by: &apos;{1}&apos;.
        /// </summary>
        internal static string AuthoringApplication_CodeTemplateNotExistsElement {
            get {
                return ResourceManager.GetString("AuthoringApplication_CodeTemplateNotExistsElement", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to A CodeTemplate with name: &apos;{0}&apos; does not exist on the pattern.
        /// </summary>
        internal static string AuthoringApplication_CodeTemplateNotExistsRoot {
            get {
                return ResourceManager.GetString("AuthoringApplication_CodeTemplateNotExistsRoot", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to A CodeTemplate with ID: &apos;{0}&apos; cannot be found on the test solution.
        /// </summary>
        internal static string AuthoringApplication_CodeTemplateNotExistsTestSolution {
            get {
                return ResourceManager.GetString("AuthoringApplication_CodeTemplateNotExistsTestSolution", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Pattern does not yet exist.
        /// </summary>
        internal static string AuthoringApplication_NoCurrentPattern {
            get {
                return ResourceManager.GetString("AuthoringApplication_NoCurrentPattern", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The expression: &apos;{0}&apos; does not resolve to an element in this pattern.
        /// </summary>
        internal static string AuthoringApplication_PathExpressionNotFound {
            get {
                return ResourceManager.GetString("AuthoringApplication_PathExpressionNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The exported test data could not be written to: &apos;{0}, error was: &apos;{1}&apos;.
        /// </summary>
        internal static string AuthoringApplication_TestDataExport_NotValidFile {
            get {
                return ResourceManager.GetString("AuthoringApplication_TestDataExport_NotValidFile", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The imported test data could not be found at location: &apos;{0}&apos;.
        /// </summary>
        internal static string AuthoringApplication_TestDataImport_NotFoundAtLocation {
            get {
                return ResourceManager.GetString("AuthoringApplication_TestDataImport_NotFoundAtLocation", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The imported test data is not valid JSON.
        /// </summary>
        internal static string AuthoringApplication_TestDataImport_NotValidJson {
            get {
                return ResourceManager.GetString("AuthoringApplication_TestDataImport_NotValidJson", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The code template with ID: &apos;{0}&apos; does not exist in the toolkit.
        /// </summary>
        internal static string CodeTemplateCommand_TemplateNotExists {
            get {
                return ResourceManager.GetString("CodeTemplateCommand_TemplateNotExists", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The command with ID: &apos;{0}&apos; could not be found on any element in this solution.
        /// </summary>
        internal static string CommandLaunchPoint_CommandIdNotFound {
            get {
                return ResourceManager.GetString("CommandLaunchPoint_CommandIdNotFound", resourceCulture);
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
        ///   Looks up a localized string similar to The solution definition could not be found for: &apos;{0}&apos;.
        /// </summary>
        internal static string JsonFileRepository_SolutionNotFound {
            get {
                return ResourceManager.GetString("JsonFileRepository_SolutionNotFound", resourceCulture);
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
        ///   Looks up a localized string similar to The pattern definition could not be found for: &apos;{0}&apos;.
        /// </summary>
        internal static string MemoryRepository_NotFound {
            get {
                return ResourceManager.GetString("MemoryRepository_NotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to An attribute with name: &apos;{0}&apos; already exists. The name must be unique.
        /// </summary>
        internal static string PatternElement_AttributeByNameExists {
            get {
                return ResourceManager.GetString("PatternElement_AttributeByNameExists", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to An element/collection with name: &apos;{0}&apos; already exists. The name cannot be the same as any element/collection.
        /// </summary>
        internal static string PatternElement_AttributeByNameExistsAsElement {
            get {
                return ResourceManager.GetString("PatternElement_AttributeByNameExistsAsElement", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The attribute with name: &apos;{0}&apos; does not exist.
        /// </summary>
        internal static string PatternElement_AttributeByNameNotExists {
            get {
                return ResourceManager.GetString("PatternElement_AttributeByNameNotExists", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The name: &apos;{0}&apos; is a reserved name, and cannot be used as a name for an attribute.
        /// </summary>
        internal static string PatternElement_AttributeNameReserved {
            get {
                return ResourceManager.GetString("PatternElement_AttributeNameReserved", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Automation with name: &apos;{0}&apos; already exists. The name must be unique.
        /// </summary>
        internal static string PatternElement_AutomationByNameExists {
            get {
                return ResourceManager.GetString("PatternElement_AutomationByNameExists", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to No commands were added to the launch point.
        /// </summary>
        internal static string PatternElement_AutomationNotExistsByName {
            get {
                return ResourceManager.GetString("PatternElement_AutomationNotExistsByName", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to A code template with name: &apos;{0}&apos; already exists. The name must be unique.
        /// </summary>
        internal static string PatternElement_CodeTemplateByNameExists {
            get {
                return ResourceManager.GetString("PatternElement_CodeTemplateByNameExists", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to A CodeTemplate with name: &apos;{0}&apos; was not found.
        /// </summary>
        internal static string PatternElement_CodeTemplateNoFound {
            get {
                return ResourceManager.GetString("PatternElement_CodeTemplateNoFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to A Command with ID: &apos;{0}&apos; does not exist anywhere on this pattern.
        /// </summary>
        internal static string PatternElement_CommandIdNotFound {
            get {
                return ResourceManager.GetString("PatternElement_CommandIdNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to An element with name: &apos;{0}&apos; already exists. The name must be unique.
        /// </summary>
        internal static string PatternElement_ElementByNameExists {
            get {
                return ResourceManager.GetString("PatternElement_ElementByNameExists", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to An attribute with name: &apos;{0}&apos; already exists. The name cannot be the same as any attribute.
        /// </summary>
        internal static string PatternElement_ElementByNameExistsAsAttribute {
            get {
                return ResourceManager.GetString("PatternElement_ElementByNameExistsAsAttribute", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The element with name: &apos;{0}&apos; does not exist.
        /// </summary>
        internal static string PatternElement_ElementByNameNotExists {
            get {
                return ResourceManager.GetString("PatternElement_ElementByNameNotExists", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to No commands were added to the launch point.
        /// </summary>
        internal static string PatternElement_NoCommandIds {
            get {
                return ResourceManager.GetString("PatternElement_NoCommandIds", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The expression: &apos;{0}&apos; is not a valid pattern expression.
        /// </summary>
        internal static string PatternPathResolver_InvalidExpression {
            get {
                return ResourceManager.GetString("PatternPathResolver_InvalidExpression", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to A pattern with the same name: &apos;{0}&apos; already exists.
        /// </summary>
        internal static string PatternStore_FoundNamed {
            get {
                return ResourceManager.GetString("PatternStore_FoundNamed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to A pattern with id: &apos;{0}&apos; cannot be found in the pattern store at location: &apos;{1}&apos;.
        /// </summary>
        internal static string PatternStore_NotFoundAtLocationWithId {
            get {
                return ResourceManager.GetString("PatternStore_NotFoundAtLocationWithId", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The installer file at: &apos;{0}&apos;, contains an invalid toolkit definition.
        /// </summary>
        internal static string PatternToolkitPackager_InvalidInstallerFile {
            get {
                return ResourceManager.GetString("PatternToolkitPackager_InvalidInstallerFile", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to An attempt to deserialize empty JSON.
        /// </summary>
        internal static string PersistableExtensions_FromJson_NoJson {
            get {
                return ResourceManager.GetString("PersistableExtensions_FromJson_NoJson", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {0} is a not a supported generic Dictionary type.
        /// </summary>
        internal static string PersistableExtensions_GenericDictionaryNoParameters {
            get {
                return ResourceManager.GetString("PersistableExtensions_GenericDictionaryNoParameters", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to is not a generic type.
        /// </summary>
        internal static string PersistableExtensions_GenericDictionaryNotGeneric {
            get {
                return ResourceManager.GetString("PersistableExtensions_GenericDictionaryNotGeneric", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {0} is a not a supported generic Dictionary with a string key.
        /// </summary>
        internal static string PersistableExtensions_GenericDictionaryNotStringKey {
            get {
                return ResourceManager.GetString("PersistableExtensions_GenericDictionaryNotStringKey", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {0} is a not a supported generic Dictionary type.
        /// </summary>
        internal static string PersistableExtensions_GenericDictionaryTooManyParameters {
            get {
                return ResourceManager.GetString("PersistableExtensions_GenericDictionaryTooManyParameters", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {0} is a not a supported generic List type.
        /// </summary>
        internal static string PersistableExtensions_GenericListNoParameters {
            get {
                return ResourceManager.GetString("PersistableExtensions_GenericListNoParameters", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to is not a generic type.
        /// </summary>
        internal static string PersistableExtensions_GenericListNotGeneric {
            get {
                return ResourceManager.GetString("PersistableExtensions_GenericListNotGeneric", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {0} is a not a supported generic List type.
        /// </summary>
        internal static string PersistableExtensions_GenericListTooManyParameters {
            get {
                return ResourceManager.GetString("PersistableExtensions_GenericListTooManyParameters", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Tried to configure solution ID: &apos;{0}&apos; to add both a new element with Name: &apos;{1}&apos;, and to a new collection with Name: &apos;{2}&apos;. You can only do one or the other.
        /// </summary>
        internal static string RuntimeApplication_ConfigureSolution_AddAndAddTo {
            get {
                return ResourceManager.GetString("RuntimeApplication_ConfigureSolution_AddAndAddTo", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The element (referenced by: &apos;{0}&apos;) already exists, and cannot be added again.
        /// </summary>
        internal static string RuntimeApplication_ConfigureSolution_AddElementExists {
            get {
                return ResourceManager.GetString("RuntimeApplication_ConfigureSolution_AddElementExists", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Tried to configure solution ID: &apos;{0}&apos; without any changes to any elements or collections.
        /// </summary>
        internal static string RuntimeApplication_ConfigureSolution_NoChanges {
            get {
                return ResourceManager.GetString("RuntimeApplication_ConfigureSolution_NoChanges", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Tried to configure solution ID: &apos;{0}&apos; to update the element &apos;{1}&apos;, and add a new element with Name: &apos;{2}&apos;. You can only do one or the other.
        /// </summary>
        internal static string RuntimeApplication_ConfigureSolution_OnAndAdd {
            get {
                return ResourceManager.GetString("RuntimeApplication_ConfigureSolution_OnAndAdd", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Tried to configure solution ID: &apos;{0}&apos; to update the element &apos;{1}&apos;, and add to a collection with Name: &apos;{2}&apos;. You can only do one or the other.
        /// </summary>
        internal static string RuntimeApplication_ConfigureSolution_OnAndAddTo {
            get {
                return ResourceManager.GetString("RuntimeApplication_ConfigureSolution_OnAndAddTo", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The element (referenced by: &apos;{0}&apos;) has not been created yet, and cannot be configured.
        /// </summary>
        internal static string RuntimeApplication_ConfigureSolution_OnElementNotExists {
            get {
                return ResourceManager.GetString("RuntimeApplication_ConfigureSolution_OnElementNotExists", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The: &apos;{0}&apos; pattern does not have an element referenced by: &apos;{1}&apos;.
        /// </summary>
        internal static string RuntimeApplication_ItemExpressionNotFound {
            get {
                return ResourceManager.GetString("RuntimeApplication_ItemExpressionNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Solution does not yet exist.
        /// </summary>
        internal static string RuntimeApplication_NoCurrentSolution {
            get {
                return ResourceManager.GetString("RuntimeApplication_NoCurrentSolution", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to No solution with ID: &apos;{0}&apos; has been created..
        /// </summary>
        internal static string RuntimeApplication_SolutionNotFound {
            get {
                return ResourceManager.GetString("RuntimeApplication_SolutionNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The installer could not be found at: &apos;{0}&apos;.
        /// </summary>
        internal static string RuntimeApplication_ToolkitInstallerNotFound {
            get {
                return ResourceManager.GetString("RuntimeApplication_ToolkitInstallerNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to No toolkit with Name: &apos;{0}&apos; is installed.
        /// </summary>
        internal static string RuntimeApplication_ToolkitNotFound {
            get {
                return ResourceManager.GetString("RuntimeApplication_ToolkitNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Cannot fetch configuration for a non-element.
        /// </summary>
        internal static string SolutionItem_ConfigurationForNonElement {
            get {
                return ResourceManager.GetString("SolutionItem_ConfigurationForNonElement", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The element (referenced by: &apos;{0}&apos;) does not have a property called: &apos;{1}&apos;.
        /// </summary>
        internal static string SolutionItem_ConfigureSolution_ElementPropertyNotExists {
            get {
                return ResourceManager.GetString("SolutionItem_ConfigureSolution_ElementPropertyNotExists", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The property: &apos;{1}&apos; (on element: &apos;{0}&apos;) can only have one of these values: &apos;{2}&apos;, it cannot be: &apos;{3}&apos;.
        /// </summary>
        internal static string SolutionItem_ConfigureSolution_ElementPropertyValueIsNotOneOf {
            get {
                return ResourceManager.GetString("SolutionItem_ConfigureSolution_ElementPropertyValueIsNotOneOf", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The property: &apos;{1}&apos; (on element: &apos;{0}&apos;) is of type: &apos;{2}&apos;, and cannot accept value: &apos;{3}&apos;.
        /// </summary>
        internal static string SolutionItem_ConfigureSolution_ElementPropertyValueNotCompatible {
            get {
                return ResourceManager.GetString("SolutionItem_ConfigureSolution_ElementPropertyValueNotCompatible", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Tried to configure property: &apos;{1}&apos; (on element: &apos;{2}&apos;) but contains invalid assignment: &apos;{0}&apos;.
        /// </summary>
        internal static string SolutionItem_ConfigureSolution_PropertyAssignmentInvalid {
            get {
                return ResourceManager.GetString("SolutionItem_ConfigureSolution_PropertyAssignmentInvalid", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to This element does not support automation.
        /// </summary>
        internal static string SolutionItem_HasNoAutomations {
            get {
                return ResourceManager.GetString("SolutionItem_HasNoAutomations", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Cannot materialise item in an non-collection element.
        /// </summary>
        internal static string SolutionItem_MaterialiseNotACollection {
            get {
                return ResourceManager.GetString("SolutionItem_MaterialiseNotACollection", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The property: &apos;{0}&apos; is not an attribute.
        /// </summary>
        internal static string SolutionItem_NotAnAttribute {
            get {
                return ResourceManager.GetString("SolutionItem_NotAnAttribute", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The property: &apos;{0}&apos; cannot be found on this element.
        /// </summary>
        internal static string SolutionItem_NotAProperty {
            get {
                return ResourceManager.GetString("SolutionItem_NotAProperty", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to This solution item is not yet materialised.
        /// </summary>
        internal static string SolutionItem_NotMaterialised {
            get {
                return ResourceManager.GetString("SolutionItem_NotMaterialised", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The Pattern: &apos;{0}&apos; has already been materialized.
        /// </summary>
        internal static string SolutionItem_PatternAlreadyMaterialised {
            get {
                return ResourceManager.GetString("SolutionItem_PatternAlreadyMaterialised", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Command: &apos;{0}&apos; does not exist on this element.
        /// </summary>
        internal static string SolutionItem_UnknownAutomation {
            get {
                return ResourceManager.GetString("SolutionItem_UnknownAutomation", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The Value has already been materialized.
        /// </summary>
        internal static string SolutionItem_ValueAlreadyMaterialised {
            get {
                return ResourceManager.GetString("SolutionItem_ValueAlreadyMaterialised", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The expression: &apos;{0}&apos; is not a valid solution expression.
        /// </summary>
        internal static string SolutionPathResolver_InvalidExpression {
            get {
                return ResourceManager.GetString("SolutionPathResolver_InvalidExpression", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to A solution with id: &apos;{0}&apos; cannot be found in the solution store at location: &apos;{1}&apos;.
        /// </summary>
        internal static string SolutionStore_NotFoundAtLocationWithId {
            get {
                return ResourceManager.GetString("SolutionStore_NotFoundAtLocationWithId", resourceCulture);
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
        
        /// <summary>
        ///   Looks up a localized string similar to Template: &apos;{0}&apos; has errors:
        ///{1}.
        /// </summary>
        internal static string TextTemplatingExtensions_HasSyntaxErrors {
            get {
                return ResourceManager.GetString("TextTemplatingExtensions_HasSyntaxErrors", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Failed to transform template: &apos;{0}&apos;, errors were:
        ///{1}.
        /// </summary>
        internal static string TextTemplatingExtensions_TransformFailed {
            get {
                return ResourceManager.GetString("TextTemplatingExtensions_TransformFailed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to A toolkit with id: &apos;{0}&apos; cannot be found in the toolkit store at location: &apos;{1}&apos;.
        /// </summary>
        internal static string ToolkitStore_NotFoundAtLocationWithId {
            get {
                return ResourceManager.GetString("ToolkitStore_NotFoundAtLocationWithId", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Requested version number: &apos;{0}&apos;, (compared to expected version &apos;{1}&apos;) must represent the breaking changes (that have been auto-detected in this toolkit):
        ///&apos;{2}&apos;.
        /// </summary>
        internal static string ToolkitVersion_IllegalVersion {
            get {
                return ResourceManager.GetString("ToolkitVersion_IllegalVersion", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Requested version number: &apos;{0}&apos;, cannot be before the current toolkit version number: &apos;{1}&apos;.
        /// </summary>
        internal static string ToolkitVersion_VersionBeforeCurrent {
            get {
                return ResourceManager.GetString("ToolkitVersion_VersionBeforeCurrent", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Requested version number: &apos;{0}&apos;, cannot be used.
        /// </summary>
        internal static string ToolkitVersion_ZeroVersion {
            get {
                return ResourceManager.GetString("ToolkitVersion_ZeroVersion", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Requested version number: &apos;{0}&apos;, is not a supported version version number.
        /// </summary>
        internal static string VersionInstruction_InvalidVersionInstruction {
            get {
                return ResourceManager.GetString("VersionInstruction_InvalidVersionInstruction", resourceCulture);
            }
        }
    }
}
