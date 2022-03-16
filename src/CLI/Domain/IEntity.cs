using System.Collections.Generic;

namespace Automate.CLI.Domain
{
    internal interface ICustomizableEntity : IIdentifiableEntity
    {
    }

    internal interface IAutomationContainer
    {
        IReadOnlyList<CodeTemplate> CodeTemplates { get; }

        IReadOnlyList<Automation> Automation { get; }
    }

    internal interface IAutomation : INamedEntity
    {
        CommandExecutionResult Execute(SolutionDefinition solution, SolutionItem target);
    }

    internal interface IElementContainer
    {
        IReadOnlyList<Element> Elements { get; }
    }

    internal interface IAttributeContainer
    {
        IReadOnlyList<Attribute> Attributes { get; }
    }

    internal interface IPatternElement : INamedEntity, ICustomizableEntity, IElementContainer, IAttributeContainer,
        IAutomationContainer
    {
        Attribute AddAttribute(string name, string type, bool isRequired, string defaultValue, List<string> choices);

        Element AddElement(string name, string displayName, string description, bool isCollection, ElementCardinality cardinality);

        CodeTemplate AttachCodeTemplate(string name, string fullPath, string extension);

        Automation AddCodeTemplateCommand(string name, string codeTemplateName, bool isTearOff, string filePath);

        Automation AddCommandLaunchPoint(string name, List<string> commandIds, IPatternElement pattern);

        Automation FindAutomation(string id);

        Attribute DeleteAttribute(string name);

        Element DeleteElement(string name);
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