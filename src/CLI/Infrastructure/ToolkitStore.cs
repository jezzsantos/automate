using automate.Domain;
using automate.Extensions;

namespace automate.Infrastructure
{
    internal class ToolkitStore : IToolkitStore
    {
        private readonly ILocalStateRepository localStateRepository;
        private readonly IToolkitRepository repository;

        public ToolkitStore(string currentDirectory) : this(new JsonFileRepository(currentDirectory))
        {
        }

        private ToolkitStore(JsonFileRepository repository) : this(repository, repository)
        {
        }

        internal ToolkitStore(IToolkitRepository patternRepository, ILocalStateRepository localStateRepository)
        {
            patternRepository.GuardAgainstNull(nameof(patternRepository));
            localStateRepository.GuardAgainstNull(nameof(localStateRepository));
            this.repository = patternRepository;
            this.localStateRepository = localStateRepository;
        }

        public string Save(PatternToolkitDefinition toolkit)
        {
            return this.repository.SaveToolkit(toolkit);
        }

        public PatternToolkitDefinition GetCurrent()
        {
            var state = this.localStateRepository.GetLocalState();
            return state.CurrentToolkit.HasValue()
                ? this.repository.GetToolkit(state.CurrentToolkit)
                : null;
        }
    }
}