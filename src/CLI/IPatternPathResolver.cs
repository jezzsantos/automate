namespace automate
{
    internal interface IPatternPathResolver
    {
        IElementContainer Resolve(PatternMetaModel pattern, string expression);
    }
}