namespace automate.Domain
{
    internal interface ITextTemplatingEngine
    {
        string Transform(string template, SolutionItem solutionItem);
    }
}