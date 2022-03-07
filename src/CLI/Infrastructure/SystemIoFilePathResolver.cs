using System.IO;
using Automate.CLI.Domain;
using Automate.CLI.Extensions;

namespace Automate.CLI.Infrastructure
{
    internal class SystemIoFilePathResolver : IFilePathResolver
    {
        public string CreatePath(string rootPath, string relativeOrAbsolutePath)
        {
            rootPath.GuardAgainstNullOrEmpty(nameof(rootPath));
            relativeOrAbsolutePath.GuardAgainstNullOrEmpty(nameof(relativeOrAbsolutePath));

            return Path.GetFullPath(Path.Combine(rootPath, relativeOrAbsolutePath));
        }

        public string GetFileExtension(string absolutePath)
        {
            return Path.GetExtension(absolutePath);
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

        public string GetFilename(string absolutePath)
        {
            return Path.GetFileName(absolutePath);
        }

        public void CreateFileAtPath(string absolutePath, byte[] contents)
        {
            File.WriteAllBytes(absolutePath, contents);
        }
    }
}