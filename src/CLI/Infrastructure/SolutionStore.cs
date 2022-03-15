using System.Collections.Generic;
using System.Linq;
using Automate.CLI.Application;
using Automate.CLI.Domain;
using Automate.CLI.Extensions;

namespace Automate.CLI.Infrastructure
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

        public SolutionDefinition GetCurrent()
        {
            var state = this.localStateRepository.GetLocalState();
            return state.CurrentSolution.HasValue()
                ? this.solutionRepository.GetSolution(state.CurrentSolution)
                : null;
        }

        public void DestroyAll()
        {
            this.solutionRepository.DestroyAll();
            this.localStateRepository.DestroyAll();
        }

        public SolutionDefinition Create(ToolkitDefinition toolkit, string name)
        {
            toolkit.GuardAgainstNull(nameof(toolkit));

            var solution = new SolutionDefinition(toolkit, name);
            this.solutionRepository.NewSolution(solution);

            var state = this.localStateRepository.GetLocalState();
            state.SetCurrentSolution(solution.Id);
            this.localStateRepository.SaveLocalState(state);

            return solution;
        }

        public void Save(SolutionDefinition solution)
        {
            this.solutionRepository.UpsertSolution(solution);
        }

        public List<SolutionDefinition> ListAll()
        {
            return this.solutionRepository.ListSolutions();
        }

        public SolutionDefinition FindById(string id)
        {
            return ListAll()
                .FirstOrDefault(sol => sol.Id == id);
        }

        public void ChangeCurrent(string id)
        {
            var solution = this.solutionRepository.FindSolutionById(id);
            if (solution.NotExists())
            {
                throw new AutomateException(
                    ExceptionMessages.SolutionStore_NotFoundAtLocationWithId.Format(id,
                        this.solutionRepository.SolutionLocation));
            }

            var state = this.localStateRepository.GetLocalState();
            state.SetCurrentSolution(solution.Id);
            this.localStateRepository.SaveLocalState(state);
        }
    }
}