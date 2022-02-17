﻿using System.IO;
using automate.Domain;
using automate.Extensions;

namespace automate.Infrastructure
{
    internal class SystemIoFileSystemWriter : IFileSystemWriter
    {
        public void Write(string contents, string path)
        {
            path.GuardAgainstNullOrEmpty(nameof(path));

            new FileInfo(path).Directory?.Create();
            File.WriteAllText(path, contents ?? string.Empty);
        }

        public bool Exists(string path)
        {
            path.GuardAgainstNullOrEmpty(nameof(path));
            return File.Exists(path);
        }
    }
}