using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Automate.Authoring.Domain;
using Automate.CLI.Extensions;
using Automate.Common.Domain;
using Automate.Common.Extensions;
using Humanizer;
using Attribute = Automate.Authoring.Domain.Attribute;

namespace Automate.CLI.Infrastructure
{
    internal enum VisitorConfigurationOptions
    {
        Simple,
        Detailed,
        OnlyLaunchPoints
    }

    public enum OutputFormat
    {
        Text = 0,
        Json = 1
    }

    internal class PatternConfigurationVisitor : IPatternVisitor
    {
        private const int MaxFilePathLength = 100;
        private readonly OutputFormat format;
        private readonly VisitorConfigurationOptions options;
        private readonly StringBuilder output;
        private int indentLevel;

        public PatternConfigurationVisitor(OutputFormat format, VisitorConfigurationOptions options)
        {
            this.output = new StringBuilder();
            this.options = options;
            this.format = format;
        }

        public static string TruncateCodeTemplatePath(string path)
        {
            return TruncatePath(path);
        }

        public object ToOutput()
        {
            var result = this.output.ToString();
            if (this.format == OutputFormat.Json)
            {
                return JsonNode.Parse(result, null, new JsonDocumentOptions
                {
                    AllowTrailingCommas = true
                });
            }

            return result;
        }

        public bool VisitPatternEnter(PatternDefinition pattern)
        {
            if (this.format == OutputFormat.Json)
            {
                PrintInline("{");
                PrintInline($"\"{nameof(PatternDefinition.Id)}\":\"{JsonEscape(pattern.Id)}\",");
                PrintInline($"\"{nameof(PatternDefinition.EditPath)}\":\"{JsonEscape(pattern.EditPath)}\",");
                PrintInline($"\"{nameof(PatternDefinition.Name)}\":\"{JsonEscape(pattern.Name)}\",");
                PrintInline($"\"{nameof(PatternDefinition.DisplayName)}\":\"{JsonEscape(pattern.DisplayName)}\",");
                PrintInline($"\"{nameof(PatternDefinition.Description)}\":\"{JsonEscape(pattern.Description)}\",");
            }
            else if (this.format == OutputFormat.Text)
            {
                PrintIndented($"- {pattern.Name}", false);
                if (this.options == VisitorConfigurationOptions.Detailed)
                {
                    PrintInline($" [{pattern.Id}]");
                }
                PrintInline(" (root element)");
                if (this.options == VisitorConfigurationOptions.Simple && pattern.CodeTemplates.HasAny())
                {
                    PrintInline($" (attached with {pattern.CodeTemplates.Count} code templates)", true);
                }
                else
                {
                    PrintEndOfLine();
                }
            }

            return VisitPatternElementEnter(pattern);
        }

        public bool VisitPatternExit(PatternDefinition pattern)
        {
            return VisitPatternElementExit(pattern);
        }

        public bool VisitElementsEnter(IReadOnlyList<Element> elements)
        {
            if (this.format == OutputFormat.Json)
            {
                if ((this.options is VisitorConfigurationOptions.OnlyLaunchPoints &&
                     elements.HasAnyDescendantLaunchPoints())
                    || this.options is not VisitorConfigurationOptions.OnlyLaunchPoints)
                {
                    PrintInline("\"Elements\":[");
                }
            }
            else if (this.format == OutputFormat.Text)
            {
                if ((this.options is VisitorConfigurationOptions.Detailed && elements.HasAny())
                    || (this.options is VisitorConfigurationOptions.OnlyLaunchPoints &&
                        elements.HasAnyDescendantLaunchPoints()))
                {
                    PrintIndented("- Elements:");
                }
            }

            return true;
        }

        public bool VisitElementsExit(IReadOnlyList<Element> elements)
        {
            if (this.format == OutputFormat.Json)
            {
                if ((this.options is VisitorConfigurationOptions.OnlyLaunchPoints &&
                     elements.HasAnyDescendantLaunchPoints())
                    || this.options is not VisitorConfigurationOptions.OnlyLaunchPoints)
                {
                    PrintInline("],");
                }
            }
            return true;
        }

