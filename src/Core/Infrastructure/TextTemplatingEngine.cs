using System.Collections;
using Automate.Application;
using Automate.Domain;
using Automate.Extensions;

namespace Automate.Infrastructure
{
    public class TextTemplatingEngine : ITextTemplatingEngine
    {
        public string Transform(string description, string textTemplate, DraftItem draftItem)
        {
            draftItem.GuardAgainstNull(nameof(draftItem));
            description.GuardAgainstNullOrEmpty(nameof(description));

            var configuration = draftItem.GetConfiguration(true);

            return configuration.Transform(description, textTemplate);
        }

        public string Transform(string description, string textTemplate, IDictionary values)
        {
            values.GuardAgainstNull(nameof(values));
            description.GuardAgainstNullOrEmpty(nameof(description));

            return values.Transform(description, textTemplate);
        }
    }
}