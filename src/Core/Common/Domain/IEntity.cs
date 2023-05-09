using System.Collections.Generic;
using Automate.Authoring.Domain;

namespace Automate.Common.Domain
{
    public interface ICustomizableEntity : IIdentifiableEntity
    {
    }

    public interface IAutomationContainer
    {
        IReadOnlyList<CodeTemplate> CodeTemplates { get; }

        IReadOnlyList<Automation> Automation { get; }
    }

    public interface IAutomation : INamedEntity
    {
        AutomationType Type { get; }

        bool IsLaunchable { get; }
    }

    public interface IElementContainer
    {
        IReadOnlyList<Element> Elements { get; }
    }

    public interface IAttributeContainer
    {
        IReadOnlyList<Attribute> Attributes { get; }
    }

    public interface IPatternElement : INamedEntity, ICustomizableEntity, IElementContainer, IAttributeContainer,
        IAutomationContainer
    {
        void RenameAndDescribe(string name, string displayName, string description);

        Attribute AddAttribute(string name, string type, bool isRequired, string defaultValue, List<string> choices);

        Attribute UpdateAttribute(string attributeName, string name, string type, bool? isRequired, string defaultValue,
            List<string> choices);

        Attribute DeleteAttribute(string name);

        Element AddElement(string name, ElementCardinality cardinality, bool autoCreate, string displayName,
            string description);

        Element UpdateElement(string elementName, string name, bool? isRequired, bool? autoCreate, string displayName,
            string description);

        Element DeleteElement(string name);

        CodeTemplate AddCodeTemplate(string name, string fullPath, string extension);

        CodeTemplate DeleteCodeTemplate(string name, bool includeReferencingAutomation);

        Automation AddCodeTemplateCommand(string name, string codeTemplateName, bool isOneOff, string filePath);

        Automation UpdateCodeTemplateCommand(string commandName, string name, bool? isOneOff, string filePath);

        public void DeleteCodeTemplateCommand(string id, bool includeReferencingAutomation);

        Automation AddCliCommand(string name, string applicationName, string arguments);

        Automation UpdateCliCommand(string commandName, string name, string applicationName, string arguments);

        public void DeleteCliCommand(string id, bool includeReferencingAutomation);

        Automation DeleteAutomation(string name);

        Automation AddCommandLaunchPoint(string name, List<string> commandIds, IPatternElement sourceElement);

        Automation UpdateCommandLaunchPoint(string launchPointName, string name, List<string> addCommandIds,
            List<string> removeCommandIds, IPatternElement sourceElement);

        public void DeleteCommandLaunchPoint(string id);

        CodeTemplate FindCodeTemplateByName(string name);

        CodeTemplateCommand FindCodeTemplateCommandByName(string name);
    }

    public interface INamedEntity : IIdentifiableEntity
    {
        string Name { get; }
    }

    public interface IIdentifiableEntity
    {
        string Id { get; }
    }

    public enum AutomationType
    {
        Unknown = 0,
        CodeTemplateCommand = 1,
        CliCommand = 2,
        CommandLaunchPoint = 10,
#if TESTINGONLY
        TestingOnlyLaunchable = 100,
        TestingOnlyLaunching = 101
#endif
    }
}