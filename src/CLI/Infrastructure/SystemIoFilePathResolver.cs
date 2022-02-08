using System.IO;
using automate.Extensions;

namespace automate.Infrastructure
{
    internal class SystemIoFilePathResolver : IFilePathResolver
    {
        public string CreatePath(string rootPath, string relativeOrAbsolutePath)
        {
            rootPath.GuardAgainstNullOrEmpty(nameof(rootPath));
            relativeOrAbsolutePath.GuardAgainstNullOrEmpty(nameof(relativeOrAbsolutePath));

            return Path.GetFullPath(Path.Combine(rootPath, relativeOrAbsolutePath));
        }

        public bool ExistsAtPath(string absolutePath)
        {
            absolutePath.GuardAgainstNullOrEmpty(nameof(absolutePath));

            return File.Exists(absolutePath);
        }

        public IFile GetFileAtPath(string absolutePath)
        {
            return new SystemIoFile(absolutePath);
        }
    }
}