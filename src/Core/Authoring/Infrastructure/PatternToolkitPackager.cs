using System;
using Automate.Authoring.Application;
using Automate.Authoring.Domain;
using Automate.Common;
using Automate.Common.Domain;
using Automate.Common.Extensions;
using Automate.Common.Infrastructure;

namespace Automate.Authoring.Infrastructure
{
    public class PatternToolkitPackager : IPatternToolkitPackager
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

        public ToolkitPackage PackAndExport(PatternDefinition pattern, VersionInstruction instruction)
        {
            var (version, toolkit) =
                Pack(pattern, instruction, (pat, temp) => this.store.DownloadCodeTemplate(pat, temp));
            this.store.Save(pattern);

            var exportedLocation = this.toolkitStore.Export(toolkit);

            return new ToolkitPackage(toolkit, exportedLocation, version.Message);
        }

        public ToolkitDefinition UnPack(IRuntimeMetadata metadata, IFile installer)
        {
            metadata.GuardAgainstNull(nameof(metadata));
            installer.GuardAgainstNull(nameof(installer));

            var toolkit = UnpackToolkit(metadata, installer);

            this.toolkitStore.Import(toolkit);

            return toolkit;
        }

        internal static (VersionUpdateResult Version, ToolkitDefinition Toolkit) Pack(PatternDefinition pattern,
            VersionInstruction instruction, Func<PatternDefinition, CodeTemplate, CodeTemplateContent> getContent)
        {
            pattern.GuardAgainstNull(nameof(pattern));

            pattern.RegisterCodeTemplatesChanges(getContent);
            var version = pattern.UpdateToolkitVersion(instruction);

            var toolkit = new ToolkitDefinition(pattern);
            toolkit.Pack(getContent);

            return (version, toolkit);
        }

        private ToolkitDefinition UnpackToolkit(IRuntimeMetadata metadata, IFile installer)
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
                    ExceptionMessages.PatternToolkitPackager_InvalidInstallerFile.Substitute(
                        installer.FullPath), ex);
            }

            if (toolkit.NotExists()
                || !toolkit.Id.HasValue())
            {
                throw new AutomateException(
                    ExceptionMessages.PatternToolkitPackager_InvalidInstallerFile.Substitute(
                        installer.FullPath));
            }

            toolkit.VerifyRuntimeCompatability(metadata);

            return toolkit;
        }
    }
}