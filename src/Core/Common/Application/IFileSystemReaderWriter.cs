using System;
using System.Collections.Generic;
using Automate.Common.Extensions;

namespace Automate.Common.Application
{
    public interface IFileSystemReaderWriter : IFileSystemReader, IFileSystemWriter
    {
    }

    public interface IFileSystemWriter
    {
        void Write(string contents, string absolutePath);

        void Delete(string absolutePath);

        void Move(string sourceAbsolutePath, string targetAbsolutePath);

        void DirectoryDelete(FileSystemDirectory directory);

        void DeleteAllDirectoryFiles(string absolutePath, string pattern);

        void EnsureFileDirectoryExists(string absolutePath);
    }

    public interface IFileSystemReader
    {
        bool FileExists(string absolutePath);

        bool DirectoryExists(string absolutePath);

        string ReadAllText(string absolutePath);

        FileSystemDirectory GetDirectory(string absolutePath);

        IEnumerable<FileSystemDirectory> GetSubDirectories(string absolutePath);

        string MakePath(string directory, string filename);

        FileSystemFile GetContent(string path);
    }

    public class FileSystemDirectory
    {
        public FileSystemDirectory(string name, string fullPath)
        {
            Name = name;
            FullPath = fullPath;
        }

        public string Name { get; }

        public string FullPath { get; }
    }

    public class FileSystemFile
    {
        public FileSystemFile(byte[] rawBytes, DateTime lastModifiedUtc)
        {
            rawBytes.GuardAgainstNull(nameof(rawBytes));
            RawBytes = rawBytes;
            LastModifiedUtc = lastModifiedUtc;
        }

        public byte[] RawBytes { get; }

        public DateTime LastModifiedUtc { get; }
    }
}