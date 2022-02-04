using automate.Extensions;

namespace automate
{
    internal class ToolkitStore
    {
        private readonly IPatternRepository repository;

        public ToolkitStore(string currentDirectory) : this(new JsonFilePatternRepository(currentDirectory))
        {
        }

        internal ToolkitStore(IPatternRepository repository)
        {
            repository.GuardAgainstNull(nameof(repository));
            this.repository = repository;
        }

        public PatternMetaModel GetCurrent()
        {
            var state = this.repository.GetState();
            return state.Current.HasValue()
                ? this.repository.Get(state.Current)
                : null;
        }
    }
}