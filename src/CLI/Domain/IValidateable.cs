using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using automate.Extensions;
using ServiceStack;

namespace automate.Domain
{
    internal interface IValidateable
    {
        ValidationResults Validate(ValidationContext context, object value);
    }

    internal class ValidationResults : Collection<ValidationResult>
    {
        public ValidationResults()
        {
        }

        public ValidationResults(IEnumerable<ValidationResult> results)
        {
            results.GuardAgainstNull(nameof(results));
            results.Safe().ToList().ForEach(Add);
        }

        public static ValidationResults None => new ValidationResults();

        public IList<ValidationResult> Results => Items;

        public void Add(ValidationContext context, string message)
        {
            Items.Add(new ValidationResult(context, message));
        }

        public void AddRange(IEnumerable<ValidationResult> results)
        {
            if (results.Exists())
            {
                results
                    .ToList()
                    .ForEach(result => Items.Add(result));
            }
        }
    }

    internal class ValidationResult
    {
        public ValidationResult(ValidationContext context, string message)
        {
            Message = message;
            Context = context;
        }

        public ValidationContext Context { get; }

        public string Message { get; }
    }

    internal class ValidationContext
    {
        private readonly List<string> pathParts = new List<string>();

        public ValidationContext(ValidationContext context)
        {
            this.pathParts = new List<string>(context.pathParts);
        }

        public ValidationContext(string path = null)
        {
            if (path.HasValue())
            {
                AddParts(path);
            }
        }

        public string Path => $"{{{this.pathParts.Join(".")}}}";

        public void Add(string path)
        {
            if (path.HasValue())
            {
                AddParts(path);
            }
        }

        private void AddParts(string path)
        {
            var parts = path.Split(".", StringSplitOptions.RemoveEmptyEntries);
            this.pathParts.AddRange(parts);
        }
    }
}