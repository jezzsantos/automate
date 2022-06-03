using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Automate.CLI.Extensions;

namespace Automate.CLI.Domain
{
    internal interface IValidateable
    {
        ValidationResults Validate(ValidationContext context, object value);
    }

    internal class ValidationResults : Collection<ValidationResult>
    {
        public static ValidationResults None => new();

        public List<ValidationResult> Results => Items.ToList();

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
        public ValidationContext(string path)
        {
            path.GuardAgainstNullOrEmpty(nameof(path));
            Path = path;
        }

        public string Path { get; }
    }
}