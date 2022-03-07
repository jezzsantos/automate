using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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

        public SolutionDefinition CreateSolution(string toolkitName, string solutionName)
        {
            var toolkit = this.toolkitStore.FindByName(toolkitName);
            if (toolkit.NotExists())
            {
                throw new AutomateException(ExceptionMessages.RuntimeApplication_ToolkitNotFound.Format(toolkitName));
            }

            return this.solutionStore.Create(toolkit, solutionName);
        }

        public List<ToolkitDefinition> ListInstalledToolkits()
        {
            return this.toolkitStore.ListAll();
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
            string addToCollectionExpression, string onElementExpression, List<string> propertyAssignments)
        {
            VerifyCurrentSolutionExists();
            var solution = this.solutionStore.GetCurrent();

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

            if (propertyAssignments.Safe().Any())
            {
                propertyAssignments.ForEach(pa => pa.GuardAgainstInvalid(IsValidAssignment,
                    nameof(propertyAssignments),
                    ExceptionMessages.RuntimeApplication_ConfigureSolution_PropertyAssignmentInvalid, solution.Id));
            }

            //
            // var solution = this.solutionStore.FindById(solution.Id);
            // if (solution.NotExists())
            // {
            //     throw new AutomateException(ExceptionMessages.RuntimeApplication_SolutionNotFound.Format(solution.Id));
            // }

            var target = solution.Model;
            if (addElementExpression.HasValue())
            {
                var solutionItem = this.solutionPathResolver.ResolveItem(solution, addElementExpression);
                if (solutionItem.NotExists())
                {
                    throw new AutomateException(
                        ExceptionMessages.RuntimeApplication_ElementExpressionNotFound.Format(
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
                        ExceptionMessages.RuntimeApplication_ElementExpressionNotFound.Format(
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
                        ExceptionMessages.RuntimeApplication_ElementExpressionNotFound.Format(
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

            if (propertyAssignments.Safe().Any())
            {
                var nameValues =
                    propertyAssignments
                        .Select(ParseAssignment)
                        .ToDictionary(pair => pair.Name, pair => pair.Value);
                foreach (var (name, value) in nameValues)
                {
                    if (!target!.HasAttribute(name))
                    {
                        throw new AutomateException(
                            ExceptionMessages.RuntimeApplication_ConfigureSolution_ElementPropertyNotExists.Format(
                                target.Name, name));
                    }

                    var property = target.GetProperty(name);
                    if (property.IsChoice)
                    {
                        if (!property.HasChoice(value))
                        {
                            throw new AutomateException(
                                ExceptionMessages.RuntimeApplication_ConfigureSolution_ElementPropertyValueIsNotOneOf
                                    .Format(
                                        target.Name, name, property.ChoiceValues.Join(";"), value));
                        }
                    }
                    else
                    {
                        if (!property.DataTypeMatches(value))
                        {
                            throw new AutomateException(
                                ExceptionMessages.RuntimeApplication_ConfigureSolution_ElementPropertyValueNotCompatible
                                    .Format(
                                        target.Name, name, property.DataType, value));
                        }
                    }

                    property.SetProperty(value);
                }
            }

            this.solutionStore.Save(solution);

            return target;
        }

        public (string Configuration, PatternDefinition Pattern, ValidationResults Validation) GetConfiguration(bool includeSchema, bool includeValidationResults)
        {
            VerifyCurrentSolutionExists();
            var solution = this.solutionStore.GetCurrent();
            var validation = includeValidationResults
                ? Validate(null)
                : ValidationResults.None;
            var schema = includeSchema
                ? solution.Toolkit.Pattern
                : null;

            return (solution.GetConfiguration(), schema, validation);
        }

        public ValidationResults Validate(string elementExpression)
        {
            VerifyCurrentSolutionExists();
            var solution = this.solutionStore.GetCurrent();

            var target = solution.Model;
            if (elementExpression.HasValue())
            {
                var solutionItem = this.solutionPathResolver.ResolveItem(solution, elementExpression);
                if (solutionItem.NotExists())
                {
                    throw new AutomateException(
                        ExceptionMessages.RuntimeApplication_ElementExpressionNotFound.Format(solution.PatternName,
                            elementExpression));
                }
                target = solutionItem;
            }

            return target.Validate(new ValidationContext());
        }

        public CommandExecutionResult ExecuteLaunchPoint(string name, string elementExpression)
        {
            VerifyCurrentSolutionExists();
            var solution = this.solutionStore.GetCurrent();

            var target = solution.Model;
            if (elementExpression.HasValue())
            {
                var solutionItem = this.solutionPathResolver.ResolveItem(solution, elementExpression);
                if (solutionItem.NotExists())
                {
                    throw new AutomateException(
                        ExceptionMessages.RuntimeApplication_ElementExpressionNotFound.Format(solution.PatternName,
                            elementExpression));
                }
                target = solutionItem;
            }

            var validationResults = solution.Model.Validate(new ValidationContext());
            if (validationResults.Any())
            {
                return new CommandExecutionResult(name, validationResults);
            }

            var result = target.ExecuteCommand(solution, name);
            this.solutionStore.Save(solution);

            return result;
        }

        private void VerifyCurrentSolutionExists()
        {
            if (this.solutionStore.GetCurrent().NotExists())
            {
                throw new AutomateException(ExceptionMessages.RuntimeApplication_NoCurrentSolution);
            }
        }

        private static bool IsValidAssignment(string assignment)
        {
            const string propertyNameExpression = @"[\w]+";
            const string propertyValueExpression = @"[\w\d \/\.\(\)]+";
            return Regex.IsMatch(assignment, $"({propertyNameExpression})[=]{{1}}({propertyValueExpression})");
        }

        private static (string Name, string Value) ParseAssignment(string expression)
        {
            var parts = expression.Split('=');
            return (parts.First(), parts.Last());
        }
    }
}