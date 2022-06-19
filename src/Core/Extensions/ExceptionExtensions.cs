using System;
using System.Text;

namespace Automate.Extensions
{
    public static class ExceptionExtensions
    {
        public static string ToMessages(this Exception ex, bool indented = false)
        {
            ex.GuardAgainstNull(nameof(ex));

            var messages = new StringBuilder();

            AddMessageOnDescendantException(ex, 0);
            return messages.ToString().TrimEnd(Environment.NewLine.ToCharArray());

            void AddMessageOnDescendantException(Exception exception, int indentLevel)
            {
                if (indented)
                {
                    messages.Append(new string('\t', indentLevel));
                }
                messages.AppendLine(exception.Message);

                if (exception.InnerException.Exists())
                {
                    AddMessageOnDescendantException(exception.InnerException, indented
                        ? indentLevel + 1
                        : indentLevel);
                }
            }
        }
    }
}