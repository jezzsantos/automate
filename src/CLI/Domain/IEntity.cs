using System.Collections.Generic;

namespace Automate.CLI.Domain
{
    internal interface ICustomizableEntity : IIdentifiableEntity
    {
    }

    internal interface IAutomationContainer
    {
        List<CodeTemplate> CodeTemplates { get; }

        List<Automation> Automation { get; }
    }

    internal interface IAutomation : INamedEntity
    {
        CommandExecutionResult Execute(SolutionDefinition solution, SolutionItem target);
    }

    internal interface IElementContainer
    {
        List<Element> Elements { get; }
    }

    internal interface IAttributeContainer
    {
        List<Attribute> Attributes { get; }
    }

    internal interface IPatternElement : INamedEntity, ICustomizableEntity, IElementContainer, IAttributeContainer,
        IAutomationContainer
    {
    }

    internal interface INamedEntity : IIdentifiableEntity
    {
        string Name { get; }
    }

    internal interface IIdentifiableEntity
    {
        string Id { get; }
    }
}