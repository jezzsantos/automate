namespace Automate.Authoring.Domain
{
    internal interface IPatternVisitable
    {
        bool Accept(IPatternVisitor visitor);
    }

    public interface IPatternVisitor
    {
        bool VisitPatternEnter(PatternDefinition pattern)
        {
            return true;
        }

        bool VisitPatternExit(PatternDefinition pattern)
        {
            return true;
        }

        bool VisitElementEnter(Element element)
        {
            return true;
        }

        bool VisitElementExit(Element element)
        {
            return true;
        }

        bool VisitAttribute(Attribute attribute)
        {
            return true;
        }

        bool VisitAutomation(Automation automation)
        {
            return true;
        }

        bool VisitCodeTemplate(CodeTemplate codeTemplate)
        {
            return true;
        }
    }
}