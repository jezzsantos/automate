#nullable enable
namespace automate.Domain
{
    internal class LocalState
    {
        public string CurrentPattern { get; set; } = null!;

        public string CurrentToolkit { get; set; } = null!;
    }
}