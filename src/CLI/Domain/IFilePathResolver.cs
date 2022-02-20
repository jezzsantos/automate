namespace Automate.CLI.Domain
{
    internal interface IFile
    {
        string FullPath { get; }

        void CopyTo(string destination);

        byte[] GetContents();
    }

    internal interface IFilePathResolver
    {
        string CreatePath(string rootPath, string relativeOrAbsolutePath);

        string GetFileExtension(string absolutePath);

        bool ExistsAtPath(string absolutePath);

        IFile GetFileAtPath(string absolutePath);
    }
}