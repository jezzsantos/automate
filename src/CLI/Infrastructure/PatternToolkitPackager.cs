using System;
using System.Linq;
using System.Text;
using automate.Application;
using automate.Domain;
using automate.Extensions;

namespace automate.Infrastructure
{
    internal class PatternToolkitPackager : IPatternToolkitPackager
    {
        private const int VersionFieldCount = 3;
        public const string AutoIncrementInstruction = "auto";
        private static readonly Version DefaultVersionNumber = new Version(0, 0, 0);
        private readonly IFilePathResolver filePathResolver;
        private readonly IPatternStore store;
        private readonly IToolkitStore toolkitStore;

        public PatternToolkitPackager(IPatternStore store, IToolkitStore toolkitStore,
            IFilePathResolver filePathResolver)
        {
            store.GuardAgainstNull(nameof(store));
            toolkitStore.GuardAgainstNull(nameof(this.toolkitStore));
            filePathResolver.GuardAgainstNull(nameof(filePathResolver));

            this.store = store;
            this.toolkitStore = toolkitStore;
            this.filePathResolver = filePathResolver;
        }

        public PatternToolkitPackage Pack(PatternDefinition pattern, string versionInstruction)
        {
            var newVersion = UpdateToolkitVersion(pattern, versionInstruction);

            var toolkit = new ToolkitDefinition(pattern, newVersion);

            PackageAssets(toolkit);

            var location = this.toolkitStore.Export(toolkit);

            return new PatternToolkitPackage(toolkit, location);
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
                toolkit = Encoding.UTF8.GetString(contents).FromJson<ToolkitDefinition>();
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
            if (toolkit.Pattern.CodeTemplates.NotExists())
            {
                return;
            }

            toolkit.CodeTemplateFiles =
                toolkit.Pattern.CodeTemplates
                    .Select(template => new CodeTemplateFile
                    {
                        Contents =
                            this.filePathResolver.GetFileAtPath(template.Metadata.OriginalFilePath).GetContents(),
                        Id = template.Id
                    })
                    .ToList();
        }

        private string UpdateToolkitVersion(PatternDefinition pattern, string versionInstruction)
        {
            var currentVersion = Version.Parse(pattern.ToolkitVersion.HasValue()
                ? pattern.ToolkitVersion
                : DefaultVersionNumber.ToString(VersionFieldCount));
            var newVersion = CalculatePackageVersion(currentVersion, versionInstruction).ToString(VersionFieldCount);

            pattern.ToolkitVersion = newVersion;
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