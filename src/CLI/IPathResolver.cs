namespace automate
{
    internal interface IPathResolver
    {
        string CreatePath(string rootPath, string relativeOrAbsolutePath);

        bool ExistsAtPath(string fullPath);
    }
}