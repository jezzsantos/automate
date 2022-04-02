using System;
using System.Collections.Generic;
using System.Linq;
using Automate.CLI;
using Automate.CLI.Application;
using Automate.CLI.Domain;
using Automate.CLI.Extensions;
using Automate.CLI.Infrastructure;
using FluentAssertions;
using Moq;
using Xunit;
using Attribute = Automate.CLI.Domain.Attribute;

namespace CLI.UnitTests.Application
{
    [Trait("Category", "Unit")]
    public class RuntimeApplicationSpec
    {
        private readonly RuntimeApplication application;
        private readonly Mock<IFilePathResolver> fileResolver;
        private readonly Mock<IPatternToolkitPackager> packager;
        private readonly PatternDefinition pattern;
        private readonly Mock<ISolutionPathResolver> solutionPathResolver;
        private readonly ISolutionStore solutionStore;
        private readonly ToolkitDefinition toolkit;
        private readonly IToolkitStore toolkitStore;

        public RuntimeApplicationSpec()
        {
            var repo = new MemoryRepository();
            this.toolkitStore = new ToolkitStore(repo, repo);
            this.solutionStore = new SolutionStore(repo, repo);
            this.fileResolver = new Mock<IFilePathResolver>();
            this.fileResolver.Setup(pr => pr.ExistsAtPath(It.IsAny<string>()))
                .Returns(true);
            this.packager = new Mock<IPatternToolkitPackager>();
            this.solutionPathResolver = new Mock<ISolutionPathResolver>();

            this.application =
                new RuntimeApplication(this.toolkitStore, this.solutionStore, this.fileResolver.Object,
                    this.packager.Object, this.solutionPathResolver.Object);
            this.pattern = new PatternDefinition("apatternname");
            this.toolkit = new ToolkitDefinition(this.pattern);
            this.toolkitStore.Import(this.toolkit);
        }

        [Fact]
        public void WhenConstructed_ThenCurrentSolutionIsNull()
        {
            this.application.CurrentSolutionId.Should().BeNull();
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
                .Returns(this.toolkit);

            var result = this.application.InstallToolkit("aninstallerlocation");

            result.Id.Should().Be(this.toolkit.Id);
        }

        [Fact]
        public void WhenCreateSolutionAndToolkitNotExist_ThenThrows()
        {
            this.application
                .Invoking(x => x.CreateSolution("atoolkitname", null))
                .Should().Throw<AutomateException>()
                .WithMessage(ExceptionMessages.RuntimeApplication_ToolkitNotFound.Format("atoolkitname"));
        }

        [Fact]
        public void WhenCreateSolution_ThenReturnsNewSolution()
        {
            var result = this.application.CreateSolution("apatternname", null);

            result.Id.Should().NotBeNull();
            result.PatternName.Should().Be("apatternname");
            result.Toolkit.Id.Should().Be(this.toolkit.Id);
            this.application.CurrentSolutionId.Should().Be(result.Id);
        }

        [Fact]
        public void WhenListInstalledToolkits_ThenReturnsToolkits()
        {
            var result = this.application.ListInstalledToolkits();

            result.Should().ContainSingle(tk => tk.Id == this.toolkit.Id);
        }

        [Fact]
        public void WhenListCreatedSolutions_ThenReturnsToolkits()
        {
            var solution = this.application.CreateSolution("apatternname", null);

            var result = this.application.ListCreatedSolutions();

            result.Should().Contain(solution);
        }

        [Fact]
        public void WhenSwitchCurrentSolutionAndSolutionNotExists_ThenThrows()
        {
            this.application
                .Invoking(x => x.SwitchCurrentSolution("asolutionid"))
                .Should().Throw<AutomateException>()
                .WithMessage(
                    ExceptionMessages.SolutionStore_NotFoundAtLocationWithId.Format("asolutionid",
                        MemoryRepository.InMemoryLocation));
        }

