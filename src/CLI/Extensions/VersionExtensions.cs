using System;

namespace automate.Extensions
{
    internal static class VersionExtensions
    {
        public static Version NextMajor(this Version version)
        {
            return new Version(version.Major + 1, version.Minor, ZeroBuild(version));
        }

        public static int ZeroBuild(this Version version)
        {
            return version.Build != -1
                ? version.Build
                : 0;
        }
    }
}