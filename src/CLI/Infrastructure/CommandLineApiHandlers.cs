using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json.Nodes;
using Automate.Authoring.Domain;
using Automate.Common.Domain;
using Automate.Common.Extensions;
using Automate.Runtime.Domain;
using JetBrains.Annotations;

namespace Automate.CLI.Infrastructure
{
    internal partial class CommandLineApi
    {
        internal abstract class HandlerBase
        {
            private static List<OutputMessage> outputMessages;

            internal static void Initialise(List<OutputMessage> messages)
            {
                outputMessages = messages;
            }

            protected static void Output(string messageTemplate, params object[] args)
            {
                outputMessages.Add(new OutputMessage(OutputMessageLevel.Information, messageTemplate, args));
            }

            protected static void OutputWarning(string messageTemplate, params object[] args)
            {
                outputMessages.Add(new OutputMessage(OutputMessageLevel.Warning, messageTemplate, args));
            }

            protected static void OutputError(string messageTemplate, params object[] args)
            {
                outputMessages.Add(new OutputMessage(OutputMessageLevel.Error, messageTemplate, args));
            }
        }

#if TESTINGONLY
        [UsedImplicitly]
        internal class TestingOnlyHandlers : HandlerBase
        {
            [SuppressMessage("ReSharper", "ParameterOnlyUsedForPreconditionCheck.Global")]
            [SuppressMessage("ReSharper", "UnusedParameter.Global")]
            internal static void Fail(string message, bool nested)
            {
                Output(OutputMessages.CommandLine_Output_TestingOnly, "avalue");
                Output(OutputMessages.CommandLine_Output_TestingOnly, JsonNode.Parse(new
                {
                    AProperty1 = new
                    {
                        AChildProperty1 = "avalue1"
                    },
                    AProperty2 = "avalue2"
                }.ToJson()));
                if (nested)
                {
                    throw new Exception(message, new Exception(message));
                }
                throw new Exception(message);
            }

            internal static void Succeed(string message, string value)
            {
                Output(message, value);
                Output(OutputMessages.CommandLine_Output_TestingOnly, JsonNode.Parse(new
                {
                    AProperty1 = new
                    {
                        AChildProperty1 = "avalue1"
                    },
                    AProperty2 = "avalue2"
                }.ToJson()));
            }
        }
#endif

        [UsedImplicitly]
        private class AuthoringHandlers : HandlerBase
        {
            internal static void ListEverything(bool outputStructured)
            {
                ListPatterns(outputStructured);
                RuntimeHandlers.ListToolkits(outputStructured);
                RuntimeHandlers.ListDrafts(outputStructured);
            }

            internal static void CreatePattern(string name, string displayedAs,
                string describedAs)
            {
                authoring.CreateNewPattern(name, displayedAs, describedAs);
                Output(OutputMessages.CommandLine_Output_PatternCreated, authoring.CurrentPatternName,
                    authoring.CurrentPatternVersion, authoring.CurrentPatternId);
            }

            internal static void UpdatePattern(string name, string displayedAs, string describedAs)
            {
                var pattern = authoring.UpdatePattern(name, displayedAs, describedAs);
                Output(OutputMessages.CommandLine_Output_PatternUpdated, pattern.Name);
            }

            internal static void SwitchPattern(string id)
            {
                authoring.SwitchCurrentPattern(id);
                Output(OutputMessages.CommandLine_Output_PatternSwitched, authoring.CurrentPatternName,
                    authoring.CurrentPatternVersion, authoring.CurrentPatternId);
            }

            internal static void ViewPattern(bool all, bool outputStructured)
            {
                var pattern = authoring.GetCurrentPattern();

                Output(OutputMessages.CommandLine_Output_PatternSchema, pattern.Name, pattern.Id,
                    pattern.ToolkitVersion.Current, FormatPatternSchema(outputStructured, pattern, all));
            }

