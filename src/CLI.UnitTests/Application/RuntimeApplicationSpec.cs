using automate;
using automate.Application;
using automate.Domain;
using automate.Extensions;
using automate.Infrastructure;
using FluentAssertions;
using Moq;
using Xunit;

namespace CLI.UnitTests.Application
{
    [Trait("Category", "Unit")]
    public class RuntimeApplicationSpec
    {
        private readonly RuntimeApplication application;
        private readonly Mock<IFilePathResolver> fileResolver;
        private readonly Mock<IPatternToolkitPackager> packager;
        private readonly Mock<ISolutionStore> solutionStore;
        private readonly Mock<IToolkitStore> toolkitStore;

        public RuntimeApplicationSpec()
        {
            this.toolkitStore = new Mock<IToolkitStore>();
            this.solutionStore = new Mock<ISolutionStore>();
            this.fileResolver = new Mock<IFilePathResolver>();
            this.fileResolver.Setup(pr => pr.ExistsAtPath(It.IsAny<string>()))
                .Returns(true);
            this.packager = new Mock<IPatternToolkitPackager>();

            this.application =
                new RuntimeApplication(this.toolkitStore.Object, this.solutionStore.Object, this.fileResolver.Object,
                    this.packager.Object);
        }

        [Fact]
        public void WhenConstructed_ThenPropertiesAssigned()
        {
            this.application.CurrentToolkitId.Should().BeNull();
        }

        [Fact]
        public void WhenInstallToolkitAndFileNotExist_ThenThrows()
        {
            this.fileResolver.Setup(pr => pr.ExistsAtPath(It.IsAny<string>()))
                .Returns(false);

            this.application
                .Invoking(x => x.InstallToolkit("aninstallerlocation"))
                .Should().Throw<AutomateException>()
                .WithMessage(
                    ExceptionMessages.RuntimeApplication_ToolkitInstallerNotFound.Format("aninstallerlocation"));
        }

        [Fact]
        public void WhenInstallToolkit_ThenReturnsInstalledToolkit()
        {
            this.packager.Setup(pkg => pkg.UnPack(It.IsAny<IFile>()))
                .Returns(new ToolkitDefinition
                {
                    Id = "atoolkitid"
                });

            var result = this.application.InstallToolkit("aninstallerlocation");

            result.Id.Should().Be("atoolkitid");
        }

        [Fact]
        public void WhenCreateSolutionAndToolkitNotExist_ThenThrows()
        {
            this.toolkitStore.Setup(ts => ts.FindByName(It.IsAny<string>()))
                .Returns((ToolkitDefinition)null);

            this.application
                .Invoking(x => x.CreateSolution("atoolkitname"))
                .Should().Throw<AutomateException>()
                .WithMessage(ExceptionMessages.RuntimeApplication_ToolkitNotFound.Format("atoolkitname"));
        }

        [Fact]
        public void WhenCreateSolution_ThenReturnsNewSolution()
        {
            this.toolkitStore.Setup(ts => ts.FindByName(It.IsAny<string>()))
                .Returns(new ToolkitDefinition
                {
                    Id = "atoolkitid"
                });

            var result = this.application.CreateSolution("atoolkitname");

            result.Id.Should().NotBeNull();
            result.PatternName.Should().Be("atoolkitname");
            this.solutionStore.Verify(ss => ss.Save(It.Is<SolutionDefinition>(s =>
                s.ToolkitId == "atoolkitid"
                && s.PatternName == "atoolkitname"
            )));
        }
    }
}