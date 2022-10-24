using Automate.CLI.Extensions;
using JetBrains.Annotations;

namespace Automate.CLI.Infrastructure
{
    internal partial class CommandLineApi
    {
        [UsedImplicitly]
        internal class AdministrativeApiHandlers : HandlerBase
        {
            internal static void Info(bool collectUsage, string usageCorrelation, bool outputStructured)
            {
                Recorder.MeasureInfo(collectUsage, usageCorrelation);
                var metadata = Metadata;
                if (outputStructured)
                {
                    var reportingIds = Recorder.GetReportingIds();
                    Output(OutputMessages.CommandLine_Output_Info, metadata.ProductName, metadata.InstallationPath,
                        metadata.RuntimeVersion.ToString(),
                        new
                        {
                            IsEnabled = collectUsage,
                            reportingIds.MachineId,
                            reportingIds.CorrelationId
                        });
                }
                else
                {
                    Output(OutputMessages.CommandLine_Output_Info, metadata.ProductName, metadata.InstallationPath,
                        metadata.RuntimeVersion.ToString(),
                        collectUsage);
                }
            }
        }
    }
}