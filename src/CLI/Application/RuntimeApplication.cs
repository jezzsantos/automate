using System;
using System.Collections.Generic;
using System.Linq;
using Automate.CLI.Domain;
using Automate.CLI.Extensions;
using Automate.CLI.Infrastructure;

namespace Automate.CLI.Application
{
    internal class RuntimeApplication
    {
        private readonly IDraftPathResolver draftPathResolver;
        private readonly IDraftStore draftStore;
        private readonly IFilePathResolver fileResolver;
        private readonly IPatternToolkitPackager packager;
        private readonly IToolkitStore toolkitStore;

        public RuntimeApplication(string currentDirectory) : this(currentDirectory, new PatternStore(currentDirectory),
            new ToolkitStore(currentDirectory), new DraftStore(currentDirectory), new SystemIoFilePathResolver(),
            new DraftPathResolver())
        {
        }

        private RuntimeApplication(string currentDirectory, IPatternStore patternStore, IToolkitStore toolkitStore,
            IDraftStore draftStore, IFilePathResolver fileResolver, IDraftPathResolver draftPathResolver) :
            this(toolkitStore, draftStore, fileResolver,
                new PatternToolkitPackager(patternStore, toolkitStore), draftPathResolver)
        {
            currentDirectory.GuardAgainstNullOrEmpty(nameof(currentDirectory));
        }

        internal RuntimeApplication(IToolkitStore toolkitStore, IDraftStore draftStore,
            IFilePathResolver fileResolver,
            IPatternToolkitPackager packager, IDraftPathResolver draftPathResolver)
        {
            toolkitStore.GuardAgainstNull(nameof(toolkitStore));
            draftStore.GuardAgainstNull(nameof(draftStore));
            fileResolver.GuardAgainstNull(nameof(fileResolver));
            packager.GuardAgainstNull(nameof(packager));
            draftPathResolver.GuardAgainstNull(nameof(draftPathResolver));
            this.toolkitStore = toolkitStore;
            this.draftStore = draftStore;
            this.fileResolver = fileResolver;
            this.packager = packager;
            this.draftPathResolver = draftPathResolver;
        }

        public string CurrentDraftId => this.draftStore.GetCurrent()?.Id;

        public string CurrentDraftName => this.draftStore.GetCurrent()?.Name;

        public ToolkitDefinition InstallToolkit(string installerLocation)
        {
            if (!this.fileResolver.ExistsAtPath(installerLocation))
            {
                throw new AutomateException(
                    ExceptionMessages.RuntimeApplication_ToolkitInstallerNotFound.Format(installerLocation));
            }

            var installer = this.fileResolver.GetFileAtPath(installerLocation);
            var toolkit = this.packager.UnPack(installer);

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
                throw new AutomateException(ExceptionMessages.RuntimeApplication_ToolkitNotFound.Format(toolkitName));
            }

            toolkit.VerifyRuntimeCompatability();

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
                    ExceptionMessages.RuntimeApplication_ConfigureDraft_NoChanges.Format(
                        draft.Id));
            }

            if (addElementExpression.HasValue() && addToCollectionExpression.HasValue())
            {
                throw new ArgumentOutOfRangeException(nameof(addElementExpression),
                    ExceptionMessages.RuntimeApplication_ConfigureDraft_AddAndAddTo.Format(
                        draft.Id, addElementExpression, addToCollectionExpression));
            }

            if (onElementExpression.HasValue() && addElementExpression.HasValue())
            {
                throw new ArgumentOutOfRangeException(nameof(onElementExpression),
                    ExceptionMessages.RuntimeApplication_ConfigureDraft_OnAndAdd.Format(
                        draft.Id, onElementExpression, addElementExpression));
            }

            if (onElementExpression.HasValue() && addToCollectionExpression.HasValue())
            {
                throw new ArgumentOutOfRangeException(nameof(onElementExpression),
                    ExceptionMessages.RuntimeApplication_ConfigureDraft_OnAndAddTo.Format(
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

        public (string Configuration, PatternDefinition Pattern, ValidationResults Validation)
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
            var target = ResolveTargetItem(draft, itemExpression);

            return target.Validate();
        }

        public CommandExecutionResult ExecuteLaunchPoint(string name, string itemExpression)
        {
            var draft = EnsureCurrentDraftExists();
            var target = ResolveTargetItem(draft, itemExpression);

            var validationResults = draft.Validate();
            if (validationResults.Any())
            {
                return new CommandExecutionResult(name, validationResults);
            }

            var result = target.ExecuteCommand(draft, name);
            this.draftStore.Save(draft);

            return result;
        }

        public DraftUpgradeResult UpgradeDraft(bool force)
        {
            var draft = EnsureCurrentDraftExists(true);

            var latestToolkit = this.toolkitStore.FindById(draft.Toolkit.Id);

            var result = draft.Upgrade(latestToolkit, force);
            this.draftStore.Save(draft);

            return result;
        }

        private DraftItem ResolveTargetItem(DraftDefinition draft, string itemExpression)
        {
            var target = draft.Model;
            if (itemExpression.HasValue())
            {
                var draftItem = this.draftPathResolver.ResolveItem(draft, itemExpression);
                if (draftItem.NotExists())
                {
                    throw new AutomateException(
                        ExceptionMessages.RuntimeApplication_ItemExpressionNotFound.Format(draft.PatternName,
                            itemExpression));
                }
                target = draftItem;
            }

            return target;
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
                        ExceptionMessages.RuntimeApplication_ItemExpressionNotFound.Format(
                            draft.PatternName,
                            addElementExpression));
                }

                if (draftItem.IsMaterialised)
                {
                    throw new AutomateException(
                        ExceptionMessages.RuntimeApplication_ConfigureDraft_AddElementExists.Format(
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
                        ExceptionMessages.RuntimeApplication_ItemExpressionNotFound.Format(
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
                        ExceptionMessages.RuntimeApplication_ItemExpressionNotFound.Format(
                            draft.PatternName,
                            onElementExpression));
                }

                if (!draftItem.IsMaterialised)
                {
                    throw new AutomateException(
                        ExceptionMessages.RuntimeApplication_ConfigureDraft_OnElementNotExists.Format(
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
                    ExceptionMessages.RuntimeApplication_CurrentDraftUpgraded.Format(draft.Name, draft.Id,
                        currentVersion, installedVersion));
            }

            toolkit.VerifyRuntimeCompatability();

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