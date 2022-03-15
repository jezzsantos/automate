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
        private readonly IFilePathResolver fileResolver;
        private readonly IPatternToolkitPackager packager;
        private readonly ISolutionPathResolver solutionPathResolver;
        private readonly ISolutionStore solutionStore;
        private readonly IToolkitStore toolkitStore;

        public RuntimeApplication(string currentDirectory) : this(currentDirectory, new PatternStore(currentDirectory),
            new ToolkitStore(currentDirectory), new SolutionStore(currentDirectory), new SystemIoFilePathResolver(),
            new SolutionPathResolver())
        {
        }

        private RuntimeApplication(string currentDirectory, IPatternStore patternStore, IToolkitStore toolkitStore,
            ISolutionStore solutionStore, IFilePathResolver fileResolver, ISolutionPathResolver solutionPathResolver) :
            this(toolkitStore, solutionStore, fileResolver,
                new PatternToolkitPackager(patternStore, toolkitStore), solutionPathResolver)
        {
            currentDirectory.GuardAgainstNullOrEmpty(nameof(currentDirectory));
        }

        internal RuntimeApplication(IToolkitStore toolkitStore, ISolutionStore solutionStore,
            IFilePathResolver fileResolver,
            IPatternToolkitPackager packager, ISolutionPathResolver solutionPathResolver)
        {
            toolkitStore.GuardAgainstNull(nameof(toolkitStore));
            solutionStore.GuardAgainstNull(nameof(solutionStore));
            fileResolver.GuardAgainstNull(nameof(fileResolver));
            packager.GuardAgainstNull(nameof(packager));
            solutionPathResolver.GuardAgainstNull(nameof(solutionPathResolver));
            this.toolkitStore = toolkitStore;
            this.solutionStore = solutionStore;
            this.fileResolver = fileResolver;
            this.packager = packager;
            this.solutionPathResolver = solutionPathResolver;
        }

        public string CurrentSolutionId => this.solutionStore.GetCurrent()?.Id;

        public string CurrentSolutionName => this.solutionStore.GetCurrent()?.Name;

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

        public SolutionDefinition CreateSolution(string toolkitName, string solutionName)
        {
            var toolkit = this.toolkitStore.FindByName(toolkitName);
            if (toolkit.NotExists())
            {
                throw new AutomateException(ExceptionMessages.RuntimeApplication_ToolkitNotFound.Format(toolkitName));
            }

            var solution = new SolutionDefinition(toolkit, solutionName);

            return this.solutionStore.Create(solution);
        }

        public List<SolutionDefinition> ListCreatedSolutions()
        {
            return this.solutionStore.ListAll();
        }

        public void SwitchCurrentSolution(string solutionId)
        {
            solutionId.GuardAgainstNullOrEmpty(nameof(solutionId));

            this.solutionStore.ChangeCurrent(solutionId);
        }

        public SolutionItem ConfigureSolution(string addElementExpression,
            string addToCollectionExpression, string onElementExpression, Dictionary<string, string> propertyAssignments)
        {
            var solution = EnsureCurrentSolutionExists();

            if (!addElementExpression.HasValue() && !addToCollectionExpression.HasValue()
                                                 && !onElementExpression.HasValue()
                                                 && propertyAssignments.HasNone())
            {
                throw new ArgumentOutOfRangeException(nameof(addElementExpression),
                    ExceptionMessages.RuntimeApplication_ConfigureSolution_NoChanges.Format(
                        solution.Id));
            }

            if (addElementExpression.HasValue() && addToCollectionExpression.HasValue())
            {
                throw new ArgumentOutOfRangeException(nameof(addElementExpression),
                    ExceptionMessages.RuntimeApplication_ConfigureSolution_AddAndAddTo.Format(
                        solution.Id, addElementExpression, addToCollectionExpression));
            }

            if (onElementExpression.HasValue() && addElementExpression.HasValue())
            {
                throw new ArgumentOutOfRangeException(nameof(onElementExpression),
                    ExceptionMessages.RuntimeApplication_ConfigureSolution_OnAndAdd.Format(
                        solution.Id, onElementExpression, addElementExpression));
            }

            if (onElementExpression.HasValue() && addToCollectionExpression.HasValue())
            {
                throw new ArgumentOutOfRangeException(nameof(onElementExpression),
                    ExceptionMessages.RuntimeApplication_ConfigureSolution_OnAndAddTo.Format(
                        solution.Id, onElementExpression, addToCollectionExpression));
            }

            var target = ResolveTargetItem(solution, addElementExpression, addToCollectionExpression, onElementExpression);
            if (propertyAssignments.Safe().Any())
            {
                target.SetProperties(propertyAssignments);
            }

            this.solutionStore.Save(solution);

            return target;
        }

        public (string Configuration, PatternDefinition Pattern, ValidationResults Validation) GetConfiguration(bool includeSchema, bool includeValidationResults)
        {
            var solution = EnsureCurrentSolutionExists();

            var validation = includeValidationResults
                ? Validate(null)
                : ValidationResults.None;

            var schema = includeSchema
                ? solution.Toolkit.Pattern
                : null;

            return (solution.GetConfiguration(), schema, validation);
        }

        public ValidationResults Validate(string itemExpression)
        {
            var solution = EnsureCurrentSolutionExists();
            var target = ResolveTargetItem(solution, itemExpression);

            return target.Validate(new ValidationContext());
        }

        public CommandExecutionResult ExecuteLaunchPoint(string name, string itemExpression)
        {
            var solution = EnsureCurrentSolutionExists();
            var target = ResolveTargetItem(solution, itemExpression);

            var validationResults = solution.Validate(new ValidationContext());
            if (validationResults.Any())
            {
                return new CommandExecutionResult(name, validationResults);
            }

            var result = target.ExecuteCommand(solution, name);
            this.solutionStore.Save(solution);

            return result;
        }

        private SolutionItem ResolveTargetItem(SolutionDefinition solution, string itemExpression)
        {
            var target = solution.Model;
            if (itemExpression.HasValue())
            {
                var solutionItem = this.solutionPathResolver.ResolveItem(solution, itemExpression);
                if (solutionItem.NotExists())
                {
                    throw new AutomateException(
                        ExceptionMessages.RuntimeApplication_ItemExpressionNotFound.Format(solution.PatternName,
                            itemExpression));
                }
                target = solutionItem;
            }

            return target;
        }

        private SolutionItem ResolveTargetItem(SolutionDefinition solution, string addElementExpression, string addToCollectionExpression, string onElementExpression)
        {
            var target = solution.Model;
            if (addElementExpression.HasValue())
            {
                var solutionItem = this.solutionPathResolver.ResolveItem(solution, addElementExpression);
                if (solutionItem.NotExists())
                {
                    throw new AutomateException(
                        ExceptionMessages.RuntimeApplication_ItemExpressionNotFound.Format(
                            solution.PatternName,
                            addElementExpression));
                }

                if (solutionItem.IsMaterialised)
                {
                    throw new AutomateException(
                        ExceptionMessages.RuntimeApplication_ConfigureSolution_AddElementExists.Format(
                            addElementExpression));
                }

                target = solutionItem.Materialise();
            }

            if (addToCollectionExpression.HasValue())
            {
                var collection = this.solutionPathResolver.ResolveItem(solution, addToCollectionExpression);
                if (collection.NotExists())
                {
                    throw new AutomateException(
                        ExceptionMessages.RuntimeApplication_ItemExpressionNotFound.Format(
                            solution.PatternName,
                            addToCollectionExpression));
                }

                target = collection.MaterialiseCollectionItem();
            }

            if (onElementExpression.HasValue())
            {
                var solutionItem = this.solutionPathResolver.ResolveItem(solution, onElementExpression);
                if (solutionItem.NotExists())
                {
                    throw new AutomateException(
                        ExceptionMessages.RuntimeApplication_ItemExpressionNotFound.Format(
                            solution.PatternName,
                            onElementExpression));
                }

                if (!solutionItem.IsMaterialised)
                {
                    throw new AutomateException(
                        ExceptionMessages.RuntimeApplication_ConfigureSolution_OnElementNotExists.Format(
                            onElementExpression));
                }

                target = solutionItem;
            }
            return target;
        }

        private SolutionDefinition EnsureCurrentSolutionExists()
        {
            var solution = this.solutionStore.GetCurrent();
            if (solution.NotExists())
            {
                throw new AutomateException(ExceptionMessages.RuntimeApplication_NoCurrentSolution);
            }

            return solution;
        }
    }
}