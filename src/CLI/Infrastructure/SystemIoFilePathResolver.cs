using System;
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

            return Path.GetFullPath(Path.Combine(ExpandVariables(rootPath), relativeOrAbsolutePath));
        }

        public string GetFileExtension(string absolutePath)
        {
            return Path.GetExtension(ExpandVariables(absolutePath));
        }

        public bool ExistsAtPath(string absolutePath)
        {
            absolutePath.GuardAgainstNullOrEmpty(nameof(absolutePath));

            return File.Exists(ExpandVariables(absolutePath));
        }

        public IFile GetFileAtPath(string absolutePath)
        {
            return new SystemIoFile(ExpandVariables(absolutePath));
        }

        public string GetFilename(string absolutePath)
        {
            return Path.GetFileName(ExpandVariables(absolutePath));
        }

        public void CreateFileAtPath(string absolutePath, byte[] contents)
        {
            File.WriteAllBytes(ExpandVariables(absolutePath), contents);
        }

        private static string ExpandVariables(string path)
        {
            return Environment.ExpandEnvironmentVariables(path);
        }
    }
}