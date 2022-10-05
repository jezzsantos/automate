namespace Automate.Common.Domain
{
    public interface IAssemblyMetadata
    {
        string RuntimeVersion { get; }

        string ProductName { get; }
    }
}