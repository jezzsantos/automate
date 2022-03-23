using System.Collections.Generic;

namespace Automate.CLI.Domain
{
    internal interface ISchema
    {
    }

    internal interface IPatternElementSchema : ISchema
    {
        string Id { get; }

        string Name { get; }

        IReadOnlyList<IAutomationSchema> Automation { get; }

        IReadOnlyList<IAttributeSchema> Attributes { get; }

        IReadOnlyList<IElementSchema> Elements { get; }

        ICodeTemplateSchema FindCodeTemplateById(string codeTemplateId);
    }

    internal interface IPatternSchema : IPatternElementSchema
    {
        IAutomationSchema FindAutomationById(string id);
    }

    internal interface IElementSchema : IPatternElementSchema
    {
        bool IsCollection { get; }

        Element Object { get; }

        bool HasCardinalityOfAtLeastOne();

        bool HasCardinalityOfAtMostOne();

        bool HasCardinalityOfMany();
    }

    internal interface IAttributeSchema : ISchema
    {
        string DataType { get; }

        string Name { get; }

        IReadOnlyList<string> Choices { get; }

        string DefaultValue { get; }

        string Id { get; }

        Attribute Object { get; }

        bool IsValidDataType(string value);

        IReadOnlyList<ValidationResult> Validate(ValidationContext context, object value);
    }

    internal interface IAutomationSchema
    {
        string Name { get; }

        string Id { get; }

        Automation Object { get; }

        CommandExecutionResult Execute(SolutionDefinition solution, SolutionItem solutionItem);
    }

    internal interface ICodeTemplateSchema
    {
        string Id { get; }
    }
}