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
using ServiceStack;
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
        private readonly Mock<ISolutionPathResolver> solutionPathResolver;
        private readonly ISolutionStore solutionStore;
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

            this.toolkitStore.Import(new ToolkitDefinition
            {
                Id = "atoolkitid",
                Pattern = new PatternDefinition("apatternname")
            });
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
            this.application
                .Invoking(x => x.CreateSolution("atoolkitname"))
                .Should().Throw<AutomateException>()
                .WithMessage(ExceptionMessages.RuntimeApplication_ToolkitNotFound.Format("atoolkitname"));
        }

        [Fact]
        public void WhenCreateSolution_ThenReturnsNewSolution()
        {
            var result = this.application.CreateSolution("apatternname");

            result.Id.Should().NotBeNull();
            result.PatternName.Should().Be("apatternname");
            result.Toolkit.Id.Should().Be("atoolkitid");
            this.application.CurrentSolutionId.Should().Be(result.Id);
        }

        [Fact]
        public void WhenListInstalledToolkits_ThenReturnsToolkits()
        {
            var result = this.application.ListInstalledToolkits();

            result.Should().ContainSingle(toolkit => toolkit.Id == "atoolkitid");
        }

        [Fact]
        public void WhenListCreatedSolutions_ThenReturnsToolkits()
        {
            var solution = this.application.CreateSolution("apatternname");

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
            var solution1 = this.application.CreateSolution("apatternname");
            this.application.CreateSolution("apatternname");

            this.application.SwitchCurrentSolution(solution1.Id);

            this.solutionStore.GetCurrent().Should().NotBeNull();
            this.application.CurrentSolutionId.Should().Be(solution1.Id);
        }

        [Fact]
        public void WhenConfigureSolutionAndCurrentSolutionNotExists_ThenThrows()
        {
            this.application
                .Invoking(x => x.ConfigureSolution(null, null, null, new List<string>()))
                .Should().Throw<AutomateException>()
                .WithMessage(ExceptionMessages.RuntimeApplication_NoCurrentSolution);
        }

        [Fact]
        public void WhenConfigureSolutionAndNoAddElementNorAddToCollectionNorOnElementNorAnyAssignments_ThenThrows()
        {
            var solution = this.application.CreateSolution("apatternname");

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
            var solution = this.application.CreateSolution("apatternname");

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
            var solution = this.application.CreateSolution("apatternname");

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
            var solution = this.application.CreateSolution("apatternname");

            this.application
                .Invoking(x => x.ConfigureSolution(null, "acollectionexpression", "anelementexpression", null))
                .Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage(
                    ExceptionMessages.RuntimeApplication_ConfigureSolution_OnAndAddTo.Format(
                        solution.Id, "anelementexpression", "acollectionexpression") + "*");
        }

        [Fact]
        public void WhenConfigureSolutionAndAnyPropertyAssigmentInvalid_ThenThrows()
        {
            var solution = this.application.CreateSolution("apatternname");

            this.application
                .Invoking(x => x.ConfigureSolution("anelementexpression", null, null, new List<string>
                {
                    "notavalidpropertyassignment"
                }))
                .Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage(
                    ExceptionMessages.RuntimeApplication_ConfigureSolution_PropertyAssignmentInvalid.Format(
                        "notavalidpropertyassignment", solution.Id) + "*");
        }

        [Fact]
        public void WhenConfigureSolutionAndAddElementButUnknown_ThenThrows()
        {
            this.application.CreateSolution("apatternname");
            this.solutionPathResolver.Setup(spr => spr.ResolveItem(It.IsAny<SolutionDefinition>(), It.IsAny<string>()))
                .Returns((SolutionItem)null);

            this.application
                .Invoking(x => x.ConfigureSolution("anelementexpression", null, null, null))
                .Should().Throw<AutomateException>()
                .WithMessage(
                    ExceptionMessages.RuntimeApplication_ElementExpressionNotFound.Format(
                        "apatternname", "anelementexpression"));
        }

        [Fact]
        public void WhenConfigureSolutionAndAddElementAlreadyMaterialised_ThenThrows()
        {
            this.application.CreateSolution("apatternname");
            this.solutionPathResolver.Setup(spr => spr.ResolveItem(It.IsAny<SolutionDefinition>(), It.IsAny<string>()))
                .Returns(new SolutionItem { IsMaterialised = true });

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
            var pattern = new PatternDefinition("apatternname");
            pattern.Attributes.Add(attribute);
            UpdateToolkit(pattern);
            var solution = this.application.CreateSolution("apatternname");

            var result = this.application.ConfigureSolution(null, null, null,
                new List<string> { "anattributename=avalue" });

            result.Id.Should().NotBeNull();
            solution.Model.Properties["anattributename"].Value.Should().Be("avalue");
        }

        [Fact]
        public void WhenConfigureSolutionWithNewElement_ThenReturnsSolution()
        {
            this.application.CreateSolution("apatternname");
            this.solutionPathResolver.Setup(spr => spr.ResolveItem(It.IsAny<SolutionDefinition>(), It.IsAny<string>()))
                .Returns(new SolutionItem(new Element("anelementname"), null));

            var result = this.application.ConfigureSolution("apatternname.anelement", null, null, null);

            result.Id.Should().NotBeNull();
        }

        [Fact]
        public void WhenConfigureSolutionAndAddCollectionElementButUnknown_ThenThrows()
        {
            this.application.CreateSolution("apatternname");
            this.solutionPathResolver.Setup(spr => spr.ResolveItem(It.IsAny<SolutionDefinition>(), It.IsAny<string>()))
                .Returns((SolutionItem)null);

            this.application
                .Invoking(x => x.ConfigureSolution(null, "acollectionexpression", null, null))
                .Should().Throw<AutomateException>()
                .WithMessage(
                    ExceptionMessages.RuntimeApplication_ElementExpressionNotFound.Format(
                        "apatternname", "acollectionexpression"));
        }

        [Fact]
        public void WhenConfigureSolutionWithNewCollectionElement_ThenReturnsSolution()
        {
            this.application.CreateSolution("apatternname");
            var solutionItem = new SolutionItem(new Element("acollectionname", null, null, true), null);
            this.solutionPathResolver.Setup(spr => spr.ResolveItem(It.IsAny<SolutionDefinition>(), It.IsAny<string>()))
                .Returns(solutionItem);

            var result = this.application.ConfigureSolution(null, "apatternname.acollectionname", null, null);

            result.Id.Should().Be(solutionItem.Items.Single().Id);
        }

        [Fact]
        public void WhenConfigureSolutionAndOnElementButUnknown_ThenThrows()
        {
            this.application.CreateSolution("apatternname");
            this.solutionPathResolver.Setup(spr => spr.ResolveItem(It.IsAny<SolutionDefinition>(), It.IsAny<string>()))
                .Returns((SolutionItem)null);

            this.application
                .Invoking(x => x.ConfigureSolution(null, null, "anelementexpression", null))
                .Should().Throw<AutomateException>()
                .WithMessage(
                    ExceptionMessages.RuntimeApplication_ElementExpressionNotFound.Format(
                        "apatternname", "anelementexpression"));
        }

        [Fact]
        public void WhenConfigureSolutionAndOnElementAndNotMaterialised_ThenThrows()
        {
            this.application.CreateSolution("apatternname");
            this.solutionPathResolver.Setup(spr => spr.ResolveItem(It.IsAny<SolutionDefinition>(), It.IsAny<string>()))
                .Returns(new SolutionItem { IsMaterialised = false });

            this.application
                .Invoking(x => x.ConfigureSolution(null, null, "anelementexpression", null))
                .Should().Throw<AutomateException>()
                .WithMessage(
                    ExceptionMessages.RuntimeApplication_ConfigureSolution_OnElementNotExists.Format(
                        "anelementexpression"));
        }

        [Fact]
        public void WhenConfigureSolutionWithUnknownProperty_ThenThrows()
        {
            this.application.CreateSolution("apatternname");
            this.solutionPathResolver.Setup(spr => spr.ResolveItem(It.IsAny<SolutionDefinition>(), It.IsAny<string>()))
                .Returns(new SolutionItem(new Element("anelementname"), null));

            this.application
                .Invoking(x => x.ConfigureSolution("anelementexpression", null, null,
                    new List<string> { "anunknownname=avalue" }))
                .Should().Throw<AutomateException>()
                .WithMessage(
                    ExceptionMessages.RuntimeApplication_ConfigureSolution_ElementPropertyNotExists.Format(
                        "anelementname", "anunknownname"));
        }

        [Fact]
        public void WhenConfigureSolutionWithWithPropertyOfWrongChoice_ThenThrows()
        {
            var attribute = new Attribute("anattributename")
            {
                Choices = new List<string> { "avalue" }
            };
            var element = new Element("anelementname");
            element.Attributes.Add(attribute);
            var pattern = new PatternDefinition("apatternname");
            pattern.Elements.Add(element);
            UpdateToolkit(pattern);
            var solution = this.application.CreateSolution("apatternname");
            var solutionItem = solution.Model.Properties["anelementname"];
            this.solutionPathResolver.Setup(spr => spr.ResolveItem(It.IsAny<SolutionDefinition>(), It.IsAny<string>()))
                .Returns(solutionItem);

            this.application
                .Invoking(x => x.ConfigureSolution("anelementexpression", null, null,
                    new List<string> { "anattributename=awrongvalue" }))
                .Should().Throw<AutomateException>()
                .WithMessage(
                    ExceptionMessages.RuntimeApplication_ConfigureSolution_ElementPropertyValueIsNotOneOf.Format(
                        "anelementname", "anattributename", new List<string> { "avalue" }.Join(";"), "awrongvalue"));
        }

        [Fact]
        public void WhenConfigureSolutionWithPropertyOfWrongDataType_ThenThrows()
        {
            var attribute = new Attribute("anattributename", "int");
            var element = new Element("anelementname");
            element.Attributes.Add(attribute);
            var pattern = new PatternDefinition("apatternname");
            pattern.Elements.Add(element);
            UpdateToolkit(pattern);
            var solution = this.application.CreateSolution("apatternname");
            var solutionItem = solution.Model.Properties["anelementname"];
            this.solutionPathResolver.Setup(spr => spr.ResolveItem(It.IsAny<SolutionDefinition>(), It.IsAny<string>()))
                .Returns(solutionItem);

            this.application
                .Invoking(x =>
                    x.ConfigureSolution("anelementexpression", null, null,
                        new List<string> { "anattributename=astring" }))
                .Should().Throw<AutomateException>()
                .WithMessage(
                    ExceptionMessages.RuntimeApplication_ConfigureSolution_ElementPropertyValueNotCompatible.Format(
                        "anelementname", "anattributename", "int", "astring"));
        }

        [Fact]
        public void WhenConfigureSolutionWithNewElementAndPropertyChoice_ThenReturnsSolution()
        {
            var attribute = new Attribute("anattributename")
            {
                Choices = new List<string> { "avalue" }
            };
            var element = new Element("anelementname");
            element.Attributes.Add(attribute);
            var pattern = new PatternDefinition("apatternname");
            pattern.Elements.Add(element);
            UpdateToolkit(pattern);
            var solution = this.application.CreateSolution("apatternname");
            var solutionItem = solution.Model.Properties["anelementname"];
            this.solutionPathResolver.Setup(spr => spr.ResolveItem(It.IsAny<SolutionDefinition>(), It.IsAny<string>()))
                .Returns(solutionItem);

            var result = this.application.ConfigureSolution("anelementexpression", null, null,
                new List<string> { "anattributename=avalue" });

            result.Id.Should().Be(solutionItem.Id);
            solutionItem.Properties["anattributename"].Value.Should().Be("avalue");
        }

        [Fact]
        public void WhenConfigureSolutionWithAddElementAndProperty_ThenReturnsSolution()
        {
            var attribute = new Attribute("anattributename");
            var element = new Element("anelementname");
            element.Attributes.Add(attribute);
            var pattern = new PatternDefinition("apatternname");
            pattern.Elements.Add(element);
            UpdateToolkit(pattern);
            var solution = this.application.CreateSolution("apatternname");
            var solutionItem = solution.Model.Properties["anelementname"];
            this.solutionPathResolver.Setup(spr => spr.ResolveItem(It.IsAny<SolutionDefinition>(), It.IsAny<string>()))
                .Returns(solutionItem);

            var result = this.application.ConfigureSolution("anelementexpression", null, null,
                new List<string> { "anattributename=avalue" });

            result.Id.Should().Be(solutionItem.Id);
            solutionItem.Properties["anattributename"].Value.Should().Be("avalue");
        }

        [Fact]
        public void WhenConfigureSolutionWithOnElementAndProperty_ThenReturnsSolution()
        {
            var attribute = new Attribute("anattributename");
            var element = new Element("anelementname");
            element.Attributes.Add(attribute);
            var pattern = new PatternDefinition("apatternname");
            pattern.Elements.Add(element);
            UpdateToolkit(pattern);
            var solution = this.application.CreateSolution("apatternname");
            var solutionItem = solution.Model.Properties["anelementname"].Materialise();
            this.solutionPathResolver.Setup(spr => spr.ResolveItem(It.IsAny<SolutionDefinition>(), It.IsAny<string>()))
                .Returns(solutionItem);

            var result = this.application.ConfigureSolution(null, null, "anelementexpression",
                new List<string> { "anattributename=avalue" });

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
            element2.Attributes.Add(attribute2);
            element1.Elements.Add(element2);
            var pattern = new PatternDefinition("apatternname");
            pattern.Attributes.Add(attribute1);
            pattern.Elements.Add(element1);
            UpdateToolkit(pattern);
            var solution = this.application.CreateSolution("apatternname");
            solution.Model.Properties["anelementname1"].Materialise();
            solution.Model.Properties["anelementname1"].Properties["anelementname2"].Materialise();

            this.solutionStore.Save(solution);

            var result = this.application.GetConfiguration(false, false);

            result.Pattern.Should().BeNull();
            result.Validation.Should().BeEquivalentTo(ValidationResults.None);
            result.Configuration.Should().Be(JsonConversions.ToJson<dynamic>(new
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
            }));
        }

        [Fact]
        public void WhenGetConfigurationWithSchemaAndValidation_ThenReturnsConfiguration()
        {
            var attribute1 = new Attribute("anattributename1", null, false, "adefaultvalue1");
            var attribute2 = new Attribute("anattributename2", null, false, "adefaultvalue2");
            var element1 = new Element("anelementname1", "adisplayname1", "adescription1");
            var element2 = new Element("anelementname2", "adisplayname2", "adescription2");
            element2.Attributes.Add(attribute2);
            element1.Elements.Add(element2);
            var pattern = new PatternDefinition("apatternname");
            pattern.Attributes.Add(attribute1);
            pattern.Elements.Add(element1);
            UpdateToolkit(pattern);
            var solution = this.application.CreateSolution("apatternname");
            solution.Model.Properties["anelementname1"].Materialise();
            solution.Model.Properties["anelementname1"].Properties["anelementname2"].Materialise();

            this.solutionStore.Save(solution);

            var result = this.application.GetConfiguration(true, true);

            result.Pattern.Should().Be(pattern);
            result.Validation.Results.Should().BeEmpty();
            result.Configuration.Should().Be(JsonConversions.ToJson<dynamic>(new
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
            }));
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
            this.application.CreateSolution("apatternname");
            this.solutionPathResolver.Setup(spr => spr.ResolveItem(It.IsAny<SolutionDefinition>(), It.IsAny<string>()))
                .Returns((SolutionItem)null);

            this.application
                .Invoking(x => x.Validate("anelementexpression"))
                .Should().Throw<AutomateException>()
                .WithMessage(ExceptionMessages.RuntimeApplication_ElementExpressionNotFound.Format("apatternname", "anelementexpression"));
        }

        [Fact]
        public void WhenValidateElement_ThenReturnsResults()
        {
            var element1 = new Element("anelementname1", "adisplayname1", "adescription1");
            var element2 = new Element("anelementname2", "adisplayname2", "adescription2");
            var pattern = new PatternDefinition("apatternname");
            pattern.Elements.Add(element1);
            pattern.Elements.Add(element2);
            UpdateToolkit(pattern);
            var solution = this.application.CreateSolution("apatternname");
            this.solutionPathResolver.Setup(spr => spr.ResolveItem(It.IsAny<SolutionDefinition>(), It.IsAny<string>()))
                .Returns(solution.Model.Properties["anelementname1"]);

            var result = this.application.Validate("{anelementname}");

            result.Results.Count.Should().Be(1);
            result.Results.Should().Contain(r => r.Context.Path == "{anelementname1}" &&
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
            collection2.Attributes.Add(attribute2);
            var pattern = new PatternDefinition("apatternname");
            pattern.Attributes.Add(attribute1);
            pattern.Elements.Add(element1);
            pattern.Elements.Add(collection2);
            UpdateToolkit(pattern);
            var solution = this.application.CreateSolution("apatternname");
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
            this.application.CreateSolution("apatternname");
            this.solutionPathResolver.Setup(spr => spr.ResolveItem(It.IsAny<SolutionDefinition>(), It.IsAny<string>()))
                .Returns((SolutionItem)null);

            this.application
                .Invoking(x => x.ExecuteLaunchPoint("acommandname", "anelementexpression"))
                .Should().Throw<AutomateException>()
                .WithMessage(ExceptionMessages.RuntimeApplication_ElementExpressionNotFound.Format("apatternname", "anelementexpression"));
        }

        [Fact]
        public void WhenExecuteLaunchPointOnElement_ThenReturnsResult()
        {
            var element = new Element("anelementname", "adisplayname", "adescription");
            var automation = new Mock<IAutomation>();
            automation.Setup(auto => auto.Name)
                .Returns("acommandname");
            automation.Setup(auto => auto.Execute(It.IsAny<SolutionDefinition>(), It.IsAny<SolutionItem>()))
                .Returns(new CommandExecutionResult("acommandname", new List<string> { "alogentry" }));
            element.Automation.Add(automation.Object);
            var pattern = new PatternDefinition("apatternname");
            pattern.Elements.Add(element);
            UpdateToolkit(pattern);
            var solution = this.application.CreateSolution("apatternname");

            this.solutionPathResolver.Setup(spr => spr.ResolveItem(It.IsAny<SolutionDefinition>(), It.IsAny<string>()))
                .Returns(solution.Model.Properties["anelementname"].Materialise());

            var result = this.application.ExecuteLaunchPoint("acommandname", "anelementname");

            result.CommandName.Should().Be("acommandname");
            result.Log.Should().ContainSingle("alogentry");
        }

        [Fact]
        public void WhenExecuteLaunchPointOnSolution_ThenReturnsResult()
        {
            var pattern = new PatternDefinition("apatternname");
            var automation = new Mock<IAutomation>();
            automation.Setup(auto => auto.Name)
                .Returns("acommandname");
            automation.Setup(auto => auto.Execute(It.IsAny<SolutionDefinition>(), It.IsAny<SolutionItem>()))
                .Returns(new CommandExecutionResult("acommandname", new List<string> { "alogentry" }));
            pattern.Automation.Add(automation.Object);
            UpdateToolkit(pattern);
            var solution = this.application.CreateSolution("apatternname");
            this.solutionPathResolver.Setup(spr => spr.ResolveItem(It.IsAny<SolutionDefinition>(), It.IsAny<string>()))
                .Returns(solution.Model);

            var result = this.application.ExecuteLaunchPoint("acommandname", null);

            result.CommandName.Should().Be("acommandname");
            result.Log.Should().ContainSingle("alogentry");
        }

        private void UpdateToolkit(PatternDefinition pattern)
        {
            this.toolkitStore.DestroyAll();
            this.toolkitStore.Import(new ToolkitDefinition
            {
                Id = "atoolkitid",
                Pattern = pattern
            });
        }
    }
}