            internal static void ListPatterns(bool outputStructured)
            {
                var currentPattern = authoring.CurrentPatternId;
                var patterns = authoring.ListPatterns();
                if (patterns.Any())
                {
                    if (outputStructured)
                    {
                        Output(OutputMessages.CommandLine_Output_EditablePatternsListed,
                            JsonNode.Parse(patterns.Select(pattern => new
                            {
                                pattern.Id,
                                pattern.Name,
                                Version = pattern.ToolkitVersion.Current,
                                IsCurrent = pattern.Id == currentPattern
                            }).ToList().ToJson()));
                    }
                    else
                    {
                        Output(OutputMessages.CommandLine_Output_EditablePatternsListed,
                            patterns.ToMultiLineText(pattern =>
                                $"\"Name\": \"{pattern.Name}\", \"Version\": \"{pattern.ToolkitVersion.Current}\", \"ID\": \"{pattern.Id}\", \"IsCurrent\": \"{(pattern.Id == currentPattern).ToString().ToLowerInvariant()}\""));
                    }
                }
                else
                {
                    Output(OutputMessages.CommandLine_Output_NoEditablePatterns);
                }
            }

            internal static void AddAttribute(string name, string isOfType, string defaultValueIs,
                bool isRequired, string isOneOf, string asChildOf, bool outputStructured)
            {
                var choices = isOneOf.SafeSplit(";").ToList();
                var (parent, attribute) =
                    authoring.AddAttribute(name, isOfType, defaultValueIs, isRequired, choices, asChildOf);
                if (outputStructured)
                {
                    Output(OutputMessages.CommandLine_Output_AttributeAdded_ForStructured, attribute.Name,
                        attribute.Id, parent.Id, attribute.IsRequired, attribute.DataType, attribute.DefaultValue,
                        attribute.Choices);
                }
                else
                {
                    Output(OutputMessages.CommandLine_Output_AttributeAdded, attribute.Name,
                        attribute.Id, parent.Id);
                }
            }

            internal static void UpdateAttribute(string attributeName, string name, string isOfType,
                string defaultValueIs, bool? isRequired, string isOneOf, string asChildOf, bool outputStructured)
            {
                var choices = isOneOf.SafeSplit(";").ToList();
                var (parent, attribute) =
                    authoring.UpdateAttribute(attributeName, name, isOfType, defaultValueIs, isRequired, choices,
                        asChildOf);
                if (outputStructured)
                {
                    Output(OutputMessages.CommandLine_Output_AttributeUpdated_ForStructured, attribute.Name,
                        attribute.Id, parent.Id, attribute.IsRequired, attribute.DataType, attribute.DefaultValue,
                        attribute.Choices);
                }
                else
                {
                    Output(OutputMessages.CommandLine_Output_AttributeUpdated, attribute.Name,
                        attribute.Id, parent.Id);
                }
            }

            internal static void DeleteAttribute(string name, string asChildOf)
            {
                var (parent, attribute) =
                    authoring.DeleteAttribute(name, asChildOf);
                Output(OutputMessages.CommandLine_Output_AttributeDeleted, name,
                    attribute.Id, parent.Id);
            }

            internal static void AddElement(string name, bool? autoCreate, string displayedAs, string describedAs,
                string asChildOf, bool isRequired, bool outputStructured)
            {
                var (parent, element) = authoring.AddElement(name,
                    isRequired
                        ? ElementCardinality.One
                        : ElementCardinality.ZeroOrOne, autoCreate ?? isRequired, displayedAs, describedAs, asChildOf);
                if (outputStructured)
                {
                    Output(OutputMessages.CommandLine_Output_ElementAdded_ForStructured, element.Name, element.Id,
                        parent.Id, element.EditPath, element.DisplayName, element.Description,
                        element.AutoCreate, element.Cardinality.ToString(), element.IsCollection);
                }
                else
                {
                    Output(OutputMessages.CommandLine_Output_ElementAdded, element.Name, element.Id,
                        parent.Id);
                }
            }

