using System;
using System.Linq;
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

        public PatternToolkitPackage Package(PatternDefinition pattern, string versionInstruction)
        {
            var newVersion = UpdateToolkitVersion(pattern, versionInstruction);

            var toolkit = new PatternToolkitDefinition(pattern, newVersion);

            PackageAssets(toolkit);

            var location = this.toolkitStore.Save(toolkit);

            return new PatternToolkitPackage(toolkit, location);
        }

        private void PackageAssets(PatternToolkitDefinition toolkit)
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
                    throw new PatternException(
                        ExceptionMessages.PatternToolkitPackager_VersionBeforeCurrent.Format(versionInstruction,
                            currentVersion.ToString(VersionFieldCount)));
                }
                return new Version(requestedVersion.Major, requestedVersion.Minor, requestedVersion.ZeroBuild());
            }

            throw new PatternException(
                ExceptionMessages.PatternToolkitPackager_InvalidVersionInstruction.Format(versionInstruction));
        }
    }
}