        [Fact]
        public void WhenSwitchCurrentSolution_ThenCurrentIsChanged()
        {
            var solution1 = this.application.CreateSolution("apatternname", null);
            this.application.CreateSolution("apatternname", null);

            this.application.SwitchCurrentSolution(solution1.Id);

            this.solutionStore.GetCurrent().Should().NotBeNull();
            this.application.CurrentSolutionId.Should().Be(solution1.Id);
        }

        [Fact]
        public void WhenConfigureSolutionAndCurrentSolutionNotExists_ThenThrows()
        {
            this.application
                .Invoking(x => x.ConfigureSolution(null, null, null, new Dictionary<string, string>()))
                .Should().Throw<AutomateException>()
                .WithMessage(ExceptionMessages.RuntimeApplication_NoCurrentSolution);
        }

        [Fact]
        public void WhenConfigureSolutionAndNoAddElementNorAddToCollectionNorOnElementNorAnyAssignments_ThenThrows()
        {
            var solution = this.application.CreateSolution("apatternname", null);

            this.application
                .Invoking(x => x.ConfigureSolution(null, null, null, null))
                .Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage(
                    ExceptionMessages.RuntimeApplication_ConfigureSolution_NoChanges.Format(
                        solution.Id) + "*");
        }

        [Fact]
        public void WhenConfigureSolutionAndBothAddElementAndAddToCollection_ThenThrows()
        {
            var solution = this.application.CreateSolution("apatternname", null);

            this.application
                .Invoking(x => x.ConfigureSolution("anelementexpression", "acollectionexpression", null, null))
                .Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage(
                    ExceptionMessages.RuntimeApplication_ConfigureSolution_AddAndAddTo.Format(
                        solution.Id, "anelementexpression", "acollectionexpression") + "*");
        }

        [Fact]
        public void WhenConfigureSolutionAndBothAddElementAndOnElement_ThenThrows()
        {
            var solution = this.application.CreateSolution("apatternname", null);

            this.application
                .Invoking(x => x.ConfigureSolution("anelementexpression", null, "anelementexpression", null))
                .Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage(
                    ExceptionMessages.RuntimeApplication_ConfigureSolution_OnAndAdd.Format(
                        solution.Id, "anelementexpression", "anelementexpression") + "*");
        }

        [Fact]
        public void WhenConfigureSolutionAndBothAddToCollectionAndOnElement_ThenThrows()
        {
            var solution = this.application.CreateSolution("apatternname", null);

            this.application
                .Invoking(x => x.ConfigureSolution(null, "acollectionexpression", "anelementexpression", null))
                .Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage(
                    ExceptionMessages.RuntimeApplication_ConfigureSolution_OnAndAddTo.Format(
                        solution.Id, "anelementexpression", "acollectionexpression") + "*");
        }

        [Fact]
        public void WhenConfigureSolutionAndAddElementButUnknown_ThenThrows()
        {
            this.application.CreateSolution("apatternname", null);
            this.solutionPathResolver.Setup(spr => spr.ResolveItem(It.IsAny<SolutionDefinition>(), It.IsAny<string>()))
                .Returns((SolutionItem)null);

            this.application
                .Invoking(x => x.ConfigureSolution("anelementexpression", null, null, null))
                .Should().Throw<AutomateException>()
                .WithMessage(
                    ExceptionMessages.RuntimeApplication_ItemExpressionNotFound.Format(
                        "apatternname", "anelementexpression"));
        }

        [Fact]
        public void WhenConfigureSolutionAndAddElementAlreadyMaterialised_ThenThrows()
        {
            this.application.CreateSolution("apatternname", null);
            this.pattern.AddElement("anelementname");
            this.solutionPathResolver.Setup(spr => spr.ResolveItem(It.IsAny<SolutionDefinition>(), It.IsAny<string>()))
                .Returns(new SolutionItem(this.toolkit, this.pattern.Elements.Single(), null).Materialise());

            this.application
                .Invoking(x => x.ConfigureSolution("anelementexpression", null, null, null))
                .Should().Throw<AutomateException>()
                .WithMessage(
                    ExceptionMessages.RuntimeApplication_ConfigureSolution_AddElementExists.Format(
                        "anelementexpression"));
        }