            internal static void UpdateElement(string elementName, string name, bool? autoCreate,
                string displayedAs, string describedAs, string asChildOf, bool? isRequired, bool outputStructured)
            {
                var (parent, element) = authoring.UpdateElement(elementName, name,
                    isRequired, autoCreate, displayedAs, describedAs, asChildOf);
                if (outputStructured)
                {
                    Output(OutputMessages.CommandLine_Output_ElementUpdated_ForStructured, element.Name, element.Id,
                        parent.Id, element.EditPath, element.DisplayName, element.Description,
                        element.AutoCreate, element.Cardinality.ToString(), element.IsCollection);
                }
                else
                {
                    Output(OutputMessages.CommandLine_Output_ElementUpdated, element.Name, element.Id,
                        parent.Id);
                }
            }

            internal static void DeleteElement(string name, string asChildOf)
            {
                var (parent, element) =
                    authoring.DeleteElement(name, asChildOf);
                Output(OutputMessages.CommandLine_Output_ElementDeleted, name,
                    element.Id, parent.Id);
            }

            internal static void AddCollection(string name, bool? autoCreate, string displayedAs,
                string describedAs, string asChildOf, bool isRequired, bool outputStructured)
            {
                var (parent, collection) = authoring.AddElement(name, isRequired
                    ? ElementCardinality.OneOrMany
                    : ElementCardinality.ZeroOrMany, autoCreate ?? isRequired, displayedAs, describedAs, asChildOf);
                if (outputStructured)
                {
                    Output(OutputMessages.CommandLine_Output_CollectionAdded_ForStructured, collection.Name,
                        collection.Id,
                        parent.Id, collection.EditPath, collection.DisplayName, collection.Description,
                        collection.AutoCreate, collection.Cardinality.ToString(), collection.IsCollection);
                }
                else
                {
                    Output(OutputMessages.CommandLine_Output_CollectionAdded, collection.Name, collection.Id,
                        parent.Id);
                }
            }

            internal static void UpdateCollection(string collectionName, string name, bool? autoCreate,
                string displayedAs, string describedAs, string asChildOf, bool? isRequired, bool outputStructured)
            {
                var (parent, collection) = authoring.UpdateElement(collectionName, name,
                    isRequired, autoCreate, displayedAs, describedAs, asChildOf);
                if (outputStructured)
                {
                    Output(OutputMessages.CommandLine_Output_CollectionUpdated_ForStructured, collection.Name,
                        collection.Id,
                        parent.Id, collection.EditPath, collection.DisplayName, collection.Description,
                        collection.AutoCreate, collection.Cardinality.ToString(), collection.IsCollection);
                }
                else
                {
                    Output(OutputMessages.CommandLine_Output_CollectionUpdated, collection.Name, collection.Id,
                        parent.Id);
                }
            }

            internal static void DeleteCollection(string name, string asChildOf)
            {
                var (parent, element) =
                    authoring.DeleteElement(name, asChildOf);
                Output(OutputMessages.CommandLine_Output_CollectionDeleted, name,
                    element.Id, parent.Id);
            }

            internal static void AddCodeTemplate(string filepath, string name, string asChildOf)
            {
                var currentDirectory = Environment.CurrentDirectory;
                var (parent, template) = authoring.AddCodeTemplate(currentDirectory, filepath, name, asChildOf);
                Output(OutputMessages.CommandLine_Output_CodeTemplatedAdded, template.Template.Name,
                    template.Template.Id,
                    parent.Id, template.Template.Metadata.OriginalFilePath, template.Location);
            }

            internal static void AddCodeTemplateWithCommand(string filepath, string name, bool isOneOff,
                string targetPath, string asChildOf)
            {
                var currentDirectory = Environment.CurrentDirectory;
                var (parent, template, command) = authoring.AddCodeTemplateWithCommand(currentDirectory, filepath,
                    name, isOneOff, targetPath, asChildOf);
                Output(OutputMessages.CommandLine_Output_CodeTemplatedAdded, template.Template.Name,
                    template.Template.Id, parent.Id, template.Template.Metadata.OriginalFilePath,
                    template.Location);
                Output(OutputMessages.CommandLine_Output_CodeTemplateCommandAdded,
                    command.Name, command.Id, parent.Id);
            }

