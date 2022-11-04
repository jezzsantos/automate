using System;
using Automate.Common.Extensions;

namespace Automate.CLI.Infrastructure
{
    internal static class InfrastructureConstants
    {
        public static string GetExportDirectory()
        {
            var isCiBuild = Environment.GetEnvironmentVariable("IS_CI_BUILD").ToBool();
            return Environment.GetFolderPath(isCiBuild
                ? Environment.SpecialFolder.LocalApplicationData
                : Environment.SpecialFolder.Desktop);
        }
    }
}