        [Fact]
        public void WhenConfigureSolutionWithChangeOnPattern_ThenReturnsSolution()
        {
            var attribute = new Attribute("anattributename", null);
            this.pattern.AddAttribute(attribute);
            UpdateToolkit();
            var solution = this.application.CreateSolution("apatternname", null);

            var result = this.application.ConfigureSolution(null, null, null,
                new Dictionary<string, string> { { "anattributename", "avalue" } });

            result.Id.Should().NotBeNull();
            solution.Model.Properties["anattributename"].Value.Should().Be("avalue");
        }

        [Fact]
        public void WhenConfigureSolutionWithNewElement_ThenReturnsSolution()
        {
            this.application.CreateSolution("apatternname", null);
            this.pattern.AddElement("anelementname");
            this.solutionPathResolver.Setup(spr => spr.ResolveItem(It.IsAny<SolutionDefinition>(), It.IsAny<string>()))
                .Returns(new SolutionItem(this.toolkit, this.pattern.Elements.Single(), null));

            var result = this.application.ConfigureSolution("apatternname.anelement", null, null, null);

            result.Id.Should().NotBeNull();
        }

        [Fact]
        public void WhenConfigureSolutionAndAddCollectionElementButUnknown_ThenThrows()
        {
            this.application.CreateSolution("apatternname", null);
            this.solutionPathResolver.Setup(spr => spr.ResolveItem(It.IsAny<SolutionDefinition>(), It.IsAny<string>()))
                .Returns((SolutionItem)null);

            this.application
                .Invoking(x => x.ConfigureSolution(null, "acollectionexpression", null, null))
                .Should().Throw<AutomateException>()
                .WithMessage(
                    ExceptionMessages.RuntimeApplication_ItemExpressionNotFound.Format(
                        "apatternname", "acollectionexpression"));
        }

        [Fact]
        public void WhenConfigureSolutionWithNewCollectionElement_ThenReturnsSolution()
        {
            this.application.CreateSolution("apatternname", null);
            this.pattern.AddElement("anelementname", isCollection: true);
            var solutionItem = new SolutionItem(this.toolkit, this.pattern.Elements.Single(), null);
            this.solutionPathResolver.Setup(spr => spr.ResolveItem(It.IsAny<SolutionDefinition>(), It.IsAny<string>()))
                .Returns(solutionItem);

            var result = this.application.ConfigureSolution(null, "apatternname.acollectionname", null, null);

            result.Id.Should().Be(solutionItem.Items.Single().Id);
        }

        [Fact]
        public void WhenConfigureSolutionAndOnElementButUnknown_ThenThrows()
        {
            this.application.CreateSolution("apatternname", null);
            this.solutionPathResolver.Setup(spr => spr.ResolveItem(It.IsAny<SolutionDefinition>(), It.IsAny<string>()))
                .Returns((SolutionItem)null);

            this.application
                .Invoking(x => x.ConfigureSolution(null, null, "anelementexpression", null))
                .Should().Throw<AutomateException>()
                .WithMessage(
                    ExceptionMessages.RuntimeApplication_ItemExpressionNotFound.Format(
                        "apatternname", "anelementexpression"));
        }

        [Fact]
        public void WhenConfigureSolutionAndOnElementAndNotMaterialised_ThenThrows()
        {
            this.application.CreateSolution("apatternname", null);
            this.solutionPathResolver.Setup(spr => spr.ResolveItem(It.IsAny<SolutionDefinition>(), It.IsAny<string>()))
                .Returns(new SolutionItem(this.toolkit, new Element("anelementname"), null));

            this.application
                .Invoking(x => x.ConfigureSolution(null, null, "anelementexpression", null))
                .Should().Throw<AutomateException>()
                .WithMessage(
                    ExceptionMessages.RuntimeApplication_ConfigureSolution_OnElementNotExists.Format(
                        "anelementexpression"));
        }

