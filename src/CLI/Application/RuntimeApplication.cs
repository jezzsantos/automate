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

        public ToolkitDefinition InstallToolkit(string installerLocation)
        {
            if (!this.fileResolver.ExistsAtPath(installerLocation))
            {
                throw new AutomateException(
                    ExceptionMessages.RuntimeApplication_ToolkitInstallerNotFound.Format(installerLocation));
            }

            var installer = this.fileResolver.GetFileAtPath(installerLocation);
            var toolkit = this.packager.UnPack(installer);

            this.toolkitStore.ChangeCurrent(toolkit.Id);

            return toolkit;
        }

        public SolutionDefinition CreateSolution(string toolkitName)
        {
            var toolkit = this.toolkitStore.FindByName(toolkitName);
            if (toolkit.NotExists())
            {
                throw new AutomateException(ExceptionMessages.RuntimeApplication_ToolkitNotFound.Format(toolkitName));
            }

            var solution = new SolutionDefinition(toolkit);
            this.solutionStore.Save(solution);

            return solution;
        }

        public List<ToolkitDefinition> ListInstalledToolkits()
        {
            return this.toolkitStore.ListAll();
        }

        public List<SolutionDefinition> ListCreatedSolutions()
        {
            return this.solutionStore.ListAll();
        }

        public SolutionItem ConfigureSolution(string solutionId, string addElementExpression,
            string addToCollectionExpression, List<string> propertyAssignments)
        {
            if (addElementExpression.HasValue() && addToCollectionExpression.HasValue())
            {
                addElementExpression.GuardAgainstInvalid(expr => false,
                    nameof(addElementExpression),
                    ExceptionMessages.RuntimeApplication_ConfigureSolution_AddAndAddTo.Format(
                        solutionId, addElementExpression, addToCollectionExpression));
            }

            if (!addElementExpression.HasValue() && !addToCollectionExpression.HasValue()
                                                 && propertyAssignments.HasNone())
            {
                throw new ArgumentOutOfRangeException(nameof(addElementExpression),
                    ExceptionMessages.RuntimeApplication_ConfigureSolution_NoChanges.Format(
                        solutionId));
            }

            if (propertyAssignments.Safe().Any())
            {
                propertyAssignments.ForEach(pa => pa.GuardAgainstInvalid(IsValidAssignment,
                    nameof(propertyAssignments),
                    ExceptionMessages.RuntimeApplication_ConfigureSolution_PropertyAssignmentInvalid, solutionId));
            }

            var solution = this.solutionStore.FindById(solutionId);
            if (solution.NotExists())
            {
                throw new AutomateException(ExceptionMessages.RuntimeApplication_SolutionNotFound.Format(solutionId));
            }

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

        public string GetSolutionConfiguration(string solutionId)
        {
            var solution = this.solutionStore.FindById(solutionId);
            if (solution.NotExists())
            {
                throw new AutomateException(ExceptionMessages.RuntimeApplication_SolutionNotFound.Format(solutionId));
            }

            return solution.GetConfiguration();
        }

        public ValidationResults Validate(string solutionId, string elementExpression)
        {
            var solution = this.solutionStore.FindById(solutionId);
            if (solution.NotExists())
            {
                throw new AutomateException(ExceptionMessages.RuntimeApplication_SolutionNotFound.Format(solutionId));
            }

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

        public CommandExecutionResult ExecuteCommand(string solutionId, string name, string elementExpression)
        {
            var solution = this.solutionStore.FindById(solutionId);
            if (solution.NotExists())
            {
                throw new AutomateException(ExceptionMessages.RuntimeApplication_SolutionNotFound.Format(solutionId));
            }

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

            var result = target.ExecuteCommand(solution.Toolkit, name);
            this.solutionStore.Save(solution);

            return result;
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