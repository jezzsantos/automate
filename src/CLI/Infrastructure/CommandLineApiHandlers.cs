using System.Collections.Generic;
using Automate.Common.Domain;

namespace Automate.CLI.Infrastructure
{
    internal partial class CommandLineApi
    {
        internal abstract class HandlerBase
        {
            private static List<OutputMessage> outputMessages;
            private static IAssemblyMetadata assemblyMetadata;

            internal static void Initialise(List<OutputMessage> messages, IAssemblyMetadata metadata)
            {
                outputMessages = messages;
                assemblyMetadata = metadata;
            }

            protected static void Output(string messageTemplate, params object[] args)
            {
                outputMessages.Add(new OutputMessage(OutputMessageLevel.Information, messageTemplate, args));
            }

            protected static void OutputWarning(string messageTemplate, params object[] args)
            {
                outputMessages.Add(new OutputMessage(OutputMessageLevel.Warning, messageTemplate, args));
            }

            protected static IAssemblyMetadata GetMetadata()
            {
                return assemblyMetadata;
            }
        }
    }
}