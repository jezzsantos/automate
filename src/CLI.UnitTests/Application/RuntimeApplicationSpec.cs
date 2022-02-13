using System;
using System.Collections.Generic;
using automate;
using automate.Application;
using automate.Domain;
using automate.Extensions;
using automate.Infrastructure;
using FluentAssertions;
using Moq;
using ServiceStack;
using Xunit;
using Attribute = automate.Domain.Attribute;

namespace CLI.UnitTests.Application
{
    [Trait("Category", "Unit")]
    public class RuntimeApplicationSpec
    {
        private readonly RuntimeApplication application;
        private readonly Mock<IFilePathResolver> fileResolver;
        private readonly Mock<IPatternToolkitPackager> packager;
        private readonly Mock<ISolutionPathResolver> solutionPathResolver;
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
            this.solutionPathResolver = new Mock<ISolutionPathResolver>();

            this.application =
                new RuntimeApplication(this.toolkitStore.Object, this.solutionStore.Object, this.fileResolver.Object,
                    this.packager.Object, this.solutionPathResolver.Object);
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
                    Id = "atoolkitid",
                    Pattern = new PatternDefinition("apatternname")
                });

            var result = this.application.CreateSolution("atoolkitname");

            result.Id.Should().NotBeNull();
            result.PatternName.Should().Be("apatternname");
            this.solutionStore.Verify(ss => ss.Save(It.Is<SolutionDefinition>(s =>
                s.ToolkitId == "atoolkitid"
                && s.PatternName == "apatternname"
            )));
        }

        [Fact]
        public void WhenListInstalledToolkits_ThenReturnsToolkits()
        {
            var toolkits = new List<ToolkitDefinition>();
            this.toolkitStore.Setup(ts => ts.ListAll())
                .Returns(toolkits);

            var result = this.application.ListInstalledToolkits();

            result.Should().BeSameAs(toolkits);
        }

        [Fact]
        public void WhenListCreatedSolutionsToolkits_ThenReturnsToolkits()
        {
            var solutions = new List<SolutionDefinition>();
            this.solutionStore.Setup(ts => ts.ListAll())
                .Returns(solutions);

            var result = this.application.ListCreatedSolutions();

            result.Should().BeSameAs(solutions);
        }

        [Fact]
        public void WhenConfigureSolutionAndNoAddToElementOrCollectionAndNoAssignments_ThenThrows()
        {
            this.application
                .Invoking(x => x.ConfigureSolution("asolutionid", null, null, null))
                .Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage(
                    ExceptionMessages.RuntimeApplication_ConfigureSolution_NoChanges.Format(
                        "asolutionid") + "*");
        }

        [Fact]
        public void WhenConfigureSolutionAndBothAddToElementAndCollection_ThenThrows()
        {
            this.application
                .Invoking(x => x.ConfigureSolution("asolutionid", "anelementexpression", "acollectionexpression", null))
                .Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage(
                    ExceptionMessages.RuntimeApplication_ConfigureSolution_AddAndAddTo.Format(
                        "asolutionid", "anelementexpression", "acollectionexpression") + "*");
        }

        [Fact]
        public void WhenConfigureSolutionAndPropertyAssigmentInvalid_ThenThrows()
        {
            this.application
                .Invoking(x => x.ConfigureSolution("asolutionid", "anelementexpression", null, new List<string>
                {
                    "notavalidpropertyassignment"
                }))
                .Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage(
                    ExceptionMessages.RuntimeApplication_ConfigureSolution_PropertyAssignmentInvalid.Format(
                        "notavalidpropertyassignment", "asolutionid") + "*");
        }

        [Fact]
        public void WhenConfigureSolutionAndSolutionNotExist_ThenThrows()
        {
            this.solutionStore.Setup(ss => ss.FindById(It.IsAny<string>()))
                .Returns((SolutionDefinition)null);

            this.application
                .Invoking(x => x.ConfigureSolution("asolutionid", "anelementexpression", null, null))
                .Should().Throw<AutomateException>()
                .WithMessage(ExceptionMessages.RuntimeApplication_SolutionNotFound.Format("asolutionid"));
        }

        [Fact]
        public void WhenConfigureSolutionAndAddElementButUnknown_ThenThrows()
        {
            this.solutionStore.Setup(ss => ss.FindById(It.IsAny<string>()))
                .Returns(new SolutionDefinition
                {
                    Id = "asolutionid",
                    Pattern = new PatternDefinition
                    {
                        Name = "apatternname"
                    }
                });
            this.solutionPathResolver.Setup(spr => spr.Resolve(It.IsAny<SolutionDefinition>(), It.IsAny<string>()))
                .Returns((SolutionItem)null);

            this.application
                .Invoking(x => x.ConfigureSolution("asolutionid", "anelementexpression", null, null))
                .Should().Throw<AutomateException>()
                .WithMessage(
                    ExceptionMessages.RuntimeApplication_ConfigureSolution_AddToExpressionNotFound.Format(
                        "apatternname", "anelementexpression"));
        }

        [Fact]
        public void WhenConfigureSolutionAndAddElementAlreadyMaterialised_ThenThrows()
        {
            this.solutionStore.Setup(ss => ss.FindById(It.IsAny<string>()))
                .Returns(new SolutionDefinition
                {
                    Id = "asolutionid",
                    Pattern = new PatternDefinition
                    {
                        Name = "apatternname"
                    }
                });
            this.solutionPathResolver.Setup(spr => spr.Resolve(It.IsAny<SolutionDefinition>(), It.IsAny<string>()))
                .Returns(new SolutionItem { IsMaterialised = true });

            this.application
                .Invoking(x => x.ConfigureSolution("asolutionid", "anelementexpression", null, null))
                .Should().Throw<AutomateException>()
                .WithMessage(
                    ExceptionMessages.RuntimeApplication_ConfigureSolution_AddElementExists.Format(
                        "anelementexpression"));
        }

        [Fact]
        public void WhenConfigureSolutionWithChangeOnPattern_ThenReturnsSolution()
        {
            var attribute = new Attribute("anattributename", null, false, null);
            var pattern = new PatternDefinition("apatternname");
            pattern.Attributes.Add(attribute);
            var solution = new SolutionDefinition("atoolkitname", pattern);
            this.solutionStore.Setup(ss => ss.FindById(It.IsAny<string>()))
                .Returns(solution);

            var result = this.application.ConfigureSolution("asolutionid", null, null,
                new List<string> { "anattributename=avalue" });

            result.Id.Should().NotBeNull();
            solution.Model.Properties["anattributename"].Value.Should().Be("avalue");
            this.solutionStore.Verify(ss => ss.Save(solution));
        }

        [Fact]
        public void WhenConfigureSolutionWithNewElement_ThenReturnsSolution()
        {
            var solution = new SolutionDefinition("atoolkitname", new PatternDefinition("apatternname"));
            this.solutionStore.Setup(ss => ss.FindById(It.IsAny<string>()))
                .Returns(solution);
            this.solutionPathResolver.Setup(spr => spr.Resolve(It.IsAny<SolutionDefinition>(), It.IsAny<string>()))
                .Returns(new SolutionItem(new Element("anelementname", null, null, false)));

            var result = this.application.ConfigureSolution("asolutionid", "apatternname.anelement", null, null);

            result.Id.Should().NotBeNull();
            this.solutionStore.Verify(ss => ss.Save(solution));
        }

        [Fact]
        public void WhenConfigureSolutionAndAddCollectionElementButUnknown_ThenThrows()
        {
            this.solutionStore.Setup(ss => ss.FindById(It.IsAny<string>()))
                .Returns(new SolutionDefinition("atoolkitname", new PatternDefinition("apatternname")));
            this.solutionPathResolver.Setup(spr => spr.Resolve(It.IsAny<SolutionDefinition>(), It.IsAny<string>()))
                .Returns((SolutionItem)null);

            this.application
                .Invoking(x => x.ConfigureSolution("asolutionid", null, "acollectionexpression", null))
                .Should().Throw<AutomateException>()
                .WithMessage(
                    ExceptionMessages.RuntimeApplication_ConfigureSolution_AddToExpressionNotFound.Format(
                        "apatternname", "acollectionexpression"));
        }

        [Fact]
        public void WhenConfigureSolutionWithNewCollectionElement_ThenReturnsSolution()
        {
            var solution = new SolutionDefinition
            {
                Id = "asolutionid",
                Pattern = new PatternDefinition("apatternname")
            };
            this.solutionStore.Setup(ss => ss.FindById(It.IsAny<string>()))
                .Returns(solution);
            this.solutionPathResolver.Setup(spr => spr.Resolve(It.IsAny<SolutionDefinition>(), It.IsAny<string>()))
                .Returns(new SolutionItem(new Element("anelementname", null, null, true)));

            var result = this.application.ConfigureSolution("asolutionid", null, "apatternname.acollection", null);

            result.Id.Should().Be("asolutionid");
            this.solutionStore.Verify(ss => ss.Save(solution));
        }

        [Fact]
        public void WhenConfigureSolutionWithUnknownProperty_ThenThrows()
        {
            this.solutionStore.Setup(ss => ss.FindById(It.IsAny<string>()))
                .Returns(new SolutionDefinition("atoolkitname", new PatternDefinition("apatternname")));
            this.solutionPathResolver.Setup(spr => spr.Resolve(It.IsAny<SolutionDefinition>(), It.IsAny<string>()))
                .Returns(new SolutionItem(new Element("anelementname", null, null, false)));

            this.application
                .Invoking(x => x.ConfigureSolution("asolutionid", "anelementexpression", null,
                    new List<string> { "anunknownname=avalue" }))
                .Should().Throw<AutomateException>()
                .WithMessage(
                    ExceptionMessages.RuntimeApplication_ConfigureSolution_ElementPropertyNotExists.Format(
                        "anelementname", "anunknownname"));
        }

        [Fact]
        public void WhenConfigureSolutionWithWithPropertyOfWrongChoice_ThenThrows()
        {
            this.solutionStore.Setup(ss => ss.FindById(It.IsAny<string>()))
                .Returns(new SolutionDefinition("atoolkitname", new PatternDefinition("apatternname")));
            var attribute = new Attribute("anattributename", "string", false, null)
            {
                Choices = new List<string> { "avalue" }
            };
            var element = new Element("anelementname", null, null, false);
            element.Attributes.Add(attribute);
            var solutionItem = new SolutionItem(element);
            this.solutionPathResolver.Setup(spr => spr.Resolve(It.IsAny<SolutionDefinition>(), It.IsAny<string>()))
                .Returns(solutionItem);

            this.application
                .Invoking(x => x.ConfigureSolution("asolutionid", "anelementexpression", null,
                    new List<string> { "anattributename=awrongvalue" }))
                .Should().Throw<AutomateException>()
                .WithMessage(
                    ExceptionMessages.RuntimeApplication_ConfigureSolution_ElementPropertyValueIsNotOneOf.Format(
                        "anelementname", "anattributename", new List<string> { "avalue" }.Join(";"), "awrongvalue"));
        }

        [Fact]
        public void WhenConfigureSolutionWithPropertyOfWrongDataType_ThenThrows()
        {
            this.solutionStore.Setup(ss => ss.FindById(It.IsAny<string>()))
                .Returns(new SolutionDefinition("atoolkitname", new PatternDefinition("apatternname")));
            var attribute = new Attribute("anattributename", "int", false, null);
            var element = new Element("anelementname", null, null, false);
            element.Attributes.Add(attribute);
            var solutionItem = new SolutionItem(element);
            this.solutionPathResolver.Setup(spr => spr.Resolve(It.IsAny<SolutionDefinition>(), It.IsAny<string>()))
                .Returns(solutionItem);

            this.application
                .Invoking(x =>
                    x.ConfigureSolution("asolutionid", "anelementexpression", null,
                        new List<string> { "anattributename=astring" }))
                .Should().Throw<AutomateException>()
                .WithMessage(
                    ExceptionMessages.RuntimeApplication_ConfigureSolution_ElementPropertyValueNotCompatible.Format(
                        "anelementname", "anattributename", "int", "astring"));
        }

        [Fact]
        public void WhenConfigureSolutionWithNewElementAndPropertyChoice_ThenReturnsSolution()
        {
            var solution = new SolutionDefinition
            {
                Id = "asolutionid",
                Pattern = new PatternDefinition("apatternname")
            };
            this.solutionStore.Setup(ss => ss.FindById(It.IsAny<string>()))
                .Returns(solution);
            var attribute = new Attribute("anattributename", "string", false, null)
            {
                Choices = new List<string> { "avalue" }
            };
            var element = new Element("anelementname", null, null, false);
            element.Attributes.Add(attribute);
            var solutionItem = new SolutionItem(element);
            this.solutionPathResolver.Setup(spr => spr.Resolve(It.IsAny<SolutionDefinition>(), It.IsAny<string>()))
                .Returns(solutionItem);

            var result = this.application.ConfigureSolution("asolutionid", "anelementexpression", null,
                new List<string> { "anattributename=avalue" });

            result.Id.Should().Be("asolutionid");
            solutionItem.Properties["anattributename"].Value.Should().Be("avalue");
            this.solutionStore.Verify(ss => ss.Save(solution));
        }

        [Fact]
        public void WhenConfigureSolutionWithNewElementAndProperty_ThenReturnsSolution()
        {
            var solution = new SolutionDefinition
            {
                Id = "asolutionid",
                Pattern = new PatternDefinition("apatternname")
            };
            this.solutionStore.Setup(ss => ss.FindById(It.IsAny<string>()))
                .Returns(solution);

            var attribute = new Attribute("anattributename", "string", false, null);
            var element = new Element("anelementname", null, null, false);
            element.Attributes.Add(attribute);
            var solutionItem = new SolutionItem(element);
            this.solutionPathResolver.Setup(spr => spr.Resolve(It.IsAny<SolutionDefinition>(), It.IsAny<string>()))
                .Returns(solutionItem);

            var result = this.application.ConfigureSolution("asolutionid", "anelementexpression", null,
                new List<string> { "anattributename=avalue" });

            result.Id.Should().Be("asolutionid");
            solutionItem.Properties["anattributename"].Value.Should().Be("avalue");
            this.solutionStore.Verify(ss => ss.Save(solution));
        }

        [Fact]
        public void WhenGetConfigurationAndSolutionNotExist_ThenThrows()
        {
            this.solutionStore.Setup(ss => ss.FindById(It.IsAny<string>()))
                .Returns((SolutionDefinition)null);

            this.application
                .Invoking(x => x.GetSolutionConfiguration("asolutionid"))
                .Should().Throw<AutomateException>()
                .WithMessage(ExceptionMessages.RuntimeApplication_SolutionNotFound.Format("asolutionid"));
        }

        [Fact]
        public void WhenGetConfiguration_ThenReturnsConfiguration()
        {
            var attribute1 = new Attribute("anattributename1", null, false, "adefaultvalue1");
            var attribute2 = new Attribute("anattributename2", null, false, "adefaultvalue2");
            var element1 = new Element("anelementname1", "adisplayname1", "adescription1", false);
            var element2 = new Element("anelementname2", "adisplayname2", "adescription2", false);
            element2.Attributes.Add(attribute2);
            element1.Elements.Add(element2);
            var pattern = new PatternDefinition("apatternname");
            pattern.Attributes.Add(attribute1);
            pattern.Elements.Add(element1);

            var solution = new SolutionDefinition("atoolkitname", pattern);
            solution.Model.Properties["anelementname1"].Materialise();
            solution.Model.Properties["anelementname1"].Properties["anelementname2"].Materialise();

            this.solutionStore.Setup(ss => ss.FindById(It.IsAny<string>()))
                .Returns(solution);

            var result = this.application.GetSolutionConfiguration("asolutionid");

            result.Should().Be(JsonConversions.ToJson<dynamic>(new
            {
                anattributename1 = "adefaultvalue1",
                anelementname1 = new
                {
                    anelementname2 = new
                    {
                        anattributename2 = "adefaultvalue2"
                    }
                }
            }));
        }
    }
}