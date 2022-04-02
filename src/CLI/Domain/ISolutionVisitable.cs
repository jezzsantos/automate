namespace Automate.CLI.Domain
{
    internal interface ISolutionVisitable
    {
        bool Accept(ISolutionVisitor visitor);
    }

    internal interface ISolutionItemVisitable
    {
        bool Accept(ISolutionItemVisitor visitor);
    }

    internal interface ISolutionVisitor : ISolutionItemVisitor
    {
        bool VisitSolutionEnter(SolutionDefinition solution)
        {
            return true;
        }

        bool VisitSolutionExit(SolutionDefinition solution)
        {
            return true;
        }
    }

    internal interface ISolutionItemVisitor
    {
        bool VisitPatternEnter(SolutionItem item)
        {
            return true;
        }

        bool VisitPatternExit(SolutionItem item)
        {
            return true;
        }

        bool VisitElementEnter(SolutionItem item)
        {
            return true;
        }

        bool VisitElementExit(SolutionItem item)
        {
            return true;
        }

        bool VisitEphemeralCollectionEnter(SolutionItem item)
        {
            return true;
        }

        bool VisitEphemeralCollectionExit(SolutionItem item)
        {
            return true;
        }

        bool VisitAttributeEnter(SolutionItem item)
        {
            return true;
        }

        bool VisitAttributeExit(SolutionItem item)
        {
            return true;
        }

        bool Visit(object value)
        {
            return true;
        }
    }
}