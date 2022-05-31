using System;
using Automate.CLI.Extensions;
using Scriban;
using Scriban.Runtime;

namespace Automate.CLI.Infrastructure
{
    internal static class TextTemplatingExtensions
    {
        private const string LegacyFunctionNamespace = "automate";

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
                var legacyFunctions = new ScriptObject();
                legacyFunctions.Import(typeof(CustomScribanStringFunctions),
                    member => member.Name is nameof(CustomScribanStringFunctions.Pascalcase)
                        or nameof(CustomScribanStringFunctions.Camelcase));

                //HACK: Support legacy functions in legacy namespace for next few versions of existing toolkits.
                var backwardsCompatibleFunctions = new ScriptObject();
                backwardsCompatibleFunctions.SetValue(LegacyFunctionNamespace, legacyFunctions, true);

                var sourceData = new ScriptObject();
                sourceData.Import(model);

                var context = new TemplateContext();
                var builtIn = (ScriptObject)context.BuiltinObject["string"];
                builtIn.Import(typeof(CustomScribanStringFunctions));
                context.PushGlobal(backwardsCompatibleFunctions);
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