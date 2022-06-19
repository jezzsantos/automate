using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Automate.Domain;
using Automate.Extensions;

namespace Automate.Infrastructure
{
    public class DraftPathResolver : IDraftPathResolver
    {
        public DraftItem ResolveItem(DraftDefinition draft, string expression)
        {
            draft.GuardAgainstNull(nameof(draft));
            expression.GuardAgainstNullOrEmpty(nameof(expression));

            var expressionPath = Regex.Match(expression, @"^\{(?<path>[a-zA-Z0-9\.]*)\}")
                .Groups["path"]
                .Captures.FirstOrDefault()?.Value;
            if (!expressionPath.HasValue())
            {
                throw new AutomateException(
                    ExceptionMessages.DraftPathResolver_InvalidExpression.Substitute(expression));
            }

            var expressionParts = expressionPath.SafeSplit(".").ToArray();
            if (expressionParts.HasNone())
            {
                throw new AutomateException(
                    ExceptionMessages.DraftPathResolver_InvalidExpression.Substitute(expression));
            }

            if (expressionParts.Length == 1
                && expressionParts.First().EqualsOrdinal(draft.PatternName))
            {
                return draft.Model;
            }

            if (expressionParts.First().EqualsOrdinal(draft.PatternName))
            {
                expressionParts = expressionParts.Skip(1).ToArray();
            }

            var remainingParts = new Queue<string>(expressionParts);
            var nextPart = remainingParts.Dequeue();
            var target = draft.Model;
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

        public string ResolveExpression(string description, string expression, DraftItem draftItem)
        {
            description.GuardAgainstNullOrEmpty(nameof(description));

            if (!expression.HasValue())
            {
                return null;
            }
            return Transform(expression, description, draftItem);
        }

        private static string Transform(string template, string description, DraftItem draftItem)
        {
            var configuration = draftItem.GetConfiguration(true);
            return configuration.Transform(description, template);
        }
    }
}