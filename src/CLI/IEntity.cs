using System.Collections.Generic;

namespace automate
{
    internal interface ICustomizableEntity : IIdentifiableEntity
    {
    }

    internal interface IAutomationContainer : IIdentifiableEntity
    {
        List<CodeTemplate> CodeTemplates { get; set; }
    }

    internal interface IElementContainer : IAttributeContainer
    {
        List<Element> Elements { get; set; }
    }

    internal interface IAttributeContainer : IIdentifiableEntity
    {
        List<Attribute> Attributes { get; set; }
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