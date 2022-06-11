﻿using System;
using System.Collections.Generic;
using Automate.CLI.Extensions;

namespace Automate.CLI.Domain
{
    internal interface IFileSystemReaderWriter : IFileSystemReader, IFileSystemWriter
    {
    }

    internal interface IFileSystemWriter
    {
        void Write(string contents, string absolutePath);

        void Delete(string absolutePath);

        void Move(string sourceAbsolutePath, string targetAbsolutePath);

        void DirectoryDelete(FileSystemDirectory directory);

        void DeleteAllDirectoryFiles(string absolutePath, string pattern);

        void EnsureFileDirectoryExists(string absolutePath);
    }

    internal interface IFileSystemReader
    {
        bool FileExists(string absolutePath);

        bool DirectoryExists(string absolutePath);

        string ReadAllText(string absolutePath);

        IEnumerable<FileSystemDirectory> GetSubDirectories(string absolutePath);

        string MakePath(string directory, string filename);

        FileSystemFile GetContent(string path);
    }

    internal class FileSystemDirectory
    {
        public FileSystemDirectory(string name, string fullPath)
        {
            Name = name;
            FullPath = fullPath;
        }

        public string Name { get; }

        public string FullPath { get; }
    }

    internal class FileSystemFile
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