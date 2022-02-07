using System;
using System.Linq;
using automate.Extensions;

namespace automate
{
    internal class PatternToolkitPackager : IPatternToolkitPackager
    {
        private const int VersionFieldCount = 3;
        public const string AutoIncrementInstruction = "auto";
        private static readonly Version DefaultVersionNumber = new Version(0, 0, 0);
        private readonly IFilePathResolver filePathResolver;
        private readonly PatternStore store;
        private readonly IToolkitRepository toolkitRepository;

        public PatternToolkitPackager(PatternStore store, IToolkitRepository toolkitRepository,
            IFilePathResolver filePathResolver)
        {
            store.GuardAgainstNull(nameof(store));
            toolkitRepository.GuardAgainstNull(nameof(toolkitRepository));
            filePathResolver.GuardAgainstNull(nameof(filePathResolver));

            this.store = store;
            this.toolkitRepository = toolkitRepository;
            this.filePathResolver = filePathResolver;
        }

        public PatternToolkitPackage Package(PatternMetaModel pattern, string versionInstruction)
        {
            var newVersion = UpdateToolkitVersion(pattern, versionInstruction);

            var toolkit = new PatternToolkit(pattern, newVersion);

            PackageAssets(toolkit);

            var location = this.toolkitRepository.Save(toolkit);

            return new PatternToolkitPackage(toolkit, location);
        }

        private void PackageAssets(PatternToolkit toolkit)
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

        private string UpdateToolkitVersion(PatternMetaModel pattern, string versionInstruction)
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