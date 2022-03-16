using System;
using System.Linq;
using Automate.CLI.Application;
using Automate.CLI.Domain;
using Automate.CLI.Extensions;

namespace Automate.CLI.Infrastructure
{
    internal class PatternToolkitPackager : IPatternToolkitPackager
    {
        private readonly IPatternStore store;
        private readonly IToolkitStore toolkitStore;

        public PatternToolkitPackager(IPatternStore store, IToolkitStore toolkitStore)
        {
            store.GuardAgainstNull(nameof(store));
            toolkitStore.GuardAgainstNull(nameof(this.toolkitStore));

            this.store = store;
            this.toolkitStore = toolkitStore;
        }

        public ToolkitPackage Pack(PatternDefinition pattern, VersionInstruction instruction)
        {
            pattern.GuardAgainstNull(nameof(pattern));

            var version = pattern.UpdateToolkitVersion(instruction);

            this.store.Save(pattern);

            var toolkit = new ToolkitDefinition(pattern, new Version(version.Version));

            PackageAssets(toolkit);

            var location = this.toolkitStore.Export(toolkit);

            return new ToolkitPackage(toolkit, location, version.Message);
        }

        public ToolkitDefinition UnPack(IFile installer)
        {
            installer.GuardAgainstNull(nameof(installer));
            
            var toolkit = UnpackToolkit(installer);

            //TODO: we will need to worry about existing versions of this toolkit, and existing products created from them  
            this.toolkitStore.Import(toolkit);

            return toolkit;
        }

        private static ToolkitDefinition UnpackToolkit(IFile installer)
        {
            var contents = installer.GetContents();

            ToolkitDefinition toolkit;

            try
            {
                var json = SystemIoFileConstants.Encoding.GetString(contents);
                toolkit = json.FromJson<ToolkitDefinition>(new AutomatePersistableFactory());
            }
            catch (Exception ex)
            {
                throw new AutomateException(
                    ExceptionMessages.PatternToolkitPackager_InvalidInstallerFile.Format(
                        installer.FullPath), ex);
            }

            if (toolkit.NotExists()
                || !toolkit.Id.HasValue())
            {
                throw new AutomateException(
                    ExceptionMessages.PatternToolkitPackager_InvalidInstallerFile.Format(
                        installer.FullPath));
            }
            return toolkit;
        }

        private void PackageAssets(ToolkitDefinition toolkit)
        {
            var codeTemplates = toolkit.Pattern.GetAllCodeTemplates();
            if (codeTemplates.HasNone())
            {
                return;
            }

            toolkit.AddCodeTemplateFiles(
                codeTemplates
                    .Select(template =>
                    {
                        var contents = this.store.DownloadCodeTemplate(toolkit.Pattern, template);
                        return new CodeTemplateFile(contents, template.Id);
                    })
                    .ToList());
        }

    }
}