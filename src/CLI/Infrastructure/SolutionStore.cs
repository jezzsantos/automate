using automate.Domain;
using automate.Extensions;

namespace automate.Infrastructure
{
    internal class SolutionStore : ISolutionStore
    {
        private readonly ILocalStateRepository localStateRepository;
        private readonly ISolutionRepository solutionRepository;

        public SolutionStore(string currentDirectory) : this(new JsonFileRepository(currentDirectory))
        {
        }

        private SolutionStore(JsonFileRepository repository) : this(repository, repository)
        {
        }

        internal SolutionStore(ISolutionRepository solutionRepository, ILocalStateRepository localStateRepository)
        {
            solutionRepository.GuardAgainstNull(nameof(solutionRepository));
            localStateRepository.GuardAgainstNull(nameof(localStateRepository));
            this.solutionRepository = solutionRepository;
            this.localStateRepository = localStateRepository;
        }

        public void DestroyAll()
        {
            this.solutionRepository.DestroyAll();
            this.localStateRepository.DestroyAll();
        }

        public void Save(SolutionDefinition solution)
        {
            this.solutionRepository.UpsertSolution(solution);
        }
    }
}