        public bool VisitElementEnter(Element element)
        {
            if (this.options == VisitorConfigurationOptions.OnlyLaunchPoints
                && !element.HasAnyDescendantLaunchPoints())
            {
                return true;
            }
            
            if (this.format == OutputFormat.Json)
            {
                PrintInline("{");
                PrintInline($"\"{nameof(Element.Id)}\":\"{JsonEscape(element.Id)}\",");
                PrintInline($"\"{nameof(Element.EditPath)}\":\"{JsonEscape(element.EditPath)}\",");
                PrintInline($"\"{nameof(Element.Name)}\":\"{JsonEscape(element.Name)}\",");
                PrintInline($"\"{nameof(Element.DisplayName)}\":\"{JsonEscape(element.DisplayName)}\",");
                PrintInline($"\"{nameof(Element.Description)}\":\"{JsonEscape(element.Description)}\",");
                if (this.options != VisitorConfigurationOptions.OnlyLaunchPoints)
                {
                    PrintInline(
                        $"\"{nameof(Element.AutoCreate)}\":{element.AutoCreate.ToString().ToLowerInvariant()},");
                    PrintInline(
                        $"\"{nameof(Element.IsCollection)}\":{element.IsCollection.ToString().ToLowerInvariant()},");
                    PrintInline($"\"{nameof(Element.Cardinality)}\":\"{element.Cardinality}\",");
                }
            }
            else if (this.format == OutputFormat.Text)
            {
                if (this.options is VisitorConfigurationOptions.Detailed
                    or VisitorConfigurationOptions.OnlyLaunchPoints)
                {
                    Indent();
                }

                PrintIndented($"- {element.Name}", false);
                if (this.options == VisitorConfigurationOptions.Detailed)
                {
                    PrintInline($" [{element.Id}]");
                }
                PrintInline($" ({(element.IsCollection ? "collection" : "element")})");
                if (this.options == VisitorConfigurationOptions.Simple && element.CodeTemplates.HasAny())
                {
                    PrintInline($" (attached with {element.CodeTemplates.Count} code templates)", true);
                }
                else
                {
                    PrintEndOfLine();
                }
            }

            return VisitPatternElementEnter(element);
        }

        public bool VisitElementExit(Element element)
        {
            if (this.options == VisitorConfigurationOptions.OnlyLaunchPoints
                && !element.HasAnyDescendantLaunchPoints())
            {
                return true;
            }
            
            if (this.format == OutputFormat.Text)
            {
                if (this.options is VisitorConfigurationOptions.Detailed
                    or VisitorConfigurationOptions.OnlyLaunchPoints)
                {
                    OutDent();
                }
            }
            var result = VisitPatternElementExit(element);

            if (this.format == OutputFormat.Json)
            {
                PrintInline(",");
            }

            return result;
        }

        public bool VisitAttributesEnter(IReadOnlyList<Attribute> attributes)
        {
            if (this.format == OutputFormat.Json)
            {
                if (this.options != VisitorConfigurationOptions.OnlyLaunchPoints)
                {
                    PrintInline("\"Attributes\":[");
                }
            }
            else if (this.format == OutputFormat.Text)
            {
                if (this.options == VisitorConfigurationOptions.Detailed && attributes.HasAny())
                {
                    PrintIndented("- Attributes:");
                }
            }
            return true;
        }

        public bool VisitAttributesExit(IReadOnlyList<Attribute> attributes)
        {
            if (this.format == OutputFormat.Json)
            {
                if (this.options != VisitorConfigurationOptions.OnlyLaunchPoints)
                {
                    PrintInline("],");
                }
            }
            return true;
        }

