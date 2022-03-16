using System;
using Automate.CLI;
using Automate.CLI.Domain;
using Automate.CLI.Extensions;
using FluentAssertions;
using Xunit;

namespace CLI.UnitTests.Domain
{
    public class ToolkitVersionSpec
    {
        [Trait("Category", "Unit")]
        public class GivenNoContext
        {
            private readonly ToolkitVersion version;

            public GivenNoContext()
            {
                this.version = new ToolkitVersion();
            }

            [Fact]
            public void WhenConstructed_ThenInitialVersion()
            {
                this.version.Current.Should().Be("0.0.0");
                this.version.LastChanges.Should().Be(VersionChange.NoChange);
                this.version.ChangeLog.Should().BeEmpty();
            }

            [Fact]
            public void WhenUpdateVersionWithZeroVersion_ThenThrows()
            {
                this.version
                    .Invoking(x => x.UpdateVersion(new VersionInstruction("0.0.0")))
                    .Should().Throw<AutomateException>()
                    .WithMessage(ExceptionMessages.ToolkitVersion_ZeroVersion.Format("0.0.0"));
            }

            [Fact]
            public void WhenUpdateVersionWithInvalidInstruction_ThenThrows()
            {
                this.version
                    .Invoking(x => x.UpdateVersion(new VersionInstruction("invalidinstruction")))
                    .Should().Throw<ArgumentOutOfRangeException>()
                    .WithMessage(ExceptionMessages.VersionInstruction_InvalidVersionInstruction.Format("invalidinstruction") + "*");
            }

            [Fact]
            public void WhenUpdateVersionWithLowerVersionThanCurrent_ThenThrows()
            {
                this.version.UpdateVersion(new VersionInstruction("0.2.0"));

                this.version
                    .Invoking(x => x.UpdateVersion(new VersionInstruction("0.1.0")))
                    .Should().Throw<AutomateException>()
                    .WithMessage(ExceptionMessages.ToolkitVersion_VersionBeforeCurrent.Format("0.1.0", "0.2.0"));
            }

            [Fact]
            public void WhenUpdateVersionWithZeroDotNumber_ThenThrows()
            {
                this.version
                    .Invoking(x => x.UpdateVersion(new VersionInstruction("2")))
                    .Should().Throw<ArgumentOutOfRangeException>()
                    .WithMessage(ExceptionMessages.VersionInstruction_InvalidVersionInstruction.Format("2") + "*");
            }

            [Fact]
            public void WhenUpdateVersionWithOneDotNumber_ThenReturnsVersion()
            {
                var result = this.version.UpdateVersion(new VersionInstruction("1.1"));

                result.Version.Should().Be("1.1.0");
                result.Message.Should().BeNull();
                this.version.Current.Should().Be(result.Version);
                this.version.LastChanges.Should().Be(VersionChange.NoChange);
                this.version.ChangeLog.Should().BeEmpty();
            }

            [Fact]
            public void WhenUpdateVersionWithTwoDotNumber_ThenReturnsVersion()
            {
                var result = this.version.UpdateVersion(new VersionInstruction("1.1.1"));

                result.Version.Should().Be("1.1.1");
                result.Message.Should().BeNull();
                this.version.Current.Should().Be(result.Version);
                this.version.LastChanges.Should().Be(VersionChange.NoChange);
                this.version.ChangeLog.Should().BeEmpty();
            }

            [Fact]
            public void WhenUpdateVersionWithThreeDotNumber_ThenThrows()
            {
                this.version
                    .Invoking(x => x.UpdateVersion(new VersionInstruction("1.1.1.1")))
                    .Should().Throw<ArgumentOutOfRangeException>()
                    .WithMessage(ExceptionMessages.VersionInstruction_InvalidVersionInstruction.Format("1.1.1.1") + "*");
            }
        }

        [Trait("Category", "Unit")]
        public class GivenNoChange
        {
            private readonly ToolkitVersion version;

            public GivenNoChange()
            {
                this.version = new ToolkitVersion();
            }

            [Fact]
            public void WhenRegisterChangeWithNoChange_ThenStillNoChanges()
            {
                this.version.RegisterChange(VersionChange.NoChange, "achange");

                this.version.LastChanges.Should().Be(VersionChange.NoChange);
                this.version.ChangeLog.Should().BeEmpty();
            }

