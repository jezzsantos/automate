﻿using System;
using System.Linq;
using System.Text.Json.Nodes;
using Automate.Authoring.Domain;
using Automate.CLI.Extensions;
using Automate.Common.Extensions;
using JetBrains.Annotations;

namespace Automate.CLI.Infrastructure.Api
{
    internal partial class CommandLineApi
    {
        [UsedImplicitly]
        private class AuthoringApiHandlers : HandlerBase
        {
            internal static void ListEverything(bool outputStructured)
            {
                Recorder.MeasureListAll();
                ListPatterns(outputStructured);
                RuntimeApiHandlers.ListToolkits(outputStructured);
                RuntimeApiHandlers.ListDrafts(outputStructured);
            }

            internal static void CreatePattern(string name, string displayedAs,
                string describedAs, bool outputStructured)
            {
                var pattern = authoring.CreateNewPattern(name, displayedAs, describedAs);

                if (outputStructured)
                {
                    Output(OutputMessages.CommandLine_Output_PatternCreated, pattern.Name,
                        new
                        {
                            pattern.ToolkitVersion.Current,
                            pattern.ToolkitVersion.Next,
                            Change = pattern.ToolkitVersion.LastChanges.ToString()
                        }, pattern.Id);
                }
                else
                {
                    Output(OutputMessages.CommandLine_Output_PatternCreated, pattern.Name,
                        pattern.ToolkitVersion.Current, pattern.Id);
                }
            }

            internal static void UpdatePattern(string name, string displayedAs, string describedAs,
                bool outputStructured)
            {
                var pattern = authoring.UpdatePattern(name, displayedAs, describedAs);

                if (outputStructured)
                {
                    Output(OutputMessages.CommandLine_Output_PatternUpdated_ForStructured, pattern.Name, pattern.Id,
                        new
                        {
                            pattern.ToolkitVersion.Current,
                            pattern.ToolkitVersion.Next,
                            Change = pattern.ToolkitVersion.LastChanges.ToString()
                        }, FormatPatternSchema(true, pattern, false));
                }
                else
                {
                    Output(OutputMessages.CommandLine_Output_PatternUpdated, pattern.Name);
                }
            }

            internal static void SwitchPattern(string id, bool outputStructured)
            {
                var pattern = authoring.SwitchCurrentPattern(id);

                if (outputStructured)
                {
                    Output(OutputMessages.CommandLine_Output_PatternSwitched, pattern.Name,
                        new
                        {
                            pattern.ToolkitVersion.Current,
                            pattern.ToolkitVersion.Next,
                            Change = pattern.ToolkitVersion.LastChanges.ToString()
                        }, pattern.Id);
                }
                else
                {
                    Output(OutputMessages.CommandLine_Output_PatternSwitched, pattern.Name,
                        pattern.ToolkitVersion.Current, pattern.Id);
                }
            }