            internal static void EditCodeTemplate(string templateName, string with, string args, string asChildOf)
            {
                var (parent, template, location) = authoring.EditCodeTemplate(templateName, with, args, asChildOf);
                Output(OutputMessages.CommandLine_Output_CodeTemplatedEdited, template.Name, template.Id,
                    parent.Id, with, location);
            }

            internal static void DeleteCodeTemplate(string templateName, string asChildOf)
            {
                var (parent, template) = authoring.DeleteCodeTemplate(templateName, asChildOf);
                Output(OutputMessages.CommandLine_Output_CodeTemplateDeleted, template.Name, template.Id, parent.Id);
            }

            internal static void TestCodeTemplate(string templateName, string asChildOf, string importData,
                string exportData)
            {
                var currentDirectory = Environment.CurrentDirectory;
                var test =
                    authoring.TestCodeTemplate(templateName, asChildOf, currentDirectory, importData, exportData);
                if (exportData.HasValue())
                {
                    Output(OutputMessages.CommandLine_Output_CodeTemplateTestExported,
                        templateName, test.Template.Id, test.ExportedFilePath);
                }

                if (importData.HasValue())
                {
                    Output(OutputMessages.CommandLine_Output_CodeTemplateTestImported,
                        templateName, test.Template.Id, importData);
                }

                Output(OutputMessages.CommandLine_Output_TextTemplatingExpressionReference);
                Output(OutputMessages.CommandLine_Output_CodeTemplateTested,
                    templateName,
                    test.Template.Id, test.Output);
            }

            internal static void AddCodeTemplateCommand(string codeTemplateName, string name, bool isOneOff,
                string targetPath, string asChildOf)
            {
                var (parent, command) =
                    authoring.AddCodeTemplateCommand(codeTemplateName, name, isOneOff, targetPath, asChildOf);
                Output(OutputMessages.CommandLine_Output_CodeTemplateCommandAdded,
                    command.Name, command.Id, parent.Id);
            }

            internal static void UpdateCodeTemplateCommand(string commandName, string name, bool? isOneOff,
                string targetPath, string asChildOf)
            {
                var (parent, command) =
                    authoring.UpdateCodeTemplateCommand(commandName, name, isOneOff, targetPath, asChildOf);
                Output(OutputMessages.CommandLine_Output_CodeTemplateCommandUpdated,
                    command.Name, command.Id, parent.Id, command.Metadata[nameof(CodeTemplateCommand.FilePath)],
                    command.Metadata[nameof(CodeTemplateCommand.IsOneOff)]);
            }

            internal static void TestCodeTemplateCommand(string commandName, string asChildOf, string importData,
                string exportData)
            {
                var currentDirectory = Environment.CurrentDirectory;
                var test = authoring.TestCodeTemplateCommand(commandName, asChildOf, currentDirectory, importData,
                    exportData);
                if (exportData.HasValue())
                {
                    Output(OutputMessages.CommandLine_Output_CodeTemplateCommandTestExported,
                        commandName, test.Command.Id, test.ExportedFilePath);
                }

                if (importData.HasValue())
                {
                    Output(OutputMessages.CommandLine_Output_CodeTemplateCommandTestImported,
                        commandName, test.Command.Id, importData);
                }

                Output(OutputMessages.CommandLine_Output_TextTemplatingExpressionReference);
                Output(OutputMessages.CommandLine_Output_CodeTemplateCommandTested,
                    commandName, test.Command.Id, test.Output);
            }

            internal static void AddCliCommand(string applicationName, string arguments, string name,
                string asChildOf)
            {
                var (parent, command) = authoring.AddCliCommand(applicationName, arguments, name, asChildOf);
                Output(OutputMessages.CommandLine_Output_CliCommandAdded,
                    command.Name, command.Id, parent.Id);
            }