            [Fact]
            public void WhenRegisterChangeWithNonBreakingChange_ThenNonBreaking()
            {
                this.version.RegisterChange(VersionChange.NonBreaking, "achange");

                this.version.LastChanges.Should().Be(VersionChange.NonBreaking);
                this.version.ChangeLog.Should().ContainSingle(entry => entry.Change == VersionChange.NonBreaking && entry.Description == "achange");
            }

            [Fact]
            public void WhenRegisterChangeWithBreakingChange_ThenBreaking()
            {
                this.version.RegisterChange(VersionChange.Breaking, "achange");

                this.version.LastChanges.Should().Be(VersionChange.Breaking);
                this.version.ChangeLog.Should().ContainSingle(entry => entry.Change == VersionChange.Breaking && entry.Description == "achange");
            }

            [Fact]
            public void WhenUpdateVersionWithNoInstruction_ThenReturnsNextMinor()
            {
                var result = this.version.UpdateVersion(new VersionInstruction());

                result.Version.Should().Be("0.1.0");
                result.Message.Should().BeNull();
                this.version.Current.Should().Be(result.Version);
                this.version.LastChanges.Should().Be(VersionChange.NoChange);
                this.version.ChangeLog.Should().BeEmpty();
            }

            [Fact]
            public void WhenUpdateVersionWithAutoInstruction_ThenReturnsNextMinor()
            {
                var result = this.version.UpdateVersion(new VersionInstruction(ToolkitVersion.AutoIncrementInstruction));

                result.Version.Should().Be("0.1.0");
                result.Message.Should().BeNull();
                this.version.Current.Should().Be(result.Version);
                this.version.LastChanges.Should().Be(VersionChange.NoChange);
                this.version.ChangeLog.Should().BeEmpty();
            }

            [Fact]
            public void WhenUpdateVersionWithSameVersionAsCurrent_ThenReturnsVersionWithWarning()
            {
                var result = this.version.UpdateVersion(new VersionInstruction("0.1.0"));

                result.Version.Should().Be("0.1.0");
                result.Message.Should().BeNull();
                this.version.Current.Should().Be(result.Version);
                this.version.LastChanges.Should().Be(VersionChange.NoChange);
                this.version.ChangeLog.Should().BeEmpty();
            }

            [Fact]
            public void WhenUpdateVersionWithGreaterMinorVersionThanCurrent_ThenReturnsNewVersion()
            {
                var result = this.version.UpdateVersion(new VersionInstruction("0.10.0"));

                result.Version.Should().Be("0.10.0");
                result.Message.Should().BeNull();
                this.version.Current.Should().Be(result.Version);
                this.version.LastChanges.Should().Be(VersionChange.NoChange);
                this.version.ChangeLog.Should().BeEmpty();
            }

            [Fact]
            public void WhenUpdateVersionWithGreaterMajorVersionThanCurrent_ThenReturnsNewVersion()
            {
                var result = this.version.UpdateVersion(new VersionInstruction("10.0.0"));

                result.Version.Should().Be("10.0.0");
                result.Message.Should().BeNull();
                this.version.Current.Should().Be(result.Version);
                this.version.LastChanges.Should().Be(VersionChange.NoChange);
                this.version.ChangeLog.Should().BeEmpty();
            }
        }

        [Trait("Category", "Unit")]
        public class GivenNonBreakingChange
        {
            private readonly ToolkitVersion version;

            public GivenNonBreakingChange()
            {
                this.version = new ToolkitVersion();
                this.version.RegisterChange(VersionChange.NonBreaking, "anonbreakingchange");
            }

            [Fact]
            public void WhenRegisterChangeWithNoChange_ThenStillNonBreaking()
            {
                this.version.RegisterChange(VersionChange.NoChange, "achange");

                this.version.LastChanges.Should().Be(VersionChange.NonBreaking);
                this.version.ChangeLog.Count.Should().Be(1);
                this.version.ChangeLog.Should().ContainSingle(entry => entry.Change == VersionChange.NonBreaking && entry.Description == "anonbreakingchange");
            }

            [Fact]
            public void WhenRegisterChangeWithNonBreakingChange_ThenStillNonBreaking()
            {
                this.version.RegisterChange(VersionChange.NonBreaking, "achange");

                this.version.LastChanges.Should().Be(VersionChange.NonBreaking);
                this.version.ChangeLog.Count.Should().Be(2);
                this.version.ChangeLog.Should().ContainSingle(entry => entry.Change == VersionChange.NonBreaking && entry.Description == "anonbreakingchange");
                this.version.ChangeLog.Should().ContainSingle(entry => entry.Change == VersionChange.NonBreaking && entry.Description == "achange");
            }

