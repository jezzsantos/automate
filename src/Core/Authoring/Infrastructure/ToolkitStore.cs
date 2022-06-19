using System.Collections.Generic;
using Automate.Authoring.Application;
using Automate.Authoring.Domain;
using Automate.Common;
using Automate.Common.Extensions;
using Automate.Common.Infrastructure;

namespace Automate.Authoring.Infrastructure
{
    public class ToolkitStore : IToolkitStore
    {
        private readonly ILocalStateRepository localStateRepository;
        private readonly IToolkitRepository toolkitRepository;

        public ToolkitStore(IToolkitRepository toolkitRepository, ILocalStateRepository localStateRepository)
        {
            toolkitRepository.GuardAgainstNull(nameof(toolkitRepository));
            localStateRepository.GuardAgainstNull(nameof(localStateRepository));
            this.toolkitRepository = toolkitRepository;
            this.localStateRepository = localStateRepository;
        }

        public ToolkitDefinition GetCurrent()
        {
            var state = this.localStateRepository.GetLocalState();
            return state.CurrentToolkit.HasValue()
                ? this.toolkitRepository.GetToolkit(state.CurrentToolkit)
                : null;
        }

        public void DestroyAll()
        {
            this.toolkitRepository.DestroyAll();
            this.localStateRepository.DestroyAll();
        }

        public string Export(ToolkitDefinition toolkit)
        {
            return this.toolkitRepository.ExportToolkit(toolkit);
        }

        public void Import(ToolkitDefinition toolkit)
        {
            this.toolkitRepository.ImportToolkit(toolkit);
            ChangeCurrent(toolkit.Id);
        }

        public ToolkitDefinition ChangeCurrent(string id)
        {
            var toolkit = this.toolkitRepository.FindToolkitById(id);
            if (toolkit.NotExists())
            {
                throw new AutomateException(
                    ExceptionMessages.ToolkitStore_NotFoundAtLocationWithId.Substitute(id,
                        this.toolkitRepository.ToolkitLocation));
            }

            var state = this.localStateRepository.GetLocalState();
            state.SetCurrentToolkit(id);
            this.localStateRepository.SaveLocalState(state);

            return toolkit;
        }

        public ToolkitDefinition FindByName(string name)
        {
            return this.toolkitRepository.FindToolkitByName(name);
        }

        public ToolkitDefinition FindById(string id)
        {
            return this.toolkitRepository.FindToolkitById(id);
        }

        public List<ToolkitDefinition> ListAll()
        {
            return this.toolkitRepository.ListToolkits();
        }
    }
}