using System;
using System.Collections.Generic;
using System.Linq;
using Automate.Authoring.Application;
using Automate.Authoring.Domain;
using Automate.Common;
using Automate.Common.Domain;
using Automate.Common.Extensions;
using Automate.Runtime.Domain;

namespace Automate.Runtime.Application
{
    public class RuntimeApplication
    {
        private readonly IAutomationExecutor automationExecutor;
        private readonly IDraftPathResolver draftPathResolver;
        private readonly IDraftStore draftStore;
        private readonly IFilePathResolver fileResolver;
        private readonly IPatternToolkitPackager packager;
        private readonly IRuntimeMetadata runtimeMetadata;
        private readonly IToolkitStore toolkitStore;

        public RuntimeApplication(IToolkitStore toolkitStore, IDraftStore draftStore,
            IFilePathResolver fileResolver,
            IPatternToolkitPackager packager, IDraftPathResolver draftPathResolver,
            IAutomationExecutor automationExecutor,
            IRuntimeMetadata runtimeMetadata
        )
        {
            toolkitStore.GuardAgainstNull(nameof(toolkitStore));
            draftStore.GuardAgainstNull(nameof(draftStore));
            fileResolver.GuardAgainstNull(nameof(fileResolver));
            packager.GuardAgainstNull(nameof(packager));
            draftPathResolver.GuardAgainstNull(nameof(draftPathResolver));
            automationExecutor.GuardAgainstNull(nameof(automationExecutor));
            runtimeMetadata.GuardAgainstNull(nameof(runtimeMetadata));
            this.toolkitStore = toolkitStore;
            this.draftStore = draftStore;
            this.fileResolver = fileResolver;
            this.packager = packager;
            this.draftPathResolver = draftPathResolver;
            this.automationExecutor = automationExecutor;
            this.runtimeMetadata = runtimeMetadata;
        }

        public string CurrentDraftId => this.draftStore.GetCurrent()?.Id;

        public string CurrentDraftName => this.draftStore.GetCurrent()?.Name;

        public ToolkitDefinition InstallToolkit(string installerLocation)
        {
            if (!this.fileResolver.ExistsAtPath(installerLocation))
            {
                throw new AutomateException(
                    ExceptionMessages.RuntimeApplication_ToolkitInstallerNotFound.Substitute(installerLocation));
            }

            var installer = this.fileResolver.GetFileAtPath(installerLocation);
            var toolkit = this.packager.UnPack(this.runtimeMetadata, installer);

            this.toolkitStore.Import(toolkit);

            return toolkit;
        }

        public List<ToolkitDefinition> ListInstalledToolkits()
        {
            return this.toolkitStore.ListAll();
        }

        public DraftDefinition CreateDraft(string toolkitName, string draftName)
        {
            var toolkit = this.toolkitStore.FindByName(toolkitName);
            if (toolkit.NotExists())
            {
                throw new AutomateException(
                    ExceptionMessages.RuntimeApplication_ToolkitNotFound.Substitute(toolkitName));
            }

            toolkit.VerifyRuntimeCompatability(this.runtimeMetadata);

            var draft = new DraftDefinition(toolkit, draftName);

            return this.draftStore.Create(draft);
        }

        public List<DraftDefinition> ListCreatedDrafts()
        {
            return this.draftStore.ListAll();
        }

        public void SwitchCurrentDraft(string draftId)
        {
            draftId.GuardAgainstNullOrEmpty(nameof(draftId));

            var draft = this.draftStore.ChangeCurrent(draftId);
            this.toolkitStore.ChangeCurrent(draft.Toolkit.Id);
        }

