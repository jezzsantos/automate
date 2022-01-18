namespace automate
{
    internal interface IPatternPathResolver
    {
        IPatternElement Resolve(PatternMetaModel pattern, string expression);
    }
}