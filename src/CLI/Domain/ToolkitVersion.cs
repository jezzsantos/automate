using System;
using System.Collections.Generic;
using Automate.CLI.Extensions;

namespace Automate.CLI.Domain
{
    internal class ToolkitVersion : IPersistable
    {
        internal const int VersionFieldCount = 3;
        internal const string AutoIncrementInstruction = "auto";
        internal static readonly Version InitialVersionNumber = new Version(0, 0, 0);
        private readonly List<VersionChangeLog> changeLog;

        public ToolkitVersion()
        {
            Current = InitialVersionNumber.ToString(VersionFieldCount);
            LastChanges = VersionChange.NoChange;
            this.changeLog = new List<VersionChangeLog>();
        }

        private ToolkitVersion(PersistableProperties properties, IPersistableFactory factory)
        {
            Current = properties.Rehydrate<string>(factory, nameof(Current));
            LastChanges = properties.Rehydrate<VersionChange>(factory, nameof(LastChanges));
            this.changeLog = properties.Rehydrate<List<VersionChangeLog>>(factory, nameof(ChangeLog));
        }

        public string Current { get; private set; }

        public VersionChange LastChanges { get; private set; }

        public IReadOnlyList<VersionChangeLog> ChangeLog => this.changeLog;

        public PersistableProperties Dehydrate()
        {
            var properties = new PersistableProperties();
            properties.Dehydrate(nameof(Current), Current);
            properties.Dehydrate(nameof(LastChanges), LastChanges);
            properties.Dehydrate(nameof(ChangeLog), ChangeLog);

            return properties;
        }

        public static ToolkitVersion Rehydrate(PersistableProperties properties, IPersistableFactory factory)
        {
            return new ToolkitVersion(properties, factory);
        }

        public void RegisterChange(VersionChange change, string description)
        {
            description.GuardAgainstNull(nameof(description));

            switch (change)
            {
                case VersionChange.NoChange:
                    return;

                case VersionChange.NonBreaking:

                    switch (LastChanges)
                    {
                        case VersionChange.NoChange:
                            this.changeLog.Add(new VersionChangeLog(change, description));
                            LastChanges = VersionChange.NonBreaking;
                            break;

                        case VersionChange.NonBreaking:
                        case VersionChange.Breaking:
                            this.changeLog.Add(new VersionChangeLog(change, description));
                            return;

                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    break;

                case VersionChange.Breaking:
                    switch (LastChanges)
                    {
                        case VersionChange.NoChange:
                        case VersionChange.NonBreaking:
                            this.changeLog.Add(new VersionChangeLog(change, description));
                            LastChanges = VersionChange.Breaking;
                            break;

                        case VersionChange.Breaking:
                            this.changeLog.Add(new VersionChangeLog(change, description));
                            return;

                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(change), change, null);
            }
        }

        public VersionUpdateResult UpdateVersion(string versionInstruction)
        {
            versionInstruction.GuardAgainstInvalid(_ => Validations.IsVersionInstruction(versionInstruction), nameof(versionInstruction),
                ExceptionMessages.ToolkitVersion_InvalidVersionInstruction);

            var result = CalculateNewVersion(versionInstruction);

            Current = result.Version;
            ResetAfterUpdate();
            return result;
        }

        private void ResetAfterUpdate()
        {
            this.changeLog.Clear();
            LastChanges = VersionChange.NoChange;
        }

        private VersionUpdateResult CalculateNewVersion(string versionInstruction)
        {
            var currentVersion = new Version(Current);

            var expectedNewVersion = LastChanges == VersionChange.Breaking
                ? currentVersion.RevMajor()
                : currentVersion.RevMinor();

            if (versionInstruction.HasNoValue())
            {
                return new VersionUpdateResult(expectedNewVersion);
            }

            if (versionInstruction.EqualsIgnoreCase(AutoIncrementInstruction))
            {
                return new VersionUpdateResult(expectedNewVersion);
            }

            if (!Version.TryParse(versionInstruction, out var requestedVersion))
            {
                throw new AutomateException(
                    ExceptionMessages.ToolkitVersion_InvalidVersionInstruction.Format(versionInstruction));
            }

            requestedVersion = requestedVersion.To2Dot();

            if (requestedVersion == InitialVersionNumber)
            {
                throw new AutomateException(
                    ExceptionMessages.ToolkitVersion_ZeroVersion.Format(requestedVersion.ToString(VersionFieldCount)));
            }

            if (requestedVersion < currentVersion)
            {
                throw new AutomateException(
                    ExceptionMessages.ToolkitVersion_VersionBeforeCurrent.Format(versionInstruction,
                        currentVersion.ToString(VersionFieldCount)));
            }

            if (requestedVersion <= expectedNewVersion)
            {
                if (LastChanges == VersionChange.Breaking)
                {
                    throw new AutomateException(ExceptionMessages.ToolkitVersion_IllegalVersion.Format(versionInstruction,
                        expectedNewVersion.ToString(VersionFieldCount), ChangeLog.ToMultiLineText(item => item.Description)));
                }

                if (LastChanges == VersionChange.NonBreaking)
                {
                    return new VersionUpdateResult(requestedVersion,
                        DomainMessages.ToolkitVersion_Warning.Format(versionInstruction,
                            ChangeLog.ToMultiLineText(item => item.Description)));
                }
            }

            return new VersionUpdateResult(requestedVersion);
        }
    }

    internal class VersionChangeLog : IPersistable
    {
        public VersionChangeLog(VersionChange change, string description)
        {
            description.GuardAgainstNullOrEmpty(nameof(description));
            Description = description;
            Change = change;
        }

        private VersionChangeLog(PersistableProperties properties, IPersistableFactory factory)
        {
            Description = properties.Rehydrate<string>(factory, nameof(Description));
            Change = properties.Rehydrate<VersionChange>(factory, nameof(Change));
        }

        public string Description { get; }

        public VersionChange Change { get; }

        public PersistableProperties Dehydrate()
        {
            var properties = new PersistableProperties();
            properties.Dehydrate(nameof(Description), Description);
            properties.Dehydrate(nameof(Change), Change);

            return properties;
        }

        public static VersionChangeLog Rehydrate(PersistableProperties properties, IPersistableFactory factory)
        {
            return new VersionChangeLog(properties, factory);
        }
    }

    internal class VersionUpdateResult
    {
        private readonly Version version;

        public VersionUpdateResult(Version version, string message = null)
        {
            this.version = new Version(version.Major, version.Minor, version.BuildOrZero());
            Message = message;
        }

        public string Version => this.version.ToString(ToolkitVersion.VersionFieldCount);

        public string Message { get; }
    }
}