        public bool VisitAttribute(Attribute attribute)
        {
            if (this.options == VisitorConfigurationOptions.OnlyLaunchPoints)
            {
                return true;
            }

            if (this.format == OutputFormat.Json)
            {
                var choices = attribute.Choices.Select(choice => $"\"{JsonEscape(choice)}\"").Join(",");

                PrintInline("{");
                PrintInline($"\"{nameof(Attribute.Id)}\":\"{JsonEscape(attribute.Id)}\",");
                PrintInline($"\"{nameof(Attribute.Name)}\":\"{JsonEscape(attribute.Name)}\",");
                PrintInline($"\"{nameof(Attribute.DataType)}\":\"{JsonEscape(attribute.DataType)}\",");
                PrintInline(
                    $"\"{nameof(Attribute.IsRequired)}\":{attribute.IsRequired.ToString().ToLowerInvariant()},");
                PrintInline($"\"{nameof(Attribute.Choices)}\":[{choices}],");
                PrintInline($"\"{nameof(Attribute.DefaultValue)}\": \"{JsonEscape(attribute.DefaultValue)}\"");
                PrintInline("},");
            }
            else if (this.format == OutputFormat.Text)
            {
                WithDetailedIndents(() =>
                {
                    PrintIndented(
                        $"- {attribute.Name}{(this.options == VisitorConfigurationOptions.Detailed ? "" : " (attribute)")} ({attribute.DataType}{(attribute.IsRequired ? ", required" : "")}{(attribute.Choices.HasAny() ? ", oneof: " + $"{attribute.Choices.ToListSafe().Join(";")}" : "")}{(attribute.DefaultValue.HasValue() ? ", default: " + $"{attribute.DefaultValue}" : "")})");
                });
            }

            return true;
        }

        public bool VisitAutomationsEnter(IReadOnlyList<Automation> automation)
        {
            if (this.format == OutputFormat.Json)
            {
                if (this.options != VisitorConfigurationOptions.OnlyLaunchPoints
                    || (this.options == VisitorConfigurationOptions.OnlyLaunchPoints &&
                        automation.Safe().Any(auto => auto.IsLaunching())))
                {
                    if (this.options == VisitorConfigurationOptions.OnlyLaunchPoints)
                    {
                        PrintInline("\"LaunchPoints\":[");
                    }
                    else
                    {
                        PrintInline("\"Automation\":[");
                    }
                }
            }
            else if (this.format == OutputFormat.Text)
            {
                if ((this.options == VisitorConfigurationOptions.Detailed && automation.HasAny())
                    || (this.options == VisitorConfigurationOptions.OnlyLaunchPoints &&
                        automation.Safe().Any(auto => auto.IsLaunching())))
                {
                    if (this.options == VisitorConfigurationOptions.OnlyLaunchPoints)
                    {
                        PrintIndented("- LaunchPoints:");
                    }
                    if (this.options == VisitorConfigurationOptions.Detailed)
                    {
                        PrintIndented("- Automation:");
                    }
                }
            }

            return true;
        }

        public bool VisitAutomationsExit(IReadOnlyList<Automation> automation)
        {
            if (this.format == OutputFormat.Json)
            {
                if (this.options != VisitorConfigurationOptions.OnlyLaunchPoints
                    || (this.options == VisitorConfigurationOptions.OnlyLaunchPoints &&
                        automation.Safe().Any(auto => auto.IsLaunching())))
                {
                    PrintInline("],");
                }
            }

            return true;
        }

