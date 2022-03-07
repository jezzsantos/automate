using System.Collections.Generic;

namespace Automate.CLI.Domain
{
    internal interface ITextTemplatingEngine
    {
        string Transform(string template, SolutionItem solutionItem);

        string Transform(string template, Dictionary<string, object> values);
    }
}