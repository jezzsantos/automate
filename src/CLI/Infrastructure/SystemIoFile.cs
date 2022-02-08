using System.IO;
using automate.Extensions;

namespace automate.Infrastructure
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