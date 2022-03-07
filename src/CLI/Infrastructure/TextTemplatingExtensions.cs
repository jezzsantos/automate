using System.Collections.Generic;
using System.Linq;
using Automate.CLI.Extensions;
using Scriban;

namespace Automate.CLI.Infrastructure
{
    internal static class TextTemplatingExtensions
    {
        public static string Transform(this Dictionary<string, object> source, string description, string textTemplate, bool modelPrefix = false)
        {
            var engine = Template.Parse(textTemplate);

            var model = modelPrefix
                ? new { Model = source }
                : (object)source;

            if (engine.HasErrors)
            {
                var message = engine.Messages
                    .Select(msg => $"({msg.Span.Start},{msg.Span.End}): {msg.Message}")
                    .Join("\n");
                throw new AutomateException(ExceptionMessages.TextTemplatingExtensions_HasErrors.Format(description, message));
            }

            return engine.Render(model);
        }
    }
}