        public bool VisitAutomation(Automation automation)
        {
            if (this.format == OutputFormat.Json)
            {
                if (this.options == VisitorConfigurationOptions.OnlyLaunchPoints)
                {
                    if (automation.IsLaunching())
                    {
                        PrintInline("{");
                        PrintInline($"\"{nameof(Automation.Id)}\":\"{JsonEscape(automation.Id)}\",");
                        PrintInline($"\"{nameof(Automation.Name)}\":\"{JsonEscape(automation.Name)}\",");
                        PrintInline($"\"{nameof(Automation.Type)}\":\"{JsonEscape(automation.Type.ToString())}\"");
                        PrintInline("},");
                    }
                }
                else
                {
                    PrintInline("{");
                    PrintInline($"\"{nameof(Automation.Id)}\":\"{JsonEscape(automation.Id)}\",");
                    PrintInline($"\"{nameof(Automation.Name)}\":\"{JsonEscape(automation.Name)}\",");
                    PrintInline($"\"{nameof(Automation.Type)}\":\"{JsonEscape(automation.Type.ToString())}\",");

                    if (automation.Type == AutomationType.CodeTemplateCommand)
                    {
                        PrintInline(
                            $"\"TemplateId\":\"{JsonEscape(automation.Metadata[nameof(CodeTemplateCommand.CodeTemplateId)]?.ToString())}\"," +
                            $"\"{nameof(CodeTemplateCommand.IsOneOff)}\":{JsonEscape(automation.Metadata[nameof(CodeTemplateCommand.IsOneOff)].ToString()?.ToLowerInvariant())}," +
                            $"\"TargetPath\":\"{JsonEscape(automation.Metadata[nameof(CodeTemplateCommand.FilePath)]?.ToString())}\"");
                    }
                    else if (automation.Type == AutomationType.CliCommand)
                    {
                        PrintInline(
                            $"\"{nameof(CliCommand.ApplicationName)}\":\"{JsonEscape(automation.Metadata[nameof(CliCommand.ApplicationName)]?.ToString())}\"," +
                            $"\"{nameof(CliCommand.Arguments)}\":\"{JsonEscape(automation.Metadata[nameof(CliCommand.Arguments)]?.ToString())}\"");
                    }
                    else if (automation.Type == AutomationType.CommandLaunchPoint)
                    {
                        var cmdIds = automation.Metadata[nameof(CommandLaunchPoint.CommandIds)]
                            .ToString()
                            .SafeSplit(CommandLaunchPoint.CommandIdDelimiter)
                            .Select(id => $"\"{JsonEscape(id)}\"")
                            .Join(",");
                        PrintInline($"\"{nameof(CommandLaunchPoint.CommandIds)}\": [{cmdIds}]");
                    }
                    PrintInline("},");
                }
            }
            else if (this.format == OutputFormat.Text)
            {
                WithDetailedIndents(() =>
                {
                    if (this.options == VisitorConfigurationOptions.Detailed)
                    {
                        PrintIndented($"- {automation.Name} [{automation.Id}] ({automation.Type})", false);
                        if (automation.Type == AutomationType.CodeTemplateCommand)
                        {
                            PrintInline(
                                $" (template: {automation.Metadata[nameof(CodeTemplateCommand.CodeTemplateId)]}, {(automation.Metadata[nameof(CodeTemplateCommand.IsOneOff)].ToString()!.ToBool() ? "onceonly" : "always")}, path: {automation.Metadata[nameof(CodeTemplateCommand.FilePath)]})",
                                true);
                        }
                        else if (automation.Type == AutomationType.CliCommand)
                        {
                            PrintInline(
                                $" (app: {automation.Metadata[nameof(CliCommand.ApplicationName)]}, args: {automation.Metadata[nameof(CliCommand.Arguments)]})",
                                true);
                        }
                        else if (automation.Type == AutomationType.CommandLaunchPoint)
                        {
                            PrintInline($" (ids: {automation.Metadata[nameof(CommandLaunchPoint.CommandIds)]})", true);
                        }
                        else
                        {
                            PrintEndOfLine();
                        }
                    }
                    if (this.options == VisitorConfigurationOptions.OnlyLaunchPoints)
                    {
                        if (automation.IsLaunching())
                        {
                            PrintIndented($"- {automation.Name} [{automation.Id}] ({automation.Type})");
                        }
                    }
                });
            }
            return true;
        }

        public bool VisitCodeTemplatesEnter(IReadOnlyList<CodeTemplate> codeTemplates)
        {
            if (this.format == OutputFormat.Json)
            {
                if (this.options != VisitorConfigurationOptions.OnlyLaunchPoints)
                {
                    PrintInline("\"CodeTemplates\":[");
                }
            }
            else if (this.format == OutputFormat.Text)
            {
                if (this.options == VisitorConfigurationOptions.Detailed && codeTemplates.HasAny())
                {
                    PrintIndented("- CodeTemplates:");
                }
            }

            return true;
        }

