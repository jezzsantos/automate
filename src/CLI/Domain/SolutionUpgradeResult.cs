﻿using System.Collections.Generic;
using Automate.CLI.Extensions;

namespace Automate.CLI.Domain
{
    internal class SolutionUpgradeResult
    {
        private readonly List<MigrationChange> log;

        public SolutionUpgradeResult(SolutionDefinition solution, string fromVersion, string toVersion) : this(solution, fromVersion, toVersion, new List<MigrationChange>())
        {
        }

        public SolutionUpgradeResult(SolutionDefinition solution, string fromVersion, string toVersion, List<MigrationChange> log)
        {
            solution.GuardAgainstNull(nameof(solution));
            fromVersion.GuardAgainstNull(nameof(fromVersion));
            toVersion.GuardAgainstNull(nameof(toVersion));
            log.GuardAgainstNull(nameof(log));

            IsSuccess = true;
            Solution = solution;
            this.log = log;
            FromVersion = fromVersion;
            ToVersion = toVersion;
        }

        public bool IsSuccess { get; private set; }

        public SolutionDefinition Solution { get; }

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

        public void Append(SolutionUpgradeResult result)
        {
            result.GuardAgainstNull(nameof(result));

            this.log.AddRange(result.Log);
            if (!result.IsSuccess)
            {
                Fail();
            }
        }
    }

    internal class MigrationChange
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

    internal enum MigrationChangeType
    {
        Abort = 0,
        NonBreaking = 1,
        Breaking = 2
    }
}