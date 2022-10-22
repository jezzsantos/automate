using Automate.CLI.Extensions;
using JetBrains.Annotations;

namespace Automate.CLI.Infrastructure
{
    internal partial class CommandLineApi
    {
        [UsedImplicitly]
        internal class AdministrativeApiHandlers : HandlerBase
        {
            internal static void Info(bool collectUsage, string usageSession, bool outputStructured)
            {
                Recorder.CountInfo(collectUsage, usageSession);
                var metadata = Metadata;
                if (outputStructured)
                {
                    var reportingIds = Recorder.GetReportingIds();
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