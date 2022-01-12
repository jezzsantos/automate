using System.Linq;
using automate;
using automate.Extensions;
using FluentAssertions;
using Moq;
using Xunit;

namespace CLI.UnitTests
{
    [Trait("Category", "Unit")]
    public class PatternApplicationSpec
    {
        private readonly PatternApplication application;
        private readonly Mock<IPathResolver> pathResolver;
        private readonly PatternStore store;

        public PatternApplicationSpec()
        {
            this.pathResolver = new Mock<IPathResolver>();
            this.pathResolver.Setup(pr => pr.CreatePath(It.IsAny<string>(), It.IsAny<string>()))
                .Returns("afullpath");
            this.pathResolver.Setup(pr => pr.ExistsAtPath(It.IsAny<string>()))
                .Returns(true);
            this.store = new PatternStore(new MemoryRepository());
            this.application = new PatternApplication(this.store, this.pathResolver.Object);
        }

        [Fact]
        public void WhenConstructed_ThenPropertiesAssigned()
        {
            this.application.CurrentPatternId.Should().BeNull();
        }

        [Fact]
        public void WhenCreateNewPatternAndAlreadyExists_ThenThrows()
        {
            this.application.CreateNewPattern("aname");

            this.application
                .Invoking(x => x.CreateNewPattern("aname"))
                .Should().Throw<PatternException>()
                .WithMessage(ExceptionMessages.PatternStore_FoundNamed.Format("aname"));
        }

        [Fact]
        public void WhenCreateNewPatternAndNotExists_ThenExists()
        {
            this.application.CreateNewPattern("aname");

            this.store.GetCurrent().Should().NotBeNull();
            this.application.CurrentPatternId.Should().NotBeNull();
        }

        [Fact]
        public void WhenSwitchCurrentPatternAndNotExists_ThenThrows()
        {
            this.store.DestroyAll();

            this.application
                .Invoking(x => x.SwitchCurrentPattern("aname"))
                .Should().Throw<PatternException>()
                .WithMessage(
                    ExceptionMessages.PatternStore_NotFoundAtLocationWithId.Format("aname",
                        MemoryRepository.InMemoryLocation));
        }

        [Fact]
        public void WhenSwitchCurrentPatternAndExists_ThenCurrentIsChanged()
        {
            this.application.CreateNewPattern("aname1");
            this.application.CreateNewPattern("aname2");

            this.application.SwitchCurrentPattern("aname1");

            this.store.GetCurrent().Should().NotBeNull();
            this.application.CurrentPatternName.Should().Be("aname1");
        }

        [Fact]
        public void WhenAttachCodeTemplateAndTemplateNotExists_ThenThrows()
        {
            this.store.Create("aname");
            this.pathResolver.Setup(pr => pr.ExistsAtPath(It.IsAny<string>()))
                .Returns(false);

            this.application
                .Invoking(x => x.AttachCodeTemplate("arootpath", "arelativepath", "atemplatename"))
                .Should().Throw<PatternException>()
                .WithMessage(
                    ExceptionMessages.PatternApplication_CodeTemplate_NotFoundAtLocation.Format("arootpath",
                        "arelativepath"));
        }

        [Fact]
        public void WhenAttachCodeTemplateAndTemplateWithSameName_ThenThrows()
        {
            this.store.Create("aname");
            this.application.AttachCodeTemplate("arootpath", "arelativepath", "atemplatename");

            this.application
                .Invoking(x => x.AttachCodeTemplate("arootpath", "arelativepath", "atemplatename"))
                .Should().Throw<PatternException>()
                .WithMessage(ExceptionMessages.PatternApplication_CodeTemplateByNameExists.Format("atemplatename"));
        }

        [Fact]
        public void WhenAttachCodeTemplateAndCurrentPatternNotExists_ThenThrows()
        {
            this.application
                .Invoking(x => x.AttachCodeTemplate("arootpath", "arelativepath", "atemplatename"))
                .Should().Throw<PatternException>()
                .WithMessage(ExceptionMessages.PatternApplication_NoCurrentPattern);
        }

        [Fact]
        public void WhenAttachCodeTemplate_ThenTemplateAdded()
        {
            this.store.Create("aname");
            this.application.AttachCodeTemplate("arootpath", "arelativepath", "atemplatename");

            this.store.GetCurrent().CodeTemplates.Single().Name.Should().Be("atemplatename");
            this.store.GetCurrent().CodeTemplates.Single().FullPath.Should().Be("afullpath");
        }

        [Fact]
        public void WhenListCodeTemplatesAndCurrentPatternNotExists_ThenThrows()
        {
            this.application
                .Invoking(x => x.ListCodeTemplates())
                .Should().Throw<PatternException>()
                .WithMessage(ExceptionMessages.PatternApplication_NoCurrentPattern);
        }

        [Fact]
        public void WhenListCodeTemplates_ThenListsTemplates()
        {
            this.store.Create("aname");
            this.application.AttachCodeTemplate("arootpath", "arelativepath", "atemplatename1");
            this.application.AttachCodeTemplate("arootpath", "arelativepath", "atemplatename2");
            this.application.AttachCodeTemplate("arootpath", "arelativepath", "atemplatename3");

            var result = this.application.ListCodeTemplates();

            result.Should().Contain(template => template.Name == "atemplatename1");
            result.Should().Contain(template => template.Name == "atemplatename2");
            result.Should().Contain(template => template.Name == "atemplatename3");
        }
    }
}