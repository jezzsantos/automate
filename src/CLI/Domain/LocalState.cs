#nullable enable
namespace Automate.CLI.Domain
{
    internal class LocalState
    {
        public string CurrentPattern { get; set; } = null!;

        public string CurrentToolkit { get; set; } = null!;
    }
}