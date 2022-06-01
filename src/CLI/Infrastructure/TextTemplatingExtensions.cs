using System;
using Automate.CLI.Extensions;
using Scriban;
using Scriban.Runtime;

namespace Automate.CLI.Infrastructure
{
    internal static class TextTemplatingExtensions
    {
        public static string Transform(this object source, string description, string textTemplate)
        {
            var engine = Template.Parse(textTemplate);

            if (engine.HasErrors)
            {
                var message = engine.Messages
                    .ToMultiLineText(msg => $"({msg.Span.Start},{msg.Span.End}): {msg.Message}");
                throw new AutomateException(
                    ExceptionMessages.TextTemplatingExtensions_HasSyntaxErrors.Format(description, message));
            }

            try
            {
                var sourceData = new ScriptObject();
                sourceData.Import(source);

                var context = new TemplateContext();
                var builtIn = (ScriptObject)context.BuiltinObject["string"];
                builtIn.Import(typeof(CustomScribanStringFunctions));
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