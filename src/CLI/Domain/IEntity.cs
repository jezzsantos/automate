﻿using System.Collections.Generic;

namespace Automate.CLI.Domain
{
    internal interface ICustomizableEntity : IIdentifiableEntity
    {
    }

    internal interface IAutomationContainer
    {
        List<CodeTemplate> CodeTemplates { get; set; }

        List<IAutomation> Automation { get; set; }
    }

    internal interface IAutomation : INamedEntity
    {
        CommandExecutionResult Execute(ToolkitDefinition toolkit, SolutionItem ownerSolution);
    }

    internal interface IElementContainer
    {
        List<Element> Elements { get; set; }
    }

    internal interface IAttributeContainer
    {
        List<Attribute> Attributes { get; set; }
    }

    internal interface IPatternElement : INamedEntity, ICustomizableEntity, IElementContainer, IAttributeContainer,
        IAutomationContainer
    {
    }

    internal interface INamedEntity : IIdentifiableEntity
    {
        string Name { get; set; }
    }

    internal interface IIdentifiableEntity
    {
        string Id { get; set; }
    }
}