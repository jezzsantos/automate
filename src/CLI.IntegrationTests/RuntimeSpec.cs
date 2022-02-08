using System;
using System.IO;
using automate;
using automate.Extensions;
using automate.Infrastructure;
using Xunit;

namespace CLI.IntegrationTests
{
    [Trait("Category", "Integration")] [Collection("CLI")]
    public class RuntimeSpec
    {
        private readonly CliTestSetup setup;

        public RuntimeSpec(CliTestSetup setup)
        {
            this.setup = setup;
            this.setup.ResetRepository();
        }

        [Fact]
        public void WhenInstallNoCommands_ThenDisplaysError()
        {
            this.setup.RunCommand($"{Program.InstallCommandName}");

            this.setup.Should().DisplayErrorForMissingCommand();
        }

        [Fact]
        public void WhenRunNoCommands_ThenDisplaysError()
        {
            this.setup.RunCommand($"{Program.RunCommandName}");

            this.setup.Should().DisplayErrorForMissingCommand();
        }

        [Fact]
        public void WhenUsingNoCommands_ThenDisplaysError()
        {
            this.setup.RunCommand($"{Program.UsingCommandName}");

            this.setup.Should().DisplayErrorForMissingCommand();
        }

        [Fact]
        public void WhenInstallToolkit_ThenInstallsToolkit()
        {
            var desktopFolder = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            var location = Path.Combine(desktopFolder, "apattern_1.0.toolkit");
            this.setup.RunCommand($"{Program.CreateCommandName} pattern apattern");
            this.setup.RunCommand($"{Program.BuildCommandName} toolkit");

            this.setup.RunCommand($"{Program.InstallCommandName} toolkit {location}");

            this.setup.Should().DisplayNoError();
            this.setup.Should()
                .DisplayMessage(
                    OutputMessages.CommandLine_Output_InstalledToolkit.FormatTemplate("apattern", "1.0.0"));
        }
    }
}