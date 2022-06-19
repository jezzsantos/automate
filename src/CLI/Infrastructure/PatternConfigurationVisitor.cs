using System;
using System.Linq;
using System.Text;
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

    internal class PatternConfigurationVisitor : IPatternVisitor
    {
        private const int MaxFilePathLength = 100;
        private readonly VisitorConfigurationOptions options;
        private readonly StringBuilder output;
        private int indentLevel;
        private bool outputAttributesHeading;
        private bool outputAutomationHeading;
        private bool outputCodeTemplatesHeading;
        private bool outputElementsHeading;

        public PatternConfigurationVisitor(VisitorConfigurationOptions options)
        {
            this.output = new StringBuilder();
            this.options = options;
        }

        public static string TruncateCodeTemplatePath(string path)
        {
            return TruncatePath(path);
        }

        public override string ToString()
        {
            return this.output.ToString();
        }

        public bool VisitPatternEnter(PatternDefinition pattern)
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

            return VisitPatternElementEnter(pattern);
        }

        public bool VisitPatternExit(PatternDefinition pattern)
        {
            return VisitPatternElementExit(pattern);
        }

        public bool VisitElementEnter(Element element)
        {
            if (this.outputElementsHeading)
            {
                PrintIndented("- Elements:");
                this.outputElementsHeading = false;
            }

            if (this.options is VisitorConfigurationOptions.Detailed or VisitorConfigurationOptions.OnlyLaunchPoints)
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

            return VisitPatternElementEnter(element);
        }

        public bool VisitElementExit(Element element)
        {
            if (this.options is VisitorConfigurationOptions.Detailed or VisitorConfigurationOptions.OnlyLaunchPoints)
            {
                OutDent();
            }
            return VisitPatternElementExit(element);
        }

        public bool VisitAttribute(Attribute attribute)
        {
            if (this.options == VisitorConfigurationOptions.OnlyLaunchPoints)
            {
                return true;
            }

            if (this.outputAttributesHeading)
            {
                PrintIndented("- Attributes:");
                this.outputAttributesHeading = false;
            }

            WithDetailedIndents(() =>
            {
                PrintIndented(
                    $"- {attribute.Name}{(this.options == VisitorConfigurationOptions.Detailed ? "" : " (attribute)")} ({attribute.DataType}{(attribute.IsRequired ? ", required" : "")}{(attribute.Choices.HasAny() ? ", oneof: " + $"{attribute.Choices.ToListSafe().Join(";")}" : "")}{(attribute.DefaultValue.HasValue() ? ", default: " + $"{attribute.DefaultValue}" : "")})");
            });

            return true;
        }

        public bool VisitAutomation(Automation automation)
        {
            if (this.outputAutomationHeading)
            {
                if (this.options == VisitorConfigurationOptions.Detailed)
                {
                    PrintIndented("- Automation:");
                }
                if (this.options == VisitorConfigurationOptions.OnlyLaunchPoints)
                {
                    PrintIndented("- LaunchPoints:");
                }
                this.outputAutomationHeading = false;
            }

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
            return true;
        }

        public bool VisitCodeTemplate(CodeTemplate codeTemplate)
        {
            if (this.options == VisitorConfigurationOptions.OnlyLaunchPoints)
            {
                return true;
            }

            if (this.outputCodeTemplatesHeading)
            {
                PrintIndented("- CodeTemplates:");
                this.outputCodeTemplatesHeading = false;
            }

            WithDetailedIndents(() =>
            {
                if (this.options == VisitorConfigurationOptions.Detailed)
                {
                    PrintIndented(
                        $"- {codeTemplate.Name} [{codeTemplate.Id}] (original: {TruncatePath(codeTemplate.Metadata.OriginalFilePath)})");
                }
            });

            return true;
        }

        private void PrintEndOfLine()
        {
            this.output.Append(Environment.NewLine);
        }

        private bool VisitPatternElementEnter(PatternElement element)
        {
            this.outputCodeTemplatesHeading =
                this.options == VisitorConfigurationOptions.Detailed && element.CodeTemplates.HasAny();
            this.outputAutomationHeading =
                (this.options == VisitorConfigurationOptions.Detailed && element.Automation.HasAny())
                || (this.options == VisitorConfigurationOptions.OnlyLaunchPoints &&
                    element.Automation.Safe().Any(auto => auto.IsLaunching()));
            this.outputAttributesHeading =
                this.options == VisitorConfigurationOptions.Detailed && element.Attributes.HasAny();
            this.outputElementsHeading =
                this.options is VisitorConfigurationOptions.Detailed or VisitorConfigurationOptions.OnlyLaunchPoints &&
                element.Elements.HasAny();

            Indent();

            return true;
        }

        private bool VisitPatternElementExit(PatternElement element)
        {
            OutDent();
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
    }
}