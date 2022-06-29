using System.Collections.Generic;

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

        bool VisitElementsEnter(IReadOnlyList<Element> elements)
        {
            return true;
        }

        bool VisitElementsExit(IReadOnlyList<Element> elements)
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

        bool VisitAttributesEnter(IReadOnlyList<Attribute> attributes)
        {
            return true;
        }

        bool VisitAttributesExit(IReadOnlyList<Attribute> attributes)
        {
            return true;
        }

        bool VisitAttribute(Attribute attribute)
        {
            return true;
        }

        bool VisitAutomationsEnter(IReadOnlyList<Automation> automation)
        {
            return true;
        }

        bool VisitAutomationsExit(IReadOnlyList<Automation> automation)
        {
            return true;
        }

        bool VisitAutomation(Automation automation)
        {
            return true;
        }

        bool VisitCodeTemplatesEnter(IReadOnlyList<CodeTemplate> codeTemplates)
        {
            return true;
        }

        bool VisitCodeTemplatesExit(IReadOnlyList<CodeTemplate> codeTemplates)
        {
            return true;
        }

        bool VisitCodeTemplate(CodeTemplate codeTemplate)
        {
            return true;
        }
    }
}