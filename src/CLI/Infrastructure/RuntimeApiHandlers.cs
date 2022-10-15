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
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Local")]
    internal partial class CommandLineApi
    {
        [UsedImplicitly]
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
        private class RuntimeApiHandlers : HandlerBase
        {
            internal static void Install(string location)
            {
                var toolkit = runtime.InstallToolkit(location);
                Output(OutputMessages.CommandLine_Output_InstalledToolkit,
                    toolkit.PatternName, toolkit.Version);
            }

            internal static void ViewToolkit(bool all, bool outputStructured)
            {
                var toolkit = runtime.ViewCurrentToolkit();

                Output(OutputMessages.CommandLine_Output_ToolkitSchema, toolkit.PatternName, toolkit.Id,
                    toolkit.Version, toolkit.RuntimeVersion,
                    AuthoringApiHandlers.FormatPatternSchema(outputStructured, toolkit.Pattern, all));
            }

            internal static void ListToolkits(bool outputStructured)
            {
                var toolkits = runtime.ListInstalledToolkits();
                if (toolkits.Any())
                {
                    if (outputStructured)
                    {
                        Output(OutputMessages.CommandLine_Output_InstalledToolkitsListed,
                            JsonNode.Parse(toolkits.Select(toolkit =>
                            {
                                var compatibility = new StructuredToolkitCompatibilityInfo(toolkit, GetMetadata());
                                return new
                                {
                                    toolkit.Id,
                                    toolkit.PatternName,
                                    Version = new
                                    {
                                        compatibility.Toolkit,
                                        compatibility.Runtime,
                                        Compatibility = compatibility.ToolkitRuntimeCompatibility.ToString()
                                    }
                                };
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
                    draft.Name, draft.Id, draft.PatternName, draft.Toolkit.Id, draft.Toolkit.Version,
                    draft.Toolkit.RuntimeVersion);
            }

            internal static void ViewDraft(bool todo, bool outputStructured)
            {
                var (configuration, pattern, validation) = runtime.GetDraftConfiguration(todo, todo, outputStructured);

                var draftId = runtime.CurrentDraftId;
                var draftName = runtime.CurrentDraftName;
                var draftVersion = runtime.CurrentDraftToolkit.Version;
                var runtimeVersion = runtime.CurrentDraftToolkit.RuntimeVersion;
                var toolkitName = runtime.CurrentDraftToolkit.PatternName;
                var toolkitId = runtime.CurrentDraftToolkit.Id;

                Output(OutputMessages.CommandLine_Output_DraftConfiguration,
                    draftName, draftId, toolkitName, toolkitId, draftVersion, runtimeVersion, outputStructured
                        ? (object)JsonNode.Parse(configuration.ToJson())
                        : configuration.ToJson());

                if (todo)
                {
                    Output(OutputMessages.CommandLine_Output_PatternSchema, pattern.Name, pattern.Id,
                        pattern.ToolkitVersion.Current,
                        AuthoringApiHandlers.FormatPatternSchema(outputStructured, pattern, true));
                    Output(OutputMessages.CommandLine_Output_PatternLaunchableAutomation, pattern.Name, pattern.Id,
                        pattern.ToolkitVersion.Current,
                        AuthoringApiHandlers.FormatPatternLaunchableAutomation(outputStructured, pattern));
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
                var pairs = runtime.ListCreatedDrafts();
                if (pairs.Any())
                {
                    if (outputStructured)
                    {
                        Output(OutputMessages.CommandLine_Output_ConfiguredDraftsListed,
                            JsonNode.Parse(pairs.Select(pair =>
                            {
                                var compatibility = new StructuredToolkitCompatibilityInfo(pair.Draft, pair.Toolkit,
                                    GetMetadata());
                                return new
                                {
                                    DraftId = pair.Draft.Id,
                                    DraftName = pair.Draft.Name,
                                    ToolkitId = pair.Draft.Toolkit.Id,
                                    ToolkitName = pair.Draft.Toolkit.PatternName,
                                    ToolkitVersion = new
                                    {
                                        DraftCompatibility = compatibility.DraftCompatibility?.ToString(),
                                        compatibility.Toolkit,
                                        compatibility.Runtime,
                                        Compatibility = compatibility.ToolkitRuntimeCompatibility.ToString()
                                    },
                                    IsCurrent = pair.Draft.Id == currentDraft
                                };
                            }).ToList().ToJson()));
                    }
                    else
                    {
                        Output(OutputMessages.CommandLine_Output_ConfiguredDraftsListed,
                            pairs.ToMultiLineText(pair =>
                                $"\"Name\": \"{pair.Draft.Name}\", \"ToolkitVersion\": \"{pair.Draft.Toolkit.Version}\", \"CurrentToolkitVersion\": \"{pair.Toolkit.Version}\", \"ID\": \"{pair.Draft.Id}\", \"IsCurrent\": \"{(pair.Draft.Id == currentDraft).ToString().ToLowerInvariant()}\""));
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
                var toolkit = runtime.CurrentDraftToolkit;
                Output(OutputMessages.CommandLine_Output_DraftSwitched, runtime.CurrentDraftName,
                    runtime.CurrentDraftId, toolkit.PatternName,
                    toolkit.Id, toolkit.Version, toolkit.RuntimeVersion);
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
                Output(OutputMessages.CommandLine_Output_DraftDeleteElement,
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

            internal static void DeleteDraft()
            {
                var draft = runtime.DeleteDraft();
                Output(OutputMessages.CommandLine_Output_DraftDeleted,
                    draft.Name, draft.Id);
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
                        return JsonNode.Parse(items
                            .Select(item =>
                            {
                                var structuredMessage =
                                    item.MessageTemplate.SubstituteTemplateStructured(item.Arguments.ToArray());
                                return new
                                {
                                    Type = item.Type.ToString(),
                                    MessageTemplate = structuredMessage.Message,
                                    Arguments = structuredMessage.Values
                                };
                            })
                            .ToList()
                            .ToJson());
                    }
                    return items.ToBulletList(item =>
                        $"{item.Type}: {item.MessageTemplate.SubstituteTemplate(item.Arguments.ToArray())}");
                }
                return $"* {OutputMessages.CommandLine_Output_UpgradedDraftSucceededNoOutput}";
            }

            private class StructuredToolkitVersionInfo
            {
                public StructuredToolkitVersionInfo(string published, string installed)
                {
                    Published = published;
                    Installed = installed;
                }

                public string Published { get; }

                public string Installed { get; }
            }

            private class StructuredToolkitCompatibilityInfo
            {
                public StructuredToolkitCompatibilityInfo(ToolkitDefinition toolkit, IAssemblyMetadata metadata)
                {
                    DraftCompatibility = null;
                    Toolkit = new StructuredToolkitVersionInfo(toolkit.Version, toolkit.Version);
                    Runtime = new StructuredToolkitVersionInfo(toolkit.RuntimeVersion,
                        metadata.RuntimeVersion.ToString());
                    ToolkitRuntimeCompatibility = toolkit.GetCompatibility(metadata);
                }

                public StructuredToolkitCompatibilityInfo(DraftDefinition draft, ToolkitDefinition toolkit,
                    IAssemblyMetadata metadata)
                {
                    DraftCompatibility = draft.GetCompatibility(toolkit);
                    Toolkit = new StructuredToolkitVersionInfo(draft.Toolkit.Version, toolkit.Version);
                    Runtime = new StructuredToolkitVersionInfo(toolkit.RuntimeVersion,
                        metadata.RuntimeVersion.ToString());
                    ToolkitRuntimeCompatibility = toolkit.GetCompatibility(metadata);
                }

                public StructuredToolkitVersionInfo Toolkit { get; }

                public StructuredToolkitVersionInfo Runtime { get; }

                public DraftToolkitVersionCompatibility? DraftCompatibility { get; }

                public ToolkitRuntimeVersionCompatibility ToolkitRuntimeCompatibility { get; }
            }
        }
    }
}