        [Fact]
        public void WhenConfigureSolutionWithAddElementAndProperty_ThenReturnsSolution()
        {
            var attribute = new Attribute("anattributename");
            var element = new Element("anelementname");
            element.AddAttribute(attribute);
            this.pattern.AddElement(element);
            UpdateToolkit();
            var solution = this.application.CreateSolution("apatternname", null);
            var solutionItem = solution.Model.Properties["anelementname"];
            this.solutionPathResolver.Setup(spr => spr.ResolveItem(It.IsAny<SolutionDefinition>(), It.IsAny<string>()))
                .Returns(solutionItem);

            var result = this.application.ConfigureSolution("anelementexpression", null, null,
                new Dictionary<string, string> { { "anattributename", "avalue" } });

            result.Id.Should().Be(solutionItem.Id);
            solutionItem.Properties["anattributename"].Value.Should().Be("avalue");
        }

        [Fact]
        public void WhenConfigureSolutionWithOnElementAndProperty_ThenReturnsSolution()
        {
            var attribute = new Attribute("anattributename");
            var element = new Element("anelementname");
            element.AddAttribute(attribute);
            this.pattern.AddElement(element);
            UpdateToolkit();
            var solution = this.application.CreateSolution("apatternname", null);
            var solutionItem = solution.Model.Properties["anelementname"].Materialise();
            this.solutionPathResolver.Setup(spr => spr.ResolveItem(It.IsAny<SolutionDefinition>(), It.IsAny<string>()))
                .Returns(solutionItem);

            var result = this.application.ConfigureSolution(null, null, "anelementexpression",
                new Dictionary<string, string> { { "anattributename", "avalue" } });

            result.Id.Should().Be(solutionItem.Id);
            solutionItem.Properties["anattributename"].Value.Should().Be("avalue");
        }

        [Fact]
        public void WhenGetConfigurationAndCurrentSolutionNotExists_ThenThrows()
        {
            this.application
                .Invoking(x => x.GetConfiguration(false, false))
                .Should().Throw<AutomateException>()
                .WithMessage(ExceptionMessages.RuntimeApplication_NoCurrentSolution);
        }

        [Fact]
        public void WhenGetConfiguration_ThenReturnsConfiguration()
        {
            var attribute1 = new Attribute("anattributename1", null, false, "adefaultvalue1");
            var attribute2 = new Attribute("anattributename2", null, false, "adefaultvalue2");
            var element1 = new Element("anelementname1", "adisplayname1", "adescription1");
            var element2 = new Element("anelementname2", "adisplayname2", "adescription2");
            element2.AddAttribute(attribute2);
            element1.AddElement(element2);
            this.pattern.AddAttribute(attribute1);
            this.pattern.AddElement(element1);
            UpdateToolkit();
            var solution = this.application.CreateSolution("apatternname", null);
            solution.Model.Properties["anelementname1"].Materialise();
            solution.Model.Properties["anelementname1"].Properties["anelementname2"].Materialise();

            this.solutionStore.Save(solution);

            var result = this.application.GetConfiguration(false, false);

            result.Pattern.Should().BeNull();
            result.Validation.Should().BeEquivalentTo(ValidationResults.None);
            result.Configuration.Should().Be(new
            {
                id = solution.Model.Id,
                anattributename1 = "adefaultvalue1",
                anelementname1 = new
                {
                    id = solution.Model.Properties["anelementname1"].Id,
                    anelementname2 = new
                    {
                        id = solution.Model.Properties["anelementname1"].Properties["anelementname2"].Id,
                        anattributename2 = "adefaultvalue2"
                    }
                }
            }.ToJson<dynamic>());
        }

