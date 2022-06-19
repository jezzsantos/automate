namespace Automate.Common.Domain
{
    public interface IRuntimeMetadata
    {
        string RuntimeVersion { get; }

        string ProductName { get; }
    }
}