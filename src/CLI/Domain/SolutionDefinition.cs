using System;
using System.Linq;
using Automate.CLI.Extensions;

namespace Automate.CLI.Domain
{
    internal class SolutionDefinition : INamedEntity
    {
        public SolutionDefinition(ToolkitDefinition toolkit, string name = null)
        {
            toolkit.GuardAgainstNull(nameof(toolkit));

            Id = IdGenerator.Create();
            Name = name.HasValue()
                ? name
                : $"{toolkit.PatternName}{GetRandomNumber()}";
            Toolkit = toolkit;
            InitialiseSchema();
        }

        /// <summary>
        ///     For serialization
        /// </summary>
        public SolutionDefinition()
        {
        }

        public ToolkitDefinition Toolkit { get; set; }

        public string PatternName => Toolkit.Pattern?.Name;

        public SolutionItem Model { get; set; }

        public string GetConfiguration()
        {
            return Model.GetConfiguration(false).ToJson();
        }

        public (IAutomation Automation, SolutionItem SolutionItem) FindByAutomation(string automationId)
        {
            return FindDescendantAutomation(Model);

            (IAutomation Automation, SolutionItem SolutionItem) FindDescendantAutomation(SolutionItem solutionItem)
            {
                if (solutionItem.IsPattern)
                {
                    var automation = solutionItem.PatternSchema.Automation.Safe()
                        .FirstOrDefault(auto => auto.Id.EqualsIgnoreCase(automationId));
                    if (automation.Exists())
                    {
                        return (automation, solutionItem);
                    }
                }
                if (solutionItem.IsElement || solutionItem.IsCollection)
                {
                    var automation = solutionItem.ElementSchema.Automation.Safe()
                        .FirstOrDefault(auto => auto.Id.EqualsIgnoreCase(automationId));
                    if (automation.Exists())
                    {
                        return (automation, solutionItem);
                    }
                }
                if (solutionItem.IsAttribute || solutionItem.IsValue)
                {
                }

                foreach (var (_, value) in solutionItem.Properties.Safe())
                {
                    var result = FindDescendantAutomation(value);
                    if (result.Automation.Exists())
                    {
                        return result;
                    }
                }
                foreach (var item in solutionItem.Items.Safe())
                {
                    var result = FindDescendantAutomation(item);
                    if (result.Automation.Exists())
                    {
                        return result;
                    }
                }

                return default;
            }
        }

        public SolutionItem FindByCodeTemplate(string codeTemplateId)
        {
            return FindDescendantCodeTemplate(Model);

            SolutionItem FindDescendantCodeTemplate(SolutionItem solutionItem)
            {
                if (solutionItem.IsPattern)
                {
                    var codeTemplate = solutionItem.PatternSchema.CodeTemplates.Safe()
                        .FirstOrDefault(temp => temp.Id.EqualsIgnoreCase(codeTemplateId));
                    if (codeTemplate.Exists())
                    {
                        return solutionItem;
                    }
                }
                if (solutionItem.IsElement || solutionItem.IsCollection)
                {
                    var codeTemplate = solutionItem.ElementSchema.CodeTemplates.Safe()
                        .FirstOrDefault(tem => tem.Id.EqualsIgnoreCase(codeTemplateId));
                    if (codeTemplate.Exists())
                    {
                        return solutionItem;
                    }
                }
                if (solutionItem.IsAttribute || solutionItem.IsValue)
                {
                }

                foreach (var (_, value) in solutionItem.Properties.Safe())
                {
                    var result = FindDescendantCodeTemplate(value);
                    if (result.Exists())
                    {
                        return result;
                    }
                }
                foreach (var item in solutionItem.Items.Safe())
                {
                    var result = FindDescendantCodeTemplate(item);
                    if (result.Exists())
                    {
                        return result;
                    }
                }

                return default;
            }
        }

        public void PopulateAncestryAfterDeserialization()
        {
            PopulateDescendantParents(Model, null);

            void PopulateDescendantParents(SolutionItem solutionItem, SolutionItem parent)
            {
                solutionItem.Parent = parent;
                var properties = solutionItem.Properties.Safe();
                foreach (var property in properties)
                {
                    PopulateDescendantParents(property.Value, solutionItem);
                }
                var items = solutionItem.Items.Safe();
                foreach (var item in items)
                {
                    PopulateDescendantParents(item, solutionItem);
                }
            }
        }

        public string Id { get; set; }

        public string Name { get; set; }

        private static string GetRandomNumber()
        {
            var number = DateTime.Now.Ticks.ToString();
            return number.Substring(number.Length - 3);
        }

        private void InitialiseSchema()
        {
            Model = new SolutionItem(Toolkit.Pattern);
        }
    }
}