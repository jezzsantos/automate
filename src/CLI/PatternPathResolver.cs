using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using automate.Extensions;

namespace automate
{
    internal class PatternPathResolver : IPatternPathResolver
    {
        public IElementContainer Resolve(PatternMetaModel pattern, string expression)
        {
            pattern.GuardAgainstNull(nameof(pattern));
            expression.GuardAgainstNullOrEmpty(nameof(expression));

            var expressionPath = Regex.Match(expression, @"^\{(?<path>[a-zA-Z0-9\.]*)\}")
                .Groups["path"]
                .Captures.FirstOrDefault()?.Value;
            if (!expressionPath.HasValue())
            {
                throw new PatternException(ExceptionMessages.PatternPathResolver_InvalidExpression.Format(expression));
            }

            var expressionParts = expressionPath.SafeSplit(".");
            if (expressionParts.HasNone())
            {
                throw new PatternException(ExceptionMessages.PatternPathResolver_InvalidExpression.Format(expression));
            }

            if (expressionParts.First().NotEqualsOrdinal(pattern.Name))
            {
                return null;
            }

            if (expressionParts.Length == 1)
            {
                return pattern;
            }

            var remainingParts = new Queue<string>(expressionParts.Skip(1));
            var nextPart = remainingParts.Dequeue();
            IElementContainer target = pattern;
            while (nextPart.Exists())
            {
                var childElement = target.Elements
                    .FirstOrDefault(element => element.Name.EqualsIgnoreCase(nextPart));
                if (childElement.NotExists())
                {
                    return null;
                }

                target = childElement;
                remainingParts.TryDequeue(out nextPart);
            }

            return target;
        }
    }
}