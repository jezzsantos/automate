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
    public class RuntimeApplicationSpec
    {
        [Trait("Category", "Unit")]
        public class GivenNoInstalledToolkit
        {
            private readonly RuntimeApplication application;

            public GivenNoInstalledToolkit()
            {
                var repo = new MemoryRepository();
                var toolkitStore = new ToolkitStore(repo, repo);
                var draftStore = new DraftStore(repo, repo);
                var fileResolver = new Mock<IFilePathResolver>();
                var packager = new Mock<IPatternToolkitPackager>();
                var draftPathResolver = new Mock<IDraftPathResolver>();

                this.application =
                    new RuntimeApplication(toolkitStore, draftStore, fileResolver.Object,
                        packager.Object, draftPathResolver.Object);
            }

            [Fact]
            public void WhenGetCurrentToolkitAndNoneInstalled_ThenThrows()
            {
                this.application
                    .Invoking(x => x.ViewCurrentToolkit())
                    .Should().Throw<AutomateException>()
                    .WithMessage(ExceptionMessages.RuntimeApplication_NoCurrentToolkit);
            }
        }

        [Trait("Category", "Unit")]
        public class GivenAnInstalledToolkit
        {
            private readonly RuntimeApplication application;
            private readonly Mock<IDraftPathResolver> draftPathResolver;
            private readonly IDraftStore draftStore;
            private readonly Mock<IFilePathResolver> fileResolver;
            private readonly Mock<IPatternToolkitPackager> packager;
            private readonly IToolkitStore toolkitStore;
            private PatternDefinition pattern;
            private ToolkitDefinition toolkit;

            public GivenAnInstalledToolkit()
            {
                var repo = new MemoryRepository();
                this.toolkitStore = new ToolkitStore(repo, repo);
                this.draftStore = new DraftStore(repo, repo);
                this.fileResolver = new Mock<IFilePathResolver>();
                this.fileResolver.Setup(pr => pr.ExistsAtPath(It.IsAny<string>()))
                    .Returns(true);
                this.packager = new Mock<IPatternToolkitPackager>();
                this.draftPathResolver = new Mock<IDraftPathResolver>();

                this.application =
                    new RuntimeApplication(this.toolkitStore, this.draftStore, this.fileResolver.Object,
                        this.packager.Object, this.draftPathResolver.Object);
                ImportToolkit("apatternname");
            }

            [Fact]
            public void WhenConstructed_ThenCurrentDraftIsNull()
            {
                this.application.CurrentDraftId.Should().BeNull();
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
            public void WhenCreateDraftAndToolkitNotExist_ThenThrows()
            {
                this.application
                    .Invoking(x => x.CreateDraft("atoolkitname", null))
                    .Should().Throw<AutomateException>()
                    .WithMessage(ExceptionMessages.RuntimeApplication_ToolkitNotFound.Format("atoolkitname"));
            }

            [Fact]
            public void WhenCreateDraft_ThenReturnsNewDraft()
            {
                var result = this.application.CreateDraft("apatternname", null);

                result.Id.Should().NotBeNull();
                result.PatternName.Should().Be("apatternname");
                result.Toolkit.Id.Should().Be(this.toolkit.Id);
                this.application.CurrentDraftId.Should().Be(result.Id);
            }

            [Fact]
            public void WhenListInstalledToolkits_ThenReturnsToolkits()
            {
                var result = this.application.ListInstalledToolkits();

                result.Should().ContainSingle(tk => tk.Id == this.toolkit.Id);
            }

            [Fact]
            public void WhenListCreatedDrafts_ThenReturnsToolkits()
            {
                var draft = this.application.CreateDraft("apatternname", null);

                var result = this.application.ListCreatedDrafts();

                result.Should().Contain(draft);
            }

            [Fact]
            public void WhenGetCurrentToolkitAndDraftNotExists_ThenReturns()
            {
                var result = this.application.ViewCurrentToolkit();

                result.Should().Be(this.application.ListInstalledToolkits().Single());
            }

            [Fact]
            public void WhenSwitchCurrentDraftAndDraftNotExists_ThenThrows()
            {
                this.application
                    .Invoking(x => x.SwitchCurrentDraft("adraftid"))
                    .Should().Throw<AutomateException>()
                    .WithMessage(
                        ExceptionMessages.DraftStore_NotFoundAtLocationWithId.Format("adraftid",
                            MemoryRepository.InMemoryLocation));
            }

            [Fact]
            public void WhenSwitchCurrentDraft_ThenSwitchesCurrentDraftAndToolkit()
            {
                ImportToolkit("apatternname2");
                var draft1 = this.application.CreateDraft("apatternname", null);
                var draft2 = this.application.CreateDraft("apatternname2", null);

                this.application.SwitchCurrentDraft(draft1.Id);

                this.draftStore.GetCurrent().Should().NotBeNull();
                this.application.CurrentDraftId.Should().Be(draft1.Id);
                this.toolkitStore.GetCurrent().Should().Be(this.application.ListInstalledToolkits()[0]);

                this.application.SwitchCurrentDraft(draft2.Id);

                this.draftStore.GetCurrent().Should().NotBeNull();
                this.application.CurrentDraftId.Should().Be(draft2.Id);
                this.toolkitStore.GetCurrent().Should().Be(this.application.ListInstalledToolkits()[1]);
            }

            [Fact]
            public void WhenConfigureDraftAndCurrentDraftNotExists_ThenThrows()
            {
                this.application
                    .Invoking(x => x.ConfigureDraft(null, null, null, new Dictionary<string, string>()))
                    .Should().Throw<AutomateException>()
                    .WithMessage(ExceptionMessages.RuntimeApplication_NoCurrentDraft);
            }

            [Fact]
            public void WhenConfigureDraftAndNoAddElementNorAddToCollectionNorOnElementNorAnyAssignments_ThenThrows()
            {
                var draft = this.application.CreateDraft("apatternname", null);

                this.application
                    .Invoking(x => x.ConfigureDraft(null, null, null, null))
                    .Should().Throw<ArgumentOutOfRangeException>()
                    .WithMessage(
                        ExceptionMessages.RuntimeApplication_ConfigureDraft_NoChanges.Format(
                            draft.Id) + "*");
            }

            [Fact]
            public void WhenConfigureDraftAndBothAddElementAndAddToCollection_ThenThrows()
            {
                var draft = this.application.CreateDraft("apatternname", null);

                this.application
                    .Invoking(x => x.ConfigureDraft("anelementexpression", "acollectionexpression", null, null))
                    .Should().Throw<ArgumentOutOfRangeException>()
                    .WithMessage(
                        ExceptionMessages.RuntimeApplication_ConfigureDraft_AddAndAddTo.Format(
                            draft.Id, "anelementexpression", "acollectionexpression") + "*");
            }

            [Fact]
            public void WhenConfigureDraftAndBothAddElementAndOnElement_ThenThrows()
            {
                var draft = this.application.CreateDraft("apatternname", null);

                this.application
                    .Invoking(x => x.ConfigureDraft("anelementexpression", null, "anelementexpression", null))
                    .Should().Throw<ArgumentOutOfRangeException>()
                    .WithMessage(
                        ExceptionMessages.RuntimeApplication_ConfigureDraft_OnAndAdd.Format(
                            draft.Id, "anelementexpression", "anelementexpression") + "*");
            }

            [Fact]
            public void WhenConfigureDraftAndBothAddToCollectionAndOnElement_ThenThrows()
            {
                var draft = this.application.CreateDraft("apatternname", null);

                this.application
                    .Invoking(x => x.ConfigureDraft(null, "acollectionexpression", "anelementexpression", null))
                    .Should().Throw<ArgumentOutOfRangeException>()
                    .WithMessage(
                        ExceptionMessages.RuntimeApplication_ConfigureDraft_OnAndAddTo.Format(
                            draft.Id, "anelementexpression", "acollectionexpression") + "*");
            }

            [Fact]
            public void WhenConfigureDraftAndAddElementButUnknown_ThenThrows()
            {
                this.application.CreateDraft("apatternname", null);
                this.draftPathResolver.Setup(spr => spr.ResolveItem(It.IsAny<DraftDefinition>(), It.IsAny<string>()))
                    .Returns((DraftItem)null);

                this.application
                    .Invoking(x => x.ConfigureDraft("anelementexpression", null, null, null))
                    .Should().Throw<AutomateException>()
                    .WithMessage(
                        ExceptionMessages.RuntimeApplication_ItemExpressionNotFound.Format(
                            "apatternname", "anelementexpression"));
            }

            [Fact]
            public void WhenConfigureDraftAndAddElementAlreadyMaterialised_ThenThrows()
            {
                this.application.CreateDraft("apatternname", null);
                this.pattern.AddElement("anelementname");
                this.draftPathResolver.Setup(spr => spr.ResolveItem(It.IsAny<DraftDefinition>(), It.IsAny<string>()))
                    .Returns(new DraftItem(this.toolkit, this.pattern.Elements.Single(), null).Materialise());

                this.application
                    .Invoking(x => x.ConfigureDraft("anelementexpression", null, null, null))
                    .Should().Throw<AutomateException>()
                    .WithMessage(
                        ExceptionMessages.RuntimeApplication_ConfigureDraft_AddElementExists.Format(
                            "anelementexpression"));
            }

            [Fact]
            public void WhenConfigureDraftWithChangeOnPattern_ThenReturnsDraft()
            {
                var attribute = new Attribute("anattributename", null);
                this.pattern.AddAttribute(attribute);
                ResetToolkit();
                var draft = this.application.CreateDraft("apatternname", null);

                var result = this.application.ConfigureDraft(null, null, null,
                    new Dictionary<string, string> { { "anattributename", "avalue" } });

                result.Id.Should().NotBeNull();
                draft.Model.Properties["anattributename"].Value.Should().Be("avalue");
            }

            [Fact]
            public void WhenConfigureDraftWithNewElement_ThenReturnsDraft()
            {
                this.application.CreateDraft("apatternname", null);
                this.pattern.AddElement("anelementname");
                this.draftPathResolver.Setup(spr => spr.ResolveItem(It.IsAny<DraftDefinition>(), It.IsAny<string>()))
                    .Returns(new DraftItem(this.toolkit, this.pattern.Elements.Single(), null));

                var result = this.application.ConfigureDraft("apatternname.anelement", null, null, null);

                result.Id.Should().NotBeNull();
            }

            [Fact]
            public void WhenConfigureDraftAndAddCollectionElementButUnknown_ThenThrows()
            {
                this.application.CreateDraft("apatternname", null);
                this.draftPathResolver.Setup(spr => spr.ResolveItem(It.IsAny<DraftDefinition>(), It.IsAny<string>()))
                    .Returns((DraftItem)null);

                this.application
                    .Invoking(x => x.ConfigureDraft(null, "acollectionexpression", null, null))
                    .Should().Throw<AutomateException>()
                    .WithMessage(
                        ExceptionMessages.RuntimeApplication_ItemExpressionNotFound.Format(
                            "apatternname", "acollectionexpression"));
            }

            [Fact]
            public void WhenConfigureDraftWithNewCollectionElement_ThenReturnsDraft()
            {
                this.application.CreateDraft("apatternname", null);
                this.pattern.AddElement("anelementname", ElementCardinality.OneOrMany);
                var draftItem = new DraftItem(this.toolkit, this.pattern.Elements.Single(), null);
                this.draftPathResolver.Setup(spr => spr.ResolveItem(It.IsAny<DraftDefinition>(), It.IsAny<string>()))
                    .Returns(draftItem);

                var result = this.application.ConfigureDraft(null, "apatternname.acollectionname", null, null);

                result.Id.Should().Be(draftItem.Items.Single().Id);
            }

            [Fact]
            public void WhenConfigureDraftAndOnElementButUnknown_ThenThrows()
            {
                this.application.CreateDraft("apatternname", null);
                this.draftPathResolver.Setup(spr => spr.ResolveItem(It.IsAny<DraftDefinition>(), It.IsAny<string>()))
                    .Returns((DraftItem)null);

                this.application
                    .Invoking(x => x.ConfigureDraft(null, null, "anelementexpression", null))
                    .Should().Throw<AutomateException>()
                    .WithMessage(
                        ExceptionMessages.RuntimeApplication_ItemExpressionNotFound.Format(
                            "apatternname", "anelementexpression"));
            }

            [Fact]
            public void WhenConfigureDraftAndOnElementAndNotMaterialised_ThenThrows()
            {
                this.application.CreateDraft("apatternname", null);
                this.draftPathResolver.Setup(spr => spr.ResolveItem(It.IsAny<DraftDefinition>(), It.IsAny<string>()))
                    .Returns(new DraftItem(this.toolkit, new Element("anelementname"), null));

                this.application
                    .Invoking(x => x.ConfigureDraft(null, null, "anelementexpression", null))
                    .Should().Throw<AutomateException>()
                    .WithMessage(
                        ExceptionMessages.RuntimeApplication_ConfigureDraft_OnElementNotExists.Format(
                            "anelementexpression"));
            }

            [Fact]
            public void WhenConfigureDraftWithAddElementAndProperty_ThenReturnsDraft()
            {
                var attribute = new Attribute("anattributename");
                var element = new Element("anelementname", autoCreate: false);
                element.AddAttribute(attribute);
                this.pattern.AddElement(element);
                ResetToolkit();
                var draft = this.application.CreateDraft("apatternname", null);
                var draftItem = draft.Model.Properties["anelementname"];
                this.draftPathResolver.Setup(spr => spr.ResolveItem(It.IsAny<DraftDefinition>(), It.IsAny<string>()))
                    .Returns(draftItem);

                var result = this.application.ConfigureDraft("anelementexpression", null, null,
                    new Dictionary<string, string> { { "anattributename", "avalue" } });

                result.Id.Should().Be(draftItem.Id);
                draftItem.Properties["anattributename"].Value.Should().Be("avalue");
            }

            [Fact]
            public void WhenConfigureDraftWithOnElementAndProperty_ThenReturnsDraft()
            {
                var attribute = new Attribute("anattributename");
                var element = new Element("anelementname");
                element.AddAttribute(attribute);
                this.pattern.AddElement(element);
                ResetToolkit();
                var draft = this.application.CreateDraft("apatternname", null);
                var draftItem = draft.Model.Properties["anelementname"].Materialise();
                this.draftPathResolver.Setup(spr => spr.ResolveItem(It.IsAny<DraftDefinition>(), It.IsAny<string>()))
                    .Returns(draftItem);

                var result = this.application.ConfigureDraft(null, null, "anelementexpression",
                    new Dictionary<string, string> { { "anattributename", "avalue" } });

                result.Id.Should().Be(draftItem.Id);
                draftItem.Properties["anattributename"].Value.Should().Be("avalue");
            }

            [Fact]
            public void WhenConfigureDraftAndResetElement_ThenResetsAllAttributes()
            {
                var attribute1 = new Attribute("anattributename1");
                var attribute2 = new Attribute("anattributename2", defaultValue: "adefaultvalue");
                var element = new Element("anelementname");
                element.AddAttribute(attribute1);
                element.AddAttribute(attribute2);
                this.pattern.AddElement(element);
                ResetToolkit();
                var draft = this.application.CreateDraft("apatternname", null);
                var draftItem = draft.Model.Properties["anelementname"].Materialise();
                this.draftPathResolver.Setup(spr => spr.ResolveItem(It.IsAny<DraftDefinition>(), It.IsAny<string>()))
                    .Returns(draftItem);

                var result = this.application.ConfigureDraftAndResetElement("anelementexpression");

                result.Properties["anattributename1"].Value.Should().BeNull();
                result.Properties["anattributename2"].Value.Should().Be("adefaultvalue");
            }

            [Fact]
            public void WhenConfigureDraftAndClearCollection_ThenEmptiesCollection()
            {
                var collection = new Element("acollectioname", ElementCardinality.ZeroOrMany);
                this.pattern.AddElement(collection);
                ResetToolkit();
                var draft = this.application.CreateDraft("apatternname", null);
                var collectionItem = draft.Model.Properties["acollectioname"].Materialise();
                collectionItem.MaterialiseCollectionItem();
                collectionItem.MaterialiseCollectionItem();
                collectionItem.MaterialiseCollectionItem();
                this.draftPathResolver.Setup(spr => spr.ResolveItem(It.IsAny<DraftDefinition>(), It.IsAny<string>()))
                    .Returns(collectionItem);

                var result = this.application.ConfigureDraftAndClearCollection("acollectionexpression");

                result.Items.Count.Should().Be(0);
            }

            [Fact]
            public void WhenConfigureDraftAndDeletePattern_ThenThrows()
            {
                var element = new Element("anelementname");
                this.pattern.AddElement(element);
                ResetToolkit();
                var draft = this.application.CreateDraft("apatternname", null);
                var draftItem = draft.Model;
                this.draftPathResolver.Setup(spr => spr.ResolveItem(It.IsAny<DraftDefinition>(), It.IsAny<string>()))
                    .Returns(draftItem);

                this.application
                    .Invoking(x => x.ConfigureDraftAndDelete("anelementexpression"))
                    .Should().Throw<AutomateException>()
                    .WithMessage(ExceptionMessages.RuntimeApplication_ConfigureDraft_DeletePattern);
            }

            [Fact]
            public void WhenConfigureDraftAndDelete_ThenDeletesElement()
            {
                var element = new Element("anelementname");
                this.pattern.AddElement(element);
                ResetToolkit();
                var draft = this.application.CreateDraft("apatternname", null);
                var draftItem = draft.Model.Properties["anelementname"].Materialise();
                this.draftPathResolver.Setup(spr => spr.ResolveItem(It.IsAny<DraftDefinition>(), It.IsAny<string>()))
                    .Returns(draftItem);

                var result = this.application.ConfigureDraftAndDelete("anelementexpression");

                result.Properties.Should().NotContainKey("anelementname");
            }

            [Fact]
            public void WhenGetDraftConfigurationAndCurrentDraftNotExists_ThenThrows()
            {
                this.application
                    .Invoking(x => x.GetDraftConfiguration(false, false))
                    .Should().Throw<AutomateException>()
                    .WithMessage(ExceptionMessages.RuntimeApplication_NoCurrentDraft);
            }

            [Fact]
            public void WhenGetDraftConfigurationAndToolkitUpgraded_ThenThrows()
            {
                ResetToolkit();
                var draft = this.application.CreateDraft("apatternname", null);
                this.toolkitStore.Import(new ToolkitDefinition(this.pattern, new Version("0.2.0")));

                this.application
                    .Invoking(x => x.GetDraftConfiguration(false, false))
                    .Should().Throw<AutomateException>()
                    .WithMessage(
                        ExceptionMessages.RuntimeApplication_CurrentDraftUpgraded.Format(draft.Name, draft.Id,
                            "0.0.0", "0.2.0"));
            }

            [Fact]
            public void WhenGetDraftConfiguration_ThenReturnsConfiguration()
            {
                var attribute1 = new Attribute("anattributename1", null, false, "adefaultvalue1");
                var attribute2 = new Attribute("anattributename2", null, false, "adefaultvalue2");
                var element1 =
                    new Element("anelementname1", displayName: "adisplayname1", description: "adescription1");
                var element2 =
                    new Element("anelementname2", displayName: "adisplayname2", description: "adescription2");
                element2.AddAttribute(attribute2);
                element1.AddElement(element2);
                this.pattern.AddAttribute(attribute1);
                this.pattern.AddElement(element1);
                ResetToolkit();
                var draft = this.application.CreateDraft("apatternname", null);
                draft.Model.Properties["anelementname1"].Materialise();
                draft.Model.Properties["anelementname1"].Properties["anelementname2"].Materialise();

                this.draftStore.Save(draft);

                var result = this.application.GetDraftConfiguration(false, false);

                result.Pattern.Should().BeNull();
                result.Validation.Should().BeEquivalentTo(ValidationResults.None);
                result.Configuration.Should().Be(new
                {
                    draft.Model.Id,
                    anattributename1 = "adefaultvalue1",
                    anelementname1 = new
                    {
                        draft.Model.Properties["anelementname1"].Id,
                        anelementname2 = new
                        {
                            draft.Model.Properties["anelementname1"].Properties["anelementname2"].Id,
                            anattributename2 = "adefaultvalue2"
                        }
                    }
                }.ToJson<dynamic>());
            }

            [Fact]
            public void WhenGetDraftConfigurationWithSchemaAndValidation_ThenReturnsConfiguration()
            {
                var attribute1 = new Attribute("anattributename1", null, false, "adefaultvalue1");
                var attribute2 = new Attribute("anattributename2", null, false, "adefaultvalue2");
                var element1 =
                    new Element("anelementname1", displayName: "adisplayname1", description: "adescription1");
                var element2 =
                    new Element("anelementname2", displayName: "adisplayname2", description: "adescription2");
                element2.AddAttribute(attribute2);
                element1.AddElement(element2);
                this.pattern.AddAttribute(attribute1);
                this.pattern.AddElement(element1);
                ResetToolkit();
                var draft = this.application.CreateDraft("apatternname", null);
                draft.Model.Properties["anelementname1"].Materialise();
                draft.Model.Properties["anelementname1"].Properties["anelementname2"].Materialise();

                this.draftStore.Save(draft);

                var result = this.application.GetDraftConfiguration(true, true);

                result.Pattern.Should().Be(this.pattern);
                result.Validation.Results.Should().BeEmpty();
                result.Configuration.Should().Be(new
                {
                    draft.Model.Id,
                    anattributename1 = "adefaultvalue1",
                    anelementname1 = new
                    {
                        draft.Model.Properties["anelementname1"].Id,
                        anelementname2 = new
                        {
                            draft.Model.Properties["anelementname1"].Properties["anelementname2"].Id,
                            anattributename2 = "adefaultvalue2"
                        }
                    }
                }.ToJson<dynamic>());
            }

            [Fact]
            public void WhenValidateDraftAndDraftNotExist_ThenThrows()
            {
                this.application
                    .Invoking(x => x.Validate(null))
                    .Should().Throw<AutomateException>()
                    .WithMessage(ExceptionMessages.RuntimeApplication_NoCurrentDraft);
            }

            [Fact]
            public void WhenValidateDraftAndElementNotExist_ThenThrows()
            {
                this.application.CreateDraft("apatternname", null);
                this.draftPathResolver.Setup(spr => spr.ResolveItem(It.IsAny<DraftDefinition>(), It.IsAny<string>()))
                    .Returns((DraftItem)null);

                this.application
                    .Invoking(x => x.Validate("anelementexpression"))
                    .Should().Throw<AutomateException>()
                    .WithMessage(
                        ExceptionMessages.RuntimeApplication_ItemExpressionNotFound.Format("apatternname",
                            "anelementexpression"));
            }

            [Fact]
            public void WhenValidateElement_ThenReturnsResults()
            {
                var element1 = new Element("anelementname1", autoCreate: false, displayName: "adisplayname1",
                    description: "adescription1");
                var element2 = new Element("anelementname2", autoCreate: false, displayName: "adisplayname2",
                    description: "adescription2");
                this.pattern.AddElement(element1);
                this.pattern.AddElement(element2);
                ResetToolkit();
                var draft = this.application.CreateDraft("apatternname", null);
                this.draftPathResolver.Setup(spr => spr.ResolveItem(It.IsAny<DraftDefinition>(), It.IsAny<string>()))
                    .Returns(draft.Model.Properties["anelementname1"]);

                var result = this.application.Validate("{anelementname}");

                result.Results.Count.Should().Be(1);
                result.Results.Should().Contain(r => r.Context.Path == "{apatternname.anelementname1}" &&
                                                     r.Message == ValidationMessages
                                                         .DraftItem_ValidationRule_ElementRequiresAtLeastOneInstance);
            }

            [Fact]
            public void WhenValidateDraft_ThenReturnsResults()
            {
                var attribute1 = new Attribute("anattributename1", null, true, "adefaultvalue1");
                var attribute2 = new Attribute("anattributename2", null, true, "adefaultvalue2");
                var element1 = new Element("anelementname1", autoCreate: false, displayName: "adisplayname1",
                    description: "adescription1");
                var collection2 = new Element("acollectionname2", ElementCardinality.OneOrMany, false, "adisplayname2",
                    "adescription2");
                collection2.AddAttribute(attribute2);
                this.pattern.AddAttribute(attribute1);
                this.pattern.AddElement(element1);
                this.pattern.AddElement(collection2);
                ResetToolkit();
                var draft = this.application.CreateDraft("apatternname", null);
                this.draftPathResolver.Setup(spr => spr.ResolveItem(It.IsAny<DraftDefinition>(), It.IsAny<string>()))
                    .Returns(draft.Model);

                var result = this.application.Validate(null);

                result.Results.Count.Should().Be(2);
                result.Results.Should().Contain(r => r.Context.Path == "{apatternname.anelementname1}" &&
                                                     r.Message == ValidationMessages
                                                         .DraftItem_ValidationRule_ElementRequiresAtLeastOneInstance);
                result.Results.Should().Contain(r => r.Context.Path == "{apatternname.acollectionname2}" &&
                                                     r.Message == ValidationMessages
                                                         .DraftItem_ValidationRule_ElementRequiresAtLeastOneInstance);
            }

            [Fact]
            public void WhenExecuteLaunchPointAndDraftNotExist_ThenThrows()
            {
                this.application
                    .Invoking(x => x.ExecuteLaunchPoint("acommandname", null))
                    .Should().Throw<AutomateException>()
                    .WithMessage(ExceptionMessages.RuntimeApplication_NoCurrentDraft);
            }

            [Fact]
            public void WhenExecuteLaunchPointAndElementNotExist_ThenThrows()
            {
                this.application.CreateDraft("apatternname", null);
                this.draftPathResolver.Setup(spr => spr.ResolveItem(It.IsAny<DraftDefinition>(), It.IsAny<string>()))
                    .Returns((DraftItem)null);

                this.application
                    .Invoking(x => x.ExecuteLaunchPoint("acommandname", "anelementexpression"))
                    .Should().Throw<AutomateException>()
                    .WithMessage(
                        ExceptionMessages.RuntimeApplication_ItemExpressionNotFound.Format("apatternname",
                            "anelementexpression"));
            }

            [Fact]
            public void WhenExecuteLaunchPointOnElement_ThenReturnsResult()
            {
                var element = new Element("anelementname", displayName: "adisplayname", description: "adescription");
                var automation =
                    new Automation("acommandname", AutomationType.TestingOnly, new Dictionary<string, object>());
                element.AddAutomation(automation);
                this.pattern.AddElement(element);
                ResetToolkit();
                var draft = this.application.CreateDraft("apatternname", null);

                this.draftPathResolver.Setup(spr => spr.ResolveItem(It.IsAny<DraftDefinition>(), It.IsAny<string>()))
                    .Returns(draft.Model.Properties["anelementname"].Materialise());

                var result = this.application.ExecuteLaunchPoint("acommandname", "anelementname");

                result.CommandName.Should().Be("acommandname");
                result.Log.Should().ContainSingle("testingonly");
            }

            [Fact]
            public void WhenExecuteLaunchPointOnDraft_ThenReturnsResult()
            {
                var automation =
                    new Automation("acommandname", AutomationType.TestingOnly, new Dictionary<string, object>());
                this.pattern.AddAutomation(automation);
                ResetToolkit();
                var draft = this.application.CreateDraft("apatternname", null);
                this.draftPathResolver.Setup(spr => spr.ResolveItem(It.IsAny<DraftDefinition>(), It.IsAny<string>()))
                    .Returns(draft.Model);

                var result = this.application.ExecuteLaunchPoint("acommandname", null);

                result.CommandName.Should().Be("acommandname");
                result.Log.Should().ContainSingle("testingonly");
            }

            [Fact]
            public void WhenUpgradeDraftAndDraftNotExist_ThenThrows()
            {
                this.application
                    .Invoking(x => x.UpgradeDraft(false))
                    .Should().Throw<AutomateException>()
                    .WithMessage(ExceptionMessages.RuntimeApplication_NoCurrentDraft);
            }

            [Fact]
            public void WhenUpgradeDraftAndNothingToUpgrade_ThenDraftUpgraded()
            {
                this.application.CreateDraft("apatternname", null);

                var result = this.application.UpgradeDraft(false);

                result.IsSuccess.Should().BeTrue();
                result.Log.Should().ContainSingle(x =>
                    x.Type == MigrationChangeType.Abort
                    && x.MessageTemplate == MigrationMessages.DraftDefinition_Upgrade_SameToolkitVersion);
                result.Draft.Id.Should().Be(this.application.CurrentDraftId);
            }

            private void ImportToolkit(string patternName)
            {
                this.pattern = new PatternDefinition(patternName);
                this.toolkit = new ToolkitDefinition(this.pattern);
                this.toolkitStore.Import(this.toolkit);
            }

            private void ResetToolkit()
            {
                this.toolkitStore.DestroyAll();
                this.toolkitStore.Import(new ToolkitDefinition(this.pattern));
            }
        }
    }
}