using System;
using System.Text;
using Automate.CLI.Domain;
using Automate.CLI.Extensions;
using Attribute = Automate.CLI.Domain.Attribute;

namespace Automate.CLI.Infrastructure
{
    internal class PatternConfigurationVisitor : IPatternVisitor
    {
        private readonly bool isDetailed;
        private readonly StringBuilder output;
        private int indentLevel;
        private bool outputAttributesHeading;
        private bool outputAutomationHeading;
        private bool outputCodeTemplatesHeading;
        private bool outputElementsHeading;

        public PatternConfigurationVisitor(bool isDetailed)
        {
            this.output = new StringBuilder();
            this.isDetailed = isDetailed;
        }

        public override string ToString()
        {
            return this.output.ToString();
        }

        public bool VisitPatternEnter(PatternDefinition pattern)
        {
            PrintIndented($"- {pattern.Name}", false);
            if (this.isDetailed)
            {
                PrintInline($" [{pattern.Id}]");
            }
            PrintInline(" (root element)");
            if (!this.isDetailed && pattern.CodeTemplates.HasAny())
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

            if (this.isDetailed)
            {
                Indent();
            }

            PrintIndented($"- {element.Name}", false);
            if (this.isDetailed)
            {
                PrintInline($" [{element.Id}]");
            }
            PrintInline($" ({(element.IsCollection ? "collection" : "element")})");
            if (!this.isDetailed && element.CodeTemplates.HasAny())
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
            if (this.isDetailed)
            {
                OutDent();
            }
            return VisitPatternElementExit(element);
        }

        public bool VisitAttribute(Attribute attribute)
        {
            if (this.outputAttributesHeading)
            {
                PrintIndented("- Attributes:");
                this.outputAttributesHeading = false;
            }

            WithDetailedIndents(() =>
            {
                PrintIndented(
                    $"- {attribute.Name}{(this.isDetailed ? "" : " (attribute)")} ({attribute.DataType}{(attribute.IsRequired ? ", required" : "")}{(attribute.Choices.HasAny() ? ", oneof: " + $"{attribute.Choices.ToListSafe().Join(";")}" : "")}{(attribute.DefaultValue.HasValue() ? ", default: " + $"{attribute.DefaultValue}" : "")})");
            });

            return true;
        }

        public bool VisitAutomation(Automation automation)
        {
            if (this.outputAutomationHeading)
            {
                PrintIndented("- Automation:");
                this.outputAutomationHeading = false;
            }

            WithDetailedIndents(() =>
            {
                if (this.isDetailed)
                {
                    PrintIndented($"- {automation.Name} [{automation.Id}] ({automation.Type})", false);
                    if (automation.Type == AutomationType.CodeTemplateCommand)
                    {
                        PrintInline(
                            $" (template: {automation.Metadata[nameof(CodeTemplateCommand.CodeTemplateId)]}, oneOff: {automation.Metadata[nameof(CodeTemplateCommand.IsOneOff)].ToString()!.ToLower()}, path: {automation.Metadata[nameof(CodeTemplateCommand.FilePath)]})",
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
            });
            return true;
        }

        public bool VisitCodeTemplate(CodeTemplate codeTemplate)
        {
            if (this.outputCodeTemplatesHeading)
            {
                PrintIndented("- CodeTemplates:");
                this.outputCodeTemplatesHeading = false;
            }

            WithDetailedIndents(() =>
            {
                if (this.isDetailed)
                {
                    PrintIndented(
                        $"- {codeTemplate.Name} [{codeTemplate.Id}] (file: {codeTemplate.Metadata.OriginalFilePath}, ext: {codeTemplate.Metadata.OriginalFileExtension})");
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
            this.outputCodeTemplatesHeading = this.isDetailed && element.CodeTemplates.HasAny();
            this.outputAutomationHeading = this.isDetailed && element.Automation.HasAny();
            this.outputAttributesHeading = this.isDetailed && element.Attributes.HasAny();
            this.outputElementsHeading = this.isDetailed && element.Elements.HasAny();

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
            if (this.isDetailed)
            {
                Indent();
            }
            action();
            if (this.isDetailed)
            {
                OutDent();
            }
        }
    }
}