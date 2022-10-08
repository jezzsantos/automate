#nullable enable
namespace Automate.Common.Domain
{
    public class LocalState : IPersistable
    {
        public LocalState()
        {
        }

        private LocalState(PersistableProperties properties,
            IPersistableFactory factory)
        {
            CurrentPattern = properties.Rehydrate<string>(factory, nameof(CurrentPattern));
            CurrentToolkit = properties.Rehydrate<string>(factory, nameof(CurrentToolkit));
            CurrentDraft = properties.Rehydrate<string>(factory, nameof(CurrentDraft));
        }

        public string CurrentPattern { get; private set; } = null!;

        public string CurrentToolkit { get; private set; } = null!;

        public string CurrentDraft { get; private set; } = null!;

        public PersistableProperties Dehydrate()
        {
            var properties = new PersistableProperties();
            properties.Dehydrate(nameof(CurrentPattern), CurrentPattern);
            properties.Dehydrate(nameof(CurrentToolkit), CurrentToolkit);
            properties.Dehydrate(nameof(CurrentDraft), CurrentDraft);

            return properties;
        }

        public static LocalState Rehydrate(PersistableProperties properties,
            IPersistableFactory factory)
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

        public void SetCurrentDraft(string id)
        {
            CurrentDraft = id;
        }

        public void ClearCurrentDraft()
        {
            CurrentDraft = null!;
        }
    }
}