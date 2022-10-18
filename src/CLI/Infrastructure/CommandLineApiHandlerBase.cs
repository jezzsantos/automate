using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Automate.Common;
using Automate.Common.Domain;

namespace Automate.CLI.Infrastructure
{
    internal partial class CommandLineApi
    {
        [SuppressMessage("ReSharper", "ParameterHidesMember")]
        internal abstract class HandlerBase
        {
            private static List<OutputMessage> messages;
            private static IAssemblyMetadata metadata;
            private static IRecorder recorder;

            internal static void Initialise(List<OutputMessage> messages, IRecorder recorder,
                IAssemblyMetadata metadata)
            {
                HandlerBase.messages = messages;
                HandlerBase.recorder = recorder;
                HandlerBase.metadata = metadata;
            }

            protected static void Output(string messageTemplate, params object[] args)
            {
                messages.Add(new OutputMessage(OutputMessageLevel.Information, messageTemplate, args));
            }

            protected static void OutputWarning(string messageTemplate, params object[] args)
            {
                messages.Add(new OutputMessage(OutputMessageLevel.Warning, messageTemplate, args));
            }

            protected static IAssemblyMetadata GetMetadata()
            {
                return metadata;
            }

            protected static IRecorder GetRecorder()
            {
                return recorder;
            }
        }
    }
}