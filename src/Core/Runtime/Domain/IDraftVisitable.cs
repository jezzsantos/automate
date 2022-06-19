namespace Automate.Runtime.Domain
{
    internal interface IDraftVisitable
    {
        bool Accept(IDraftVisitor visitor);
    }

    internal interface IDraftItemVisitable
    {
        bool Accept(IDraftItemVisitor visitor);
    }

    public interface IDraftVisitor : IDraftItemVisitor
    {
        bool VisitDraftEnter(DraftDefinition draft)
        {
            return true;
        }

        bool VisitDraftExit(DraftDefinition draft)
        {
            return true;
        }
    }

    public interface IDraftItemVisitor
    {
        bool VisitPatternEnter(DraftItem item)
        {
            return true;
        }

        bool VisitPatternExit(DraftItem item)
        {
            return true;
        }

        bool VisitElementEnter(DraftItem item)
        {
            return true;
        }

        bool VisitElementExit(DraftItem item)
        {
            return true;
        }

        bool VisitEphemeralCollectionEnter(DraftItem item)
        {
            return true;
        }

        bool VisitEphemeralCollectionExit(DraftItem item)
        {
            return true;
        }

        bool VisitAttributeEnter(DraftItem item)
        {
            return true;
        }

        bool VisitAttributeExit(DraftItem item)
        {
            return true;
        }

        bool Visit(object value)
        {
            return true;
        }
    }
}