using JetBrains.Annotations;

namespace Automate.CLI.Infrastructure
{
    internal partial class CommandLineApi
    {
        [UsedImplicitly]
        internal class AdministrativeApiHandlers : HandlerBase
        {
            internal static void Info(bool collectUsage, bool outputStructured)
            {
                var metadata = GetMetadata();
                var recorder = GetRecorder();
                if (outputStructured)
                {
                    Output(OutputMessages.CommandLine_Output_Info, metadata.ProductName,
                        metadata.RuntimeVersion.ToString(),
                        new
                        {
                            IsEnabled = collectUsage,
                            UserId = recorder.GetUserId()
                        });
                }
                else
                {
                    Output(OutputMessages.CommandLine_Output_Info, metadata.ProductName,
                        metadata.RuntimeVersion.ToString(),
                        collectUsage);
                }
            }
        }
    }
}