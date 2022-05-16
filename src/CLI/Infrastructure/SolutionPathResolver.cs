using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Automate.CLI.Domain;
using Automate.CLI.Extensions;

namespace Automate.CLI.Infrastructure
{
    internal class SolutionPathResolver : ISolutionPathResolver
    {
        public SolutionItem ResolveItem(SolutionDefinition solution, string expression)
        {
            solution.GuardAgainstNull(nameof(solution));
            expression.GuardAgainstNullOrEmpty(nameof(expression));

            var expressionPath = Regex.Match(expression, @"^\{(?<path>[a-zA-Z0-9\.]*)\}")
                .Groups["path"]
                .Captures.FirstOrDefault()?.Value;
            if (!expressionPath.HasValue())
            {
                throw new AutomateException(
                    ExceptionMessages.SolutionPathResolver_InvalidExpression.Format(expression));
            }

            var expressionParts = expressionPath.SafeSplit(".").ToArray();
            if (expressionParts.HasNone())
            {
                throw new AutomateException(
                    ExceptionMessages.SolutionPathResolver_InvalidExpression.Format(expression));
            }

            if (expressionParts.Length == 1
                && expressionParts.First().EqualsOrdinal(solution.PatternName))
            {
                return solution.Model;
            }

            if (expressionParts.First().EqualsOrdinal(solution.PatternName))
            {
                expressionParts = expressionParts.Skip(1).ToArray();
            }

            var remainingParts = new Queue<string>(expressionParts);
            var nextPart = remainingParts.Dequeue();
            var target = solution.Model;
            while (nextPart.Exists())
            {
                var descendantProperty = target.Properties.Exists()
                    ? target.Properties.GetValueOrDefault(nextPart)
                    : null;
                var descendantItem = target.Items.Exists()
                    ? target.Items.FirstOrDefault(item => item.Id.EqualsIgnoreCase(nextPart))
                    : null;
                if (descendantProperty.NotExists() && descendantItem.NotExists())
                {
                    return null;
                }
                if (descendantProperty.Exists())
                {
                    target = descendantProperty;
                }
                if (descendantItem.Exists())
                {
                    target = descendantItem;
                }

                remainingParts.TryDequeue(out nextPart);
            }

            return target;
        }

        public string ResolveExpression(string description, string expression, SolutionItem solutionItem)
        {
            description.GuardAgainstNullOrEmpty(nameof(description));

            if (!expression.HasValue())
            {
                return null;
            }
            return Transform(expression, description, solutionItem);
        }

        private static string Transform(string template, string description, SolutionItem solutionItem)
        {
            var configuration = solutionItem.GetConfiguration(true);
            return configuration.Transform(description, template);
        }
    }
}