            internal static void UpdateCliCommand(string commandName, string app, string arguments,
                string name, string asChildOf)
            {
                var (parent, command) = authoring.UpdateCliCommand(commandName, name, app, arguments, asChildOf);
                Output(OutputMessages.CommandLine_Output_CliCommandUpdated,
                    command.Name, command.Id, parent.Id, command.Metadata[nameof(CliCommand.ApplicationName)],
                    command.Metadata[nameof(CliCommand.Arguments)]);
            }

            internal static void DeleteCommand(string commandName, string asChildOf)
            {
                var (parent, command) = authoring.DeleteCommand(commandName, asChildOf);
                Output(OutputMessages.CommandLine_Output_CommandDeleted,
                    command.Name, command.Id, parent.Id);
            }

            internal static void AddCommandLaunchPoint(string commandIdentifiers, string name, string from,
                string asChildOf)
            {
                var cmdIds = commandIdentifiers.SafeSplit(CommandLaunchPoint.CommandIdDelimiter,
                    StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).ToList();
                var (parent, launchPoint) = authoring.AddCommandLaunchPoint(name, cmdIds, from, asChildOf);
                Output(OutputMessages.CommandLine_Output_LaunchPointAdded,
                    launchPoint.Name, launchPoint.Id, parent.Id,
                    launchPoint.Metadata[nameof(CommandLaunchPoint.CommandIds)]);
            }

            internal static void UpdateLaunchPoint(string launchPointName, string name, string add,
                string from, string asChildOf)
            {
                var cmdIds = add.SafeSplit(CommandLaunchPoint.CommandIdDelimiter,
                    StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).ToList();
                var (parent, launchPoint) =
                    authoring.UpdateCommandLaunchPoint(launchPointName, name, cmdIds, from, asChildOf);
                Output(OutputMessages.CommandLine_Output_LaunchPointUpdated,
                    launchPoint.Name, launchPoint.Id, parent.Id,
                    launchPoint.Metadata[nameof(CommandLaunchPoint.CommandIds)]);
            }

            internal static void DeleteLaunchPoint(string launchPointName, string asChildOf)
            {
                var (parent, launchPoint) = authoring.DeleteCommandLaunchPoint(launchPointName, asChildOf);
                Output(OutputMessages.CommandLine_Output_LaunchPointDeleted,
                    launchPoint.Name, launchPoint.Id, parent.Id);
            }

            // ReSharper disable once IdentifierTypo
            internal static void Publish(string asversion, bool force, bool install)
            {
                var package = authoring.BuildAndExportToolkit(asversion, force);
                Output(OutputMessages.CommandLine_Output_BuiltToolkit,
                    package.Toolkit.PatternName, package.Toolkit.Version, package.ExportedLocation);
                if (package.Message.HasValue())
                {
                    OutputWarning(OutputMessages.CommandLine_Output_BuiltToolkit_Warning,
                        package.Message);
                }
                if (install)
                {
                    RuntimeHandlers.Install(package.ExportedLocation);
                }
            }

            internal static object FormatPatternSchema(bool outputStructured, PatternDefinition pattern,
                bool isDetailed)
            {
                var configuration = new PatternConfigurationVisitor(outputStructured
                    ? OutputFormat.Json
                    : OutputFormat.Text, isDetailed
                    ? VisitorConfigurationOptions.Detailed
                    : VisitorConfigurationOptions.Simple);
                pattern.TraverseDescendants(configuration);

                return configuration.ToOutput();
            }

            internal static object FormatPatternLaunchableAutomation(bool outputStructured, PatternDefinition pattern)
            {
                var configuration =
                    new PatternConfigurationVisitor(outputStructured
                        ? OutputFormat.Json
                        : OutputFormat.Text, VisitorConfigurationOptions.OnlyLaunchPoints);
                pattern.TraverseDescendants(configuration);

                return configuration.ToOutput();
            }
        }

        [UsedImplicitly]
        private class RuntimeHandlers : HandlerBase
        {
            internal static void Install(string location)
            {
                var toolkit = runtime.InstallToolkit(location);
                Output(OutputMessages.CommandLine_Output_InstalledToolkit,
                    toolkit.PatternName, toolkit.Version);
            }