        [Fact]
        public void WhenGetConfigurationWithSchemaAndValidation_ThenReturnsConfiguration()
        {
            var attribute1 = new Attribute("anattributename1", null, false, "adefaultvalue1");
            var attribute2 = new Attribute("anattributename2", null, false, "adefaultvalue2");
            var element1 = new Element("anelementname1", "adisplayname1", "adescription1");
            var element2 = new Element("anelementname2", "adisplayname2", "adescription2");
            element2.AddAttribute(attribute2);
            element1.AddElement(element2);
            this.pattern.AddAttribute(attribute1);
            this.pattern.AddElement(element1);
            UpdateToolkit();
            var solution = this.application.CreateSolution("apatternname", null);
            solution.Model.Properties["anelementname1"].Materialise();
            solution.Model.Properties["anelementname1"].Properties["anelementname2"].Materialise();

            this.solutionStore.Save(solution);

            var result = this.application.GetConfiguration(true, true);

            result.Pattern.Should().Be(this.pattern);
            result.Validation.Results.Should().BeEmpty();
            result.Configuration.Should().Be(new
            {
                id = solution.Model.Id,
                anattributename1 = "adefaultvalue1",
                anelementname1 = new
                {
                    id = solution.Model.Properties["anelementname1"].Id,
                    anelementname2 = new
                    {
                        id = solution.Model.Properties["anelementname1"].Properties["anelementname2"].Id,
                        anattributename2 = "adefaultvalue2"
                    }
                }
            }.ToJson<dynamic>());
        }

        [Fact]
        public void WhenValidateSolutionAndSolutionNotExist_ThenThrows()
        {
            this.application
                .Invoking(x => x.Validate(null))
                .Should().Throw<AutomateException>()
                .WithMessage(ExceptionMessages.RuntimeApplication_NoCurrentSolution);
        }

        [Fact]
        public void WhenValidateSolutionAndElementNotExist_ThenThrows()
        {
            this.application.CreateSolution("apatternname", null);
            this.solutionPathResolver.Setup(spr => spr.ResolveItem(It.IsAny<SolutionDefinition>(), It.IsAny<string>()))
                .Returns((SolutionItem)null);

            this.application
                .Invoking(x => x.Validate("anelementexpression"))
                .Should().Throw<AutomateException>()
                .WithMessage(ExceptionMessages.RuntimeApplication_ItemExpressionNotFound.Format("apatternname", "anelementexpression"));
        }

        [Fact]
        public void WhenValidateElement_ThenReturnsResults()
        {
            var element1 = new Element("anelementname1", "adisplayname1", "adescription1");
            var element2 = new Element("anelementname2", "adisplayname2", "adescription2");
            this.pattern.AddElement(element1);
            this.pattern.AddElement(element2);
            UpdateToolkit();
            var solution = this.application.CreateSolution("apatternname", null);
            this.solutionPathResolver.Setup(spr => spr.ResolveItem(It.IsAny<SolutionDefinition>(), It.IsAny<string>()))
                .Returns(solution.Model.Properties["anelementname1"]);

            var result = this.application.Validate("{anelementname}");

            result.Results.Count.Should().Be(1);
            result.Results.Should().Contain(r => r.Context.Path == "{apatternname.anelementname1}" &&
                                                 r.Message == ValidationMessages
                                                     .SolutionItem_ValidationRule_ElementRequiresAtLeastOneInstance);
        }

        [Fact]
        public void WhenValidateSolution_ThenReturnsResults()
        {
            var attribute1 = new Attribute("anattributename1", null, true, "adefaultvalue1");
            var attribute2 = new Attribute("anattributename2", null, true, "adefaultvalue2");
            var element1 = new Element("anelementname1", "adisplayname1", "adescription1");
            var collection2 = new Element("acollectionname2", "adisplayname2", "adescription2", true,
                ElementCardinality.OneOrMany);
            collection2.AddAttribute(attribute2);
            this.pattern.AddAttribute(attribute1);
            this.pattern.AddElement(element1);
            this.pattern.AddElement(collection2);
            UpdateToolkit();
            var solution = this.application.CreateSolution("apatternname", null);
            this.solutionPathResolver.Setup(spr => spr.ResolveItem(It.IsAny<SolutionDefinition>(), It.IsAny<string>()))
                .Returns(solution.Model);

            var result = this.application.Validate(null);

            result.Results.Count.Should().Be(2);
            result.Results.Should().Contain(r => r.Context.Path == "{apatternname.anelementname1}" &&
                                                 r.Message == ValidationMessages
                                                     .SolutionItem_ValidationRule_ElementRequiresAtLeastOneInstance);
            result.Results.Should().Contain(r => r.Context.Path == "{apatternname.acollectionname2}" &&
                                                 r.Message == ValidationMessages
                                                     .SolutionItem_ValidationRule_ElementRequiresAtLeastOneInstance);
        }

