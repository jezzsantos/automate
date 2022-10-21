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
                if (outputStructured)
                {
                    var recorder = GetRecorder();
                    var reportingIds = recorder.GetReportingIds();
                    Output(OutputMessages.CommandLine_Output_Info, metadata.ProductName,
                        metadata.RuntimeVersion.ToString(),
                        new
                        {
                            IsEnabled = collectUsage,
                            reportingIds.MachineId,
                            reportingIds.SessionId
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