            internal static void ViewToolkit(bool all, bool outputStructured)
            {
                var pattern = runtime.ViewCurrentToolkit().Pattern;

                Output(OutputMessages.CommandLine_Output_ToolkitSchema, pattern.Name, pattern.Id,
                    pattern.ToolkitVersion.Current,
                    AuthoringHandlers.FormatPatternSchema(outputStructured, pattern, all));
            }

            internal static void ListToolkits(bool outputStructured)
            {
                var toolkits = runtime.ListInstalledToolkits();
                if (toolkits.Any())
                {
                    if (outputStructured)
                    {
                        Output(OutputMessages.CommandLine_Output_InstalledToolkitsListed,
                            JsonNode.Parse(toolkits.Select(toolkit => new
                            {
                                toolkit.Id,
                                toolkit.PatternName,
                                toolkit.Version
                            }).ToList().ToJson()));
                    }
                    else
                    {
                        Output(OutputMessages.CommandLine_Output_InstalledToolkitsListed,
                            toolkits.ToMultiLineText(toolkit =>
                                $"\"Name\": \"{toolkit.PatternName}\", \"Version\": \"{toolkit.Version}\", \"ID\": \"{toolkit.Id}\""));
                    }
                }
                else
                {
                    Output(OutputMessages.CommandLine_Output_NoInstalledToolkits);
                }
            }

            internal static void NewDraft(string patternName, string name)
            {
                var draft = runtime.CreateDraft(patternName, name);
                Output(OutputMessages.CommandLine_Output_CreateDraftFromToolkit,
                    draft.Name, draft.Id, draft.PatternName, draft.Toolkit.Id, draft.Toolkit.Version);
            }

            internal static void ViewDraft(bool todo, bool outputStructured)
            {
                var (configuration, pattern, validation) = runtime.GetDraftConfiguration(todo, todo, outputStructured);

                var draftId = runtime.CurrentDraftId;
                var draftName = runtime.CurrentDraftName;

                Output(OutputMessages.CommandLine_Output_DraftConfiguration,
                    draftName, draftId, outputStructured
                        ? (object)JsonNode.Parse(configuration.ToJson())
                        : configuration.ToJson());

                if (todo)
                {
                    Output(OutputMessages.CommandLine_Output_PatternSchema, pattern.Name, pattern.Id,
                        pattern.ToolkitVersion.Current,
                        AuthoringHandlers.FormatPatternSchema(outputStructured, pattern, true));
                    Output(OutputMessages.CommandLine_Output_PatternLaunchableAutomation, pattern.Name, pattern.Id,
                        pattern.ToolkitVersion.Current,
                        AuthoringHandlers.FormatPatternLaunchableAutomation(outputStructured, pattern));
                    if (validation.HasAny())
                    {
                        OutputWarning(OutputMessages.CommandLine_Output_DraftValidationFailed,
                            draftName, draftId, FormatValidationErrors(outputStructured, validation));
                    }
                    else
                    {
                        Output(OutputMessages.CommandLine_Output_DraftValidationSuccess, draftName, draftId);
                    }
                }
            }

            internal static void ListDrafts(bool outputStructured)
            {
                var currentDraft = runtime.CurrentDraftId;
                var drafts = runtime.ListCreatedDrafts();
                if (drafts.Any())
                {
                    if (outputStructured)
                    {
                        Output(OutputMessages.CommandLine_Output_ConfiguredDraftsListed,
                            JsonNode.Parse(drafts.Select(draft => new
                            {
                                draft.Id,
                                draft.Name,
                                ToolkitId = draft.Toolkit.Id,
                                draft.Toolkit.Version,
                                IsCurrent = draft.Id == currentDraft
                            }).ToList().ToJson()));
                    }
                    else
                    {
                        Output(OutputMessages.CommandLine_Output_ConfiguredDraftsListed,
                            drafts.ToMultiLineText(draft =>
                                $"\"Name\": \"{draft.Name}\", \"Version\": \"{draft.Toolkit.Version}\", \"ID\": \"{draft.Id}\", \"IsCurrent\": \"{(draft.Id == currentDraft).ToString().ToLowerInvariant()}\""));
                    }
                }
                else
                {
                    Output(OutputMessages.CommandLine_Output_NoConfiguredDrafts);
                }
            }

