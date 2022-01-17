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
        private readonly Mock<IFilePathResolver> filePathResolver;
        private readonly Mock<IPatternPathResolver> patternPathResolver;
        private readonly PatternStore store;

        public PatternApplicationSpec()
        {
            this.filePathResolver = new Mock<IFilePathResolver>();
            this.filePathResolver.Setup(pr => pr.CreatePath(It.IsAny<string>(), It.IsAny<string>()))
                .Returns("afullpath");
            this.filePathResolver.Setup(pr => pr.ExistsAtPath(It.IsAny<string>()))
                .Returns(true);
            this.patternPathResolver = new Mock<IPatternPathResolver>();
            this.patternPathResolver.Setup(ppr => ppr.Resolve(It.IsAny<PatternMetaModel>(), It.IsAny<string>()))
                .Returns((PatternMetaModel model, string _) => model);
            this.store = new PatternStore(new MemoryRepository());
            this.application =
                new PatternApplication(this.store, this.filePathResolver.Object, this.patternPathResolver.Object);
        }

        [Fact]
        public void WhenConstructed_ThenPropertiesAssigned()
        {
            this.application.CurrentPatternId.Should().BeNull();
        }

        [Fact]
        public void WhenCreateNewPatternAndAlreadyExists_ThenThrows()
        {
            this.application.CreateNewPattern("apatternname");

            this.application
                .Invoking(x => x.CreateNewPattern("apatternname"))
                .Should().Throw<PatternException>()
                .WithMessage(ExceptionMessages.PatternStore_FoundNamed.Format("apatternname"));
        }

        [Fact]
        public void WhenCreateNewPatternAndNotExists_ThenExists()
        {
            this.application.CreateNewPattern("apatternname");

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
            this.filePathResolver.Setup(pr => pr.ExistsAtPath(It.IsAny<string>()))
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

        [Fact]
        public void WhenAddAttributeAndCurrentPatternNotExists_ThenThrows()
        {
            this.application
                .Invoking(x => x.AddAttribute("anattributename", null, null, false, null, null))
                .Should().Throw<PatternException>()
                .WithMessage(ExceptionMessages.PatternApplication_NoCurrentPattern);
        }

        [Fact]
        public void WhenAddAttributeAndAlreadyExists_ThenThrows()
        {
            this.application.CreateNewPattern("apatternname");
            this.application.AddAttribute("anattributename", null, null, false, null, null);

            this.application
                .Invoking(x => x.AddAttribute("anattributename", null, null, false, null, null))
                .Should().Throw<PatternException>()
                .WithMessage(ExceptionMessages.PatternApplication_AttributeByNameExists.Format("anattributename"));
        }

        [Fact]
        public void WhenAddAttribute_TheAddsAndReturnsPatternId()
        {
            this.application.CreateNewPattern("apatternname");

            var result = this.application.AddAttribute("anattributename", "string", "adefaultvalue", false, null, null);

            var attribute = this.store.GetCurrent().Attributes.Single();
            attribute.Name.Should().Be("anattributename");
            attribute.Type.Should().Be("string");
            attribute.DefaultValue.Should().Be("adefaultvalue");
            attribute.IsRequired.Should().BeFalse();
            attribute.Choices.Should().BeEmpty();
            result.Id.Should().Be(this.store.GetCurrent().Id);
        }

        [Fact]
        public void WhenAddAttributeAndDefaultValueIsNotOneOfChoices_ThenThrows()
        {
            this.application.CreateNewPattern("apatternname");

            this.application
                .Invoking(x =>
                    x.AddAttribute("anattributename", null, "adefaultvalue", false, "avalue1;avalue2;avalue3", null))
                .Should().Throw<PatternException>()
                .WithMessage(ExceptionMessages.PatternApplication_AttributeDefaultValueIsNotAChoice);
        }

        [Fact]
        public void WhenAddAttributeAndNoType_TheAddsAttributeWithDefaultType()
        {
            this.application.CreateNewPattern("apatternname");

            this.application.AddAttribute("anattributename", null, null, false, null, null);

            var attribute = this.store.GetCurrent().Attributes.Single();
            attribute.Name.Should().Be("anattributename");
            attribute.Type.Should().Be("string");
            attribute.DefaultValue.Should().BeNull();
            attribute.IsRequired.Should().BeFalse();
            attribute.Choices.Should().BeEmpty();
        }

        [Fact]
        public void WhenAddAttributeAndDefaultValueIsOneOfChoices_ThenAddsAttribute()
        {
            this.application.CreateNewPattern("apatternname");

            this.application.AddAttribute("anattributename", null, "avalue2", false, "avalue1;avalue2;avalue3", null);

            var attribute = this.store.GetCurrent().Attributes.Single();
            attribute.DefaultValue.Should().Be("avalue2");
            attribute.IsRequired.Should().BeFalse();
            attribute.Choices.Should().ContainInOrder("avalue1", "avalue2", "avalue3");
        }

        [Fact]
        public void WhenAddAttributeAndParentNotExists_ThenThrows()
        {
            this.patternPathResolver.Setup(ppr => ppr.Resolve(It.IsAny<PatternMetaModel>(), It.IsAny<string>()))
                .Returns((PatternMetaModel)null);

            this.application.CreateNewPattern("apatternname");

            this.application
                .Invoking(x =>
                    x.AddAttribute("anattributename", null, null, false, null, "anunknownparent"))
                .Should().Throw<PatternException>()
                .WithMessage(
                    ExceptionMessages.PatternApplication_NodeExpressionNotFound.Format("anunknownparent"));
        }

        [Fact]
        public void WhenAddAttributeAndParentIsElement_ThenAddsAttributeToElement()
        {
            this.application.CreateNewPattern("apatternname");
            this.application.AddElement("anelementname", null, null, false, null);
            var parentElement = new Element("anelementname", null, null, false);
            this.patternPathResolver
                .Setup(ppr => ppr.Resolve(It.IsAny<PatternMetaModel>(), "{apatternname.anelementname}"))
                .Returns(parentElement);

            var result = this.application.AddAttribute("anattributename", null, null, false, null,
                "{apatternname.anelementname}");

            var attribute = result.Attributes.Single();
            attribute.Name.Should().Be("anattributename");
            result.Id.Should().Be(parentElement.Id);
        }

        [Fact]
        public void WhenAddElementAndCurrentPatternNotExists_ThenThrows()
        {
            this.application
                .Invoking(x => x.AddElement("anelementname", null, null, false, null))
                .Should().Throw<PatternException>()
                .WithMessage(ExceptionMessages.PatternApplication_NoCurrentPattern);
        }

        [Fact]
        public void WhenAddElementAndAlreadyExists_ThenThrows()
        {
            this.application.CreateNewPattern("apatternname");
            this.application.AddElement("anelementname", null, null, false, null);

            this.application
                .Invoking(x => x.AddElement("anelementname", null, null, false, null))
                .Should().Throw<PatternException>()
                .WithMessage(ExceptionMessages.PatternApplication_ElementByNameExists.Format("anelementname"));
        }

        [Fact]
        public void WhenAddElement_TheAddsAndReturnsPatternId()
        {
            this.application.CreateNewPattern("apatternname");

            var result = this.application.AddElement("anelementname", "adisplayname", "adescription", false, null);

            var element = this.store.GetCurrent().Elements.Single();
            element.Name.Should().Be("anelementname");
            element.DisplayName.Should().Be("adisplayname");
            element.Description.Should().Be("adescription");
            element.IsCollection.Should().BeFalse();
            result.Id.Should().Be(this.store.GetCurrent().Id);
        }

        [Fact]
        public void WhenAddElementAndParentNotExists_ThenThrows()
        {
            this.patternPathResolver.Setup(ppr => ppr.Resolve(It.IsAny<PatternMetaModel>(), It.IsAny<string>()))
                .Returns((PatternMetaModel)null);

            this.application.CreateNewPattern("apatternname");

            this.application
                .Invoking(x =>
                    x.AddElement("anelementname", null, null, false, "anunknownparent"))
                .Should().Throw<PatternException>()
                .WithMessage(
                    ExceptionMessages.PatternApplication_NodeExpressionNotFound.Format("anunknownparent"));
        }

        [Fact]
        public void WhenAddElementAndParentIsElement_ThenAddsElementToElement()
        {
            this.application.CreateNewPattern("apatternname");
            this.application.AddElement("aparentelementname", null, null, false, null);
            var parentElement = new Element("aparentelementname", null, null, false);
            this.patternPathResolver.Setup(ppr =>
                    ppr.Resolve(It.IsAny<PatternMetaModel>(), "{apatternname.aparentelementname}"))
                .Returns(parentElement);

            var result =
                this.application.AddElement("achildelementname", null, null, false,
                    "{apatternname.aparentelementname}");

            var element = result.Elements.Single();
            element.Name.Should().Be("achildelementname");
            result.Id.Should().Be(parentElement.Id);
        }
    }
}