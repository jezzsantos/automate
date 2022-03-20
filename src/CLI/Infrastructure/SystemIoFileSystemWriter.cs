using System.IO;
using Automate.CLI.Domain;
using Automate.CLI.Extensions;

namespace Automate.CLI.Infrastructure
{
    internal class SystemIoFileSystemWriter : IFileSystemWriter
    {
        public void Write(string contents, string absolutePath)
        {
            absolutePath.GuardAgainstNullOrEmpty(nameof(absolutePath));

            new FileInfo(absolutePath).Directory?.Create();
            File.WriteAllText(absolutePath, contents ?? string.Empty);
        }

        public bool Exists(string absolutePath)
        {
            absolutePath.GuardAgainstNullOrEmpty(nameof(absolutePath));
            return File.Exists(absolutePath);
        }

        public void Delete(string absolutePath)
        {
            absolutePath.GuardAgainstNullOrEmpty(nameof(absolutePath));

            if (File.Exists(absolutePath))
            {
                File.Delete(absolutePath);
            }
        }

        public void Move(string sourceAbsolutePath, string targetAbsolutePath)
        {
            sourceAbsolutePath.GuardAgainstNullOrEmpty(nameof(sourceAbsolutePath));
            targetAbsolutePath.GuardAgainstNullOrEmpty(nameof(targetAbsolutePath));

            if (File.Exists(sourceAbsolutePath))
            {
                new FileInfo(targetAbsolutePath).Directory?.Create();
                File.Move(sourceAbsolutePath, targetAbsolutePath, true);
            }
        }
    }
}