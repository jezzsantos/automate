using System;
using System.IO;
using Automate.CLI.Infrastructure;
using FluentAssertions;
using Xunit;

namespace CLI.UnitTests.Infrastructure
{
    [Trait("Category", "Unit")]
    public class CliAssemblyMetadataSpec
    {
        [Fact]
        public void WhenLocalStatePathAndSameAsDotNetToolsPath_ThenModifiedPath()
        {
            var dotnetToolsPath =
                Environment.ExpandEnvironmentVariables(CliRuntimeMetadata.DotNetToolsInstallationPath);

            var result = CliRuntimeMetadata.CalculateLocalStatePath(dotnetToolsPath);

            result.Should().Be(Path.Combine(Path.GetFullPath(dotnetToolsPath),
                $"_{CliRuntimeMetadata.RootPersistencePath}"));
        }

        [Fact]
        public void WhenLocalStatePathAndNotDotNetToolsPath_ThenNormalPath()
        {
            var result = CliRuntimeMetadata.CalculateLocalStatePath(Environment.CurrentDirectory);

            result.Should().Be(Path.Combine(Environment.CurrentDirectory, CliRuntimeMetadata.RootPersistencePath));
        }
    }
}