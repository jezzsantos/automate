﻿namespace Automate.CLI.Domain
{
    internal interface IFileSystemWriter
    {
        void Write(string contents, string path);

        bool Exists(string path);
    }
}