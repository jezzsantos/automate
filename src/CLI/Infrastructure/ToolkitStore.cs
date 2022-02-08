using automate.Domain;
using automate.Extensions;

namespace automate.Infrastructure
{
    internal class ToolkitStore : IToolkitStore
    {
        private readonly ILocalStateRepository localStateRepository;
        private readonly IToolkitRepository toolkitRepository;

        public ToolkitStore(string currentDirectory) : this(new JsonFileRepository(currentDirectory))
        {
        }

        private ToolkitStore(JsonFileRepository repository) : this(repository, repository)
        {
        }

        internal ToolkitStore(IToolkitRepository toolkitRepository, ILocalStateRepository localStateRepository)
        {
            toolkitRepository.GuardAgainstNull(nameof(toolkitRepository));
            localStateRepository.GuardAgainstNull(nameof(localStateRepository));
            this.toolkitRepository = toolkitRepository;
            this.localStateRepository = localStateRepository;
        }

        public void DestroyAll()
        {
            this.toolkitRepository.DestroyAll();
            this.localStateRepository.DestroyAll();
        }

        public string Export(PatternToolkitDefinition toolkit)
        {
            return this.toolkitRepository.ExportToolkit(toolkit);
        }

        public void Import(PatternToolkitDefinition toolkit)
        {
            this.toolkitRepository.ImportToolkit(toolkit);
        }

        public PatternToolkitDefinition GetCurrent()
        {
            var state = this.localStateRepository.GetLocalState();
            return state.CurrentToolkit.HasValue()
                ? this.toolkitRepository.GetToolkit(state.CurrentToolkit)
                : null;
        }

        public void ChangeCurrent(string id)
        {
            var toolkit = this.toolkitRepository.FindToolkitById(id);
            if (toolkit.NotExists())
            {
                throw new PatternException(
                    ExceptionMessages.ToolkitStore_NotFoundAtLocationWithId.Format(id,
                        this.toolkitRepository.ToolkitLocation));
            }

            var state = this.localStateRepository.GetLocalState();
            state.CurrentToolkit = id;
            this.localStateRepository.SaveLocalState(state);
        }
    }
}