            internal static void SwitchDraft(string draftId)
            {
                runtime.SwitchCurrentDraft(draftId);
                Output(OutputMessages.CommandLine_Output_DraftSwitched, runtime.CurrentDraftName,
                    runtime.CurrentDraftId, runtime.CurrentDraftToolkit.PatternName,
                    runtime.CurrentDraftToolkit.Id, runtime.CurrentDraftToolkit.Version);
            }

            internal static void ConfigureDraftOn(string expression, string[] andSet, bool outputStructured)
            {
                var sets = new List<string>();
                if (andSet.HasAny())
                {
                    sets.AddRange(andSet);
                }
                var nameValues = sets
                    .Select(set => set.SplitPropertyAssignment())
                    .ToDictionary(pair => pair.Name, pair => pair.Value);

                var (configuration, draftItem) =
                    runtime.ConfigureDraft(null, null, expression, nameValues, outputStructured);
                Output(OutputMessages.CommandLine_Output_DraftConfigured,
                    draftItem.Name, draftItem.Id, outputStructured
                        ? (object)JsonNode.Parse(configuration.ToJson())
                        : configuration.ToJson());
            }

            internal static void ConfigureDraftAddTo(string expression, string[] andSet, bool outputStructured)
            {
                var sets = new List<string>();
                if (andSet.HasAny())
                {
                    sets.AddRange(andSet);
                }

                var nameValues = sets
                    .Select(set => set.SplitPropertyAssignment())
                    .ToDictionary(pair => pair.Name, pair => pair.Value);

                var (configuration, draftItem) =
                    runtime.ConfigureDraft(expression, null, null, nameValues, outputStructured);
                Output(OutputMessages.CommandLine_Output_DraftConfigured,
                    draftItem.Name, draftItem.Id, outputStructured
                        ? (object)JsonNode.Parse(configuration.ToJson())
                        : configuration.ToJson());
            }

            internal static void ConfigureDraftAddOneTo(string expression, string[] andSet, bool outputStructured)
            {
                var sets = new List<string>();
                if (andSet.HasAny())
                {
                    sets.AddRange(andSet);
                }
                var nameValues = sets
                    .Select(set => set.SplitPropertyAssignment())
                    .ToDictionary(pair => pair.Name, pair => pair.Value);

                var (configuration, draftItem) =
                    runtime.ConfigureDraft(null, expression, null, nameValues, outputStructured);
                Output(OutputMessages.CommandLine_Output_DraftConfigured,
                    draftItem.Name, draftItem.Id, outputStructured
                        ? (object)JsonNode.Parse(configuration.ToJson())
                        : configuration.ToJson());
            }

            internal static void ConfigureDraftResetElement(string expression)
            {
                var draftItem = runtime.ConfigureDraftAndResetElement(expression);
                Output(OutputMessages.CommandLine_Output_DraftResetElement,
                    draftItem.Name, draftItem.Id);
            }

            internal static void ConfigureDraftClearCollection(string expression)
            {
                var draftItem = runtime.ConfigureDraftAndClearCollection(expression);
                Output(OutputMessages.CommandLine_Output_DraftEmptyCollection,
                    draftItem.Name, draftItem.Id);
            }

            internal static void ConfigureDraftDeleteElement(string expression)
            {
                var draftItem = runtime.ConfigureDraftAndDelete(expression);
                Output(OutputMessages.CommandLine_Output_DraftDelete,
                    draftItem.Name, draftItem.Id);
            }

