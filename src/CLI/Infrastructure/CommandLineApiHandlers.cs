using System.Collections.Generic;

namespace Automate.CLI.Infrastructure
{
    internal partial class CommandLineApi
    {
        internal abstract class HandlerBase
        {
            private static List<OutputMessage> outputMessages;

            internal static void Initialise(List<OutputMessage> messages)
            {
                outputMessages = messages;
            }

            protected static void Output(string messageTemplate, params object[] args)
            {
                outputMessages.Add(new OutputMessage(OutputMessageLevel.Information, messageTemplate, args));
            }

            protected static void OutputWarning(string messageTemplate, params object[] args)
            {
                outputMessages.Add(new OutputMessage(OutputMessageLevel.Warning, messageTemplate, args));
            }

            protected static void OutputError(string messageTemplate, params object[] args)
            {
                outputMessages.Add(new OutputMessage(OutputMessageLevel.Error, messageTemplate, args));
            }
        }
    }
}