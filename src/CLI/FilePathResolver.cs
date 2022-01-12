using System.IO;
using automate.Extensions;

namespace automate
{
    internal class FilePathResolver : IPathResolver
    {
        public string CreatePath(string rootPath, string relativeOrAbsolutePath)
        {
            rootPath.GuardAgainstNullOrEmpty(nameof(rootPath));
            relativeOrAbsolutePath.GuardAgainstNullOrEmpty(nameof(relativeOrAbsolutePath));

            return Path.GetFullPath(Path.Combine(rootPath, relativeOrAbsolutePath));
        }

        public bool ExistsAtPath(string fullPath)
        {
            fullPath.GuardAgainstNullOrEmpty(nameof(fullPath));

            return File.Exists(fullPath);
        }
    }
}