        public DraftItem ConfigureDraft(string addElementExpression,
            string addToCollectionExpression, string onElementExpression,
            Dictionary<string, string> propertyAssignments)
        {
            var draft = EnsureCurrentDraftExists();

            if (addElementExpression.HasNoValue()
                && addToCollectionExpression.HasNoValue()
                && onElementExpression.HasNoValue()
                && propertyAssignments.HasNone())
            {
                throw new ArgumentOutOfRangeException(nameof(addElementExpression),
                    ExceptionMessages.RuntimeApplication_ConfigureDraft_NoChanges.Substitute(
                        draft.Id));
            }

            if (addElementExpression.HasValue() && addToCollectionExpression.HasValue())
            {
                throw new ArgumentOutOfRangeException(nameof(addElementExpression),
                    ExceptionMessages.RuntimeApplication_ConfigureDraft_AddAndAddTo.Substitute(
                        draft.Id, addElementExpression, addToCollectionExpression));
            }

            if (onElementExpression.HasValue() && addElementExpression.HasValue())
            {
                throw new ArgumentOutOfRangeException(nameof(onElementExpression),
                    ExceptionMessages.RuntimeApplication_ConfigureDraft_OnAndAdd.Substitute(
                        draft.Id, onElementExpression, addElementExpression));
            }

            if (onElementExpression.HasValue() && addToCollectionExpression.HasValue())
            {
                throw new ArgumentOutOfRangeException(nameof(onElementExpression),
                    ExceptionMessages.RuntimeApplication_ConfigureDraft_OnAndAddTo.Substitute(
                        draft.Id, onElementExpression, addToCollectionExpression));
            }

            var target = ResolveTargetItem(draft, addElementExpression, addToCollectionExpression,
                onElementExpression);
            if (propertyAssignments.Safe().Any())
            {
                target.SetProperties(propertyAssignments);
            }

            this.draftStore.Save(draft);

            return target;
        }

        public DraftItem ConfigureDraftAndResetElement(string elementExpression)
        {
            elementExpression.GuardAgainstNullOrEmpty(nameof(elementExpression));

            var draft = EnsureCurrentDraftExists();

            var target = ResolveTargetItem(draft, null, null, elementExpression);
            target.ResetAllProperties();

            this.draftStore.Save(draft);

            return target;
        }

        public DraftItem ConfigureDraftAndClearCollection(string collectionExpression)
        {
            collectionExpression.GuardAgainstNullOrEmpty(nameof(collectionExpression));

            var draft = EnsureCurrentDraftExists();

            var target = ResolveTargetItem(draft, null, null, collectionExpression);
            target.ClearCollectionItems();

            this.draftStore.Save(draft);

            return target;
        }

        public DraftItem ConfigureDraftAndDelete(string expression)
        {
            expression.GuardAgainstNullOrEmpty(nameof(expression));

            var draft = EnsureCurrentDraftExists();

            var target = ResolveTargetItem(draft, null, null, expression);

            if (target.IsPattern)
            {
                throw new AutomateException(ExceptionMessages.RuntimeApplication_ConfigureDraft_DeletePattern);
            }

            target.Parent.Delete(target);

            this.draftStore.Save(draft);

            return target;
        }

        public (LazyDraftItemDictionary Configuration, PatternDefinition Pattern, ValidationResults Validation)
            GetDraftConfiguration(bool includeSchema, bool includeValidationResults)
        {
            var draft = EnsureCurrentDraftExists();

            var validation = includeValidationResults
                ? Validate(null)
                : ValidationResults.None;

            var schema = includeSchema
                ? draft.Toolkit.Pattern
                : null;

            return (draft.GetConfiguration(), schema, validation);
        }

        public ToolkitDefinition ViewCurrentToolkit()
        {
            var toolkit = EnsureCurrentToolkitExists();

            return toolkit;
        }

        public ValidationResults Validate(string itemExpression)
        {
            var draft = EnsureCurrentDraftExists();

            return draft.Validate(this.draftPathResolver, itemExpression);
        }

        public CommandExecutionResult ExecuteLaunchPoint(string name, string itemExpression)
        {
            var draft = EnsureCurrentDraftExists();

            var result = draft.ExecuteCommand(this.draftPathResolver, itemExpression, name);
            if (result.IsInvalid)
            {
                return result;
            }

            this.automationExecutor.Execute(result);

            this.draftStore.Save(draft);

            return result;
        }

