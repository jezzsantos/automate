using Automate.CLI.Domain;
using Scriban;

namespace Automate.CLI.Infrastructure
{
    internal class TextTemplatingEngine : ITextTemplatingEngine
    {
        public string Transform(string template, SolutionItem solutionItem)
        {
            var engine = Template.Parse(template);

            var configuration = solutionItem.GetConfiguration(true);
            return engine.Render(new
            {
                Model = configuration
            });
        }
    }
}