            [Fact]
            public void WhenRegisterChangeWithBreakingChange_ThenBreaking()
            {
                this.version.RegisterChange(VersionChange.Breaking, "achange");

                this.version.LastChanges.Should().Be(VersionChange.Breaking);
                this.version.ChangeLog.Count.Should().Be(2);
                this.version.ChangeLog.Should().ContainSingle(entry => entry.Change == VersionChange.NonBreaking && entry.Description == "anonbreakingchange");
                this.version.ChangeLog.Should().ContainSingle(entry => entry.Change == VersionChange.Breaking && entry.Description == "achange");
            }

            [Fact]
            public void WhenUpdateVersionWithNoInstruction_ThenReturnsNextMinor()
            {
                var result = this.version.UpdateVersion(new VersionInstruction());

                result.Version.Should().Be("0.1.0");
                result.Message.Should().BeNull();
                this.version.Current.Should().Be(result.Version);
                this.version.LastChanges.Should().Be(VersionChange.NoChange);
                this.version.ChangeLog.Should().BeEmpty();
            }

            [Fact]
            public void WhenUpdateVersionWithAutoInstruction_ThenReturnsNextMinor()
            {
                var result = this.version.UpdateVersion(new VersionInstruction(ToolkitVersion.AutoIncrementInstruction));

                result.Version.Should().Be("0.1.0");
                result.Message.Should().BeNull();
                this.version.Current.Should().Be(result.Version);
                this.version.LastChanges.Should().Be(VersionChange.NoChange);
                this.version.ChangeLog.Should().BeEmpty();
            }

            [Fact]
            public void WhenUpdateVersionWithSameVersionAsCurrent_ThenReturnsNewVersionWithWarning()
            {
                var result = this.version.UpdateVersion(new VersionInstruction("0.1.0"));

                result.Version.Should().Be("0.1.0");
                result.Message.Should().Be(DomainMessages.ToolkitVersion_Warning.Format("0.1.0", "* anonbreakingchange"));
                this.version.Current.Should().Be(result.Version);
                this.version.LastChanges.Should().Be(VersionChange.NoChange);
                this.version.ChangeLog.Should().BeEmpty();
            }

            [Fact]
            public void WhenUpdateVersionWithGreaterMinorVersionThanCurrent_ThenReturnsNewVersion()
            {
                var result = this.version.UpdateVersion(new VersionInstruction("0.10.0"));

                result.Version.Should().Be("0.10.0");
                result.Message.Should().BeNull();
                this.version.Current.Should().Be(result.Version);
                this.version.LastChanges.Should().Be(VersionChange.NoChange);
                this.version.ChangeLog.Should().BeEmpty();
            }

            [Fact]
            public void WhenUpdateVersionWithGreaterMajorVersionThanCurrent_ThenReturnsNewVersion()
            {
                var result = this.version.UpdateVersion(new VersionInstruction("10.0.0"));

                result.Version.Should().Be("10.0.0");
                result.Message.Should().BeNull();
                this.version.Current.Should().Be(result.Version);
                this.version.LastChanges.Should().Be(VersionChange.NoChange);
                this.version.ChangeLog.Should().BeEmpty();
            }
        }

        [Trait("Category", "Unit")]
        public class GivenBreakingChange
        {
            private readonly ToolkitVersion version;

            public GivenBreakingChange()
            {
                this.version = new ToolkitVersion();
                this.version.RegisterChange(VersionChange.Breaking, "abreakingchange");
            }

            [Fact]
            public void WhenRegisterChangeWithNoChange_ThenStillBreaking()
            {
                this.version.RegisterChange(VersionChange.NoChange, "achange");

                this.version.LastChanges.Should().Be(VersionChange.Breaking);
                this.version.ChangeLog.Count.Should().Be(1);
                this.version.ChangeLog.Should().ContainSingle(entry => entry.Change == VersionChange.Breaking && entry.Description == "abreakingchange");
            }

            [Fact]
            public void WhenRegisterChangeWithNonBreakingChange_ThenStillBreaking()
            {
                this.version.RegisterChange(VersionChange.NonBreaking, "achange");

                this.version.LastChanges.Should().Be(VersionChange.Breaking);
                this.version.ChangeLog.Count.Should().Be(2);
                this.version.ChangeLog.Should().ContainSingle(entry => entry.Change == VersionChange.Breaking && entry.Description == "abreakingchange");
                this.version.ChangeLog.Should().ContainSingle(entry => entry.Change == VersionChange.NonBreaking && entry.Description == "achange");
            }

