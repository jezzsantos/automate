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
        void RenameAndDescribe(string name, string displayName, string description);
        
        Attribute AddAttribute(string name, string type, bool isRequired, string defaultValue, List<string> choices);

        Attribute UpdateAttribute(string attributeName, string name, string type, bool? isRequired, string defaultValue,
            List<string> choices);

        Attribute DeleteAttribute(string name);

        Element AddElement(string name, ElementCardinality cardinality, string displayName, string description);

        Element UpdateElement(string elementName, string name, bool? isRequired, string displayName,
            string description);

        Element DeleteElement(string name);

        CodeTemplate AddCodeTemplate(string name, string fullPath, string extension);

        CodeTemplate DeleteCodeTemplate(string name, bool includeReferencingAutomation);

        Automation AddCodeTemplateCommand(string name, string codeTemplateName, bool isTearOff, string filePath);

        Automation UpdateCodeTemplateCommand(string commandName, string name, bool? isTearOff, string filePath);

        public void DeleteCodeTemplateCommand(string id, bool includeReferencingAutomation);

        Automation AddCliCommand(string name, string applicationName, string arguments);

        Automation UpdateCliCommand(string commandName, string name, string applicationName, string arguments);

        public void DeleteCliCommand(string id, bool includeReferencingAutomation);

        Automation DeleteAutomation(string name);

        Automation AddCommandLaunchPoint(string name, List<string> commandIds);

        Automation UpdateCommandLaunchPoint(string launchPointName, string name, List<string> commandIds, IPatternElement sourceElement);

        public void DeleteCommandLaunchPoint(string id);

        CodeTemplate FindCodeTemplateByName(string name);
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