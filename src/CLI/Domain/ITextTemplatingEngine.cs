using System.Collections;

namespace Automate.CLI.Domain
{
    internal interface ITextTemplatingEngine
    {
        string Transform(string description, string textTemplate, SolutionItem solutionItem);

        string Transform(string description, string textTemplate, IDictionary values);
    }
}