            [Fact]
            public void WhenRegisterChangeWithBreakingChange_ThenStillBreaking()
            {
                this.version.RegisterChange(VersionChange.Breaking, "achange");

                this.version.LastChanges.Should().Be(VersionChange.Breaking);
                this.version.ChangeLog.Count.Should().Be(2);
                this.version.ChangeLog.Should().ContainSingle(entry => entry.Change == VersionChange.Breaking && entry.Description == "abreakingchange");
                this.version.ChangeLog.Should().ContainSingle(entry => entry.Change == VersionChange.Breaking && entry.Description == "achange");
            }

            [Fact]
            public void WhenUpdateVersionWithNoInstruction_ThenReturnsNextMajor()
            {
                var result = this.version.UpdateVersion(new VersionInstruction());

                result.Version.Should().Be("1.0.0");
                result.Message.Should().BeNull();
                this.version.Current.Should().Be(result.Version);
                this.version.LastChanges.Should().Be(VersionChange.NoChange);
                this.version.ChangeLog.Should().BeEmpty();
            }

            [Fact]
            public void WhenUpdateVersionWithAutoInstruction_ThenReturnsNextMajor()
            {
                var result = this.version.UpdateVersion(new VersionInstruction(ToolkitVersion.AutoIncrementInstruction));

                result.Version.Should().Be("1.0.0");
                result.Message.Should().BeNull();
                this.version.Current.Should().Be(result.Version);
                this.version.LastChanges.Should().Be(VersionChange.NoChange);
                this.version.ChangeLog.Should().BeEmpty();
            }

            [Fact]
            public void WhenUpdateVersionWithSameVersionAsCurrent_ThenThrows()
            {
                this.version
                    .Invoking(x => x.UpdateVersion(new VersionInstruction("0.1.0")))
                    .Should().Throw<AutomateException>()
                    .WithMessage(ExceptionMessages.ToolkitVersion_IllegalVersion.Format("0.1.0", "1.0.0", "abreakingchange"));
            }

            [Fact]
            public void WhenUpdateVersionWithSameVersionAsCurrentAndForce_ThenReturnsSameVersion()
            {
                var result = this.version.UpdateVersion(new VersionInstruction("0.1.0", true));

                result.Version.Should().Be("0.1.0");
                result.Message.Should().Be(DomainMessages.ToolkitVersion_Forced.Format("0.1.0", "* abreakingchange"));
                this.version.Current.Should().Be(result.Version);
                this.version.LastChanges.Should().Be(VersionChange.NoChange);
                this.version.ChangeLog.Should().BeEmpty();
            }

            [Fact]
            public void WhenUpdateVersionWithGreaterMinorVersionThanCurrent_ThenThrows()
            {
                this.version
                    .Invoking(x => x.UpdateVersion(new VersionInstruction("0.10.0")))
                    .Should().Throw<AutomateException>()
                    .WithMessage(ExceptionMessages.ToolkitVersion_IllegalVersion.Format("0.10.0", "1.0.0", "abreakingchange"));
            }

            [Fact]
            public void WhenUpdateVersionWithGreaterMinorVersionAsCurrentAndForce_ThenReturnsSameVersion()
            {
                var result = this.version.UpdateVersion(new VersionInstruction("0.10.0", true));

                result.Version.Should().Be("0.10.0");
                result.Message.Should().Be(DomainMessages.ToolkitVersion_Forced.Format("0.10.0", "* abreakingchange"));
                this.version.Current.Should().Be(result.Version);
                this.version.LastChanges.Should().Be(VersionChange.NoChange);
                this.version.ChangeLog.Should().BeEmpty();
            }

            [Fact]
            public void WhenUpdateVersionWithGreaterMajorVersionThanCurrent_ThenReturnsNewVersion()
            {
                var result = this.version.UpdateVersion(new VersionInstruction("2.0.0"));

                result.Version.Should().Be("2.0.0");
                result.Message.Should().BeNull();
                this.version.Current.Should().Be(result.Version);
                this.version.LastChanges.Should().Be(VersionChange.NoChange);
                this.version.ChangeLog.Should().BeEmpty();
            }
        }
    }
}