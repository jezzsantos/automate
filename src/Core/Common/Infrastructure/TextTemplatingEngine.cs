using System.Collections;
using Automate.Common.Application;
using Automate.Common.Extensions;
using Automate.Runtime.Domain;

namespace Automate.Common.Infrastructure
{
    public class TextTemplatingEngine : ITextTemplatingEngine
    {
        public string Transform(string description, string textTemplate, DraftItem draftItem)
        {
            draftItem.GuardAgainstNull(nameof(draftItem));
            description.GuardAgainstNullOrEmpty(nameof(description));

            var configuration = draftItem.GetConfiguration(true, false);

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