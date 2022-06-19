using System.Collections.Generic;
using Automate.Common.Domain;
using Automate.Runtime.Domain;

namespace Automate.Authoring.Domain
{
    public interface ISchema
    {
    }

    public interface IPatternElementSchema : ISchema
    {
        string Id { get; }

        string Name { get; }

        IReadOnlyList<IAutomationSchema> Automation { get; }

        IReadOnlyList<IAttributeSchema> Attributes { get; }

        IReadOnlyList<IElementSchema> Elements { get; }

        ICodeTemplateSchema FindCodeTemplateById(string codeTemplateId);
    }

    public interface IPatternSchema : IPatternElementSchema
    {
        IAutomationSchema FindAutomationById(string id);
    }

    public interface IElementSchema : IPatternElementSchema
    {
        bool IsCollection { get; }

        Element Element { get; }

        bool HasCardinalityOfAtLeastOne();

        bool HasCardinalityOfAtMostOne();

        bool HasCardinalityOfMany();

        bool ShouldAutoCreate();
    }

    public interface IAttributeSchema : ISchema
    {
        string DataType { get; }

        string Name { get; }

        IReadOnlyList<string> Choices { get; }

        string DefaultValue { get; }

        string Id { get; }

        Attribute Attribute { get; }

        bool IsValidDataType(string value);

        IReadOnlyList<ValidationResult> Validate(ValidationContext context, object value);
    }

    public interface IAutomationSchema
    {
        string Name { get; }

        string Id { get; }

        Automation Automation { get; }

        IAutomation GetExecutable(DraftDefinition draft, DraftItem draftItem);

        bool IsLaunching();
    }

    public interface ICodeTemplateSchema
    {
        string Id { get; }
    }
}