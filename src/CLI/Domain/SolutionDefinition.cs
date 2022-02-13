using System.Collections.Generic;
using automate.Extensions;
using StringExtensions = ServiceStack.StringExtensions;

namespace automate.Domain
{
    internal class SolutionDefinition
    {
        public SolutionDefinition(string toolkitId, PatternDefinition pattern)
        {
            toolkitId.GuardAgainstNullOrEmpty(nameof(toolkitId));
            pattern.GuardAgainstNull(nameof(pattern));

            Id = IdGenerator.Create();
            ToolkitId = toolkitId;
            Pattern = pattern;
            InitialiseSchema();
        }

        /// <summary>
        ///     For serialization
        /// </summary>
        public SolutionDefinition()
        {
        }

        public PatternDefinition Pattern { get; set; }

        public string ToolkitId { get; set; }

        public string PatternName => Pattern?.Name;

        public string Id { get; set; }

        public SolutionItem Model { get; set; }

        public string GetConfiguration()
        {
            var properties = new Dictionary<string, object>();
            ConvertToDictionary(Model, properties);
            return properties.ToJson();

            object ConvertToDictionary(SolutionItem solutionItem, IDictionary<string, object> props)
            {
                if (solutionItem.IsAttribute || solutionItem.IsValue)
                {
                    return solutionItem.Value;
                }
                if (solutionItem.Properties.HasAny())
                {
                    foreach (var (key, value) in solutionItem.Properties)
                    {
                        props.Add(ConvertName(key), ConvertToDictionary(value, new Dictionary<string, object>()));
                    }
                }
                if (solutionItem.Items.HasAny())
                {
                    var items = new List<object>();
                    solutionItem.Items.ForEach(item =>
                        items.Add(ConvertToDictionary(item, new Dictionary<string, object>())));
                    props.Add(ConvertName(nameof(SolutionItem.Items)), items);
                }

                return props;
            }

            string ConvertName(string name)
            {
                return StringExtensions.ToLowercaseUnderscore(name);
            }
        }

        private void InitialiseSchema()
        {
            Model = new SolutionItem(Pattern);
        }
    }
}