        public bool VisitCodeTemplatesExit(IReadOnlyList<CodeTemplate> codeTemplates)
        {
            if (this.format == OutputFormat.Json)
            {
                if (this.options != VisitorConfigurationOptions.OnlyLaunchPoints)
                {
                    PrintInline("],");
                }
            }
            return true;
        }

        public bool VisitCodeTemplate(CodeTemplate codeTemplate)
        {
            if (this.options == VisitorConfigurationOptions.OnlyLaunchPoints)
            {
                return true;
            }

            if (this.format == OutputFormat.Json)
            {
                PrintInline("{");
                PrintInline($"\"{nameof(CodeTemplate.Id)}\":\"{JsonEscape(codeTemplate.Id)}\",");
                PrintInline($"\"{nameof(CodeTemplate.Name)}\":\"{JsonEscape(codeTemplate.Name)}\",");
                PrintInline(
                    $"\"{nameof(CodeTemplateMetadata.OriginalFilePath)}\":\"{JsonEscape(codeTemplate.Metadata.OriginalFilePath)}\",");
                PrintInline(
                    $"\"{nameof(CodeTemplateMetadata.OriginalFileExtension)}\":\"{JsonEscape(codeTemplate.Metadata.OriginalFileExtension)}\"");
                PrintInline("},");
            }
            else if (this.format == OutputFormat.Text)
            {
                WithDetailedIndents(() =>
                {
                    if (this.options == VisitorConfigurationOptions.Detailed)
                    {
                        PrintIndented(
                            $"- {codeTemplate.Name} [{codeTemplate.Id}] (original: {TruncatePath(codeTemplate.Metadata.OriginalFilePath)})");
                    }
                });
            }

            return true;
        }

        private void PrintEndOfLine()
        {
            this.output.Append(Environment.NewLine);
        }

        // ReSharper disable once UnusedParameter.Local
        private bool VisitPatternElementEnter(PatternElement element)
        {
            if (this.format == OutputFormat.Text)
            {
                Indent();
            }
            return true;
        }

        // ReSharper disable once UnusedParameter.Local
        private bool VisitPatternElementExit(PatternElement element)
        {
            if (this.format == OutputFormat.Json)
            {
                PrintInline(" }");
            }
            else if (this.format == OutputFormat.Text)
            {
                OutDent();
            }
            return true;
        }

        private void Indent()
        {
            this.indentLevel++;
        }

        private void OutDent()
        {
            this.indentLevel--;
        }

        private void PrintIndented(string text, bool endOfLine = true)
        {
            this.output.Append(new string('\t', this.indentLevel));
            if (endOfLine)
            {
                this.output.AppendLine(text);
            }
            else
            {
                this.output.Append(text);
            }
        }

        private void PrintInline(string text, bool endOfLine = false)
        {
            if (endOfLine)
            {
                this.output.AppendLine(text);
            }
            else
            {
                this.output.Append(text);
            }
        }

        private void WithDetailedIndents(Action action)
        {
            if (this.options is VisitorConfigurationOptions.Detailed or VisitorConfigurationOptions.OnlyLaunchPoints)
            {
                Indent();
            }
            action();
            if (this.options is VisitorConfigurationOptions.Detailed or VisitorConfigurationOptions.OnlyLaunchPoints)
            {
                OutDent();
            }
        }

        private static string TruncatePath(string path)
        {
            return path.Truncate(MaxFilePathLength, Truncator.FixedLength, TruncateFrom.Left);
        }

        private static string JsonEscape(string value)
        {
            if (value.HasNoValue())
            {
                return null;
            }

            return JsonEncodedText.Encode(value).ToString();
        }
    }

    internal static class VisitorExtensions
    {
        internal static bool HasAnyDescendantLaunchPoints(this IReadOnlyList<Element> elements)
        {
            return elements.Any(element => element.HasAnyDescendantLaunchPoints());
        }

        internal static bool HasAnyDescendantLaunchPoints(this Element element)
        {
            var isLaunching = element.Automation.Any(auto => auto.IsLaunching);
            if (isLaunching)
            {
                return true;
            }

            return element.Elements.HasAnyDescendantLaunchPoints();
        }
    }
}