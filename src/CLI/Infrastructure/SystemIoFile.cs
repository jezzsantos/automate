using System;
using System.IO;
using Automate.CLI.Domain;
using Automate.CLI.Extensions;

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

        public DateTime LastModifiedUtc => File.GetLastWriteTimeUtc(FullPath);

        public void CopyTo(string destination)
        {
            var directoryName = Path.GetDirectoryName(destination) ?? string.Empty;
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
    }
}