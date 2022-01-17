namespace automate
{
    internal interface IFilePathResolver
    {
        string CreatePath(string rootPath, string relativeOrAbsolutePath);

        bool ExistsAtPath(string fullPath);
    }
}