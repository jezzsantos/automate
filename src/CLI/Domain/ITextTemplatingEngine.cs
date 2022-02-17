namespace Automate.CLI.Domain
{
    internal interface ITextTemplatingEngine
    {
        string Transform(string template, SolutionItem solutionItem);
    }
}