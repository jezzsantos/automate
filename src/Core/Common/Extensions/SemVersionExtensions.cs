using Semver;

namespace Automate.Common.Extensions
{
    public static class SemVersionExtensions
    {
        public static SemVersion NextMinor(this SemVersion version)
        {
            return SemVersion.ParsedFrom(version.Major, version.Minor + 1);
        }

        public static SemVersion NextMajor(this SemVersion version)
        {
            return SemVersion.ParsedFrom(version.Major + 1);
        }
    }
}