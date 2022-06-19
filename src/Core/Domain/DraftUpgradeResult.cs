using System.Collections.Generic;
using Automate.Extensions;

namespace Automate.Domain
{
    public class DraftUpgradeResult
    {
        private readonly List<MigrationChange> log;

        public DraftUpgradeResult(DraftDefinition draft, string fromVersion, string toVersion) : this(draft,
            fromVersion, toVersion, new List<MigrationChange>())
        {
        }

        public DraftUpgradeResult(DraftDefinition draft, string fromVersion, string toVersion,
            List<MigrationChange> log)
        {
            draft.GuardAgainstNull(nameof(draft));
            fromVersion.GuardAgainstNull(nameof(fromVersion));
            toVersion.GuardAgainstNull(nameof(toVersion));
            log.GuardAgainstNull(nameof(log));

            IsSuccess = true;
            Draft = draft;
            this.log = log;
            FromVersion = fromVersion;
            ToVersion = toVersion;
        }

        public bool IsSuccess { get; private set; }

        public DraftDefinition Draft { get; }

        public IReadOnlyList<MigrationChange> Log => this.log;

        public string FromVersion { get; }

        public string ToVersion { get; }

        public void Fail()
        {
            IsSuccess = false;
        }

        public void Fail(MigrationChangeType type, string messageTemplate, params object[] args)
        {
            Fail();
            Add(type, messageTemplate, args);
        }

        public void Add(MigrationChange change)
        {
            change.GuardAgainstNull(nameof(change));

            this.log.Add(change);
        }

        public void Add(MigrationChangeType type, string messageTemplate, params object[] args)
        {
            Add(new MigrationChange(type, messageTemplate, args));
        }

        public void Append(DraftUpgradeResult result)
        {
            result.GuardAgainstNull(nameof(result));

            this.log.AddRange(result.Log);
            if (!result.IsSuccess)
            {
                Fail();
            }
        }
    }

    public class MigrationChange
    {
        public MigrationChange(MigrationChangeType type, string messageTemplate, params object[] args)
        {
            messageTemplate.GuardAgainstNullOrEmpty(nameof(messageTemplate));

            Type = type;
            MessageTemplate = messageTemplate;
            Arguments = args;
        }

        public MigrationChangeType Type { get; }

        public string MessageTemplate { get; }

        public IReadOnlyList<object> Arguments { get; }
    }

    public enum MigrationChangeType
    {
        Abort = 0,
        NonBreaking = 1,
        Breaking = 2
    }
}