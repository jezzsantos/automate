using System;
using Automate.CLI.Extensions;
using Scriban;
using Scriban.Runtime;

namespace Automate.CLI.Infrastructure
{
    internal static class TextTemplatingExtensions
    {
        private const string FunctionQualifyingNamespace = "automate";

        public static string Transform(this object source, string description, string textTemplate,
            bool modelPrefix = false)
        {
            var engine = Template.Parse(textTemplate);

            var model = modelPrefix
                ? new { Model = source }
                : source;

            if (engine.HasErrors)
            {
                var message = engine.Messages
                    .ToMultiLineText(msg => $"({msg.Span.Start},{msg.Span.End}): {msg.Message}");
                throw new AutomateException(
                    ExceptionMessages.TextTemplatingExtensions_HasSyntaxErrors.Format(description, message));
            }

            try
            {
                var customFunctions = new ScriptObject();
                customFunctions.Import(typeof(CustomScribanStringFunctions));

                var stringFunctions = new ScriptObject();
                stringFunctions.SetValue(FunctionQualifyingNamespace, customFunctions, true);

                var sourceData = new ScriptObject();
                sourceData.Import(model);

                var context = new TemplateContext();
                context.PushGlobal(customFunctions);
                context.PushGlobal(stringFunctions);
                context.PushGlobal(sourceData);

                return engine.Render(context);
            }
            catch (Exception ex)
            {
                throw new AutomateException(
                    ExceptionMessages.TextTemplatingExtensions_TransformFailed.Format(description, ex.Message));
            }
        }
    }
}