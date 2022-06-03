namespace Automate.CLI.Domain
{
    internal interface IDraftPathResolver
    {
        DraftItem ResolveItem(DraftDefinition draft, string expression);

        string ResolveExpression(string description, string expression, DraftItem draftItem);
    }
}