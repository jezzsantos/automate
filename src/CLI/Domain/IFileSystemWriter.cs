namespace Automate.CLI.Domain
{
    internal interface IFileSystemWriter
    {
        void Write(string contents, string absolutePath);

        bool Exists(string absolutePath);

        void Delete(string absolutePath);

        void Move(string sourceAbsolutePath, string targetAbsolutePath);
    }
}