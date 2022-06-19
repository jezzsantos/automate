using System;
using System.Reflection;
using Automate.Common.Extensions;
using Scriban;
using Scriban.Runtime;

namespace Automate.Common.Infrastructure
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
                    ExceptionMessages.TextTemplatingExtensions_HasSyntaxErrors.Substitute(description, message));
            }

            try
            {
                var context = new TemplateContext();
                AddStringFunctionsToBuiltIn(context);
                context.PushGlobal(CreateScriptForSource(source));

                return engine.Render(context);
            }
            catch (Exception ex)
            {
                throw new AutomateException(
                    ExceptionMessages.TextTemplatingExtensions_TransformFailed.Substitute(description, ex.Message));
            }

            static void AddStringFunctionsToBuiltIn(TemplateContext context)
            {
                var builtIn = (ScriptObject)context.BuiltinObject["string"];
                builtIn.Import(typeof(CustomScribanStringFunctions));
            }

            static IScriptObject CreateScriptForSource(object source)
            {
                var script = new ScriptObject();
                script.Import(source, null, AsIsMemberName);

                return script;
            }

            static string AsIsMemberName(MemberInfo member)
            {
                return member.Name;
            }
        }
    }
}