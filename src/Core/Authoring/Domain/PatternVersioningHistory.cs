using System;
using System.Collections.Generic;
using System.Linq;
using Automate.Common;
using Automate.Common.Domain;
using Automate.Common.Extensions;
using Semver;

namespace Automate.Authoring.Domain
{
    public class PatternVersioningHistory : IPersistable
    {
        internal const string AutoIncrementInstruction = "auto";
        private static readonly SemVersion InitialVersionNumber = new(0, 0, 0);
        private readonly List<VersionChangeLog> changeLog;

        public PatternVersioningHistory()
        {
            Current = InitialVersionNumber.ToString();
            LastChanges = VersionChange.NoChange;
            this.changeLog = new List<VersionChangeLog>();
        }

        private PatternVersioningHistory(PersistableProperties properties,
            IPersistableFactory factory)
        {
            Current = properties.Rehydrate<string>(factory, nameof(Current));
            LastChanges = properties.Rehydrate<VersionChange>(factory, nameof(LastChanges));
            this.changeLog = properties.Rehydrate<List<VersionChangeLog>>(factory, nameof(ChangeLog));
        }

        public string Current { get; private set; }

        public string Next => EstimateNextVersion().ToString();

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

        public static PatternVersioningHistory Rehydrate(PersistableProperties properties,
            IPersistableFactory factory)
        {
            return new PatternVersioningHistory(properties, factory);
        }

        public void RegisterChange(VersionChange change, string description, params object[] args)
        {
            description.GuardAgainstNull(nameof(description));

            switch (change)
            {
                case VersionChange.NoChange:
                    this.changeLog.Add(new VersionChangeLog(change, description, args));
                    return;

                case VersionChange.NonBreaking:

                    switch (LastChanges)
                    {
                        case VersionChange.NoChange:
                            this.changeLog.Add(new VersionChangeLog(change, description, args));
                            LastChanges = VersionChange.NonBreaking;
                            break;

                        case VersionChange.NonBreaking:
                        case VersionChange.Breaking:
                            this.changeLog.Add(new VersionChangeLog(change, description, args));
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
                            this.changeLog.Add(new VersionChangeLog(change, description, args));
                            LastChanges = VersionChange.Breaking;
                            break;

                        case VersionChange.Breaking:
                            this.changeLog.Add(new VersionChangeLog(change, description, args));
                            return;

                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(change), change, null);
            }
        }

        public VersionUpdateResult UpdateVersion(VersionInstruction instruction)
        {
            instruction.GuardAgainstNull(nameof(instruction));

            var result = CalculateNewVersion(instruction);
            if (Current.ToSemVersion() == result.Version)
            {
                return result;
            }

            this.changeLog.Add(new VersionChangeLog(VersionChange.NoChange, VersionChanges.ToolkitVersion_NewVersion,
                Current, result.Version));
            Current = result.Version.ToString();
            ResetAfterUpdate();
            return result;
        }

        private void ResetAfterUpdate()
        {
            this.changeLog.Clear();
            LastChanges = VersionChange.NoChange;
        }

        private VersionUpdateResult CalculateNewVersion(VersionInstruction instruction)
        {
            var currentVersion = Current.ToSemVersion();

            var estimatedNextVersion = EstimateNextVersion(currentVersion);

            if (instruction.Instruction.HasNoValue())
            {
                return new VersionUpdateResult(estimatedNextVersion);
            }

            if (instruction.Instruction.EqualsIgnoreCase(AutoIncrementInstruction))
            {
                return new VersionUpdateResult(estimatedNextVersion);
            }

            if (!SemVersion.TryParse(instruction.Instruction, SemVersionStyles.Any, out var instructedVersion))
            {
                throw new AutomateException(
                    ExceptionMessages.VersionInstruction_InvalidVersionInstruction.Substitute(instruction));
            }

            if (instructedVersion == InitialVersionNumber)
            {
                throw new AutomateException(
                    ExceptionMessages.ToolkitVersion_ZeroVersion.Substitute(
                        instructedVersion.ToString()));
            }

            if (instructedVersion < currentVersion)
            {
                throw new AutomateException(
                    ExceptionMessages.ToolkitVersion_VersionBeforeCurrent.Substitute(instruction.Instruction,
                        currentVersion.ToString()));
            }

            if (instructedVersion <= estimatedNextVersion)
            {
                if (LastChanges == VersionChange.Breaking)
                {
                    if (instruction.Force)
                    {
                        return new VersionUpdateResult(instructedVersion,
                            DomainMessages.ToolkitVersion_Forced.Substitute(instruction.Instruction,
                                ChangeLog.ToBulletList(item => item.Message)));
                    }
                    throw new AutomateException(ExceptionMessages.ToolkitVersion_IllegalVersion.Substitute(
                        instruction.Instruction,
                        estimatedNextVersion.ToString(),
                        ChangeLog.ToMultiLineText(item => item.Message)));
                }

                if (LastChanges == VersionChange.NonBreaking)
                {
                    return new VersionUpdateResult(instructedVersion,
                        DomainMessages.ToolkitVersion_Warning.Substitute(instruction.Instruction,
                            ChangeLog.ToBulletList(item => item.Message)));
                }
            }

            return new VersionUpdateResult(instructedVersion);
        }

        private SemVersion EstimateNextVersion()
        {
            return EstimateNextVersion(Current.ToSemVersion());
        }

        private SemVersion EstimateNextVersion(SemVersion currentVersion)
        {
            return LastChanges switch
            {
                VersionChange.NonBreaking => currentVersion.NextMinor(),
                VersionChange.Breaking => currentVersion.NextMajor(),
                _ => Current == InitialVersionNumber.ToString()
                    ? currentVersion.NextMinor()
                    : currentVersion
            };
        }
    }

    public class VersionChangeLog : IPersistable
    {
        public VersionChangeLog(VersionChange change, string description, params object[] args)
        {
            description.GuardAgainstNullOrEmpty(nameof(description));
            Change = change;
            MessageTemplate = description;
            Message = description.SubstituteTemplate(args);
            Arguments = args.Safe().Select(arg => arg.ToString()).ToList();
        }

        private VersionChangeLog(PersistableProperties properties,
            IPersistableFactory factory)
        {
            Change = properties.Rehydrate<VersionChange>(factory, nameof(Change));
            Message = properties.Rehydrate<string>(factory, nameof(Message));
            MessageTemplate = properties.Rehydrate<string>(factory, nameof(MessageTemplate));
            Arguments = properties.Rehydrate<List<string>>(factory, nameof(Arguments));
        }

        public VersionChange Change { get; }

        public string Message { get; }

        public string MessageTemplate { get; }

        public IReadOnlyList<string> Arguments { get; }

        public PersistableProperties Dehydrate()
        {
            var properties = new PersistableProperties();
            properties.Dehydrate(nameof(Change), Change);
            properties.Dehydrate(nameof(Message), Message);
            properties.Dehydrate(nameof(MessageTemplate), MessageTemplate);
            properties.Dehydrate(nameof(Arguments), Arguments);

            return properties;
        }

        public static VersionChangeLog Rehydrate(PersistableProperties properties,
            IPersistableFactory factory)
        {
            return new VersionChangeLog(properties, factory);
        }
    }

    public class VersionUpdateResult
    {
        public VersionUpdateResult(SemVersion version, string message = null)
        {
            Version = version;
            Message = message;
        }

        public SemVersion Version { get; }

        public string Message { get; }
    }
}