#nullable enable
namespace Automate.CLI.Domain
{
    internal class LocalState : IPersistable
    {
        public LocalState()
        {
        }

        private LocalState(PersistableProperties properties, IPersistableFactory factory)
        {
            CurrentPattern = properties.Rehydrate<string>(factory, nameof(CurrentPattern));
            CurrentToolkit = properties.Rehydrate<string>(factory, nameof(CurrentToolkit));
            CurrentSolution = properties.Rehydrate<string>(factory, nameof(CurrentSolution));
        }

        public string CurrentPattern { get; private set; } = null!;

        public string CurrentToolkit { get; private set; } = null!;

        public string CurrentSolution { get; private set; } = null!;

        public PersistableProperties Dehydrate()
        {
            var properties = new PersistableProperties();
            properties.Dehydrate(nameof(CurrentPattern), CurrentPattern);
            properties.Dehydrate(nameof(CurrentToolkit), CurrentToolkit);
            properties.Dehydrate(nameof(CurrentSolution), CurrentSolution);

            return properties;
        }

        public static LocalState Rehydrate(PersistableProperties properties, IPersistableFactory factory)
        {
            return new LocalState(properties, factory);
        }

        public void SetCurrentPattern(string id)
        {
            CurrentPattern = id;
        }

        public void SetCurrentToolkit(string id)
        {
            CurrentToolkit = id;
        }

        public void SetCurrentSolution(string id)
        {
            CurrentSolution = id;
        }
    }
}