using System;
using System.Linq;
using Automate.CLI.Application;
using Automate.CLI.Domain;
using Automate.CLI.Extensions;

namespace Automate.CLI.Infrastructure
{
    internal class PatternToolkitPackager : IPatternToolkitPackager
    {
        private const int VersionFieldCount = 3;
        public const string AutoIncrementInstruction = "auto";
        private readonly IPatternStore store;
        private readonly IToolkitStore toolkitStore;

        public PatternToolkitPackager(IPatternStore store, IToolkitStore toolkitStore)
        {
            store.GuardAgainstNull(nameof(store));
            toolkitStore.GuardAgainstNull(nameof(this.toolkitStore));

            this.store = store;
            this.toolkitStore = toolkitStore;
        }

        public ToolkitPackage Pack(PatternDefinition pattern, string versionInstruction)
        {
            var newVersion = UpdateToolkitVersion(pattern, versionInstruction);

            var toolkit = new ToolkitDefinition(pattern, newVersion);

            PackageAssets(toolkit);

            var location = this.toolkitStore.Export(toolkit);

            return new ToolkitPackage(toolkit, location);
        }

        public ToolkitDefinition UnPack(IFile installer)
        {
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

        private string UpdateToolkitVersion(PatternDefinition pattern, string versionInstruction)
        {
            var currentVersion = Version.Parse(pattern.ToolkitVersion.HasValue()
                ? pattern.ToolkitVersion
                : PatternDefinition.DefaultVersionNumber.ToString(VersionFieldCount));
            var newVersion = CalculatePackageVersion(currentVersion, versionInstruction).ToString(VersionFieldCount);

            pattern.UpdateToolkitVersion(newVersion);
            this.store.Save(pattern);

            return newVersion;
        }

        private static Version CalculatePackageVersion(Version currentVersion, string versionInstruction)
        {
            if (!versionInstruction.HasValue())
            {
                return currentVersion.NextMajor();
            }

            if (versionInstruction.EqualsIgnoreCase(AutoIncrementInstruction))
            {
                return currentVersion.NextMajor();
            }

            if (Version.TryParse(versionInstruction, out var requestedVersion))
            {
                if (requestedVersion < currentVersion)
                {
                    throw new AutomateException(
                        ExceptionMessages.PatternToolkitPackager_VersionBeforeCurrent.Format(versionInstruction,
                            currentVersion.ToString(VersionFieldCount)));
                }
                return new Version(requestedVersion.Major, requestedVersion.Minor, requestedVersion.ZeroBuild());
            }

            throw new AutomateException(
                ExceptionMessages.PatternToolkitPackager_InvalidVersionInstruction.Format(versionInstruction));
        }
    }
}