            internal static void ValidateDraft(string on, bool outputStructured)
            {
                var results = runtime.Validate(on);

                var draftId = runtime.CurrentDraftId;
                var draftName = runtime.CurrentDraftName;
                if (results.HasAny())
                {
                    OutputWarning(OutputMessages.CommandLine_Output_DraftValidationFailed,
                        draftName, draftId, FormatValidationErrors(outputStructured, results));
                }
                else
                {
                    Output(OutputMessages.CommandLine_Output_DraftValidationSuccess,
                        draftName, draftId);
                }
            }

            internal static void UpgradeDraft(bool force, bool outputStructured)
            {
                var upgrade = runtime.UpgradeDraft(force);
                if (upgrade.Log.Any(entry => entry.Type == MigrationChangeType.Abort))
                {
                    OutputWarning(OutputMessages.CommandLine_Output_DraftUpgradeWithWarning,
                        upgrade.Draft.Name, upgrade.Draft.Id, upgrade.Draft.PatternName,
                        upgrade.FromVersion, upgrade.ToVersion, FormatUpgradeLog(outputStructured, upgrade.Log));
                }
                else
                {
                    Output(OutputMessages.CommandLine_Output_DraftUpgradeSucceeded,
                        upgrade.Draft.Name, upgrade.Draft.Id, upgrade.Draft.PatternName,
                        upgrade.FromVersion, upgrade.ToVersion, FormatUpgradeLog(outputStructured, upgrade.Log));
                }
            }

            internal static void ExecuteLaunchPoint(string name, string on, bool outputStructured)
            {
                var execution = runtime.ExecuteLaunchPoint(name, on);
                if (execution.IsSuccess)
                {
                    Output(OutputMessages.CommandLine_Output_CommandExecutionSucceeded,
                        execution.CommandName, FormatExecutionLog(outputStructured, execution.Log));
                }
                else
                {
                    if (execution.IsInvalid)
                    {
                        if (outputStructured)
                        {
                            OutputWarning(OutputMessages.CommandLine_Output_CommandExecutionFailed_WithValidation,
                                execution.CommandName,
                                FormatValidationErrors(true, execution.ValidationErrors));
                        }
                        else
                        {
                            OutputWarning(OutputMessages.CommandLine_Output_CommandExecutionFailed_WithValidation,
                                execution.CommandName,
                                FormatValidationErrors(false, execution.ValidationErrors));
                        }
                    }
                    else
                    {
                        OutputWarning(OutputMessages.CommandLine_Output_CommandExecutionFailed,
                            execution.CommandName, FormatExecutionLog(outputStructured, execution.Log));
                    }
                }
            }

            private static object FormatValidationErrors(bool outputStructured, ValidationResults results)
            {
                if (outputStructured)
                {
                    return JsonNode.Parse(results.ToJson());
                }

                var counter = 1;
                return results.Results.ToMultiLineText(item => $"{counter++}. {item.Context.Path} {item.Message}");
            }

            private static object FormatExecutionLog(bool outputStructured,
                IReadOnlyList<CommandExecutionLogItem> items)
            {
                if (items.HasAny())
                {
                    if (outputStructured)
                    {
                        return JsonNode.Parse(items
                            .Select(item => new
                            {
                                item.Message,
                                Type = item.Type.ToString()
                            })
                            .ToList()
                            .ToJson());
                    }
                    return items.ToBulletList(item => $"{item.Type}: {item.Message}");
                }
                return $"* {OutputMessages.CommandLine_Output_ExecuteLaunchPointSucceededNoOutput}";
            }

            private static object FormatUpgradeLog(bool outputStructured, IReadOnlyList<MigrationChange> items)
            {
                if (items.HasAny())
                {
                    if (outputStructured)
                    {
                        return JsonNode.Parse(items.ToJson());
                    }
                    return items.ToBulletList(item =>
                        $"{item.Type}: {item.MessageTemplate.SubstituteTemplate(item.Arguments.ToArray())}");
                }
                return $"* {OutputMessages.CommandLine_Output_UpgradedDraftSucceededNoOutput}";
            }
        }
    }
}