﻿using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using automate.Domain;
using automate.Extensions;
using ServiceStack;

namespace automate.Infrastructure
{
    internal class SolutionPathResolver : ISolutionPathResolver
    {
        public SolutionItem Resolve(SolutionDefinition solution, string expression)
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
                var descendant = target.Properties.GetValueOrDefault(nextPart);
                if (descendant.NotExists())
                {
                    return null;
                }

                target = descendant;
                remainingParts.TryDequeue(out nextPart);
            }

            return target;
        }
    }
}