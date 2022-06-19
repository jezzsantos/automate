namespace Automate.Runtime.Domain
{
    public interface IDraftPathResolver
    {
        DraftItem ResolveItem(DraftDefinition draft, string expression);

        string ResolveExpression(string description, string expression, DraftItem draftItem);
    }
}