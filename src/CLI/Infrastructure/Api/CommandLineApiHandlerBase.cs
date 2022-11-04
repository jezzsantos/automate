using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Automate.Common;
using Automate.Common.Domain;

namespace Automate.CLI.Infrastructure.Api
{
    internal partial class CommandLineApi
    {
        [SuppressMessage("ReSharper", "ParameterHidesMember")]
        internal abstract class HandlerBase
        {
            private static List<OutputMessage> messages;

            protected static IRuntimeMetadata Metadata { get; private set; }

            protected static IRecorder Recorder { get; private set; }

            internal static void Initialise(List<OutputMessage> messages, IRecorder recorder,
                IRuntimeMetadata metadata)
            {
                HandlerBase.messages = messages;
                Recorder = recorder;
                Metadata = metadata;
            }

            protected static void Output(string messageTemplate, params object[] args)
            {
                messages.Add(new OutputMessage(OutputMessageLevel.Information, messageTemplate, args));
            }

            protected static void OutputWarning(string messageTemplate, params object[] args)
            {
                messages.Add(new OutputMessage(OutputMessageLevel.Warning, messageTemplate, args));
            }
        }
    }
}