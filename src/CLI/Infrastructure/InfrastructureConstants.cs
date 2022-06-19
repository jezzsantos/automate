using System;
using Automate.Extensions;

namespace Automate.CLI.Infrastructure
{
    internal static class InfrastructureConstants
    {
        public const string RootPersistencePath = "automate";

        public static string GetExportDirectory()
        {
            var isCiBuild = Environment.GetEnvironmentVariable("IS_CI_BUILD").ToBool();
            return Environment.GetFolderPath(isCiBuild
                ? Environment.SpecialFolder.LocalApplicationData
                : Environment.SpecialFolder.Desktop);
        }
    }
}