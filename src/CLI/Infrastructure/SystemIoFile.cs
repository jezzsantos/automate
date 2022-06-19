using System;
using System.IO;
using Automate.Application;
using Automate.Extensions;

namespace Automate.CLI.Infrastructure
{
    internal class SystemIoFile : IFile
    {
        public SystemIoFile(string absolutePath)
        {
            absolutePath.GuardAgainstNullOrEmpty(nameof(absolutePath));
            absolutePath.GuardAgainstInvalid(File.Exists, nameof(absolutePath),
                ExceptionMessages.SystemIoFile_SourceFileNotExist);
            FullPath = absolutePath;
        }

        public string FullPath { get; }

        public string Extension => Path.GetExtension(FullPath);

        public void CopyTo(string destination)
        {
            var directoryName = Path.GetDirectoryName(ExpandVariables(destination)) ?? string.Empty;
            if (!Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }

            File.Copy(FullPath, destination, true);
        }

        public byte[] GetContents()
        {
            return File.ReadAllBytes(FullPath);
        }

        private static string ExpandVariables(string path)
        {
            return Environment.ExpandEnvironmentVariables(path);
        }
    }
}