        [Fact]
        public void WhenExecuteLaunchPointAndSolutionNotExist_ThenThrows()
        {
            this.application
                .Invoking(x => x.ExecuteLaunchPoint("acommandname", null))
                .Should().Throw<AutomateException>()
                .WithMessage(ExceptionMessages.RuntimeApplication_NoCurrentSolution);
        }

        [Fact]
        public void WhenExecuteLaunchPointAndElementNotExist_ThenThrows()
        {
            this.application.CreateSolution("apatternname", null);
            this.solutionPathResolver.Setup(spr => spr.ResolveItem(It.IsAny<SolutionDefinition>(), It.IsAny<string>()))
                .Returns((SolutionItem)null);

            this.application
                .Invoking(x => x.ExecuteLaunchPoint("acommandname", "anelementexpression"))
                .Should().Throw<AutomateException>()
                .WithMessage(ExceptionMessages.RuntimeApplication_ItemExpressionNotFound.Format("apatternname", "anelementexpression"));
        }

        [Fact]
        public void WhenExecuteLaunchPointOnElement_ThenReturnsResult()
        {
            var element = new Element("anelementname", "adisplayname", "adescription");
            var automation = new Automation("acommandname", AutomationType.TestingOnly, new Dictionary<string, object>());
            element.AddAutomation(automation);
            this.pattern.AddElement(element);
            UpdateToolkit();
            var solution = this.application.CreateSolution("apatternname", null);

            this.solutionPathResolver.Setup(spr => spr.ResolveItem(It.IsAny<SolutionDefinition>(), It.IsAny<string>()))
                .Returns(solution.Model.Properties["anelementname"].Materialise());

            var result = this.application.ExecuteLaunchPoint("acommandname", "anelementname");

            result.CommandName.Should().Be("acommandname");
            result.Log.Should().ContainSingle("testingonly");
        }

        [Fact]
        public void WhenExecuteLaunchPointOnSolution_ThenReturnsResult()
        {
            var automation = new Automation("acommandname", AutomationType.TestingOnly, new Dictionary<string, object>());
            this.pattern.AddAutomation(automation);
            UpdateToolkit();
            var solution = this.application.CreateSolution("apatternname", null);
            this.solutionPathResolver.Setup(spr => spr.ResolveItem(It.IsAny<SolutionDefinition>(), It.IsAny<string>()))
                .Returns(solution.Model);

            var result = this.application.ExecuteLaunchPoint("acommandname", null);

            result.CommandName.Should().Be("acommandname");
            result.Log.Should().ContainSingle("testingonly");
        }

        [Fact]
        public void WhenUpgradeSolutionAndSolutionNotExist_ThenThrows()
        {
            this.application
                .Invoking(x => x.UpgradeSolution(false))
                .Should().Throw<AutomateException>()
                .WithMessage(ExceptionMessages.RuntimeApplication_NoCurrentSolution);
        }

        [Fact]
        public void WhenUpgradeSolutionAndNothingToUpgrade_ThenSolutionUpgraded()
        {
            this.application.CreateSolution("apatternname", null);

            var result = this.application.UpgradeSolution(false);

            result.IsSuccess.Should().BeTrue();
            result.Log.Should().ContainSingle(x =>
                x.Type == MigrationChangeType.Abort
                && x.MessageTemplate == MigrationMessages.SolutionDefinition_Upgrade_SameToolkitVersion);
            result.Solution.Id.Should().Be(this.application.CurrentSolutionId);
        }

        private void UpdateToolkit()
        {
            this.toolkitStore.DestroyAll();
            this.toolkitStore.Import(new ToolkitDefinition(this.pattern));
        }
    }
}