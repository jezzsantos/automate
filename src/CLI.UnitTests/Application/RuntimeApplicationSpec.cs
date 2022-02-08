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

        public RuntimeApplicationSpec()
        {
            var store = new Mock<IToolkitStore>();
            this.fileResolver = new Mock<IFilePathResolver>();
            this.fileResolver.Setup(pr => pr.ExistsAtPath(It.IsAny<string>()))
                .Returns(true);
            this.packager = new Mock<IPatternToolkitPackager>();

            this.application = new RuntimeApplication(store.Object, this.fileResolver.Object, this.packager.Object);
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
                .Should().Throw<PatternException>()
                .WithMessage(
                    ExceptionMessages.RuntimeApplication_ToolkitInstallerNotFound.Format("aninstallerlocation"));
        }

        [Fact]
        public void WhenInstallToolkit_ThenReturnsInstalledToolkit()
        {
            this.packager.Setup(pkg => pkg.UnPack(It.IsAny<IFile>()))
                .Returns(new PatternToolkitDefinition
                {
                    Id = "atoolkitid"
                });

            var result = this.application.InstallToolkit("aninstallerlocation");

            result.Id.Should().Be("atoolkitid");
        }
    }
}