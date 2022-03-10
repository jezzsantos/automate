using System.Collections;
using Automate.CLI.Domain;
using Automate.CLI.Extensions;

namespace Automate.CLI.Infrastructure
{
    internal class TextTemplatingEngine : ITextTemplatingEngine
    {
        public string Transform(string description, string textTemplate, SolutionItem solutionItem)
        {
            solutionItem.GuardAgainstNull(nameof(solutionItem));
            description.GuardAgainstNullOrEmpty(nameof(description));

            var configuration = solutionItem.GetConfiguration(true);

            return configuration.Transform(description, textTemplate, true);
        }

        public string Transform(string description, string textTemplate, IDictionary values)
        {
            values.GuardAgainstNull(nameof(values));
            description.GuardAgainstNullOrEmpty(nameof(description));

            return values.Transform(description, textTemplate, true);
        }
    }
}