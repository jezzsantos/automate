namespace Automate.Application
{
    public interface IFile
    {
        string FullPath { get; }

        string Extension { get; }

        void CopyTo(string destination);

        byte[] GetContents();
    }

    public interface IFilePathResolver
    {
        string CreatePath(string rootPath, string relativeOrAbsolutePath);

        string GetFileExtension(string absolutePath);

        bool ExistsAtPath(string absolutePath);

        IFile GetFileAtPath(string absolutePath);

        string GetFilename(string absolutePath);

        void CreateFileAtPath(string absolutePath, byte[] contents);
    }
}