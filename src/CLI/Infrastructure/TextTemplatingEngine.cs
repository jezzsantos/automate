using System.Collections.Generic;
using Automate.CLI.Domain;
using Automate.CLI.Extensions;
using Scriban;

namespace Automate.CLI.Infrastructure
{
    internal class TextTemplatingEngine : ITextTemplatingEngine
    {
        public string Transform(string template, SolutionItem solutionItem)
        {
            solutionItem.GuardAgainstNull(nameof(solutionItem));

            var configuration = solutionItem.GetConfiguration(true);

            return TransformInternal(template, configuration);
        }

        public string Transform(string template, Dictionary<string, object> values)
        {
            values.GuardAgainstNull(nameof(values));

            return TransformInternal(template, values);
        }

        private static string TransformInternal(string template, Dictionary<string, object> configuration)
        {
            var engine = Template.Parse(template);
            return engine.Render(new
            {
                Model = configuration
            });
        }
    }
}