        public DraftUpgradeResult UpgradeDraft(bool force)
        {
            var draft = EnsureCurrentDraftExists(true);

            var latestToolkit = this.toolkitStore.FindById(draft.Toolkit.Id);

            var result = draft.Upgrade(latestToolkit, force);
            if (!result.IsSuccess)
            {
                throw new AutomateException(ExceptionMessages.RuntimeApplication_UpgradeDraftFailed.Substitute(
                    result.Draft.Name, result.Draft.Id, result.Draft.PatternName, result.FromVersion,
                    result.ToVersion,
                    result.Log.ToBulletList(change =>
                        $"{change.Type}: {change.MessageTemplate.SubstituteTemplate(change.Arguments.ToArray())}")));
            }

            this.draftStore.Save(draft);

            return result;
        }

        private DraftItem ResolveTargetItem(DraftDefinition draft, string addElementExpression,
            string addToCollectionExpression, string onElementExpression)
        {
            var target = draft.Model;
            if (addElementExpression.HasValue())
            {
                var draftItem = this.draftPathResolver.ResolveItem(draft, addElementExpression);
                if (draftItem.NotExists())
                {
                    throw new AutomateException(
                        ExceptionMessages.DraftDefinition_ItemExpressionNotFound.Substitute(
                            draft.PatternName,
                            addElementExpression));
                }

                if (draftItem.IsMaterialised)
                {
                    throw new AutomateException(
                        ExceptionMessages.RuntimeApplication_ConfigureDraft_AddElementExists.Substitute(
                            addElementExpression));
                }

                target = draftItem.Materialise();
            }

            if (addToCollectionExpression.HasValue())
            {
                var collection = this.draftPathResolver.ResolveItem(draft, addToCollectionExpression);
                if (collection.NotExists())
                {
                    throw new AutomateException(
                        ExceptionMessages.DraftDefinition_ItemExpressionNotFound.Substitute(
                            draft.PatternName,
                            addToCollectionExpression));
                }

                target = collection.MaterialiseCollectionItem();
            }

            if (onElementExpression.HasValue())
            {
                var draftItem = this.draftPathResolver.ResolveItem(draft, onElementExpression);
                if (draftItem.NotExists())
                {
                    throw new AutomateException(
                        ExceptionMessages.DraftDefinition_ItemExpressionNotFound.Substitute(
                            draft.PatternName,
                            onElementExpression));
                }

                if (!draftItem.IsMaterialised)
                {
                    throw new AutomateException(
                        ExceptionMessages.RuntimeApplication_ConfigureDraft_OnElementNotExists.Substitute(
                            onElementExpression));
                }

                target = draftItem;
            }
            return target;
        }

        private DraftDefinition EnsureCurrentDraftExists(bool skipVersionChecks = false)
        {
            var draft = this.draftStore.GetCurrent();
            if (draft.NotExists())
            {
                throw new AutomateException(ExceptionMessages.RuntimeApplication_NoCurrentDraft);
            }

            if (skipVersionChecks)
            {
                return draft;
            }

            var currentVersion = draft.Toolkit.Version;
            var toolkit = this.toolkitStore.FindById(draft.Toolkit.Id);
            var installedVersion = toolkit.Version;
            if (currentVersion != installedVersion)
            {
                throw new AutomateException(
                    ExceptionMessages.RuntimeApplication_CurrentDraftUpgraded.Substitute(draft.Name, draft.Id,
                        currentVersion, installedVersion));
            }

            toolkit.VerifyRuntimeCompatability(this.runtimeMetadata);

            return draft;
        }

        private ToolkitDefinition EnsureCurrentToolkitExists()
        {
            var toolkit = this.toolkitStore.GetCurrent();
            if (toolkit.NotExists())
            {
                throw new AutomateException(
                    ExceptionMessages.RuntimeApplication_NoCurrentToolkit);
            }

            return toolkit;
        }
    }
}