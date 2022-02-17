using automate.Domain;
using Scriban;

namespace automate.Infrastructure
{
    internal class TextTemplatingEngine : ITextTemplatingEngine
    {
        public string Transform(string template, SolutionItem solutionItem)
        {
            var engine = Template.Parse(template);
            return engine.Render(new
            {
                Model = solutionItem.GetConfiguration()
            });
        }
    }
}