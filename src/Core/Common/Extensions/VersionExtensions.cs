using System;

namespace Automate.Common.Extensions
{
    public static class VersionExtensions
    {
        public static Version To2Dot(this Version version)
        {
            return new Version(version.Major, version.Minor, BuildOrZero(version));
        }

        public static Version RevMajor(this Version version)
        {
            return new Version(version.Major + 1, 0, BuildOrZero(version));
        }

        public static Version RevMinor(this Version version)
        {
            return new Version(version.Major, version.Minor + 1, BuildOrZero(version));
        }

        public static int BuildOrZero(this Version version)
        {
            return version.Build != -1
                ? version.Build
                : 0;
        }
    }
}