using System.Collections.Generic;
using System.IO;
using System.Linq;
using Automate.Common.Application;
using Automate.Common.Extensions;

namespace Automate.CLI.Infrastructure
{
    internal class SystemIoFileSystemReaderWriter : IFileSystemReaderWriter
    {
        public void Write(string contents, string absolutePath)
        {
            absolutePath.GuardAgainstNullOrEmpty(nameof(absolutePath));

            new FileInfo(absolutePath).Directory?.Create();
            File.WriteAllText(absolutePath, contents ?? string.Empty);
        }

        public bool FileExists(string absolutePath)
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

        public void DirectoryDelete(FileSystemDirectory directory)
        {
            directory.GuardAgainstNull(nameof(directory));
            if (Directory.Exists(directory.FullPath))
            {
                Directory.Delete(directory.FullPath, true);
            }
        }

        public void DeleteAllDirectoryFiles(string absolutePath, string pattern)
        {
            absolutePath.GuardAgainstNullOrEmpty(nameof(absolutePath));
            pattern.GuardAgainstNullOrEmpty(nameof(pattern));

            var directory = new DirectoryInfo(absolutePath);
            if (directory.Exists)
            {
                var files = directory.GetFiles(pattern);
                foreach (var file in files)
                {
                    file.Delete();
                }
            }
        }

        public void EnsureDirectoryExists(string absolutePath)
        {
            absolutePath.GuardAgainstNullOrEmpty(nameof(absolutePath));

            if (!Directory.Exists(absolutePath))
            {
                Directory.CreateDirectory(absolutePath!);
            }
        }

        public void EnsureFileDirectoryExists(string absolutePath)
        {
            absolutePath.GuardAgainstNullOrEmpty(nameof(absolutePath));

            var fullPath = Directory.GetParent(absolutePath)?.FullName ?? string.Empty;
            if (fullPath.HasValue())
            {
                if (!Directory.Exists(fullPath))
                {
                    Directory.CreateDirectory(fullPath);
                }
            }
        }

        public bool DirectoryExists(string absolutePath)
        {
            absolutePath.GuardAgainstNullOrEmpty(nameof(absolutePath));

            return Directory.Exists(absolutePath);
        }

        public string ReadAllText(string absolutePath)
        {
            absolutePath.GuardAgainstNullOrEmpty(nameof(absolutePath));

            return File.ReadAllText(absolutePath);
        }

        public FileSystemDirectory GetDirectory(string absolutePath)
        {
            absolutePath.GuardAgainstNullOrEmpty(nameof(absolutePath));

            return new FileSystemDirectory(new DirectoryInfo(absolutePath).Name, absolutePath);
        }

        public IEnumerable<FileSystemDirectory> GetSubDirectories(string absolutePath)
        {
            absolutePath.GuardAgainstNullOrEmpty(nameof(absolutePath));

            return Directory.GetDirectories(absolutePath)
                .Select(fullPath => new FileSystemDirectory(new DirectoryInfo(fullPath).Name, fullPath));
        }

        public string MakeAbsolutePath(string directory)
        {
            directory.GuardAgainstNullOrEmpty(nameof(directory));
            return Path.GetFullPath(directory);
        }

        public string MakeAbsolutePath(string directory, string filename)
        {
            directory.GuardAgainstNullOrEmpty(nameof(directory));
            filename.GuardAgainstNullOrEmpty(nameof(filename));
            return Path.Combine(MakeAbsolutePath(directory), filename);
        }

        public FileSystemFile GetContent(string absolutePath)
        {
            absolutePath.GuardAgainstNullOrEmpty(nameof(absolutePath));

            var raw = File.ReadAllBytes(absolutePath);
            var lastModified = File.GetLastWriteTimeUtc(absolutePath);

            return new FileSystemFile(raw, lastModified);
        }
    }
}