            internal static void ViewPattern(bool all, bool outputStructured)
            {
                var pattern = authoring.GetCurrentPattern();

                if (outputStructured)
                {
                    Output(OutputMessages.CommandLine_Output_PatternSchema, pattern.Name, pattern.Id,
                        new
                        {
                            pattern.ToolkitVersion.Current,
                            pattern.ToolkitVersion.Next,
                            Change = pattern.ToolkitVersion.LastChanges.ToString()
                        }, FormatPatternSchema(true, pattern, all));
                }
                else
                {
                    Output(OutputMessages.CommandLine_Output_PatternSchema, pattern.Name, pattern.Id,
                        pattern.ToolkitVersion.Current, FormatPatternSchema(false, pattern, all));
                }
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
                                Version = new
                                {
                                    pattern.ToolkitVersion.Current,
                                    pattern.ToolkitVersion.Next,
                                    Change = pattern.ToolkitVersion.LastChanges.ToString()
                                },
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

            internal static void AddCodeTemplate(string filepath, string name, string asChildOf, bool outputStructured)
            {
                var currentDirectory = Metadata.CurrentExecutionPath;
                var (parent, template) = authoring.AddCodeTemplate(currentDirectory, filepath, name, asChildOf);
                if (outputStructured)
                {
                    Output(OutputMessages.CommandLine_Output_CodeTemplateAdded_ForStructured, template.Template.Name,
                        template.Template.Id, parent.Id, template.Template.Metadata.OriginalFilePath,
                        template.Template.Metadata.OriginalFileExtension,
                        template.Location);
                }
                else
                {
                    Output(OutputMessages.CommandLine_Output_CodeTemplateAdded, template.Template.Name,
                        template.Template.Id, parent.Id, template.Template.Metadata.OriginalFilePath,
                        template.Location);
                }
            }

            internal static void AddCodeTemplateWithCommand(string filepath, string name, string commandName,
                bool isOneOff, string targetPath, string asChildOf, bool outputStructured)
            {
                var currentDirectory = Metadata.CurrentExecutionPath;
                var (parent, template, command) = authoring.AddCodeTemplateWithCommand(currentDirectory, filepath,
                    name, commandName, isOneOff, targetPath, asChildOf);
                if (outputStructured)
                {
                    Output(OutputMessages.CommandLine_Output_CodeTemplateWithCommandAdded_CodeTemplate_ForStructured,
                        JsonNode.Parse(new
                        {
                            template.Template.Name,
                            TemplateId = template.Template.Id,
                            ParentId = parent.Id,
                            template.Template.Metadata.OriginalFilePath,
                            template.Template.Metadata.OriginalFileExtension,
                            EditorPath = template.Location
                        }.ToJson()));
                    Output(OutputMessages.CommandLine_Output_CodeTemplateWithCommandAdded_Command_ForStructured,
                        JsonNode.Parse(new
                        {
                            command.Name,
                            CommandId = command.Id,
                            ParentId = parent.Id,
                            Type = command.Type.ToString(),
                            command.Metadata
                        }.ToJson()));
                }
                else
                {
                    Output(OutputMessages.CommandLine_Output_CodeTemplateAdded, template.Template.Name,
                        template.Template.Id, parent.Id, template.Template.Metadata.OriginalFilePath,
                        template.Location);
                    Output(OutputMessages.CommandLine_Output_CodeTemplateCommandAdded,
                        command.Name, command.Id, parent.Id);
                }
            }

            internal static void EditCodeTemplateContent(string templateName, string with, string args,
                string asChildOf)
            {
                var (parent, template, location) =
                    authoring.EditCodeTemplateContent(templateName, with, args, asChildOf);
                Output(OutputMessages.CommandLine_Output_CodeTemplateContentEdited, template.Name, template.Id,
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
                var currentDirectory = Metadata.CurrentExecutionPath;
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

            internal static void ViewCodeTemplateContent(string templateName, string asChildOf)
            {
                var (parent, template, location, content) = authoring.ViewCodeTemplateContent(templateName, asChildOf);
                Output(OutputMessages.CommandLine_Output_CodeTemplateContentViewed, template.Name, template.Id,
                    parent.Id, template.Metadata.OriginalFilePath, template.Metadata.OriginalFileExtension, location,
                    content);
            }

            internal static void AddCodeTemplateCommand(string codeTemplateName, string name, bool isOneOff,
                string targetPath, string asChildOf, bool outputStructured)
            {
                var (parent, command) =
                    authoring.AddCodeTemplateCommand(codeTemplateName, name, isOneOff, targetPath, asChildOf);
                if (outputStructured)
                {
                    Output(OutputMessages.CommandLine_Output_CommandAdded_ForStructured,
                        command.Name, command.Id, parent.Id, command.Type.ToString(), command.Metadata);
                }
                else
                {
                    Output(OutputMessages.CommandLine_Output_CodeTemplateCommandAdded,
                        command.Name, command.Id, parent.Id);
                }
            }

            internal static void UpdateCodeTemplateCommand(string commandName, string name, bool? isOneOff,
                string targetPath, string asChildOf, bool outputStructured)
            {
                var (parent, command) =
                    authoring.UpdateCodeTemplateCommand(commandName, name, isOneOff, targetPath, asChildOf);
                if (outputStructured)
                {
                    Output(OutputMessages.CommandLine_Output_CommandUpdated_ForStructured,
                        command.Name, command.Id, parent.Id, command.Type.ToString(), command.Metadata);
                }
                else
                {
                    Output(OutputMessages.CommandLine_Output_CodeTemplateCommandUpdated,
                        command.Name, command.Id, parent.Id, command.Metadata[nameof(CodeTemplateCommand.FilePath)],
                        command.Metadata[nameof(CodeTemplateCommand.IsOneOff)]);
                }
            }

            internal static void TestCodeTemplateCommand(string commandName, string asChildOf, string importData,
                string exportData)
            {
                var currentDirectory = Metadata.CurrentExecutionPath;
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
                string asChildOf, bool outputStructured)
            {
                var (parent, command) = authoring.AddCliCommand(applicationName, arguments, name, asChildOf);
                if (outputStructured)
                {
                    Output(OutputMessages.CommandLine_Output_CommandAdded_ForStructured,
                        command.Name, command.Id, parent.Id, command.Type.ToString(), command.Metadata);
                }
                else
                {
                    Output(OutputMessages.CommandLine_Output_CliCommandAdded,
                        command.Name, command.Id, parent.Id);
                }
            }

            internal static void UpdateCliCommand(string commandName, string app, string arguments,
                string name, string asChildOf, bool outputStructured)
            {
                var (parent, command) = authoring.UpdateCliCommand(commandName, name, app, arguments, asChildOf);
                if (outputStructured)
                {
                    Output(OutputMessages.CommandLine_Output_CommandUpdated_ForStructured,
                        command.Name, command.Id, parent.Id, command.Type.ToString(), command.Metadata);
                }
                else
                {
                    Output(OutputMessages.CommandLine_Output_CliCommandUpdated,
                        command.Name, command.Id, parent.Id, command.Metadata[nameof(CliCommand.ApplicationName)],
                        command.Metadata[nameof(CliCommand.Arguments)]);
                }
            }

            internal static void DeleteCommand(string commandName, string asChildOf)
            {
                var (parent, command) = authoring.DeleteCommand(commandName, asChildOf);
                Output(OutputMessages.CommandLine_Output_CommandDeleted,
                    command.Name, command.Id, parent.Id);
            }

            internal static void AddCommandLaunchPoint(string commandIdentifiers, string name, string from,
                string asChildOf, bool outputStructured)
            {
                var cmdIds = commandIdentifiers.SafeSplit(CommandLaunchPoint.CommandIdDelimiter,
                    StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).ToList();
                var (parent, launchPoint) = authoring.AddCommandLaunchPoint(name, cmdIds, from, asChildOf);
                if (outputStructured)
                {
                    Output(OutputMessages.CommandLine_Output_LaunchPointAdded_ForStructured,
                        launchPoint.Name, launchPoint.Id, parent.Id, launchPoint.Type.ToString(), launchPoint.Metadata);
                }
                else
                {
                    Output(OutputMessages.CommandLine_Output_LaunchPointAdded,
                        launchPoint.Name, launchPoint.Id, parent.Id,
                        launchPoint.Metadata[nameof(CommandLaunchPoint.CommandIds)]);
                }
            }

            internal static void UpdateLaunchPoint(string launchPointName, string name, string add, string remove,
                string from, string asChildOf, bool outputStructured)
            {
                var addIds = add.SafeSplit(CommandLaunchPoint.CommandIdDelimiter,
                    StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).ToList();
                var removeIds = remove.SafeSplit(CommandLaunchPoint.CommandIdDelimiter,
                    StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).ToList();
                var (parent, launchPoint) =
                    authoring.UpdateCommandLaunchPoint(launchPointName, name, addIds, removeIds, from, asChildOf);
                if (outputStructured)
                {
                    Output(OutputMessages.CommandLine_Output_LaunchPointUpdated_ForStructured,
                        launchPoint.Name, launchPoint.Id, parent.Id, launchPoint.Type.ToString(), launchPoint.Metadata);
                }
                else
                {
                    Output(OutputMessages.CommandLine_Output_LaunchPointUpdated,
                        launchPoint.Name, launchPoint.Id, parent.Id,
                        launchPoint.Metadata[nameof(CommandLaunchPoint.CommandIds)]);
                }
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
                    RuntimeApiHandlers.Install(package.ExportedLocation);
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
    }
}